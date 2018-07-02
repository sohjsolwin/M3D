using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;

namespace M3D.Spooling.Preprocessors
{
  internal class GCodeInitializationPreprocessor : IPreprocessor
  {
    private const int nNozzlePreheatTemperature = 175;
    private const int nNozzleCoolingMargin = 45;

    public override string Name
    {
      get
      {
        return "init";
      }
    }

    private bool ProcessGCode_HelperLayerComments(GCodeFileReader input_reader, GCodeFileWriter output_writer, JobDetails jobdetails, InternalPrinterProfile printerProfile, InitialPrintPreProcessorData initial_print_settings)
    {
      var flag1 = false;
      var num1 = 0;
      var num2 = 0;
      var flag2 = false;
      var flag3 = false;
      var flag4 = false;
      var flag5 = false;
      var useFanPreprocessor = jobdetails.jobParams.options.use_fan_preprocessor;
      GCode nextLine;
      while ((nextLine = input_reader.GetNextLine(false)) != null)
      {
        if (nextLine.orig.StartsWith(";LAYER:"))
        {
          if (nextLine.ToString().ToLower().Contains(";layer:-"))
          {
            flag5 = true;
          }

          flag1 = true;
          flag2 = true;
        }
        else if (nextLine.hasZ & flag3 && !flag1)
        {
          output_writer.Write(new GCode(";LAYER:" + (object) num1++));
          flag2 = true;
        }
        else if (nextLine.hasG)
        {
          if (nextLine.G == (ushort) 90)
          {
            flag3 = true;
          }
          else if (nextLine.G == (ushort) 91)
          {
            flag3 = false;
          }
        }
        if (!(nextLine.orig.ToLower() == "t0") && nextLine.orig.ToLower().IndexOf(" t0 ") <= -1 && (!nextLine.hasM || nextLine.M != (ushort) 104 && nextLine.M != (ushort) 109) && (!useFanPreprocessor || !nextLine.hasM || nextLine.M != (ushort) 106 && nextLine.M != (ushort) 107))
        {
          var flag6 = false;
          if (nextLine.orig.StartsWith(";LAYER:"))
          {
            output_writer.Write(nextLine);
            flag6 = true;
          }
          if (flag2)
          {
            int num3;
            switch (num2)
            {
              case 0:
                var layerTemperature = (float) initial_print_settings.FirstRaftLayerTemperature;
                output_writer.Write(new GCode("M109 S" + layerTemperature.ToString()));
                output_writer.Write(new GCode("G90"));
                BoundingBox bounds = printerProfile.PrinterSizeConstants.WarningRegion.bounds_list[0];
                output_writer.Write(new GCode(PrinterCompatibleString.Format("G0 X{0} Y{1} Z0.5 F3600", (object) (float) ((double) bounds.max.x - 1.0), (object) (float) (((double) bounds.min.y + (double) bounds.max.y) / 2.0 + 10.0))));
                output_writer.Write(new GCode("G91                  ;Go relative"));
                output_writer.Write(new GCode(PrinterCompatibleString.Format("G0 E{0} F{1}          ;prime the nozzle", (object) initial_print_settings.PrimeAmount, (object) 72f)));
                output_writer.Write(new GCode("G4 S0"));
                output_writer.Write(new GCode("G92 E0               ;reset E"));
                output_writer.Write(new GCode("G90                  ;Go absolute"));
                goto label_22;
              case 1:
                num3 = initial_print_settings.SecondRaftResetTemp ? 1 : 0;
                break;
              default:
                num3 = 0;
                break;
            }
            var num4 = flag5 ? 1 : 0;
            if ((num3 & num4) != 0)
            {
              var filamentTemperature = (float) jobdetails.jobParams.filament_temperature;
              output_writer.Write(new GCode(";Reset to the ideal temperature"));
              output_writer.Write(new GCode("M104 S" + filamentTemperature.ToString()));
              flag4 = true;
            }
label_22:
            ++num2;
            flag2 = false;
          }
          if (!flag4 && (flag5 && nextLine.ToString().ToLower() == ";layer:0" || !flag5 && num2 > 2 || num2 > 4))
          {
            var filamentTemperature = (float) jobdetails.jobParams.filament_temperature;
            output_writer.Write(new GCode(";Reset to the ideal temperature"));
            output_writer.Write(new GCode("M104 S" + filamentTemperature.ToString()));
            flag4 = true;
          }
          if (!flag6)
          {
            output_writer.Write(nextLine);
          }
        }
      }
      return true;
    }

