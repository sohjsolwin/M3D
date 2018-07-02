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
      m_oParentFirmwareController = ParentFirmwareController;
      job_timer = new Stopwatch();
    }

    public CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo)
    {
      return InitPrintJob(new JobParams(jobParams), "", ulLineToSkipTo) == JobController.Result.Success ? CommandResult.Success : CommandResult.Failed_Exception;
    }

    public void ConnectToRunningSDPrint(string user)
    {
      PrinterInfo currentPrinterInfo = m_oParentFirmwareController.CurrentPrinterInfo;
      JobParams jobParams = currentPrinterInfo.persistantData.SavedJobInformation == null ? new JobParams((string) null, "Print From SD", (string) null, FilamentSpool.TypeEnum.OtherOrUnknown, 0.0f, 0.0f) : currentPrinterInfo.persistantData.SavedJobInformation.Params;
      ClearAllWarnings();
      m_sGcodeFile = "";
      Mode = JobParams.Mode.FirmwarePrintingFromSDCard;
      var firmwareSdPrintJob = new FirmwareSDPrintJob(jobParams, user, m_oParentFirmwareController.MyPrinterProfile);
      m_oJobImplementation.Value = (AbstractJob) firmwareSdPrintJob;
      firmwareSdPrintJob.ConnectToRunningSDPrint();
      Processed = true;
      Printing = true;
      SaveJobState();
    }

    public JobController.Result InitPrintJob(JobParams jobParams, string user, ulong ulFastForward = 0)
    {
      if (MyJobImplementation != null)
      {
        return JobController.Result.FAILED_JobAlreadyStarted;
      }

      Processed = false;
      Printing = false;
      IsSavingToSD = false;
      ClearAllWarnings();
      m_sGcodeFile = jobParams.gcodefile;
      Mode = jobParams.jobMode;
      PrinterInfo currentPrinterInfo = m_oParentFirmwareController.CurrentPrinterInfo;
      AbstractJob abstractJob;
      if (Mode == JobParams.Mode.SaveToBinaryGCodeFile)
      {
        abstractJob = (AbstractJob) new SaveGCodeToFileJob(jobParams, user, m_oParentFirmwareController.MyPrinterProfile);
      }
      else if (Mode != JobParams.Mode.FirmwarePrintingFromSDCard)
      {
        if (Mode == JobParams.Mode.SavingToSDCard || Mode == JobParams.Mode.SavingToSDCardAutoStartPrint)
        {
          m_sGcodeFile = GetFilenameForSDCard(jobParams.jobname, currentPrinterInfo.filament_info);
        }

        IsSimultaneousPrint = false;
        IsSavingToSD = Mode != JobParams.Mode.DirectPrinting;
        abstractJob = (AbstractJob) new SpoolerHostedJob(jobParams, user, m_oParentFirmwareController.MyPrinterProfile, IsSavingToSD && !IsSimultaneousPrint, ulFastForward);
      }
      else
      {
        if (Mode != JobParams.Mode.FirmwarePrintingFromSDCard)
        {
          throw new NotImplementedException("Software does not support printing from the SD card.");
        }

        abstractJob = (AbstractJob) new FirmwareSDPrintJob(jobParams, user, m_oParentFirmwareController.MyPrinterProfile);
      }
      m_oJobImplementation.Value = abstractJob;
      JobCreateResult jobCreateResult = abstractJob.Create(currentPrinterInfo);
      if (jobCreateResult.Result == ProcessReturn.SUCCESS)
      {
        m_olWarnings = jobCreateResult.Warnings;
        Processed = true;
        return JobController.Result.Success;
      }
      m_oJobImplementation.Value = (AbstractJob) null;
      if (jobCreateResult.Result == ProcessReturn.FAILURE_OUT_OF_BOUNDS)
      {
        return JobController.Result.FAILED_OutOfBounds;
      }

      return jobCreateResult.Result == ProcessReturn.FAILURE_FILAMENT_MISMATCH ? JobController.Result.FAILED_IncompatibleFilament : JobController.Result.FAILED_Create;
    }

    public JobController.Result Start(out List<string> start_gcode)
    {
      start_gcode = (List<string>) null;
      if (MyJobImplementation == null)
      {
        return JobController.Result.FAILED_Create;
      }

      ClearAllWarnings();
      if (m_JobMode.Value == JobParams.Mode.SavingToSDCardAutoStartPrint)
      {
        m_lsAdditionalTimeRemaining = MyJobImplementation.EstimatedPrintTime;
      }

      if (!MyJobImplementation.Start(out List<string> start_gcode1))
      {
        return JobController.Result.FAILED_JobNotStarted;
      }

      job_timer.Restart();
      if (IsSavingToSD)
      {
        if (start_gcode == null)
        {
          start_gcode = new List<string>();
        }

        start_gcode = new List<string>()
        {
          string.Format("M28 {0}", (object) m_sGcodeFile)
        };
      }
      else if (MyJobImplementation.Details.jobParams.jobMode != JobParams.Mode.SaveToBinaryGCodeFile && MyJobImplementation.Details.jobParams.options.calibrate_before_print)
      {
        var calibrateBeforePrintZ = MyJobImplementation.Details.jobParams.options.calibrate_before_print_z;
        start_gcode = PrinterCalibration.CreateCalibrationGCode(PrinterCalibration.Type.G30, calibrateBeforePrintZ, 5f, m_oParentFirmwareController.MyPrinterProfile.OptionsConstants);
      }
      if (start_gcode == null)
      {
        start_gcode = start_gcode1;
      }
      else if (start_gcode1 != null)
      {
        start_gcode.AddRange((IEnumerable<string>) start_gcode1);
      }

      Printing = true;
      SaveJobState();
      return JobController.Result.Success;
    }

    public JobController.Result ForceShutdownNow()
    {
      AbstractJob jobImplementation = MyJobImplementation;
      if (MyJobImplementation == null)
      {
        return JobController.Result.FAILED_JobNotStarted;
      }

      SaveJobState();
      MyJobImplementation.Stop();
      job_timer.Stop();
      m_oJobImplementation.Value = (AbstractJob) null;
      Printing = false;
      return JobController.Result.Success;
    }

    public JobController.Result StopJob(out List<string> end_gcode)
    {
      AbstractJob jobImplementation = MyJobImplementation;
      end_gcode = (List<string>) null;
      if (MyJobImplementation == null)
      {
        return JobController.Result.FAILED_JobNotStarted;
      }

      FinalizeEndOfJob(out end_gcode);
      job_timer.Stop();
      Printing = false;
      return JobController.Result.Success;
    }

    public void FinalizeEndOfJob(out List<string> end_gcode)
    {
      end_gcode = (List<string>) null;
      if (IsSavingToSD)
      {
        end_gcode = new List<string>();
        end_gcode.Add("M29");
        if (!m_oJobImplementation.Value.Done)
        {
          end_gcode.Add(string.Format("M30 {0}", (object)m_sGcodeFile));
        }

        MyJobImplementation.Stop();
      }
      m_oJobImplementation.Value = (AbstractJob) null;
      SaveJobState();
    }

    public bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      pause_gcode = (List<string>) null;
      AbstractJob jobImplementation = MyJobImplementation;
      if (jobImplementation == null || jobImplementation.Status == JobStatus.Paused || IsSavingToSD)
      {
        return false;
      }

      job_timer.Stop();
      var flag = jobImplementation.Pause(out pause_gcode, spool);
      if (flag)
      {
        if (pause_gcode == null)
        {
          pause_gcode = new List<string>();
        }

        var num1 = 90f;
        var num2 = 1800f;
        Vector2D cornerPositionBoxTop = m_oParentFirmwareController.MyPrinterProfile.PrinterSizeConstants.BackCornerPositionBoxTop;
        string str;
        if ((double)m_oParentFirmwareController.CurrentPrinterInfo.extruder.Temperature > 0.0)
        {
          m_fRetractionAtPause = !((FilamentSpool) null != spool) || spool.filament_type == FilamentSpool.TypeEnum.NoFilament ? 10f : (float) FilamentProfile.CreateFilamentProfile(spool, (PrinterProfile)m_oParentFirmwareController.MyPrinterProfile).preprocessor.initialPrint.PrimeAmount;
          str = PrinterCompatibleString.Format("G0 Z{0} F{1} E-{2}", (object) 8f, (object) num1, (object)m_fRetractionAtPause);
        }
        else
        {
          m_fRetractionAtPause = 0.0f;
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
        m_oParentFirmwareController.OnGotUpdatedPosition += new ScriptCallback(OnReceivedUpdatedPosition);
        m_bUpdatedDataReceivedAfterPause = false;
      }
      return flag;
    }

    public JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      resume_gcode = (List<string>) null;
      AbstractJob jobImplementation = MyJobImplementation;
      if (jobImplementation == null)
      {
        return JobController.Result.FAILED_Create;
      }

      JobController.Result result = jobImplementation.Resume(out List<string> resume_gcode1, spool);
      if (result == JobController.Result.Success)
      {
        var num1 = !((FilamentSpool) null != spool) || spool.filament_type == FilamentSpool.TypeEnum.NoFilament ? (int) byte.MaxValue : FilamentProfile.CreateFilamentProfile(spool, (PrinterProfile)m_oParentFirmwareController.MyPrinterProfile).preprocessor.initialPrint.StartingFanValue;
        var num2 = 90f;
        resume_gcode = new List<string>()
        {
          PrinterCompatibleString.Format("M106 S{0}", (object) num1),
          PrinterCompatibleString.Format("M109 S{0}", (object) spool.filament_temperature)
        };
        if (m_bUpdatedDataReceivedAfterPause)
        {
          resume_gcode.Add("G90");
          resume_gcode.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2}", (object)m_v3DeExtruderLocationAtPause.pos.x, (object)m_v3DeExtruderLocationAtPause.pos.y, (object) num2));
          resume_gcode.Add(PrinterCompatibleString.Format("G0 Z{0} F{1}", (object)m_v3DeExtruderLocationAtPause.pos.z, (object) num2));
          resume_gcode.Add(PrinterCompatibleString.Format("G92 E{0}", (object) (float) ((double)m_v3DeExtruderLocationAtPause.e - (double)m_fRetractionAtPause)));
        }
        if (resume_gcode1 != null && resume_gcode1.Count > 0)
        {
          resume_gcode.AddRange((IEnumerable<string>) resume_gcode1);
        }

        job_timer.Start();
      }
      return result;
    }

    public bool CanPauseImmediately
    {
      get
      {
        AbstractJob jobImplementation = MyJobImplementation;
        if (jobImplementation == null)
        {
          return false;
        }

        return jobImplementation.CanPauseImmediately;
      }
    }

    public bool HasWarnings
    {
      get
      {
        if (m_olWarnings != null)
        {
          return m_olWarnings.Count > 0;
        }

        return false;
      }
    }

    public MessageType GetNextWarning()
    {
      if (m_olWarnings != null && m_olWarnings.Count > 0)
      {
        return m_olWarnings[0];
      }

      return MessageType.PrinterError;
    }

    public void ClearCurrentWarning()
    {
      if (m_olWarnings == null || m_olWarnings.Count <= 0)
      {
        return;
      }

      m_olWarnings.RemoveAt(0);
    }

    public GCode GetNextGCode()
    {
      return MyJobImplementation?.GetNextCommand();
    }

    public void Update()
    {
      AbstractJob jobImplementation = MyJobImplementation;
      PrinterInfo currentPrinterInfo = m_oParentFirmwareController.CurrentPrinterInfo;
      if (jobImplementation == null)
      {
        return;
      }

      jobImplementation.Update(currentPrinterInfo);
      if (!jobImplementation.Done || !SwitchToFirmwarePrintWhenDone)
      {
        return;
      }

      m_lsAdditionalTimeRemaining = 0.0f;
      var jobParams = new JobParams(jobImplementation.Details.jobParams)
      {
        preview_image_file_name = jobImplementation.PreviewImageFileName,
        jobMode = JobParams.Mode.FirmwarePrintingFromSDCard
      };
      Mode = JobParams.Mode.FirmwarePrintingFromSDCard;
      jobParams.gcodefile = m_sGcodeFile;
      var abstractJob = (AbstractJob) new FirmwareSDPrintJob(jobParams, User, jobImplementation.MyPrinterProfile);
      abstractJob.Create(currentPrinterInfo);
      FinalizeEndOfJob(out List<string> end_gcode);
      if (end_gcode != null && end_gcode.Count > 0)
      {
        var num1 = (int)m_oParentFirmwareController.WriteManualCommands(end_gcode.ToArray());
      }
      IsSimultaneousPrint = false;
      IsSavingToSD = false;
      m_oJobImplementation.Value = abstractJob;
      var num2 = (int)Start(out List<string> start_gcode);
      if (start_gcode == null || start_gcode.Count <= 0)
      {
        return;
      }

      var num3 = (int)m_oParentFirmwareController.WriteManualCommands(start_gcode.ToArray());
    }

    public JobInfo Info
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return (JobInfo) null;
        }

        JobInfo info = MyJobImplementation.GetInfo();
        if (IsSavingToSD)
        {
          info.Status = !IsSimultaneousPrint ? JobStatus.SavingToSD : JobStatus.Buffering;
        }

        var timeRemaining = info.TimeRemaining;
        if ((double) timeRemaining > 0.0)
        {
          timeRemaining += m_lsAdditionalTimeRemaining;
        }

        info.TimeRemaining = timeRemaining;
        return info;
      }
    }

    public JobParams.Mode Mode
    {
      get
      {
        return m_JobMode.Value;
      }
      private set
      {
        m_JobMode.Value = value;
      }
    }

    public JobOptions Options
    {
      get
      {
        return Info?.Params.options;
      }
    }

    public bool HasJob
    {
      get
      {
        return MyJobImplementation != null;
      }
    }

    public bool Printing
    {
      get
      {
        return printing.Value;
      }
      private set
      {
        printing.Value = value;
      }
    }

    public JobStatus Status
    {
      get
      {
        AbstractJob jobImplementation = MyJobImplementation;
        if (jobImplementation != null)
        {
          return jobImplementation.Status;
        }

        return JobStatus.Unitialized;
      }
    }

    public string JobName
    {
      get
      {
        return Info?.JobName;
      }
    }

    public string User
    {
      get
      {
        return Info?.User;
      }
    }

    public float PercentComplete
    {
      get
      {
        JobInfo info = Info;
        if (info != null)
        {
          return info.PercentComplete;
        }

        return 0.0f;
      }
    }

    public float GetElapsedReset
    {
      get
      {
        var isRunning = job_timer.IsRunning;
        var num = (float) (job_timer.ElapsedMilliseconds / 1000L) / 600f;
        if (isRunning && (double) num < 1.0)
        {
          return 0.0f;
        }

        job_timer.Stop();
        job_timer.Reset();
        if (isRunning)
        {
          job_timer.Start();
        }

        return num;
      }
    }

    public JobDetails Details
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return (JobDetails) null;
        }

        return MyJobImplementation.Details;
      }
    }

    public bool RetractionRequired
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return false;
        }

        return MyJobImplementation.RetractionRequired;
      }
    }

    public bool Processed
    {
      get
      {
        return m_bProcessed.Value;
      }
      private set
      {
        m_bProcessed.Value = value;
      }
    }

    public bool Stopped
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return true;
        }

        return MyJobImplementation.Stopped;
      }
    }

    public bool Aborted
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return true;
        }

        return MyJobImplementation.Aborted;
      }
    }

    public bool Done
    {
      get
      {
        if (MyJobImplementation == null)
        {
          return true;
        }

        if (MyJobImplementation.Done)
        {
          return !SwitchToFirmwarePrintWhenDone;
        }

        return false;
      }
    }

    public bool IsSavingToSD
    {
      get
      {
        return m_bSavingToSD.Value;
      }
      private set
      {
        m_bSavingToSD.Value = value;
      }
    }

    public bool IsSimultaneousPrint
    {
      get
      {
        return m_bSimultaneousPrint.Value;
      }
      private set
      {
        m_bSimultaneousPrint.Value = value;
      }
    }

    public bool IsSavingToSDOnly
    {
      get
      {
        if (IsSavingToSD)
        {
          return !IsSimultaneousPrint;
        }

        return false;
      }
    }

    public double MinutesElapsed
    {
      get
      {
        if (MyJobImplementation != null)
        {
          return MyJobImplementation.MinutesElapsed;
        }

        return 0.0;
      }
    }

    private void SaveJobState()
    {
      AbstractJob jobImplementation = MyJobImplementation;
      var parameters = (PersistantJobData) null;
      if (jobImplementation != null)
      {
        parameters = new PersistantJobData(jobImplementation.CurrentFileLineNumber, jobImplementation.Details.jobParams);
      }

      m_oParentFirmwareController.SaveJobParamsToPersistantData(parameters);
    }

    private string GetFilenameForSDCard(string jobname, FilamentSpool filament)
    {
      var str = filament.filament_type == FilamentSpool.TypeEnum.NoFilament || filament.filament_type == FilamentSpool.TypeEnum.OtherOrUnknown ? "gcode" : filament.filament_type.ToString();
      return string.Format("{0}.{1}", (object) jobname.Replace(" ", "_"), (object) str);
    }

    private void OnReceivedUpdatedPosition(IPublicFirmwareController connection, PrinterInfo info)
    {
      m_v3DeExtruderLocationAtPause = info.extruder.position;
      m_bUpdatedDataReceivedAfterPause = true;
    }

    private void ClearAllWarnings()
    {
      if (m_olWarnings == null)
      {
        return;
      }

      m_olWarnings.Clear();
      m_olWarnings = (List<MessageType>) null;
    }

    private AbstractJob MyJobImplementation
    {
      get
      {
        return m_oJobImplementation.Value;
      }
    }

    private bool SwitchToFirmwarePrintWhenDone
    {
      get
      {
        return Mode == JobParams.Mode.SavingToSDCardAutoStartPrint;
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
