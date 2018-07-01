// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Core.Controllers.IPublicFirmwareController
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

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
