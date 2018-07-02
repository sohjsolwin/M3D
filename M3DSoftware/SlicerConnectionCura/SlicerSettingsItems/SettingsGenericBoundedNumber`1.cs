// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsGenericBoundedNumber`1
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public abstract class SettingsGenericBoundedNumber<T> : SlicerSettingsItem where T : IComparable<T>
  {
    [XmlAttribute("Value")]
    public T value;
    [XmlElement("Warning")]
    public Range<T> warning_range;
    [XmlElement("Error")]
    public Range<T> error_range;

    public SettingsGenericBoundedNumber(T value, Range<T> warning_range, Range<T> error_range)
    {
      this.value = value;
      this.warning_range = warning_range;
      this.error_range = error_range;
    }

    public override bool HasWarning
    {
      get
      {
        if (value.CompareTo(warning_range.min) >= 0)
        {
          return value.CompareTo(warning_range.max) > 0;
        }

        return true;
      }
    }

    public override bool HasError
    {
      get
      {
        if (value.CompareTo(error_range.min) >= 0)
        {
          return value.CompareTo(error_range.max) > 0;
        }

        return true;
      }
    }

    public override string GetErrorMsg()
    {
      if (HasError)
      {
        return "Required: " + (object)error_range.min + "-" + (object)error_range.max;
      }

      if (!HasWarning)
      {
        return "";
      }

      return "Suggested: " + (object)warning_range.min + "-" + (object)warning_range.max;
    }
  }
}
