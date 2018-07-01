// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Calibration_Tabs.CatScreenTab
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using OpenTK.Graphics;
using QuickFont;
using System.Collections.Generic;

namespace M3D.GUI.SettingsPages.Calibration_Tabs
{
  public class CatScreenTab : SettingsPage
  {
    private CatScreenTab.ProbeLocation m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
    private const float MAX_OFFSET = 3f;
    private const float OFFSET_INC = 0.05f;
    private const float MAX_Z_OFFSET = 3f;
    private const float Z_OFFSET_INC = 0.01f;
    private bool m_bhasUnsavedChanges;
    private Calibration m_ocUnsavedCalibrationValues;
    private PopupMessageBox messagebox;
    private SettingsManager main_controller;
    private SpoolerConnection spooler_connection;
    private TextWidget ZO_Text;
    private TextWidget FRO_Text;
    private TextWidget FLO_Text;
    private TextWidget BRO_Text;
    private TextWidget BLO_Text;
    private TextWidget Please_Connect_Text;
    private TextWidget Calibration_Not_Supported;
    private TextWidget pleasewaittext;
    private XMLFrame MainFrame;
    private XMLFrame PrinterBusyFrame;
    private PrinterObject previously_selected_printer;

    public CatScreenTab(int ID, SettingsManager main_controller, SpoolerConnection spooler_connection, PopupMessageBox messagebox)
      : base(ID)
    {
      this.messagebox = messagebox;
      this.main_controller = main_controller;
      this.spooler_connection = spooler_connection;
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (this.previously_selected_printer != selectedPrinter)
      {
        this.OnPrinterSwitch(selectedPrinter);
        this.previously_selected_printer = selectedPrinter;
      }
      if (selectedPrinter != null)
      {
        this.MainFrame.Visible = true;
        if (!selectedPrinter.isBusy && selectedPrinter.Info.Status != PrinterStatus.Firmware_IsWaitingToPause && (selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPausedProcessing))
        {
          bool flag = true;
          if (selectedPrinter.Info.supportedFeatures.UsesSupportedFeatures)
          {
            int featureSlot = selectedPrinter.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
            if (featureSlot >= 0)
              flag = (uint) selectedPrinter.Info.supportedFeatures.GetStatus(featureSlot) > 0U;
          }
          if (flag)
          {
            if (this.PrinterBusyFrame.Visible && !this.m_bhasUnsavedChanges)
              this.GetUpdatedPrinterStats();
            this.PrinterBusyFrame.Visible = false;
            this.Please_Connect_Text.Visible = false;
            this.Calibration_Not_Supported.Visible = false;
            this.MainFrame.Enabled = true;
            this.MainFrame.Visible = true;
          }
          else
          {
            this.Calibration_Not_Supported.Visible = true;
            this.PrinterBusyFrame.Visible = false;
            this.Please_Connect_Text.Visible = false;
            this.MainFrame.Enabled = false;
            this.MainFrame.Visible = false;
          }
        }
        else
        {
          if (selectedPrinter.Info.current_job != null && CatScreenTab.ProbeLocation.Unknown != this.m_lastProbeLocation)
            this.m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
          if (this.pleasewaittext != null)
            this.pleasewaittext.Text = selectedPrinter.Info.Status != PrinterStatus.Firmware_Calibrating ? "Please wait.\nThe printer is busy perfoming the requested actions." : "Please wait.\nThe printer is calibrating.";
          this.PrinterBusyFrame.Visible = true;
          this.Please_Connect_Text.Visible = false;
          this.MainFrame.Enabled = false;
          this.MainFrame.Visible = false;
        }
      }
      else
      {
        this.MainFrame.Visible = false;
        this.PrinterBusyFrame.Visible = false;
        this.Please_Connect_Text.Visible = true;
      }
    }

    private void OnPrinterSwitch(PrinterObject new_printer)
    {
      if (this.previously_selected_printer != null && this.m_bhasUnsavedChanges)
        this.OnUnsavedChangesCallback(this.previously_selected_printer);
      this.GetUpdatedPrinterStats();
    }

