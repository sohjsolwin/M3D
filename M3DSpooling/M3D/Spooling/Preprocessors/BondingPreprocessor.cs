using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;

namespace M3D.Spooling.Preprocessors
{
  internal class BondingPreprocessor : IPreprocessor
  {
    private const float WAVE_PERIOD_LENGTH = 5f;
    private const float WAVE_PERIOD_LENGTH_QUARTER = 1.25f;
    private const float WAVE_SIZE = 0.15f;

    public override string Name
    {
      get
      {
        return "bonding";
      }
    }

    internal override bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      var wave_step = 0;
      var num1 = 0;
      var flag1 = true;
      var flag2 = true;
      var flag3 = false;
      var num2 = 0;
      var num3 = 0.0;
      var position = new Position();
      var num4 = (float) num3;
      var num5 = 0.0f;
      var firstLayerTemp = jobdetails.jobParams.preprocessor.bonding.FirstLayerTemp;
      var secondLayerTemp = jobdetails.jobParams.preprocessor.bonding.SecondLayerTemp;
      while (true)
      {
        GCode nextLine = input_reader.GetNextLine(false);
        if (nextLine != null)
        {
          if (!jobdetails.jobParams.options.use_wave_bonding)
          {
            output_writer.Write(nextLine);
          }
          else
          {
            if (nextLine.ToString().Contains(";LAYER:"))
            {
              int num6;
              try
              {
                num6 = int.Parse(nextLine.ToString().Substring(7));
              }
              catch (Exception ex)
              {
                num6 = 0;
                ErrorLogger.LogException("Exception in BondingPreProcessor.ProcessGcode " + ex.Message, ex);
              }
              if (num6 < num1)
              {
                num1 = num6;
              }

              flag2 = num6 == num1;
            }
            if (nextLine.HasG && (nextLine.G == 0 || nextLine.G == 1) && !flag1)
            {
              if (nextLine.HasX || nextLine.HasY)
              {
                flag3 = true;
              }

              var num6 = !nextLine.HasX ? 0.0f : nextLine.X - position.relativeX;
              var num7 = !nextLine.HasY ? 0.0f : nextLine.Y - position.relativeY;
              var num8 = !nextLine.HasZ ? 0.0f : nextLine.Z - position.relativeZ;
              var num9 = !nextLine.HasE ? 0.0f : nextLine.E - position.relativeE;
              position.absoluteX += num6;
              position.absoluteY += num7;
              position.absoluteZ += num8;
              position.absoluteE += num9;
              position.relativeX += num6;
              position.relativeY += num7;
              position.relativeZ += num8;
              position.relativeE += num9;
              if (nextLine.hasF)
              {
                position.F = nextLine.F;
              }

              var num10 = (float) Math.Sqrt(num6 * (double)num6 + num7 * (double)num7);
              var num11 = 1;
              if (num10 > 1.25)
              {
                num11 = (int)(num10 / 1.25);
              }

              var num12 = position.absoluteX - num6;
              var num13 = position.absoluteY - num7;
              var num14 = position.relativeX - num6;
              var num15 = position.relativeY - num7;
              var num16 = position.relativeZ - num8;
              var num17 = position.relativeE - num9;
              var num18 = num6 / num10;
              var num19 = num7 / num10;
              var num20 = num8 / num10;
              var num21 = num9 / num10;
              if (flag2 && num9 > 0.0)
              {
                for (var index = 1; index < num11 + 1; ++index)
                {
                  float num22;
                  float num23;
                  float num24;
                  float num25;
                  if (index == num11)
                  {
                    var absoluteX = (double) position.absoluteX;
                    var absoluteY = (double) position.absoluteY;
                    num22 = position.relativeX;
                    num23 = position.relativeY;
                    num24 = position.relativeZ;
                    num25 = position.relativeE;
                  }
                  else
                  {
                    num22 = num14 + index * 1.25f * num18;
                    num23 = num15 + index * 1.25f * num19;
                    num24 = num16 + index * 1.25f * num20;
                    num25 = num17 + index * 1.25f * num21;
                  }
                  var num26 = num25 - (double)num5;
                  if (index != num11)
                  {
                    var code = new GCode
                    {
                      G = nextLine.G
                    };
                    if (nextLine.HasX)
                    {
                      code.X = (float)(position.relativeX - (double)num6 + (num22 - (double)num14));
                    }

                    if (nextLine.HasY)
                    {
                      code.Y = (float)(position.relativeY - (double)num7 + (num23 - (double)num15));
                    }

                    if (nextLine.hasF && index == 1)
                    {
                      code.F = nextLine.F;
                    }

                    if (flag3)
                    {
                      code.Z = (float)(position.relativeZ - (double)num8 + (num24 - (double)num16)) + CurrentAdjustmentsZ(ref wave_step);
                    }
                    else if (nextLine.HasZ && (num8 > 1.40129846432482E-45 || num8 < -1.40129846432482E-45))
                    {
                      code.Z = (float)(position.relativeZ - (double)num8 + (num24 - (double)num16));
                    }

                    code.E = (float)(position.relativeE - (double)num9 + (num25 - (double)num17)) + num4;
                    output_writer.Write(code);
                  }
                  else
                  {
                    if (flag3)
                    {
                      if (nextLine.HasZ)
                      {
                        nextLine.Z += CurrentAdjustmentsZ(ref wave_step);
                      }
                      else
                      {
                        nextLine.Z = num16 + num8 + CurrentAdjustmentsZ(ref wave_step);
                      }
                    }
                    nextLine.E += num4;
                  }
                  num5 = num25;
                }
              }
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
              flag1 = false;
            }
            else if (nextLine.HasG && nextLine.G == 91)
            {
              flag1 = true;
            }

            output_writer.Write(nextLine);
            ++num2;
          }
        }
        else
        {
          break;
        }
      }
      return true;
    }

