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
      return string.Format(PrinterCompatibleString.PRINTER_CULTURE, format, args);
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
      var allowedCharacters = new HashSet<char>("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.-_");
      return new string(((IEnumerable<char>) text.ToCharArray()).Where<char>(c => allowedCharacters.Contains(c)).ToArray<char>());
    }
  }
}
