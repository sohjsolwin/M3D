// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Paths
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.IO;

namespace M3D.Spooling.Core
{
  public static class Paths
  {
    public static char fs = Path.DirectorySeparatorChar;

    public static string M3DPublicFolder
    {
      get
      {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        try
        {
          if (Directory.Exists(folderPath + Paths.fs.ToString() + "com.M3D.software"))
          {
            Directory.Delete(folderPath + Paths.fs.ToString() + "com.M3D.software", true);
          }
        }
        catch (Exception ex)
        {
        }
        return folderPath + Paths.fs.ToString() + "M3D";
      }
    }

    public static string SharedDataFolder
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + Paths.fs.ToString() + "M3D" + Paths.fs.ToString() + "Spooler";
      }
    }

    public static string SpoolerFolder
    {
      get
      {
        return Paths.M3DPublicFolder + Paths.fs.ToString() + "Spooler";
      }
    }

    public static string LogPath
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Paths.fs.ToString() + "M3D" + Paths.fs.ToString() + "Spooler" + Paths.fs.ToString() + "log";
      }
    }

    public static string QueuePath
    {
      get
      {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Paths.fs.ToString() + "M3D" + Paths.fs.ToString() + "Spooler" + Paths.fs.ToString() + "queue";
      }
    }

    public static string SpoolerStorage
    {
      get
      {
        return Paths.SharedDataFolder + Paths.fs.ToString() + "storage";
      }
    }
  }
}
