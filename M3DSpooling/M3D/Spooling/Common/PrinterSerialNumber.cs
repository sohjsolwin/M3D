// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Common.PrinterSerialNumber
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Xml.Serialization;

namespace M3D.Spooling.Common
{
  public class PrinterSerialNumber
  {
    public static PrinterSerialNumber Undefined = new PrinterSerialNumber("00-00-00-00-00-000-000");
    [XmlIgnore]
    private string serial;

    public PrinterSerialNumber()
      : this("0")
    {
    }

    public PrinterSerialNumber(string serialnumber)
    {
      if (string.IsNullOrEmpty(serialnumber))
      {
        throw new ArgumentNullException();
      }

      var num1 = 0;
      foreach (var ch in serialnumber)
      {
        if (ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z' || ch >= '0' && ch <= '9')
        {
          serial += ch.ToString();
          ++num1;
          if (num1 == 2 || num1 == 4 || (num1 == 6 || num1 == 8) || (num1 == 10 || num1 == 13))
          {
            serial += "-";
          }
        }
        else if (ch != '-')
        {
          throw new FormatException();
        }

        if (num1 == 16)
        {
          return;
        }
      }
      if (num1 < 7)
      {
        serial = new PrinterSerialNumber(serialnumber + "0000000000000000").ToString();
      }
      else
      {
        if (num1 >= 16)
        {
          return;
        }

        var num2 = 16 - num1;
        var str = "";
        for (var index = 0; index < num2; ++index)
        {
          str += "0";
        }

        serial = serial.Substring(0, 10) + str + serial.Substring(10);
      }
    }

    public override string ToString()
    {
      return serial;
    }

    public override bool Equals(object obj)
    {
      if (obj == null)
      {
        return false;
      }

      var printerSerialNumber = obj as PrinterSerialNumber;
      if ((object) printerSerialNumber == null)
      {
        return false;
      }

      return serial == printerSerialNumber.serial;
    }

    public override int GetHashCode()
    {
      return serial.GetHashCode();
    }

    [XmlIgnore]
    public string Color
    {
      get
      {
        return serial.Substring(0, 2);
      }
    }

    [XmlIgnore]
    public string Date
    {
      get
      {
        return serial.Substring(3, 8);
      }
    }

    [XmlIgnore]
    public string Batch
    {
      get
      {
        return serial.Substring(12, 2);
      }
    }

    [XmlIgnore]
    public string TotalNumber
    {
      get
      {
        return serial.Substring(15, 3);
      }
    }

    [XmlIgnore]
    public string Number
    {
      get
      {
        return serial.Substring(19);
      }
    }

    public static bool operator ==(PrinterSerialNumber a, PrinterSerialNumber b)
    {
      if ((object) a == (object) b)
      {
        return true;
      }

      if ((object) a == null || (object) b == null)
      {
        return false;
      }

      return a.serial == b.serial;
    }

    public static bool operator !=(PrinterSerialNumber a, PrinterSerialNumber b)
    {
      return !(a == b);
    }

    [XmlAttribute("internal")]
    public string InternalDataXMLOnly
    {
      get
      {
        return serial;
      }
      set
      {
        serial = value;
      }
    }
  }
}
