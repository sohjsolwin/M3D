// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.FirmwareSDPrintJob
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal class FirmwareSDPrintJob : AbstractJob
  {
    private static GCode M27 = new GCode(nameof (M27));
    private ThreadSafeVariable<bool> m_bDone = new ThreadSafeVariable<bool>(false);
    private Stopwatch m_oswRefreshTimer;
    private string m_sGCodeFileOnSDCard;
    private long m_lSDFilePos;
    private long m_lSDFileSize;

    public FirmwareSDPrintJob(JobParams jobParams, string user, InternalPrinterProfile printerProfile)
      : base(jobParams, user, printerProfile)
    {
      this.m_oswRefreshTimer = new Stopwatch();
      this.PreviewImageFileName = jobParams.preview_image_file_name;
    }

    public override JobCreateResult Create(PrinterInfo printerInfo)
    {
      try
      {
        this.m_sGCodeFileOnSDCard = this.Details.jobParams.gcodefile;
      }
      catch (Exception ex)
      {
        return new JobCreateResult((AbstractJob) this, ProcessReturn.FAILURE, (List<MessageType>) null);
      }
      FilamentSpool.TypeEnum filamentTypeFromName = FirmwareSDPrintJob.GetFilamentTypeFromName(this.Details.jobParams.gcodefile);
      switch (filamentTypeFromName)
      {
        case FilamentSpool.TypeEnum.NoFilament:
        case FilamentSpool.TypeEnum.OtherOrUnknown:
          return new JobCreateResult((AbstractJob) this, ProcessReturn.SUCCESS, (List<MessageType>) null);
        default:
          if (FilamentProfile.CreateFilamentProfile(printerInfo.filament_info, (PrinterProfile) this.MyPrinterProfile).Type != filamentTypeFromName)
            return new JobCreateResult((AbstractJob) null, ProcessReturn.FAILURE_FILAMENT_MISMATCH, (List<MessageType>) null);
          goto case FilamentSpool.TypeEnum.NoFilament;
      }
    }

    public override void Update(PrinterInfo printerInfo)
    {
      if (this.m_lSDFileSize > 0L || this.JobBeginTimer.ElapsedMilliseconds > 5000L && printerInfo.IsIdle)
        this.m_bDone.Value = !printerInfo.accessories.SDCardStatus.IsPrintingFromSD;
      this.m_lSDFilePos = printerInfo.accessories.SDCardStatus.SDFilePosition;
      this.m_lSDFileSize = printerInfo.accessories.SDCardStatus.SDFileLength;
    }

    public override bool Start(out List<string> start_gcode)
    {
      start_gcode = new List<string>()
      {
        string.Format("M32 {0}", (object) this.m_sGCodeFileOnSDCard)
      };
      this.ConnectToRunningSDPrint();
      return true;
    }

    public void ConnectToRunningSDPrint()
    {
      this.m_oswRefreshTimer.Start();
      this.JobBeginTimer.Stop();
      this.JobBeginTimer.Reset();
      this.Status = JobStatus.Printing;
      this.m_lSDFilePos = 0L;
      this.m_lSDFileSize = 0L;
    }

    public override bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      this.Status = JobStatus.Paused;
      pause_gcode = new List<string>() { "M25" };
      return true;
    }

    public override JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      this.Status = JobStatus.Printing;
      resume_gcode = new List<string>() { "M24" };
      return JobController.Result.Success;
    }

    public override void Stop()
    {
    }

    public override GCode GetNextCommand()
    {
      this.OnGetNextCommand();
      if (this.Status == JobStatus.Paused || this.m_oswRefreshTimer.ElapsedMilliseconds <= 1000L)
        return (GCode) null;
      this.m_oswRefreshTimer.Restart();
      return FirmwareSDPrintJob.M27;
    }

    public override bool Aborted
    {
      get
      {
        return false;
      }
    }

    public override bool Done
    {
      get
      {
        return this.m_bDone.Value;
      }
    }

    public override bool RetractionRequired
    {
      get
      {
        return false;
      }
    }

    public override float PercentComplete
    {
      get
      {
        if (this.m_lSDFileSize <= 0L)
          return 0.0f;
        return (float) this.m_lSDFilePos / (float) this.m_lSDFileSize;
      }
    }

    public override bool CanPauseImmediately
    {
      get
      {
        return true;
      }
    }

    public override ulong CurrentFileLineNumber
    {
      get
      {
        return 0;
      }
    }

    public static FilamentSpool.TypeEnum GetFilamentTypeFromName(string gcodefile)
    {
      string extension = Path.GetExtension(gcodefile);
      FilamentSpool.TypeEnum result;
      if (!string.IsNullOrEmpty(extension) && 3 < extension.Length && Enum.TryParse<FilamentSpool.TypeEnum>(extension.Substring(1), true, out result))
        return result;
      return FilamentSpool.TypeEnum.NoFilament;
    }
  }
}
