// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.PrinterCalibration
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;

namespace M3D.Spooling.Common.Utils
{
  internal static class PrinterCalibration
  {
    internal static List<string> CreateCalibrationGCode(PrinterCalibration.Type type, float calibrationZ, float startingZ, SpoolerOptionsProfile spoolerOptions)
    {
      var stringList = new List<string>();
      if (1.0 < (double) startingZ)
      {
        stringList.Add("M104 S150");
        stringList.Add("G90");
        stringList.Add("M17");
        stringList.Add(PrinterCompatibleString.Format("G0 Z{0}", (object) startingZ));
      }
      if (spoolerOptions.HomeAndSetTempOnCalibration)
      {
        if (1.0 > (double) startingZ)
        {
          stringList.Add("M104 S150");
        }

        stringList.Add("G28");
        stringList.Add("M109 S150");
      }
      string str;
      if (type != PrinterCalibration.Type.G30)
      {
        if (type != PrinterCalibration.Type.G32)
        {
          throw new NotImplementedException();
        }

        str = "G32";
      }
      else
      {
        str = "G30";
      }

      stringList.Add(PrinterCompatibleString.Format("{0} Z{1}", (object) str, (object) calibrationZ));
      stringList.Add("M104 S0");
      stringList.Add("M18");
      return stringList;
    }

    internal enum Type
    {
      G30,
      G32,
    }
  }
}
