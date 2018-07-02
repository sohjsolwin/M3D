using M3D.Spooling.Printer_Profiles;

namespace M3D.Spooling.Common
{
  public class SupportedFeatures
  {
    public uint FeaturesBitField;
    public bool UsesSupportedFeatures;

    public SupportedFeatures()
    {
    }

    public SupportedFeatures(SupportedFeatures other)
    {
      FeaturesBitField = other.FeaturesBitField;
      UsesSupportedFeatures = other.UsesSupportedFeatures;
    }

    public SupportedFeatures.Status GetStatus(string key, SupportedFeaturesProfile profileConstants)
    {
      var featureSlot = profileConstants.GetFeatureSlot(key);
      if (featureSlot >= 0)
      {
        return GetStatus(featureSlot);
      }

      return SupportedFeatures.Status.Unavailable;
    }

    public bool Available(string key, SupportedFeaturesProfile profileConstants)
    {
      var featureSlot = profileConstants.GetFeatureSlot(key);
      if (featureSlot >= 0)
      {
        return (uint)GetStatus(featureSlot) > 0U;
      }

      return false;
    }

    public SupportedFeatures.Status GetStatus(int feature_slot)
    {
      if (feature_slot < 0)
      {
        return SupportedFeatures.Status.Unavailable;
      }

      var num = (ushort) (feature_slot * 2);
      return (SupportedFeatures.Status) ((FeaturesBitField & 3U << (int) num) >> (int) num);
    }

    public enum Status
    {
      Unavailable,
      DevelopmentalFeature,
      Available,
    }
  }
}
