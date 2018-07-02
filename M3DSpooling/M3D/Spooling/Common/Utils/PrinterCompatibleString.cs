// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.Utils.PrinterCompatibleString
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace M3D.Spooling.Common.Utils
{
  public static class PrinterCompatibleString
  {
    public static CultureInfo PRINTER_CULTURE
    {
      get
      {
        return CultureInfo.InvariantCulture;
      }
    }

    public static NumberFormatInfo NUMBER_FORMAT
    {
      get
      {
        return PrinterCompatibleString.PRINTER_CULTURE.NumberFormat;
      }
    }

    public static string Format(string format, params object[] args)
    {
      return string.Format((IFormatProvider) PrinterCompatibleString.PRINTER_CULTURE, format, args);
    }

    public static bool VerifyNumber(string number_string)
    {
      return float.TryParse(number_string, out var result);
    }

    public static float ToFloatCurrentCulture(string number_string)
    {
      if (float.TryParse(number_string, out var result))
      {
        return result;
      }

      return float.NaN;
    }

    public static string RemoveIllegalCharacters(string text)
    {
      var allowedCharacters = new HashSet<char>((IEnumerable<char>) "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-_");
      return new string(((IEnumerable<char>) text.ToCharArray()).Where<char>((Func<char, bool>) (c => allowedCharacters.Contains(c))).ToArray<char>());
    }
  }
}
