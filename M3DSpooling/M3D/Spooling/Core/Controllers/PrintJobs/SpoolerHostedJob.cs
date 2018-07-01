// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.SpoolerHostedJob
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal class SpoolerHostedJob : AbstractPreprocessedJob
  {
    private ThreadSafeVariable<ulong> currentline = new ThreadSafeVariable<ulong>(0UL);
    private ThreadSafeVariable<bool> endreadched = new ThreadSafeVariable<bool>(false);
    private const float ESTIMATED_GCODES_PER_SECOND = 50f;
    private IGCodeReader m_oGCodeReader;
    private float CurrentPrintJobExtrusion;
    private float ExtrusionAtPause;
    private long MaxLines;
    private bool aborted;
    private bool m_bSDOnly;
    private ulong m_ulFastForward;

    public SpoolerHostedJob(JobParams jobParams, string user, InternalPrinterProfile printerProfile, bool sdOnly, ulong ulFastForward)
      : base(jobParams, user, printerProfile)
    {
      this.aborted = false;
      this.m_bSDOnly = sdOnly;
      this.m_ulFastForward = ulFastForward;
      if (this.m_ulFastForward == 0UL)
        return;
      this.Details.jobParams.options.dont_use_preprocessors = true;
      this.Details.jobParams.options.dont_use_copy_to_spooler = true;
    }

    public override void Update(PrinterInfo printerInfo)
    {
    }

    public override bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      pause_gcode = (List<string>) null;
      this.Status = JobStatus.Paused;
      this.ExtrusionAtPause = this.CurrentPrintJobExtrusion;
      this.CurrentPrintJobExtrusion = 0.0f;
      return true;
    }

    public override JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      resume_gcode = (List<string>) null;
      if (spool == (FilamentSpool) null)
        spool = this.InitialSpoolUsed;
      if (spool == (FilamentSpool) null)
        return JobController.Result.FAILED_NoFilament;
      if (this.Status != JobStatus.Paused)
        return JobController.Result.FAILED_NotPaused;
      if (!this.m_bSDOnly)
      {
        this.CurrentPrintJobExtrusion = this.ExtrusionAtPause;
        this.Status = JobStatus.Heating;
      }
      return JobController.Result.Success;
    }

    public override bool Start(out List<string> start_gcode)
    {
      float fEStartingLocation;
      this.m_oGCodeReader = (IGCodeReader) new GCodeBufferedFileReader(this.GCodeFilename, this.m_ulFastForward, out fEStartingLocation);
      start_gcode = (List<string>) null;
      if (!this.m_oGCodeReader.IsOpen)
        return false;
      try
      {
        this.MaxLines = this.m_oGCodeReader.MaxLines;
        if (this.m_bSDOnly)
          this.Details.jobParams.estimatedTime = (float) this.MaxLines / 50f;
      }
      catch (NotImplementedException ex)
      {
        this.MaxLines = 0L;
      }
      this.JobBeginTimer.Stop();
      this.JobBeginTimer.Reset();
      this.Status = JobStatus.Printing;
      this.CurrentLineNumber = this.m_ulFastForward;
      start_gcode = new List<string>()
      {
        string.Format("N{0} M110", (object) (this.CurrentLineNumber % 65536UL))
      };
      if (this.m_ulFastForward != 0UL)
        start_gcode.Insert(0, string.Format("G92 E{0}", (object) fEStartingLocation));
      return true;
    }

    public override void Stop()
    {
      this.JobBeginTimer.Stop();
      if (this.m_oGCodeReader != null)
        this.m_oGCodeReader.Close();
      this.aborted = true;
    }

    public override GCode GetNextCommand()
    {
      this.OnGetNextCommand();
      if (this.Status != JobStatus.Cancelled && this.m_oGCodeReader != null && this.m_oGCodeReader.IsOpen)
      {
        this.Status = JobStatus.Printing;
        GCode nextLine = this.m_oGCodeReader.GetNextLine(true);
        if (nextLine != null)
        {
          if (nextLine.hasM && nextLine.M == (ushort) 109 || nextLine.M == (ushort) 116 || nextLine.hasG && nextLine.G == (ushort) 4)
            this.Status = JobStatus.Heating;
          if (nextLine.hasG && (nextLine.G == (ushort) 0 || nextLine.G == (ushort) 1) && nextLine.hasE)
            this.CurrentPrintJobExtrusion = nextLine.E;
          ++this.CurrentLineNumber;
          nextLine.N = (int) (this.CurrentLineNumber % 65536UL);
          return nextLine;
        }
        this.endreadched.Value = true;
      }
      return (GCode) null;
    }

    public override float PercentComplete
    {
      get
      {
        if (this.MaxLines > 0L)
          return (float) this.CurrentLineNumber / (float) this.MaxLines;
        return 0.0f;
      }
    }

    public override bool CanPauseImmediately
    {
      get
      {
        return false;
      }
    }

    public ulong CurrentLineNumber
    {
      get
      {
        return this.currentline.Value;
      }
      private set
      {
        this.currentline.Value = value;
      }
    }

    public override bool Aborted
    {
      get
      {
        return this.aborted;
      }
    }

    public override bool Done
    {
      get
      {
        if (this.Status != JobStatus.Queued)
          return this.endreadched.Value;
        return false;
      }
    }

    public JobParams Params
    {
      get
      {
        return new JobParams(this.Details.jobParams);
      }
    }

    public override bool RetractionRequired
    {
      get
      {
        if ((double) this.CurrentPrintJobExtrusion > 0.0)
          return !this.m_bSDOnly;
        return false;
      }
    }

    public override ulong CurrentFileLineNumber
    {
      get
      {
        return this.CurrentLineNumber;
      }
    }
  }
}
