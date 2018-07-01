// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Paths
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System;
using System.IO;
using System.Reflection;

namespace M3D.GUI
{
  public static class Paths
  {
    internal const string OutputGCodeFileName = "m3doutput.gcode";

    public static string M3DPublicFolder
    {
      get
      {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "M3D");
      }
    }

    public static string PublicDataFolder
    {
      get
      {
        return Path.Combine(Paths.M3DPublicFolder, "M3DSoftware");
      }
    }

    public static string GUIImagesFolder
    {
      get
      {
        return Path.Combine(Paths.SoftwareDataFolder, "GUIImages");
      }
    }

    public static string GUIObjectsFolder
    {
      get
      {
        return Path.Combine(Paths.SoftwareDataFolder, "GUIObjects");
      }
    }

    public static string DebugLogFolder
    {
      get
      {
        return Path.Combine(Paths.PublicDataFolder, "DebugLog");
      }
    }

    public static string SoftwareDataFolder
    {
      get
      {
        return Path.Combine(Paths.PublicDataFolder, "Data");
      }
    }

    public static string ReadOnlyDataFolder
    {
      get
      {
        return Path.Combine(Paths.ResourceFolder, "Data");
      }
    }

    public static string WorkingFolder
    {
      get
      {
        return Path.Combine(Paths.PublicDataFolder, "Working");
      }
    }

    internal static string CombinedSTLPath
    {
      get
      {
        return Path.Combine(Paths.WorkingFolder, "m3dawesome.stl");
      }
    }

    internal static string RecentDBPath
    {
      get
      {
        return Path.Combine(Paths.SoftwareDataFolder, "recent.db");
      }
    }

    internal static string SettingsPath
    {
      get
      {
        return Path.Combine(Paths.SoftwareDataFolder, "settings.xml");
      }
    }

    internal static string PrintHistoryPath
    {
      get
      {
        return Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints", "history.xml");
      }
    }

    internal static string StartupLogPath
    {
      get
      {
        return Path.Combine(Paths.DebugLogFolder, "debug-startuplog.txt");
      }
    }

    internal static string DebugLogPath(long nlTimeAsMSCount)
    {
      return Path.Combine(Paths.DebugLogFolder, "debuglog" + nlTimeAsMSCount.ToString() + ".txt");
    }

    internal static string SlicerSettingsPath(string sConfigFileName)
    {
      return Path.Combine(Paths.WorkingFolder, sConfigFileName);
    }

    public static string AssemblyDirectory
    {
      get
      {
        return Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
      }
    }

    public static string ResourceFolder
    {
      get
      {
        return Path.Combine(Paths.AssemblyDirectory, "Resources");
      }
    }

    public static string DownloadedHashXML_URL
    {
      get
      {
        return "http://printm3d.com/files/version_pro_alpha.xml";
      }
    }

    public static string DownloadedExecutableFile
    {
      get
      {
        return Path.Combine(Path.GetTempPath(), "M3D.exe");
      }
    }
  }
}
