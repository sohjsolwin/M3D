using System.Collections.Generic;

namespace M3D.Spooling.ConnectionManager
{
  public abstract class IUSBPrinterFinder
  {
    public abstract List<string> UsbAttached(uint VID, uint PID);
  }
}
