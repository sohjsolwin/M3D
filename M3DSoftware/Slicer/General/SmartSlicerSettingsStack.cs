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
      mSmartSlicerSettings = smartSlicerSettings;
      mSettingsStackList = new Dictionary<string, Stack<SmartSlicerSettingsBase>>();
    }

    public void SetCurrentSettingsFromPrinterProfile(IPrinter oPrinter)
    {
      var profileName = oPrinter.MyPrinterProfile.ProfileName;
      if (!mSettingsStackList.ContainsKey(profileName))
      {
        var slicerSettingsBaseStack = new Stack<SmartSlicerSettingsBase>();
        slicerSettingsBaseStack.Push(mSmartSlicerSettings);
        mSettingsStackList.Add(profileName, slicerSettingsBaseStack);
      }
      mCurrentSettingsStack = mSettingsStackList[profileName];
      SlicerSettings?.ConfigureFromPrinterData(oPrinter);
      LoadSlicerSettings(SlicerSettings);
    }

    private void LoadSlicerSettings(SmartSlicerSettingsBase slicer_settings)
    {
      var filename = Path.Combine(Paths.WorkingFolder, slicer_settings.ConfigurationFileName);
      if (slicer_settings.ParseFile(filename))
      {
        return;
      }

      slicer_settings.SetToDefault();
    }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        if (mCurrentSettingsStack != null)
        {
          return mCurrentSettingsStack.Peek();
        }

        return (SmartSlicerSettingsBase) null;
      }
    }

    public void PushSettings()
    {
      mCurrentSettingsStack.Push(SlicerSettings.Clone());
    }

    public void PopSettings()
    {
      if (mCurrentSettingsStack.Count <= 1)
      {
        return;
      }

      mCurrentSettingsStack.Pop();
    }

    public void SaveSettingsDown()
    {
      if (mCurrentSettingsStack.Count < 2)
      {
        return;
      }

      SmartSlicerSettingsBase slicerSettingsBase = mCurrentSettingsStack.Pop();
      mCurrentSettingsStack.Pop();
      mCurrentSettingsStack.Push(slicerSettingsBase);
    }
  }
}
