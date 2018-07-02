using M3D.Spooling.Common;
using M3D.Spooling.Core.Controllers;

namespace M3D.Spooling.Printer_Profiles
{
  internal class ProScriptsProfile : Micro1ScriptsProfile
  {
    public override void StartupScript(IPublicFirmwareController connection, PrinterInfo info)
    {
      var num1 = (int) connection.WriteManualCommands("G91");
      connection.RequestEEPROMMapping();
      var num2 = (int) connection.WriteManualCommands("M576", "M578", "M572", "M581", "M117", "M114", "M404");
    }
  }
}
