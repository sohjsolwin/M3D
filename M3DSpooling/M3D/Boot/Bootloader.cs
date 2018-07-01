// Decompiled with JetBrains decompiler
// Type: M3D.Boot.Bootloader
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Core;
using System;
using System.IO;
using System.Threading;

namespace M3D.Boot
{
  internal class Bootloader
  {
    private ISerialPortIo sp;
    private readonly byte m_yPaddingByte;
    private ChipData mChipData;
    private readonly ushort mEndOfReadableEEPROM;
    private readonly int mBytesPerEEPROMAddress;

    public Bootloader(ISerialPortIo port, byte yPaddingByte, ChipData chipData, ushort endOfReadableEEPROM, int bytesPerEEPROMAddress)
    {
      this.sp = port;
      this.m_yPaddingByte = yPaddingByte;
      this.mChipData = chipData;
      this.mEndOfReadableEEPROM = endOfReadableEEPROM;
      this.mBytesPerEEPROMAddress = bytesPerEEPROMAddress;
    }

    public void UploadNewFirmware(byte[] newFirmware, uint test_crc)
    {
      if (!this.sp.IsOpen)
        throw new Exception("Unable to write program memory.  The serial port is not open.");
      this.EraseChip();
      this.SetAddress((byte) 0, (byte) 0);
      this.WriteFirmwareToFlash(newFirmware);
      this.SetAddress((byte) 0, (byte) 0);
    }

    public bool WriteToEEPROM(ushort startAddress, byte[] bytes)
    {
      this.SPout((byte) 85);
      this.SPout((byte) ((uint) startAddress >> 8));
      this.SPout((byte) startAddress);
      ushort num = (ushort) (bytes.Length / this.mBytesPerEEPROMAddress);
      this.SPout((byte) ((uint) num >> 8));
      this.SPout((byte) num);
      for (int index = 0; index < bytes.Length; ++index)
        this.SPout(bytes[index]);
      if (this.ReadBytes(1)[0] != (byte) 13)
        throw new Exception("Error writing to EEPROM");
      return true;
    }

    public byte[] ReadAllReadableEEPROM()
    {
      this.SPout((byte) 83);
      byte[] numArray = this.ReadBytes(((int) this.mEndOfReadableEEPROM + 1) * this.mBytesPerEEPROMAddress);
      if (this.ReadBytes(1)[0] == (byte) 13)
        return numArray;
      throw new Exception("Error reading flash");
    }

    public void SPout(byte output)
    {
      this.sp.Write(new byte[1]{ output }, 0, 1);
    }

    public void SPout(string outputString)
    {
      foreach (byte output in outputString)
        this.SPout(output);
    }

    public uint GetCRCFromChip(CRC_Type crcType)
    {
      this.SPout((byte) 67);
      if (crcType == CRC_Type.App)
        this.SPout((byte) 65);
      else
        this.SPout((byte) 66);
      CRCBytes crcBytes = new CRCBytes();
      byte[] numArray = this.ReadBytes(4);
      crcBytes.Byte1 = numArray[0];
      crcBytes.Byte2 = numArray[1];
      crcBytes.Byte3 = numArray[2];
      crcBytes.Byte4 = numArray[3];
      return crcBytes.Int1;
    }

    private void EraseChip()
    {
      this.SPout((byte) 69);
      if (this.ReadBytes(1)[0] != (byte) 13)
        throw new Exception("Error erasing flash");
    }

    public byte[] ReadBytes(int bytesToRead)
    {
      while (this.sp.BytesToRead < bytesToRead)
        Thread.Sleep(10);
      byte[] bytes = new byte[bytesToRead];
      this.sp.Read(bytes, 0, bytesToRead);
      return bytes;
    }

    public void FlushIncomingBytes()
    {
      while (this.sp.BytesToRead > 0)
      {
        int num = (int) this.sp.ReadByte();
      }
    }

    private void SetAddress(byte addressByte1, byte addressByte2)
    {
      this.SPout((byte) 65);
      this.SPout(addressByte1);
      this.SPout(addressByte2);
      if (this.ReadBytes(1)[0] != (byte) 13)
        throw new Exception("After attempting to set address the micro controller did not reply correctly\r\n");
    }

    private void WriteFirmwareToFlash(byte[] newFirmware)
    {
      int num1 = newFirmware.Length / 2 / this.mChipData.PageSize;
      if (newFirmware.Length / 2 % this.mChipData.PageSize != 0)
        ++num1;
      for (int index1 = 0; index1 < num1; ++index1)
      {
        this.SPout((byte) 66);
        this.SPout((byte) (this.mChipData.PageSize * 2 >> 8 & (int) byte.MaxValue));
        this.SPout((byte) (this.mChipData.PageSize * 2 & (int) byte.MaxValue));
        Thread.Sleep(20);
        byte[] numArray = new byte[2];
        for (int index2 = 0; index2 < this.mChipData.PageSize * 2; ++index2)
        {
          int num2 = index2 + this.mChipData.PageSize * index1 * 2;
          this.SPout(num2 % 2 != 0 ? (num2 - 1 >= newFirmware.Length ? this.m_yPaddingByte : newFirmware[num2 - 1]) : (num2 + 1 >= newFirmware.Length ? this.m_yPaddingByte : newFirmware[num2 + 1]));
        }
        if (this.ReadBytes(1)[0] != (byte) 13)
          throw new Exception("Error writing flash memory\n");
        Thread.Sleep(20);
      }
    }

    public bool JumpToApplication()
    {
      try
      {
        this.SPout((byte) 81);
      }
      catch (Exception ex)
      {
        if (!(ex is IOException))
          ErrorLogger.LogErrorMsg("Exception in Bootloader.JumpToApplication " + ex.Message + " " + ex.GetType().ToString(), "Exception");
        return true;
      }
      return true;
    }
  }
}
