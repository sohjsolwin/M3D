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
      var warnings = new List<MessageType>();
      ProcessReturn result = PrepareJobForPrinter(printerInfo, MyPrinterProfile, warnings);
      if (AutoStarting && warnings != null)
      {
        warnings.Clear();
      }

      return new JobCreateResult((AbstractJob) null, result, warnings);
    }

    public string GCodeFilename { get; protected set; }

    public float EstimatedFilamentUsed { get; private set; }

    private ProcessReturn PrepareJobForPrinter(PrinterInfo printerInfo, InternalPrinterProfile printerProfile, List<MessageType> warnings)
    {
      var flag = false;
      var jobGuid = Details.jobParams.jobGuid;
      if (!Details.jobParams.options.dont_use_copy_to_spooler)
      {
        GCodeFilename = Path.Combine(Paths.QueuePath, jobGuid);
        if (Details.jobParams.gcodefile.ToLower().IndexOf("backlash_calibration") > -1)
        {
          flag = true;
          BacklashCalibrationPrint.Create(GCodeFilename, Details.jobParams.filament_type, printerProfile.PrinterSizeConstants.HomeLocation.x, printerProfile.PrinterSizeConstants.HomeLocation.y);
        }
        else
        {
          File.Copy(Details.jobParams.gcodefile, GCodeFilename);
        }
      }
      else
      {
        GCodeFilename = Details.jobParams.gcodefile;
      }

      if (!string.IsNullOrEmpty(Details.jobParams.preview_image_file_name))
      {
        if (Details.jobParams.preview_image_file_name != "null")
        {
          try
          {
            var str = Details.jobParams.preview_image_file_name.Substring(Details.jobParams.preview_image_file_name.LastIndexOf("."));
            PreviewImageFileName = Path.Combine(Paths.QueuePath, jobGuid) + str;
            File.Copy(Details.jobParams.preview_image_file_name, PreviewImageFileName, true);
            goto label_10;
          }
          catch (Exception ex)
          {
            PreviewImageFileName = "null";
            goto label_10;
          }
        }
      }
      PreviewImageFileName = "null";
label_10:
      if (!flag && !Details.jobParams.options.dont_use_preprocessors)
      {
        switch (GatherInitialInformation(GCodeFilename, printerProfile))
        {
          case AbstractPreprocessedJob.PrintJobWarning.Error_OutOfBounds:
            return ProcessReturn.FAILURE_OUT_OF_BOUNDS;
          case AbstractPreprocessedJob.PrintJobWarning.Warning_ABS_Size:
            warnings.Add(MessageType.WarningABSPrintLarge);
            break;
        }
        var num = 0;
        foreach (IPreprocessor preprocessor in printerProfile.PreprocessorConstants.preprocessor_list)
        {
          try
          {
            GCodeFilename = PreprocessGCode(GCodeFilename, printerInfo, printerProfile, preprocessor, Details, ++num);
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException(string.Format(">> {0}::", (object) preprocessor.Name), ex);
            throw new AbstractPreprocessedJob.PreprocessorException(string.Format("{0}::{1}", (object) preprocessor.Name, (object) ex.Message));
          }
        }
      }
      Details.jobParams.gcodefile = GCodeFilename;
      return ProcessReturn.SUCCESS;
    }

    public string PreprocessGCode(string filename, PrinterInfo printerInfo, InternalPrinterProfile printerProfile, IPreprocessor processor, JobDetails bounds, int number)
    {
      var fileInfo = new FileInfo(filename);
      var directoryName = fileInfo.DirectoryName;
      var name = fileInfo.Name;
      var length = name.IndexOf('_');
      string str;
      if (length > 0)
      {
        str = name.Substring(0, length) + "_" + (object) number + "_" + processor.Name + "_processed.gcode";
      }
      else
      {
        str = name.Substring(0, name.Length - fileInfo.Extension.Length) + "_" + (object) number + "_" + processor.Name + "_processed.gcode";
      }

      var gcodefilename = directoryName + Path.DirectorySeparatorChar.ToString() + str;
      var input_reader = new GCodeFileReader(filename);
      var output_writer = new GCodeFileWriter(gcodefilename);
      if (!processor.ProcessGCode(input_reader, output_writer, printerInfo.calibration, bounds, printerProfile))
      {
        return (string) null;
      }

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
        {
          throw new Exception(string.Format("Unable to open file: {0}", (object) filename));
        }
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      catch (Exception ex)
      {
        throw new Exception("PrinterJob.GatherInitialInformation ", ex);
      }
      EstimatedFilamentUsed = 0.0f;
      if (gcodeFileReader != null)
      {
        var boundsCheckXy = Details.jobParams.options.bounds_check_xy;
        var flag = false;
        var p = new Vector3D(float.NaN, float.NaN, float.NaN);
        GCode nextLine;
        while ((nextLine = gcodeFileReader.GetNextLine(false)) != null)
        {
          if (nextLine.hasG)
          {
            if (nextLine.G == (ushort) 90)
            {
              flag = true;
            }
            else if (nextLine.G == (ushort) 91)
            {
              flag = false;
            }
            else if (((nextLine.G == (ushort) 0 ? 1 : (nextLine.G == (ushort) 1 ? 1 : 0)) & (flag ? 1 : 0)) != 0)
            {
              if (nextLine.hasE)
              {
                if (flag)
                {
                  EstimatedFilamentUsed = nextLine.E;
                }
                else
                {
                  EstimatedFilamentUsed += nextLine.E;
                }
              }
              if (nextLine.hasZ)
              {
                p.z = nextLine.Z;
                if ((double) nextLine.Z < (double)Details.bounds.min.z)
                {
                  Details.bounds.min.z = nextLine.Z;
                }

                if ((double) nextLine.Z > (double)Details.bounds.max.z)
                {
                  Details.bounds.max.z = nextLine.Z;
                }
              }
              if (boundsCheckXy)
              {
                if (nextLine.hasX)
                {
                  p.x = nextLine.X;
                  if ((double) nextLine.X < (double)Details.bounds.min.x)
                  {
                    Details.bounds.min.x = nextLine.X;
                  }

                  if ((double) nextLine.X > (double)Details.bounds.max.x)
                  {
                    Details.bounds.max.x = nextLine.X;
                  }
                }
                if (nextLine.hasY)
                {
                  p.y = nextLine.Y;
                  if ((double) nextLine.Y < (double)Details.bounds.min.y)
                  {
                    Details.bounds.min.y = nextLine.Y;
                  }

                  if ((double) nextLine.Y > (double)Details.bounds.max.y)
                  {
                    Details.bounds.max.y = nextLine.Y;
                  }
                }
              }
              if (!printerProfile.PrinterSizeConstants.PrintableRegion.InRegionNaN(p))
              {
                gcodeFileReader.Close();
                return AbstractPreprocessedJob.PrintJobWarning.Error_OutOfBounds;
              }
            }
            if (nextLine != null && nextLine.ToString().IndexOf("ideal temp:") > -1)
            {
              Details.ideal_temperature = (int) float.Parse(Regex.Match(nextLine.ToString(), "ideal temp:([-.0-9]+)").Groups[1].Value);
            }
          }
          if (nextLine != null && nextLine.ToString().IndexOf("ideal temp:") > -1)
          {
            Details.ideal_temperature = (int) float.Parse(Regex.Match(nextLine.ToString(), "ideal temp:([-.0-9]+)").Groups[1].Value);
          }
        }
        gcodeFileReader.Close();
        if (Details.jobParams.filament_type == FilamentSpool.TypeEnum.ABS && ((double)Details.bounds.max.x - (double)Details.bounds.min.x > (double) printerProfile.PrinterSizeConstants.ABSWarningDim || (double)Details.bounds.max.y - (double)Details.bounds.min.y > (double) printerProfile.PrinterSizeConstants.ABSWarningDim || (double)Details.bounds.max.z - (double)Details.bounds.min.z > (double) printerProfile.PrinterSizeConstants.ABSWarningDim))
        {
          return AbstractPreprocessedJob.PrintJobWarning.Warning_ABS_Size;
        }
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
