﻿using System.Collections.Generic;
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
      SlicerSettingStack = new SmartSlicerSettingsStack(toCloneFrom);
    }

    public abstract string SlicerPath { get; }

    public string FullExecutablePath
    {
      get
      {
        return Path.Combine(ExeResourceFolder, SlicerPath);
      }
    }

    public string WorkingFolder { get; private set; }

    public SmartSlicerSettingsStack SlicerSettingStack { get; private set; }

    public SmartSlicerSettingsBase SlicerSettings
    {
      get
      {
        return SlicerSettingStack.SlicerSettings;
      }
    }

    public abstract int EstimatedPrintTimeSeconds { get; }

    public abstract float EstimatedFilament { get; }

    public abstract float EstimatedPercentComplete { get; }

    public abstract SlicerResult StartSlicingUsingCurrentSettings(string CombinedSTLFilePath, string OutputGCodeFile, PrintSettings printsettings);

    public abstract void Cancel();

    public void SendMessageToQueue(string msg)
    {
      lock (threadsync)
      {
        slicer_msg.Enqueue(msg);
      }
    }

    public string GetMessageFromQueue()
    {
      var str = (string) null;
      lock (threadsync)
      {
        if (slicer_msg.Count > 0)
        {
          str = slicer_msg.Dequeue();
        }
      }
      return str;
    }
  }
}
