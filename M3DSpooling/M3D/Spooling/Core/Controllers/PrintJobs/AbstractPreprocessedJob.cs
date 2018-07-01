// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.PrintJobs.AbstractPreprocessedJob
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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace M3D.Spooling.Core.Controllers.PrintJobs
{
  internal abstract class AbstractPreprocessedJob : AbstractJob
  {
    public AbstractPreprocessedJob(JobParams jobParams, string user, InternalPrinterProfile printerProfile)
      : base(jobParams, user, printerProfile)
    {
    }

    public override JobCreateResult Create(PrinterInfo printerInfo)
    {
      List<MessageType> warnings = new List<MessageType>();
      ProcessReturn result = this.PrepareJobForPrinter(printerInfo, this.MyPrinterProfile, warnings);
      if (this.AutoStarting && warnings != null)
        warnings.Clear();
      return new JobCreateResult((AbstractJob) null, result, warnings);
    }

    public string GCodeFilename { get; protected set; }

    public float EstimatedFilamentUsed { get; private set; }

    private ProcessReturn PrepareJobForPrinter(PrinterInfo printerInfo, InternalPrinterProfile printerProfile, List<MessageType> warnings)
    {
      bool flag = false;
      string jobGuid = this.Details.jobParams.jobGuid;
      if (!this.Details.jobParams.options.dont_use_copy_to_spooler)
      {
        this.GCodeFilename = Path.Combine(Paths.QueuePath, jobGuid);
        if (this.Details.jobParams.gcodefile.ToLower().IndexOf("backlash_calibration") > -1)
        {
          flag = true;
          BacklashCalibrationPrint.Create(this.GCodeFilename, this.Details.jobParams.filament_type, printerProfile.PrinterSizeConstants.HomeLocation.x, printerProfile.PrinterSizeConstants.HomeLocation.y);
        }
        else
          File.Copy(this.Details.jobParams.gcodefile, this.GCodeFilename);
      }
      else
        this.GCodeFilename = this.Details.jobParams.gcodefile;
      if (!string.IsNullOrEmpty(this.Details.jobParams.preview_image_file_name))
      {
        if (this.Details.jobParams.preview_image_file_name != "null")
        {
          try
          {
            string str = this.Details.jobParams.preview_image_file_name.Substring(this.Details.jobParams.preview_image_file_name.LastIndexOf("."));
            this.PreviewImageFileName = Path.Combine(Paths.QueuePath, jobGuid) + str;
            File.Copy(this.Details.jobParams.preview_image_file_name, this.PreviewImageFileName, true);
            goto label_10;
          }
          catch (Exception ex)
          {
            this.PreviewImageFileName = "null";
            goto label_10;
          }
        }
      }
      this.PreviewImageFileName = "null";
label_10:
      if (!flag && !this.Details.jobParams.options.dont_use_preprocessors)
      {
        switch (this.GatherInitialInformation(this.GCodeFilename, printerProfile))
        {
          case AbstractPreprocessedJob.PrintJobWarning.Error_OutOfBounds:
            return ProcessReturn.FAILURE_OUT_OF_BOUNDS;
          case AbstractPreprocessedJob.PrintJobWarning.Warning_ABS_Size:
            warnings.Add(MessageType.WarningABSPrintLarge);
            break;
        }
        int num = 0;
        foreach (IPreprocessor preprocessor in printerProfile.PreprocessorConstants.preprocessor_list)
        {
          try
          {
            this.GCodeFilename = this.PreprocessGCode(this.GCodeFilename, printerInfo, printerProfile, preprocessor, this.Details, ++num);
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException(string.Format(">> {0}::", (object) preprocessor.Name), ex);
            throw new AbstractPreprocessedJob.PreprocessorException(string.Format("{0}::{1}", (object) preprocessor.Name, (object) ex.Message));
          }
        }
      }
      this.Details.jobParams.gcodefile = this.GCodeFilename;
      return ProcessReturn.SUCCESS;
    }

    public string PreprocessGCode(string filename, PrinterInfo printerInfo, InternalPrinterProfile printerProfile, IPreprocessor processor, JobDetails bounds, int number)
    {
      FileInfo fileInfo = new FileInfo(filename);
      string directoryName = fileInfo.DirectoryName;
      string name = fileInfo.Name;
      int length = name.IndexOf('_');
      string str;
      if (length > 0)
        str = name.Substring(0, length) + "_" + (object) number + "_" + processor.Name + "_processed.gcode";
      else
        str = name.Substring(0, name.Length - fileInfo.Extension.Length) + "_" + (object) number + "_" + processor.Name + "_processed.gcode";
      string gcodefilename = directoryName + Path.DirectorySeparatorChar.ToString() + str;
      GCodeFileReader input_reader = new GCodeFileReader(filename);
      GCodeFileWriter output_writer = new GCodeFileWriter(gcodefilename);
      if (!processor.ProcessGCode(input_reader, output_writer, printerInfo.calibration, bounds, printerProfile))
        return (string) null;
      input_reader.Close();
      output_writer.Close();
      return gcodefilename;
    }

    private AbstractPreprocessedJob.PrintJobWarning GatherInitialInformation(string filename, InternalPrinterProfile printerProfile)
    {
      GCodeFileReader gcodeFileReader;
      try
      {
        gcodeFileReader = new GCodeFileReader(filename);
        if (!gcodeFileReader.IsOpen)
          throw new Exception(string.Format("Unable to open file: {0}", (object) filename));
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        throw new Exception("PrinterJob.GatherInitialInformation ", ex);
      }
      this.EstimatedFilamentUsed = 0.0f;
      if (gcodeFileReader != null)
      {
        bool boundsCheckXy = this.Details.jobParams.options.bounds_check_xy;
        bool flag = false;
        Vector3D p = new Vector3D(float.NaN, float.NaN, float.NaN);
        GCode nextLine;
        while ((nextLine = gcodeFileReader.GetNextLine(false)) != null)
        {
          if (nextLine.hasG)
          {
            if (nextLine.G == (ushort) 90)
              flag = true;
            else if (nextLine.G == (ushort) 91)
              flag = false;
            else if (((nextLine.G == (ushort) 0 ? 1 : (nextLine.G == (ushort) 1 ? 1 : 0)) & (flag ? 1 : 0)) != 0)
            {
              if (nextLine.hasE)
              {
                if (flag)
                  this.EstimatedFilamentUsed = nextLine.E;
                else
                  this.EstimatedFilamentUsed += nextLine.E;
              }
              if (nextLine.hasZ)
              {
                p.z = nextLine.Z;
                if ((double) nextLine.Z < (double) this.Details.bounds.min.z)
                  this.Details.bounds.min.z = nextLine.Z;
                if ((double) nextLine.Z > (double) this.Details.bounds.max.z)
                  this.Details.bounds.max.z = nextLine.Z;
              }
              if (boundsCheckXy)
              {
                if (nextLine.hasX)
                {
                  p.x = nextLine.X;
                  if ((double) nextLine.X < (double) this.Details.bounds.min.x)
                    this.Details.bounds.min.x = nextLine.X;
                  if ((double) nextLine.X > (double) this.Details.bounds.max.x)
                    this.Details.bounds.max.x = nextLine.X;
                }
                if (nextLine.hasY)
                {
                  p.y = nextLine.Y;
                  if ((double) nextLine.Y < (double) this.Details.bounds.min.y)
                    this.Details.bounds.min.y = nextLine.Y;
                  if ((double) nextLine.Y > (double) this.Details.bounds.max.y)
                    this.Details.bounds.max.y = nextLine.Y;
                }
              }
              if (!printerProfile.PrinterSizeConstants.PrintableRegion.InRegionNaN(p))
              {
                gcodeFileReader.Close();
                return AbstractPreprocessedJob.PrintJobWarning.Error_OutOfBounds;
              }
            }
            if (nextLine != null && nextLine.ToString().IndexOf("ideal temp:") > -1)
              this.Details.ideal_temperature = (int) float.Parse(Regex.Match(nextLine.ToString(), "ideal temp:([-.0-9]+)").Groups[1].Value);
          }
          if (nextLine != null && nextLine.ToString().IndexOf("ideal temp:") > -1)
            this.Details.ideal_temperature = (int) float.Parse(Regex.Match(nextLine.ToString(), "ideal temp:([-.0-9]+)").Groups[1].Value);
        }
        gcodeFileReader.Close();
        if (this.Details.jobParams.filament_type == FilamentSpool.TypeEnum.ABS && ((double) this.Details.bounds.max.x - (double) this.Details.bounds.min.x > (double) printerProfile.PrinterSizeConstants.ABSWarningDim || (double) this.Details.bounds.max.y - (double) this.Details.bounds.min.y > (double) printerProfile.PrinterSizeConstants.ABSWarningDim || (double) this.Details.bounds.max.z - (double) this.Details.bounds.min.z > (double) printerProfile.PrinterSizeConstants.ABSWarningDim))
          return AbstractPreprocessedJob.PrintJobWarning.Warning_ABS_Size;
      }
      return AbstractPreprocessedJob.PrintJobWarning.Job_OK;
    }

    private enum PrintJobWarning
    {
      Job_OK,
      Error_OutOfBounds,
      Warning_ABS_Size,
    }

    internal class PreprocessorException : Exception
    {
      public PreprocessorException(string message)
        : base(message)
      {
      }
    }
  }
}
