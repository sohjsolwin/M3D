// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.SaveGCodeToFileJob
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
  internal class SaveGCodeToFileJob : AbstractPreprocessedJob
  {
    public SaveGCodeToFileJob(JobParams jobParams, string user, InternalPrinterProfile printerProfile)
      : base(jobParams, user, printerProfile)
    {
    }

    public override JobCreateResult Create(PrinterInfo printerInfo)
    {
      Status = JobStatus.SavingToFile;
      JobCreateResult jobCreateResult = base.Create(printerInfo);
      if (jobCreateResult.Result == ProcessReturn.SUCCESS)
      {
        try
        {
          using (var gcodeFileReader = new GCodeFileReader(GCodeFilename))
          {
            using (var gcodeFileWriter = new GCodeFileWriter(Details.jobParams.outputfile, true))
            {
              GCode nextLine;
              while ((nextLine = gcodeFileReader.GetNextLine(true)) != null)
              {
                gcodeFileWriter.Write(nextLine);
              }
            }
          }
        }
        catch (Exception ex)
        {
          jobCreateResult.Result = ProcessReturn.FAILURE;
        }
      }
      return jobCreateResult;
    }

    public override void Update(PrinterInfo printerInfo)
    {
    }

    public override bool Start(out List<string> start_gcode)
    {
      start_gcode = (List<string>) null;
      return true;
    }

    public override bool Pause(out List<string> pause_gcode, FilamentSpool spool)
    {
      throw new NotImplementedException();
    }

    public override JobController.Result Resume(out List<string> resume_gcode, FilamentSpool spool)
    {
      throw new NotImplementedException();
    }

    public override void Stop()
    {
      throw new NotImplementedException();
    }

    public override GCode GetNextCommand()
    {
      return (GCode) null;
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
        return true;
      }
    }

    public override bool RetractionRequired
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public override float PercentComplete
    {
      get
      {
        return 0.0f;
      }
    }

    public override bool CanPauseImmediately
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    public override ulong CurrentFileLineNumber
    {
      get
      {
        return 0;
      }
    }
  }
}
