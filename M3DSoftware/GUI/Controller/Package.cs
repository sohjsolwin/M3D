using M3D.Spooling.Common;
using System;
using System.Globalization;
using System.Numerics;
using System.Xml.Serialization;

namespace M3D.GUI.Controller
{
  [Serializable]
  public class Package
  {
    [XmlAttribute("Distribution")]
    public Package.DistributionType Distribution;
    [XmlIgnore]
    public BigInteger? n128FileHash;
    [XmlIgnore]
    public VersionNumber Version;
    [XmlIgnore]
    public Uri Url;

    [XmlElement("MD5Hash")]
    public string Hash_Str
    {
      get
      {
        if (n128FileHash.HasValue)
        {
          return n128FileHash.ToString();
        }

        return "";
      }
      set
      {
        try
        {
          n128FileHash = new BigInteger?(BigInteger.Parse(value.Trim(), NumberStyles.HexNumber));
        }
        catch
        {
          n128FileHash = new BigInteger?();
        }
      }
    }

    [XmlElement("Url")]
    public string Address_Str
    {
      get
      {
        return Url.ToString();
      }
      set
      {
        if (!Uri.TryCreate(value, UriKind.Absolute, out Url) && Url.Scheme != Uri.UriSchemeHttps)
        {
          throw new ArgumentException("Url must be absolute and https");
        }
      }
    }

    [XmlAttribute("Version")]
    public string Version_Str
    {
      get
      {
        return Version.ToString();
      }
      set
      {
        Version = new VersionNumber(value);
      }
    }

    public enum DistributionType
    {
      Mac,
      Linux,
      Windows,
    }
  }
}
