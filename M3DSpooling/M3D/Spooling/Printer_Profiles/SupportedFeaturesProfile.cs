// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.SupportedFeaturesProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class SupportedFeaturesProfile
  {
    private WriteOnce<Dictionary<string, int>> m_supportedFeatureMapping;

    public SupportedFeaturesProfile(Dictionary<string, int> mapping)
      : this()
    {
      this.m_supportedFeatureMapping.Value = new Dictionary<string, int>((IDictionary<string, int>) mapping);
    }

    public SupportedFeaturesProfile()
    {
      this.m_supportedFeatureMapping = new WriteOnce<Dictionary<string, int>>((Dictionary<string, int>) null);
    }

    public SupportedFeaturesProfile(SupportedFeaturesProfile other)
    {
      this.m_supportedFeatureMapping = new WriteOnce<Dictionary<string, int>>();
      if (other.m_supportedFeatureMapping.Value == null)
        return;
      this.m_supportedFeatureMapping.Value = new Dictionary<string, int>((IDictionary<string, int>) other.m_supportedFeatureMapping.Value);
    }

    public int GetFeatureSlot(string featureName)
    {
      if (this.m_supportedFeatureMapping.Value.ContainsKey(featureName))
        return this.m_supportedFeatureMapping.Value[featureName];
      return -1;
    }

    public IEnumerable<KeyValuePair<string, int>> EnumerateList()
    {
      return (IEnumerable<KeyValuePair<string, int>>) this.SupportedFeatureMapping;
    }

    [XmlIgnore]
    public bool HasSupportedFeatures
    {
      get
      {
        if (this.m_supportedFeatureMapping.Value != null)
          return this.m_supportedFeatureMapping.Value.Count > 0;
        return false;
      }
    }

    public SerializableDictionary<string, int> SupportedFeatureMapping
    {
      get
      {
        if (this.m_supportedFeatureMapping.Value != null)
          return new SerializableDictionary<string, int>(this.m_supportedFeatureMapping.Value);
        return (SerializableDictionary<string, int>) null;
      }
      set
      {
        try
        {
          this.m_supportedFeatureMapping.Value = (Dictionary<string, int>) value;
        }
        catch (InvalidOperationException ex)
        {
          throw new InvalidOperationException("SupportedFeatureMapping can not be modified after creation.", (Exception) ex);
        }
      }
    }
  }
}
