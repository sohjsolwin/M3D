// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemFloatSecondsType
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      return (SlicerSettingsItem) new SettingsItemFloatSecondsType(value, warning_range, error_range);
    }
  }
}
