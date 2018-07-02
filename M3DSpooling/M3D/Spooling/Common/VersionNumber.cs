// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.VersionNumber
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class VersionNumber : IComparable, IComparable<VersionNumber>
  {
    [XmlAttribute("major")]
    public uint major;
    [XmlAttribute("minor")]
    public uint minor;
    [XmlAttribute("build")]
    public uint build;
    [XmlAttribute("hotfix")]
    public uint hotfix;
    [XmlAttribute("rc")]
    public uint rc;
    [XmlAttribute("stage")]
    public VersionNumber.Stage stage;

    public VersionNumber()
    {
      major = 0U;
      minor = 0U;
      build = 0U;
      hotfix = 0U;
      rc = 0U;
      stage = VersionNumber.Stage.Release;
    }

    public VersionNumber(string versionString)
      : this()
    {
      if (string.IsNullOrEmpty(versionString))
      {
        return;
      }

      var num1 = versionString.IndexOf('-');
      var str1 = versionString;
      var str2 = (string) null;
      if (num1 >= 0)
      {
        str1 = versionString.Substring(0, num1);
        str2 = versionString.Substring(num1);
      }
      string[] strArray = str1.Split('.');
      if (strArray.Length != 4)
      {
        return;
      }

      uint.TryParse(strArray[0], out major);
      uint.TryParse(strArray[1], out minor);
      uint.TryParse(strArray[2], out build);
      uint.TryParse(strArray[3], out hotfix);
      if (string.IsNullOrEmpty(str2))
      {
        return;
      }

      try
      {
        var num2 = str2.IndexOf("-RC");
        if (num2 >= 0)
        {
          var startIndex = num2 + 3;
          var s = str2.Substring(startIndex);
          var length = s.IndexOf('-');
          if (length > 0)
          {
            s = s.Substring(0, length);
          }

          uint.TryParse(s, out rc);
          stage = VersionNumber.Stage.ReleaseCandidate;
        }
        if (str2.Contains("ALPHA"))
        {
          stage = VersionNumber.Stage.Alpha;
        }

        if (str2.Contains("BETA"))
        {
          stage = VersionNumber.Stage.Beta;
        }

        if (!str2.Contains("DEBUG"))
        {
          return;
        }

        stage = VersionNumber.Stage.DEBUG;
      }
      catch (Exception ex)
      {
      }
    }

    public VersionNumber(uint major, uint minor, uint build, uint hotfix, uint rc, VersionNumber.Stage stage)
    {
      this.major = major;
      this.minor = minor;
      this.build = build;
      this.hotfix = hotfix;
      this.rc = rc;
      this.stage = stage;
      if (rc > 0U && stage == VersionNumber.Stage.Release)
      {
        this.stage = VersionNumber.Stage.ReleaseCandidate;
      }

      if (VersionNumber.Stage.ReleaseCandidate != this.stage || rc != 0U)
      {
        return;
      }

      rc = 1U;
    }

    public int CompareTo(object obj)
    {
      if (obj == null)
      {
        return 1;
      }

      var other = obj as VersionNumber;
      if (other == (VersionNumber) null)
      {
        throw new ArgumentException("A Version object is required for comparison.", nameof (obj));
      }

      return CompareTo(other);
    }

    public int CompareTo(VersionNumber other)
    {
      if ((object) other == null)
      {
        return 1;
      }

      if (Equals(other))
      {
        return 0;
      }

      return major > other.major || (int)major == (int) other.major && minor > other.minor || (int)major == (int) other.major && (int)minor == (int) other.minor && build > other.build || (int)major == (int) other.major && (int)minor == (int) other.minor && ((int)build == (int) other.build && hotfix > other.hotfix) || (int)major == (int) other.major && (int)minor == (int) other.minor && ((int)build == (int) other.build && (int)hotfix == (int) other.hotfix) && rc > other.rc ? 1 : -1;
    }

    public override string ToString()
    {
      var str = "";
      if (stage == VersionNumber.Stage.Alpha)
      {
        str = "-ALPHA";
      }
      else if (stage == VersionNumber.Stage.Beta)
      {
        str = "-BETA";
      }
      else if (stage == VersionNumber.Stage.DEBUG)
      {
        str = "-DEBUG";
      }

      return major.ToString() + "." + minor.ToString() + "." + build.ToString() + "." + hotfix.ToString() + "-" + M3D.Spooling.Version.OS + (rc != 0U ? "-RC" + rc.ToString() : "") + str;
    }

    public static int Compare(VersionNumber left, VersionNumber right)
    {
      if ((object) left == (object) right)
      {
        return 0;
      }

      if ((object) left == null)
      {
        return -1;
      }

      return left.CompareTo(right);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }

      var version = obj as M3D.Spooling.Version;
      if (version == null)
      {
        return false;
      }

      return Equals((object) version);
    }

    public bool Equals(VersionNumber other)
    {
      if ((object) other == null || (int)major != (int) other.major || ((int)minor != (int) other.minor || (int)build != (int) other.build))
      {
        return false;
      }

      return (int)hotfix == (int) other.hotfix;
    }

    public override int GetHashCode()
    {
      return (int)major ^ (int)minor ^ (int)build ^ (int)hotfix ^ (int)rc;
    }

    public static bool operator ==(VersionNumber a, VersionNumber b)
    {
      if ((object) a == (object) b)
      {
        return true;
      }

      if ((object) a == null || (object) b == null)
      {
        return false;
      }

      return a.Equals(b);
    }

    public static bool operator !=(VersionNumber a, VersionNumber b)
    {
      return !(a == b);
    }

    public static bool operator <(VersionNumber left, VersionNumber right)
    {
      return VersionNumber.Compare(left, right) < 0;
    }

    public static bool operator >(VersionNumber left, VersionNumber right)
    {
      return VersionNumber.Compare(left, right) > 0;
    }

    public enum Stage
    {
      Release,
      Alpha,
      Beta,
      ReleaseCandidate,
      DEBUG,
    }
  }
}
