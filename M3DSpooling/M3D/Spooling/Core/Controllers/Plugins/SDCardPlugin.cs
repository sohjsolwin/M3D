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
      m_oPrinterSDCardStatus = PrinterSDCardStatus;
      m_OnActiveSDPrint = activePrintCallback;
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
      {
        return;
      }

      var str1 = resultFromPrinter;
      char[] chArray = new char[1]{ '\n' };
      foreach (var str2 in str1.Split(chArray))
      {
        if (str2.Contains("Not SD printing."))
        {
          m_oPrinterSDCardStatus.IsPrintingFromSD = false;
          m_oPrinterSDCardStatus.SDFileLength = 0L;
          m_oPrinterSDCardStatus.SDFilePosition = 0L;
        }
        else if (str2.StartsWith("SD printing byte"))
        {
          try
          {
            string[] strArray = str2.Substring("SD printing byte".Length).Split('/');
            if (strArray.Length == 2)
            {
              long.TryParse(strArray[0], out m_oPrinterSDCardStatus.SDFilePosition);
              long.TryParse(strArray[1], out m_oPrinterSDCardStatus.SDFileLength);
              m_oPrinterSDCardStatus.IsPrintingFromSD = true;
            }
          }
          catch (Exception ex)
          {
          }
          if (printerInfo.current_job == null && m_OnActiveSDPrint != null)
          {
            m_OnActiveSDPrint();
            m_OnActiveSDPrint = (SDCardPlugin.ActiveSDPrintCallback) null;
          }
        }
      }
    }

    public void RegisterGCodes(IGCodePluginable controller)
    {
      var num = (int) controller.LinkGCodeWithPlugin("M27", ID);
    }

    public delegate void ActiveSDPrintCallback();
  }
}
