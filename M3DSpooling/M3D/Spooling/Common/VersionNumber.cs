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
      this.major = 0U;
      this.minor = 0U;
      this.build = 0U;
      this.hotfix = 0U;
      this.rc = 0U;
      this.stage = VersionNumber.Stage.Release;
    }

    public VersionNumber(string versionString)
      : this()
    {
      if (string.IsNullOrEmpty(versionString))
        return;
      int num1 = versionString.IndexOf('-');
      string str1 = versionString;
      string str2 = (string) null;
      if (num1 >= 0)
      {
        str1 = versionString.Substring(0, num1);
        str2 = versionString.Substring(num1);
      }
      string[] strArray = str1.Split('.');
      if (strArray.Length != 4)
        return;
      uint.TryParse(strArray[0], out this.major);
      uint.TryParse(strArray[1], out this.minor);
      uint.TryParse(strArray[2], out this.build);
      uint.TryParse(strArray[3], out this.hotfix);
      if (string.IsNullOrEmpty(str2))
        return;
      try
      {
        int num2 = str2.IndexOf("-RC");
        if (num2 >= 0)
        {
          int startIndex = num2 + 3;
          string s = str2.Substring(startIndex);
          int length = s.IndexOf('-');
          if (length > 0)
            s = s.Substring(0, length);
          uint.TryParse(s, out this.rc);
          this.stage = VersionNumber.Stage.ReleaseCandidate;
        }
        if (str2.Contains("ALPHA"))
          this.stage = VersionNumber.Stage.Alpha;
        if (str2.Contains("BETA"))
          this.stage = VersionNumber.Stage.Beta;
        if (!str2.Contains("DEBUG"))
          return;
        this.stage = VersionNumber.Stage.DEBUG;
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
        this.stage = VersionNumber.Stage.ReleaseCandidate;
      if (VersionNumber.Stage.ReleaseCandidate != this.stage || rc != 0U)
        return;
      rc = 1U;
    }

    public int CompareTo(object obj)
    {
      if (obj == null)
        return 1;
      VersionNumber other = obj as VersionNumber;
      if (other == (VersionNumber) null)
        throw new ArgumentException("A Version object is required for comparison.", nameof (obj));
      return this.CompareTo(other);
    }

    public int CompareTo(VersionNumber other)
    {
      if ((object) other == null)
        return 1;
      if (this.Equals(other))
        return 0;
      return this.major > other.major || (int) this.major == (int) other.major && this.minor > other.minor || (int) this.major == (int) other.major && (int) this.minor == (int) other.minor && this.build > other.build || (int) this.major == (int) other.major && (int) this.minor == (int) other.minor && ((int) this.build == (int) other.build && this.hotfix > other.hotfix) || (int) this.major == (int) other.major && (int) this.minor == (int) other.minor && ((int) this.build == (int) other.build && (int) this.hotfix == (int) other.hotfix) && this.rc > other.rc ? 1 : -1;
    }

    public override string ToString()
    {
      string str = "";
      if (this.stage == VersionNumber.Stage.Alpha)
        str = "-ALPHA";
      else if (this.stage == VersionNumber.Stage.Beta)
        str = "-BETA";
      else if (this.stage == VersionNumber.Stage.DEBUG)
        str = "-DEBUG";
      return this.major.ToString() + "." + this.minor.ToString() + "." + this.build.ToString() + "." + this.hotfix.ToString() + "-" + M3D.Spooling.Version.OS + (this.rc != 0U ? "-RC" + this.rc.ToString() : "") + str;
    }

    public static int Compare(VersionNumber left, VersionNumber right)
    {
      if ((object) left == (object) right)
        return 0;
      if ((object) left == null)
        return -1;
      return left.CompareTo(right);
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;
      M3D.Spooling.Version version = obj as M3D.Spooling.Version;
      if (version == null)
        return false;
      return this.Equals((object) version);
    }

    public bool Equals(VersionNumber other)
    {
      if ((object) other == null || (int) this.major != (int) other.major || ((int) this.minor != (int) other.minor || (int) this.build != (int) other.build))
        return false;
      return (int) this.hotfix == (int) other.hotfix;
    }

    public override int GetHashCode()
    {
      return (int) this.major ^ (int) this.minor ^ (int) this.build ^ (int) this.hotfix ^ (int) this.rc;
    }

    public static bool operator ==(VersionNumber a, VersionNumber b)
    {
      if ((object) a == (object) b)
        return true;
      if ((object) a == null || (object) b == null)
        return false;
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
