using M3D.Spooling.Common;

namespace M3D.Spooler
{
  public struct PrintOptions
  {
    public FilamentSpool.TypeEnum type;
    public int temperature;
    public bool use_preprocessors;
    public bool calibrateZ;
    public JobParams.Mode jobMode;
  }
}
