// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.BacklashCalibrationPrint
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Preprocessors.Foundation;
using System;
using System.IO;
using System.Text;

namespace M3D.Spooling.Common.Utils
{
  internal static class BacklashCalibrationPrint
  {
    private const int backlashSpeed = 500;
    private const int printSpeed = 500;

    public static void Create(string filename, FilamentSpool.TypeEnum filament_type, float XStart, float YStart)
    {
      int num1 = 215;
      switch (filament_type)
      {
        case FilamentSpool.TypeEnum.ABS:
          num1 = 270;
          break;
        case FilamentSpool.TypeEnum.HIPS:
          num1 = 260;
          break;
        case FilamentSpool.TypeEnum.ABS_R:
          num1 = 240;
          break;
      }
      BacklashCalibrationPrint.Direction dirXPrev = BacklashCalibrationPrint.Direction.Neither;
      BacklashCalibrationPrint.Direction dirYPrev = BacklashCalibrationPrint.Direction.Neither;
      using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
      {
        using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
        {
          streamWriter.WriteLine("G90");
          streamWriter.WriteLine("M104 S" + (object) num1);
          streamWriter.WriteLine("G28");
          streamWriter.WriteLine("G0 Z2 F60");
          streamWriter.WriteLine("M109 S" + (object) num1);
          switch (filament_type)
          {
            case FilamentSpool.TypeEnum.ABS:
              streamWriter.WriteLine("M106 S1");
              break;
            case FilamentSpool.TypeEnum.PLA:
            case FilamentSpool.TypeEnum.FLX:
            case FilamentSpool.TypeEnum.TGH:
              streamWriter.WriteLine("M106 S255");
              break;
          }
          float backlashCompensationX = 0.0f;
          float backlashCompensationY = 0.0f;
          int points1 = 4;
          float num2 = 8f;
          float layerHeight = 0.2f;
          Position pos = new Position();
          pos.relativeX = XStart;
          pos.relativeY = YStart;
          pos.relativeZ = 0.4f;
          streamWriter.WriteLine(PrinterCompatibleString.Format("G0 X{0:0.0000} Y{1:0.0000} F500", (object) XStart, (object) YStart));
          bool isFirstMove = false;
          for (int index1 = 0; index1 < 5; ++index1)
          {
            streamWriter.WriteLine(PrinterCompatibleString.Format("G0 Z{0:0.000}", (object) pos.relativeZ));
            pos.relativeZ += 0.2f;
            for (int index2 = 0; index2 < 5; ++index2)
            {
              float EXTRUSION_PER_MM_XY;
              switch (index1)
              {
                case 0:
                  EXTRUSION_PER_MM_XY = 0.2508f;
                  break;
                case 1:
                  EXTRUSION_PER_MM_XY = 0.0528f;
                  break;
                default:
                  EXTRUSION_PER_MM_XY = 0.044f;
                  break;
              }
              string polynomialGcode = BacklashCalibrationPrint.GetPolynomialGCode(ref backlashCompensationX, ref backlashCompensationY, ref dirXPrev, ref dirYPrev, XStart, YStart, pos, points1, num2 - (float) index2, 0.25f, 1.06f, 0.0f, EXTRUSION_PER_MM_XY, isFirstMove);
              isFirstMove = false;
              streamWriter.WriteLine(polynomialGcode);
            }
          }
          streamWriter.WriteLine("M106 s255");
          int points2 = 96;
          float radius = 5f;
          int num3 = (int) (Math.Ceiling(30.0 / (double) layerHeight) + 0.100000001490116);
          float num4 = 1f;
          float num5 = 2f;
          float num6 = num4 / (float) num3;
          float num7 = num5 / (float) num3;
          float X_BACKLASH = 0.0f;
          float Y_BACKLASH = 0.0f;
          int num8 = 0;
          while ((double) X_BACKLASH < (double) num4 && (double) Y_BACKLASH < (double) num5)
          {
            streamWriter.WriteLine(PrinterCompatibleString.Format("G0 Z{0:0.000}", (object) pos.relativeZ));
            string polynomialGcode = BacklashCalibrationPrint.GetPolynomialGCode(ref backlashCompensationX, ref backlashCompensationY, ref dirXPrev, ref dirYPrev, XStart, YStart, pos, points2, radius, X_BACKLASH, Y_BACKLASH, layerHeight, 0.0369f, false);
            streamWriter.WriteLine(polynomialGcode);
            X_BACKLASH += num6;
            Y_BACKLASH += num7;
            ++num8;
          }
          pos.relativeZ += 4f;
          streamWriter.WriteLine("M104 S0");
          streamWriter.WriteLine(";PrinterJob");
          streamWriter.WriteLine(PrinterCompatibleString.Format("G0 Z{0:0.000}", (object) pos.relativeZ));
          streamWriter.WriteLine("G91");
          streamWriter.WriteLine("G0 X20 Y20");
          streamWriter.WriteLine("M18");
        }
      }
    }

