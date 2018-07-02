using M3D.Spooling.Common;
using RepetierHost.model;

namespace M3D.Spooling.Core.Controllers
{
  internal interface FirmwareControllerPlugin
  {
    string ID { get; }

    void ProcessGCodeResult(GCode gcode, string resultFromPrinter, PrinterInfo printerInfo);

    void RegisterGCodes(IGCodePluginable controller);
  }
}
