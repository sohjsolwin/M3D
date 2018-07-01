// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.General.SlicerConnectionBase
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Collections.Generic;
using System.IO;

namespace M3D.Slicer.General
{
  public abstract class SlicerConnectionBase
  {
    private object threadsync = new object();
    private Queue<string> slicer_msg = new Queue<string>();
    private string ExeResourceFolder;

    protected SlicerConnectionBase(string WorkingFolder, string ExeResourceFolder, SmartSlicerSettingsBase toCloneFrom)
    {
      this.WorkingFolder = WorkingFolder;
      this.ExeResourceFolder = ExeResourceFolder;
      this.SlicerSettingStack = new SmartSlicerSettingsStack(toCloneFrom);
    }

    public abstract string SlicerPath { get; }

    public string FullExecutablePath
    {
      get
      {
        return Path.Combine(this.ExeResourceFolder, this.SlicerPath);
      }
    }

    public string WorkingFolder { get; private set; }

    public SmartSlicerSettingsStack SlicerSettingStack { get; private set; }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        return this.SlicerSettingStack.SlicerSettings;
      }
    }

    public abstract int EstimatedPrintTimeSeconds { get; }

    public abstract float EstimatedFilament { get; }

    public abstract float EstimatedPercentComplete { get; }

    public abstract SlicerResult StartSlicingUsingCurrentSettings(string CombinedSTLFilePath, string OutputGCodeFile, PrintSettings printsettings);

    public abstract void Cancel();

    public void SendMessageToQueue(string msg)
    {
      lock (this.threadsync)
        this.slicer_msg.Enqueue(msg);
    }

    public string GetMessageFromQueue()
    {
      string str = (string) null;
      lock (this.threadsync)
      {
        if (this.slicer_msg.Count > 0)
          str = this.slicer_msg.Dequeue();
      }
      return str;
    }
  }
}
