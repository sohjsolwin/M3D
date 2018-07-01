// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.Plugins.SDCardPlugin
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using RepetierHost.model;
using System;

namespace M3D.Spooling.Core.Controllers.Plugins
{
  internal class SDCardPlugin : FirmwareControllerPlugin
  {
    private SDCard m_oPrinterSDCardStatus;
    private SDCardPlugin.ActiveSDPrintCallback m_OnActiveSDPrint;

    public SDCardPlugin(SDCard PrinterSDCardStatus, SDCardPlugin.ActiveSDPrintCallback activePrintCallback)
    {
      this.m_oPrinterSDCardStatus = PrinterSDCardStatus;
      this.m_OnActiveSDPrint = activePrintCallback;
    }

    public string ID
    {
      get
      {
        return "INTERNAL::SDCardPlugin";
      }
    }

    public void ProcessGCodeResult(GCode gcode, string resultFromPrinter, PrinterInfo printerInfo)
    {
      if (!gcode.hasM || gcode.M != (ushort) 27)
        return;
      string str1 = resultFromPrinter;
      char[] chArray = new char[1]{ '\n' };
      foreach (string str2 in str1.Split(chArray))
      {
        if (str2.Contains("Not SD printing."))
        {
          this.m_oPrinterSDCardStatus.IsPrintingFromSD = false;
          this.m_oPrinterSDCardStatus.SDFileLength = 0L;
          this.m_oPrinterSDCardStatus.SDFilePosition = 0L;
        }
        else if (str2.StartsWith("SD printing byte"))
        {
          try
          {
            string[] strArray = str2.Substring("SD printing byte".Length).Split('/');
            if (strArray.Length == 2)
            {
              long.TryParse(strArray[0], out this.m_oPrinterSDCardStatus.SDFilePosition);
              long.TryParse(strArray[1], out this.m_oPrinterSDCardStatus.SDFileLength);
              this.m_oPrinterSDCardStatus.IsPrintingFromSD = true;
            }
          }
          catch (Exception ex)
          {
          }
          if (printerInfo.current_job == null && this.m_OnActiveSDPrint != null)
          {
            this.m_OnActiveSDPrint();
            this.m_OnActiveSDPrint = (SDCardPlugin.ActiveSDPrintCallback) null;
          }
        }
      }
    }

    public void RegisterGCodes(IGCodePluginable controller)
    {
      int num = (int) controller.LinkGCodeWithPlugin("M27", this.ID);
    }

    public delegate void ActiveSDPrintCallback();
  }
}
