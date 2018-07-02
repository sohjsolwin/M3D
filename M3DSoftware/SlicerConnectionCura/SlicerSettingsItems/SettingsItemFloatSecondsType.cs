using System;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemFloatSecondsType : SettingsItemFloatMMType
  {
    public SettingsItemFloatSecondsType()
    {
    }

    public SettingsItemFloatSecondsType(float value, Range<float> warning_range, Range<float> error_range)
      : base(value, warning_range, error_range)
    {
    }

    public override bool HasError
    {
      get
      {
        return base.HasError;
      }
    }

    public override bool HasWarning
    {
      get
      {
        return base.HasWarning;
      }
    }

    public override string GetErrorMsg()
    {
      return base.GetErrorMsg();
    }

    public override SlicerSettingsItem Clone()
    {
      return new SettingsItemFloatSecondsType(value, warning_range, error_range);
    }
  }
}
