using M3D.Spooling.Common;
using M3D.Spooling.Core.Controllers.PrintJobs;
using M3D.Spooling.Printer_Profiles;
using RepetierHost.model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace M3D.Spooling.Core.Controllers.Plugins
{
  internal class PowerRecoveryPlugin : FirmwareControllerPlugin
  {
    private PowerRecovery m_oPrinterPowerRecovery;
    private float m_fLocationX;
    private float m_fLocationY;
    private int m_iPI_ProgressIndicator;
    private IPublicFirmwareController m_oFirmwareController;
    private PowerRecoveryPlugin.RecoverySpoolerPrintCallback m_OnRecoverSpooledPrint;

    public PowerRecoveryPlugin(PowerRecovery powerRecovery, IPublicFirmwareController firmwareController, PowerRecoveryPlugin.RecoverySpoolerPrintCallback OnRecoverSpooledPrint)
    {
      m_oPrinterPowerRecovery = powerRecovery;
      m_oFirmwareController = firmwareController;
      m_OnRecoverSpooledPrint = OnRecoverSpooledPrint;
    }

    public string ID
    {
      get
      {
        return "INTERNAL::PowerRecoveryPlugin";
      }
    }

    public CommandResult ClearPowerRecoveryFault(bool bResetOnPrinter = true)
    {
      m_oPrinterPowerRecovery.PrintingStatus = PowerRecovery.PowerResetState.NotPrinting;
      m_oPrinterPowerRecovery.iRC_Code = 0;
      m_oPrinterPowerRecovery.iRR_Code = 0;
      m_fLocationX = float.NaN;
      m_fLocationY = float.NaN;
      m_iPI_ProgressIndicator = 0;
      if (!bResetOnPrinter)
      {
        return CommandResult.Success;
      }

      return ResetRecoveryInfoOnPrinter();
    }

    public CommandResult RecoveryPrintFromPowerFailure(bool bHomingRequired)
    {
      if (m_oPrinterPowerRecovery.PrintingStatus == PowerRecovery.PowerResetState.NotPrinting)
      {
        return CommandResult.Failed_Exception;
      }

      PrinterInfo currentPrinterInfo = m_oFirmwareController.CurrentPrinterInfo;
      if (currentPrinterInfo.persistantData.SavedJobInformation == null || currentPrinterInfo.persistantData.SavedJobInformation.Params == null)
      {
        return CommandResult.Failed_RequiredDataNotAvailable;
      }

      JobParams jobParams = currentPrinterInfo.persistantData.SavedJobInformation.Params;
      FilamentSpool filament;
      if (jobParams.filament_temperature > 0 && jobParams.filament_type != FilamentSpool.TypeEnum.OtherOrUnknown && jobParams.filament_type != FilamentSpool.TypeEnum.NoFilament)
      {
        filament = new FilamentSpool(jobParams.filament_type, jobParams.filament_temperature);
      }
      else if (null != currentPrinterInfo.filament_info)
      {
        filament = new FilamentSpool(currentPrinterInfo.filament_info);
      }
      else
      {
        FilamentSpool.TypeEnum filamentTypeFromName = FirmwareSDPrintJob.GetFilamentTypeFromName(jobParams.gcodefile);
        switch (filamentTypeFromName)
        {
          case FilamentSpool.TypeEnum.NoFilament:
          case FilamentSpool.TypeEnum.OtherOrUnknown:
            return CommandResult.Failed_RequiredDataNotAvailable;
          default:
            filament = new FilamentSpool(filamentTypeFromName, FilamentConstants.Temperature.Default(filamentTypeFromName));
            break;
        }
      }
      var filamentProfile = FilamentProfile.CreateFilamentProfile(currentPrinterInfo.filament_info, m_oFirmwareController.MyPrinterProfile);
      PrepareForPrinting(jobParams, filament, filamentProfile, bHomingRequired || float.IsNaN(m_fLocationX) || float.IsNaN(m_fLocationY));
      CommandResult commandResult = PowerRecovery.PowerResetState.PowerFailureSDPrint != m_oPrinterPowerRecovery.PrintingStatus ? RecoverSpooledPrint(currentPrinterInfo.persistantData.SavedJobInformation) : RecoverSDPrint(jobParams);
      if (commandResult == CommandResult.Success)
      {
        var num = (int)ClearPowerRecoveryFault(true);
      }
      else
      {
        m_oFirmwareController.SendEmergencyStop();
      }

      return commandResult;
    }

    private void PrepareForPrinting(JobParams jobParams, FilamentSpool filament, FilamentProfile filamentProfile, bool bHomingRequired)
    {
      var stringList = new List<string>
      {
        string.Format("M106 S170"),
        string.Format("M109 S{0}", filament.filament_temperature),
        string.Format("M106 S255"),
        string.Format("M114"),
        string.Format("M117")
      };
      if (bHomingRequired)
      {
        stringList.Add(string.Format("G91"));
        stringList.Add(string.Format("G0 Z2"));
        stringList.Add(string.Format("G28"));
      }
      else
      {
        stringList.Add(string.Format("G92 X{0} Y{1}", m_fLocationX, m_fLocationY));
        stringList.Add(string.Format("M114"));
      }
      if (jobParams.options.use_heated_bed)
      {
        stringList.Add(string.Format("M190 S{0}", filamentProfile.preprocessor.initialPrint.BedTemperature));
      }

      if (bHomingRequired)
      {
        stringList.Add(string.Format("G91"));
        stringList.Add(string.Format("G0 Z-2"));
      }
      stringList.Add(string.Format("G90"));
      var num = (int)m_oFirmwareController.WriteManualCommands(stringList.ToArray());
    }

    private CommandResult ResetRecoveryInfoOnPrinter()
    {
      m_oFirmwareController.AddManualCommandToFront(new List<string>()
      {
        string.Format("M405")
      }.ToArray());
      return CommandResult.Success;
    }

    private CommandResult RecoverSDPrint(JobParams jobParams)
    {
      var num = (int)m_oFirmwareController.WriteManualCommands(new List<string>() { string.Format("M23 {0}", jobParams.gcodefile), string.Format("M26 S{0}", m_iPI_ProgressIndicator), string.Format("M24"), string.Format("M27") }.ToArray());
      return CommandResult.Success;
    }

    private CommandResult RecoverSpooledPrint(PersistantJobData jobData)
    {
      var currentLineNumber = (long) jobData.CurrentLineNumber;
      var progressIndicator = m_iPI_ProgressIndicator;
      var num = (int) ((ulong) currentLineNumber % 65536UL);
      if (progressIndicator > num)
      {
        progressIndicator -= 65536;
      }

      var ulLineToSkipTo = (ulong) currentLineNumber + (ulong) (progressIndicator - num);
      return m_OnRecoverSpooledPrint(jobData.Params, ulLineToSkipTo);
    }

    public void ProcessGCodeResult(GCode gcode, string resultFromPrinter, PrinterInfo printerInfo)
    {
      if (!gcode.hasM || gcode.M != 404)
      {
        return;
      }

      var num1 = (int)ClearPowerRecoveryFault(false);
      foreach (Match match in new Regex("(\\w*):(\\d+\\.?\\d?)").Matches(resultFromPrinter))
      {
        var str1 = match.Groups[2].Value;
        var str2 = match.Groups[1].Value;
        if (!(str2 == "RC"))
        {
          if (!(str2 == "RR"))
          {
            if (!(str2 == "PS"))
            {
              if (!(str2 == "PI"))
              {
                if (!(str2 == "LX"))
                {
                  if (str2 == "LY")
                  {
                    m_fLocationY = SafeToFloat(match.Groups[2].Value);
                  }
                }
                else
                {
                  m_fLocationX = SafeToFloat(match.Groups[2].Value);
                }
              }
              else
              {
                m_iPI_ProgressIndicator = SafeToInt(match.Groups[2].Value);
              }
            }
            else
            {
              var num2 = SafeToInt(match.Groups[2].Value);
              if (3 <= num2)
              {
                num2 = 0;
              }

              m_oPrinterPowerRecovery.PrintingStatus = (PowerRecovery.PowerResetState) num2;
            }
          }
          else
          {
            m_oPrinterPowerRecovery.iRR_Code = SafeToInt(match.Groups[2].Value);
          }
        }
        else
        {
          m_oPrinterPowerRecovery.iRC_Code = SafeToInt(match.Groups[2].Value);
        }
      }
    }

    public void RegisterGCodes(IGCodePluginable controller)
    {
      var num = (int) controller.LinkGCodeWithPlugin("M404", ID);
    }

    private int SafeToInt(string value)
    {
      if (int.TryParse(value, out var result))
      {
        return result;
      }

      return 0;
    }

    private float SafeToFloat(string value)
    {
      if (float.TryParse(value, out var result))
      {
        return result;
      }

      return float.NaN;
    }

    public delegate CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo);
  }
}
