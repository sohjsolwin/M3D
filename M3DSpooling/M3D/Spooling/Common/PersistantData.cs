// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PersistantData
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common.Utils;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  [XmlRoot("PrinterPersistantData")]
  public class PersistantData
  {
    [XmlIgnore]
    internal PersistantData.RestartAction MyRestartAction;
    [XmlAttribute("RestartActionParam")]
    public int RestartActionParam;
    [XmlAttribute]
    public int bootloader_version;
    [XmlAttribute]
    public float hours_used;
    [XmlElement("LastUpdate")]
    public DateTime LastUpdate;
    [XmlElement("SavedJobInformation")]
    public PersistantJobData SavedJobInformation;
    public SerializableDictionary<string, string> SavedData;
    private float unsaved_print_time;
    private bool gantry_clips_removed;
    private static XmlSerializer __class_serializer;

    public PersistantData()
    {
      LastUpdate = DateTime.Now;
      unsaved_print_time = 0.0f;
      gantry_clips_removed = false;
      bootloader_version = 0;
      hours_used = 0.0f;
      MyRestartAction = PersistantData.RestartAction.None;
      SavedJobInformation = (PersistantJobData) null;
      SavedData = new SerializableDictionary<string, string>();
    }

    public PersistantData(PersistantData other)
    {
      LastUpdate = other.LastUpdate;
      unsaved_print_time = other.unsaved_print_time;
      gantry_clips_removed = other.gantry_clips_removed;
      bootloader_version = other.bootloader_version;
      hours_used = other.hours_used;
      MyRestartAction = other.MyRestartAction;
      SavedData = new SerializableDictionary<string, string>((Dictionary<string, string>) other.SavedData);
      SavedJobInformation = other.SavedJobInformation != null ? new PersistantJobData(other.SavedJobInformation) : (PersistantJobData) null;
    }

    [XmlIgnore]
    public bool CanBeRemoved
    {
      get
      {
        return (double)UnsavedPrintTime < 1.0;
      }
    }

    internal PersistantData.RestartOptions PopRestartAction()
    {
      PersistantData.RestartOptions restartOptions;
      if (LastUpdate - DateTime.Now > new TimeSpan(0, 3, 0))
      {
        restartOptions.RestartAction = PersistantData.RestartAction.None;
        restartOptions.RestartActionParam = 0;
      }
      else
      {
        restartOptions.RestartAction = MyRestartAction;
        restartOptions.RestartActionParam = RestartActionParam;
      }
      MyRestartAction = PersistantData.RestartAction.None;
      return restartOptions;
    }

    [XmlAttribute("RestartAction")]
    public string RestartXML
    {
      get
      {
        return MyRestartAction.ToString();
      }
      set
      {
        MyRestartAction = (PersistantData.RestartAction) Enum.Parse(typeof (PersistantData.RestartAction), value, false);
      }
    }

    [XmlAttribute("UnsavedPrintTime")]
    public float UnsavedPrintTime
    {
      get
      {
        return unsaved_print_time;
      }
      set
      {
        unsaved_print_time = value;
        LastUpdate = DateTime.Now;
      }
    }

    [XmlAttribute("GantryClipsRemoved")]
    public bool GantryClipsRemoved
    {
      get
      {
        return gantry_clips_removed;
      }
      set
      {
        gantry_clips_removed = value;
        LastUpdate = DateTime.Now;
      }
    }

    public static XmlSerializer ClassSerializer
    {
      get
      {
        if (PersistantData.__class_serializer == null)
        {
          PersistantData.__class_serializer = new XmlSerializer(typeof (PersistantData));
        }

        return PersistantData.__class_serializer;
      }
    }

    public enum RestartAction
    {
      None,
      ForceUpdateFirmware,
      ForceStayBootloader,
      SetExtruderCurrent,
      SetFan,
    }

    internal struct RestartOptions
    {
      internal PersistantData.RestartAction RestartAction;
      internal int RestartActionParam;

      public RestartOptions(PersistantData.RestartAction RestartAction, int RestartActionParam)
      {
        this.RestartAction = RestartAction;
        this.RestartActionParam = RestartActionParam;
      }
    }
  }
}
