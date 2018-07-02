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
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (previously_selected_printer != selectedPrinter)
      {
        OnPrinterSwitch(selectedPrinter);
        previously_selected_printer = selectedPrinter;
      }
      if (selectedPrinter != null)
      {
        MainFrame.Visible = true;
        if (!selectedPrinter.isBusy && selectedPrinter.Info.Status != PrinterStatus.Firmware_IsWaitingToPause && (selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPaused && selectedPrinter.Info.Status != PrinterStatus.Firmware_PrintingPausedProcessing))
        {
          var flag = true;
          if (selectedPrinter.Info.supportedFeatures.UsesSupportedFeatures)
          {
            var featureSlot = selectedPrinter.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
            if (featureSlot >= 0)
            {
              flag = (uint) selectedPrinter.Info.supportedFeatures.GetStatus(featureSlot) > 0U;
            }
          }
          if (flag)
          {
            if (PrinterBusyFrame.Visible && !m_bhasUnsavedChanges)
            {
              GetUpdatedPrinterStats();
            }

            PrinterBusyFrame.Visible = false;
            Please_Connect_Text.Visible = false;
            Calibration_Not_Supported.Visible = false;
            MainFrame.Enabled = true;
            MainFrame.Visible = true;
          }
          else
          {
            Calibration_Not_Supported.Visible = true;
            PrinterBusyFrame.Visible = false;
            Please_Connect_Text.Visible = false;
            MainFrame.Enabled = false;
            MainFrame.Visible = false;
          }
        }
        else
        {
          if (selectedPrinter.Info.current_job != null && CatScreenTab.ProbeLocation.Unknown != m_lastProbeLocation)
          {
            m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
          }

          if (pleasewaittext != null)
          {
            pleasewaittext.Text = selectedPrinter.Info.Status != PrinterStatus.Firmware_Calibrating ? "Please wait.\nThe printer is busy perfoming the requested actions." : "Please wait.\nThe printer is calibrating.";
          }

          PrinterBusyFrame.Visible = true;
          Please_Connect_Text.Visible = false;
          MainFrame.Enabled = false;
          MainFrame.Visible = false;
        }
      }
      else
      {
        MainFrame.Visible = false;
        PrinterBusyFrame.Visible = false;
        Please_Connect_Text.Visible = true;
      }
    }

    private void OnPrinterSwitch(PrinterObject new_printer)
    {
      if (previously_selected_printer != null && m_bhasUnsavedChanges)
      {
        OnUnsavedChangesCallback(previously_selected_printer);
      }

      GetUpdatedPrinterStats();
    }

    public void Init(GUIHost host)
    {
      MainFrame = CreateMainFrame(host);
      AddChildElement(MainFrame);
      PrinterBusyFrame = CreatePrinterBusyFrame(host);
      AddChildElement(PrinterBusyFrame);
      Please_Connect_Text = new TextWidget(1)
      {
        Color = new Color4(byte.MaxValue, 127, 39, byte.MaxValue)
      };
      Please_Connect_Text.SetPosition(0, 0);
      Please_Connect_Text.SetSize(400, 200);
      Please_Connect_Text.RelativeWidth = 1f;
      Please_Connect_Text.RelativeHeight = 1f;
      Please_Connect_Text.Text = "Sorry, but a printer has not been connected.";
      Please_Connect_Text.Size = FontSize.Medium;
      Please_Connect_Text.Alignment = QFontAlignment.Centre;
      Please_Connect_Text.VAlignment = TextVerticalAlignment.Middle;
      AddChildElement(Please_Connect_Text);
      Calibration_Not_Supported = new TextWidget(1)
      {
        Color = new Color4(byte.MaxValue, 127, 39, byte.MaxValue)
      };
      Calibration_Not_Supported.SetPosition(0, 0);
      Calibration_Not_Supported.SetSize(400, 200);
      Calibration_Not_Supported.RelativeWidth = 1f;
      Calibration_Not_Supported.RelativeHeight = 1f;
      Calibration_Not_Supported.Text = "Sorry. Calibration is not currently supported on your printer.";
      Calibration_Not_Supported.Size = FontSize.Medium;
      Calibration_Not_Supported.Alignment = QFontAlignment.Centre;
      Calibration_Not_Supported.VAlignment = TextVerticalAlignment.Middle;
      AddChildElement(Calibration_Not_Supported);
      Calibration_Not_Supported.Visible = false;
      MainFrame.Visible = false;
      PrinterBusyFrame.Visible = false;
    }

    private XMLFrame CreatePrinterBusyFrame(GUIHost host)
    {
      var xmlFrame = new XMLFrame(0)
      {
        RelativeX = 0.0f,
        RelativeY = 0.0f,
        RelativeWidth = 1f,
        RelativeHeight = 1f
      };
      pleasewaittext = new TextWidget(1)
      {
        Color = new Color4(byte.MaxValue, 127, 39, byte.MaxValue),
        Text = "Please wait.\nThe printer is busy perfoming the requested actions.",
        RelativeWidth = 1f,
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre
      };
      pleasewaittext.SetPosition(0, -30);
      xmlFrame.AddChildElement(pleasewaittext);
      var spriteAnimationWidget = new SpriteAnimationWidget(3);
      spriteAnimationWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.CenterVerticallyInParent = true;
      spriteAnimationWidget.CenterHorizontallyInParent = true;
      xmlFrame.AddChildElement(spriteAnimationWidget);
      Sprite.pixel_perfect = false;
      return xmlFrame;
    }

    private XMLFrame CreateMainFrame(GUIHost host)
    {
      var xmlFrame = new XMLFrame(0);
      var calibrationPanel = Resources.BedOffsetCalibrationPanel;
      xmlFrame.Init(host, calibrationPanel, new ButtonCallback(MyButtonCallback));
      xmlFrame.SetPosition(0, 0);
      xmlFrame.RelativeWidth = 1f;
      xmlFrame.RelativeHeight = 1f;
      ZO_Text = (TextWidget) xmlFrame.FindChildElement(121);
      FRO_Text = (TextWidget) xmlFrame.FindChildElement(122);
      FLO_Text = (TextWidget) xmlFrame.FindChildElement(123);
      BRO_Text = (TextWidget) xmlFrame.FindChildElement(124);
      BLO_Text = (TextWidget) xmlFrame.FindChildElement(125);
      return xmlFrame;
    }

    public override void SetVisible(bool bVisible)
    {
      GetUpdatedPrinterStats();
      base.SetVisible(bVisible);
    }

    private void GetUpdatedPrinterStats()
    {
      var selectedPrinter = (IPrinter)spooler_connection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        ZO_Text.Text = selectedPrinter.Info.calibration.ENTIRE_Z_HEIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.ENTIRE_Z_HEIGHT_OFFSET.ToString();
        ZO_Text.Text = float.Parse(ZO_Text.Text.ToString()).ToString("F2");
        FRO_Text.Text = selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET.ToString();
        FRO_Text.Text = float.Parse(FRO_Text.Text.ToString()).ToString("F2");
        FLO_Text.Text = selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET.ToString();
        FLO_Text.Text = float.Parse(FLO_Text.Text.ToString()).ToString("F2");
        BRO_Text.Text = selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET.ToString();
        BRO_Text.Text = float.Parse(BRO_Text.Text.ToString()).ToString("F2");
        BLO_Text.Text = selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET >= 3.0 ? M3DGlobalization.ToLocalString(3f, "F2") : selectedPrinter.Info.calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET.ToString();
        BLO_Text.Text = float.Parse(BLO_Text.Text.ToString()).ToString("F2");
      }
      else
      {
        ZO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        FRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        FLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        BRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        BLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
      }
    }

    private void SetBedOffsetsAfterLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as CatScreenTab.BedInfoCallbackData;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        messagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      }
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        m_bhasUnsavedChanges = false;
        AsyncCallback callback = !asyncState.releaseLock ? new AsyncCallback(asyncState.printer.ShowLockError) : new AsyncCallback(ReleasePrinterAfterCommand);
        var num = (int) asyncState.printer.SetOffsetInfo(callback, asyncState.printer, asyncState.offsets);
      }
      else
      {
        messagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
      }
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock(new AsyncCallback(OnRelease), null);
    }

    private void OnRelease(IAsyncCallResult ar)
    {
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
          messagebox.AddMessageToQueue("Your settings have been applied to the printer.");
          break;
        default:
          messagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
          break;
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      switch (button.ID)
      {
        case 100:
          BLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          BRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          FRO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          FLO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          ZO_Text.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
          m_bhasUnsavedChanges = true;
          break;
        case 101:
          ApplyCalibrationSettings(selectedPrinter, GetCalibrationSettingsFromOptions(selectedPrinter.Info), true);
          break;
        case 102:
          var num1 = float.Parse(ZO_Text.Text) + 0.01f;
          if (num1 > 3.0)
          {
            num1 = 3f;
          }

          ZO_Text.Text = num1.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.Center);
          break;
        case 103:
          var num2 = float.Parse(ZO_Text.Text) - 0.01f;
          if (num2 < -3.0)
          {
            num2 = -3f;
          }

          ZO_Text.Text = num2.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.Center);
          break;
        case 104:
          var num3 = float.Parse(BLO_Text.Text) + 0.05f;
          if (num3 > 3.0)
          {
            num3 = 3f;
          }

          BLO_Text.Text = num3.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.BackLeft);
          break;
        case 105:
          var num4 = float.Parse(BLO_Text.Text) - 0.05f;
          if (num4 < -3.0)
          {
            num4 = -3f;
          }

          BLO_Text.Text = num4.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.BackLeft);
          break;
        case 106:
          var num5 = float.Parse(BRO_Text.Text) + 0.05f;
          if (num5 > 3.0)
          {
            num5 = 3f;
          }

          BRO_Text.Text = num5.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.BackRight);
          break;
        case 107:
          var num6 = float.Parse(BRO_Text.Text) - 0.05f;
          if (num6 < -3.0)
          {
            num6 = -3f;
          }

          BRO_Text.Text = num6.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.BackRight);
          break;
        case 108:
          var num7 = float.Parse(FLO_Text.Text) + 0.05f;
          if (num7 > 3.0)
          {
            num7 = 3f;
          }

          FLO_Text.Text = num7.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.FrontLeft);
          break;
        case 109:
          var num8 = float.Parse(FLO_Text.Text) - 0.05f;
          if (num8 < -3.0)
          {
            num8 = -3f;
          }

          FLO_Text.Text = num8.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.FrontLeft);
          break;
        case 110:
          var num9 = float.Parse(FRO_Text.Text) + 0.05f;
          if (num9 > 3.0)
          {
            num9 = 3f;
          }

          FRO_Text.Text = num9.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.FrontRight);
          break;
        case 111:
          var num10 = float.Parse(FRO_Text.Text) - 0.05f;
          if (num10 < -3.0)
          {
            num10 = -3f;
          }

          FRO_Text.Text = num10.ToString("F2");
          OnOffsetChanged(CatScreenTab.ProbeLocation.FrontRight);
          break;
        case 112:
          MoveToPoint(CatScreenTab.ProbeLocation.Center, true);
          break;
        case 113:
          MoveToPoint(CatScreenTab.ProbeLocation.BackLeft, true);
          break;
        case 114:
          MoveToPoint(CatScreenTab.ProbeLocation.BackRight, true);
          break;
        case 115:
          MoveToPoint(CatScreenTab.ProbeLocation.FrontLeft, true);
          break;
        case 116:
          MoveToPoint(CatScreenTab.ProbeLocation.FrontRight, true);
          break;
      }
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    private void MoveToPoint(CatScreenTab.ProbeLocation location, bool bMoveUp = true)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
      {
        return;
      }

      var compensationPreprocessor = new BedCompensationPreprocessor();
      compensationPreprocessor.UpdateConfigurations(GetCalibrationSettingsFromOptions(selectedPrinter.Info), selectedPrinter.MyPrinterProfile.PrinterSizeConstants);
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
      m_lastProbeLocation = location;
      var stringList = new List<string>
      {
        "M1012",
        "G90"
      };
      if (bMoveUp)
      {
        stringList.Add("G0 Z2");
      }

      stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F3000", vector.x, vector.y));
      stringList.Add(PrinterCompatibleString.Format("G0 Z{0} F100", (object) vector.z));
      stringList.Add("M1011");
      selectedPrinter.SendCommandAutoLock(false, true, new AsyncCallback(AutoLockCallBack), selectedPrinter, stringList.ToArray());
    }

    public void AutoLockCallBack(IAsyncCallResult ar)
    {
      var lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
      {
        return;
      }

      m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
      messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnOffsetChanged(CatScreenTab.ProbeLocation location)
    {
      m_bhasUnsavedChanges = true;
      if (location != m_lastProbeLocation)
      {
        return;
      }

      MoveToPoint(location, false);
    }

    public override void OnOpen()
    {
      m_bhasUnsavedChanges = false;
      m_lastProbeLocation = CatScreenTab.ProbeLocation.Unknown;
    }

    public override void OnClose()
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
      {
        return;
      }

      if (m_bhasUnsavedChanges)
      {
        OnUnsavedChangesCallback(selectedPrinter);
      }
      else
      {
        if (!selectedPrinter.HasLock)
        {
          return;
        }

        var num = (int) selectedPrinter.ReleaseLock(null, null);
      }
    }

    private void OnUnsavedChangesCallback(PrinterObject printer)
    {
      m_ocUnsavedCalibrationValues = GetCalibrationSettingsFromOptions(printer.Info);
      m_bhasUnsavedChanges = false;
      messagebox.AddMessageToQueue("Your calibration changes were not saved to the printer. Would you like to save them now?", "Yes", "No", null, new PopupMessageBox.OnUserSelectionDel(OnUnsavedChangesCallback), printer);
    }

    private void OnUnsavedChangesCallback(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      var printer = user_data as PrinterObject;
      if (printer == null)
      {
        return;
      }

      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        ApplyCalibrationSettings(printer, m_ocUnsavedCalibrationValues, true);
      }
      else
      {
        var num = (int) printer.ReleaseLock(null, null);
      }
    }

    private void ApplyCalibrationSettings(PrinterObject printer, Calibration newSettings, bool releaselock)
    {
      var entireZHeightOffset = newSettings.ENTIRE_Z_HEIGHT_OFFSET;
      var frontRightOffset = newSettings.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      var heightFrontLeftOffset = newSettings.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      var heightBackRightOffset = newSettings.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      var heightBackLeftOffset = newSettings.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      if (printer == null)
      {
        return;
      }

      var num = (int) printer.AcquireLock(new AsyncCallback(SetBedOffsetsAfterLock), new CatScreenTab.BedInfoCallbackData(printer, new BedOffsets(heightBackLeftOffset, heightBackRightOffset, frontRightOffset, heightFrontLeftOffset, entireZHeightOffset), releaselock));
    }

    private Calibration GetCalibrationSettingsFromOptions(PrinterInfo printerInfo)
    {
      return new Calibration(printerInfo.calibration)
      {
        CORNER_HEIGHT_BACK_LEFT_OFFSET = float.Parse(BLO_Text.Text),
        CORNER_HEIGHT_BACK_RIGHT_OFFSET = float.Parse(BRO_Text.Text),
        CORNER_HEIGHT_FRONT_LEFT_OFFSET = float.Parse(FLO_Text.Text),
        CORNER_HEIGHT_FRONT_RIGHT_OFFSET = float.Parse(FRO_Text.Text),
        ENTIRE_Z_HEIGHT_OFFSET = float.Parse(ZO_Text.Text)
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
        return type + "<" + serial + ">";
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
