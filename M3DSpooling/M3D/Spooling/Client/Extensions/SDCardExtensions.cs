// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Client.Extensions.SDCardExtensions
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections.Generic;

namespace M3D.Spooling.Client.Extensions
{
  public class SDCardExtensions : IPrinterPlugin
  {
    public EventHandler OnReceivedFileList;
    private IPrinter m_oPrinter;
    private List<string> m_SDCardFileList;

    public SDCardExtensions(IPrinter printer)
    {
      this.m_oPrinter = printer;
      this.m_SDCardFileList = new List<string>();
    }

    public string[] GetSDCardFileList()
    {
      return this.m_SDCardFileList.ToArray();
    }

    public SpoolerResult RefreshSDCardList(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      return this.m_oPrinter.SendManualGCode(callback, state, "M20");
    }

    public SpoolerResult DeleteFileFromSDCard(M3D.Spooling.Client.AsyncCallback callback, object state, string filename)
    {
      return this.m_oPrinter.SendManualGCode(callback, state, string.Format("M30 {0}", (object) filename));
    }

    public bool Available
    {
      get
      {
        if (this.m_oPrinter.Info.supportedFeatures.UsesSupportedFeatures)
          return this.m_oPrinter.Info.supportedFeatures.Available("Untethered Printing", this.m_oPrinter.MyPrinterProfile.SupportedFeaturesConstants);
        return false;
      }
    }

    public static string ID
    {
      get
      {
        return "Extension::SDCardExtensions";
      }
    }

    public void OnReceivedPluginMessage(string gcode, string result)
    {
      if (!gcode.StartsWith("M20"))
        return;
      this.m_SDCardFileList.Clear();
      string[] strArray = result.Split('\n');
      bool flag = false;
      foreach (string str in strArray)
      {
        if (str == "Begin file list:")
          flag = true;
        else if (str == "End file list")
          flag = false;
        else if (flag)
          this.m_SDCardFileList.Add(str);
      }
      if (this.OnReceivedFileList == null)
        return;
      this.OnReceivedFileList((object) this, (EventArgs) null);
    }

    public string[] GetGCodes()
    {
      return new string[1]{ "M20" };
    }
  }
}
