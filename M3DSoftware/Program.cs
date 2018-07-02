// Decompiled with JetBrains decompiler
// Type: M3D.Program
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI;
using M3D.GUI.Forms;
using M3D.GUI.Forms.Splash;
using System;
using System.IO;
using System.Windows.Forms;

namespace M3D
{
  internal static class Program
  {
    public static bool runfirst_start;

    [STAThread]
    private static void Main(string[] args)
    {
      if (!FileAssociationSingleInstance.RegisterAsSingleInstance() && args.Length != 0 && (args.Length <= 1 || !(args[1] == "OPEN")) && FileAssociationSingleInstance.SendParametersToSingleInstance(args))
      {
        return;
      }

      Directory.SetCurrentDirectory(Paths.AssemblyDirectory);
      try
      {
        Directory.CreateDirectory(Paths.PublicDataFolder);
        Directory.CreateDirectory(Paths.SoftwareDataFolder);
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "Working"));
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "MyLibrary"));
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Prints"));
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "Temp"));
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "Utility"));
        Directory.CreateDirectory(Path.Combine(Paths.PublicDataFolder, "ExtractedZipFiles"));
        Directory.CreateDirectory(Paths.DebugLogFolder);
      }
      catch (Exception ex)
      {
        var num = (int) MessageBox.Show("Unable to create application folder. \"" + Paths.PublicDataFolder + "\"" + ex.Message);
        return;
      }
      var path = Path.Combine(Paths.PublicDataFolder, "version.info");
      Program.runfirst_start = false;
      try
      {
        var binaryReader = new BinaryReader((Stream) new FileStream(path, FileMode.Open, FileAccess.Read));
        var num1 = binaryReader.ReadUInt32();
        uint num2;
        try
        {
          num2 = binaryReader.ReadUInt32();
        }
        catch (Exception ex)
        {
          num2 = 0U;
        }
        uint num3;
        try
        {
          num3 = binaryReader.ReadUInt32();
        }
        catch (Exception ex)
        {
          num3 = 0U;
        }
        uint num4;
        try
        {
          num4 = binaryReader.ReadUInt32();
        }
        catch (Exception ex)
        {
          num4 = 0U;
        }
        if ((int) num1 != (int) M3D.Spooling.Version.Client_Version.major || (int) num2 != (int) M3D.Spooling.Version.Client_Version.minor || ((int) num3 != (int) M3D.Spooling.Version.Client_Version.build || (int) num4 != (int) M3D.Spooling.Version.Client_Version.hotfix))
        {
          Program.runfirst_start = true;
        }

        binaryReader.Close();
      }
      catch (Exception ex)
      {
        Program.runfirst_start = true;
      }
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      if (Program.runfirst_start)
      {
        Application.Run((Form) new SplashFormFirstRun());
      }

      Application.Run((Form) new Form1(new SplashForm(), args));
    }

    public static bool isObfuscated
    {
      get
      {
        return false;
      }
    }

    public static bool isDEBUGBUILD
    {
      get
      {
        return false;
      }
    }
  }
}
