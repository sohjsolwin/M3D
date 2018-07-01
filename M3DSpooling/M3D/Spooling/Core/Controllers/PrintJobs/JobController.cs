// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.JobController
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal class JobController
  {
    private ThreadSafeVariable<AbstractJob> m_oJobImplementation = new ThreadSafeVariable<AbstractJob>((AbstractJob) null);
    private ThreadSafeVariable<JobParams.Mode> m_JobMode = new ThreadSafeVariable<JobParams.Mode>();
    private ThreadSafeVariable<bool> m_bProcessed = new ThreadSafeVariable<bool>(false);
    private ThreadSafeVariable<bool> m_bSavingToSD = new ThreadSafeVariable<bool>(false);
    private ThreadSafeVariable<bool> m_bSimultaneousPrint = new ThreadSafeVariable<bool>(false);
    private ThreadSafeVariable<bool> printing = new ThreadSafeVariable<bool>(false);
    private IPublicFirmwareController m_oParentFirmwareController;
    private List<MessageType> m_olWarnings;
    private Stopwatch job_timer;
    private string m_sGcodeFile;
    private bool m_bUpdatedDataReceivedAfterPause;
    private Vector3DE m_v3DeExtruderLocationAtPause;
    private float m_lsAdditionalTimeRemaining;
    private float m_fRetractionAtPause;
    private const float DefaultRetractionAtPause = 10f;
    private const int DefaultFanReset = 255;

    public JobController(IPublicFirmwareController ParentFirmwareController)
    {
      this.m_oParentFirmwareController = ParentFirmwareController;
      this.job_timer = new Stopwatch();
    }

    public CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo)
    {
      return this.InitPrintJob(new JobParams(jobParams), "", ulLineToSkipTo) == JobController.Result.Success ? CommandResult.Success : CommandResult.Failed_Exception;
    }

    public void ConnectToRunningSDPrint(string user)
    {
      PrinterInfo currentPrinterInfo = this.m_oParentFirmwareController.CurrentPrinterInfo;
      JobParams jobParams = currentPrinterInfo.persistantData.SavedJobInformation == null ? new JobParams((string) null, "Print From SD", (string) null, FilamentSpool.TypeEnum.OtherOrUnknown, 0.0f, 0.0f) : currentPrinterInfo.persistantData.SavedJobInformation.Params;
      this.ClearAllWarnings();
      this.m_sGcodeFile = "";
      this.Mode = JobParams.Mode.FirmwarePrintingFromSDCard;
      FirmwareSDPrintJob firmwareSdPrintJob = new FirmwareSDPrintJob(jobParams, user, this.m_oParentFirmwareController.MyPrinterProfile);
      this.m_oJobImplementation.Value = (AbstractJob) firmwareSdPrintJob;
      firmwareSdPrintJob.ConnectToRunningSDPrint();
      this.Processed = true;
      this.Printing = true;
      this.SaveJobState();
    }

    public JobController.Result InitPrintJob(JobParams jobParams, string user, ulong ulFastForward = 0)
    {
      if (this.MyJobImplementation != null)
        return JobController.Result.FAILED_JobAlreadyStarted;
      this.Processed = false;
      this.Printing = false;
      this.IsSavingToSD = false;
      this.ClearAllWarnings();
      this.m_sGcodeFile = jobParams.gcodefile;
      this.Mode = jobParams.jobMode;
      PrinterInfo currentPrinterInfo = this.m_oParentFirmwareController.CurrentPrinterInfo;
      AbstractJob abstractJob;
      if (this.Mode == JobParams.Mode.SaveToBinaryGCodeFile)
        abstractJob = (AbstractJob) new SaveGCodeToFileJob(jobParams, user, this.m_oParentFirmwareController.MyPrinterProfile);
      else if (this.Mode != JobParams.Mode.FirmwarePrintingFromSDCard)
      {
        if (this.Mode == JobParams.Mode.SavingToSDCard || this.Mode == JobParams.Mode.SavingToSDCardAutoStartPrint)
          this.m_sGcodeFile = this.GetFilenameForSDCard(jobParams.jobname, currentPrinterInfo.filament_info);
        this.IsSimultaneousPrint = false;
        this.IsSavingToSD = this.Mode != JobParams.Mode.DirectPrinting;
        abstractJob = (AbstractJob) new SpoolerHostedJob(jobParams, user, this.m_oParentFirmwareController.MyPrinterProfile, this.IsSavingToSD && !this.IsSimultaneousPrint, ulFastForward);
      }
      else
      {
        if (this.Mode != JobParams.Mode.FirmwarePrintingFromSDCard)
          throw new NotImplementedException("Software does not support printing from the SD card.");
        abstractJob = (AbstractJob) new FirmwareSDPrintJob(jobParams, user, this.m_oParentFirmwareController.MyPrinterProfile);
      }
      this.m_oJobImplementation.Value = abstractJob;
      JobCreateResult jobCreateResult = abstractJob.Create(currentPrinterInfo);
      if (jobCreateResult.Result == ProcessReturn.SUCCESS)
      {
        this.m_olWarnings = jobCreateResult.Warnings;
        this.Processed = true;
        return JobController.Result.Success;
      }
      this.m_oJobImplementation.Value = (AbstractJob) null;
      if (jobCreateResult.Result == ProcessReturn.FAILURE_OUT_OF_BOUNDS)
        return JobController.Result.FAILED_OutOfBounds;
      return jobCreateResult.Result == ProcessReturn.FAILURE_FILAMENT_MISMATCH ? JobController.Result.FAILED_IncompatibleFilament : JobController.Result.FAILED_Create;
    }

    public JobController.Result Start(out List<string> start_gcode)
    {
      start_gcode = (List<string>) null;
      if (this.MyJobImplementation == null)
        return JobController.Result.FAILED_Create;
      this.ClearAllWarnings();
      if (this.m_JobMode.Value == JobParams.Mode.SavingToSDCardAutoStartPrint)
        this.m_lsAdditionalTimeRemaining = this.MyJobImplementation.EstimatedPrintTime;
      List<string> start_gcode1;
      if (!this.MyJobImplementation.Start(out start_gcode1))
        return JobController.Result.FAILED_JobNotStarted;
      this.job_timer.Restart();
      if (this.IsSavingToSD)
      {
        if (start_gcode == null)
          start_gcode = new List<string>();
        start_gcode = new List<string>()
        {
          string.Format("M28 {0}", (object) this.m_sGcodeFile)
        };
      }
      else if (this.MyJobImplementation.Details.jobParams.jobMode != JobParams.Mode.SaveToBinaryGCodeFile && this.MyJobImplementation.Details.jobParams.options.calibrate_before_print)
      {
        float calibrateBeforePrintZ = this.MyJobImplementation.Details.jobParams.options.calibrate_before_print_z;
        start_gcode = PrinterCalibration.CreateCalibrationGCode(PrinterCalibration.Type.G30, calibrateBeforePrintZ, 5f, this.m_oParentFirmwareController.MyPrinterProfile.OptionsConstants);
      }
      if (start_gcode == null)
        start_gcode = start_gcode1;
      else if (start_gcode1 != null)
        start_gcode.AddRange((IEnumerable<string>) start_gcode1);
      this.Printing = true;
      this.SaveJobState();
      return JobController.Result.Success;
    }

    public JobController.Result ForceShutdownNow()
    {
      AbstractJob jobImplementation = this.MyJobImplementation;
      if (this.MyJobImplementation == null)
        return JobController.Result.FAILED_JobNotStarted;
      this.SaveJobState();
      this.MyJobImplementation.Stop();
      this.job_timer.Stop();
      this.m_oJobImplementation.Value = (AbstractJob) null;
      this.Printing = false;
      return JobController.Result.Success;
    }

    public JobController.Result StopJob(out List<string> end_gcode)
    {
      AbstractJob jobImplementation = this.MyJobImplementation;
      end_gcode = (List<string>) null;
      if (this.MyJobImplementation == null)
        return JobController.Result.FAILED_JobNotStarted;
      this.FinalizeEndOfJob(out end_gcode);
      this.job_timer.Stop();
      this.Printing = false;
      return JobController.Result.Success;
    }

    public void FinalizeEndOfJob(out List<string> end_gcode)
    {
      end_gcode = (List<string>) null;
      if (this.IsSavingToSD)
      {
        end_gcode = new List<string>();
        end_gcode.Add("M29");
        if (!this.m_oJobImplementation.Value.Done)
          end_gcode.Add(string.Format("M30 {0}", (object) this.m_sGcodeFile));
        this.MyJobImplementation.Stop();
      }
      this.m_oJobImplementation.Value = (AbstractJob) null;
      this.SaveJobState();
    }

    public bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      pause_gcode = (List<string>) null;
      AbstractJob jobImplementation = this.MyJobImplementation;
      if (jobImplementation == null || jobImplementation.Status == JobStatus.Paused || this.IsSavingToSD)
        return false;
      this.job_timer.Stop();
      bool flag = jobImplementation.Pause(out pause_gcode, spool);
      if (flag)
      {
        if (pause_gcode == null)
          pause_gcode = new List<string>();
        float num1 = 90f;
        float num2 = 1800f;
        Vector2D cornerPositionBoxTop = this.m_oParentFirmwareController.MyPrinterProfile.PrinterSizeConstants.BackCornerPositionBoxTop;
        string str;
        if ((double) this.m_oParentFirmwareController.CurrentPrinterInfo.extruder.Temperature > 0.0)
        {
          this.m_fRetractionAtPause = !((FilamentSpool) null != spool) || spool.filament_type == FilamentSpool.TypeEnum.NoFilament ? 10f : (float) FilamentProfile.CreateFilamentProfile(spool, (PrinterProfile) this.m_oParentFirmwareController.MyPrinterProfile).preprocessor.initialPrint.PrimeAmount;
          str = PrinterCompatibleString.Format("G0 Z{0} F{1} E-{2}", (object) 8f, (object) num1, (object) this.m_fRetractionAtPause);
        }
        else
        {
          this.m_fRetractionAtPause = 0.0f;
          str = PrinterCompatibleString.Format("G0 Z{0} F{1}", (object) 8f, (object) num1);
        }
        pause_gcode.AddRange((IEnumerable<string>) new string[7]
        {
          "G4 S0",
          "M114",
          "G91",
          str,
          "G90",
          PrinterCompatibleString.Format("G0 X{0} Y{1} F{2}", (object) cornerPositionBoxTop.x, (object) cornerPositionBoxTop.y, (object) num2),
          "M104 S0"
        });
        this.m_oParentFirmwareController.OnGotUpdatedPosition += new ScriptCallback(this.OnReceivedUpdatedPosition);
        this.m_bUpdatedDataReceivedAfterPause = false;
      }
      return flag;
    }

    public JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      resume_gcode = (List<string>) null;
      AbstractJob jobImplementation = this.MyJobImplementation;
      if (jobImplementation == null)
        return JobController.Result.FAILED_Create;
      List<string> resume_gcode1;
      JobController.Result result = jobImplementation.Resume(out resume_gcode1, spool);
      if (result == JobController.Result.Success)
      {
        int num1 = !((FilamentSpool) null != spool) || spool.filament_type == FilamentSpool.TypeEnum.NoFilament ? (int) byte.MaxValue : FilamentProfile.CreateFilamentProfile(spool, (PrinterProfile) this.m_oParentFirmwareController.MyPrinterProfile).preprocessor.initialPrint.StartingFanValue;
        float num2 = 90f;
        resume_gcode = new List<string>()
        {
          PrinterCompatibleString.Format("M106 S{0}", (object) num1),
          PrinterCompatibleString.Format("M109 S{0}", (object) spool.filament_temperature)
        };
        if (this.m_bUpdatedDataReceivedAfterPause)
        {
          resume_gcode.Add("G90");
          resume_gcode.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2}", (object) this.m_v3DeExtruderLocationAtPause.pos.x, (object) this.m_v3DeExtruderLocationAtPause.pos.y, (object) num2));
          resume_gcode.Add(PrinterCompatibleString.Format("G0 Z{0} F{1}", (object) this.m_v3DeExtruderLocationAtPause.pos.z, (object) num2));
          resume_gcode.Add(PrinterCompatibleString.Format("G92 E{0}", (object) (float) ((double) this.m_v3DeExtruderLocationAtPause.e - (double) this.m_fRetractionAtPause)));
        }
        if (resume_gcode1 != null && resume_gcode1.Count > 0)
          resume_gcode.AddRange((IEnumerable<string>) resume_gcode1);
        this.job_timer.Start();
      }
      return result;
    }

    public bool CanPauseImmediately
    {
      get
      {
        AbstractJob jobImplementation = this.MyJobImplementation;
        if (jobImplementation == null)
          return false;
        return jobImplementation.CanPauseImmediately;
      }
    }

    public bool HasWarnings
    {
      get
      {
        if (this.m_olWarnings != null)
          return this.m_olWarnings.Count > 0;
        return false;
      }
    }

    public MessageType GetNextWarning()
    {
      if (this.m_olWarnings != null && this.m_olWarnings.Count > 0)
        return this.m_olWarnings[0];
      return MessageType.PrinterError;
    }

    public void ClearCurrentWarning()
    {
      if (this.m_olWarnings == null || this.m_olWarnings.Count <= 0)
        return;
      this.m_olWarnings.RemoveAt(0);
    }

    public GCode GetNextGCode()
    {
      return this.MyJobImplementation?.GetNextCommand();
    }

    public void Update()
    {
      AbstractJob jobImplementation = this.MyJobImplementation;
      PrinterInfo currentPrinterInfo = this.m_oParentFirmwareController.CurrentPrinterInfo;
      if (jobImplementation == null)
        return;
      jobImplementation.Update(currentPrinterInfo);
      if (!jobImplementation.Done || !this.SwitchToFirmwarePrintWhenDone)
        return;
      this.m_lsAdditionalTimeRemaining = 0.0f;
      JobParams jobParams = new JobParams(jobImplementation.Details.jobParams);
      jobParams.preview_image_file_name = jobImplementation.PreviewImageFileName;
      jobParams.jobMode = JobParams.Mode.FirmwarePrintingFromSDCard;
      this.Mode = JobParams.Mode.FirmwarePrintingFromSDCard;
      jobParams.gcodefile = this.m_sGcodeFile;
      AbstractJob abstractJob = (AbstractJob) new FirmwareSDPrintJob(jobParams, this.User, jobImplementation.MyPrinterProfile);
      abstractJob.Create(currentPrinterInfo);
      List<string> end_gcode;
      this.FinalizeEndOfJob(out end_gcode);
      if (end_gcode != null && end_gcode.Count > 0)
      {
        int num1 = (int) this.m_oParentFirmwareController.WriteManualCommands(end_gcode.ToArray());
      }
      this.IsSimultaneousPrint = false;
      this.IsSavingToSD = false;
      this.m_oJobImplementation.Value = abstractJob;
      List<string> start_gcode;
      int num2 = (int) this.Start(out start_gcode);
      if (start_gcode == null || start_gcode.Count <= 0)
        return;
      int num3 = (int) this.m_oParentFirmwareController.WriteManualCommands(start_gcode.ToArray());
    }

    public JobInfo Info
    {
      get
      {
        if (this.MyJobImplementation == null)
          return (JobInfo) null;
        JobInfo info = this.MyJobImplementation.GetInfo();
        if (this.IsSavingToSD)
          info.Status = !this.IsSimultaneousPrint ? JobStatus.SavingToSD : JobStatus.Buffering;
        float timeRemaining = info.TimeRemaining;
        if ((double) timeRemaining > 0.0)
          timeRemaining += this.m_lsAdditionalTimeRemaining;
        info.TimeRemaining = timeRemaining;
        return info;
      }
    }

    public JobParams.Mode Mode
    {
      get
      {
        return this.m_JobMode.Value;
      }
      private set
      {
        this.m_JobMode.Value = value;
      }
    }

    public JobOptions Options
    {
      get
      {
        return this.Info?.Params.options;
      }
    }

    public bool HasJob
    {
      get
      {
        return this.MyJobImplementation != null;
      }
    }

    public bool Printing
    {
      get
      {
        return this.printing.Value;
      }
      private set
      {
        this.printing.Value = value;
      }
    }

    public JobStatus Status
    {
      get
      {
        AbstractJob jobImplementation = this.MyJobImplementation;
        if (jobImplementation != null)
          return jobImplementation.Status;
        return JobStatus.Unitialized;
      }
    }

    public string JobName
    {
      get
      {
        return this.Info?.JobName;
      }
    }

    public string User
    {
      get
      {
        return this.Info?.User;
      }
    }

    public float PercentComplete
    {
      get
      {
        JobInfo info = this.Info;
        if (info != null)
          return info.PercentComplete;
        return 0.0f;
      }
    }

    public float GetElapsedReset
    {
      get
      {
        bool isRunning = this.job_timer.IsRunning;
        float num = (float) (this.job_timer.ElapsedMilliseconds / 1000L) / 600f;
        if (isRunning && (double) num < 1.0)
          return 0.0f;
        this.job_timer.Stop();
        this.job_timer.Reset();
        if (isRunning)
          this.job_timer.Start();
        return num;
      }
    }

    public JobDetails Details
    {
      get
      {
        if (this.MyJobImplementation == null)
          return (JobDetails) null;
        return this.MyJobImplementation.Details;
      }
    }

    public bool RetractionRequired
    {
      get
      {
        if (this.MyJobImplementation == null)
          return false;
        return this.MyJobImplementation.RetractionRequired;
      }
    }

    public bool Processed
    {
      get
      {
        return this.m_bProcessed.Value;
      }
      private set
      {
        this.m_bProcessed.Value = value;
      }
    }

    public bool Stopped
    {
      get
      {
        if (this.MyJobImplementation == null)
          return true;
        return this.MyJobImplementation.Stopped;
      }
    }

    public bool Aborted
    {
      get
      {
        if (this.MyJobImplementation == null)
          return true;
        return this.MyJobImplementation.Aborted;
      }
    }

    public bool Done
    {
      get
      {
        if (this.MyJobImplementation == null)
          return true;
        if (this.MyJobImplementation.Done)
          return !this.SwitchToFirmwarePrintWhenDone;
        return false;
      }
    }

    public bool IsSavingToSD
    {
      get
      {
        return this.m_bSavingToSD.Value;
      }
      private set
      {
        this.m_bSavingToSD.Value = value;
      }
    }

    public bool IsSimultaneousPrint
    {
      get
      {
        return this.m_bSimultaneousPrint.Value;
      }
      private set
      {
        this.m_bSimultaneousPrint.Value = value;
      }
    }

    public bool IsSavingToSDOnly
    {
      get
      {
        if (this.IsSavingToSD)
          return !this.IsSimultaneousPrint;
        return false;
      }
    }

    public double MinutesElapsed
    {
      get
      {
        if (this.MyJobImplementation != null)
          return this.MyJobImplementation.MinutesElapsed;
        return 0.0;
      }
    }

    private void SaveJobState()
    {
      AbstractJob jobImplementation = this.MyJobImplementation;
      PersistantJobData parameters = (PersistantJobData) null;
      if (jobImplementation != null)
        parameters = new PersistantJobData(jobImplementation.CurrentFileLineNumber, jobImplementation.Details.jobParams);
      this.m_oParentFirmwareController.SaveJobParamsToPersistantData(parameters);
    }

    private string GetFilenameForSDCard(string jobname, FilamentSpool filament)
    {
      string str = filament.filament_type == FilamentSpool.TypeEnum.NoFilament || filament.filament_type == FilamentSpool.TypeEnum.OtherOrUnknown ? "gcode" : filament.filament_type.ToString();
      return string.Format("{0}.{1}", (object) jobname.Replace(" ", "_"), (object) str);
    }

    private void OnReceivedUpdatedPosition(IPublicFirmwareController connection, PrinterInfo info)
    {
      this.m_v3DeExtruderLocationAtPause = info.extruder.position;
      this.m_bUpdatedDataReceivedAfterPause = true;
    }

    private void ClearAllWarnings()
    {
      if (this.m_olWarnings == null)
        return;
      this.m_olWarnings.Clear();
      this.m_olWarnings = (List<MessageType>) null;
    }

    private AbstractJob MyJobImplementation
    {
      get
      {
        return this.m_oJobImplementation.Value;
      }
    }

    private bool SwitchToFirmwarePrintWhenDone
    {
      get
      {
        return this.Mode == JobParams.Mode.SavingToSDCardAutoStartPrint;
      }
    }

    public enum Result
    {
      Success,
      FAILED_Create,
      FAILED_JobAlreadyStarted,
      FAILED_JobNotStarted,
      FAILED_OutOfBounds,
      FAILED_NotPaused,
      FAILED_NoFilament,
      FAILED_IncompatibleFilament,
    }
  }
}
