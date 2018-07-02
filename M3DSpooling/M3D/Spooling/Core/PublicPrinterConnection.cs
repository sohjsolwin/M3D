using M3D.Spooling.Common;

namespace M3D.Spooling.Core
{
  public interface PublicPrinterConnection
  {
    CommandResult SaveEEPROMDataToXMLFile(string filename);
  }
}