    private void ProcessGCode_AddStartGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, JobOptions jobOptions, Calibration calibration, InternalPrinterProfile printerProfile, InitialPrintPreProcessorData initial_print_settings)
    {
      var num = 0;
      if (jobOptions.use_heated_bed && printerProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
      {
        num = initial_print_settings.BedTemperature;
      }

      output_writer.Write(new GCode("M106 S1"));
      if (0 < num)
      {
        output_writer.Write(new GCode("M18"));
        output_writer.Write(new GCode("M104 S0"));
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M140 S{0}", (object) num)));
        for (var index = 0; index < 8; ++index)
        {
          output_writer.Write(new GCode("G4 S15"));
        }

        output_writer.Write(new GCode("M140 S0"));
      }
      output_writer.Write(new GCode("M17"));
      output_writer.Write(new GCode("G4 S1"));
      if (0 < num)
      {
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M104 S{0}", (object) 175)));
      }
      else
      {
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M104 S{0}", (object) initial_print_settings.StartingTemp)));
      }

      output_writer.Write(new GCode("G90"));
      output_writer.Write(new GCode(PrinterCompatibleString.Format("G0 Z5 F{0}", (object) (float) ((double) printerProfile.SpeedLimitConstants.DEFAULT_FEEDRATE_Z * 60.0))));
      if (0 < num)
      {
        output_writer.Write(new GCode("M18"));
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M109 S{0}", (object) 175)));
        output_writer.Write(new GCode("G4 S5"));
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M104 S{0}", (object) 130)));
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M140 S{0}", (object) num)));
        for (var index = 0; index < 8; ++index)
        {
          output_writer.Write(new GCode("G4 S15"));
        }

        output_writer.Write(new GCode(PrinterCompatibleString.Format("M104 S{0}", (object) initial_print_settings.StartingTemp)));
        for (var index = 0; index < 3; ++index)
        {
          output_writer.Write(new GCode("G4 S15"));
        }

        output_writer.Write(new GCode("M17"));
        for (var index = 0; index < 1; ++index)
        {
          output_writer.Write(new GCode("G4 S15"));
        }
      }
      output_writer.Write(new GCode("G28"));
      if (0 < num)
      {
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M190 S{0}", (object) num)));
      }

      output_writer.Write(new GCode("M106 S1"));
      output_writer.Write(new GCode("G90"));
      output_writer.Write(new GCode(PrinterCompatibleString.Format("M109 S{0}", (object) initial_print_settings.StartingTemp)));
      if (initial_print_settings.StartingTempStabilizationDelay != 0)
      {
        output_writer.Write(new GCode(PrinterCompatibleString.Format("G4 S{0}", (object) initial_print_settings.StartingTempStabilizationDelay)));
      }

      if (jobOptions.use_fan_preprocessor && initial_print_settings.StartingFanValue != 0)
      {
        output_writer.Write(new GCode(PrinterCompatibleString.Format("M106 S{0}", (object) initial_print_settings.StartingFanValue)));
      }

      output_writer.Write(new GCode(PrinterCompatibleString.Format("G0 F{0}", (object) 1800)));
      output_writer.Write(new GCode("; can extrude"));
    }

    private void ProcessGCode_EndGCode(GCodeFileWriter output_writer, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      foreach (var s in GCodeInitializationPreprocessor.GenerateEndGCode(jobdetails, printerProfile, true))
      {
        output_writer.Write(new GCode(s));
      }
    }

    internal override bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      var num1 = 1;
      InitialPrintPreProcessorData initialPrint = jobdetails.jobParams.preprocessor.initialPrint;
      ProcessGCode_AddStartGCode(input_reader, output_writer, jobdetails.jobParams.options, calibration, printerProfile, initialPrint);
      var num2 = ProcessGCode_HelperLayerComments(input_reader, output_writer, jobdetails, printerProfile, initialPrint) ? 1 : 0;
      var num3 = num1 & num2;
      if (num3 == 0)
      {
        return num3 != 0;
      }

      ProcessGCode_EndGCode(output_writer, jobdetails, printerProfile);
      return num3 != 0;
    }

    public static List<string> GenerateEndGCode(JobDetails jobdetails, InternalPrinterProfile printerProfile, bool retract)
    {
      var stringList = new List<string>();
      InitialPrintPreProcessorData initialPrint = jobdetails.jobParams.preprocessor.initialPrint;
      PrinterSizeProfile printerSizeConstants = printerProfile.PrinterSizeConstants;
      stringList.Add("G91");
      if (retract)
      {
        var primeAmount = (float) initialPrint.PrimeAmount;
        var num1 = (float) Math.Round(0.25 * (double) primeAmount);
        var num2 = primeAmount - num1;
        var num3 = 1800f;
        stringList.Add(PrinterCompatibleString.Format("G0 X5 Y5 E{1} F{0}", (object) num3, (object) (float) -(double) num1));
        var num4 = 360f;
        stringList.Add(PrinterCompatibleString.Format("G0 E{1} F{0}", (object) num4, (object) (float) -(double) num2));
      }
      stringList.Add("M104 S0");
      if (printerProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed && jobdetails.jobParams.options.use_heated_bed)
      {
        stringList.Add("M140 S0");
      }

      if ((double) jobdetails.bounds.max.z > (double) printerSizeConstants.BoxTopLimitZ)
      {
        BoundingBox bounds = printerSizeConstants.WarningRegion.bounds_list[printerSizeConstants.WarningRegion.bounds_list.Count - 1];
        if ((double) jobdetails.bounds.max.z + 1.0 < (double) bounds.max.z)
        {
          var num = 90f;
          stringList.Add(PrinterCompatibleString.Format("G0 Z1 F{0}", (object) num));
        }
        var num1 = 1800f;
        stringList.Add("G90");
        stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2}", (object) printerSizeConstants.BackCornerPositionBoxTop.x, (object) printerSizeConstants.BackCornerPositionBoxTop.y, (object) num1));
      }
      else
      {
        var num1 = 90f;
        stringList.Add(PrinterCompatibleString.Format("G0 Z3 F{0}", (object) num1));
        stringList.Add("G90");
        var num2 = 1800f;
        stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} F{2}", (object) printerSizeConstants.BackCornerPosition.x, (object) printerSizeConstants.BackCornerPosition.y, (object) num2));
      }
      stringList.Add("M18");
      return stringList;
    }
  }
}
