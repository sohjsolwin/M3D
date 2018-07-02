// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.PrinterObject
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Tools;
using M3D.GUI.Views;
using M3D.Spooling.Client;
using M3D.Spooling.Client.Extensions;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace M3D.GUI.Controller
{
  public class PrinterObject : PrinterDecorator
  {
    private ThreadSafeVariable<bool> m_tsbFeaturePanelShown = new ThreadSafeVariable<bool>(false);
    private bool readyAndFilamentChecked;
    private const string MicroFamilyName = "Micro";
    private const string ProFamilyName = "Pro";
    private Stopwatch update_firmware_timer;
    private bool ok_sent;
    private bool m_bPowerOutageRecoveryDetected;
    private ThreadSafeVariable<PrinterObject.State> printerState;
    private ThreadSafeVariable<bool> marked_as_busy;
    private bool calibration_message_sent;
    private bool calibration_invalid;
    private bool firmware_message_sent;
    private bool invalid_firmware;
    private bool gantry_clip_message_sent;
    private bool heater_on;
    private object heater_lock;
    private bool was_calibrating;
    private bool m_bSupportedFeaturesChecked;
    private ThreadSafeVariable<bool> printer_shutdown;
    private ThreadSafeVariable<bool> printer_cyclepower;
    private MessagePopUp infobox;
    private PopupMessageBox messagebox;
    private SpoolerConnection spooler_connection;
    private SettingsManager settings_manager;
    private FilamentProfile filament_profile;
    private Stopwatch heater_counter;
    private bool heater_time_vested;

    public PrinterObject(Printer base_printer, PopupMessageBox messagebox, MessagePopUp infobox, SpoolerConnection spooler_connection, SettingsManager settings_manager)
      : base((IPrinter) base_printer)
    {
      this.infobox = infobox;
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      this.settings_manager = settings_manager;
      HasHeatedBed = false;
      invalid_firmware = false;
      calibration_invalid = false;
      heater_time_vested = false;
      heater_on = false;
      firmware_message_sent = false;
      gantry_clip_message_sent = false;
      calibration_message_sent = false;
      m_bSupportedFeaturesChecked = false;
      heater_lock = new object();
      printerState = new ThreadSafeVariable<PrinterObject.State>
      {
        Value = GetStateFromPrinter()
      };
      marked_as_busy = new ThreadSafeVariable<bool>
      {
        Value = false
      };
      heater_counter = new Stopwatch();
      update_firmware_timer = new Stopwatch();
      base_printer.OnUpdateData += new OnUpdateDataDel(OnUpdateData);
      base_printer.OnProcessSpoolerMessage += new OnProcessSpoolerMessageDel(OnProcessSpoolerMessage);
      was_calibrating = false;
      printer_shutdown = new ThreadSafeVariable<bool>(false);
      printer_cyclepower = new ThreadSafeVariable<bool>(false);
      SDCardExtension = new SDCardExtensions((IPrinter) this);
      RegisterPlugin(SDCardExtensions.ID, (IPrinterPlugin)SDCardExtension);
    }

    public void ProcessState()
    {
      printerState.Value = GetStateFromPrinter();
      if (printerState.Value == PrinterObject.State.IsCalibrating)
      {
        was_calibrating = true;
      }

      if (!was_calibrating || printerState.Value == PrinterObject.State.IsCalibrating)
      {
        return;
      }

      was_calibrating = false;
      infobox.AddMessageToQueue("Printer calibration has completed.");
    }

    public void OnUpdateData(PrinterInfo info)
    {
      invalid_firmware = false;
      if (Info.Status == PrinterStatus.Bootloader_InvalidFirmware)
      {
        invalid_firmware = true;
      }
      else
      {
        firmware_message_sent = false;
      }

      if (Info.Status == PrinterStatus.Firmware_Calibrating)
      {
        calibration_message_sent = true;
      }

      if (!readyAndFilamentChecked && Info.Status != PrinterStatus.Connected && (Info.Status != PrinterStatus.Connecting && Info.InFirmwareMode))
      {
        settings_manager.AssociateFilamentToPrinter(Info.serial_number, GetCurrentFilament());
        readyAndFilamentChecked = true;
      }
      if (filament_profile == null && Info.filament_info.filament_type != FilamentSpool.TypeEnum.NoFilament)
      {
        filament_profile = FilamentProfile.CreateFilamentProfile(Info.filament_info, MyPrinterProfile);
      }

      if (invalid_firmware && !firmware_message_sent)
      {
        firmware_message_sent = true;
        SendFirmwareMessageUserConfirm();
      }
      if (!invalid_firmware && !Info.InBootloaderMode && Info.Status != PrinterStatus.Bootloader_UpdatingFirmware)
      {
        if (!gantry_clip_message_sent && !Info.persistantData.GantryClipsRemoved)
        {
          gantry_clip_message_sent = true;
          SendGantryClipMessage();
        }
        calibration_invalid = !Info.calibration.Calibration_Valid || !Info.extruder.Z_Valid;
        if (Info.persistantData.GantryClipsRemoved)
        {
          if (calibration_invalid)
          {
            if (!calibration_message_sent)
            {
              calibration_message_sent = true;
              if (!Info.calibration.Calibration_Valid)
              {
                RequestCalibrationFromUser(new M3D.Spooling.Client.AsyncCallback(ReleaseLockAfterCommand), (object) null, PrinterObject.CalibrationType.CalibrateFull_G32);
              }
              else
              {
                RequestCalibrationFromUser(new M3D.Spooling.Client.AsyncCallback(ReleaseLockAfterCommand), (object) null, PrinterObject.CalibrationType.CalibrateQuick_G30);
              }
            }
          }
          else if (info.calibration.G32_VERSION < 1)
          {
            if (!calibration_message_sent && !isBusy)
            {
              calibration_message_sent = true;
              SendCalibrationOutofDateMessage(new M3D.Spooling.Client.AsyncCallback(ReleaseLockAfterCommand), (object) null);
            }
          }
          else if (was_calibrating)
          {
            calibration_message_sent = false;
          }
        }
      }
      CheckPowerOutageState();
      lock (heater_lock)
      {
        if (heater_on)
        {
          if (!heater_time_vested)
          {
            if (!heater_counter.IsRunning)
            {
              heater_counter.Reset();
              heater_counter.Start();
            }
            else if (heater_counter.ElapsedMilliseconds > 30000L)
            {
              heater_time_vested = true;
              heater_counter.Stop();
            }
          }
          else if ((double)Info.extruder.Temperature < -100.0)
          {
            heater_on = false;
            heater_time_vested = false;
            heater_counter.Stop();
            heater_counter.Reset();
          }
        }
        else if ((double)Info.extruder.Temperature > 150.0)
        {
          heater_on = true;
        }
      }
      CheckSupportedFeatures();
    }

    private void CheckSupportedFeatures()
    {
      if (!Info.supportedFeatures.UsesSupportedFeatures || m_bSupportedFeaturesChecked)
      {
        return;
      }

      if (MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
      {
        HasHeatedBed = Info.supportedFeatures.Available("Heated Bed Control", MyPrinterProfile.SupportedFeaturesConstants);
      }

      if (PrinterState != PrinterObject.State.IsReady)
      {
        return;
      }

      m_bSupportedFeaturesChecked = true;
      if (m_tsbFeaturePanelShown.Value)
      {
        return;
      }

      m_tsbFeaturePanelShown.Value = true;
      var flag = true;
      var featuresBitField = Info.supportedFeatures.FeaturesBitField;
      var profileName = MyPrinterProfile.ProfileName;
      if (settings_manager.CurrentMiscSettings.LastProFeaturesFlag.ContainsKey(profileName))
      {
        if ((int)settings_manager.CurrentMiscSettings.LastProFeaturesFlag[profileName] == (int) featuresBitField)
        {
          flag = false;
        }
        else
        {
          settings_manager.CurrentMiscSettings.LastProFeaturesFlag[profileName] = featuresBitField;
          settings_manager.SaveSettings();
        }
      }
      else
      {
        settings_manager.CurrentMiscSettings.LastProFeaturesFlag.Add(profileName, featuresBitField);
        settings_manager.SaveSettings();
      }
      if (!flag)
      {
        return;
      }

      FeaturesDialog.Show(messagebox, spooler_connection, this);
    }

    private void ReleaseLockAfterCommand(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      var num = (int)ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private PrinterObject.State GetStateFromPrinter()
    {
      if (isConnected())
      {
        if (Info.Status == PrinterStatus.Firmware_Calibrating)
        {
          return PrinterObject.State.IsCalibrating;
        }

        if (Info.Status == PrinterStatus.Firmware_Executing || Info.Status == PrinterStatus.Firmware_Homing || (Info.Status == PrinterStatus.Firmware_WarmingUp || Info.Status == PrinterStatus.Connecting))
        {
          return PrinterObject.State.IsBusy;
        }

        if (Info.Status == PrinterStatus.Firmware_Ready || Info.Status == PrinterStatus.Firmware_Idle)
        {
          return PrinterObject.State.IsReady;
        }

        if (Info.Status == PrinterStatus.Firmware_Printing)
        {
          return PrinterObject.State.IsPrinting;
        }

        if (Info.Status == PrinterStatus.Firmware_PrintingPaused || Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing)
        {
          return PrinterObject.State.IsPaused;
        }

        if (Info.Status == PrinterStatus.Bootloader_UpdatingFirmware)
        {
          return PrinterObject.State.IsUpdatingFirmware;
        }
      }
      return PrinterObject.State.IsNotHealthy;
    }

    public void OnProcessSpoolerMessage(SpoolerMessage message)
    {
      switch (message.Type)
      {
        case MessageType.PrinterShutdown:
          printer_shutdown.Value = true;
          break;
        case MessageType.BedLocationMustBeCalibrated:
        case MessageType.BedOrientationMustBeCalibrated:
          ReshowErrors();
          break;
        case MessageType.PrinterTimeout:
          lock (heater_lock)
          {
            heater_on = false;
            break;
          }
        case MessageType.CheckGantryClips:
          SendGantryClipMessage();
          break;
        case MessageType.FirmwareMustBeUpdated:
          firmware_message_sent = true;
          SendFirmwareMessageUserConfirm();
          break;
        case MessageType.FirmwareErrorCyclePower:
          printer_cyclepower.Value = true;
          break;
      }
    }

    public SpoolerResult DoFirmwareUpdate()
    {
      update_firmware_timer.Stop();
      update_firmware_timer.Reset();
      printerState.Value = PrinterObject.State.IsUpdatingFirmware;
      SendFirmwareUpdate();
      return AcquireLock(new M3D.Spooling.Client.AsyncCallback(UpdateFirmwareAfterLock), (object) this);
    }

    public void TurnOnHeater(M3D.Spooling.Client.AsyncCallback callback, object state, int temperature)
    {
      var num = (int)SendManualGCode(callback, state, PrinterCompatibleString.Format("M104 S{0}", (object) temperature));
      lock (heater_lock)
      {
        heater_on = true;
      }
    }

    public void TurnOffHeater(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      lock (heater_lock)
      {
        heater_on = false;
      }

      var num = (int)SendManualGCode(callback, state, "M104 S0");
    }

    public void ReshowErrors()
    {
      calibration_message_sent = false;
      gantry_clip_message_sent = false;
    }

    public SpoolerResult PrintTestBorder(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      var str1 = Path.Combine(Paths.WorkingFolder, "m3doutput.gcode");
      if (MyFilamentProfile == null || MyFilamentProfile.Type == FilamentSpool.TypeEnum.NoFilament)
      {
        return SpoolerResult.Error;
      }

      var compensationPreprocessor = new BedCompensationPreprocessor();
      compensationPreprocessor.UpdateConfigurations(Info.calibration, MyPrinterProfile.PrinterSizeConstants, true);
      StreamWriter text = File.CreateText(str1);
      Vector frontLeft = compensationPreprocessor.FrontLeft;
      Vector frontRight = compensationPreprocessor.FrontRight;
      Vector backLeft = compensationPreprocessor.BackLeft;
      Vector backRight = compensationPreprocessor.BackRight;
      foreach (var str2 in GCodeGeneration.CreatePrintTestBorder(frontLeft.x, frontLeft.y, frontRight.x, backRight.y, 1f, (float)MyFilamentProfile.Temperature, 1.75f))
      {
        text.WriteLine(str2);
      }

      text.Close();
      return PrintModel(callback, state, new JobParams(str1, nameof (PrintTestBorder), (string) null, MyFilamentProfile.Type, 0.0f, 0.0f) { preprocessor = MyFilamentProfile.preprocessor, filament_temperature = MyFilamentProfile.Temperature, options = { bounds_check_xy = false, autostart_ignorewarnings = true } });
    }

    public void ShowLockError(IAsyncCallResult ar)
    {
      var lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
      {
        return;
      }

      messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
    }

    public static string GetLockErrorMessage(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success_LockReleased || (ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived))
      {
        return (string) null;
      }

      string str;
      switch (ar.CallResult)
      {
        case CommandResult.Failed_PrinterDoesNotHaveLock:
          str = Locale.GlobalLocale.T("T_Failed_PrinterDoesNotHaveLock");
          break;
        case CommandResult.Failed_PrinterAlreadyLocked:
          str = Locale.GlobalLocale.T("T_Failed_PrinterAlreadyLocked");
          break;
        case CommandResult.Failed_PreviousCommandNotCompleted:
          str = Locale.GlobalLocale.T("T_Failed_PreviousCommandNotCompleted");
          break;
        case CommandResult.Failed_NotInFirmware:
          str = Locale.GlobalLocale.T("T_Failed_NotInFirmware");
          break;
        case CommandResult.Failed_PrinterNotPaused:
          str = Locale.GlobalLocale.T("T_Failed_PrinterNotPaused");
          break;
        case CommandResult.Failed_PrinterNotPrinting:
          str = Locale.GlobalLocale.T("T_Failed_PrinterNotPrinting");
          break;
        case CommandResult.Failed_CannotPauseSavingToSD:
          str = Locale.GlobalLocale.T("T_Failed_CannotPauseWhileSavingToPrinter");
          break;
        case CommandResult.Failed_ThePrinterIsPrintingOrPaused:
          str = Locale.GlobalLocale.T("T_Failed_ThePrinterIsPrintingOrPaused");
          break;
        case CommandResult.CommandInterruptedByM0:
          str = Locale.GlobalLocale.T("T_CommandInterruptedByM0");
          break;
        default:
          str = Locale.GlobalLocale.T("T_Failed_Default");
          break;
      }
      return str;
    }

    public bool CheckForLockError(IAsyncCallResult ar)
    {
      var lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
      {
        return true;
      }

      messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
      return false;
    }

    private void onAutoCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject.AutoReleaseState;
      if (asyncState == null)
      {
        throw new Exception("type doesn't match AutoReleaseState");
      }

      if (ar.CallResult != CommandResult.Success)
      {
        if (asyncState.callback == null)
        {
          return;
        }

        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
      {
        if (!asyncState.autoRelease)
        {
          return;
        }

        var num = (int) asyncState.printer.ReleaseLock(asyncState.callback, asyncState.state);
      }
    }

    private void onAutoLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject.AutoReleaseState;
      if (asyncState == null)
      {
        throw new Exception("type doesn't match AutoReleaseState");
      }

      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        if (asyncState.callback == null)
        {
          return;
        }

        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
      {
        SendGCodeAutoHome(asyncState);
      }
    }

    private void SendGCodeAutoHome(PrinterObject.AutoReleaseState state)
    {
      var stringList = new List<string>();
      if (state.homeFirst && Info.extruder.ishomed != Trilean.True)
      {
        if ((double)Info.extruder.position.pos.z < 2.0)
        {
          stringList.Add("G91");
          stringList.Add("G0 Z2");
        }
        stringList.Add("G28");
        stringList.Add("M114");
      }
      stringList.AddRange((IEnumerable<string>) state.gcode);
      var num = (int) state.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(onAutoCommand), (object) state, stringList.ToArray());
    }

    public void SendCommandAutoLockReleaseHome(M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      SendCommandAutoLock(false, true, callback, state, gcode);
    }

    public void SendCommandAutoLockRelease(M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      SendCommandAutoLock(false, false, callback, state, gcode);
    }

    public void SendCommandAutoLock(bool keeplock, bool homeIfNeeded, M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      var state1 = new PrinterObject.AutoReleaseState(callback, state, (IPrinter) this, gcode, homeIfNeeded, !keeplock);
      if (HasLock)
      {
        SendGCodeAutoHome(state1);
      }
      else
      {
        var num = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(onAutoLock), (object) new PrinterObject.AutoReleaseState(callback, state, (IPrinter) this, gcode, true, true));
      }
    }

    public bool MarkedAsBusy
    {
      get
      {
        return marked_as_busy.Value;
      }
      set
      {
        marked_as_busy.Value = value;
      }
    }

    public bool HeaterOn
    {
      get
      {
        lock (heater_lock)
        {
          return heater_on;
        }
      }
    }

    public bool isBusy
    {
      get
      {
        switch (PrinterState)
        {
          case PrinterObject.State.IsCalibrating:
          case PrinterObject.State.IsUpdatingFirmware:
          case PrinterObject.State.IsBusy:
          case PrinterObject.State.IsPrinting:
            return true;
          default:
            if (Info.Status != PrinterStatus.Connecting && !HasLock)
            {
              return WaitingForCommandToComplete;
            }

            goto case PrinterObject.State.IsCalibrating;
        }
      }
    }

    public bool isHealthy
    {
      get
      {
        switch (PrinterState)
        {
          case PrinterObject.State.IsUpdatingFirmware:
          case PrinterObject.State.IsNotHealthy:
            return false;
          default:
            if (!calibration_invalid)
            {
              return !invalid_firmware;
            }

            goto case PrinterObject.State.IsUpdatingFirmware;
        }
      }
    }

    public PrinterObject.State PrinterState
    {
      get
      {
        return printerState.Value;
      }
    }

    public bool IsPausedorPausing
    {
      get
      {
        if (Info.Status != PrinterStatus.Firmware_IsWaitingToPause && Info.Status != PrinterStatus.Firmware_PrintingPaused)
        {
          return Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing;
        }

        return true;
      }
    }

    public bool HasHeatedBed { get; private set; }

    public static string GetFamilyFromProfile(string sProfileName)
    {
      var str = "";
      if (sProfileName.StartsWith("Micro"))
      {
        str = "Micro";
      }
      else if (sProfileName.StartsWith("Pro"))
      {
        str = "Pro";
      }

      return str;
    }

    public static bool IsProfileMemberOfFamily(string sProfileName, string sFamilyName)
    {
      return sProfileName.StartsWith("Micro") && sFamilyName.Equals("Micro") || sProfileName.StartsWith("Pro") && sFamilyName.Equals("Pro");
    }

    public void CalibrateBed(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode)
    {
      if (!true)
      {
        messagebox.AddMessageToQueue("Your printer will begin calibrating. Are you using a clear flexible plastic print bed or a hard one?", Locale.GlobalLocale.T("T_Flexible"), Locale.GlobalLocale.T("T_Hard"), Locale.GlobalLocale.T("T_Cancel"), (PopupMessageBox.OnUserSelectionDel) ((result, type, sn, user_data) =>
        {
          var calibrationData = (PrinterObject.CalibrationData) user_data;
          if (!Connected)
          {
            return;
          }

          if (result == PopupMessageBox.PopupResult.Button1_YesOK)
          {
            CalibrationWorker(callback, state, calibrationData.mode, 0.4f);
          }
          else
          {
            if (result != PopupMessageBox.PopupResult.Button2_NoCancel)
            {
              return;
            }

            CalibrationWorker(callback, state, calibrationData.mode, 0.0f);
          }
        }), (object) new PrinterObject.CalibrationData(mode));
      }
      else
      {
        CalibrationWorker(callback, state, mode, 0.4f);
      }
    }

    public void RequestCalibrationFromUser(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode)
    {
      messagebox.AddMessageToQueue(new SpoolerMessage(mode != PrinterObject.CalibrationType.CalibrateFull_G32 ? MessageType.BedLocationMustBeCalibrated : MessageType.BedOrientationMustBeCalibrated, Info.serial_number, (string) null), PopupMessageBox.MessageBoxButtons.YESNO, (PopupMessageBox.OnUserSelectionDel) ((result, type, sn, user_data) =>
      {
        if (result != PopupMessageBox.PopupResult.Button1_YesOK || !(sn == Info.serial_number) || !Connected)
        {
          return;
        }

        var num = (int)AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
        {
          var asyncState = ar.AsyncState as PrinterObject.CalibrationDataCallback;
          CalibrateBed(asyncState.callback, asyncState.state, asyncState.mode);
        }), (object) new PrinterObject.CalibrationDataCallback(callback, state, mode));
      }), (object) new PrinterObject.CalibrationData(mode));
    }

    private void CalibrationWorker(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode, float z_offset)
    {
      var stringList = new List<string>();
      stringList.Add("G91");
      stringList.Add("M104 S150");
      if (PrinterObject.IsProfileMemberOfFamily(MyPrinterProfile.ProfileName, "Pro"))
      {
        stringList.Add("G0 Z8 F300");
      }
      else
      {
        stringList.Add("G0 Y20 Z2 F3000");
      }

      if (mode == PrinterObject.CalibrationType.CalibrateQuick_G30 && PrinterObject.IsProfileMemberOfFamily(MyPrinterProfile.ProfileName, "Pro"))
      {
        stringList.Add("G28");
      }

      stringList.Add("M109 S150");
      stringList.Add("M106 S0");
      if (Info.calibration.UsesCalibrationOffset)
      {
        z_offset += Info.calibration.CALIBRATION_OFFSET;
      }

      if (mode == PrinterObject.CalibrationType.CalibrateFull_G32)
      {
        stringList.Add("G32 Z" + (object) z_offset);
      }
      else
      {
        stringList.Add("G30 Z" + (object) z_offset);
      }

      stringList.Add("M104 S0");
      var num = (int)SendManualGCode(callback, state, stringList.ToArray());
      printerState.Value = PrinterObject.State.IsCalibrating;
      infobox.AddMessageToQueue(Info.serial_number.ToString() + " is calibrating.");
    }

    public void SendFirmwareMessageUserConfirm()
    {
      var message = new SpoolerMessage(MessageType.UserDefined, Info.serial_number, "");
      var imageWidget = CreateImageWidget(Info.serial_number.ToString());
      var xmlsource = "<?xml version=\"1.0\" encoding=\"utf-16\"?><XMLFrame id=\"1000\" width=\"550\" height=\"300\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1001\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"640\" texture-v0=\"320\" texture-u1=\"704\" texture-v1=\"383\" center-vertically=\"1\" center-horizontally=\"1\" leftbordersize-pixels=\"41\" rightbordersize-pixels=\"8\" minimumwidth=\"64\" topbordersize-pixels=\"35\" bottombordersize-pixels=\"8\" minimumheight=\"64\" />    <TextWidget id=\"1002\" x=\"50\" y=\"2\" width=\"298\" height=\"35\" font-size=\"Large\" font-color=\"#FF808080\" alignment=\"Left\">Firmware Update</TextWidget>    <XMLFrame id=\"1004\" x=\"190\" y=\"20\" width=\"330\" height=\"260\">        <TextWidget id=\"1003\" x=\"0\" y=\"32\" relative-width=\"1.0\" height=\"110\" font-size=\"Medium\" font-color=\"#FF404040\" alignment=\"Centre\">" + Locale.GlobalLocale.T("T_FIRMWARE_UPDATE_REQUIRED") + "</TextWidget>        <ButtonWidget id=\"2000\" x=\"20\" y=\"132\" width=\"200\" height=\"48\" font-size=\"Large\" alignment=\"Centre\" has_focus=\"1\" center-horizontally=\"1\">Update Now</ButtonWidget>    </XMLFrame>" + imageWidget + "</XMLFrame>";
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(message, xmlsource, new PopupMessageBox.XMLButtonCallback(OnFirmwareUpdateButton), (object) null, new ElementStandardDelegate(OnFirmwareOnUpdateCallback)));
    }

    private void SendFirmwareUpdate()
    {
      var message = new SpoolerMessage(MessageType.UserDefined, Info.serial_number, "");
      ok_sent = false;
      update_firmware_timer.Start();
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(message, GenerateFirmwareUpdateXML(Info.serial_number.ToString()), (PopupMessageBox.XMLButtonCallback) null, (object) null, new ElementStandardDelegate(OnFirmwareOnUpdateCallback)));
    }

    private void OnFirmwareUpdateButton(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID != 2000)
      {
        return;
      }

      if (childFrame.Host != null)
      {
        childFrame.Host.SetFocus((Element2D) null);
      }

      childFrame.Init(childFrame.Host, GenerateFirmwareUpdateXML(Info.serial_number.ToString()), (ButtonCallback) null);
      childFrame.DoOnUpdate = new ElementStandardDelegate(OnFirmwareOnUpdateCallback);
      var num = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(UpdateFirmwareAfterLock), (object) this);
      ok_sent = false;
      update_firmware_timer.Start();
    }

    private void UpdateFirmwareAfterLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        VerifyPrinterResults(ar);
      }
      else
      {
        var num = (int)DoFirmwareUpdate(new M3D.Spooling.Client.AsyncCallback(VerifyPrinterResults), (object) this);
      }
    }

    private void VerifyPrinterResults(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        messagebox.CloseCurrent();
        infobox.AddMessageToQueue(Info.serial_number.ToString() + " failed because it is already in use.");
      }
      else
      {
        if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success_LockReleased || ar.CallResult == CommandResult.Success)
        {
          return;
        }

        messagebox.CloseCurrent();
        infobox.AddMessageToQueue(Info.serial_number.ToString() + " failed. Please try again.");
      }
    }

    private void OnFirmwareOnUpdateCallback()
    {
      if (update_firmware_timer.IsRunning && update_firmware_timer.ElapsedMilliseconds > 5000L)
      {
        if (PrinterState == PrinterObject.State.IsReady && !ok_sent)
        {
          ok_sent = true;
          update_firmware_timer.Stop();
          messagebox.CloseCurrent();
          infobox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(MessageType.FirmwareUpdateComplete));
          spooler_connection.SelectPrinterBySerialNumber(Info.serial_number.ToString());
        }
        if ((update_firmware_timer.ElapsedMilliseconds > 300000L || Info.Status == PrinterStatus.Bootloader_FirmwareUpdateFailed) && !ok_sent)
        {
          ok_sent = true;
          update_firmware_timer.Stop();
          messagebox.CloseCurrent();
          messagebox.AddMessageToQueue("Failed to update printer " + Info.serial_number.ToString() + ". Please reset the printer by disconnecting it and then reconnecting it to power.");
        }
      }
      if (!printer_shutdown.Value && !printer_cyclepower.Value || ok_sent)
      {
        return;
      }

      ok_sent = true;
      messagebox.CloseCurrent();
      infobox.AddMessageToQueue(Info.serial_number.ToString() + " was disconnected while updating firmware");
      if (update_firmware_timer.IsRunning && printer_shutdown.Value)
      {
        messagebox.AddMessageToQueue("Critical Error: The printer was disconnected while updating. This may have cause problems with the USB connection. Please disconnect the printer from the computer and from power and then reset the computer. Then reconnect the printer.");
      }

      printer_shutdown.Value = false;
      printer_cyclepower.Value = false;
      update_firmware_timer.Stop();
    }

    private string GenerateFirmwareUpdateXML(string sn)
    {
      return "<?xml version=\"1.0\" encoding=\"utf-16\"?><XMLFrame id=\"1000\" width=\"550\" height=\"300\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1001\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"640\" texture-v0=\"320\" texture-u1=\"704\" texture-v1=\"383\" center-vertically=\"1\" center-horizontally=\"1\" leftbordersize-pixels=\"41\" rightbordersize-pixels=\"8\" minimumwidth=\"64\" topbordersize-pixels=\"35\" bottombordersize-pixels=\"8\" minimumheight=\"64\" />    <TextWidget id=\"1002\" x=\"50\" y=\"2\" width=\"298\" height=\"35\" font-size=\"Large\" font-color=\"#FF808080\" alignment=\"Left\">Firmware Update</TextWidget>    <XMLFrame id=\"1004\" x=\"190\" y=\"20\" width=\"330\" height=\"260\">        <TextWidget id=\"1003\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"110\" font-size=\"Medium\" font-color=\"#FF404040\" alignment=\"Centre\">Please wait. Updates are being sent to the printer. Please do not disconnect or unplug the printer.</TextWidget>        <SpriteAnimationWidget id=\"1001\" x=\"0\" y=\"120\" width=\"128\" height=\"108\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"768\" texture-u1=\"767\" texture-v1=\"1023\" center-horizontally=\"1\" columns=\"6\" rows=\"2\" frames=\"12\" frame-time=\"200\" />    </XMLFrame>" + CreateImageWidget(Info.serial_number.ToString()) + "</XMLFrame>";
    }

    private string CreateImageWidget(string sn)
    {
      ImageResourceMapping.PixelCoordinate pixelCoordinate = ImageResourceMapping.PrinterColorPosition(sn);
      return "<ImageWidget id=\"1001\" text-area-height=\"20\" image-area-width=\"130\" x=\"30\" y=\"80\" width=\"150\" height=\"150\" src=\"extendedcontrols\" " + string.Format("texture-u0=\"{0}\" texture-v0=\"{1}\" texture-u1=\"{2}\" texture-v1=\"{3}\" vertical-alignment=\"Bottom\" font-size=\"Small\">", (object) pixelCoordinate.u0, (object) pixelCoordinate.v0, (object) pixelCoordinate.u1, (object) pixelCoordinate.v1) + Info.serial_number.ToString() + "</ImageWidget>";
    }

    public void SendGantryClipMessage()
    {
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(MessageType.CheckGantryClips, Info.serial_number, ""), "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"397\" height=\"397\" src=\"extendedcontrols\" texture-u0=\"450\" texture-v0=\"194\" texture-u1=\"847\" texture-v1=\"591\" center-vertically=\"1\" />    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\" />    <TextWidget id=\"1003\" x=\"418\" y=\"20\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">A new 3D printer has been connected.</TextWidget>    <XMLFrame id=\"1004\" x=\"418\" y=\"84\" width=\"272\" height=\"336\" center-vertically=\"0\">        <TextWidget id=\"1005\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"150\" font-size=\"Medium\" font-color=\"#FF646464\" alignment=\"Centre\" center-horizontally=\"1\">Before continuing, the printer needs to verify that it's gantry clips have been removed. Please make sure the gantry clips have been removed and that the print area is clear.        </TextWidget>        <ButtonWidget id=\"1006\" x=\"20\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\" has_focus=\"1\">Verify</ButtonWidget>        <ButtonWidget id=\"1007\" x=\"-120\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Cancel</ButtonWidget>    </XMLFrame></XMLFrame>", new PopupMessageBox.XMLButtonCallback(OnGantryClipButton), (object) null, new ElementStandardDelegate(CloseIfNotConnected)));
    }

    private void OnGantryClipButton(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID == 1007)
      {
        parentFrame.AddMessageToQueue("You will not be able to use your printer until your gantry clips have been removed.", PopupMessageBox.MessageBoxButtons.OK);
        parentFrame.Visible = false;
      }
      else
      {
        if (button.ID != 1006)
        {
          return;
        }

        var xmlScript = "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"397\" height=\"397\" src=\"extendedcontrols\" texture-u0=\"450\" texture-v0=\"194\" texture-u1=\"847\" texture-v1=\"591\" center-vertically=\"1\"></ImageWidget>    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\"></ImageWidget>    <TextWidget id=\"1003\" x=\"418\" y=\"20\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">Please wait. Your printer is being tested.</TextWidget>    <XMLFrame id=\"1004\" x=\"418\" y=\"42\" width=\"272\" height=\"336\" center-vertically=\"0\">        <SpriteAnimationWidget id=\"1001\" x=\"0\" y=\"0\" width=\"128\" height=\"108\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"768\" texture-u1=\"767\" texture-v1=\"1023\" center-vertically=\"1\" center-horizontally=\"1\" columns=\"6\" rows=\"2\" frames=\"12\" frame-time=\"200\" />    </XMLFrame></XMLFrame>";
        if (childFrame.Host != null)
        {
          childFrame.Host.SetFocus((Element2D) null);
        }

        childFrame.Init(childFrame.Host, xmlScript, (ButtonCallback) null);
        var num = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step1_Lock);
      }
    }

    private void GantryClipCallBack(IAsyncCallResult ar)
    {
      var asyncState = (PrinterObject.GantryClipStep) ar.AsyncState;
      if (asyncState == PrinterObject.GantryClipStep.Step1_Lock && ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num1 = (int)SendManualGCode(new M3D.Spooling.Client.AsyncCallback(GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step2_SendM583, "M583");
      }
      else if (asyncState == PrinterObject.GantryClipStep.Step2_SendM583 && ar.CallResult == CommandResult.Success)
      {
        var num2 = (int)ReleaseLock(new M3D.Spooling.Client.AsyncCallback(GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step3_CheckStateAndRelease);
      }
      else if (asyncState == PrinterObject.GantryClipStep.Step3_CheckStateAndRelease && ar.CallResult == CommandResult.Success_LockReleased)
      {
        messagebox.CloseCurrent();
        if (Info.persistantData.GantryClipsRemoved)
        {
          messagebox.AddMessageToQueue("We have verified that your gantry clips have been removed.", PopupMessageBox.MessageBoxButtons.OK);
        }
        else
        {
          messagebox.AddMessageToQueue("Sorry, but your gantry clips have not been removed.", PopupMessageBox.MessageBoxButtons.OK);
          SendGantryClipMessage();
        }
      }
      else
      {
        messagebox.CloseCurrent();
        if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
        {
          messagebox.AddMessageToQueue("Unable to verify because the printer is being used by another application.", PopupMessageBox.MessageBoxButtons.OK);
        }
        else
        {
          messagebox.AddMessageToQueue("Unable to communicate with the printer. Please try again.", PopupMessageBox.MessageBoxButtons.OK);
          SendGantryClipMessage();
        }
      }
    }

    private void CloseIfNotConnected()
    {
      if (Connected)
      {
        return;
      }

      messagebox.CloseCurrent();
    }

    public void SendCalibrationOutofDateMessage(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(MessageType.CheckGantryClips, Info.serial_number, ""), "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"367\" height=\"282\" src=\"extendedcontrols\" texture-u0=\"448\" texture-v0=\"592\" texture-u1=\"814\" texture-v1=\"873\" center-vertically=\"1\" />    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\" />    <TextWidget id=\"1003\" x=\"400\" y=\"60\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">Calibration Update Available</TextWidget>    <XMLFrame id=\"1004\" x=\"400\" y=\"124\" width=\"272\" height=\"336\" center-vertically=\"0\">        <TextWidget id=\"1005\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"150\" font-size=\"Medium\" font-color=\"#FF646464\" alignment=\"Centre\" center-horizontally=\"1\">Print calibration has been updated. Before continuing, it is highly recommended that the printer be recalibrated.\n\nThis may take up to 30 minutes.        </TextWidget>        <ButtonWidget id=\"1006\" x=\"20\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Calibrate</ButtonWidget>        <ButtonWidget id=\"1007\" x=\"-120\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Ignore</ButtonWidget>    </XMLFrame></XMLFrame>", new PopupMessageBox.XMLButtonCallback(OnCalibrationOutOfDateButton), (object) new PrinterObject.CalibrationDataCallback(callback, state, PrinterObject.CalibrationType.CalibrateFull_G32)));
    }

    private void OnCalibrationOutOfDateButton(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID == 1007)
      {
        parentFrame.CloseCurrent();
      }
      else
      {
        if (button.ID != 1006)
        {
          return;
        }

        parentFrame.CloseCurrent();
        var calibrationDataCallback = data as PrinterObject.CalibrationDataCallback;
        if (calibrationDataCallback == null)
        {
          return;
        }

        CalibrateBed(calibrationDataCallback.callback, calibrationDataCallback.state, calibrationDataCallback.mode);
      }
    }

    public override SpoolerResult SetFilamentInfo(M3D.Spooling.Client.AsyncCallback callback, object state, FilamentSpool info)
    {
      if (info == (FilamentSpool) null)
      {
        return SetFilamentToNone(callback, state);
      }

      filament_profile = FilamentProfile.CreateFilamentProfile(info, MyPrinterProfile);
      return base.SetFilamentInfo(callback, state, info);
    }

    public override SpoolerResult SetFilamentToNone(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      filament_profile = (FilamentProfile) null;
      return base.SetFilamentToNone(callback, state);
    }

    public FilamentProfile MyFilamentProfile
    {
      get
      {
        return filament_profile;
      }
    }

    private void CheckPowerOutageState()
    {
      if (m_bPowerOutageRecoveryDetected && !Info.powerRecovery.bPowerOutageWhilePrinting)
      {
        m_bPowerOutageRecoveryDetected = false;
      }
      else
      {
        if (!Info.powerRecovery.bPowerOutageWhilePrinting || m_bPowerOutageRecoveryDetected)
        {
          return;
        }

        m_bPowerOutageRecoveryDetected = true;
        messagebox.AddMessageToQueue("We detected that a power failure interrupted your print. Would you like to resume printing?", "Power Outage Recovery", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(OnPowerOutageMessageBox));
      }
    }

    private void OnPowerOutageMessageBox(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        messagebox.AddMessageToQueue(new PopupMessageBox.MessageDataStandard(new SpoolerMessage(MessageType.UserDefined, "Was the extruder head moved manually after power failure? If so, we must re-home the extruder and accuracy of the continued print may be somewhat degraded."), PopupMessageBox.MessageBoxButtons.CUSTOM, new PopupMessageBox.OnUserSelectionDel(OnPowerOutageConfirmG28MessageBox), (object) null)
        {
          custom_button1_text = "Yes\nHead was moved",
          custom_button2_text = "No\nHead was not moved",
          custom_button3_text = "Cancel",
          title = "Power Outage Recovery"
        });
      }
      else
      {
        var num = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.ClearFault);
      }
    }

    private void OnPowerOutageConfirmG28MessageBox(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        var num1 = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.RecoverG28);
      }
      else
      {
        if (result != PopupMessageBox.PopupResult.Button2_NoCancel)
        {
          return;
        }

        var num2 = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Recover);
      }
    }

    private void PowerRecoveryAfterLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        CheckForLockError(ar);
      }
      else
      {
        var asyncState = (PrinterObject.PowerOutageAction) ar.AsyncState;
        switch (asyncState)
        {
          case PrinterObject.PowerOutageAction.ClearFault:
            var num1 = (int)ClearPowerRecoveryFault(new M3D.Spooling.Client.AsyncCallback(PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Release);
            break;
          case PrinterObject.PowerOutageAction.Recover:
          case PrinterObject.PowerOutageAction.RecoverG28:
            var num2 = (int)RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Release, PrinterObject.PowerOutageAction.RecoverG28 == asyncState);
            break;
          default:
            var num3 = (int)ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
            break;
        }
      }
    }

    public SDCardExtensions SDCardExtension { get; private set; }

    public void AutoLockAndPrint(JobParams UserJob)
    {
      var num = (int)AcquireLock(new M3D.Spooling.Client.AsyncCallback(OnLockForPrintGCode), (object) UserJob);
    }

    private void OnLockForPrintGCode(IAsyncCallResult ar)
    {
      var asyncState = (JobParams) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        var num = (int)PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, asyncState);
      }
      else
      {
        ShowLockError(ar);
      }
    }

    public enum State
    {
      IsReady,
      IsCalibrating,
      IsUpdatingFirmware,
      IsBusy,
      IsPrinting,
      IsPaused,
      IsNotHealthy,
    }

    public enum CalibrationType
    {
      CalibrateFull_G32,
      CalibrateQuick_G30,
    }

    private class AutoReleaseState
    {
      public M3D.Spooling.Client.AsyncCallback callback;
      public object state;
      public IPrinter printer;
      public string[] gcode;
      public bool homeFirst;
      public bool autoRelease;

      public AutoReleaseState(M3D.Spooling.Client.AsyncCallback callback, object state, IPrinter printer, string[] gcode, bool homeFirst, bool autoRelease)
      {
        this.callback = callback;
        this.state = state;
        this.printer = printer;
        this.gcode = gcode;
        this.homeFirst = homeFirst;
        this.autoRelease = autoRelease;
      }
    }

    private struct CalibrationData
    {
      public PrinterObject.CalibrationType mode;

      public CalibrationData(PrinterObject.CalibrationType mode)
      {
        this.mode = mode;
      }
    }

    private class CalibrationDataCallback
    {
      public M3D.Spooling.Client.AsyncCallback callback;
      public object state;
      public PrinterObject.CalibrationType mode;

      public CalibrationDataCallback(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode)
      {
        this.callback = callback;
        this.state = state;
        this.mode = mode;
      }
    }

    private enum GantryClipStep
    {
      Step1_Lock,
      Step2_SendM583,
      Step3_CheckStateAndRelease,
    }

    private enum PowerOutageAction
    {
      ClearFault,
      Recover,
      RecoverG28,
      Release,
    }
  }
}
