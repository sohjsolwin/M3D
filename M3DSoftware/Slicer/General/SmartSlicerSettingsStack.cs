// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.SmartSlicerSettingsStack
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI;
using M3D.Spooling.Client;
using System.Collections.Generic;
using System.IO;

namespace M3D.Slicer.General
{
  public class SmartSlicerSettingsStack
  {
    private Stack<SmartSlicerSettingsBase> mCurrentSettingsStack;
    private Dictionary<string, Stack<SmartSlicerSettingsBase>> mSettingsStackList;
    private SmartSlicerSettingsBase mSmartSlicerSettings;

    public SmartSlicerSettingsStack(SmartSlicerSettingsBase smartSlicerSettings)
    {
      this.mSmartSlicerSettings = smartSlicerSettings;
      this.mSettingsStackList = new Dictionary<string, Stack<SmartSlicerSettingsBase>>();
    }

    public void SetCurrentSettingsFromPrinterProfile(IPrinter oPrinter)
    {
      string profileName = oPrinter.MyPrinterProfile.ProfileName;
      if (!this.mSettingsStackList.ContainsKey(profileName))
      {
        Stack<SmartSlicerSettingsBase> slicerSettingsBaseStack = new Stack<SmartSlicerSettingsBase>();
        slicerSettingsBaseStack.Push(this.mSmartSlicerSettings);
        this.mSettingsStackList.Add(profileName, slicerSettingsBaseStack);
      }
      this.mCurrentSettingsStack = this.mSettingsStackList[profileName];
      this.SlicerSettings?.ConfigureFromPrinterData(oPrinter);
      this.LoadSlicerSettings(this.SlicerSettings);
    }

    private void LoadSlicerSettings(SmartSlicerSettingsBase slicer_settings)
    {
      string filename = Path.Combine(Paths.WorkingFolder, slicer_settings.ConfigurationFileName);
      if (slicer_settings.ParseFile(filename))
        return;
      slicer_settings.SetToDefault();
    }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        if (this.mCurrentSettingsStack != null)
          return this.mCurrentSettingsStack.Peek();
        return (SmartSlicerSettingsBase) null;
      }
    }

    public void PushSettings()
    {
      this.mCurrentSettingsStack.Push(this.SlicerSettings.Clone());
    }

    public void PopSettings()
    {
      if (this.mCurrentSettingsStack.Count <= 1)
        return;
      this.mCurrentSettingsStack.Pop();
    }

    public void SaveSettingsDown()
    {
      if (this.mCurrentSettingsStack.Count < 2)
        return;
      SmartSlicerSettingsBase slicerSettingsBase = this.mCurrentSettingsStack.Pop();
      this.mCurrentSettingsStack.Pop();
      this.mCurrentSettingsStack.Push(slicerSettingsBase);
    }
  }
}
