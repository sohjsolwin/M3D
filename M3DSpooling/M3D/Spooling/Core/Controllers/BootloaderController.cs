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
      PersistantDetails.bootloader_version = bootloader_version;
      if (bootloader_version >= 1 && bootloader_version <= 5)
      {
        mInterfaceVersion = BootloaderController.BootloaderInterfaceVersion.V1;
      }
      else
      {
        if (bootloader_version < 6 || bootloader_version > 8)
        {
          throw new Exception(string.Format("Unsupported Bootloader Interface - {0}", (object) bootloader_version));
        }

        mInterfaceVersion = BootloaderController.BootloaderInterfaceVersion.V2;
      }
      mCheckedFirmwareStatus = new Dictionary<char, BootloaderController.FirmwareStatus>();
      foreach (KeyValuePair<char, FirmwareDetails> firmware in MyPrinterProfile.ProductConstants.FirmwareList)
      {
        mCheckedFirmwareStatus.Add(firmware.Key, BootloaderController.FirmwareStatus.Unknown);
      }

      StartBootloaderMode();
    }

    public override void DoConnectionLogic()
    {
      DoBootLoop();
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
        lock (m_oLockBootloaderStep)
        {
          return m_eBootloaderStep;
        }
      }
      set
      {
        lock (m_oLockBootloaderStep)
        {
          m_eBootloaderStep = value;
        }
      }
    }

    public override CommandResult WriteManualCommands(params string[] commands)
    {
      if (commands.Length != 1)
      {
        throw new ArgumentException("Must always send exactly one command to the bootloader");
      }

      if (Status != PrinterStatus.Bootloader_Ready && commands[0].Contains("Q"))
      {
        WriteLog(">> Unable to start printer because it not ready.", Logger.TextType.Error);
        return CommandResult.Failed_PrinterInUnconfiguredState;
      }
      if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
      {
        if (commands[0] == "Q")
        {
          GotoFirmware();
        }
        else
        {
          lock (m_oLockBootloaderWait)
          {
            m_oBootloaderConnection.SPout(commands[0]);
            WriteLog("<< " + commands[0], Logger.TextType.Write);
          }
        }
      }
      return CommandResult.Success;
    }

    public bool WriteSerialdata(byte[] data)
    {
      lock (m_oLockBootloaderWait)
      {
        if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
        {
          CurrentBootloaderStep = BootloaderController.BootLoaderStep.ProcessingRequest;
          m_oBootloaderConnection.FlushIncomingBytes();
          foreach (var output in data)
          {
            m_oBootloaderConnection.SPout(output);
          }

          CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        }
      }
      return true;
    }

    public string WriteSerialdata(byte[] data, int getbytes)
    {
      lock (m_oLockBootloaderWait)
      {
        if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.Waiting)
        {
          CurrentBootloaderStep = BootloaderController.BootLoaderStep.ProcessingRequest;
          Thread.Sleep(500);
          m_oBootloaderConnection.FlushIncomingBytes();
          foreach (var output in data)
          {
            m_oBootloaderConnection.SPout(output);
          }

          byte[] numArray1 = new byte[1];
          byte[] inArray = new byte[getbytes];
          for (var index = 0; index < getbytes; ++index)
          {
            byte[] numArray2 = m_oBootloaderConnection.ReadBytes(1);
            if (index != 0 || numArray2[0] != (byte) 13)
            {
              inArray[index] = numArray2[0];
            }
            else
            {
              --index;
            }
          }
          if (m_oBootloaderConnection.ReadBytes(1)[0] != (byte) 13)
          {
            return "FAIL";
          }

          BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.RawData, MySerialNumber, Convert.ToBase64String(inArray)).Serialize());
        }
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
      }
      return "OK";
    }

    private void StartBootloaderMode()
    {
      m_oBootloaderConnection = new Bootloader(SerialPort, MyPrinterProfile.ProductConstants.m_yPaddingByte, MyPrinterProfile.ProductConstants.chipData, MyPrinterProfile.EEPROMConstants.EndOfBootloaderReadableEEPROM, MyPrinterProfile.EEPROMConstants.BytesPerEEPROMAddress);
      CurrentBootloaderStep = BootloaderController.BootLoaderStep.Startup;
      Status = PrinterStatus.Bootloader_StartingUp;
      m_oBootloaderConnection.FlushIncomingBytes();
    }

    private void DoBootLoop()
    {
      if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.Startup)
      {
        BootLoaderStartup();
      }
      else if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.CheckFirmware)
      {
        CheckFirmwareStep();
      }
      else if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.UpdateFirmware || CurrentBootloaderStep == BootloaderController.BootLoaderStep.ForceFirmwareUpdate)
      {
        UpdateFirmwareStep();
      }
      else if (CurrentBootloaderStep == BootloaderController.BootLoaderStep.GotoApp)
      {
        QuitBootloaderAndGotoApp();
      }
      else
      {
        if (CurrentBootloaderStep != BootloaderController.BootLoaderStep.Waiting)
        {
          return;
        }

        lock (m_oLockBootloaderWait)
        {
          var str = "";
          try
          {
            str = ReadExisting();
          }
          catch (TimeoutException ex)
          {
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in BootloaderConnection.DoBootLoop " + ex.Message, ex);
          }
          if (string.IsNullOrEmpty(str))
          {
            return;
          }

          WriteLog(">> " + str, Logger.TextType.Read);
        }
      }
    }

    private void BootLoaderStartup()
    {
      FirmwareDetails firmware = MyPrinterProfile.ProductConstants.FirmwareList['M'];
      if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
      {
        SetBootloader('M');
      }

      MyPrinterInfo.hardware.machine_type = "The_Micro";
      var eepromMapping = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      var flag = false;
      var str = "";
      byte[] bytesFromLocation = eepromMapping.GetBytesFromLocation("SerialNumber", 16);
      for (var index = 0; index < 16; ++index)
      {
        if (bytesFromLocation[index] == byte.MaxValue)
        {
          flag = true;
        }

        str += ((char) bytesFromLocation[index]).ToString();
      }
      if (flag)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, MySerialNumber, "There was a major error in your printer. Please reset the printer. If the problem persist, contact M3D.").Serialize());
        Status = PrinterStatus.Error_PrinterNotAlive;
      }
      else
      {
        MySerialNumber = new PrinterSerialNumber(str);
        foreach (IFirstRunUpdater updater in MyPrinterProfile.FirstRunConstants.updater_list)
        {
          updater.CheckForUpdate(str, eepromMapping.GetAllEEPROMData(), m_oBootloaderConnection, MyPrinterProfile);
        }

        logger.ResetWithSerialNumber(MySerialNumber.ToString());
        LoadPersistantData();
        PersistantDetails.hours_used = eepromMapping.GetFloat("HoursCounterSpooler");
        HardwareDetails.firmware_version = eepromMapping.GetUInt32("FirmwareVersion");
        PersistantData.RestartOptions restartOptions = PersistantDetails.PopRestartAction();
        SavePersistantData();
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.CheckFirmware;
        if (restartOptions.RestartAction == PersistantData.RestartAction.SetExtruderCurrent)
        {
          SetExtruderCurrent((ushort) restartOptions.RestartActionParam);
        }
        else if (restartOptions.RestartAction == PersistantData.RestartAction.SetFan)
        {
          SetFanConstants((FanConstValues.FanType) restartOptions.RestartActionParam);
        }
        else if (restartOptions.RestartAction == PersistantData.RestartAction.ForceStayBootloader)
        {
          Status = PrinterStatus.Bootloader_Ready;
          CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        }
        else
        {
          if (restartOptions.RestartAction != PersistantData.RestartAction.ForceUpdateFirmware)
          {
            return;
          }

          CurrentBootloaderStep = BootloaderController.BootLoaderStep.ForceFirmwareUpdate;
        }
      }
    }

    private void SetBootloader(char bootloaderID)
    {
      m_oBootloaderConnection.SPout((byte) 84);
      m_oBootloaderConnection.SPout((byte) bootloaderID);
      if (m_oBootloaderConnection.ReadBytes(1)[0] != (byte) 13)
      {
        throw new Exception(string.Format("Unable to switch to bootloader {0}", (object) bootloaderID));
      }
    }

    private void CheckFirmwareStep()
    {
      foreach (KeyValuePair<char, FirmwareDetails> firmware in MyPrinterProfile.ProductConstants.FirmwareList)
      {
        var key = firmware.Key;
        FirmwareDetails firmwareDetails = firmware.Value;
        if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
        {
          SetBootloader(key);
        }

        var eepromMapping = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
        var uint32_1 = eepromMapping.GetUInt32("FirmwareVersion");
        var uint32_2 = eepromMapping.GetUInt32("FirmwareCRC");
        BootloaderController.FirmwareStatus firmwareStatus = BootloadCheckFirmware(firmwareDetails, uint32_1, uint32_2);
        if (mCheckedFirmwareStatus.ContainsKey(key))
        {
          mCheckedFirmwareStatus[key] = firmwareStatus;
        }
        else
        {
          mCheckedFirmwareStatus.Add(key, firmwareStatus);
        }
      }
      var bFirmwareIsInvalid = false;
      foreach (KeyValuePair<char, BootloaderController.FirmwareStatus> checkedFirmwareStatu in mCheckedFirmwareStatus)
      {
        if (BootloaderController.FirmwareStatus.Bad == checkedFirmwareStatu.Value)
        {
          bFirmwareIsInvalid = true;
          break;
        }
      }
      SetNextActionFromFirmwareStatus(bFirmwareIsInvalid);
    }

    private BootloaderController.FirmwareStatus BootloadCheckFirmware(FirmwareDetails firmwareDetails, uint firmwareVersionFromEEPROM, uint firmwareCRCFromEEPROM)
    {
      var flag1 = firmwareVersionFromEEPROM < 150994944U;
      try
      {
        ValidateCRCWithPrinter(firmwareCRCFromEEPROM);
      }
      catch (Exception ex)
      {
        WriteLog(ex.Message, Logger.TextType.Error);
        if (ex.Message == "Firmware failed redundancy check.")
        {
          flag1 = true;
        }
        else if (ex.Message == "Unsupported Bootloader Interface")
        {
          throw ex;
        }
      }
      var flag2 = SpoolerServer.CHECK_INCOMPATIBLE_FIRMWARE && (int) firmwareVersionFromEEPROM != (int) firmwareDetails.firmware_version;
      if (!(flag1 | flag2))
      {
        return BootloaderController.FirmwareStatus.Good;
      }

      Status = PrinterStatus.Bootloader_InvalidFirmware;
      return BootloaderController.FirmwareStatus.Bad;
    }

    private void SetNextActionFromFirmwareStatus(bool bFirmwareIsInvalid)
    {
      if (!bFirmwareIsInvalid)
      {
        if (!SpoolerServer.StayInBootloader)
        {
          CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
        }
        else
        {
          CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
          Status = PrinterStatus.Bootloader_Ready;
        }
      }
      else if (SpoolerServer.AUTO_UPDATE_FIRMWARE)
      {
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.UpdateFirmware;
      }
      else
      {
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
      }
    }

    private void UpdateFirmwareStep()
    {
      var flag = false;
      foreach (KeyValuePair<char, FirmwareDetails> firmware in MyPrinterProfile.ProductConstants.FirmwareList)
      {
        FirmwareDetails firmwareDetails = firmware.Value;
        var key = firmware.Key;
        if (!mCheckedFirmwareStatus.ContainsKey(key) || mCheckedFirmwareStatus[key] != BootloaderController.FirmwareStatus.Good && mCheckedFirmwareStatus[key] != BootloaderController.FirmwareStatus.GoodQuit)
        {
          if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
          {
            SetBootloader(key);
          }

          try
          {
            var stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            flag = BootloaderUpdateFirmware(firmwareDetails);
            stopwatch.Stop();
            WriteLog("<< time: " + (object) stopwatch.Elapsed, Logger.TextType.Write);
          }
          catch (Exception ex)
          {
            ErrorLogger.LogException("Exception in BootloaderConnection.DoBootLoop " + ex.Message, ex);
            flag = true;
          }
          if (!flag)
          {
            mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.Good;
          }

          if (flag)
          {
            break;
          }
        }
      }
      if (flag)
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareUpdateFailed, MySerialNumber, "null").Serialize());
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.Waiting;
        Status = PrinterStatus.Bootloader_FirmwareUpdateFailed;
      }
      else
      {
        BroadcastServer.BroadcastMessage(new SpoolerMessage(MessageType.FirmwareUpdateComplete, MySerialNumber, "null").Serialize());
        CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
      }
    }

    private bool BootloaderUpdateFirmware(FirmwareDetails firmwareDetails)
    {
      var flag = true;
      if ((CurrentBootloaderStep == BootloaderController.BootLoaderStep.ForceFirmwareUpdate || CurrentBootloaderStep == BootloaderController.BootLoaderStep.UpdateFirmware || MyPrinterInfo.FirmwareIsInvalid) && !string.IsNullOrEmpty(firmwareDetails.embedded_firmware))
      {
        Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(firmwareDetails.embedded_firmware);
        if (manifestResourceStream == null)
        {
          WriteLog("Embedded Firmware Resource Not Found", Logger.TextType.Error);
          flag = true;
        }
        else
        {
          try
          {
            byte[] firmwareBytesFromStream = GetFirmwareBytesFromStream(manifestResourceStream, MyPrinterProfile.ProductConstants.m_uFirmwareMaxSizeBytes);
            Status = PrinterStatus.Bootloader_UpdatingFirmware;
            WriteFirmwareVersionToEEPROM(0U, 0U);
            m_oBootloaderConnection.UploadNewFirmware(firmwareBytesFromStream, firmwareDetails.firmware_crc);
            CheckEEPROMValuesAfterUpdate();
            WriteFirmwareVersionToEEPROM(firmwareDetails.firmware_version, firmwareDetails.firmware_crc);
            ValidateCRCWithPrinter(firmwareDetails.firmware_crc);
            flag = false;
          }
          catch (Exception ex)
          {
            WriteLog(string.Format("Error updating firmware:{0}", (object) ex.Message), Logger.TextType.Error);
            Status = PrinterStatus.Bootloader_FirmwareUpdateFailed;
            flag = true;
          }
        }
      }
      return flag;
    }

    private byte[] GetFirmwareBytesFromStream(Stream input, uint maxFirmwareSize)
    {
      byte[] numArray1 = new byte[(int) maxFirmwareSize];
      for (var index = 0; (long) index < (long) maxFirmwareSize; ++index)
      {
        numArray1[index] = byte.MaxValue;
      }

      var num = input.ReadByte();
      var index1 = 0;
      while (num >= 0)
      {
        numArray1[index1] = (byte) ((int) byte.MaxValue & num);
        num = input.ReadByte();
        ++index1;
      }
      input.Dispose();
      var length = index1;
      byte[] numArray2 = new byte[length];
      for (var index2 = 0; index2 < length; ++index2)
      {
        numArray2[index2] = numArray1[index2];
      }

      return numArray2;
    }

    private void ValidateCRCWithPrinter(uint expectedCRC)
    {
      if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V1)
      {
        if ((int) expectedCRC != (int)m_oBootloaderConnection.GetCRCFromChip(CRC_Type.App))
        {
          throw new Exception("Firmware failed redundancy check.");
        }
      }
      else
      {
        if (mInterfaceVersion != BootloaderController.BootloaderInterfaceVersion.V2)
        {
          throw new Exception("Unsupported Bootloader Interface");
        }

        m_oBootloaderConnection.SPout((byte) 75);
        m_oBootloaderConnection.SPout((byte) 65);
        byte[] numArray = m_oBootloaderConnection.ReadBytes(3);
        var str = string.Format("{0}{1}{2}", (object) (char) numArray[0], (object) (char) numArray[1], (object) (char) numArray[2]);
        if (str == "ok\r")
        {
          return;
        }

        if (str == "no\r")
        {
          throw new Exception("Firmware failed redundancy check.");
        }

        throw new Exception(string.Format("'K' gave an unexpected result - {0}", (object) str));
      }
    }

    public void WriteFirmwareVersionToEEPROM(uint firmware_version, uint firmware_crc)
    {
      HardwareDetails.firmware_version = firmware_version;
      byte[] bytes1 = BitConverter.GetBytes(HardwareDetails.firmware_version);
      byte[] bytes2 = BitConverter.GetBytes(firmware_crc);
      m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("FirmwareVersion"), bytes1);
      m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("FirmwareCRC"), bytes2);
    }

    private void CheckEEPROMValuesAfterUpdate()
    {
      var eepromMapping = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      var uint32 = (int) eepromMapping.GetUInt32("FirmwareVersion");
      byte[] bytes = new byte[4];
      if (eepromMapping.GetUInt16("SavedZState") != (ushort) 0)
      {
        return;
      }

      m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("LastRecordedZValue"), bytes);
    }

    private bool QuitBootloaderAndGotoApp()
    {
      var flag = true;
      foreach (KeyValuePair<char, FirmwareDetails> firmware in MyPrinterProfile.ProductConstants.FirmwareList)
      {
        FirmwareDetails firmwareDetails = firmware.Value;
        var key = firmware.Key;
        if (mCheckedFirmwareStatus.ContainsKey(key))
        {
          switch (mCheckedFirmwareStatus[key])
          {
            case BootloaderController.FirmwareStatus.Unknown:
            case BootloaderController.FirmwareStatus.Good:
              if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V2)
              {
                SetBootloader(key);
                if (key != 'M')
                {
                  flag = QuitSecondaryBootloader();
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
            mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.GoodQuit;
          }
          else
          {
            mCheckedFirmwareStatus[key] = BootloaderController.FirmwareStatus.Bad;
            return false;
          }
        }
      }
      broadcast_shutdown = false;
      m_oBootloaderConnection.FlushIncomingBytes();
      WriteLog("<< Q", Logger.TextType.Write);
      if (!m_oBootloaderConnection.JumpToApplication())
      {
        return false;
      }

      Status = PrinterStatus.Error_PrinterNotAlive;
      return true;
    }

    private bool QuitSecondaryBootloader()
    {
      m_oBootloaderConnection.FlushIncomingBytes();
      m_oBootloaderConnection.SPout((byte) 81);
      byte[] numArray1 = m_oBootloaderConnection.ReadBytes(1);
      if (numArray1[0] == (byte) 13)
      {
        return true;
      }

      byte[] numArray2 = m_oBootloaderConnection.ReadBytes(2);
      var str = string.Format("{0}{1}", (object) (char) numArray1[0], (object) (char) numArray2[1]);
      if (str == "no\r")
      {
        return false;
      }

      throw new Exception(string.Format("'Q' gave an unexpected result - {0}", (object) str));
    }

    private void SwitchBootloaderToApplication(bool bIsOnMBoard)
    {
      m_oBootloaderConnection.SPout((byte) 81);
      if (mInterfaceVersion == BootloaderController.BootloaderInterfaceVersion.V1)
      {
        return;
      }

      if (mInterfaceVersion != BootloaderController.BootloaderInterfaceVersion.V2)
      {
        throw new Exception("Unsupported Bootloader Interface");
      }

      try
      {
        byte[] numArray1 = m_oBootloaderConnection.ReadBytes(1);
        if (numArray1[0] == (byte) 13)
        {
          return;
        }

        byte[] numArray2 = m_oBootloaderConnection.ReadBytes(2);
        var str = string.Format("{0}{1}", (object) (char) numArray1[0], (object) (char) numArray2[1]);
        if (str == "no\r")
        {
          throw new Exception("CRC Failure");
        }

        throw new Exception(string.Format("'Q' gave an unexpected result - {0}", (object) str));
      }
      catch (IOException ex)
      {
        if (!bIsOnMBoard)
        {
          throw new Exception("Unexpected IO Failure");
        }
      }
    }

    public void GotoFirmware()
    {
      if (Status != PrinterStatus.Bootloader_Ready)
      {
        return;
      }

      CurrentBootloaderStep = BootloaderController.BootLoaderStep.GotoApp;
    }

    public override void UpdateFirmware()
    {
      CurrentBootloaderStep = BootloaderController.BootLoaderStep.UpdateFirmware;
    }

    public override void SetFanConstants(FanConstValues.FanType fanType)
    {
      var eepromMapping1 = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      var alignedByte1 = (int) eepromMapping1.GetAlignedByte("FANTYPE");
      var alignedByte2 = eepromMapping1.GetAlignedByte("FANOFFSET");
      var num1 = eepromMapping1.GetFloat("FANSCALE");
      FanConstValues.FanValues fanConstant = FanConstValues.FanConstants[fanType];
      var num2 = (int) (byte) fanType;
      if (alignedByte1 != num2)
      {
        m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("FANTYPE"), eepromMapping1.AlignedByteToBytaArray((ushort) fanType));
      }

      if ((int) alignedByte2 != fanConstant.Offset)
      {
        m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("FANOFFSET"), eepromMapping1.AlignedByteToBytaArray((ushort) fanConstant.Offset));
      }

      if ((double) num1 != (double) fanConstant.Scale)
      {
        m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("FANSCALE"), BitConverter.GetBytes(fanConstant.Scale));
      }

      var eepromMapping2 = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      var alignedByte3 = (int) eepromMapping1.GetAlignedByte("FANTYPE");
      var alignedByte4 = eepromMapping1.GetAlignedByte("FANOFFSET");
      var num3 = eepromMapping1.GetFloat("FANSCALE");
      var num4 = (int) (byte) fanType;
      if (alignedByte3 == num4 && (int) alignedByte4 == fanConstant.Offset && (double) num3 == (double) fanConstant.Scale)
      {
        WriteLog(">> ok", Logger.TextType.Read);
      }
      else
      {
        WriteLog(">> failed", Logger.TextType.Read);
      }
    }

    public override void SetExtruderCurrent(ushort current)
    {
      var eepromMapping1 = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      if ((int) eepromMapping1.GetUInt16("ExtruderCurrent") == (int) current)
      {
        return;
      }

      m_oBootloaderConnection.WriteToEEPROM(GetEEPROMDataLocation("ExtruderCurrent"), BitConverter.GetBytes(current));
      var eepromMapping2 = new EEPROMMapping(m_oBootloaderConnection.ReadAllReadableEEPROM(), MyPrinterProfile.EEPROMConstants);
      if ((int) eepromMapping1.GetUInt16("ExtruderCurrent") == (int) current)
      {
        WriteLog(">> ok", Logger.TextType.Read);
      }
      else
      {
        WriteLog(">> failed", Logger.TextType.Read);
      }
    }

    private ushort GetEEPROMDataLocation(string name)
    {
      EepromAddressInfo eepromInfo = MyPrinterProfile.EEPROMConstants.GetEepromInfo(name);
      if (eepromInfo != null)
      {
        return eepromInfo.EepromAddr;
      }

      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return 0;
    }

    public override bool Idle
    {
      get
      {
        return !IsWorking;
      }
    }

    public override bool IsWorking
    {
      get
      {
        if (Status == PrinterStatus.Bootloader_UpdatingFirmware || CurrentBootloaderStep != BootloaderController.BootLoaderStep.Waiting)
        {
          return CurrentBootloaderStep != BootloaderController.BootLoaderStep.GotoApp;
        }

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
