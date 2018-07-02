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
