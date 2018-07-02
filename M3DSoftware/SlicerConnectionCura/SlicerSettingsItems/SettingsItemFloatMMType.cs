using M3D.Graphics.Widgets2D;
using M3D.Slicer.General;
using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemFloatMMType : SettingsGenericBoundedNumber<float>, IReportFormat
  {
    [XmlIgnore]
    private bool formatError;

    public SettingsItemFloatMMType()
      : base(float.NaN, new Range<float>(), new Range<float>())
    {
    }

    public SettingsItemFloatMMType(float value, Range<float> warning_range, Range<float> error_range)
      : base(value, warning_range, error_range)
    {
    }

    protected override bool SetFromSlicerValue(string val)
    {
      formatError = false;
      try
      {
        value = (float) int.Parse(val) / 1000f;
        if ((double)value < 0.0)
        {
          value = -1f;
        }
      }
      catch (Exception ex)
      {
        formatError = true;
        return false;
      }
      return true;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.FloatMMType;
    }

    public override string TranslateToSlicerValue()
    {
      var num = -1;
      if ((double)value >= 0.0)
      {
        num = (int) ((double)value * 1000.0);
      }

      return num.ToString();
    }

    public override string TranslateToUserValue()
    {
      return value.ToString("0.000");
    }

    public override void ParseUserValue(string val)
    {
      formatError = false;
      try
      {
        value = float.Parse(val);
      }
      catch (Exception ex)
      {
        formatError = true;
      }
    }

    public override bool HasError
    {
      get
      {
        if (!formatError)
        {
          return base.HasError;
        }

        return true;
      }
    }

    public override bool HasWarning
    {
      get
      {
        if (!formatError)
        {
          return base.HasWarning;
        }

        return true;
      }
    }

    public override string GetErrorMsg()
    {
      if (formatError)
      {
        return "Not a number";
      }

      return base.GetErrorMsg();
    }

    public override SlicerSettingsItem Clone()
    {
      return (SlicerSettingsItem) new SettingsItemFloatMMType(value, warning_range, error_range);
    }

    [XmlAttribute("Number_Format")]
    public NumFormat Format { get; set; } = NumFormat.Thousands;
  }
}
