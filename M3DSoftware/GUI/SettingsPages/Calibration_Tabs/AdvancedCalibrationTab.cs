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
      m_oSettingsManager = main_controller;
      m_oSpoolerConnection = spooler_connection;
      m_oMessagebox = messagebox;
      m_oswWaitToVerifyBusyTimer = new Stopwatch();
    }

    public void Init(GUIHost host)
    {
      m_oMainFrame_BasicCalibration = new XMLFrame();
      var calibrationPanel = Resources.AdvancedCalibrationPanel;
      m_oMainFrame_BasicCalibration.Init(host, calibrationPanel, new ButtonCallback(MyButtonCallback));
      m_oMainFrame_BasicCalibration.SetPosition(0, 0);
      m_oMainFrame_BasicCalibration.RelativeWidth = 1f;
      m_oMainFrame_BasicCalibration.RelativeHeight = 1f;
      AddChildElement((Element2D)m_oMainFrame_BasicCalibration);
      m_G32SettingsCog_button = (ButtonWidget)m_oMainFrame_BasicCalibration.FindChildElement(6);
      m_oebwBacklashX_edit = (EditBoxWidget)m_oMainFrame_BasicCalibration.FindChildElement(7);
      m_oebwBacklashY_edit = (EditBoxWidget)m_oMainFrame_BasicCalibration.FindChildElement(8);
      m_oebwBacklashSpeed_edit = (EditBoxWidget)m_oMainFrame_BasicCalibration.FindChildElement(9);
      m_obwCalibratePrintBorder_button = (ButtonWidget)m_oMainFrame_BasicCalibration.FindChildElement(5);
      m_obwCalibrateBedlocation_button = (ButtonWidget)m_oMainFrame_BasicCalibration.FindChildElement(2);
      m_oebwCalibrationOffset_edit = (EditBoxWidget)m_oMainFrame_BasicCalibration.FindChildElement(15);
      m_oSubFrame_CalibrationOffsetFrame = (Frame)m_oMainFrame_BasicCalibration.FindChildElement(3002);
      m_oMainFrame_G32Calibration = new XMLFrame();
      var calibrationGantryPanel = Resources.AdvancedCalibrationGantryPanel;
      m_oMainFrame_G32Calibration.Init(host, calibrationGantryPanel, new ButtonCallback(MyButtonCallback));
      m_oMainFrame_G32Calibration.Visible = false;
      m_oMainFrame_G32Calibration.Enabled = false;
      m_oMainFrame_G32Calibration.SetPosition(0, 0);
      m_oMainFrame_G32Calibration.RelativeWidth = 1f;
      m_oMainFrame_G32Calibration.RelativeHeight = 1f;
      AddChildElement((Element2D)m_oMainFrame_G32Calibration);
      m_obwCalibrateBedOrientation_button = (ButtonWidget)m_oMainFrame_G32Calibration.FindChildElement(3);
      m_oMainFrame_Busy = new Frame(0);
      m_oMainFrame_Busy.SetPosition(0, 0);
      m_oMainFrame_Busy.RelativeWidth = 1f;
      m_oMainFrame_Busy.RelativeHeight = 1f;
      m_otwStatusText = new TextWidget(1001)
      {
        Text = "",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left,
        VAlignment = TextVerticalAlignment.Top
      };
      m_otwStatusText.SetPosition(16, 16);
      m_otwStatusText.SetSize(500, 320);
      m_otwStatusText.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      m_otwStatusText.Visible = false;
      m_otwStatusText.Enabled = false;
      m_oMainFrame_Busy.AddChildElement((Element2D)m_otwStatusText);
      m_osawProgressWidget = new SpriteAnimationWidget(1);
      m_osawProgressWidget.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      m_osawProgressWidget.SetSize(128, 108);
      m_osawProgressWidget.SetPosition(14, 192);
      m_osawProgressWidget.Visible = false;
      m_osawProgressWidget.CenterHorizontallyInParent = true;
      m_osawProgressWidget.CenterVerticallyInParent = true;
      m_oMainFrame_Busy.AddChildElement((Element2D)m_osawProgressWidget);
      AddChildElement((Element2D)m_oMainFrame_Busy);
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible)
      {
        return;
      }

      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      CheckPrinterStatus();
    }

    public override void OnUpdate()
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (m_opoPreviouslySelectedPrinter != selectedPrinter)
      {
        OnPrinterSwitch(selectedPrinter);
        m_opoPreviouslySelectedPrinter = selectedPrinter;
      }
      if (selectedPrinter != null)
      {
        if (!m_bIsCalibrating && selectedPrinter.Info.Status == PrinterStatus.Firmware_Calibrating)
        {
          ShowBusyCalibratingZ();
        }
        else if (selectedPrinter.isBusy || selectedPrinter.Info.Status == PrinterStatus.Firmware_IsWaitingToPause || (selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing))
        {
          if (!m_bShowingBusy)
          {
            ShowBusyPage();
          }
        }
        else if (!m_bIsCalibrating && !m_bWaitToVerifyNotBusy)
        {
          if (m_bShowingBusy)
          {
            if (!m_bShowingG32ExpertSettingsPage)
            {
              ShowBasicCalibrationPage(selectedPrinter);
            }
            else
            {
              ShowGantryCalibrationG32Page();
            }
          }
        }
        else
        {
          CheckPrinterStatus();
        }
      }
      else
      {
        if (m_bShowingBusy || !m_bShowingG32ExpertSettingsPage)
        {
          ShowBasicCalibrationPage((PrinterObject) null);
        }

        m_oMainFrame_BasicCalibration.Enabled = false;
      }
      base.OnUpdate();
    }

    private void OnPrinterSwitch(PrinterObject new_printer)
    {
      m_bIsCalibrating = false;
      m_bWaitToVerifyNotBusy = false;
      m_bShowingBusy = false;
      m_bShowingG32ExpertSettingsPage = false;
      ShowBusyIfPrinterIsWorking(false);
      GetUpdatedPrinterStats();
    }

    private void GetUpdatedPrinterStats()
    {
      var selectedPrinter = (IPrinter)m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        if (m_oebwBacklashX_edit != null)
        {
          m_oebwBacklashX_edit.Text = selectedPrinter.Info.calibration.BACKLASH_X.ToString();
        }

        if (m_oebwBacklashY_edit != null)
        {
          m_oebwBacklashY_edit.Text = selectedPrinter.Info.calibration.BACKLASH_Y.ToString();
        }

        if (m_oebwBacklashSpeed_edit != null)
        {
          m_oebwBacklashSpeed_edit.Text = selectedPrinter.Info.calibration.BACKLASH_SPEED.ToString();
        }

        if (m_oebwCalibrationOffset_edit == null)
        {
          return;
        }

        m_oebwCalibrationOffset_edit.Text = selectedPrinter.Info.calibration.CALIBRATION_OFFSET.ToString();
      }
      else
      {
        if (m_oebwBacklashX_edit != null)
        {
          m_oebwBacklashX_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        }

        if (m_oebwBacklashY_edit != null)
        {
          m_oebwBacklashY_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        }

        if (m_oebwBacklashSpeed_edit != null)
        {
          m_oebwBacklashSpeed_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
        }

        if (m_oebwCalibrationOffset_edit == null)
        {
          return;
        }

        m_oebwCalibrationOffset_edit.Text = M3DGlobalization.ToLocalString(0.0f, "F2");
      }
    }

    private void CheckPrinterStatus()
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter != null)
      {
        var str = "";
        if (!selectedPrinter.Info.extruder.Z_Valid)
        {
          ShowBasicCalibrationPage(selectedPrinter);
          str = "\n\nSorry. Your printer has lost calibration. This can happen if the printer losses power either during a print or soon after a print has completed. You can not safely print with the printer in this state. You must calibrate the bed location before you can print.";
        }
        if (selectedPrinter.Info.Status == PrinterStatus.Bootloader_InvalidFirmware)
        {
          ShowBasicCalibrationPage(selectedPrinter);
          m_otwStatusText.Text = "Your printer has a problem and needs to be updated before you can use it.";
          m_oMainFrame_Busy.Visible = true;
          m_oMainFrame_BasicCalibration.Visible = true;
          m_oMainFrame_BasicCalibration.Enabled = false;
          m_osawProgressWidget.Visible = false;
        }
        else
        {
          if (selectedPrinter.isBusy || !m_bCanCheckReady || m_oswWaitToVerifyBusyTimer.ElapsedMilliseconds <= 2000L)
          {
            return;
          }

          if ((m_bIsCalibrating || m_bWaitToVerifyNotBusy) && selectedPrinter.Info.Status == PrinterStatus.Firmware_Idle)
          {
            m_bCanCheckReady = false;
            m_bIsCalibrating = false;
            m_bWaitToVerifyNotBusy = false;
            m_bShowingBusy = false;
            m_oswWaitToVerifyBusyTimer.Stop();
            m_oswWaitToVerifyBusyTimer.Reset();
            ShowBasicCalibrationPage(selectedPrinter);
          }
          if (m_bShowingG32ExpertSettingsPage || selectedPrinter.Info.Status != PrinterStatus.Firmware_Idle)
          {
            return;
          }

          ShowBasicCalibrationPage(selectedPrinter);
          m_otwStatusText.Text = str;
        }
      }
      else
      {
        ShowBusyPage();
        m_otwStatusText.Text = "A 3D Printer has not been connected.";
        m_otwStatusText.Enabled = true;
        m_osawProgressWidget.Visible = false;
        m_bIsCalibrating = false;
      }
    }

    private void ShowGantryCalibrationG32Page()
    {
      m_oMainFrame_BasicCalibration.Visible = false;
      m_oMainFrame_BasicCalibration.Enabled = false;
      m_oMainFrame_Busy.Visible = false;
      m_oMainFrame_Busy.Enabled = false;
      m_oMainFrame_G32Calibration.Visible = true;
      m_oMainFrame_G32Calibration.Enabled = true;
      m_bShowingG32ExpertSettingsPage = true;
      m_bShowingBusy = false;
    }

    private void ShowBasicCalibrationPage(PrinterObject printer)
    {
      m_oMainFrame_BasicCalibration.Visible = true;
      m_oMainFrame_BasicCalibration.Enabled = true;
      m_oMainFrame_Busy.Visible = false;
      m_oMainFrame_Busy.Enabled = false;
      m_oMainFrame_G32Calibration.Visible = false;
      m_oMainFrame_G32Calibration.Enabled = false;
      m_bShowingG32ExpertSettingsPage = false;
      m_bShowingBusy = false;
      var flag1 = true;
      var flag2 = true;
      if (printer == null)
      {
        return;
      }

      if (printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        var featureSlot1 = printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Multi Point Automatic Bed Calibration");
        var featureSlot2 = printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
        if (featureSlot1 >= 0)
        {
          flag1 = (uint) printer.Info.supportedFeatures.GetStatus(featureSlot1) > 0U;
        }

        if (featureSlot2 >= 0)
        {
          flag2 = (uint) printer.Info.supportedFeatures.GetStatus(featureSlot2) > 0U;
        }
      }
      m_oSubFrame_CalibrationOffsetFrame.Visible = printer.Info.calibration.UsesCalibrationOffset;
      m_obwCalibrateBedlocation_button.Enabled = flag2;
      m_obwCalibrateBedOrientation_button.Enabled = flag1;
      m_obwCalibratePrintBorder_button.Enabled = flag1;
      m_G32SettingsCog_button.Enabled = flag1;
    }

    private void ShowBusyPage()
    {
      m_oMainFrame_BasicCalibration.Visible = false;
      m_oMainFrame_BasicCalibration.Enabled = false;
      m_oMainFrame_Busy.Visible = true;
      m_oMainFrame_Busy.Enabled = true;
      m_osawProgressWidget.Visible = true;
      m_otwStatusText.Visible = true;
      m_oMainFrame_G32Calibration.Visible = false;
      m_oMainFrame_G32Calibration.Enabled = false;
      m_bShowingBusy = true;
    }

    public void ShowBusyCalibratingZ()
    {
      m_bCanCheckReady = false;
      CheckPrinterStatus();
      if (m_oSpoolerConnection.SelectedPrinter == null)
      {
        return;
      }

      ShowBusyPage();
      m_otwStatusText.Text = "Calibrating the 3D printer. Please do not disconnect the printer from the computer or unplug it. Please keep the printer extremely still without vibrations or tilts. It may be silent for up to a minute or more at a time but it is still calibrating and should not be disturbed by motion or vibration.";
      m_bIsCalibrating = true;
      m_bCanCheckReady = true;
      m_oswWaitToVerifyBusyTimer.Reset();
      m_oswWaitToVerifyBusyTimer.Start();
    }

    public void ShowBusyIfPrinterIsWorking(bool waitToVerifyNotBusy)
    {
      ShowBusyPage();
      m_bWaitToVerifyNotBusy = waitToVerifyNotBusy;
      m_bCanCheckReady = false;
      CheckPrinterStatus();
      if (m_oSpoolerConnection.SelectedPrinter == null)
      {
        return;
      }

      m_otwStatusText.Text = "Please wait while the printer completes its operation.";
      m_bCanCheckReady = true;
      m_oswWaitToVerifyBusyTimer.Reset();
      m_oswWaitToVerifyBusyTimer.Start();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject printer = m_oSpoolerConnection.SelectedPrinter;
      switch (button.ID)
      {
        case 2:
          if (printer == null || printer.Info.InBootloaderMode)
          {
            break;
          }

          var num1 = (int) printer.AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
          {
            if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
            {
              m_oMessagebox.AddMessageToQueue("Unable to calibrate the printer because it is being used by another process.");
            }
            else if (ar.CallResult != CommandResult.Success_LockAcquired)
            {
              m_oMessagebox.AddMessageToQueue("There was a problem sending data to the printer. Please try again");
            }
            else
            {
              printer.CalibrateBed(new M3D.Spooling.Client.AsyncCallback(ReleaseAfterOperation), (object) printer, PrinterObject.CalibrationType.CalibrateQuick_G30);
            }
          }), (object) printer);
          break;
        case 3:
          if (printer == null || printer.Info.InBootloaderMode)
          {
            break;
          }

          var num2 = (int) printer.AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
          {
            if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
            {
              m_oMessagebox.AddMessageToQueue("Unable to calibrate the printer because it is being used by another process.");
            }
            else if (ar.CallResult != CommandResult.Success_LockAcquired)
            {
              m_oMessagebox.AddMessageToQueue("There was a problem sending data to the printer. Please try again");
            }
            else
            {
              printer.CalibrateBed(new M3D.Spooling.Client.AsyncCallback(ReleaseAfterOperation), (object) printer, PrinterObject.CalibrationType.CalibrateFull_G32);
            }
          }), (object) printer);
          break;
        case 4:
          var fileName = Paths.DebugLogPath(DateTime.Now.Ticks / 10000L);
          Form1.debugLogger.Print(fileName);
          break;
        case 5:
          if (printer.GetCurrentFilament() == (FilamentSpool) null)
          {
            m_oMessagebox.AddMessageToQueue("Please insert filament into your printer.");
            break;
          }
          if (!printer.Info.extruder.Z_Valid || !printer.Info.calibration.Calibration_Valid)
          {
            m_oMessagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NotCalibrated"));
          }

          ShowBusyIfPrinterIsWorking(true);
          var num3 = (int) printer.AcquireLock(new M3D.Spooling.Client.AsyncCallback(DoTestBorderPrintOnLock), (object) printer);
          break;
        case 6:
          ShowGantryCalibrationG32Page();
          break;
        case 10:
          OnApplyBacklashSettings();
          break;
        case 11:
          if (printer == null || printer.Info.InBootloaderMode)
          {
            break;
          }

          m_oebwBacklashSpeed_edit.Text = printer.MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed.ToString();
          break;
        case 12:
          if (printer == null || printer.Info.InBootloaderMode)
          {
            break;
          }

          m_oebwBacklashSpeed_edit.Text = printer.MyPrinterProfile.SpeedLimitConstants.FastestPossible.ToString();
          break;
        case 13:
          ShowBasicCalibrationPage(printer);
          break;
        case 16:
          OnApplyCalibrationOffset();
          break;
      }
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(OnRelease), (object) null);
    }

    private void OnRelease(IAsyncCallResult ar)
    {
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
          m_oMessagebox.AddMessageToQueue("Your settings have been applied to the printer.");
          break;
        default:
          m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
          break;
      }
    }

    private void ReleaseAfterOperation(IAsyncCallResult ar)
    {
      var num = (int) (ar.AsyncState as PrinterObject).ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private void DoTestBorderPrintOnLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        throw new Exception("Big bad C# exception");
      }

      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num = (int) asyncState.PrintTestBorder(new M3D.Spooling.Client.AsyncCallback(asyncState.ShowLockError), (object) asyncState);
      }
      else
      {
        asyncState.ShowLockError(ar);
      }
    }

    private void OnApplyBacklashSettings()
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        m_oMessagebox.AddMessageToQueue("You must connect a printer first.");
      }
      else
      {
        var fastestPossible = selectedPrinter.MyPrinterProfile.SpeedLimitConstants.FastestPossible;
        var result1 = -1f;
        var result2 = -1f;
        var result3 = -1f;
        if (!float.TryParse(m_oebwBacklashX_edit.Text, out result1))
        {
          m_oMessagebox.AddMessageToQueue("Sorry, but the X backlash is not a number. Please try again.");
        }
        else if ((double) result1 < 0.0 || (double) result1 > 3.0)
        {
          m_oMessagebox.AddMessageToQueue("Sorry. Backlash X values must be between 0 mm and " + (object) 3f + " mm");
        }
        else if (!float.TryParse(m_oebwBacklashY_edit.Text, out result2))
        {
          m_oMessagebox.AddMessageToQueue("Sorry, but the Y backlash is not a number. Please try again.");
        }
        else if ((double) result2 < 0.0 || (double) result2 > 3.0)
        {
          m_oMessagebox.AddMessageToQueue("Sorry. Backlash Y values must be between 0 mm and " + (object) 3f + " mm");
        }
        else if (!float.TryParse(m_oebwBacklashSpeed_edit.Text, out result3))
        {
          m_oMessagebox.AddMessageToQueue("Sorry, but the backlash speed is not a number. Please try again.");
        }
        else if ((double) result3 < 500.0 || (double) result3 > (double) fastestPossible)
        {
          m_oMessagebox.AddMessageToQueue(string.Format("Sorry. Backlash speed must be between {0} and {1} mm/min.", (object) 500f, (object) fastestPossible));
        }
        else
        {
          var num = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(SetBacklashAfterLock), (object) new AdvancedCalibrationTab.BacklashDetails(selectedPrinter, result1, result2, result3));
        }
      }
    }

    private void SetBacklashAfterLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as AdvancedCalibrationTab.BacklashDetails;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        m_oMessagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      }
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num = (int) asyncState.printer.SetBacklash(new M3D.Spooling.Client.AsyncCallback(ReleasePrinterAfterCommand), (object) asyncState.printer, new BacklashSettings(asyncState.backlash_x, asyncState.backlash_y, asyncState.backlash_speed));
      }
      else
      {
        m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
      }
    }

    private void OnApplyCalibrationOffset()
    {
      PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        m_oMessagebox.AddMessageToQueue("You must connect a printer first.");
      }
      else
      {
        var num1 = 2f;
        var num2 = -1f;
        if (!float.TryParse(m_oebwCalibrationOffset_edit.Text, out var result))
        {
          m_oMessagebox.AddMessageToQueue("Sorry, but the 'Calibration Offset' is not a number. Please try again.");
        }
        else if ((double)num2 > (double)result || (double)num1 < (double)result)
        {
          m_oMessagebox.AddMessageToQueue(string.Format("Sorry. The 'Calibration Offset' must be between {0} and {1} mm.", (object)num2, (object)num1));
        }
        else
        {
          var num3 = (int)selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(SetCalibrationOffsetAfterLock), (object)new AdvancedCalibrationTab.CalibrationDetails(selectedPrinter, result));
        }
      }
    }

    private void SetCalibrationOffsetAfterLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as AdvancedCalibrationTab.CalibrationDetails;
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        m_oMessagebox.AddMessageToQueue("Unable to send data to the printer because it is being used by another process.");
      }
      else if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num = (int) asyncState.printer.SetCalibrationOffset(new M3D.Spooling.Client.AsyncCallback(ReleasePrinterAfterCommand), (object) asyncState.printer, asyncState.calibration_offset);
      }
      else
      {
        m_oMessagebox.AddMessageToQueue("Unable to send data to the printer. Please try again.");
      }
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
