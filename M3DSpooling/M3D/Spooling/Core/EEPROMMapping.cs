// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.EEPROMMapping
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Printer_Profiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace M3D.Spooling.Core
{
  internal class EEPROMMapping : IDisposable
  {
    private EEPROMProfile eepromProfile;
    private byte[] eeprom_data;

    public EEPROMMapping(EEPROMProfile eepromProfile)
      : this(((int) eepromProfile.EndOfBootloaderReadableEEPROM + 1) * eepromProfile.BytesPerEEPROMAddress, eepromProfile)
    {
    }

    public EEPROMMapping(int size, EEPROMProfile eepromProfile)
    {
      this.eeprom_data = new byte[size];
      this.eepromProfile = eepromProfile;
    }

    public EEPROMMapping(byte[] data, EEPROMProfile eepromProfile)
    {
      this.eeprom_data = data;
      this.eepromProfile = eepromProfile;
    }

    public void Dispose()
    {
      this.eeprom_data = (byte[]) null;
      this.eepromProfile = (EEPROMProfile) null;
    }

    public byte[] GetAllEEPROMData()
    {
      return (byte[]) this.eeprom_data.Clone();
    }

    public bool SetValue(EepromAddressInfo eepromValue, int data)
    {
      if (eepromValue == null)
        return false;
      int eepromAddr = (int) eepromValue.EepromAddr;
      int mappedAddr = this.EepromAddrToMappedAddr(eepromAddr);
      if (mappedAddr < 0 || mappedAddr > this.eeprom_data.Length)
        return false;
      byte[] bytes = BitConverter.GetBytes(data);
      this.SetBytes(eepromAddr, bytes, eepromValue.Size);
      return true;
    }

    public bool HasKey(string name)
    {
      return this.eepromProfile.GetEepromInfo(name) != null;
    }

    public int GetLocationFromName(string name)
    {
      EepromAddressInfo eepromInfo = this.eepromProfile.GetEepromInfo(name);
      if (eepromInfo != null)
        return (int) eepromInfo.EepromAddr;
      return -1;
    }

    public bool SetFloat(string name, float value)
    {
      int locationFromName = this.GetLocationFromName(name);
      if (locationFromName < 0)
      {
        if (Debugger.IsAttached)
          Debugger.Break();
        return false;
      }
      byte[] bytes = BitConverter.GetBytes(value);
      this.SetBytes(locationFromName, bytes, ((IEnumerable<byte>) bytes).Count<byte>());
      return true;
    }

    public ushort GetAlignedByte(string name)
    {
      EepromAddressInfo eepromInfo = this.eepromProfile.GetEepromInfo(name);
      if (eepromInfo != null)
      {
        int mappedAddr = this.EepromAddrToMappedAddr((int) eepromInfo.EepromAddr);
        if (eepromInfo.Type == typeof (byte))
          return (ushort) this.eeprom_data[mappedAddr];
        if (eepromInfo.Type == typeof (ushort))
          return BitConverter.ToUInt16(this.eeprom_data, mappedAddr);
      }
      if (Debugger.IsAttached)
        Debugger.Break();
      return 0;
    }

    public uint GetUInt32(string name)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(this.GetLocationFromName(name));
      if (mappedAddr >= 0)
        return BitConverter.ToUInt32(this.eeprom_data, mappedAddr);
      if (Debugger.IsAttached)
        Debugger.Break();
      return 0;
    }

    public byte[] GetBytesFromLocation(string name, int count)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(this.GetLocationFromName(name));
      if (mappedAddr < 0)
      {
        if (Debugger.IsAttached)
          Debugger.Break();
        return (byte[]) null;
      }
      byte[] numArray = new byte[count];
      for (int index = 0; index < count; ++index)
        numArray[index] = this.eeprom_data[mappedAddr + index];
      return numArray;
    }

    public ushort GetUInt16(string name)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(this.GetLocationFromName(name));
      if (mappedAddr >= 0)
        return BitConverter.ToUInt16(this.eeprom_data, mappedAddr);
      if (Debugger.IsAttached)
        Debugger.Break();
      return 0;
    }

    public int GetInt32(string name)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(this.GetLocationFromName(name));
      if (mappedAddr >= 0)
        return BitConverter.ToInt32(this.eeprom_data, mappedAddr);
      if (Debugger.IsAttached)
        Debugger.Break();
      return 0;
    }

    public float GetFloat(string name)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(this.GetLocationFromName(name));
      if (mappedAddr < 0)
        return 0.0f;
      this.SanitizeLocation(mappedAddr);
      return BitConverter.ToSingle(this.eeprom_data, mappedAddr);
    }

    public byte[] AlignedByteToBytaArray(ushort alignedbyte)
    {
      byte[] numArray = (byte[]) null;
      if (this.eepromProfile.BytesPerEEPROMAddress == 1)
        numArray = new byte[1]{ (byte) alignedbyte };
      else if (this.eepromProfile.BytesPerEEPROMAddress == 2)
        numArray = BitConverter.GetBytes(alignedbyte);
      else if (this.eepromProfile.BytesPerEEPROMAddress == 4)
        numArray = BitConverter.GetBytes((uint) alignedbyte);
      return numArray;
    }

    public int Size
    {
      get
      {
        return this.eeprom_data.Length;
      }
    }

    public byte[] EEPROMData
    {
      get
      {
        return this.eeprom_data;
      }
    }

    public bool SaveToXMLFile(string filename)
    {
      TextWriter textWriter;
      try
      {
        textWriter = (TextWriter) new StreamWriter(filename);
      }
      catch (IOException ex)
      {
        textWriter = (TextWriter) null;
      }
      if (textWriter == null)
        return false;
      SortedList<int, EepromAddressInfo> allData = this.eepromProfile.GetAllData();
      SerializableEEPROMMapping serializableEepromMapping = new SerializableEEPROMMapping(this.eepromProfile.ProfileName);
      foreach (KeyValuePair<int, EepromAddressInfo> keyValuePair in allData)
      {
        EepromAddressInfo eepromAddressInfo = keyValuePair.Value;
        SerializableEEPROMMapping.SerializableData serializableData;
        serializableData.key = eepromAddressInfo.Name;
        serializableData.value = "";
        if ((int) eepromAddressInfo.EepromAddr <= this.eeprom_data.Length)
        {
          int mappedAddr = this.EepromAddrToMappedAddr((int) eepromAddressInfo.EepromAddr);
          if (eepromAddressInfo.Type == typeof (float))
          {
            float single = BitConverter.ToSingle(this.eeprom_data, mappedAddr);
            serializableData.value = single.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (uint))
          {
            uint uint32 = BitConverter.ToUInt32(this.eeprom_data, mappedAddr);
            serializableData.value = uint32.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (byte))
          {
            byte num = this.eeprom_data[mappedAddr];
            serializableData.value = num.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (ushort))
          {
            ushort uint16 = BitConverter.ToUInt16(this.eeprom_data, mappedAddr);
            serializableData.value = uint16.ToString();
          }
          else
          {
            if (!(eepromAddressInfo.Type == typeof (int)))
              throw new NotImplementedException();
            int int16 = (int) BitConverter.ToInt16(this.eeprom_data, mappedAddr);
            serializableData.value = int16.ToString();
          }
          serializableEepromMapping.serializedData.Add(serializableData);
        }
      }
      XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
      namespaces.Add(string.Empty, string.Empty);
      new XmlSerializer(typeof (SerializableEEPROMMapping)).Serialize(textWriter, (object) serializableEepromMapping, namespaces);
      textWriter.Close();
      return true;
    }

    public static int FloatToBinaryInt(float f)
    {
      return BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
    }

    public static float BinaryIntToFloat(int i)
    {
      return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
    }

    private void SetBytes(int eepromAddr, byte[] bytes)
    {
      this.SetBytes(eepromAddr, bytes, ((IEnumerable<byte>) bytes).Count<byte>());
    }

    private void SetBytes(int eepromAddr, byte[] bytes, int count)
    {
      int mappedAddr = this.EepromAddrToMappedAddr(eepromAddr);
      for (int index = 0; index < count; ++index)
        this.eeprom_data[mappedAddr + index] = bytes[index];
    }

    private void SanitizeLocation(int location)
    {
      if (BitConverter.ToUInt32(this.eeprom_data, location) != uint.MaxValue)
        return;
      this.eeprom_data[location] = (byte) 0;
      this.eeprom_data[location + 1] = (byte) 0;
      this.eeprom_data[location + 2] = (byte) 0;
      this.eeprom_data[location + 3] = (byte) 0;
    }

    private int EepromAddrToMappedAddr(int eepromAddr)
    {
      return eepromAddr * this.eepromProfile.BytesPerEEPROMAddress;
    }
  }
}
