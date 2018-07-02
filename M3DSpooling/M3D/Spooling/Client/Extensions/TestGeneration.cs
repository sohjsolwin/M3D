using M3D.Spooling.Common.Utils;
using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;

namespace M3D.Spooling.Client.Extensions
{
  public static class TestGeneration
  {
    private const float XY_SPEED = 3000f;
    private const float Z_SPEED = 90f;
    private const float E_SPEED = 345f;

    public static List<string> FastRecenter(PrinterProfile profile)
    {
      var x = profile.PrinterSizeConstants.printBedSize.x;
      var y = profile.PrinterSizeConstants.printBedSize.y;
      return new List<string>() { "G91", PrinterCompatibleString.Format("G0 Y{0} F{1}", y, 3000f), PrinterCompatibleString.Format("G0 X{0} F{1}", x, 3000f), PrinterCompatibleString.Format("G0 Y-{0} X-{1}", (float)((double)y / 2.0), (float)((double)x / 2.0)) };
    }

    public static List<string> CreateXSpeedTest(PrinterProfile profile)
    {
      var x = profile.PrinterSizeConstants.printBedSize.x;
      return new List<string>() { "G91", PrinterCompatibleString.Format("G0 X{0} F{1}", x, 3000f), PrinterCompatibleString.Format("G0 X-{0}", (object) x), PrinterCompatibleString.Format("G0 X{0}", (object) x), PrinterCompatibleString.Format("G0 X-{0}", (object) x), PrinterCompatibleString.Format("G0 X{0}", (object) x), PrinterCompatibleString.Format("G0 X-{0}", (object) x) };
    }

    public static List<string> CreateYSpeedTest(PrinterProfile profile)
    {
      var y = profile.PrinterSizeConstants.printBedSize.y;
      return new List<string>() { "G91", PrinterCompatibleString.Format("G0 Y{0} F{1}", y, 3000f), PrinterCompatibleString.Format("G0 Y-{0}", (object) y), PrinterCompatibleString.Format("G0 Y{0}", (object) y), PrinterCompatibleString.Format("G0 Y-{0}", (object) y), PrinterCompatibleString.Format("G0 Y{0}", (object) y), PrinterCompatibleString.Format("G0 Y-{0}", (object) y) };
    }

    public static List<string> CreateXSkipTestMinus(PrinterProfile profile)
    {
      return TestGeneration.CreateSkipTestInternal(profile.PrinterSizeConstants.printBedSize.x, -4, 'X');
    }

    public static List<string> CreateXSkipTestPlus(PrinterProfile profile)
    {
      return TestGeneration.CreateSkipTestInternal(profile.PrinterSizeConstants.printBedSize.x, 4, 'X');
    }

    public static List<string> CreateYSkipTestMinus(PrinterProfile profile)
    {
      return TestGeneration.CreateSkipTestInternal(profile.PrinterSizeConstants.printBedSize.y, -5, 'Y');
    }

    public static List<string> CreateYSkipTestPlus(PrinterProfile profile)
    {
      return TestGeneration.CreateSkipTestInternal(profile.PrinterSizeConstants.printBedSize.y, 5, 'Y');
    }

    private static List<string> CreateSkipTestInternal(float max, int stride, char param)
    {
      var stringList = new List<string>();
      var num1 = (int)(max / (double)Math.Abs(stride));
      var num2 = stride < 0 ? 1 : -1;
      stringList.Add("G91");
      for (var index1 = 0; index1 < 1; ++index1)
      {
        stringList.Add("G4 S1");
        stringList.Add(PrinterCompatibleString.Format("G0 {0}{1} F{2}", param, (float)((double)max * (double)num2), 3000f));
        for (var index2 = 0; index2 < num1; ++index2)
        {
          stringList.Add("G4 S1");
          stringList.Add(PrinterCompatibleString.Format("G0 {0}{1}", param, stride));
        }
      }
      return stringList;
    }
  }
}
