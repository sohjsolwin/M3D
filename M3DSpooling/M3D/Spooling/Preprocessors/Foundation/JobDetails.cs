using M3D.Spooling.Common;

namespace M3D.Spooling.Preprocessors.Foundation
{
  public class JobDetails
  {
    public BoundingBox bounds;
    public int ideal_temperature;
    public JobParams jobParams;

    public FilamentSpool.TypeEnum FilamentType
    {
      get
      {
        return jobParams.filament_type;
      }
    }
  }
}
