// Decompiled with JetBrains decompiler
// Type: M3D.SlicerConnectionCura.SlicerSettingsItems.SettingsItemBoolRPCType
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Slicer.General;
using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public class SettingsItemBoolRPCType : SlicerSettingsItem
  {
    [XmlAttribute("property")]
    public string m_sPropertyName;
    private SmartSlicerSettingsBase m_oParentObject;

    public SettingsItemBoolRPCType()
    {
    }

    public SettingsItemBoolRPCType(string propertyName, SmartSlicerSettingsBase parentObject)
    {
      this.m_oParentObject = parentObject;
      this.m_sPropertyName = propertyName;
    }

    protected override bool SetFromSlicerValue(string val)
    {
      return false;
    }

    public override SettingItemType GetItemType()
    {
      return SettingItemType.BoolType;
    }

    public override string TranslateToSlicerValue()
    {
      return !this.PropertyAccessor ? "0" : "1";
    }

    public override string TranslateToUserValue()
    {
      return !this.PropertyAccessor ? "false" : "true";
    }

    public override void ParseUserValue(string value)
    {
    }

    public override bool HasWarning
    {
      get
      {
        return false;
      }
    }

    public override bool HasError
    {
      get
      {
        return false;
      }
    }

    public override string GetErrorMsg()
    {
      return "";
    }

    public override SlicerSettingsItem Clone()
    {
      return (SlicerSettingsItem) new SettingsItemBoolRPCType(this.m_sPropertyName, this.m_oParentObject);
    }

    public void SetParentSettings(SmartSlicerSettingsBase parentObject)
    {
      this.m_oParentObject = parentObject;
    }

    private bool PropertyAccessor
    {
      get
      {
        if (this.m_oParentObject != null)
        {
          try
          {
            object obj = this.m_oParentObject.GetType().GetProperty(this.m_sPropertyName).GetValue((object) this.m_oParentObject);
            if (obj is bool)
              return (bool) obj;
          }
          catch (Exception ex)
          {
            if (Debugger.IsAttached)
              Debugger.Break();
          }
        }
        return false;
      }
    }
  }
}
