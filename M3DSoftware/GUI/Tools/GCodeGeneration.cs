// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Tools.GCodeGeneration
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;

namespace M3D.GUI.Tools
{
  public static class GCodeGeneration
  {
    public static List<string> CreatePrintTestBorder(float x1, float y1, float x2, float y2, float flow_rate, float temperature, float filament_diameter)
    {
      List<string> stringList = new List<string>();
      float num1 = 0.0f;
      stringList.Add("M106");
      stringList.Add(PrinterCompatibleString.Format("M109 S{0}", (object) temperature));
      stringList.Add(PrinterCompatibleString.Format(";ideal temp:{0}", (object) temperature));
      stringList.Add("G90");
      stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} Z0.15 F900", (object) x1, (object) y1));
      float num2 = num1 + 6f;
      stringList.Add(PrinterCompatibleString.Format("G0 Z0.4 E{0}", (object) num2));
      stringList.Add("G4 S10");
      float E1 = num2 + 0.3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) E1));
      stringList.Add(GCodeGeneration.GotoPoint(x1, y1, x2, y1, 0.4f, 2.15f, 1f, filament_diameter, ref E1));
      stringList.Add("G4 S10");
      float E2 = E1 + 0.3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) E2));
      stringList.Add(GCodeGeneration.GotoPoint(x2, y1, x2, y2, 0.4f, 2.15f, 1f, filament_diameter, ref E2));
      stringList.Add("G4 S10");
      float E3 = E2 + 0.3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) E3));
      stringList.Add(GCodeGeneration.GotoPoint(x2, y2, x1, y2, 0.4f, 2.15f, 1f, filament_diameter, ref E3));
      stringList.Add("G4 S10");
      float E4 = E3 + 0.3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) E4));
      stringList.Add(GCodeGeneration.GotoPoint(x1, y2, x1, y1, 0.4f, 2.15f, 1f, filament_diameter, ref E4));
      stringList.Add("G4 S10");
      float num3 = E4 + 0.3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) num3));
      float num4 = num3 + 1.5169f;
      stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} Z0.4 E{2}", (object) (float) ((double) x1 - 1.5), (object) (float) ((double) y1 - 1.0), (object) num4));
      float num5 = num4 + 5.3093f;
      stringList.Add(PrinterCompatibleString.Format("G0 X{0} Y{1} Z2.4 E{2} F1800", (object) (float) ((double) x1 - 3.0), (object) (float) ((double) y1 - 2.5), (object) num5));
      float num6 = num5 - 3f;
      stringList.Add(PrinterCompatibleString.Format("G0 E{0}", (object) num6));
      stringList.Add("G0 X4 Y12.5 Z25 F1800");
      return stringList;
    }

    public static string GotoPoint(float x1, float y1, float x2, float y2, float layer_height, float line_thickness, float flow_rate, float filament_diameter, ref float E)
    {
      float extrusion = GCodeGeneration.CalculateExtrusion(x1, y1, x2, y2, layer_height, line_thickness, flow_rate, filament_diameter);
      E += extrusion;
      return PrinterCompatibleString.Format("G0 X{0} Y{1} E{2}", (object) x2, (object) y2, (object) E);
    }

    public static float CalculateExtrusion(float x1, float y1, float x2, float y2, float layer_height, float extrusion_width, float flow_rate, float filament_diameter)
    {
      double num1 = Math.Sqrt(((double) x2 - (double) x1) * ((double) x2 - (double) x1) + ((double) y2 - (double) y1) * ((double) y2 - (double) y1));
      float num2 = filament_diameter / 2f;
      double num3 = (double) layer_height;
      return (float) (num1 * num3 * (double) extrusion_width / (3.1415901184082 * (double) num2 * (double) num2));
    }
  }
}
