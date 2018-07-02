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
        return "Required: " + error_range.min + "-" + error_range.max;
      }

      if (!HasWarning)
      {
        return "";
      }

      return "Suggested: " + warning_range.min + "-" + warning_range.max;
    }
  }
}
