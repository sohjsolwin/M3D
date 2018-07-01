// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.M3DGlobalization
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
