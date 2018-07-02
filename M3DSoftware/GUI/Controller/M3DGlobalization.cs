using System.Globalization;

namespace M3D.GUI.Controller
{
  internal class M3DGlobalization
  {
    public static CultureInfo SYSTEM_CULTURE = new CultureInfo("en-US");

    public static string ToLocalString(float val)
    {
      return val.ToString();
    }

    public static string ToLocalString(float val, string format)
    {
      return val.ToString(format);
    }

    public static string ToLocalString(double val)
    {
      return val.ToString();
    }

    public static string ToLocalString(double val, string format)
    {
      return val.ToString(format);
    }

    public static string ToLocalString(int val)
    {
      return val.ToString();
    }

    public static string ToLocalString(int val, string format)
    {
      return val.ToString(format);
    }
  }
}
