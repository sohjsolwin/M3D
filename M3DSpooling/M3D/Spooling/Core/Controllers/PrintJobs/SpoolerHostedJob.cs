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
      aborted = false;
      m_bSDOnly = sdOnly;
      m_ulFastForward = ulFastForward;
      if (m_ulFastForward == 0UL)
      {
        return;
      }

      Details.jobParams.options.dont_use_preprocessors = true;
      Details.jobParams.options.dont_use_copy_to_spooler = true;
    }

    public override void Update(PrinterInfo printerInfo)
    {
    }

    public override bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      pause_gcode = (List<string>) null;
      Status = JobStatus.Paused;
      ExtrusionAtPause = CurrentPrintJobExtrusion;
      CurrentPrintJobExtrusion = 0.0f;
      return true;
    }

    public override JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      resume_gcode = (List<string>) null;
      if (spool == (FilamentSpool) null)
      {
        spool = InitialSpoolUsed;
      }

      if (spool == (FilamentSpool) null)
      {
        return JobController.Result.FAILED_NoFilament;
      }

      if (Status != JobStatus.Paused)
      {
        return JobController.Result.FAILED_NotPaused;
      }

      if (!m_bSDOnly)
      {
        CurrentPrintJobExtrusion = ExtrusionAtPause;
        Status = JobStatus.Heating;
      }
      return JobController.Result.Success;
    }

    public override bool Start(out List<string> start_gcode)
    {
      m_oGCodeReader = (IGCodeReader)new GCodeBufferedFileReader(GCodeFilename, m_ulFastForward, out var fEStartingLocation);
      start_gcode = (List<string>) null;
      if (!m_oGCodeReader.IsOpen)
      {
        return false;
      }

      try
      {
        MaxLines = m_oGCodeReader.MaxLines;
        if (m_bSDOnly)
        {
          Details.jobParams.estimatedTime = (float)MaxLines / 50f;
        }
      }
      catch (NotImplementedException ex)
      {
        MaxLines = 0L;
      }
      JobBeginTimer.Stop();
      JobBeginTimer.Reset();
      Status = JobStatus.Printing;
      CurrentLineNumber = m_ulFastForward;
      start_gcode = new List<string>()
      {
        string.Format("N{0} M110", (object) (CurrentLineNumber % 65536UL))
      };
      if (m_ulFastForward != 0UL)
      {
        start_gcode.Insert(0, string.Format("G92 E{0}", (object) fEStartingLocation));
      }

      return true;
    }

    public override void Stop()
    {
      JobBeginTimer.Stop();
      if (m_oGCodeReader != null)
      {
        m_oGCodeReader.Close();
      }

      aborted = true;
    }

    public override GCode GetNextCommand()
    {
      OnGetNextCommand();
      if (Status != JobStatus.Cancelled && m_oGCodeReader != null && m_oGCodeReader.IsOpen)
      {
        Status = JobStatus.Printing;
        GCode nextLine = m_oGCodeReader.GetNextLine(true);
        if (nextLine != null)
        {
          if (nextLine.hasM && nextLine.M == (ushort) 109 || nextLine.M == (ushort) 116 || nextLine.hasG && nextLine.G == (ushort) 4)
          {
            Status = JobStatus.Heating;
          }

          if (nextLine.hasG && (nextLine.G == (ushort) 0 || nextLine.G == (ushort) 1) && nextLine.hasE)
          {
            CurrentPrintJobExtrusion = nextLine.E;
          }

          ++CurrentLineNumber;
          nextLine.N = (int) (CurrentLineNumber % 65536UL);
          return nextLine;
        }
        endreadched.Value = true;
      }
      return (GCode) null;
    }

    public override float PercentComplete
    {
      get
      {
        if (MaxLines > 0L)
        {
          return (float)CurrentLineNumber / (float)MaxLines;
        }

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
        return currentline.Value;
      }
      private set
      {
        currentline.Value = value;
      }
    }

    public override bool Aborted
    {
      get
      {
        return aborted;
      }
    }

    public override bool Done
    {
      get
      {
        if (Status != JobStatus.Queued)
        {
          return endreadched.Value;
        }

        return false;
      }
    }

    public JobParams Params
    {
      get
      {
        return new JobParams(Details.jobParams);
      }
    }

    public override bool RetractionRequired
    {
      get
      {
        if ((double)CurrentPrintJobExtrusion > 0.0)
        {
          return !m_bSDOnly;
        }

        return false;
      }
    }

    public override ulong CurrentFileLineNumber
    {
      get
      {
        return CurrentLineNumber;
      }
    }
  }
}
