using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Core.Controllers
{
  internal interface IPublicFirmwareController
  {
    ScriptCallback OnGotUpdatedPosition { get; set; }

    CommandResult WriteManualCommands(params string[] commands);

    void AddManualCommandToFront(params string[] commands);

    void RequestEEPROMMapping();

    InternalPrinterProfile MyPrinterProfile { get; }

    EEPROMMapping EEPROM { get; }

    PrinterInfo CurrentPrinterInfo { get; }

    void ProcessEEPROMData();

    void SaveJobParamsToPersistantData(PersistantJobData parameters);

    void SendEmergencyStop();
  }
}