    private static string GetPolynomialGCode(ref float backlashCompensationX, ref float backlashCompensationY, ref BacklashCalibrationPrint.Direction dirXPrev, ref BacklashCalibrationPrint.Direction dirYPrev, float XStart, float YStart, Position pos, int points, float radius, float X_BACKLASH, float Y_BACKLASH, float layerHeight, float EXTRUSION_PER_MM_XY, bool isFirstMove)
    {
      StringBuilder stringBuilder = new StringBuilder();
      float num1 = (float) (360.0 / (double) points * (Math.PI / 180.0));
      float num2 = layerHeight / (float) points;
      for (int index = 0; index < points; ++index)
      {
        float num3 = (float) (45.0 + (double) index * (double) num1);
        double num4 = (double) XStart + (double) radius * Math.Cos((double) num3);
        double num5 = (double) YStart + (double) radius * Math.Sin((double) num3);
        float num6 = (float) num4 - pos.relativeX;
        float num7 = (float) num5 - pos.relativeY;
        float num8 = (float) Math.Sqrt((double) num6 * (double) num6 + (double) num7 * (double) num7) * EXTRUSION_PER_MM_XY;
        if (isFirstMove)
        {
          isFirstMove = false;
          num8 = 0.0f;
        }
        pos.relativeE += num8;
        pos.absoluteE += num8;
        pos.relativeZ += num2;
        BacklashCalibrationPrint.Direction direction1 = (double) num6 <= 1.40129846432482E-45 ? ((double) num6 >= -1.40129846432482E-45 ? dirXPrev : BacklashCalibrationPrint.Direction.Negative) : BacklashCalibrationPrint.Direction.Positive;
        BacklashCalibrationPrint.Direction direction2 = (double) num7 <= 1.40129846432482E-45 ? ((double) num7 >= -1.40129846432482E-45 ? dirYPrev : BacklashCalibrationPrint.Direction.Negative) : BacklashCalibrationPrint.Direction.Positive;
        if (direction1 != dirXPrev && dirXPrev != BacklashCalibrationPrint.Direction.Neither || direction2 != dirYPrev && dirYPrev != BacklashCalibrationPrint.Direction.Neither)
        {
          if (direction1 != dirXPrev && dirXPrev != BacklashCalibrationPrint.Direction.Neither)
            backlashCompensationX += direction1 == BacklashCalibrationPrint.Direction.Positive ? X_BACKLASH : -X_BACKLASH;
          if (direction2 != dirYPrev && dirYPrev != BacklashCalibrationPrint.Direction.Neither)
            backlashCompensationY += direction2 == BacklashCalibrationPrint.Direction.Positive ? Y_BACKLASH : -Y_BACKLASH;
          float num9 = pos.relativeX + backlashCompensationX;
          float num10 = pos.relativeY + backlashCompensationY;
          stringBuilder.AppendLine(PrinterCompatibleString.Format("G0 X{0:0.0000} Y{1:0.0000} F{2} ;X_BACKLASH:{3:0.000} ;Y_BACKLASH:{4:0.000}", (object) num9, (object) num10, (object) 500, (object) X_BACKLASH, (object) Y_BACKLASH));
        }
        float num11 = (float) num4 + backlashCompensationX;
        float num12 = (float) num5 + backlashCompensationY;
        stringBuilder.AppendLine(PrinterCompatibleString.Format("G0 X{0:0.0000} Y{1:0.0000} Z{2:0.0000} E{3:0.0000} F{4}", (object) num11, (object) num12, (object) pos.relativeZ, (object) pos.relativeE, (object) 500));
        pos.relativeX += num6;
        pos.relativeY += num7;
        pos.absoluteX += num6;
        pos.absoluteY += num7;
        dirXPrev = direction1;
        dirYPrev = direction2;
      }
      return stringBuilder.ToString();
    }

    private enum Direction
    {
      Positive,
      Negative,
      Neither,
    }
  }
}
