// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.ConnectionManager.LinuxUSBPrinterFinder
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.Spooling.ConnectionManager
{
  public class LinuxUSBPrinterFinder : IUSBPrinterFinder
  {
    private List<string> com_ports = new List<string>();
    private string idVendor;
    private string idProduct;

    public override List<string> UsbAttached(uint VID, uint PID)
    {
      this.idVendor = VID.ToString("X4");
      this.idProduct = PID.ToString("X4");
      lock (this.com_ports)
        this.com_ports = this.find_all_tty_usb();
      return new List<string>((IEnumerable<string>) this.com_ports);
    }

    private List<string> find_all_tty_usb()
    {
      List<string> stringList = new List<string>();
      foreach (string directory1 in Directory.GetDirectories("/sys/bus/usb/devices"))
      {
        string str1 = Path.Combine("/sys/bus/usb/devices", directory1);
        if (File.Exists(Path.Combine(str1, "idVendor")) && File.Exists(Path.Combine(str1, "idProduct")) && (string.Concat(File.ReadAllLines(Path.Combine(str1, "idVendor"))).Trim().Equals(this.idVendor, StringComparison.OrdinalIgnoreCase) && string.Concat(File.ReadAllLines(Path.Combine(str1, "idProduct"))).Trim().Equals(this.idProduct, StringComparison.OrdinalIgnoreCase)))
        {
          foreach (string directory2 in Directory.GetDirectories(str1))
          {
            string str2 = directory1 + Path.DirectorySeparatorChar.ToString() + Path.GetFileName(directory1) + ":";
            if (directory2.StartsWith(str2))
            {
              foreach (string directory3 in Directory.GetDirectories(Path.Combine(str1, directory2)))
              {
                if (directory3.Contains("tty"))
                {
                  foreach (string directory4 in Directory.GetDirectories(directory3))
                  {
                    string fileName = Path.GetFileName(directory4);
                    stringList.Add(Path.Combine("/dev", fileName));
                  }
                }
              }
            }
          }
        }
      }
      return stringList;
    }
  }
}
