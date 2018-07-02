// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Preprocessors.BackLashPreprocessor
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        if (nextLine.hasG && (nextLine.G == (ushort) 0 || nextLine.G == (ushort) 1) && !flag)
        {
          if (nextLine.hasF)
          {
            num1 = nextLine.F;
          }

          var num4 = !nextLine.hasX ? 0.0f : nextLine.X - position.relativeX;
          var num5 = !nextLine.hasY ? 0.0f : nextLine.Y - position.relativeY;
          var num6 = !nextLine.hasZ ? 0.0f : nextLine.Z - position.relativeZ;
          var num7 = !nextLine.hasE ? 0.0f : nextLine.E - position.relativeE;
          BackLashPreprocessor.Direction direction3 = (double) num4 <= 1.40129846432482E-45 ? ((double) num4 >= -1.40129846432482E-45 ? direction1 : BackLashPreprocessor.Direction.Negative) : BackLashPreprocessor.Direction.Positive;
          BackLashPreprocessor.Direction direction4 = (double) num5 <= 1.40129846432482E-45 ? ((double) num5 >= -1.40129846432482E-45 ? direction2 : BackLashPreprocessor.Direction.Negative) : BackLashPreprocessor.Direction.Positive;
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
          if (nextLine.hasX)
          {
            nextLine.X += num2;
          }

          if (nextLine.hasY)
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
        else if (nextLine.hasG && nextLine.G == (ushort) 92)
        {
          if (nextLine.hasE)
          {
            position.relativeE = nextLine.E;
          }

          if (printerProfile.OptionsConstants.G92WorksOnAllAxes)
          {
            if (nextLine.hasX)
            {
              position.relativeX = nextLine.X;
            }

            if (nextLine.hasY)
            {
              position.relativeY = nextLine.Y;
            }

            if (nextLine.hasZ)
            {
              position.relativeZ = nextLine.Z;
            }
          }
          if (!nextLine.hasE && !nextLine.hasX && (!nextLine.hasY && !nextLine.hasZ))
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
        else if (nextLine.hasG && nextLine.G == (ushort) 90)
        {
          flag = false;
        }
        else if (nextLine.hasG && nextLine.G == (ushort) 91)
        {
          flag = true;
        }
        else if (nextLine.hasG && nextLine.G == (ushort) 28)
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