    public void Init(GUIHost host)
    {
      this.MainFrame = this.CreateMainFrame(host);
      this.AddChildElement((Element2D) this.MainFrame);
      this.PrinterBusyFrame = this.CreatePrinterBusyFrame(host);
      this.AddChildElement((Element2D) this.PrinterBusyFrame);
      this.Please_Connect_Text = new TextWidget(1);
      this.Please_Connect_Text.Color = new Color4(byte.MaxValue, (byte) 127, (byte) 39, byte.MaxValue);
      this.Please_Connect_Text.SetPosition(0, 0);
      this.Please_Connect_Text.SetSize(400, 200);
      this.Please_Connect_Text.RelativeWidth = 1f;
      this.Please_Connect_Text.RelativeHeight = 1f;
      this.Please_Connect_Text.Text = "Sorry, but a printer has not been connected.";
      this.Please_Connect_Text.Size = FontSize.Medium;
      this.Please_Connect_Text.Alignment = QFontAlignment.Centre;
      this.Please_Connect_Text.VAlignment = TextVerticalAlignment.Middle;
      this.AddChildElement((Element2D) this.Please_Connect_Text);
      this.Calibration_Not_Supported = new TextWidget(1);
      this.Calibration_Not_Supported.Color = new Color4(byte.MaxValue, (byte) 127, (byte) 39, byte.MaxValue);
      this.Calibration_Not_Supported.SetPosition(0, 0);
      this.Calibration_Not_Supported.SetSize(400, 200);
      this.Calibration_Not_Supported.RelativeWidth = 1f;
      this.Calibration_Not_Supported.RelativeHeight = 1f;
      this.Calibration_Not_Supported.Text = "Sorry. Calibration is not currently supported on your printer.";
      this.Calibration_Not_Supported.Size = FontSize.Medium;
      this.Calibration_Not_Supported.Alignment = QFontAlignment.Centre;
      this.Calibration_Not_Supported.VAlignment = TextVerticalAlignment.Middle;
      this.AddChildElement((Element2D) this.Calibration_Not_Supported);
      this.Calibration_Not_Supported.Visible = false;
      this.MainFrame.Visible = false;
      this.PrinterBusyFrame.Visible = false;
    }

    private XMLFrame CreatePrinterBusyFrame(GUIHost host)
    {
      XMLFrame xmlFrame = new XMLFrame(0);
      xmlFrame.RelativeX = 0.0f;
      xmlFrame.RelativeY = 0.0f;
      xmlFrame.RelativeWidth = 1f;
      xmlFrame.RelativeHeight = 1f;
      this.pleasewaittext = new TextWidget(1);
      this.pleasewaittext.Color = new Color4(byte.MaxValue, (byte) 127, (byte) 39, byte.MaxValue);
      this.pleasewaittext.Text = "Please wait.\nThe printer is busy perfoming the requested actions.";
      this.pleasewaittext.RelativeWidth = 1f;
      this.pleasewaittext.Size = FontSize.Medium;
      this.pleasewaittext.Alignment = QFontAlignment.Centre;
      this.pleasewaittext.SetPosition(0, -30);
      xmlFrame.AddChildElement((Element2D) this.pleasewaittext);
      SpriteAnimationWidget spriteAnimationWidget = new SpriteAnimationWidget(3);
      spriteAnimationWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.CenterVerticallyInParent = true;
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      xmlFrame.AddChildElement((Element2D) spriteAnimationWidget);
      Sprite.pixel_perfect = false;
      return xmlFrame;
    }

    private XMLFrame CreateMainFrame(GUIHost host)
    {
      XMLFrame xmlFrame = new XMLFrame(0);
      string calibrationPanel = Resources.BedOffsetCalibrationPanel;
      xmlFrame.Init(host, calibrationPanel, new ButtonCallback(this.MyButtonCallback));
      xmlFrame.SetPosition(0, 0);
      xmlFrame.RelativeWidth = 1f;
      xmlFrame.RelativeHeight = 1f;
      this.ZO_Text = (TextWidget) xmlFrame.FindChildElement(121);
      this.FRO_Text = (TextWidget) xmlFrame.FindChildElement(122);
      this.FLO_Text = (TextWidget) xmlFrame.FindChildElement(123);
      this.BRO_Text = (TextWidget) xmlFrame.FindChildElement(124);
      this.BLO_Text = (TextWidget) xmlFrame.FindChildElement(125);
      return xmlFrame;
    }

