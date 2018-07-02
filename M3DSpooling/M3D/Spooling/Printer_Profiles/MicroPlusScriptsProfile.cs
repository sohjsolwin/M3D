using M3D.Spooling.Common;
using M3D.Spooling.Core.Controllers;

namespace M3D.Spooling.Printer_Profiles
{
  internal class MicroPlusScriptsProfile : ScriptsProfile
  {
    public override void StartupScript(IPublicFirmwareController connection, PrinterInfo info)
    {
      var num1 = (int) connection.WriteManualCommands("G91");
      connection.RequestEEPROMMapping();
      var num2 = (int) connection.WriteManualCommands("M576", "M578", "M573", "M572", "M581", "M117", "M114", "M404");
      if (!(info.serial_number.Color.ToLower() == "sl"))
      {
        return;
      }

      var num3 = (int) connection.WriteManualCommands("M420 T25");
    }
  }
}
