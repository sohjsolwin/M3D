using M3D.Spooling.Common;
using M3D.Spooling.Common.Types;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M3D.Spooling.Preprocessors
{
  public class BedCompensationPreprocessor : IPreprocessor
  {
    public const int PrintTestBorderID = 255;
    private Vector backRight;
    private Vector backLeft;
    private Vector frontLeft;
    private Vector frontRight;
    private Vector center;
    public const float CHANGE_IN_HEIGHT_THAT_DOUBLES_EXTRUSION = 0.15f;
    public const float PROBE_Z_DISTANCE = 55f;
    public const float SEGMENT_LENGTH = 2f;
    public const float CALIBRATION_OFFSET_FOR_UNKNOWN_DYNAMIC = 0.0f;
    public const bool moveZToCompensate = true;
    public const bool firstLayerOnly = false;
    public float corner_height_back_right;
    public float corner_height_back_left;
    public float corner_height_front_left;
    public float corner_height_front_right;
    public int g32_version;
    public float entire_z_height_offset;

    public override string Name
    {
      get
      {
        return "bed";
      }
    }

    public Vector BackRight
    {
      get
      {
        return new Vector(backRight);
      }
    }

    public Vector BackLeft
    {
      get
      {
        return new Vector(backLeft);
      }
    }

    public Vector FrontLeft
    {
      get
      {
        return new Vector(frontLeft);
      }
    }

    public Vector FrontRight
    {
      get
      {
        return new Vector(frontRight);
      }
    }

    public Vector Center
    {
      get
      {
        return new Vector(center);
      }
    }

    public void UpdateConfigurations(Calibration calibration, PrinterSizeProfile sizeprofile)
    {
      UpdateConfigurations(calibration, sizeprofile, false);
    }

    public void UpdateConfigurations(Calibration calibration, PrinterSizeProfile sizeprofile, bool bUseSpecialPrintTestPoints)
    {
      entire_z_height_offset = calibration.ENTIRE_Z_HEIGHT_OFFSET;
      corner_height_back_right = calibration.CORNER_HEIGHT_BACK_RIGHT + calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      corner_height_back_left = calibration.CORNER_HEIGHT_BACK_LEFT + calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      corner_height_front_left = calibration.CORNER_HEIGHT_FRONT_LEFT + calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      corner_height_front_right = calibration.CORNER_HEIGHT_FRONT_RIGHT + calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      g32_version = calibration.G32_VERSION;
      RectCoordinates rectCoordinates = !bUseSpecialPrintTestPoints || !sizeprofile.G32ProbePoints.ContainsKey((int) byte.MaxValue) ? (!sizeprofile.G32ProbePoints.ContainsKey(g32_version) ? sizeprofile.G32ProbePoints.First<KeyValuePair<int, RectCoordinates>>().Value : sizeprofile.G32ProbePoints[g32_version]) : sizeprofile.G32ProbePoints[(int) byte.MaxValue];
      backRight = new Vector(rectCoordinates.right, rectCoordinates.back, corner_height_back_right);
      backLeft = new Vector(rectCoordinates.left, rectCoordinates.back, corner_height_back_left);
      frontLeft = new Vector(rectCoordinates.left, rectCoordinates.front, corner_height_front_left);
      frontRight = new Vector(rectCoordinates.right, rectCoordinates.front, corner_height_front_right);
      center = new Vector(sizeprofile.HomeLocation.x, sizeprofile.HomeLocation.y, 0.0f);
    }

    internal override bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      UpdateConfigurations(calibration, printerProfile.PrinterSizeConstants);
      var flag1 = true;
      var flag2 = true;
      var flag3 = false;
      var flag4 = false;
      var num1 = 0;
      var num2 = 0;
      var num3 = 0.0;
      var position = new Position();
      var num4 = (float) num3;
      var num5 = 0.0f;
      while (true)
      {
        GCode nextLine = input_reader.GetNextLine(false);
        if (nextLine != null)
        {
          if (nextLine.hasG && (nextLine.G == (ushort) 0 || nextLine.G == (ushort) 1) && !flag1)
          {
            if (nextLine.hasX || nextLine.hasY)
            {
              flag4 = true;
            }

            if (nextLine.hasZ)
            {
              nextLine.Z += entire_z_height_offset;
            }

            var num6 = !nextLine.hasX ? 0.0f : nextLine.X - position.relativeX;
            var num7 = !nextLine.hasY ? 0.0f : nextLine.Y - position.relativeY;
            var num8 = !nextLine.hasZ ? 0.0f : nextLine.Z - position.relativeZ;
            var num9 = !nextLine.hasE ? 0.0f : nextLine.E - position.relativeE;
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

            if ((double) num8 > 1.40129846432482E-45 || (double) num8 < -1.40129846432482E-45)
            {
              if (!flag3)
              {
                num2 = 1;
              }
              else
              {
                ++num2;
              }

              flag2 = num2 == 0 || num2 == 1;
            }
            var num10 = (float) Math.Sqrt((double) num6 * (double) num6 + (double) num7 * (double) num7);
            var num11 = 1;
            if ((double) num10 > 2.0)
            {
              num11 = (int) ((double) num10 / 2.0);
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
            if ((double) num9 > 0.0)
            {
              flag3 = true;
            }

            var num22 = flag2 ? 1 : 0;
            if ((double) num9 > 0.0)
            {
              for (var index = 1; index < num11 + 1; ++index)
              {
                float x;
                float y;
                float num23;
                float num24;
                float num25;
                float num26;
                if (index == num11)
                {
                  x = position.absoluteX;
                  y = position.absoluteY;
                  num23 = position.relativeX;
                  num24 = position.relativeY;
                  num25 = position.relativeZ;
                  num26 = position.relativeE;
                }
                else
                {
                  x = num12 + (float) index * 2f * num18;
                  y = num13 + (float) index * 2f * num19;
                  num23 = num14 + (float) index * 2f * num18;
                  num24 = num15 + (float) index * 2f * num19;
                  num25 = num16 + (float) index * 2f * num20;
                  num26 = num17 + (float) index * 2f * num21;
                }
                var adjustmentRequired = GetHeightAdjustmentRequired(x, y);
                if (index != num11)
                {
                  var code = new GCode
                  {
                    G = nextLine.G
                  };
                  if (nextLine.hasX)
                  {
                    code.X = (float) ((double) position.relativeX - (double) num6 + ((double) num23 - (double) num14));
                  }

                  if (nextLine.hasY)
                  {
                    code.Y = (float) ((double) position.relativeY - (double) num7 + ((double) num24 - (double) num15));
                  }

                  if (nextLine.hasF && index == 1)
                  {
                    code.F = nextLine.F;
                  }

                  if (flag4)
                  {
                    code.Z = (float) ((double) position.relativeZ - (double) num8 + ((double) num25 - (double) num16)) + adjustmentRequired;
                  }
                  else if (nextLine.hasZ && ((double) num8 > 1.40129846432482E-45 || (double) num8 < -1.40129846432482E-45))
                  {
                    code.Z = (float) ((double) position.relativeZ - (double) num8 + ((double) num25 - (double) num16));
                  }

                  code.E = (float) ((double) position.relativeE - (double) num9 + ((double) num26 - (double) num17)) + num4;
                  output_writer.Write(code);
                }
                else
                {
                  if (flag4)
                  {
                    if (nextLine.hasZ)
                    {
                      nextLine.Z += adjustmentRequired;
                    }
                    else
                    {
                      nextLine.Z = num16 + num8 + adjustmentRequired;
                    }
                  }
                  nextLine.E += num4;
                }
                num5 = num26;
              }
            }
            else
            {
              if (flag4)
              {
                var num23 = flag2 ? 1 : 0;
                var adjustmentRequired = GetHeightAdjustmentRequired(position.absoluteX, position.absoluteY);
                if (nextLine.hasZ)
                {
                  nextLine.Z += adjustmentRequired;
                }
                else
                {
                  nextLine.Z = position.relativeZ + adjustmentRequired;
                }
              }
              if (nextLine.hasE)
              {
                nextLine.E += num4;
              }

              num5 = position.relativeE;
            }
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
            flag1 = false;
          }
          else if (nextLine.hasG && nextLine.G == (ushort) 91)
          {
            flag1 = true;
          }
          else if (nextLine.hasG && nextLine.G == (ushort) 28)
          {
            position.relativeX = position.absoluteX = 54f;
            position.relativeY = position.absoluteY = 50f;
          }
          output_writer.Write(nextLine);
          ++num1;
        }
        else
        {
          break;
        }
      }
      return true;
    }

    public float GetHeightAdjustmentRequired(float x, float y)
    {
      if (!false)
      {
        var num = 2;
        var length = 2;
        Vector[][] vectorArray = new Vector[2][];
        for (var index = 0; index < num; ++index)
        {
          vectorArray[index] = new Vector[length];
        }

        vectorArray[1][1] = backRight;
        vectorArray[1][0] = backLeft;
        vectorArray[0][1] = frontRight;
        vectorArray[0][0] = frontLeft;
        Vector planeEquation1 = generatePlaneEquation(backLeft, backRight, center);
        Vector planeEquation2 = generatePlaneEquation(backLeft, frontLeft, center);
        Vector planeEquation3 = generatePlaneEquation(backRight, frontRight, center);
        Vector planeEquation4 = generatePlaneEquation(frontLeft, frontRight, center);
        var x1 = frontLeft.x;
        var x2 = frontRight.x;
        var y1 = frontLeft.y;
        var y2 = backRight.y;
        var vector = new Vector(x, y, 0.0f);
        if ((double) x <= (double) x1 && (double) y >= (double) y2)
        {
          vector.z = (float) (((double)GetZFromXYAndPlane(vector, planeEquation1) + (double)GetZFromXYAndPlane(vector, planeEquation2)) / 2.0);
        }
        else if ((double) x <= (double) x1 && (double) y <= (double) y1)
        {
          vector.z = (float) (((double)GetZFromXYAndPlane(vector, planeEquation4) + (double)GetZFromXYAndPlane(vector, planeEquation2)) / 2.0);
        }
        else if ((double) x >= (double) x2 && (double) y <= (double) y1)
        {
          vector.z = (float) (((double)GetZFromXYAndPlane(vector, planeEquation4) + (double)GetZFromXYAndPlane(vector, planeEquation3)) / 2.0);
        }
        else if ((double) x >= (double) x2 && (double) y >= (double) y2)
        {
          vector.z = (float) (((double)GetZFromXYAndPlane(vector, planeEquation1) + (double)GetZFromXYAndPlane(vector, planeEquation3)) / 2.0);
        }
        else if ((double) x <= (double) x1)
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation2);
        }
        else if ((double) x >= (double) x2)
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation3);
        }
        else if ((double) y >= (double) y2)
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation1);
        }
        else if ((double) y <= (double) y1)
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation4);
        }
        else if (IsPointInTriangle(vector, center, frontLeft, backLeft))
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation2);
        }
        else if (IsPointInTriangle(vector, center, frontRight, backRight))
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation3);
        }
        else if (IsPointInTriangle(vector, center, backLeft, backRight))
        {
          vector.z = GetZFromXYAndPlane(vector, planeEquation1);
        }
        else
        {
          if (!IsPointInTriangle(vector, center, frontLeft, frontRight))
          {
            throw new Exception("not possible");
          }

          vector.z = GetZFromXYAndPlane(vector, planeEquation4);
        }
        return vector.z + 0.0f;
      }
      float num1;
      float num2;
      if (g32_version == 1)
      {
        num1 = 90f;
        num2 = 90f;
      }
      else
      {
        num1 = 109f;
        num2 = 103f;
      }
      var num3 = ((double)corner_height_back_left - (double)corner_height_front_left) / (double) num1;
      var num4 = (corner_height_back_right - corner_height_front_right) / num2;
      var num5 = (double) y - (double)frontLeft.y;
      var num6 = (float) (num3 * num5) + corner_height_front_left;
      return (float) (((double) num4 * ((double) y - (double)frontLeft.y) + (double)corner_height_front_right - (double) num6) / (double) num1 * ((double) x - (double)frontLeft.x) + (double) num6 + 0.0);
    }

    public float GetZFromXYAndPlane(Vector point, Vector planeABC)
    {
      var num1 = (double) planeABC[0];
      var num2 = planeABC[1];
      var num3 = planeABC[2];
      var num4 = planeABC[3];
      var x = (double) point.x;
      return (float) ((num1 * x + (double) num2 * (double) point.y + (double) num4) / -(double) num3);
    }

    public Vector CalculatePlaneNormalVector(Vector v1, Vector v2, Vector v3)
    {
      Vector vector1 = v2 - v1;
      Vector vector2 = v3 - v1;
      return new Vector() { [0] = (float) ((double) vector1[1] * (double) vector2[2] - (double) vector2[1] * (double) vector1[2]), [1] = (float) ((double) vector1[2] * (double) vector2[0] - (double) vector2[2] * (double) vector1[0]), [2] = (float) ((double) vector1[0] * (double) vector2[1] - (double) vector2[0] * (double) vector1[1]) };
    }

    private Vector generatePlaneEquation(Vector v1, Vector v2, Vector v3)
    {
      var vector = new Vector();
      Vector planeNormalVector = CalculatePlaneNormalVector(v1, v2, v3);
      vector[0] = planeNormalVector[0];
      vector[1] = planeNormalVector[1];
      vector[2] = planeNormalVector[2];
      vector[3] = (float) -((double) vector[0] * (double) v1[0] + (double) vector[1] * (double) v1[1] + (double) vector[2] * (double) v1[2]);
      return vector;
    }

    private float sign(Vector p1, Vector p2, Vector p3)
    {
      return (float) (((double) p1.x - (double) p3.x) * ((double) p2.y - (double) p3.y) - ((double) p2.x - (double) p3.x) * ((double) p1.y - (double) p3.y));
    }

    private bool IsPointInTriangle(Vector pt, Vector v1, Vector v2, Vector v3)
    {
      var num1 = 0.01f;
      Vector vector1 = v1 - v2 + (v1 - v3);
      vector1.Normalize();
      Vector vector2 = v1 + vector1 * num1;
      Vector vector3 = v2 - v1 + (v2 - v3);
      vector3.Normalize();
      Vector vector4 = v2 + vector3 * num1;
      Vector vector5 = v3 - v1 + (v3 - v2);
      vector5.Normalize();
      Vector vector6 = v3 + vector5 * num1;
      var num2 = (double)sign(pt, vector2, vector4) < 0.0 ? 1 : 0;
      var flag1 = (double)sign(pt, vector4, vector6) < 0.0;
      var flag2 = (double)sign(pt, vector6, vector2) < 0.0;
      var num3 = flag1 ? 1 : 0;
      if (num2 == num3)
      {
        return flag1 == flag2;
      }

      return false;
    }
  }
}
