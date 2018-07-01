// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Calibration_Tabs.AdvancedCalibrationTab
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System;
using System.Diagnostics;

namespace M3D.GUI.SettingsPages.Calibration_Tabs
{
  public class AdvancedCalibrationTab : SettingsPage
  {
    private bool m_bCanCheckReady = true;
    private const float MAX_BACKLASH = 3f;
    private const float MIN_BACKLASH_SPEED = 500f;
    private bool m_bShowingBusy;
    private bool m_bShowingG32ExpertSettingsPage;
    private bool m_bWaitToVerifyNotBusy;
    private Stopwatch m_oswWaitToVerifyBusyTimer;
    private PopupMessageBox m_oMessagebox;
    private SettingsManager m_oSettingsManager;
    private SpoolerConnection m_oSpoolerConnection;
    private TextWidget m_otwStatusText;
    private SpriteAnimationWidget m_osawProgressWidget;
    private ButtonWidget m_obwCalibrateBedlocation_button;
    private ButtonWidget m_obwCalibrateBedOrientation_button;
    private ButtonWidget m_G32SettingsCog_button;
    private EditBoxWidget m_oebwBacklashX_edit;
    private EditBoxWidget m_oebwBacklashY_edit;
    private EditBoxWidget m_oebwBacklashSpeed_edit;
    private EditBoxWidget m_oebwCalibrationOffset_edit;
    private XMLFrame m_oMainFrame_BasicCalibration;
    private XMLFrame m_oMainFrame_G32Calibration;
    private Frame m_oMainFrame_Busy;
    private Frame m_oSubFrame_CalibrationOffsetFrame;
    private ButtonWidget m_obwCalibratePrintBorder_button;
    private bool m_bIsCalibrating;
    private PrinterObject m_opoPreviouslySelectedPrinter;

    public AdvancedCalibrationTab(int ID, SettingsManager main_controller, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.m_oSettingsManager = main_controller;
      this.m_oSpoolerConnection = spooler_connection;
      this.m_oMessagebox = messagebox;
      this.m_oswWaitToVerifyBusyTimer = new Stopwatch();
    }

