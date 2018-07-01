// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.BootloaderController
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Boot;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Embedded_Firmware;
using M3D.Spooling.FirstRunUpdates;
using M3D.Spooling.Printer_Profiles;
using M3D.Spooling.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace M3D.Spooling.Core.Controllers
{
  internal class BootloaderController : BaseController
  {
    private BootloaderController.BootLoaderStep m_eBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
    private object m_oLockBootloaderStep = new object();
    private object m_oLockBootloaderWait = new object();
    private Bootloader m_oBootloaderConnection;
    private BootloaderController.BootloaderInterfaceVersion mInterfaceVersion;
    private Dictionary<char, BootloaderController.FirmwareStatus> mCheckedFirmwareStatus;
    public const char DefaultBootloaderID = 'M';

    public BootloaderController(int bootloader_version, PrinterConnection base_printer, PrinterInfo info, Logger logger, ThreadSafeVariable<bool> shared_shutdown, IBroadcastServer broadcastserver, InternalPrinterProfile printerProfile)
      : base(base_printer, info, logger, broadcastserver, printerProfile)
    {
      this.PersistantDetails.bootloader_version = bootloader_version;
      if (bootloader_version >= 1 && bootloader_version <= 5)
      {
        this.mInterfaceVersion = BootloaderController.BootloaderInterfaceVersion.V1;
      }
      else
      {
        if (bootloader_version < 6 || bootloader_version > 8)
          throw new Exception(string.Format("Unsupported Bootloader Interface - {0}", (object) bootloader_version));
        this.mInterfaceVersion = BootloaderController.BootloaderInterfaceVersion.V2;
      }
      this.mCheckedFirmwareStatus = new Dictionary<char, BootloaderController.FirmwareStatus>();
      foreach (KeyValuePair<char, FirmwareDetails> firmware in this.MyPrinterProfile.ProductConstants.FirmwareList)
        this.mCheckedFirmwareStatus.Add(firmware.Key, BootloaderController.FirmwareStatus.Unknown);
      this.StartBootloaderMode();
    }

    public override void DoConnectionLogic()
    {
      this.DoBootLoop();
      Thread.Sleep(10);
    }

    public override void Shutdown()
    {
      base.Shutdown();
    }

    private BootloaderController.BootLoaderStep CurrentBootloaderStep
    {
      get
      {
        lock (this.m_oLockBootloaderStep)
          return this.m_eBootloaderStep;
      }
      set
      {
        lock (this.m_oLockBootloaderStep)
          this.m_eBootloaderStep = value;
      }
    }

    public override CommandResult WriteManualCommands(params string[] commands)
    {
      if (commands.Length != 1)
        throw new ArgumentException("Must always send exactly one command to the bootloader");
      if (this.Status != PrinterStatus.Bootloader_Ready && commands[0].Contains("Q"))
      {
        this.WriteLog(">> Unable to start printer because it not ready.", Logger.TextType.Error);
        return CommandResult.Failed_PrinterInUnconfiguredState;
      }
      if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
      {
        if (commands[0] == "Q")
        {
          this.GotoFirmware();
        }
        else
        {
          lock (this.m_oLockBootloaderWait)
          {
            this.m_oBootloaderConnection.SPout(commands[0]);
            this.WriteLog("<< " + commands[0], Logger.TextType.Write);
          }
        }
      }
      return CommandResult.Success;
    }

    public bool WriteSerialdata(byte[] data)
    {
      lock (this.m_oLockBootloaderWait)
      {
        if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
        {
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.ProcessingRequest;
          this.m_oBootloaderConnection.FlushIncomingBytes();
          foreach (byte output in data)
            this.m_oBootloaderConnection.SPout(output);
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        }
      }
      return true;
    }

    public string WriteSerialdata(byte[] data, int getbytes)
    {
      lock (this.m_oLockBootloaderWait)
      {
        if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
        {
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.ProcessingRequest;
          Thread.Sleep(500);
          this.m_oBootloaderConnection.FlushIncomingBytes();
          foreach (byte output in data)
            this.m_oBootloaderConnection.SPout(output);
          byte[] numArray1 = new byte[1];
          byte[] inArray = new byte[getbytes];
          for (int index = 0; index < getbytes; ++index)
          {
            byte[] numArray2 = this.m_oBootloaderConnection.ReadBytes(1);
            if (index != 0 || numArray2[0] != (byte) 13)
              inArray[index] = numArray2[0];
            else
              --index;
          }
          if (this.m_oBootloaderConnection.ReadBytes(1)[0] != (byte) 13)
            return "FAIL";
          this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.RawData, this.MySerialNumber, Convert.ToBase64String(inArray)).Serialize());
        }
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
      }
      return "OK";
    }

    private void StartBootloaderMode()
    {
      this.m_oBootloaderConnection = new Bootloader(this.SerialPort, this.MyPrinterProfile.ProductConstants.m_yPaddingByte, this.MyPrinterProfile.ProductConstants.chipData, this.MyPrinterProfile.EEPROMConstants.EndOfBootloaderReadableEEPROM, this.MyPrinterProfile.EEPROMConstants.BytesPerEEPROMAddress);
      this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Startup;
      this.Status = PrinterStatus.Bootloader_StartingUp;
      this.m_oBootloaderConnection.FlushIncomingBytes();
    }

    private void DoBootLoop()
    {
      if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.Startup)
        this.BootLoaderStartup();
      else if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.CheckFirmware)
        this.CheckFirmwareStep();
      else if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.UpdateFirmware || this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.ForceFirmwareUpdate)
        this.UpdateFirmwareStep();
      else if (this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.GotoApp)
      {
        this.QuitBootloaderAndGotoApp();
      }
      else
      {
        if (this.CurrentBootloaderStep != BootloaderController.BootLoaderStep.Waiting)
          return;
        lock (this.m_oLockBootloaderWait)
        {
          string str = "";
          try
          {
            str = this.ReadExisting();
          }
          catch (TimeoutException ex)
          {
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in BootloaderConnection.DoBootLoop " + ex.Message, ex);
          }
          if (string.IsNullOrEmpty(str))
            return;
          this.WriteLog(">> " + str, Logger.TextType.Read);
        }
      }
    }

    private void BootLoaderStartup()
    {
      FirmwareDetails firmware = this.MyPrinterProfile.ProductConstants.FirmwareList['M'];
      if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
        this.SetBootloader('M');
      this.MyPrinterInfo.hardware.machine_type = "The_Micro";
      EEPROMMapping eepromMapping = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      bool flag = false;
      string str = "";
      byte[] bytesFromLocation = eepromMapping.GetBytesFromLocation("SerialNumber", 16);
      for (int index = 0; index < 16; ++index)
      {
        if (bytesFromLocation[index] == byte.MaxValue)
          flag = true;
        str += ((char) bytesFromLocation[index]).ToString();
      }
      if (flag)
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, this.MySerialNumber, "There was a major error in your printer. Please reset the printer. If the problem persist, contact M3D.").Serialize());
        this.Status = PrinterStatus.Error_PrinterNotAlive;
      }
      else
      {
        this.MySerialNumber = new PrinterSerialNumber(str);
        foreach (IFirstRunUpdater updater in this.MyPrinterProfile.FirstRunConstants.updater_list)
          updater.CheckForUpdate(str, eepromMapping.GetAllEEPROMData(), this.m_oBootloaderConnection, this.MyPrinterProfile);
        this.logger.ResetWithSerialNumber(this.MySerialNumber.ToString());
        this.LoadPersistantData();
        this.PersistantDetails.hours_used = eepromMapping.GetFloat("HoursCounterSpooler");
        this.HardwareDetails.firmware_version = eepromMapping.GetUInt32("FirmwareVersion");
        PersistantData.RestartOptions restartOptions = this.PersistantDetails.PopRestartAction();
        this.SavePersistantData();
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.CheckFirmware;
        if (restartOptions.RestartAction == PersistantData.RestartAction.SetExtruderCurrent)
          this.SetExtruderCurrent((ushort) restartOptions.RestartActionParam);
        else if (restartOptions.RestartAction == PersistantData.RestartAction.SetFan)
          this.SetFanConstants((FanConstValues.FanType) restartOptions.RestartActionParam);
        else if (restartOptions.RestartAction == PersistantData.RestartAction.ForceStayBootloader)
        {
          this.Status = PrinterStatus.Bootloader_Ready;
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        }
        else
        {
          if (restartOptions.RestartAction != PersistantData.RestartAction.ForceUpdateFirmware)
            return;
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.ForceFirmwareUpdate;
        }
      }
    }

    private void SetBootloader(char bootloaderID)
    {
      this.m_oBootloaderConnection.SPout((byte) 84);
      this.m_oBootloaderConnection.SPout((byte) bootloaderID);
      if (this.m_oBootloaderConnection.ReadBytes(1)[0] != (byte) 13)
        throw new Exception(string.Format("Unable to switch to bootloader {0}", (object) bootloaderID));
    }

    private void CheckFirmwareStep()
    {
      foreach (KeyValuePair<char, FirmwareDetails> firmware in this.MyPrinterProfile.ProductConstants.FirmwareList)
      {
        char key = firmware.Key;
        FirmwareDetails firmwareDetails = firmware.Value;
        if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
          this.SetBootloader(key);
        EEPROMMapping eepromMapping = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
        uint uint32_1 = eepromMapping.GetUInt32("FirmwareVersion");
        uint uint32_2 = eepromMapping.GetUInt32("FirmwareCRC");
        BootloaderController.FirmwareStatus firmwareStatus = this.BootloadCheckFirmware(firmwareDetails, uint32_1, uint32_2);
        if (this.mCheckedFirmwareStatus.ContainsKey(key))
          this.mCheckedFirmwareStatus[key] = firmwareStatus;
        else
          this.mCheckedFirmwareStatus.Add(key, firmwareStatus);
      }
      bool bFirmwareIsInvalid = false;
      foreach (KeyValuePair<char, BootloaderController.FirmwareStatus> checkedFirmwareStatu in this.mCheckedFirmwareStatus)
      {
        if (BootloaderController.FirmwareStatus.Bad == checkedFirmwareStatu.Value)
        {
          bFirmwareIsInvalid = true;
          break;
        }
      }
      this.SetNextActionFromFirmwareStatus(bFirmwareIsInvalid);
    }

    private BootloaderController.FirmwareStatus BootloadCheckFirmware(FirmwareDetails firmwareDetails, uint firmwareVersionFromEEPROM, uint firmwareCRCFromEEPROM)
    {
      bool flag1 = firmwareVersionFromEEPROM < 150994944U;
      try
      {
        this.ValidateCRCWithPrinter(firmwareCRCFromEEPROM);
      }
      catch (Exception ex)
      {
        this.WriteLog(ex.Message, Logger.TextType.Error);
        if (ex.Message == "Firmware failed redundancy check.")
          flag1 = true;
        else if (ex.Message == "Unsupported Bootloader Interface")
          throw ex;
      }
      bool flag2 = SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE && (int) firmwareVersionFromEEPROM != (int) firmwareDetails.firmware_version;
      if (!(flag1 | flag2))
        return BootloaderController.FirmwareStatus.Good;
      this.Status = PrinterStatus.Bootloader_InvalidFirmware;
      return BootloaderController.FirmwareStatus.Bad;
    }

    private void SetNextActionFromFirmwareStatus(bool bFirmwareIsInvalid)
    {
      if (!bFirmwareIsInvalid)
      {
        if (!SpoolerServer.StayInBootloader)
        {
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
        }
        else
        {
          this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
          this.Status = PrinterStatus.Bootloader_Ready;
        }
      }
      else if (SpoolerServer.AUTO_UPDATE_FIRMWARE)
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.UpdateFirmware;
      else
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
    }

    private void UpdateFirmwareStep()
    {
      bool flag = false;
      foreach (KeyValuePair<char, FirmwareDetails> firmware in this.MyPrinterProfile.ProductConstants.FirmwareList)
      {
        FirmwareDetails firmwareDetails = firmware.Value;
        char key = firmware.Key;
        if (!this.mCheckedFirmwareStatus.ContainsKey(key) || this.mCheckedFirmwareStatus[key] != BootloaderController.FirmwareStatus.Good && this.mCheckedFirmwareStatus[key] != BootloaderController.FirmwareStatus.GoodQuit)
        {
          if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
            this.SetBootloader(key);
          try
          {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            flag = this.BootloaderUpdateFirmware(firmwareDetails);
            stopwatch.Stop();
            this.WriteLog("<< time: " + (object) stopwatch.Elapsed, Logger.TextType.Write);
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in BootloaderConnection.DoBootLoop " + ex.Message, ex);
            flag = true;
          }
          if (!flag)
            this.mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.Good;
          if (flag)
            break;
        }
      }
      if (flag)
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareUpdateFailed, this.MySerialNumber, "null").Serialize());
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        this.Status = PrinterStatus.Bootloader_FirmwareUpdateFailed;
      }
      else
      {
        this.BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareUpdateComplete, this.MySerialNumber, "null").Serialize());
        this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
      }
    }

    private bool BootloaderUpdateFirmware(FirmwareDetails firmwareDetails)
    {
      bool flag = true;
      if ((this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.ForceFirmwareUpdate || this.CurrentBootloaderStep == BootloaderController.BootLoaderStep.UpdateFirmware || this.MyPrinterInfo.FirmwareIsInvalid) && !string.IsNullOrEmpty(firmwareDetails.embedded_firmware))
      {
        Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(firmwareDetails.embedded_firmware);
        if (manifestResourceStream == null)
        {
          this.WriteLog("Embedded Firmware Resource Not Found", Logger.TextType.Error);
          flag = true;
        }
        else
        {
          try
          {
            byte[] firmwareBytesFromStream = this.GetFirmwareBytesFromStream(manifestResourceStream, this.MyPrinterProfile.ProductConstants.m_uFirmwareMaxSizeBytes);
            this.Status = PrinterStatus.Bootloader_UpdatingFirmware;
            this.WriteFirmwareVersionToEEPROM(0U, 0U);
            this.m_oBootloaderConnection.UploadNewFirmware(firmwareBytesFromStream, firmwareDetails.firmware_crc);
            this.CheckEEPROMValuesAfterUpdate();
            this.WriteFirmwareVersionToEEPROM(firmwareDetails.firmware_version, firmwareDetails.firmware_crc);
            this.ValidateCRCWithPrinter(firmwareDetails.firmware_crc);
            flag = false;
          }
          catch (Exception ex)
          {
            this.WriteLog(string.Format("Error updating firmware:{0}", (object) ex.Message), Logger.TextType.Error);
            this.Status = PrinterStatus.Bootloader_FirmwareUpdateFailed;
            flag = true;
          }
        }
      }
      return flag;
    }

    private byte[] GetFirmwareBytesFromStream(Stream input, uint maxFirmwareSize)
    {
      byte[] numArray1 = new byte[(int) maxFirmwareSize];
      for (int index = 0; (long) index < (long) maxFirmwareSize; ++index)
        numArray1[index] = byte.MaxValue;
      int num = input.ReadByte();
      int index1 = 0;
      while (num >= 0)
      {
        numArray1[index1] = (byte) ((int) byte.MaxValue & num);
        num = input.ReadByte();
        ++index1;
      }
      input.Dispose();
      int length = index1;
      byte[] numArray2 = new byte[length];
      for (int index2 = 0; index2 < length; ++index2)
        numArray2[index2] = numArray1[index2];
      return numArray2;
    }

    private void ValidateCRCWithPrinter(uint expectedCRC)
    {
      if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V1)
      {
        if ((int) expectedCRC != (int) this.m_oBootloaderConnection.GetCRCFromChip(CRC_Type.App))
          throw new Exception("Firmware failed redundancy check.");
      }
      else
      {
        if (this.mInterfaceVersion != BootloaderController.BootloaderInterfaceVersion.V2)
          throw new Exception("Unsupported Bootloader Interface");
        this.m_oBootloaderConnection.SPout((byte) 75);
        this.m_oBootloaderConnection.SPout((byte) 65);
        byte[] numArray = this.m_oBootloaderConnection.ReadBytes(3);
        string str = string.Format("{0}{1}{2}", (object) (char) numArray[0], (object) (char) numArray[1], (object) (char) numArray[2]);
        if (str == "ok\r")
          return;
        if (str == "no\r")
          throw new Exception("Firmware failed redundancy check.");
        throw new Exception(string.Format("'K' gave an unexpected result - {0}", (object) str));
      }
    }

    public void WriteFirmwareVersionToEEPROM(uint firmware_version, uint firmware_crc)
    {
      this.HardwareDetails.firmware_version = firmware_version;
      byte[] bytes1 = BitConverter.GetBytes(this.HardwareDetails.firmware_version);
      byte[] bytes2 = BitConverter.GetBytes(firmware_crc);
      this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("FirmwareVersion"), bytes1);
      this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("FirmwareCRC"), bytes2);
    }

    private void CheckEEPROMValuesAfterUpdate()
    {
      EEPROMMapping eepromMapping = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      int uint32 = (int) eepromMapping.GetUInt32("FirmwareVersion");
      byte[] bytes = new byte[4];
      if (eepromMapping.GetUInt16("SavedZState") != (ushort) 0)
        return;
      this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("LastRecordedZValue"), bytes);
    }

    private bool QuitBootloaderAndGotoApp()
    {
      bool flag = true;
      foreach (KeyValuePair<char, FirmwareDetails> firmware in this.MyPrinterProfile.ProductConstants.FirmwareList)
      {
        FirmwareDetails firmwareDetails = firmware.Value;
        char key = firmware.Key;
        if (this.mCheckedFirmwareStatus.ContainsKey(key))
        {
          switch (this.mCheckedFirmwareStatus[key])
          {
            case BootloaderController.FirmwareStatus.Unknown:
            case BootloaderController.FirmwareStatus.Good:
              if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
              {
                this.SetBootloader(key);
                if (key != 'M')
                {
                  flag = this.QuitSecondaryBootloader();
                  break;
                }
                break;
              }
              break;
            case BootloaderController.FirmwareStatus.GoodQuit:
              continue;
            default:
              flag = false;
              break;
          }
          if (flag)
          {
            this.mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.GoodQuit;
          }
          else
          {
            this.mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.Bad;
            return false;
          }
        }
      }
      this.broadcast_shutdown = false;
      this.m_oBootloaderConnection.FlushIncomingBytes();
      this.WriteLog("<< Q", Logger.TextType.Write);
      if (!this.m_oBootloaderConnection.JumpToApplication())
        return false;
      this.Status = PrinterStatus.Error_PrinterNotAlive;
      return true;
    }

    private bool QuitSecondaryBootloader()
    {
      this.m_oBootloaderConnection.FlushIncomingBytes();
      this.m_oBootloaderConnection.SPout((byte) 81);
      byte[] numArray1 = this.m_oBootloaderConnection.ReadBytes(1);
      if (numArray1[0] == (byte) 13)
        return true;
      byte[] numArray2 = this.m_oBootloaderConnection.ReadBytes(2);
      string str = string.Format("{0}{1}", (object) (char) numArray1[0], (object) (char) numArray2[1]);
      if (str == "no\r")
        return false;
      throw new Exception(string.Format("'Q' gave an unexpected result - {0}", (object) str));
    }

    private void SwitchBootloaderToApplication(bool bIsOnMBoard)
    {
      this.m_oBootloaderConnection.SPout((byte) 81);
      if (this.mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V1)
        return;
      if (this.mInterfaceVersion != BootloaderController.BootloaderInterfaceVersion.V2)
        throw new Exception("Unsupported Bootloader Interface");
      try
      {
        byte[] numArray1 = this.m_oBootloaderConnection.ReadBytes(1);
        if (numArray1[0] == (byte) 13)
          return;
        byte[] numArray2 = this.m_oBootloaderConnection.ReadBytes(2);
        string str = string.Format("{0}{1}", (object) (char) numArray1[0], (object) (char) numArray2[1]);
        if (str == "no\r")
          throw new Exception("CRC Failure");
        throw new Exception(string.Format("'Q' gave an unexpected result - {0}", (object) str));
      }
      catch (IOException ex)
      {
        if (!bIsOnMBoard)
          throw new Exception("Unexpected IO Failure");
      }
    }

    public void GotoFirmware()
    {
      if (this.Status != PrinterStatus.Bootloader_Ready)
        return;
      this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
    }

    public override void UpdateFirmware()
    {
      this.CurrentBootloaderStep = BootloaderController.BootLoaderStep.UpdateFirmware;
    }

    public override void SetFanConstants(FanConstValues.FanType fanType)
    {
      EEPROMMapping eepromMapping1 = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      int alignedByte1 = (int) eepromMapping1.GetAlignedByte("FANTYPE");
      ushort alignedByte2 = eepromMapping1.GetAlignedByte("FANOFFSET");
      float num1 = eepromMapping1.GetFloat("FANSCALE");
      FanConstValues.FanValues fanConstant = FanConstValues.FanConstants[fanType];
      int num2 = (int) (byte) fanType;
      if (alignedByte1 != num2)
        this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("FANTYPE"), eepromMapping1.AlignedByteToBytaArray((ushort) fanType));
      if ((int) alignedByte2 != fanConstant.Offset)
        this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("FANOFFSET"), eepromMapping1.AlignedByteToBytaArray((ushort) fanConstant.Offset));
      if ((double) num1 != (double) fanConstant.Scale)
        this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("FANSCALE"), BitConverter.GetBytes(fanConstant.Scale));
      EEPROMMapping eepromMapping2 = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      int alignedByte3 = (int) eepromMapping1.GetAlignedByte("FANTYPE");
      ushort alignedByte4 = eepromMapping1.GetAlignedByte("FANOFFSET");
      float num3 = eepromMapping1.GetFloat("FANSCALE");
      int num4 = (int) (byte) fanType;
      if (alignedByte3 == num4 && (int) alignedByte4 == fanConstant.Offset && (double) num3 == (double) fanConstant.Scale)
        this.WriteLog(">> ok", Logger.TextType.Read);
      else
        this.WriteLog(">> failed", Logger.TextType.Read);
    }

    public override void SetExtruderCurrent(ushort current)
    {
      EEPROMMapping eepromMapping1 = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      if ((int) eepromMapping1.GetUInt16("ExtruderCurrent") == (int) current)
        return;
      this.m_oBootloaderConnection.WriteToEEPROM(this.GetEEPROMDataLocation("ExtruderCurrent"), BitConverter.GetBytes(current));
      EEPROMMapping eepromMapping2 = new EEPROMMapping(this.m_oBootloaderConnection.ReadAllReadableEEPROM(), this.MyPrinterProfile.EEPROMConstants);
      if ((int) eepromMapping1.GetUInt16("ExtruderCurrent") == (int) current)
        this.WriteLog(">> ok", Logger.TextType.Read);
      else
        this.WriteLog(">> failed", Logger.TextType.Read);
    }

    private ushort GetEEPROMDataLocation(string name)
    {
      EepromAddressInfo eepromInfo = this.MyPrinterProfile.EEPROMConstants.GetEepromInfo(name);
      if (eepromInfo != null)
        return eepromInfo.EepromAddr;
      if (Debugger.IsAttached)
        Debugger.Break();
      return 0;
    }

    public override bool Idle
    {
      get
      {
        return !this.IsWorking;
      }
    }

    public override bool IsWorking
    {
      get
      {
        if (this.Status == PrinterStatus.Bootloader_UpdatingFirmware || this.CurrentBootloaderStep != BootloaderController.BootLoaderStep.Waiting)
          return this.CurrentBootloaderStep != BootloaderController.BootLoaderStep.GotoApp;
        return false;
      }
    }

    public override bool HasActiveJob
    {
      get
      {
        return false;
      }
    }

    public override bool IsPrinting
    {
      get
      {
        return false;
      }
    }

    public override int GetJobsCount()
    {
      return 0;
    }

    private enum BootloaderInterfaceVersion
    {
      V1,
      V2,
    }

    private enum FirmwareStatus
    {
      Unknown,
      Bad,
      Good,
      GoodQuit,
    }

    private enum BootLoaderStep
    {
      Startup,
      UpdateFirmware,
      UploadedFirmware,
      GotoApp,
      Waiting,
      ProcessingRequest,
      ForceFirmwareUpdate,
      CheckFirmware,
    }
  }
}
