// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.Plugins.PowerRecoveryPlugin
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
      this.m_oPrinterPowerRecovery = powerRecovery;
      this.m_oFirmwareController = firmwareController;
      this.m_OnRecoverSpooledPrint = OnRecoverSpooledPrint;
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
      this.m_oPrinterPowerRecovery.PrintingStatus = PowerRecovery.PowerResetState.NotPrinting;
      this.m_oPrinterPowerRecovery.iRC_Code = 0;
      this.m_oPrinterPowerRecovery.iRR_Code = 0;
      this.m_fLocationX = float.NaN;
      this.m_fLocationY = float.NaN;
      this.m_iPI_ProgressIndicator = 0;
      if (!bResetOnPrinter)
        return CommandResult.Success;
      return this.ResetRecoveryInfoOnPrinter();
    }

    public CommandResult RecoveryPrintFromPowerFailure(bool bHomingRequired)
    {
      if (this.m_oPrinterPowerRecovery.PrintingStatus == PowerRecovery.PowerResetState.NotPrinting)
        return CommandResult.Failed_Exception;
      PrinterInfo currentPrinterInfo = this.m_oFirmwareController.CurrentPrinterInfo;
      if (currentPrinterInfo.persistantData.SavedJobInformation == null || currentPrinterInfo.persistantData.SavedJobInformation.Params == null)
        return CommandResult.Failed_RequiredDataNotAvailable;
      JobParams jobParams = currentPrinterInfo.persistantData.SavedJobInformation.Params;
      FilamentSpool filament;
      if (jobParams.filament_temperature > 0 && jobParams.filament_type != FilamentSpool.TypeEnum.OtherOrUnknown && jobParams.filament_type != FilamentSpool.TypeEnum.NoFilament)
        filament = new FilamentSpool(jobParams.filament_type, jobParams.filament_temperature);
      else if ((FilamentSpool) null != currentPrinterInfo.filament_info)
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
      FilamentProfile filamentProfile = FilamentProfile.CreateFilamentProfile(currentPrinterInfo.filament_info, (PrinterProfile) this.m_oFirmwareController.MyPrinterProfile);
      this.PrepareForPrinting(jobParams, filament, filamentProfile, bHomingRequired || float.IsNaN(this.m_fLocationX) || float.IsNaN(this.m_fLocationY));
      CommandResult commandResult = PowerRecovery.PowerResetState.PowerFailureSDPrint != this.m_oPrinterPowerRecovery.PrintingStatus ? this.RecoverSpooledPrint(currentPrinterInfo.persistantData.SavedJobInformation) : this.RecoverSDPrint(jobParams);
      if (commandResult == CommandResult.Success)
      {
        int num = (int) this.ClearPowerRecoveryFault(true);
      }
      else
        this.m_oFirmwareController.SendEmergencyStop();
      return commandResult;
    }

    private void PrepareForPrinting(JobParams jobParams, FilamentSpool filament, FilamentProfile filamentProfile, bool bHomingRequired)
    {
      List<string> stringList = new List<string>();
      stringList.Add(string.Format("M106 S170"));
      stringList.Add(string.Format("M109 S{0}", (object) filament.filament_temperature));
      stringList.Add(string.Format("M106 S255"));
      stringList.Add(string.Format("M114"));
      stringList.Add(string.Format("M117"));
      if (bHomingRequired)
      {
        stringList.Add(string.Format("G91"));
        stringList.Add(string.Format("G0 Z2"));
        stringList.Add(string.Format("G28"));
      }
      else
      {
        stringList.Add(string.Format("G92 X{0} Y{1}", (object) this.m_fLocationX, (object) this.m_fLocationY));
        stringList.Add(string.Format("M114"));
      }
      if (jobParams.options.use_heated_bed)
        stringList.Add(string.Format("M190 S{0}", (object) filamentProfile.preprocessor.initialPrint.BedTemperature));
      if (bHomingRequired)
      {
        stringList.Add(string.Format("G91"));
        stringList.Add(string.Format("G0 Z-2"));
      }
      stringList.Add(string.Format("G90"));
      int num = (int) this.m_oFirmwareController.WriteManualCommands(stringList.ToArray());
    }

    private CommandResult ResetRecoveryInfoOnPrinter()
    {
      this.m_oFirmwareController.AddManualCommandToFront(new List<string>()
      {
        string.Format("M405")
      }.ToArray());
      return CommandResult.Success;
    }

    private CommandResult RecoverSDPrint(JobParams jobParams)
    {
      int num = (int) this.m_oFirmwareController.WriteManualCommands(new List<string>() { string.Format("M23 {0}", (object) jobParams.gcodefile), string.Format("M26 S{0}", (object) this.m_iPI_ProgressIndicator), string.Format("M24"), string.Format("M27") }.ToArray());
      return CommandResult.Success;
    }

    private CommandResult RecoverSpooledPrint(PersistantJobData jobData)
    {
      long currentLineNumber = (long) jobData.CurrentLineNumber;
      int progressIndicator = this.m_iPI_ProgressIndicator;
      int num = (int) ((ulong) currentLineNumber % 65536UL);
      if (progressIndicator > num)
        progressIndicator -= 65536;
      ulong ulLineToSkipTo = (ulong) currentLineNumber + (ulong) (progressIndicator - num);
      return this.m_OnRecoverSpooledPrint(jobData.Params, ulLineToSkipTo);
    }

    public void ProcessGCodeResult(GCode gcode, string resultFromPrinter, PrinterInfo printerInfo)
    {
      if (!gcode.hasM || gcode.M != (ushort) 404)
        return;
      int num1 = (int) this.ClearPowerRecoveryFault(false);
      foreach (Match match in new Regex("(\\w*):(\\d+\\.?\\d?)").Matches(resultFromPrinter))
      {
        string str1 = match.Groups[2].Value;
        string str2 = match.Groups[1].Value;
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
                    this.m_fLocationY = this.SafeToFloat(match.Groups[2].Value);
                }
                else
                  this.m_fLocationX = this.SafeToFloat(match.Groups[2].Value);
              }
              else
                this.m_iPI_ProgressIndicator = this.SafeToInt(match.Groups[2].Value);
            }
            else
            {
              int num2 = this.SafeToInt(match.Groups[2].Value);
              if (3 <= num2)
                num2 = 0;
              this.m_oPrinterPowerRecovery.PrintingStatus = (PowerRecovery.PowerResetState) num2;
            }
          }
          else
            this.m_oPrinterPowerRecovery.iRR_Code = this.SafeToInt(match.Groups[2].Value);
        }
        else
          this.m_oPrinterPowerRecovery.iRC_Code = this.SafeToInt(match.Groups[2].Value);
      }
    }

    public void RegisterGCodes(IGCodePluginable controller)
    {
      int num = (int) controller.LinkGCodeWithPlugin("M404", this.ID);
    }

    private int SafeToInt(string value)
    {
      int result;
      if (int.TryParse(value, out result))
        return result;
      return 0;
    }

    private float SafeToFloat(string value)
    {
      float result;
      if (float.TryParse(value, out result))
        return result;
      return float.NaN;
    }

    public delegate CommandResult RecoverySpoolerPrintCallback(JobParams jobParams, ulong ulLineToSkipTo);
  }
}