    public override void SetVisible(bool bVisible)
    {
      this.GetUpdatedPrinterStats();
      base.SetVisible(bVisible);
    }

    private void GetUpdatedPrinterStats()
    {
      IPrinter selectedPrinter = (IPrinter) this.spooler_connection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        this.ZO_Text.Text = (double) selectedPrinter.Info.calibration.ENTIRE_Z_HEIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.ENTIRE_Z_HEIGHT_OFFSET.ToString();
        this.ZO_Text.Text = float.Parse(this.ZO_Text.Text.ToString()).ToString("F2");
        this.FRO_Text.Text = (double) selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET.ToString();
        this.FRO_Text.Text = float.Parse(this.FRO_Text.Text.ToString()).ToString("F2");
        this.FLO_Text.Text = (double) selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET.ToString();
        this.FLO_Text.Text = float.Parse(this.FLO_Text.Text.ToString()).ToString("F2");
        this.BRO_Text.Text = (double) selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET.ToString();
        this.BRO_Text.Text = float.Parse(this.BRO_Text.Text.ToString()).ToString("F2");
        this.BLO_Text.Text = (double) selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET.ToString();
        this.BLO_Text.Text = float.Parse(this.BLO_Text.Text.ToString()).ToString("F2");
      }
      else
      {
        this.ZO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        this.FRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        this.FLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        this.BRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        this.BLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
      }
    }

