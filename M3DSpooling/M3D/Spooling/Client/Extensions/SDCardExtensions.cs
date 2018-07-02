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
      m_oPrinter = printer;
      m_SDCardFileList = new List<string>();
    }

    public string[] GetSDCardFileList()
    {
      return m_SDCardFileList.ToArray();
    }

    public SpoolerResult RefreshSDCardList(M3D.Spooling.Client.AsyncCallback callback, object state)
    {
      return m_oPrinter.SendManualGCode(callback, state, "M20");
    }

    public SpoolerResult DeleteFileFromSDCard(M3D.Spooling.Client.AsyncCallback callback, object state, string filename)
    {
      return m_oPrinter.SendManualGCode(callback, state, string.Format("M30 {0}", filename));
    }

    public bool Available
    {
      get
      {
        if (m_oPrinter.Info.supportedFeatures.UsesSupportedFeatures)
        {
          return m_oPrinter.Info.supportedFeatures.Available("Untethered Printing", m_oPrinter.MyPrinterProfile.SupportedFeaturesConstants);
        }

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
      {
        return;
      }

      m_SDCardFileList.Clear();
      string[] strArray = result.Split('\n');
      var flag = false;
      foreach (var str in strArray)
      {
        if (str == "Begin file list:")
        {
          flag = true;
        }
        else if (str == "End file list")
        {
          flag = false;
        }
        else if (flag)
        {
          m_SDCardFileList.Add(str);
        }
      }
      if (OnReceivedFileList == null)
      {
        return;
      }

      OnReceivedFileList(this, null);
    }

    public string[] GetGCodes()
    {
      return new string[1]{ "M20" };
    }
  }
}
