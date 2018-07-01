// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Preprocessors.BedCompensationPreprocessor
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
        return new Vector(this.backRight);
      }
    }

    public Vector BackLeft
    {
      get
      {
        return new Vector(this.backLeft);
      }
    }

    public Vector FrontLeft
    {
      get
      {
        return new Vector(this.frontLeft);
      }
    }

    public Vector FrontRight
    {
      get
      {
        return new Vector(this.frontRight);
      }
    }

    public Vector Center
    {
      get
      {
        return new Vector(this.center);
      }
    }

    public void UpdateConfigurations(Calibration calibration, PrinterSizeProfile sizeprofile)
    {
      this.UpdateConfigurations(calibration, sizeprofile, false);
    }

    public void UpdateConfigurations(Calibration calibration, PrinterSizeProfile sizeprofile, bool bUseSpecialPrintTestPoints)
    {
      this.entire_z_height_offset = calibration.ENTIRE_Z_HEIGHT_OFFSET;
      this.corner_height_back_right = calibration.CORNER_HEIGHT_BACK_RIGHT + calibration.CORNER_HEIGHT_BACK_RIGHT_OFFSET;
      this.corner_height_back_left = calibration.CORNER_HEIGHT_BACK_LEFT + calibration.CORNER_HEIGHT_BACK_LEFT_OFFSET;
      this.corner_height_front_left = calibration.CORNER_HEIGHT_FRONT_LEFT + calibration.CORNER_HEIGHT_FRONT_LEFT_OFFSET;
      this.corner_height_front_right = calibration.CORNER_HEIGHT_FRONT_RIGHT + calibration.CORNER_HEIGHT_FRONT_RIGHT_OFFSET;
      this.g32_version = calibration.G32_VERSION;
      RectCoordinates rectCoordinates = !bUseSpecialPrintTestPoints || !sizeprofile.G32ProbePoints.ContainsKey((int) byte.MaxValue) ? (!sizeprofile.G32ProbePoints.ContainsKey(this.g32_version) ? sizeprofile.G32ProbePoints.First<KeyValuePair<int, RectCoordinates>>().Value : sizeprofile.G32ProbePoints[this.g32_version]) : sizeprofile.G32ProbePoints[(int) byte.MaxValue];
      this.backRight = new Vector(rectCoordinates.right, rectCoordinates.back, this.corner_height_back_right);
      this.backLeft = new Vector(rectCoordinates.left, rectCoordinates.back, this.corner_height_back_left);
      this.frontLeft = new Vector(rectCoordinates.left, rectCoordinates.front, this.corner_height_front_left);
      this.frontRight = new Vector(rectCoordinates.right, rectCoordinates.front, this.corner_height_front_right);
      this.center = new Vector(sizeprofile.HomeLocation.x, sizeprofile.HomeLocation.y, 0.0f);
    }

    internal override bool ProcessGCode(GCodeFileReader input_reader, GCodeFileWriter output_writer, Calibration calibration, JobDetails jobdetails, InternalPrinterProfile printerProfile)
    {
      this.UpdateConfigurations(calibration, printerProfile.PrinterSizeConstants);
      bool flag1 = true;
      bool flag2 = true;
      bool flag3 = false;
      bool flag4 = false;
      int num1 = 0;
      int num2 = 0;
      double num3 = 0.0;
      Position position = new Position();
      float num4 = (float) num3;
      float num5 = 0.0f;
      while (true)
      {
        GCode nextLine = input_reader.GetNextLine(false);
        if (nextLine != null)
        {
          if (nextLine.hasG && (nextLine.G == (ushort) 0 || nextLine.G == (ushort) 1) && !flag1)
          {
            if (nextLine.hasX || nextLine.hasY)
              flag4 = true;
            if (nextLine.hasZ)
              nextLine.Z += this.entire_z_height_offset;
            float num6 = !nextLine.hasX ? 0.0f : nextLine.X - position.relativeX;
            float num7 = !nextLine.hasY ? 0.0f : nextLine.Y - position.relativeY;
            float num8 = !nextLine.hasZ ? 0.0f : nextLine.Z - position.relativeZ;
            float num9 = !nextLine.hasE ? 0.0f : nextLine.E - position.relativeE;
            position.absoluteX += num6;
            position.absoluteY += num7;
            position.absoluteZ += num8;
            position.absoluteE += num9;
            position.relativeX += num6;
            position.relativeY += num7;
            position.relativeZ += num8;
            position.relativeE += num9;
            if (nextLine.hasF)
              position.F = nextLine.F;
            if ((double) num8 > 1.40129846432482E-45 || (double) num8 < -1.40129846432482E-45)
            {
              if (!flag3)
                num2 = 1;
              else
                ++num2;
              flag2 = num2 == 0 || num2 == 1;
            }
            float num10 = (float) Math.Sqrt((double) num6 * (double) num6 + (double) num7 * (double) num7);
            int num11 = 1;
            if ((double) num10 > 2.0)
              num11 = (int) ((double) num10 / 2.0);
            float num12 = position.absoluteX - num6;
            float num13 = position.absoluteY - num7;
            float num14 = position.relativeX - num6;
            float num15 = position.relativeY - num7;
            float num16 = position.relativeZ - num8;
            float num17 = position.relativeE - num9;
            float num18 = num6 / num10;
            float num19 = num7 / num10;
            float num20 = num8 / num10;
            float num21 = num9 / num10;
            if ((double) num9 > 0.0)
              flag3 = true;
            int num22 = flag2 ? 1 : 0;
            if ((double) num9 > 0.0)
            {
              for (int index = 1; index < num11 + 1; ++index)
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
                float adjustmentRequired = this.GetHeightAdjustmentRequired(x, y);
                if (index != num11)
                {
                  GCode code = new GCode();
                  code.G = nextLine.G;
                  if (nextLine.hasX)
                    code.X = (float) ((double) position.relativeX - (double) num6 + ((double) num23 - (double) num14));
                  if (nextLine.hasY)
                    code.Y = (float) ((double) position.relativeY - (double) num7 + ((double) num24 - (double) num15));
                  if (nextLine.hasF && index == 1)
                    code.F = nextLine.F;
                  if (flag4)
                    code.Z = (float) ((double) position.relativeZ - (double) num8 + ((double) num25 - (double) num16)) + adjustmentRequired;
                  else if (nextLine.hasZ && ((double) num8 > 1.40129846432482E-45 || (double) num8 < -1.40129846432482E-45))
                    code.Z = (float) ((double) position.relativeZ - (double) num8 + ((double) num25 - (double) num16));
                  code.E = (float) ((double) position.relativeE - (double) num9 + ((double) num26 - (double) num17)) + num4;
                  output_writer.Write(code);
                }
                else
                {
                  if (flag4)
                  {
                    if (nextLine.hasZ)
                      nextLine.Z += adjustmentRequired;
                    else
                      nextLine.Z = num16 + num8 + adjustmentRequired;
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
                int num23 = flag2 ? 1 : 0;
                float adjustmentRequired = this.GetHeightAdjustmentRequired(position.absoluteX, position.absoluteY);
                if (nextLine.hasZ)
                  nextLine.Z += adjustmentRequired;
                else
                  nextLine.Z = position.relativeZ + adjustmentRequired;
              }
              if (nextLine.hasE)
                nextLine.E += num4;
              num5 = position.relativeE;
            }
          }
          else if (nextLine.hasG && nextLine.G == (ushort) 92)
          {
            if (nextLine.hasE)
              position.relativeE = nextLine.E;
            if (printerProfile.OptionsConstants.G92WorksOnAllAxes)
            {
              if (nextLine.hasX)
                position.relativeX = nextLine.X;
              if (nextLine.hasY)
                position.relativeY = nextLine.Y;
              if (nextLine.hasZ)
                position.relativeZ = nextLine.Z;
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
            flag1 = false;
          else if (nextLine.hasG && nextLine.G == (ushort) 91)
            flag1 = true;
          else if (nextLine.hasG && nextLine.G == (ushort) 28)
          {
            position.relativeX = position.absoluteX = 54f;
            position.relativeY = position.absoluteY = 50f;
          }
          output_writer.Write(nextLine);
          ++num1;
        }
        else
          break;
      }
      return true;
    }

    public float GetHeightAdjustmentRequired(float x, float y)
    {
      if (!false)
      {
        int num = 2;
        int length = 2;
        Vector[][] vectorArray = new Vector[2][];
        for (int index = 0; index < num; ++index)
          vectorArray[index] = new Vector[length];
        vectorArray[1][1] = this.backRight;
        vectorArray[1][0] = this.backLeft;
        vectorArray[0][1] = this.frontRight;
        vectorArray[0][0] = this.frontLeft;
        Vector planeEquation1 = this.generatePlaneEquation(this.backLeft, this.backRight, this.center);
        Vector planeEquation2 = this.generatePlaneEquation(this.backLeft, this.frontLeft, this.center);
        Vector planeEquation3 = this.generatePlaneEquation(this.backRight, this.frontRight, this.center);
        Vector planeEquation4 = this.generatePlaneEquation(this.frontLeft, this.frontRight, this.center);
        float x1 = this.frontLeft.x;
        float x2 = this.frontRight.x;
        float y1 = this.frontLeft.y;
        float y2 = this.backRight.y;
        Vector vector = new Vector(x, y, 0.0f);
        if ((double) x <= (double) x1 && (double) y >= (double) y2)
          vector.z = (float) (((double) this.GetZFromXYAndPlane(vector, planeEquation1) + (double) this.GetZFromXYAndPlane(vector, planeEquation2)) / 2.0);
        else if ((double) x <= (double) x1 && (double) y <= (double) y1)
          vector.z = (float) (((double) this.GetZFromXYAndPlane(vector, planeEquation4) + (double) this.GetZFromXYAndPlane(vector, planeEquation2)) / 2.0);
        else if ((double) x >= (double) x2 && (double) y <= (double) y1)
          vector.z = (float) (((double) this.GetZFromXYAndPlane(vector, planeEquation4) + (double) this.GetZFromXYAndPlane(vector, planeEquation3)) / 2.0);
        else if ((double) x >= (double) x2 && (double) y >= (double) y2)
          vector.z = (float) (((double) this.GetZFromXYAndPlane(vector, planeEquation1) + (double) this.GetZFromXYAndPlane(vector, planeEquation3)) / 2.0);
        else if ((double) x <= (double) x1)
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation2);
        else if ((double) x >= (double) x2)
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation3);
        else if ((double) y >= (double) y2)
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation1);
        else if ((double) y <= (double) y1)
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation4);
        else if (this.IsPointInTriangle(vector, this.center, this.frontLeft, this.backLeft))
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation2);
        else if (this.IsPointInTriangle(vector, this.center, this.frontRight, this.backRight))
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation3);
        else if (this.IsPointInTriangle(vector, this.center, this.backLeft, this.backRight))
        {
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation1);
        }
        else
        {
          if (!this.IsPointInTriangle(vector, this.center, this.frontLeft, this.frontRight))
            throw new Exception("not possible");
          vector.z = this.GetZFromXYAndPlane(vector, planeEquation4);
        }
        return vector.z + 0.0f;
      }
      float num1;
      float num2;
      if (this.g32_version == 1)
      {
        num1 = 90f;
        num2 = 90f;
      }
      else
      {
        num1 = 109f;
        num2 = 103f;
      }
      double num3 = ((double) this.corner_height_back_left - (double) this.corner_height_front_left) / (double) num1;
      float num4 = (this.corner_height_back_right - this.corner_height_front_right) / num2;
      double num5 = (double) y - (double) this.frontLeft.y;
      float num6 = (float) (num3 * num5) + this.corner_height_front_left;
      return (float) (((double) num4 * ((double) y - (double) this.frontLeft.y) + (double) this.corner_height_front_right - (double) num6) / (double) num1 * ((double) x - (double) this.frontLeft.x) + (double) num6 + 0.0);
    }

    public float GetZFromXYAndPlane(Vector point, Vector planeABC)
    {
      double num1 = (double) planeABC[0];
      float num2 = planeABC[1];
      float num3 = planeABC[2];
      float num4 = planeABC[3];
      double x = (double) point.x;
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
      Vector vector = new Vector();
      Vector planeNormalVector = this.CalculatePlaneNormalVector(v1, v2, v3);
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
      float num1 = 0.01f;
      Vector vector1 = v1 - v2 + (v1 - v3);
      vector1.Normalize();
      Vector vector2 = v1 + vector1 * num1;
      Vector vector3 = v2 - v1 + (v2 - v3);
      vector3.Normalize();
      Vector vector4 = v2 + vector3 * num1;
      Vector vector5 = v3 - v1 + (v3 - v2);
      vector5.Normalize();
      Vector vector6 = v3 + vector5 * num1;
      int num2 = (double) this.sign(pt, vector2, vector4) < 0.0 ? 1 : 0;
      bool flag1 = (double) this.sign(pt, vector4, vector6) < 0.0;
      bool flag2 = (double) this.sign(pt, vector6, vector2) < 0.0;
      int num3 = flag1 ? 1 : 0;
      if (num2 == num3)
        return flag1 == flag2;
      return false;
    }
  }
}
