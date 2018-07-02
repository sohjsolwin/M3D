// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SlicerSettingsEnumItem
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  public abstract class SlicerSettingsEnumItem : SlicerSettingsItem
  {
    protected bool formatError;

    public SlicerSettingsEnumItem(Type enumType)
    {
      EnumType = enumType;
    }

    public override bool HasError
    {
      get
      {
        return formatError;
      }
    }

    public override string GetErrorMsg()
    {
      return HasError ? "invalid option" : "";
    }

    public Type EnumType { get; set; }

    public int ValueInt { get; set; }
  }
}
