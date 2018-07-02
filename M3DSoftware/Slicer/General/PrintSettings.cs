using M3D.Spooling.Common;

namespace M3D.Slicer.General
{
  public class PrintSettings
  {
    public Matrix4x4 transformation;
    public FilamentSpool filament_info;

    public PrintSettings()
    {
      transformation = new Matrix4x4();
      SetPrintDefaults();
    }

    public void SetPrintDefaults()
    {
      transformation.Identity();
      filament_info = new FilamentSpool();
    }
  }
}
