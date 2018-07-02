// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.Micro1ScriptsProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Core.Controllers;

namespace M3D.Spooling.Printer_Profiles
{
  internal class Micro1ScriptsProfile : ScriptsProfile
  {
    public override void StartupScript(IPublicFirmwareController connection, PrinterInfo info)
    {
      var num1 = (int) connection.WriteManualCommands("G91");
      connection.RequestEEPROMMapping();
      var num2 = (int) connection.WriteManualCommands("M576", "M578", "M573", "M572", "M581", "M117", "M114", "M404");
      if (!(info.serial_number.Color.ToLower() == "cl"))
      {
        return;
      }

      var num3 = (int) connection.WriteManualCommands("M420 T25");
    }
  }
}
