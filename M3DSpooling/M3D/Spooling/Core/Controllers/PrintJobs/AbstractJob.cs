// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.AbstractJob
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal abstract class AbstractJob
  {
    private ThreadSafeVariable<JobStatus> status = new ThreadSafeVariable<JobStatus>();
    private JobDetails jobDetails;
    private Stopwatch job_begin_timer;

    public AbstractJob(JobParams jobParams, string user, InternalPrinterProfile printerProfile)
    {
      this.MyPrinterProfile = printerProfile;
      this.Status = JobStatus.Queued;
      this.PreviewImageFileName = "null";
      this.jobDetails = new JobDetails();
      this.Details.jobParams = jobParams;
      this.Details.bounds = new BoundingBox(float.MaxValue, float.MaxValue, float.MaxValue, float.MinValue, float.MinValue, float.MinValue);
      this.Details.ideal_temperature = 0;
      this.User = user;
      this.job_begin_timer = new Stopwatch();
      this.InitialSpoolUsed = new FilamentSpool();
      this.InitialSpoolUsed.filament_temperature = jobParams.filament_temperature;
      this.InitialSpoolUsed.filament_type = jobParams.filament_type;
    }

    public abstract JobCreateResult Create(PrinterInfo printerInfo);

    public abstract void Update(PrinterInfo printerInfo);

    public abstract bool Start(out List<string> start_gcode);

    public abstract bool Pause(out List<string> pause_gcode, FilamentSpool spool);

    public abstract JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool);

    public abstract void Stop();

    public abstract GCode GetNextCommand();

    public abstract bool Aborted { get; }

    public abstract bool Done { get; }

    public abstract bool RetractionRequired { get; }

    public abstract float PercentComplete { get; }

    public abstract bool CanPauseImmediately { get; }

    public abstract ulong CurrentFileLineNumber { get; }

    public JobInfo GetInfo()
    {
      return new JobInfo(this.Details.jobParams.jobname, this.User, this.Status, this.PreviewImageFileName, this.PercentComplete, this.TimeRemaining, this.Details.jobParams);
    }

    public string JobName
    {
      get
      {
        return this.Details.jobParams.jobname;
      }
    }

    public string User { get; private set; }

    public string PreviewImageFileName { get; protected set; }

    public JobStatus Status
    {
      get
      {
        return this.status.Value;
      }
      protected set
      {
        this.status.Value = value;
      }
    }

    public JobDetails Details
    {
      get
      {
        return this.jobDetails;
      }
    }

    public bool Stopped
    {
      get
      {
        if (!this.Aborted)
          return this.Done;
        return true;
      }
    }

    public float EstimatedPrintTime
    {
      get
      {
        return this.Details.jobParams.estimatedTime;
      }
    }

    public bool AutoStarting
    {
      get
      {
        return this.Details.jobParams.options.autostart_ignorewarnings;
      }
    }

    public float TimeRemaining
    {
      get
      {
        float num = (float) ((double) this.JobBeginTimer.ElapsedMilliseconds / 1000.0 / 60.0);
        if ((double) num > 5.0)
        {
          if ((double) this.Details.jobParams.estimatedTime > 0.0)
            return this.Details.jobParams.estimatedTime - this.Details.jobParams.estimatedTime * this.PercentComplete;
          if ((double) this.PercentComplete > 0.0)
            return (float) TimeSpan.FromMinutes((double) num / (double) this.PercentComplete - (double) num).TotalSeconds;
        }
        return 0.0f;
      }
    }

    public double MinutesElapsed
    {
      get
      {
        return this.JobBeginTimer.Elapsed.TotalMinutes;
      }
    }

    public InternalPrinterProfile MyPrinterProfile { get; private set; }

    protected void OnGetNextCommand()
    {
      if (this.JobBeginTimer.IsRunning)
        return;
      this.JobBeginTimer.Start();
    }

    protected Stopwatch JobBeginTimer
    {
      get
      {
        return this.job_begin_timer;
      }
    }

    protected FilamentSpool InitialSpoolUsed { get; private set; }

    public enum JobMode
    {
      DirectPrinting,
      PrintingToSDCard,
      PrintingToSDCardAutoStartPrint,
      FirmwarePrintingFromSDCard,
      SimultaneousSDSaveAndPrint,
    }
  }
}