    public void Init(GUIHost host)
    {
      this.m_oMainFrame_BasicCalibration = new XMLFrame();
      string calibrationPanel = Resources.AdvancedCalibrationPanel;
      this.m_oMainFrame_BasicCalibration.Init(host, calibrationPanel, new ButtonCallback(this.MyButtonCallback));
      this.m_oMainFrame_BasicCalibration.SetPosition(0, 0);
      this.m_oMainFrame_BasicCalibration.RelativeWidth = 1f;
      this.m_oMainFrame_BasicCalibration.RelativeHeight = 1f;
      this.AddChildElement((Element2D) this.m_oMainFrame_BasicCalibration);
      this.m_G32SettingsCog_button = (ButtonWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(6);
      this.m_oebwBacklashX_edit = (EditBoxWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(7);
      this.m_oebwBacklashY_edit = (EditBoxWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(8);
      this.m_oebwBacklashSpeed_edit = (EditBoxWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(9);
      this.m_obwCalibratePrintBorder_button = (ButtonWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(5);
      this.m_obwCalibrateBedlocation_button = (ButtonWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(2);
      this.m_oebwCalibrationOffset_edit = (EditBoxWidget) this.m_oMainFrame_BasicCalibration.FindChildElement(15);
      this.m_oSubFrame_CalibrationOffsetFrame = (Frame) this.m_oMainFrame_BasicCalibration.FindChildElement(3002);
      this.m_oMainFrame_G32Calibration = new XMLFrame();
      string calibrationGantryPanel = Resources.AdvancedCalibrationGantryPanel;
      this.m_oMainFrame_G32Calibration.Init(host, calibrationGantryPanel, new ButtonCallback(this.MyButtonCallback));
      this.m_oMainFrame_G32Calibration.Visible = false;
      this.m_oMainFrame_G32Calibration.Enabled = false;
      this.m_oMainFrame_G32Calibration.SetPosition(0, 0);
      this.m_oMainFrame_G32Calibration.RelativeWidth = 1f;
      this.m_oMainFrame_G32Calibration.RelativeHeight = 1f;
      this.AddChildElement((Element2D) this.m_oMainFrame_G32Calibration);
      this.m_obwCalibrateBedOrientation_button = (ButtonWidget) this.m_oMainFrame_G32Calibration.FindChildElement(3);
      this.m_oMainFrame_Busy = new Frame(0);
      this.m_oMainFrame_Busy.SetPosition(0, 0);
      this.m_oMainFrame_Busy.RelativeWidth = 1f;
      this.m_oMainFrame_Busy.RelativeHeight = 1f;
      this.m_otwStatusText = new TextWidget(1001);
      this.m_otwStatusText.Text = "";
      this.m_otwStatusText.Size = FontSize.Medium;
      this.m_otwStatusText.Alignment = QFontAlignment.Left;
      this.m_otwStatusText.VAlignment = TextVerticalAlignment.Top;
      this.m_otwStatusText.SetPosition(16, 16);
      this.m_otwStatusText.SetSize(500, 320);
      this.m_otwStatusText.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.m_otwStatusText.Visible = false;
      this.m_otwStatusText.Enabled = false;
      this.m_oMainFrame_Busy.AddChildElement((Element2D) this.m_otwStatusText);
      this.m_osawProgressWidget = new SpriteAnimationWidget(1);
      this.m_osawProgressWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      this.m_osawProgressWidget.SetSize(128, 108);
      this.m_osawProgressWidget.SetPosition(14, 192);
      this.m_osawProgressWidget.Visible = false;
      this.m_osawProgressWidget.CenterHorizontallyInParent = true;
      this.m_osawProgressWidget.CenterVerticallyInParent = true;
      this.m_oMainFrame_Busy.AddChildElement((Element2D) this.m_osawProgressWidget);
      this.AddChildElement((Element2D) this.m_oMainFrame_Busy);
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible)
        return;
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      this.CheckPrinterStatus();
    }

    public override void OnUpdate()
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (this.m_opoPreviouslySelectedPrinter != selectedPrinter)
      {
        this.OnPrinterSwitch(selectedPrinter);
        this.m_opoPreviouslySelectedPrinter = selectedPrinter;
      }
      if (selectedPrinter != null)
      {
        if (!this.m_bIsCalibrating && selectedPrinter.Info.Status == PrinterStatus.Firmware_Calibrating)
          this.ShowBusyCalibratingZ();
        else if (selectedPrinter.isBusy || selectedPrinter.Info.Status == PrinterStatus.Firmware_IsWaitingToPause || (selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing))
        {
          if (!this.m_bShowingBusy)
            this.ShowBusyPage();
        }
        else if (!this.m_bIsCalibrating && !this.m_bWaitToVerifyNotBusy)
        {
          if (this.m_bShowingBusy)
          {
            if (!this.m_bShowingG32ExpertSettingsPage)
              this.ShowBasicCalibrationPage(selectedPrinter);
            else
              this.ShowGantryCalibrationG32Page();
          }
        }
        else
          this.CheckPrinterStatus();
      }
      else
      {
        if (this.m_bShowingBusy || !this.m_bShowingG32ExpertSettingsPage)
          this.ShowBasicCalibrationPage((PrinterObject) null);
        this.m_oMainFrame_BasicCalibration.Enabled = false;
      }
      base.OnUpdate();
    }

    private void OnPrinterSwitch(PrinterObject new_printer)
    {
      this.m_bIsCalibrating = false;
      this.m_bWaitToVerifyNotBusy = false;
      this.m_bShowingBusy = false;
      this.m_bShowingG32ExpertSettingsPage = false;
      this.ShowBusyIfPrinterIsWorking(false);
      this.GetUpdatedPrinterStats();
    }

    private void GetUpdatedPrinterStats()
    {
      IPrinter selectedPrinter = (IPrinter) this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        if (this.m_oebwBacklashX_edit != null)
          this.m_oebwBacklashX_edit.Text = selectedPrinter.Info.calibration.BACKLASH_X.ToString();
        if (this.m_oebwBacklashY_edit != null)
          this.m_oebwBacklashY_edit.Text = selectedPrinter.Info.calibration.BACKLASH_Y.ToString();
        if (this.m_oebwBacklashSpeed_edit != null)
          this.m_oebwBacklashSpeed_edit.Text = selectedPrinter.Info.calibration.BACKLASH_SPEED.ToString();
        if (this.m_oebwCalibrationOffset_edit == null)
          return;
        this.m_oebwCalibrationOffset_edit.Text = selectedPrinter.Info.calibration.CALIBRATION_OFFSET.ToString();
      }
      else
      {
        if (this.m_oebwBacklashX_edit != null)
          this.m_oebwBacklashX_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        if (this.m_oebwBacklashY_edit != null)
          this.m_oebwBacklashY_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        if (this.m_oebwBacklashSpeed_edit != null)
          this.m_oebwBacklashSpeed_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        if (this.m_oebwCalibrationOffset_edit == null)
          return;
        this.m_oebwCalibrationOffset_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
      }
    }

    private void CheckPrinterStatus()
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        string str = "";
        if (!selectedPrinter.Info.extruder.Z_Valid)
        {
          this.ShowBasicCalibrationPage(selectedPrinter);
          str = "\n\nSorry. Your printer has lost calibration. This can happen if the printer losses power either during a print or soon after a print has completed. You can not safely print with the printer in this state. You must calibrate the bed location before you can print.";
        }
        if (selectedPrinter.Info.Status == PrinterStatus.Bootloader_InvalidFirmware)
        {
          this.ShowBasicCalibrationPage(selectedPrinter);
          this.m_otwStatusText.Text = "Your printer has a problem and needs to be updated before you can use it.";
          this.m_oMainFrame_Busy.Visible = true;
          this.m_oMainFrame_BasicCalibration.Visible = true;
          this.m_oMainFrame_BasicCalibration.Enabled = false;
          this.m_osawProgressWidget.Visible = false;
        }
        else
        {
          if (selectedPrinter.isBusy || !this.m_bCanCheckReady || this.m_oswWaitToVerifyBusyTimer.ElapsedMilliseconds <= 2000L)
            return;
          if ((this.m_bIsCalibrating || this.m_bWaitToVerifyNotBusy) && selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle)
          {
            this.m_bCanCheckReady = false;
            this.m_bIsCalibrating = false;
            this.m_bWaitToVerifyNotBusy = false;
            this.m_bShowingBusy = false;
            this.m_oswWaitToVerifyBusyTimer.Stop();
            this.m_oswWaitToVerifyBusyTimer.Reset();
            this.ShowBasicCalibrationPage(selectedPrinter);
          }
          if (this.m_bShowingG32ExpertSettingsPage || selectedPrinter.Info.Status != PrinterStatus.Firmware_Idle)
            return;
          this.ShowBasicCalibrationPage(selectedPrinter);
          this.m_otwStatusText.Text = str;
        }
      }
      else
      {
        this.ShowBusyPage();
        this.m_otwStatusText.Text = "A 3D Printer has not been connected.";
        this.m_otwStatusText.Enabled = true;
        this.m_osawProgressWidget.Visible = false;
        this.m_bIsCalibrating = false;
      }
    }

    private void ShowGantryCalibrationG32Page()
    {
      this.m_oMainFrame_BasicCalibration.Visible = false;
      this.m_oMainFrame_BasicCalibration.Enabled = false;
      this.m_oMainFrame_Busy.Visible = false;
      this.m_oMainFrame_Busy.Enabled = false;
      this.m_oMainFrame_G32Calibration.Visible = true;
      this.m_oMainFrame_G32Calibration.Enabled = true;
      this.m_bShowingG32ExpertSettingsPage = true;
      this.m_bShowingBusy = false;
    }

    private void ShowBasicCalibrationPage(PrinterObject printer)
    {
      this.m_oMainFrame_BasicCalibration.Visible = true;
      this.m_oMainFrame_BasicCalibration.Enabled = true;
      this.m_oMainFrame_Busy.Visible = false;
      this.m_oMainFrame_Busy.Enabled = false;
      this.m_oMainFrame_G32Calibration.Visible = false;
      this.m_oMainFrame_G32Calibration.Enabled = false;
      this.m_bShowingG32ExpertSettingsPage = false;
      this.m_bShowingBusy = false;
      bool flag1 = true;
      bool flag2 = true;
      if (printer == null)
        return;
      if (printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        int featureSlot1 = printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
        int featureSlot2 = printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
        if (featureSlot1 >= 0)
          flag1 = (uint) printer.Info.supportedFeatures.GetStatus(featureSlot1) > 0U;
        if (featureSlot2 >= 0)
          flag2 = (uint) printer.Info.supportedFeatures.GetStatus(featureSlot2) > 0U;
      }
      this.m_oSubFrame_CalibrationOffsetFrame.Visible = printer.Info.calibration.UsesCalibrationOffset;
      this.m_obwCalibrateBedlocation_button.Enabled = flag2;
      this.m_obwCalibrateBedOrientation_button.Enabled = flag1;
      this.m_obwCalibratePrintBorder_button.Enabled = flag1;
      this.m_G32SettingsCog_button.Enabled = flag1;
    }

    private void ShowBusyPage()
    {
      this.m_oMainFrame_BasicCalibration.Visible = false;
      this.m_oMainFrame_BasicCalibration.Enabled = false;
      this.m_oMainFrame_Busy.Visible = true;
      this.m_oMainFrame_Busy.Enabled = true;
      this.m_osawProgressWidget.Visible = true;
      this.m_otwStatusText.Visible = true;
      this.m_oMainFrame_G32Calibration.Visible = false;
      this.m_oMainFrame_G32Calibration.Enabled = false;
      this.m_bShowingBusy = true;
    }

    public void ShowBusyCalibratingZ()
    {
      this.m_bCanCheckReady = false;
      this.CheckPrinterStatus();
      if (this.m_oSpoolerConnection.SelectedPrinter == null)
        return;
      this.ShowBusyPage();
      this.m_otwStatusText.Text = "Calibrating the 3D printer. Please do not disconnect the printer from the computer or unplug it. Please keep the printer extremely still without vibrations or tilts. It may be silent for up to a minute or more at a time but it is still calibrating and should not be disturbed by motion or vibration.";
      this.m_bIsCalibrating = true;
      this.m_bCanCheckReady = true;
      this.m_oswWaitToVerifyBusyTimer.Reset();
      this.m_oswWaitToVerifyBusyTimer.Start();
    }

    public void ShowBusyIfPrinterIsWorking(bool waitToVerifyNotBusy)
    {
      this.ShowBusyPage();
      this.m_bWaitToVerifyNotBusy = waitToVerifyNotBusy;
      this.m_bCanCheckReady = false;
      this.CheckPrinterStatus();
      if (this.m_oSpoolerConnection.SelectedPrinter == null)
        return;
      this.m_otwStatusText.Text = "Please wait while the printer completes its operation.";
      this.m_bCanCheckReady = true;
      this.m_oswWaitToVerifyBusyTimer.Reset();
      this.m_oswWaitToVerifyBusyTimer.Start();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject printer = this.m_oSpoolerConnection.SelectedPrinter;
      switch (button.ID)
      {
        case 2:
          if (printer == null || printer.Info.InBootloaderMode)
            break;
          int num1 = (int) printer.AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
          {
            if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
              this.m_oMessagebox.AddMessageToQueue("Unable to calibrate the printer because it is being used by another process.");
            else if (ar.CallResult != CommandResult.Success_LockAcquired)
              this.m_oMessagebox.AddMessageToQueue("There was a problem sending data to the printer. Please try again");
            else
              printer.CalibrateBed(new M3D.Spooling.Client.AsyncCallback(this.ReleaseAfterOperation), (object) printer, PrinterObject.CalibrationType.CalibrateQuick_G30);
          }), (object) printer);
          break;
        case 3:
          if (printer == null || printer.Info.InBootloaderMode)
            break;
          int num2 = (int) printer.AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
          {
            if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
              this.m_oMessagebox.AddMessageToQueue("Unable to calibrate the printer because it is being used by another process.");
            else if (ar.CallResult != CommandResult.Success_LockAcquired)
              this.m_oMessagebox.AddMessageToQueue("There was a problem sending data to the printer. Please try again");
            else
              printer.CalibrateBed(new M3D.Spooling.Client.AsyncCallback(this.ReleaseAfterOperation), (object) printer, PrinterObject.CalibrationType.CalibrateFull_G32);
          }), (object) printer);
          break;
        case 4:
          string fileName = Paths.DebugLogPath(DateTime.Now.Ticks / 10000L);
          Form1.debugLogger.Print(fileName);
          break;
        case 5:
          if (printer.GetCurrentFilament() == (FilamentSpool) null)
          {
            this.m_oMessagebox.AddMessageToQueue("Please insert filament into your printer.");
            break;
          }
          if (!printer.Info.extruder.Z_Valid || !printer.Info.calibration.Calibration_Valid)
            this.m_oMessagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NotCalibrated"));
          this.ShowBusyIfPrinterIsWorking(true);
          int num3 = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.DoTestBorderPrintOnLock), (object) printer);
          break;
        case 6:
          this.ShowGantryCalibrationG32Page();
          break;
        case 10:
          this.OnApplyBacklashSettings();
          break;
        case 11:
          if (printer == null || printer.Info.InBootloaderMode)
            break;
          this.m_oebwBacklashSpeed_edit.Text = printer.MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed.ToString();
          break;
        case 12:
          if (printer == null || printer.Info.InBootloaderMode)
            break;
          this.m_oebwBacklashSpeed_edit.Text = printer.MyPrinterProfile.SpeedLimitConstants.FastestPossible.ToString();
          break;
        case 13:
          this.ShowBasicCalibrationPage(printer);
          break;
        case 16:
          this.OnApplyCalibrationOffset();
          break;
      }
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
        return;
      int num = (int) asyncState.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(this.OnRelease), (object) null);
    }

    private void OnRelease(IAsyncCallResult ar)
    {
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
          this.m_oMessagebox.AddMessageToQueue("Your settings have been applied to the printer.");
          break;
        default:
          this.m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
          break;
      }
    }

    private void ReleaseAfterOperation(IAsyncCallResult ar)
    {
      int num = (int) (ar.AsyncState as PrinterObject).ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private void DoTestBorderPrintOnLock(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        throw new Exception("Big bad C# exception");
      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        int num = (int) asyncState.PrintTestBorder(new M3D.Spooling.Client.AsyncCallback(asyncState.ShowLockError), (object) asyncState);
      }
      else
        asyncState.ShowLockError(ar);
    }

    private void OnApplyBacklashSettings()
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        this.m_oMessagebox.AddMessageToQueue("You must connect a printer first.");
      }
      else
      {
        float fastestPossible = selectedPrinter.MyPrinterProfile.SpeedLimitConstants.FastestPossible;
        float result1 = -1f;
        float result2 = -1f;
        float result3 = -1f;
        if (!float.TryParse(this.m_oebwBacklashX_edit.Text, out result1))
          this.m_oMessagebox.AddMessageToQueue("Sorry, but the X backlash is not a number. Please try again.");
        else if ((double) result1 < 0.0 || (double) result1 > 3.0)
          this.m_oMessagebox.AddMessageToQueue("Sorry. Backlash X values must be between 0 mm and " + (object) 3f + " mm");
        else if (!float.TryParse(this.m_oebwBacklashY_edit.Text, out result2))
          this.m_oMessagebox.AddMessageToQueue("Sorry, but the Y backlash is not a number. Please try again.");
        else if ((double) result2 < 0.0 || (double) result2 > 3.0)
          this.m_oMessagebox.AddMessageToQueue("Sorry. Backlash Y values must be between 0 mm and " + (object) 3f + " mm");
        else if (!float.TryParse(this.m_oebwBacklashSpeed_edit.Text, out result3))
          this.m_oMessagebox.AddMessageToQueue("Sorry, but the backlash speed is not a number. Please try again.");
        else if ((double) result3 < 500.0 || (double) result3 > (double) fastestPossible)
        {
          this.m_oMessagebox.AddMessageToQueue(string.Format("Sorry. Backlash speed must be between {0} and {1} mm/min.", (object) 500f, (object) fastestPossible));
        }
        else
        {
          int num = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.SetBacklashAfterLock), (object) new AdvancedCalibrationTab.BacklashDetails(selectedPrinter, result1, result2, result3));
        }
      }
    }

    private void SetBacklashAfterLock(IAsyncCallResult ar)
    {
      AdvancedCalibrationTab.BacklashDetails asyncState = ar.AsyncState as AdvancedCalibrationTab.BacklashDetails;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
        this.m_oMessagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        int num = (int) asyncState.printer.SetBacklash(new M3D.Spooling.Client.AsyncCallback(this.ReleasePrinterAfterCommand), (object) asyncState.printer, new BacklashSettings(asyncState.backlash_x, asyncState.backlash_y, asyncState.backlash_speed));
      }
      else
        this.m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
    }

    private void OnApplyCalibrationOffset()
    {
      PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        this.m_oMessagebox.AddMessageToQueue("You must connect a printer first.");
      }
      else
      {
        float num1 = 2f;
        float num2 = -1f;
        float result;
        if (!float.TryParse(this.m_oebwCalibrationOffset_edit.Text, out result))
          this.m_oMessagebox.AddMessageToQueue("Sorry, but the 'Calibration Offset' is not a number. Please try again.");
        else if ((double) num2 > (double) result || (double) num1 < (double) result)
        {
          this.m_oMessagebox.AddMessageToQueue(string.Format("Sorry. The 'Calibration Offset' must be between {0} and {1} mm.", (object) num2, (object) num1));
        }
        else
        {
          int num3 = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.SetCalibrationOffsetAfterLock), (object) new AdvancedCalibrationTab.CalibrationDetails(selectedPrinter, result));
        }
      }
    }

    private void SetCalibrationOffsetAfterLock(IAsyncCallResult ar)
    {
      AdvancedCalibrationTab.CalibrationDetails asyncState = ar.AsyncState as AdvancedCalibrationTab.CalibrationDetails;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
        this.m_oMessagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        int num = (int) asyncState.printer.SetCalibrationOffset(new M3D.Spooling.Client.AsyncCallback(this.ReleasePrinterAfterCommand), (object) asyncState.printer, asyncState.calibration_offset);
      }
      else
        this.m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
    }

    private enum ControlIDs
    {
      FirmwareProgress = 1,
      CalibrateBedLocationButton = 2,
      CalibrateBedOrientationButton = 3,
      PrintDebugLogButton = 4,
      CalibratePrintBorderButton = 5,
      ExpertSettings = 6,
      BacklashX = 7,
      BacklashY = 8,
      BacklashSpeed = 9,
      BacklashApply = 10, // 0x0000000A
      BacklashDefault = 11, // 0x0000000B
      BacklashMax = 12, // 0x0000000C
      BackButton = 13, // 0x0000000D
      SettingsWindowTitle = 14, // 0x0000000E
      CalibrationOffset = 15, // 0x0000000F
      ApplyCalibrationOffset = 16, // 0x00000010
      FirmwareText = 1001, // 0x000003E9
      BacklashSettingsFrame = 2000, // 0x000007D0
      GantrySettingsFrame = 3000, // 0x00000BB8
      CalibrationButtonsFrame = 3001, // 0x00000BB9
      CalibrationOffsetsFrame = 3002, // 0x00000BBA
    }

    private class BacklashDetails
    {
      public PrinterObject printer;
      public float backlash_x;
      public float backlash_y;
      public float backlash_speed;

      public BacklashDetails(PrinterObject printer, float backlash_x, float backlash_y, float backlash_speed)
      {
        this.printer = printer;
        this.backlash_x = backlash_x;
        this.backlash_y = backlash_y;
        this.backlash_speed = backlash_speed;
      }
    }

    private class CalibrationDetails
    {
      public PrinterObject printer;
      public float calibration_offset;

      public CalibrationDetails(PrinterObject printer, float calibration_offset)
      {
        this.printer = printer;
        this.calibration_offset = calibration_offset;
      }
    }
  }
}
