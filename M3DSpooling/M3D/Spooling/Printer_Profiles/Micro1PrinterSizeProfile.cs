using M3D.Spooling.Common;
using M3D.Spooling.Common.Types;
using M3D.Spooling.Common.Utils;

namespace M3D.Spooling.Printer_Profiles
{
  public class Micro1PrinterSizeProfile : PrinterSizeProfile
  {
    private static readonly StackedBoundingBox region_warning = new StackedBoundingBox(new BoundingBox[3] { new BoundingBox(1f, 106f, 0.0f, 104f, 0.0f, 5f), new BoundingBox(2.8f, 106f, -6.6f, 104f, 5f, 72.5f), new BoundingBox(8f, 94f, 20.05f, 82.5f, 72.5f, 110f) });
    private const float TIER_BASE_X_MAX = 109f;
    private const float TIER_BASE_Y_MAX = 113.5f;
    private const float FIRST_LAYER_Y_EXCLUSION = 8.5f;
    private const float CENTER_XY_DISTANCE = 55f;
    private const float PBTolerance = 0.0f;

    public Micro1PrinterSizeProfile() : base(
      case_type: PrinterSizeProfile.CaseType.Micro1Case,
      shell_size: new Vector3D(185f, 185f, 185f),
      printBedSize: new Vector2D(109f, 105f),
      fluff_height: 31.2999992370605f,
      WarningRegion: Micro1PrinterSizeProfile.region_warning,
      PrintableRegion: Micro1PrinterSizeProfile.region_warning,
      UnhomedSafeZRange: new Range(1f, 72.5f),
      HomeLocation: new Vector3D(54f, 50f, float.NaN),
      BackCornerPosition: new Vector2D(95f, 95f),
      BackCornerPositionBoxTop: new Vector2D(90f, 84f), 
      BoxTopLimitZ: 67.0f,
      ABSWarningDim: 65f,
      ZAfterProbing: 0.0f,
      ZAfterG33: 0.100000001490116f,
      G32ProbePoints: new SerializableDictionary<int, RectCoordinates>(){
        { 0, new RectCoordinates(2f, 105f, 0.0f, 109f) },
        { 1, new RectCoordinates(5f, 95f, 9f, 99f)},
        { (int) byte.MaxValue, new RectCoordinates(9.5f, 99f, 1f, 102.9f) }
      }
    )
    { }
  }
}
