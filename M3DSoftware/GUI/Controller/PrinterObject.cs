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
      this.HasHeatedBed = false;
      this.invalid_firmware = false;
      this.calibration_invalid = false;
      this.heater_time_vested = false;
      this.heater_on = false;
      this.firmware_message_sent = false;
      this.gantry_clip_message_sent = false;
      this.calibration_message_sent = false;
      this.m_bSupportedFeaturesChecked = false;
      this.heater_lock = new object();
      this.printerState = new ThreadSafeVariable<PrinterObject.State>();
      this.printerState.Value = this.GetStateFromPrinter();
      this.marked_as_busy = new ThreadSafeVariable<bool>();
      this.marked_as_busy.Value = false;
      this.heater_counter = new Stopwatch();
      this.update_firmware_timer = new Stopwatch();
      base_printer.OnUpdateData += new OnUpdateDataDel(this.OnUpdateData);
      base_printer.OnProcessSpoolerMessage += new OnProcessSpoolerMessageDel(this.OnProcessSpoolerMessage);
      this.was_calibrating = false;
      this.printer_shutdown = new ThreadSafeVariable<bool>(false);
      this.printer_cyclepower = new ThreadSafeVariable<bool>(false);
      this.SDCardExtension = new SDCardExtensions((IPrinter) this);
      this.RegisterPlugin(SDCardExtensions.ID, (IPrinterPlugin) this.SDCardExtension);
    }

    public void ProcessState()
    {
      this.printerState.Value = this.GetStateFromPrinter();
      if (this.printerState.Value == PrinterObject.State.IsCalibrating)
        this.was_calibrating = true;
      if (!this.was_calibrating || this.printerState.Value == PrinterObject.State.IsCalibrating)
        return;
      this.was_calibrating = false;
      this.infobox.AddMessageToQueue("Printer calibration has completed.");
    }

    public void OnUpdateData(PrinterInfo info)
    {
      this.invalid_firmware = false;
      if (this.Info.Status == PrinterStatus.Bootloader_InvalidFirmware)
        this.invalid_firmware = true;
      else
        this.firmware_message_sent = false;
      if (this.Info.Status == PrinterStatus.Firmware_Calibrating)
        this.calibration_message_sent = true;
      if (!this.readyAndFilamentChecked && this.Info.Status != PrinterStatus.Connected && (this.Info.Status != PrinterStatus.Connecting && this.Info.InFirmwareMode))
      {
        this.settings_manager.AssociateFilamentToPrinter(this.Info.serial_number, this.GetCurrentFilament());
        this.readyAndFilamentChecked = true;
      }
      if (this.filament_profile == null && this.Info.filament_info.filament_type != FilamentSpool.TypeEnum.NoFilament)
        this.filament_profile = FilamentProfile.CreateFilamentProfile(this.Info.filament_info, this.MyPrinterProfile);
      if (this.invalid_firmware && !this.firmware_message_sent)
      {
        this.firmware_message_sent = true;
        this.SendFirmwareMessageUserConfirm();
      }
      if (!this.invalid_firmware && !this.Info.InBootloaderMode && this.Info.Status != PrinterStatus.Bootloader_UpdatingFirmware)
      {
        if (!this.gantry_clip_message_sent && !this.Info.persistantData.GantryClipsRemoved)
        {
          this.gantry_clip_message_sent = true;
          this.SendGantryClipMessage();
        }
        this.calibration_invalid = !this.Info.calibration.Calibration_Valid || !this.Info.extruder.Z_Valid;
        if (this.Info.persistantData.GantryClipsRemoved)
        {
          if (this.calibration_invalid)
          {
            if (!this.calibration_message_sent)
            {
              this.calibration_message_sent = true;
              if (!this.Info.calibration.Calibration_Valid)
                this.RequestCalibrationFromUser(new M3D.Spooling.Client.AsyncCallback(this.ReleaseLockAfterCommand), (object) null, PrinterObject.CalibrationType.CalibrateFull_G32);
              else
                this.RequestCalibrationFromUser(new M3D.Spooling.Client.AsyncCallback(this.ReleaseLockAfterCommand), (object) null, PrinterObject.CalibrationType.CalibrateQuick_G30);
            }
          }
          else if (info.calibration.G32_VERSION < 1)
          {
            if (!this.calibration_message_sent && !this.isBusy)
            {
              this.calibration_message_sent = true;
              this.SendCalibrationOutofDateMessage(new M3D.Spooling.Client.AsyncCallback(this.ReleaseLockAfterCommand), (object) null);
            }
          }
          else if (this.was_calibrating)
            this.calibration_message_sent = false;
        }
      }
      this.CheckPowerOutageState();
      lock (this.heater_lock)
      {
        if (this.heater_on)
        {
          if (!this.heater_time_vested)
          {
            if (!this.heater_counter.IsRunning)
            {
              this.heater_counter.Reset();
              this.heater_counter.Start();
            }
            else if (this.heater_counter.ElapsedMilliseconds > 30000L)
            {
              this.heater_time_vested = true;
              this.heater_counter.Stop();
            }
          }
          else if ((double) this.Info.extruder.Temperature < -100.0)
          {
            this.heater_on = false;
            this.heater_time_vested = false;
            this.heater_counter.Stop();
            this.heater_counter.Reset();
          }
        }
        else if ((double) this.Info.extruder.Temperature > 150.0)
          this.heater_on = true;
      }
      this.CheckSupportedFeatures();
    }

    private void CheckSupportedFeatures()
    {
      if (!this.Info.supportedFeatures.UsesSupportedFeatures || this.m_bSupportedFeaturesChecked)
        return;
      if (this.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
        this.HasHeatedBed = this.Info.supportedFeatures.Available("Heated Bed Control", this.MyPrinterProfile.SupportedFeaturesConstants);
      if (this.PrinterState != PrinterObject.State.IsReady)
        return;
      this.m_bSupportedFeaturesChecked = true;
      if (this.m_tsbFeaturePanelShown.Value)
        return;
      this.m_tsbFeaturePanelShown.Value = true;
      bool flag = true;
      uint featuresBitField = this.Info.supportedFeatures.FeaturesBitField;
      string profileName = this.MyPrinterProfile.ProfileName;
      if (this.settings_manager.CurrentMiscSettings.LastProFeaturesFlag.ContainsKey(profileName))
      {
        if ((int) this.settings_manager.CurrentMiscSettings.LastProFeaturesFlag[profileName] == (int) featuresBitField)
        {
          flag = false;
        }
        else
        {
          this.settings_manager.CurrentMiscSettings.LastProFeaturesFlag[profileName] = featuresBitField;
          this.settings_manager.SaveSettings();
        }
      }
      else
      {
        this.settings_manager.CurrentMiscSettings.LastProFeaturesFlag.Add(profileName, featuresBitField);
        this.settings_manager.SaveSettings();
      }
      if (!flag)
        return;
      FeaturesDialog.Show(this.messagebox, this.spooler_connection, this);
    }

    private void ReleaseLockAfterCommand(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
        return;
      int num = (int) this.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private PrinterObject.State GetStateFromPrinter()
    {
      if (this.isConnected())
      {
        if (this.Info.Status == PrinterStatus.Firmware_Calibrating)
          return PrinterObject.State.IsCalibrating;
        if (this.Info.Status == PrinterStatus.Firmware_Executing || this.Info.Status == PrinterStatus.Firmware_Homing || (this.Info.Status == PrinterStatus.Firmware_WarmingUp || this.Info.Status == PrinterStatus.Connecting))
          return PrinterObject.State.IsBusy;
        if (this.Info.Status == PrinterStatus.Firmware_Ready || this.Info.Status == PrinterStatus.Firmware_Idle)
          return PrinterObject.State.IsReady;
        if (this.Info.Status == PrinterStatus.Firmware_Printing)
          return PrinterObject.State.IsPrinting;
        if (this.Info.Status == PrinterStatus.Firmware_PrintingPaused || this.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing)
          return PrinterObject.State.IsPaused;
        if (this.Info.Status == PrinterStatus.Bootloader_UpdatingFirmware)
          return PrinterObject.State.IsUpdatingFirmware;
      }
      return PrinterObject.State.IsNotHealthy;
    }

    public void OnProcessSpoolerMessage(SpoolerMessage message)
    {
      switch (message.Type)
      {
        case MessageType.PrinterShutdown:
          this.printer_shutdown.Value = true;
          break;
        case MessageType.BedLocationMustBeCalibrated:
        case MessageType.BedOrientationMustBeCalibrated:
          this.ReshowErrors();
          break;
        case MessageType.PrinterTimeout:
          lock (this.heater_lock)
          {
            this.heater_on = false;
            break;
          }
        case MessageType.CheckGantryClips:
          this.SendGantryClipMessage();
          break;
        case MessageType.FirmwareMustBeUpdated:
          this.firmware_message_sent = true;
          this.SendFirmwareMessageUserConfirm();
          break;
        case MessageType.FirmwareErrorCyclePower:
          this.printer_cyclepower.Value = true;
          break;
      }
    }

    public SpoolerResult DoFirmwareUpdate()
    {
      this.update_firmware_timer.Stop();
      this.update_firmware_timer.Reset();
      this.printerState.Value = PrinterObject.State.IsUpdatingFirmware;
      this.SendFirmwareUpdate();
      return this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.UpdateFirmwareAfterLock), (object) this);
    }

    public void TurnOnHeater(M3D.Spooling.Client.AsyncCallback callback, object state, int temperature)
    {
      int num = (int) this.SendManualGCode(callback, state, PrinterCompatibleString.Format("M104 S{0}", (object) temperature));
      lock (this.heater_lock)
        this.heater_on = true;
    }

    public void TurnOffHeater(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      lock (this.heater_lock)
        this.heater_on = false;
      int num = (int) this.SendManualGCode(callback, state, "M104 S0");
    }

    public void ReshowErrors()
    {
      this.calibration_message_sent = false;
      this.gantry_clip_message_sent = false;
    }

    public SpoolerResult PrintTestBorder(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      string str1 = Path.Combine(Paths.WorkingFolder, "m3doutput.gcode");
      if (this.MyFilamentProfile == null || this.MyFilamentProfile.Type == FilamentSpool.TypeEnum.NoFilament)
        return SpoolerResult.Error;
      BedCompensationPreprocessor compensationPreprocessor = new BedCompensationPreprocessor();
      compensationPreprocessor.UpdateConfigurations(this.Info.calibration, this.MyPrinterProfile.PrinterSizeConstants, true);
      StreamWriter text = File.CreateText(str1);
      Vector frontLeft = compensationPreprocessor.FrontLeft;
      Vector frontRight = compensationPreprocessor.FrontRight;
      Vector backLeft = compensationPreprocessor.BackLeft;
      Vector backRight = compensationPreprocessor.BackRight;
      foreach (string str2 in GCodeGeneration.CreatePrintTestBorder(frontLeft.x, frontLeft.y, frontRight.x, backRight.y, 1f, (float) this.MyFilamentProfile.Temperature, 1.75f))
        text.WriteLine(str2);
      text.Close();
      return this.PrintModel(callback, state, new JobParams(str1, nameof (PrintTestBorder), (string) null, this.MyFilamentProfile.Type, 0.0f, 0.0f) { preprocessor = this.MyFilamentProfile.preprocessor, filament_temperature = this.MyFilamentProfile.Temperature, options = { bounds_check_xy = false, autostart_ignorewarnings = true } });
    }

    public void ShowLockError(IAsyncCallResult ar)
    {
      string lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
        return;
      this.messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
    }

    public static string GetLockErrorMessage(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success_LockReleased || (ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived))
        return (string) null;
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
      string lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
        return true;
      this.messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
      return false;
    }

    private void onAutoCommand(IAsyncCallResult ar)
    {
      PrinterObject.AutoReleaseState asyncState = ar.AsyncState as PrinterObject.AutoReleaseState;
      if (asyncState == null)
        throw new Exception("type doesn't match AutoReleaseState");
      if (ar.CallResult != CommandResult.Success)
      {
        if (asyncState.callback == null)
          return;
        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
      {
        if (!asyncState.autoRelease)
          return;
        int num = (int) asyncState.printer.ReleaseLock(asyncState.callback, asyncState.state);
      }
    }

    private void onAutoLock(IAsyncCallResult ar)
    {
      PrinterObject.AutoReleaseState asyncState = ar.AsyncState as PrinterObject.AutoReleaseState;
      if (asyncState == null)
        throw new Exception("type doesn't match AutoReleaseState");
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        if (asyncState.callback == null)
          return;
        asyncState.callback((IAsyncCallResult) new SimpleAsyncCallResult(asyncState.state, ar.CallResult));
      }
      else
        this.SendGCodeAutoHome(asyncState);
    }

    private void SendGCodeAutoHome(PrinterObject.AutoReleaseState state)
    {
      List<string> stringList = new List<string>();
      if (state.homeFirst && this.Info.extruder.ishomed != Trilean.True)
      {
        if ((double) this.Info.extruder.position.pos.z < 2.0)
        {
          stringList.Add("G91");
          stringList.Add("G0 Z2");
        }
        stringList.Add("G28");
        stringList.Add("M114");
      }
      stringList.AddRange((IEnumerable<string>) state.gcode);
      int num = (int) state.printer.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.onAutoCommand), (object) state, stringList.ToArray());
    }

    public void SendCommandAutoLockReleaseHome(M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      this.SendCommandAutoLock(false, true, callback, state, gcode);
    }

    public void SendCommandAutoLockRelease(M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      this.SendCommandAutoLock(false, false, callback, state, gcode);
    }

    public void SendCommandAutoLock(bool keeplock, bool homeIfNeeded, M3D.Spooling.Client.AsyncCallback callback, object state, params string[] gcode)
    {
      PrinterObject.AutoReleaseState state1 = new PrinterObject.AutoReleaseState(callback, state, (IPrinter) this, gcode, homeIfNeeded, !keeplock);
      if (this.HasLock)
      {
        this.SendGCodeAutoHome(state1);
      }
      else
      {
        int num = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.onAutoLock), (object) new PrinterObject.AutoReleaseState(callback, state, (IPrinter) this, gcode, true, true));
      }
    }

    public bool MarkedAsBusy
    {
      get
      {
        return this.marked_as_busy.Value;
      }
      set
      {
        this.marked_as_busy.Value = value;
      }
    }

    public bool HeaterOn
    {
      get
      {
        lock (this.heater_lock)
          return this.heater_on;
      }
    }

    public bool isBusy
    {
      get
      {
        switch (this.PrinterState)
        {
          case PrinterObject.State.IsCalibrating:
          case PrinterObject.State.IsUpdatingFirmware:
          case PrinterObject.State.IsBusy:
          case PrinterObject.State.IsPrinting:
            return true;
          default:
            if (this.Info.Status != PrinterStatus.Connecting && !this.HasLock)
              return this.WaitingForCommandToComplete;
            goto case PrinterObject.State.IsCalibrating;
        }
      }
    }

    public bool isHealthy
    {
      get
      {
        switch (this.PrinterState)
        {
          case PrinterObject.State.IsUpdatingFirmware:
          case PrinterObject.State.IsNotHealthy:
            return false;
          default:
            if (!this.calibration_invalid)
              return !this.invalid_firmware;
            goto case PrinterObject.State.IsUpdatingFirmware;
        }
      }
    }

    public PrinterObject.State PrinterState
    {
      get
      {
        return this.printerState.Value;
      }
    }

    public bool IsPausedorPausing
    {
      get
      {
        if (this.Info.Status != PrinterStatus.Firmware_IsWaitingToPause && this.Info.Status != PrinterStatus.Firmware_PrintingPaused)
          return this.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing;
        return true;
      }
    }

    public bool HasHeatedBed { get; private set; }

    public static string GetFamilyFromProfile(string sProfileName)
    {
      string str = "";
      if (sProfileName.StartsWith("Micro"))
        str = "Micro";
      else if (sProfileName.StartsWith("Pro"))
        str = "Pro";
      return str;
    }

    public static bool IsProfileMemberOfFamily(string sProfileName, string sFamilyName)
    {
      return sProfileName.StartsWith("Micro") && sFamilyName.Equals("Micro") || sProfileName.StartsWith("Pro") && sFamilyName.Equals("Pro");
    }

    public void CalibrateBed(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode)
    {
      if (!true)
        this.messagebox.AddMessageToQueue("Your printer will begin calibrating. Are you using a clear flexible plastic print bed or a hard one?", Locale.GlobalLocale.T("T_Flexible"), Locale.GlobalLocale.T("T_Hard"), Locale.GlobalLocale.T("T_Cancel"), (PopupMessageBox.OnUserSelectionDel) ((result, type, sn, user_data) =>
        {
          PrinterObject.CalibrationData calibrationData = (PrinterObject.CalibrationData) user_data;
          if (!this.Connected)
            return;
          if (result == PopupMessageBox.PopupResult.Button1_YesOK)
          {
            this.CalibrationWorker(callback, state, calibrationData.mode, 0.4f);
          }
          else
          {
            if (result != PopupMessageBox.PopupResult.Button2_NoCancel)
              return;
            this.CalibrationWorker(callback, state, calibrationData.mode, 0.0f);
          }
        }), (object) new PrinterObject.CalibrationData(mode));
      else
        this.CalibrationWorker(callback, state, mode, 0.4f);
    }

    public void RequestCalibrationFromUser(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode)
    {
      this.messagebox.AddMessageToQueue(new SpoolerMessage(mode != PrinterObject.CalibrationType.CalibrateFull_G32 ? MessageType.BedLocationMustBeCalibrated : MessageType.BedOrientationMustBeCalibrated, this.Info.serial_number, (string) null), PopupMessageBox.MessageBoxButtons.YESNO, (PopupMessageBox.OnUserSelectionDel) ((result, type, sn, user_data) =>
      {
        if (result != PopupMessageBox.PopupResult.Button1_YesOK || !(sn == this.Info.serial_number) || !this.Connected)
          return;
        int num = (int) this.AcquireLock((M3D.Spooling.Client.AsyncCallback) (ar =>
        {
          PrinterObject.CalibrationDataCallback asyncState = ar.AsyncState as PrinterObject.CalibrationDataCallback;
          this.CalibrateBed(asyncState.callback, asyncState.state, asyncState.mode);
        }), (object) new PrinterObject.CalibrationDataCallback(callback, state, mode));
      }), (object) new PrinterObject.CalibrationData(mode));
    }

    private void CalibrationWorker(M3D.Spooling.Client.AsyncCallback callback, object state, PrinterObject.CalibrationType mode, float z_offset)
    {
      List<string> stringList = new List<string>();
      stringList.Add("G91");
      stringList.Add("M104 S150");
      if (PrinterObject.IsProfileMemberOfFamily(this.MyPrinterProfile.ProfileName, "Pro"))
        stringList.Add("G0 Z8 F300");
      else
        stringList.Add("G0 Y20 Z2 F3000");
      if (mode == PrinterObject.CalibrationType.CalibrateQuick_G30 && PrinterObject.IsProfileMemberOfFamily(this.MyPrinterProfile.ProfileName, "Pro"))
        stringList.Add("G28");
      stringList.Add("M109 S150");
      stringList.Add("M106 S0");
      if (this.Info.calibration.UsesCalibrationOffset)
        z_offset += this.Info.calibration.CALIBRATION_OFFSET;
      if (mode == PrinterObject.CalibrationType.CalibrateFull_G32)
        stringList.Add("G32 Z" + (object) z_offset);
      else
        stringList.Add("G30 Z" + (object) z_offset);
      stringList.Add("M104 S0");
      int num = (int) this.SendManualGCode(callback, state, stringList.ToArray());
      this.printerState.Value = PrinterObject.State.IsCalibrating;
      this.infobox.AddMessageToQueue(this.Info.serial_number.ToString() + " is calibrating.");
    }

    public void SendFirmwareMessageUserConfirm()
    {
      SpoolerMessage message = new SpoolerMessage(MessageType.UserDefined, this.Info.serial_number, "");
      string imageWidget = this.CreateImageWidget(this.Info.serial_number.ToString());
      string xmlsource = "<?xml version=\"1.0\" encoding=\"utf-16\"?><XMLFrame id=\"1000\" width=\"550\" height=\"300\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1001\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"640\" texture-v0=\"320\" texture-u1=\"704\" texture-v1=\"383\" center-vertically=\"1\" center-horizontally=\"1\" leftbordersize-pixels=\"41\" rightbordersize-pixels=\"8\" minimumwidth=\"64\" topbordersize-pixels=\"35\" bottombordersize-pixels=\"8\" minimumheight=\"64\" />    <TextWidget id=\"1002\" x=\"50\" y=\"2\" width=\"298\" height=\"35\" font-size=\"Large\" font-color=\"#FF808080\" alignment=\"Left\">Firmware Update</TextWidget>    <XMLFrame id=\"1004\" x=\"190\" y=\"20\" width=\"330\" height=\"260\">        <TextWidget id=\"1003\" x=\"0\" y=\"32\" relative-width=\"1.0\" height=\"110\" font-size=\"Medium\" font-color=\"#FF404040\" alignment=\"Centre\">" + Locale.GlobalLocale.T("T_FIRMWARE_UPDATE_REQUIRED") + "</TextWidget>        <ButtonWidget id=\"2000\" x=\"20\" y=\"132\" width=\"200\" height=\"48\" font-size=\"Large\" alignment=\"Centre\" has_focus=\"1\" center-horizontally=\"1\">Update Now</ButtonWidget>    </XMLFrame>" + imageWidget + "</XMLFrame>";
      this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(message, xmlsource, new PopupMessageBox.XMLButtonCallback(this.OnFirmwareUpdateButton), (object) null, new ElementStandardDelegate(this.OnFirmwareOnUpdateCallback)));
    }

    private void SendFirmwareUpdate()
    {
      SpoolerMessage message = new SpoolerMessage(MessageType.UserDefined, this.Info.serial_number, "");
      this.ok_sent = false;
      this.update_firmware_timer.Start();
      this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(message, this.GenerateFirmwareUpdateXML(this.Info.serial_number.ToString()), (PopupMessageBox.XMLButtonCallback) null, (object) null, new ElementStandardDelegate(this.OnFirmwareOnUpdateCallback)));
    }

    private void OnFirmwareUpdateButton(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID != 2000)
        return;
      if (childFrame.Host != null)
        childFrame.Host.SetFocus((Element2D) null);
      childFrame.Init(childFrame.Host, this.GenerateFirmwareUpdateXML(this.Info.serial_number.ToString()), (ButtonCallback) null);
      childFrame.DoOnUpdate = new ElementStandardDelegate(this.OnFirmwareOnUpdateCallback);
      int num = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.UpdateFirmwareAfterLock), (object) this);
      this.ok_sent = false;
      this.update_firmware_timer.Start();
    }

    private void UpdateFirmwareAfterLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        this.VerifyPrinterResults(ar);
      }
      else
      {
        int num = (int) this.DoFirmwareUpdate(new M3D.Spooling.Client.AsyncCallback(this.VerifyPrinterResults), (object) this);
      }
    }

    private void VerifyPrinterResults(IAsyncCallResult ar)
    {
      if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        this.messagebox.CloseCurrent();
        this.infobox.AddMessageToQueue(this.Info.serial_number.ToString() + " failed because it is already in use.");
      }
      else
      {
        if (ar.CallResult == CommandResult.Success_LockAcquired || ar.CallResult == CommandResult.Success_LockReleased || ar.CallResult == CommandResult.Success)
          return;
        this.messagebox.CloseCurrent();
        this.infobox.AddMessageToQueue(this.Info.serial_number.ToString() + " failed. Please try again.");
      }
    }

    private void OnFirmwareOnUpdateCallback()
    {
      if (this.update_firmware_timer.IsRunning && this.update_firmware_timer.ElapsedMilliseconds > 5000L)
      {
        if (this.PrinterState == PrinterObject.State.IsReady && !this.ok_sent)
        {
          this.ok_sent = true;
          this.update_firmware_timer.Stop();
          this.messagebox.CloseCurrent();
          this.infobox.AddMessageToQueue(SpoolerConnection.LocalizedSpoolerMessageString(MessageType.FirmwareUpdateComplete));
          this.spooler_connection.SelectPrinterBySerialNumber(this.Info.serial_number.ToString());
        }
        if ((this.update_firmware_timer.ElapsedMilliseconds > 300000L || this.Info.Status == PrinterStatus.Bootloader_FirmwareUpdateFailed) && !this.ok_sent)
        {
          this.ok_sent = true;
          this.update_firmware_timer.Stop();
          this.messagebox.CloseCurrent();
          this.messagebox.AddMessageToQueue("Failed to update printer " + this.Info.serial_number.ToString() + ". Please reset the printer by disconnecting it and then reconnecting it to power.");
        }
      }
      if (!this.printer_shutdown.Value && !this.printer_cyclepower.Value || this.ok_sent)
        return;
      this.ok_sent = true;
      this.messagebox.CloseCurrent();
      this.infobox.AddMessageToQueue(this.Info.serial_number.ToString() + " was disconnected while updating firmware");
      if (this.update_firmware_timer.IsRunning && this.printer_shutdown.Value)
        this.messagebox.AddMessageToQueue("Critical Error: The printer was disconnected while updating. This may have cause problems with the USB connection. Please disconnect the printer from the computer and from power and then reset the computer. Then reconnect the printer.");
      this.printer_shutdown.Value = false;
      this.printer_cyclepower.Value = false;
      this.update_firmware_timer.Stop();
    }

    private string GenerateFirmwareUpdateXML(string sn)
    {
      return "<?xml version=\"1.0\" encoding=\"utf-16\"?><XMLFrame id=\"1000\" width=\"550\" height=\"300\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1001\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"640\" texture-v0=\"320\" texture-u1=\"704\" texture-v1=\"383\" center-vertically=\"1\" center-horizontally=\"1\" leftbordersize-pixels=\"41\" rightbordersize-pixels=\"8\" minimumwidth=\"64\" topbordersize-pixels=\"35\" bottombordersize-pixels=\"8\" minimumheight=\"64\" />    <TextWidget id=\"1002\" x=\"50\" y=\"2\" width=\"298\" height=\"35\" font-size=\"Large\" font-color=\"#FF808080\" alignment=\"Left\">Firmware Update</TextWidget>    <XMLFrame id=\"1004\" x=\"190\" y=\"20\" width=\"330\" height=\"260\">        <TextWidget id=\"1003\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"110\" font-size=\"Medium\" font-color=\"#FF404040\" alignment=\"Centre\">Please wait. Updates are being sent to the printer. Please do not disconnect or unplug the printer.</TextWidget>        <SpriteAnimationWidget id=\"1001\" x=\"0\" y=\"120\" width=\"128\" height=\"108\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"768\" texture-u1=\"767\" texture-v1=\"1023\" center-horizontally=\"1\" columns=\"6\" rows=\"2\" frames=\"12\" frame-time=\"200\" />    </XMLFrame>" + this.CreateImageWidget(this.Info.serial_number.ToString()) + "</XMLFrame>";
    }

    private string CreateImageWidget(string sn)
    {
      ImageResourceMapping.PixelCoordinate pixelCoordinate = ImageResourceMapping.PrinterColorPosition(sn);
      return "<ImageWidget id=\"1001\" text-area-height=\"20\" image-area-width=\"130\" x=\"30\" y=\"80\" width=\"150\" height=\"150\" src=\"extendedcontrols\" " + string.Format("texture-u0=\"{0}\" texture-v0=\"{1}\" texture-u1=\"{2}\" texture-v1=\"{3}\" vertical-alignment=\"Bottom\" font-size=\"Small\">", (object) pixelCoordinate.u0, (object) pixelCoordinate.v0, (object) pixelCoordinate.u1, (object) pixelCoordinate.v1) + this.Info.serial_number.ToString() + "</ImageWidget>";
    }

    public void SendGantryClipMessage()
    {
      this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(MessageType.CheckGantryClips, this.Info.serial_number, ""), "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"397\" height=\"397\" src=\"extendedcontrols\" texture-u0=\"450\" texture-v0=\"194\" texture-u1=\"847\" texture-v1=\"591\" center-vertically=\"1\" />    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\" />    <TextWidget id=\"1003\" x=\"418\" y=\"20\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">A new 3D printer has been connected.</TextWidget>    <XMLFrame id=\"1004\" x=\"418\" y=\"84\" width=\"272\" height=\"336\" center-vertically=\"0\">        <TextWidget id=\"1005\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"150\" font-size=\"Medium\" font-color=\"#FF646464\" alignment=\"Centre\" center-horizontally=\"1\">Before continuing, the printer needs to verify that it's gantry clips have been removed. Please make sure the gantry clips have been removed and that the print area is clear.        </TextWidget>        <ButtonWidget id=\"1006\" x=\"20\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\" has_focus=\"1\">Verify</ButtonWidget>        <ButtonWidget id=\"1007\" x=\"-120\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Cancel</ButtonWidget>    </XMLFrame></XMLFrame>", new PopupMessageBox.XMLButtonCallback(this.OnGantryClipButton), (object) null, new ElementStandardDelegate(this.CloseIfNotConnected)));
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
          return;
        string xmlScript = "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"397\" height=\"397\" src=\"extendedcontrols\" texture-u0=\"450\" texture-v0=\"194\" texture-u1=\"847\" texture-v1=\"591\" center-vertically=\"1\"></ImageWidget>    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\"></ImageWidget>    <TextWidget id=\"1003\" x=\"418\" y=\"20\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">Please wait. Your printer is being tested.</TextWidget>    <XMLFrame id=\"1004\" x=\"418\" y=\"42\" width=\"272\" height=\"336\" center-vertically=\"0\">        <SpriteAnimationWidget id=\"1001\" x=\"0\" y=\"0\" width=\"128\" height=\"108\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"768\" texture-u1=\"767\" texture-v1=\"1023\" center-vertically=\"1\" center-horizontally=\"1\" columns=\"6\" rows=\"2\" frames=\"12\" frame-time=\"200\" />    </XMLFrame></XMLFrame>";
        if (childFrame.Host != null)
          childFrame.Host.SetFocus((Element2D) null);
        childFrame.Init(childFrame.Host, xmlScript, (ButtonCallback) null);
        int num = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step1_Lock);
      }
    }

    private void GantryClipCallBack(IAsyncCallResult ar)
    {
      PrinterObject.GantryClipStep asyncState = (PrinterObject.GantryClipStep) ar.AsyncState;
      if (asyncState == PrinterObject.GantryClipStep.Step1_Lock && ar.CallResult == CommandResult.Success_LockAcquired)
      {
        int num1 = (int) this.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(this.GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step2_SendM583, "M583");
      }
      else if (asyncState == PrinterObject.GantryClipStep.Step2_SendM583 && ar.CallResult == CommandResult.Success)
      {
        int num2 = (int) this.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(this.GantryClipCallBack), (object) PrinterObject.GantryClipStep.Step3_CheckStateAndRelease);
      }
      else if (asyncState == PrinterObject.GantryClipStep.Step3_CheckStateAndRelease && ar.CallResult == CommandResult.Success_LockReleased)
      {
        this.messagebox.CloseCurrent();
        if (this.Info.persistantData.GantryClipsRemoved)
        {
          this.messagebox.AddMessageToQueue("We have verified that your gantry clips have been removed.", PopupMessageBox.MessageBoxButtons.OK);
        }
        else
        {
          this.messagebox.AddMessageToQueue("Sorry, but your gantry clips have not been removed.", PopupMessageBox.MessageBoxButtons.OK);
          this.SendGantryClipMessage();
        }
      }
      else
      {
        this.messagebox.CloseCurrent();
        if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
        {
          this.messagebox.AddMessageToQueue("Unable to verify because the printer is being used by another application.", PopupMessageBox.MessageBoxButtons.OK);
        }
        else
        {
          this.messagebox.AddMessageToQueue("Unable to communicate with the printer. Please try again.", PopupMessageBox.MessageBoxButtons.OK);
          this.SendGantryClipMessage();
        }
      }
    }

    private void CloseIfNotConnected()
    {
      if (this.Connected)
        return;
      this.messagebox.CloseCurrent();
    }

    public void SendCalibrationOutofDateMessage(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(MessageType.CheckGantryClips, this.Info.serial_number, ""), "<?xml version=\"1.0\" encoding=\"utf-16\"?>  <XMLFrame id=\"1000\" width=\"700\" height=\"420\" center-vertically=\"1\" center-horizontally=\"1\">    <ImageWidget id=\"1000\" x=\"0\" y=\"0\" relative-width=\"1.0\" relative-height=\"1.0\" src=\"guicontrols\" texture-u0=\"768\" texture-v0=\"384\" texture-u1=\"895\" texture-v1=\"511\" leftbordersize-pixels=\"12\" rightbordersize-pixels=\"12\" minimumwidth=\"128\" topbordersize-pixels=\"12\" bottombordersize-pixels=\"12\" minimumheight=\"128\" />    <ImageWidget id=\"1001\" x=\"10\" y=\"10\" width=\"367\" height=\"282\" src=\"extendedcontrols\" texture-u0=\"448\" texture-v0=\"592\" texture-u1=\"814\" texture-v1=\"873\" center-vertically=\"1\" />    <ImageWidget id=\"1002\" x=\"-160\" y=\"-40\" width=\"129\" height=\"31\" src=\"guicontrols\" texture-u0=\"0\" texture-v0=\"737\" texture-u1=\"128\" texture-v1=\"767\" />    <TextWidget id=\"1003\" x=\"400\" y=\"60\" width=\"272\" height=\"64\" font-size=\"Large\" font-color=\"#FFff6600\" alignment=\"Centre\">Calibration Update Available</TextWidget>    <XMLFrame id=\"1004\" x=\"400\" y=\"124\" width=\"272\" height=\"336\" center-vertically=\"0\">        <TextWidget id=\"1005\" x=\"0\" y=\"0\" relative-width=\"1.0\" height=\"150\" font-size=\"Medium\" font-color=\"#FF646464\" alignment=\"Centre\" center-horizontally=\"1\">Print calibration has been updated. Before continuing, it is highly recommended that the printer be recalibrated.\n\nThis may take up to 30 minutes.        </TextWidget>        <ButtonWidget id=\"1006\" x=\"20\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Calibrate</ButtonWidget>        <ButtonWidget id=\"1007\" x=\"-120\" y=\"-164\" width=\"100\" height=\"48\" font-size=\"Large\" alignment=\"Centre\">Ignore</ButtonWidget>    </XMLFrame></XMLFrame>", new PopupMessageBox.XMLButtonCallback(this.OnCalibrationOutOfDateButton), (object) new PrinterObject.CalibrationDataCallback(callback, state, PrinterObject.CalibrationType.CalibrateFull_G32)));
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
          return;
        parentFrame.CloseCurrent();
        PrinterObject.CalibrationDataCallback calibrationDataCallback = data as PrinterObject.CalibrationDataCallback;
        if (calibrationDataCallback == null)
          return;
        this.CalibrateBed(calibrationDataCallback.callback, calibrationDataCallback.state, calibrationDataCallback.mode);
      }
    }

    public override SpoolerResult SetFilamentInfo(M3D.Spooling.Client.AsyncCallback callback, object state, FilamentSpool info)
    {
      if (info == (FilamentSpool) null)
        return this.SetFilamentToNone(callback, state);
      this.filament_profile = FilamentProfile.CreateFilamentProfile(info, this.MyPrinterProfile);
      return base.SetFilamentInfo(callback, state, info);
    }

    public override SpoolerResult SetFilamentToNone(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      this.filament_profile = (FilamentProfile) null;
      return base.SetFilamentToNone(callback, state);
    }

    public FilamentProfile MyFilamentProfile
    {
      get
      {
        return this.filament_profile;
      }
    }

    private void CheckPowerOutageState()
    {
      if (this.m_bPowerOutageRecoveryDetected && !this.Info.powerRecovery.bPowerOutageWhilePrinting)
      {
        this.m_bPowerOutageRecoveryDetected = false;
      }
      else
      {
        if (!this.Info.powerRecovery.bPowerOutageWhilePrinting || this.m_bPowerOutageRecoveryDetected)
          return;
        this.m_bPowerOutageRecoveryDetected = true;
        this.messagebox.AddMessageToQueue("We detected that a power failure interrupted your print. Would you like to resume printing?", "Power Outage Recovery", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.OnPowerOutageMessageBox));
      }
    }

    private void OnPowerOutageMessageBox(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        this.messagebox.AddMessageToQueue(new PopupMessageBox.MessageDataStandard(new SpoolerMessage(MessageType.UserDefined, "Was the extruder head moved manually after power failure? If so, we must re-home the extruder and accuracy of the continued print may be somewhat degraded."), PopupMessageBox.MessageBoxButtons.CUSTOM, new PopupMessageBox.OnUserSelectionDel(this.OnPowerOutageConfirmG28MessageBox), (object) null)
        {
          custom_button1_text = "Yes\nHead was moved",
          custom_button2_text = "No\nHead was not moved",
          custom_button3_text = "Cancel",
          title = "Power Outage Recovery"
        });
      }
      else
      {
        int num = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.ClearFault);
      }
    }

    private void OnPowerOutageConfirmG28MessageBox(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        int num1 = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.RecoverG28);
      }
      else
      {
        if (result != PopupMessageBox.PopupResult.Button2_NoCancel)
          return;
        int num2 = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Recover);
      }
    }

    private void PowerRecoveryAfterLock(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success_LockAcquired)
      {
        this.CheckForLockError(ar);
      }
      else
      {
        PrinterObject.PowerOutageAction asyncState = (PrinterObject.PowerOutageAction) ar.AsyncState;
        switch (asyncState)
        {
          case PrinterObject.PowerOutageAction.ClearFault:
            int num1 = (int) this.ClearPowerRecoveryFault(new M3D.Spooling.Client.AsyncCallback(this.PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Release);
            break;
          case PrinterObject.PowerOutageAction.Recover:
          case PrinterObject.PowerOutageAction.RecoverG28:
            int num2 = (int) this.RecoveryPrintFromPowerFailure(new M3D.Spooling.Client.AsyncCallback(this.PowerRecoveryAfterLock), (object) PrinterObject.PowerOutageAction.Release, PrinterObject.PowerOutageAction.RecoverG28 == asyncState);
            break;
          default:
            int num3 = (int) this.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
            break;
        }
      }
    }

    public SDCardExtensions SDCardExtension { get; private set; }

    public void AutoLockAndPrint(JobParams UserJob)
    {
      int num = (int) this.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.OnLockForPrintGCode), (object) UserJob);
    }

    private void OnLockForPrintGCode(IAsyncCallResult ar)
    {
      JobParams asyncState = (JobParams) ar.AsyncState;
      if (ar.CallResult == CommandResult.Success_LockAcquired)
      {
        int num = (int) this.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, asyncState);
      }
      else
        this.ShowLockError(ar);
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