    private void SetBedOffsetsAfterLock(IAsyncCallResult ar)
    {
      CatScreenTab.BedInfoCallbackData asyncState = ar.AsyncState as CatScreenTab.BedInfoCallbackData;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
        this.messagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        this.m_bhasUnsavedChanges = false;
        AsyncCallback callback = !asyncState.releaseLock ? new AsyncCallback(asyncState.printer.ShowLockError) : new AsyncCallback(this.ReleasePrinterAfterCommand);
        int num = (int) asyncState.printer.SetOffsetInfo(callback, (object) asyncState.printer, asyncState.offsets);
      }
      else
        this.messagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
        return;
      int num = (int) asyncState.ReleaseLock(new AsyncCallback(this.OnRelease), (object) null);
    }

    private void OnRelease(IAsyncCallResult ar)
    {
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
          this.messagebox.AddMessageToQueue("Your settings have been applied to the printer.");
          break;
        default:
          this.messagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
          break;
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      switch (button.ID)
      {
        case 100:
          this.BLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          this.BRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          this.FRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          this.FLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          this.ZO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          this.m_bhasUnsavedChanges = true;
          break;
        case 101:
          this.ApplyCalibrationSettings(selectedPrinter, this.GetCalibrationSettingsFromOptions(selectedPrinter.Info), true);
          break;
        case 102:
          float num1 = float.Parse(this.ZO_Text.Text) + 0.01f;
          if ((double) num1 > 3.0)
            num1 = 3f;
          this.ZO_Text.Text = num1.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.Center);
          break;
        case 103:
          float num2 = float.Parse(this.ZO_Text.Text) - 0.01f;
          if ((double) num2 < -3.0)
            num2 = -3f;
          this.ZO_Text.Text = num2.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.Center);
          break;
        case 104:
          float num3 = float.Parse(this.BLO_Text.Text) + 0.05f;
          if ((double) num3 > 3.0)
            num3 = 3f;
          this.BLO_Text.Text = num3.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.BackLeft);
          break;
        case 105:
          float num4 = float.Parse(this.BLO_Text.Text) - 0.05f;
          if ((double) num4 < -3.0)
            num4 = -3f;
          this.BLO_Text.Text = num4.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.BackLeft);
          break;
        case 106:
          float num5 = float.Parse(this.BRO_Text.Text) + 0.05f;
          if ((double) num5 > 3.0)
            num5 = 3f;
          this.BRO_Text.Text = num5.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.BackRight);
          break;
        case 107:
          float num6 = float.Parse(this.BRO_Text.Text) - 0.05f;
          if ((double) num6 < -3.0)
            num6 = -3f;
          this.BRO_Text.Text = num6.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.BackRight);
          break;
        case 108:
          float num7 = float.Parse(this.FLO_Text.Text) + 0.05f;
          if ((double) num7 > 3.0)
            num7 = 3f;
          this.FLO_Text.Text = num7.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.FrontLeft);
          break;
        case 109:
          float num8 = float.Parse(this.FLO_Text.Text) - 0.05f;
          if ((double) num8 < -3.0)
            num8 = -3f;
          this.FLO_Text.Text = num8.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.FrontLeft);
          break;
        case 110:
          float num9 = float.Parse(this.FRO_Text.Text) + 0.05f;
          if ((double) num9 > 3.0)
            num9 = 3f;
          this.FRO_Text.Text = num9.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.FrontRight);
          break;
        case 111:
          float num10 = float.Parse(this.FRO_Text.Text) - 0.05f;
          if ((double) num10 < -3.0)
            num10 = -3f;
          this.FRO_Text.Text = num10.ToString("F2");
          this.OnOffsetChanged(CatScreenTab.ProbeLocation.FrontRight);
          break;
        case 112:
          this.MoveToPoint(CatScreenTab.ProbeLocation.Center, true);
          break;
        case 113:
          this.MoveToPoint(CatScreenTab.ProbeLocation.BackLeft, true);
          break;
        case 114:
          this.MoveToPoint(CatScreenTab.ProbeLocation.BackRight, true);
          break;
        case 115:
          this.MoveToPoint(CatScreenTab.ProbeLocation.FrontLeft, true);
          break;
        case 116:
          this.MoveToPoint(CatScreenTab.ProbeLocation.FrontRight, true);
          break;
      }
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    private void MoveToPoint(CatScreenTab.ProbeLocation location, bool bMoveUp = true)
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
        return;
      BedCompensationPreprocessor compensationPreprocessor = new BedCompensationPreprocessor();
      compensationPreprocessor.UpdateConfigurations(this.GetCalibrationSettingsFromOptions(selectedPrinter.Info), selectedPrinter.MyPrinterProfile.PrinterSizeConstants);
      Vector vector;
      switch (location)
      {
        case CatScreenTab.ProbeLocation.Center:
          vector = compensationPreprocessor.Center;
          break;
        case CatScreenTab.ProbeLocation.FrontLeft:
          vector = compensationPreprocessor.FrontLeft;
          break;
        case CatScreenTab.ProbeLocation.FrontRight:
          vector = compensationPreprocessor.FrontRight;
          break;
        case CatScreenTab.ProbeLocation.BackLeft:
          vector = compensationPreprocessor.BackLeft;
          break;
        case CatScreenTab.ProbeLocation.BackRight:
          vector = compensationPreprocessor.BackRight;
          break;
        default:
          return;
      }
      vector.z = 0.1f + compensationPreprocessor.GetHeightAdjustmentRequired(vector.x, vector.y) + compensationPreprocessor.entire_z_height_offset;
      this.m_lastProbeLocation = location;
      List<string> stringList = new List<string>();
      stringList.Add("M1012");
      stringList.Add("G90");
      if (bMoveUp)
        stringList.Add("G0 Z2");
      stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F3000", (object) vector.x, (object) vector.y));
      stringList.Add(PrinterCompatibleString.Format("G0 Z{0} F100", (object) vector.z));
      stringList.Add("M1011");
      selectedPrinter.SendCommandAutoLock(false, true, new AsyncCallback(this.AutoLockCallBack), (object) selectedPrinter, stringList.ToArray());
    }

    public void AutoLockCallBack(IAsyncCallResult ar)
    {
      string lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
        return;
      this.m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
      this.messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnOffsetChanged(CatScreenTab.ProbeLocation location)
    {
      this.m_bhasUnsavedChanges = true;
      if (location != this.m_lastProbeLocation)
        return;
      this.MoveToPoint(location, false);
    }

    public override void OnOpen()
    {
      this.m_bhasUnsavedChanges = false;
      this.m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
    }

    public override void OnClose()
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
        return;
      if (this.m_bhasUnsavedChanges)
      {
        this.OnUnsavedChangesCallback(selectedPrinter);
      }
      else
      {
        if (!selectedPrinter.HasLock)
          return;
        int num = (int) selectedPrinter.ReleaseLock((AsyncCallback) null, (object) null);
      }
    }

    private void OnUnsavedChangesCallback(PrinterObject printer)
    {
      this.m_ocUnsavedCalibrationValues = this.GetCalibrationSettingsFromOptions(printer.Info);
      this.m_bhasUnsavedChanges = false;
      this.messagebox.AddMessageToQueue("Your calibration changes were not saved to the printer. Would you like to save them now?", "Yes", "No", (string) null, new PopupMessageBox.OnUserSelectionDel(this.OnUnsavedChangesCallback), (object) printer);
    }

    private void OnUnsavedChangesCallback(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      PrinterObject printer = user_data as PrinterObject;
      if (printer == null)
        return;
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        this.ApplyCalibrationSettings(printer, this.m_ocUnsavedCalibrationValues, true);
      }
      else
      {
        int num = (int) printer.ReleaseLock((AsyncCallback) null, (object) null);
      }
    }

    private void ApplyCalibrationSettings(PrinterObject printer, Calibration newSettings, bool releaselock)
    {
      float entireZHeightOffset = newSettings.ENTIRE_Z_HEIGHT_OFFSET;
      float frontRightOffset = newSettings.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      float heightFrontLeftOffset = newSettings.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      float heightBackRightOffset = newSettings.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      float heightBackLeftOffset = newSettings.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      if (printer == null)
        return;
      int num = (int) printer.AcquireLock(new AsyncCallback(this.SetBedOffsetsAfterLock), (object) new CatScreenTab.BedInfoCallbackData(printer, new BedOffsets(heightBackLeftOffset, heightBackRightOffset, frontRightOffset, heightFrontLeftOffset, entireZHeightOffset), releaselock));
    }

    private Calibration GetCalibrationSettingsFromOptions(PrinterInfo printerInfo)
    {
      return new Calibration(printerInfo.calibration)
      {
        CORNER_HEIGHT_BACK_LEFT_OFFSET = float.Parse(this.BLO_Text.Text),
        CORNER_HEIGHT_BACK_RIGHT_OFFSET = float.Parse(this.BRO_Text.Text),
        CORNER_HEIGHT_FRONT_LEFT_OFFSET = float.Parse(this.FLO_Text.Text),
        CORNER_HEIGHT_FRONT_RIGHT_OFFSET = float.Parse(this.FRO_Text.Text),
        ENTIRE_Z_HEIGHT_OFFSET = float.Parse(this.ZO_Text.Text)
      };
    }

    private enum ControlIDs
    {
      ResetAll = 100, // 0x00000064
      Apply = 101, // 0x00000065
      ZO_Up = 102, // 0x00000066
      ZO_Down = 103, // 0x00000067
      BLO_Up = 104, // 0x00000068
      BLO_Down = 105, // 0x00000069
      BRO_Up = 106, // 0x0000006A
      BRO_Down = 107, // 0x0000006B
      FLO_Up = 108, // 0x0000006C
      FLO_Down = 109, // 0x0000006D
      FRO_Up = 110, // 0x0000006E
      FRO_Down = 111, // 0x0000006F
      Z_Move = 112, // 0x00000070
      BL_Move = 113, // 0x00000071
      BR_Move = 114, // 0x00000072
      FL_Move = 115, // 0x00000073
      FR_Move = 116, // 0x00000074
      ZO_Text = 121, // 0x00000079
      FRO_Text = 122, // 0x0000007A
      FLO_Text = 123, // 0x0000007B
      BRO_Text = 124, // 0x0000007C
      BLO_Text = 125, // 0x0000007D
    }

    private enum ProbeLocation
    {
      Center,
      FrontLeft,
      FrontRight,
      BackLeft,
      BackRight,
      Unknown,
    }

    private class PrinterDetails
    {
      public string type = "";
      public string comm = "";
      public PrinterSerialNumber serial;

      public override string ToString()
      {
        return this.type + "<" + (object) this.serial + ">";
      }
    }

    private class BedInfoCallbackData
    {
      public PrinterObject printer;
      public BedOffsets offsets;
      public bool releaseLock;

      public BedInfoCallbackData(PrinterObject printer, BedOffsets offsets, bool releaseLock)
      {
        this.printer = printer;
        this.offsets = offsets;
        this.releaseLock = releaseLock;
      }
    }
  }
}
