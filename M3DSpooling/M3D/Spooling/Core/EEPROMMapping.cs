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
      eeprom_data = new byte[size];
      this.eepromProfile = eepromProfile;
    }

    public EEPROMMapping(byte[] data, EEPROMProfile eepromProfile)
    {
      eeprom_data = data;
      this.eepromProfile = eepromProfile;
    }

    public void Dispose()
    {
      eeprom_data = (byte[]) null;
      eepromProfile = (EEPROMProfile) null;
    }

    public byte[] GetAllEEPROMData()
    {
      return (byte[])eeprom_data.Clone();
    }

    public bool SetValue(EepromAddressInfo eepromValue, int data)
    {
      if (eepromValue == null)
      {
        return false;
      }

      var eepromAddr = (int) eepromValue.EepromAddr;
      var mappedAddr = EepromAddrToMappedAddr(eepromAddr);
      if (mappedAddr < 0 || mappedAddr > eeprom_data.Length)
      {
        return false;
      }

      byte[] bytes = BitConverter.GetBytes(data);
      SetBytes(eepromAddr, bytes, eepromValue.Size);
      return true;
    }

    public bool HasKey(string name)
    {
      return eepromProfile.GetEepromInfo(name) != null;
    }

    public int GetLocationFromName(string name)
    {
      EepromAddressInfo eepromInfo = eepromProfile.GetEepromInfo(name);
      if (eepromInfo != null)
      {
        return (int) eepromInfo.EepromAddr;
      }

      return -1;
    }

    public bool SetFloat(string name, float value)
    {
      var locationFromName = GetLocationFromName(name);
      if (locationFromName < 0)
      {
        if (Debugger.IsAttached)
        {
          Debugger.Break();
        }

        return false;
      }
      byte[] bytes = BitConverter.GetBytes(value);
      SetBytes(locationFromName, bytes, ((IEnumerable<byte>) bytes).Count<byte>());
      return true;
    }

    public ushort GetAlignedByte(string name)
    {
      EepromAddressInfo eepromInfo = eepromProfile.GetEepromInfo(name);
      if (eepromInfo != null)
      {
        var mappedAddr = EepromAddrToMappedAddr((int) eepromInfo.EepromAddr);
        if (eepromInfo.Type == typeof (byte))
        {
          return (ushort)eeprom_data[mappedAddr];
        }

        if (eepromInfo.Type == typeof (ushort))
        {
          return BitConverter.ToUInt16(eeprom_data, mappedAddr);
        }
      }
      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return 0;
    }

    public uint GetUInt32(string name)
    {
      var mappedAddr = EepromAddrToMappedAddr(GetLocationFromName(name));
      if (mappedAddr >= 0)
      {
        return BitConverter.ToUInt32(eeprom_data, mappedAddr);
      }

      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return 0;
    }

    public byte[] GetBytesFromLocation(string name, int count)
    {
      var mappedAddr = EepromAddrToMappedAddr(GetLocationFromName(name));
      if (mappedAddr < 0)
      {
        if (Debugger.IsAttached)
        {
          Debugger.Break();
        }

        return (byte[]) null;
      }
      byte[] numArray = new byte[count];
      for (var index = 0; index < count; ++index)
      {
        numArray[index] = eeprom_data[mappedAddr + index];
      }

      return numArray;
    }

    public ushort GetUInt16(string name)
    {
      var mappedAddr = EepromAddrToMappedAddr(GetLocationFromName(name));
      if (mappedAddr >= 0)
      {
        return BitConverter.ToUInt16(eeprom_data, mappedAddr);
      }

      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return 0;
    }

    public int GetInt32(string name)
    {
      var mappedAddr = EepromAddrToMappedAddr(GetLocationFromName(name));
      if (mappedAddr >= 0)
      {
        return BitConverter.ToInt32(eeprom_data, mappedAddr);
      }

      if (Debugger.IsAttached)
      {
        Debugger.Break();
      }

      return 0;
    }

    public float GetFloat(string name)
    {
      var mappedAddr = EepromAddrToMappedAddr(GetLocationFromName(name));
      if (mappedAddr < 0)
      {
        return 0.0f;
      }

      SanitizeLocation(mappedAddr);
      return BitConverter.ToSingle(eeprom_data, mappedAddr);
    }

    public byte[] AlignedByteToBytaArray(ushort alignedbyte)
    {
      var numArray = (byte[]) null;
      if (eepromProfile.BytesPerEEPROMAddress == 1)
      {
        numArray = new byte[1]{ (byte) alignedbyte };
      }
      else if (eepromProfile.BytesPerEEPROMAddress == 2)
      {
        numArray = BitConverter.GetBytes(alignedbyte);
      }
      else if (eepromProfile.BytesPerEEPROMAddress == 4)
      {
        numArray = BitConverter.GetBytes((uint) alignedbyte);
      }

      return numArray;
    }

    public int Size
    {
      get
      {
        return eeprom_data.Length;
      }
    }

    public byte[] EEPROMData
    {
      get
      {
        return eeprom_data;
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
      {
        return false;
      }

      SortedList<int, EepromAddressInfo> allData = eepromProfile.GetAllData();
      var serializableEepromMapping = new SerializableEEPROMMapping(eepromProfile.ProfileName);
      foreach (KeyValuePair<int, EepromAddressInfo> keyValuePair in allData)
      {
        EepromAddressInfo eepromAddressInfo = keyValuePair.Value;
        SerializableEEPROMMapping.SerializableData serializableData;
        serializableData.key = eepromAddressInfo.Name;
        serializableData.value = "";
        if ((int) eepromAddressInfo.EepromAddr <= eeprom_data.Length)
        {
          var mappedAddr = EepromAddrToMappedAddr((int) eepromAddressInfo.EepromAddr);
          if (eepromAddressInfo.Type == typeof (float))
          {
            var single = BitConverter.ToSingle(eeprom_data, mappedAddr);
            serializableData.value = single.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (uint))
          {
            var uint32 = BitConverter.ToUInt32(eeprom_data, mappedAddr);
            serializableData.value = uint32.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (byte))
          {
            var num = eeprom_data[mappedAddr];
            serializableData.value = num.ToString();
          }
          else if (eepromAddressInfo.Type == typeof (ushort))
          {
            var uint16 = BitConverter.ToUInt16(eeprom_data, mappedAddr);
            serializableData.value = uint16.ToString();
          }
          else
          {
            if (!(eepromAddressInfo.Type == typeof (int)))
            {
              throw new NotImplementedException();
            }

            var int16 = (int) BitConverter.ToInt16(eeprom_data, mappedAddr);
            serializableData.value = int16.ToString();
          }
          serializableEepromMapping.serializedData.Add(serializableData);
        }
      }
      var namespaces = new XmlSerializerNamespaces();
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
      SetBytes(eepromAddr, bytes, ((IEnumerable<byte>) bytes).Count<byte>());
    }

    private void SetBytes(int eepromAddr, byte[] bytes, int count)
    {
      var mappedAddr = EepromAddrToMappedAddr(eepromAddr);
      for (var index = 0; index < count; ++index)
      {
        eeprom_data[mappedAddr + index] = bytes[index];
      }
    }

    private void SanitizeLocation(int location)
    {
      if (BitConverter.ToUInt32(eeprom_data, location) != uint.MaxValue)
      {
        return;
      }

      eeprom_data[location] = (byte) 0;
      eeprom_data[location + 1] = (byte) 0;
      eeprom_data[location + 2] = (byte) 0;
      eeprom_data[location + 3] = (byte) 0;
    }

    private int EepromAddrToMappedAddr(int eepromAddr)
    {
      return eepromAddr * eepromProfile.BytesPerEEPROMAddress;
    }
  }
}