    private float CurrentAdjustmentsZ(ref int wave_step)
    {
      var num1 = wave_step != 0 ? (wave_step != 2 ? 0.0 : -1.5) : 1.0;
      wave_step = (wave_step + 1) % 4;
      var num2 = 0.150000005960464;
      return (float) (num1 * num2);
    }

    private void ProcessForTackPoints(GCodeFileWriter output_writer, GCode currLine, GCode prevLine, ref GCode Last_Tack_Point, ref int cornercount)
    {
      if (cornercount <= 1)
      {
        if (!isSharpCorner(currLine, prevLine))
        {
          return;
        }

        if (Last_Tack_Point == null)
        {
          doTackPoint(currLine, prevLine, output_writer);
        }

        Last_Tack_Point = currLine;
        ++cornercount;
      }
      else
      {
        if (cornercount < 1 || !isSharpCorner(currLine, Last_Tack_Point))
        {
          return;
        }

        doTackPoint(currLine, Last_Tack_Point, output_writer);
        Last_Tack_Point = currLine;
      }
    }

    public double distance(BondingPreprocessor.Vector2 A, BondingPreprocessor.Vector2 B)
    {
      return Math.Sqrt(Math.Pow(A.x - (double)B.x, 2.0) + Math.Pow(A.y - (double)B.y, 2.0));
    }

    public bool isSharpCorner(GCode currLine, GCode prevLine)
    {
      var flag = false;
      var dot1 = new BondingPreprocessor.Vector2(currLine);
      var dot2 = new BondingPreprocessor.Vector2(prevLine);
      var num1 = Math.Pow(dot1.Dot(dot1), 2.0);
      var num2 = Math.Pow(dot2.Dot(dot2), 2.0);
      var num3 = Math.Acos(dot1.Dot(dot2) / (num1 * num2));
      if (num3 > 0.0 && num3 < 1.57079633)
      {
        flag = true;
      }

      return flag;
    }

    public void doTackPoint(GCode currLine, GCode Last_Tack_Point, GCodeFileWriter output_writer)
    {
      var num = (int) Math.Ceiling(distance(new BondingPreprocessor.Vector2(currLine), new BondingPreprocessor.Vector2(Last_Tack_Point)));
      if (num <= 5)
      {
        return;
      }

      output_writer.Write(new GCode("G4 P" + num.ToString()));
    }

    public class Vector2
    {
      public float x;
      public float y;

      public Vector2(GCode G)
      {
        x = G.X;
        y = G.Y;
      }

      public float Dot(BondingPreprocessor.Vector2 dot)
      {
        return x * dot.x + y + dot.y;
      }
    }
  }
}
