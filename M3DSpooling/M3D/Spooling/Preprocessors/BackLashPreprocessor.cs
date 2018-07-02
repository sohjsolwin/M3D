using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;

namespace M3D.Spooling.Preprocessors
{
  internal class BackLashPreprocessor : IPreprocessor
  {
    public override string Name
    {
      get
      {
        return "backlash";
      }
    }

    internal override bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      var backlashX = calibration.BACKLASH_X;
      var backlashY = calibration.BACKLASH_Y;
      var flag = true;
      var position = new Position();
      var num1 = 1000f;
      BackLashPreprocessor.Direction direction1 = BackLashPreprocessor.Direction.Neither;
      BackLashPreprocessor.Direction direction2 = BackLashPreprocessor.Direction.Neither;
      var num2 = 0.0f;
      var num3 = 0.0f;
      for (GCode nextLine = input_reader.GetNextLine(false); nextLine != null; nextLine = input_reader.GetNextLine(false))
      {
        if (nextLine.HasG && (nextLine.G == 0 || nextLine.G == 1) && !flag)
        {
          if (nextLine.hasF)
          {
            num1 = nextLine.F;
          }

          var num4 = !nextLine.HasX ? 0.0f : nextLine.X - position.relativeX;
          var num5 = !nextLine.HasY ? 0.0f : nextLine.Y - position.relativeY;
          var num6 = !nextLine.HasZ ? 0.0f : nextLine.Z - position.relativeZ;
          var num7 = !nextLine.HasE ? 0.0f : nextLine.E - position.relativeE;
          BackLashPreprocessor.Direction direction3 = num4 <= 1.40129846432482E-45 ? num4 >= -1.40129846432482E-45 ? direction1 : BackLashPreprocessor.Direction.Negative : BackLashPreprocessor.Direction.Positive;
          BackLashPreprocessor.Direction direction4 = num5 <= 1.40129846432482E-45 ? num5 >= -1.40129846432482E-45 ? direction2 : BackLashPreprocessor.Direction.Negative : BackLashPreprocessor.Direction.Positive;
          var code = new GCode
          {
            G = nextLine.G
          };
          if (direction3 != direction1 && direction1 != BackLashPreprocessor.Direction.Neither || direction4 != direction2 && direction2 != BackLashPreprocessor.Direction.Neither)
          {
            if (direction3 != direction1 && direction1 != BackLashPreprocessor.Direction.Neither)
            {
              num2 += direction3 == BackLashPreprocessor.Direction.Positive ? backlashX : -backlashX;
            }

            if (direction4 != direction2 && direction2 != BackLashPreprocessor.Direction.Neither)
            {
              num3 += direction4 == BackLashPreprocessor.Direction.Positive ? backlashY : -backlashY;
            }

            var num8 = position.relativeX + num2;
            var num9 = position.relativeY + num3;
            code.X = num8;
            code.Y = num9;
            code.F = calibration.BACKLASH_SPEED;
            output_writer.Write(code);
            nextLine.F = num1;
          }
          if (nextLine.HasX)
          {
            nextLine.X += num2;
          }

          if (nextLine.HasY)
          {
            nextLine.Y += num3;
          }

          position.relativeX += num4;
          position.relativeY += num5;
          position.relativeZ += num6;
          position.relativeE += num7;
          if (nextLine.hasF)
          {
            position.F = nextLine.F;
          }

          position.absoluteX += num4;
          position.absoluteY += num5;
          position.absoluteZ += num6;
          position.absoluteE += num7;
          direction1 = direction3;
          direction2 = direction4;
        }
        else if (nextLine.HasG && nextLine.G == 92)
        {
          if (nextLine.HasE)
          {
            position.relativeE = nextLine.E;
          }

          if (printerProfile.OptionsConstants.G92WorksOnAllAxes)
          {
            if (nextLine.HasX)
            {
              position.relativeX = nextLine.X;
            }

            if (nextLine.HasY)
            {
              position.relativeY = nextLine.Y;
            }

            if (nextLine.HasZ)
            {
              position.relativeZ = nextLine.Z;
            }
          }
          if (!nextLine.HasE && !nextLine.HasX && (!nextLine.HasY && !nextLine.HasZ))
          {
            position.relativeE = 0.0f;
            if (printerProfile.OptionsConstants.G92WorksOnAllAxes)
            {
              position.relativeX = 0.0f;
              position.relativeY = 0.0f;
              position.relativeZ = 0.0f;
            }
          }
        }
        else if (nextLine.HasG && nextLine.G == 90)
        {
          flag = false;
        }
        else if (nextLine.HasG && nextLine.G == 91)
        {
          flag = true;
        }
        else if (nextLine.HasG && nextLine.G == 28)
        {
          position.relativeX = position.absoluteX = 54f;
          position.relativeY = position.absoluteY = 50f;
        }
        output_writer.Write(nextLine);
      }
      return true;
    }

    private enum Direction
    {
      Positive,
      Negative,
      Neither,
    }
  }
}
