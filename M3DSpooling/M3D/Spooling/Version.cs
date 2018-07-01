// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Version
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;

namespace M3D.Spooling
{
  public class Version
  {
    public static readonly VersionNumber Client_Version = new VersionNumber(1U, 8U, 3U, 0U, 0U, VersionNumber.Stage.Release);
    public const uint CLIENT_VERSION_DATE = 20171218;
    public const uint RESET_OFFSET_VERSION = 2015080402;

    public static string OS
    {
      get
      {
        return "WIN";
      }
    }

    public static string VersionText
    {
      get
      {
        string str = 20171218U.ToString();
        return str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2) + "-v" + Version.VersionTextNoDate;
      }
    }

    public static string VersionTextNoDate
    {
      get
      {
        return Version.Client_Version.ToString();
      }
    }
  }
}
