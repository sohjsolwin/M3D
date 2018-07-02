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
      idVendor = VID.ToString("X4");
      idProduct = PID.ToString("X4");
      lock (com_ports)
      {
        com_ports = find_all_tty_usb();
      }

      return new List<string>((IEnumerable<string>)com_ports);
    }

    private List<string> find_all_tty_usb()
    {
      var stringList = new List<string>();
      foreach (var directory1 in Directory.GetDirectories("/sys/bus/usb/devices"))
      {
        var str1 = Path.Combine("/sys/bus/usb/devices", directory1);
        if (File.Exists(Path.Combine(str1, "idVendor")) && File.Exists(Path.Combine(str1, "idProduct")) && (string.Concat(File.ReadAllLines(Path.Combine(str1, "idVendor"))).Trim().Equals(idVendor, StringComparison.OrdinalIgnoreCase) && string.Concat(File.ReadAllLines(Path.Combine(str1, "idProduct"))).Trim().Equals(idProduct, StringComparison.OrdinalIgnoreCase)))
        {
          foreach (var directory2 in Directory.GetDirectories(str1))
          {
            var str2 = directory1 + Path.DirectorySeparatorChar.ToString() + Path.GetFileName(directory1) + ":";
            if (directory2.StartsWith(str2))
            {
              foreach (var directory3 in Directory.GetDirectories(Path.Combine(str1, directory2)))
              {
                if (directory3.Contains("tty"))
                {
                  foreach (var directory4 in Directory.GetDirectories(directory3))
                  {
                    var fileName = Path.GetFileName(directory4);
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
