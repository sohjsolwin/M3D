// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.ProPrinterSizeProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Types;
using M3D.Spooling.Common.Utils;

namespace M3D.Spooling.Printer_Profiles
{
  public class ProPrinterSizeProfile : PrinterSizeProfile
  {
    private static readonly StackedBoundingBox region_warning = new StackedBoundingBox(new BoundingBox[2]{ new BoundingBox(4.099998f, 177.9f, 13.2f, 191f, 0.0f, 152.4f), new BoundingBox(16.8f, 165.2f, 25.84f, 159f, 152.4f, 188.5f) });
    private static readonly StackedBoundingBox region_error = ProPrinterSizeProfile.region_warning;
    private const float PHYSICAL_EXTENT_X_MAX = 182f;
    private const float PHYSICAL_EXTENT_Y_MAX = 195f;
    private const float TIER_BASE_X_MAX = 177.8f;
    private const float TIER_BASE_Y_MAX = 177.8f;
    private const float MARGIN_X_DISTANCE_MOVINGLEFT__AS_FLOAT = 2.1f;
    private const float MARGIN_Y_DISTANCE_MOVINGFORWARD__AS_FLOAT = 4f;
    private const float CENTER_XY_DISTANCE = 88.9f;
    private const float SAFE_TO_HOME_WHEN_Z_BELOW = 152.4f;
    private const float PBTolerance = 0.0f;
    private const float ProtoSafetyTolerance = 2f;

    public ProPrinterSizeProfile() : base(
      case_type: PrinterSizeProfile.CaseType.ProCase,
      shell_size: new Vector3D(266.85f, 266.7f, 266.7f),
      printBedSize: new Vector2D(188.2f, 188.2f),
      fluff_height: 44.0f,
      WarningRegion: ProPrinterSizeProfile.region_warning,
      PrintableRegion: ProPrinterSizeProfile.region_warning,
      UnhomedSafeZRange: new Range(1f, 152.4f),
      HomeLocation: new Vector3D(90.99999f, 102.1f, float.NaN),
      BackCornerPosition: new Vector2D(182f, 195f),
      BackCornerPositionBoxTop: new Vector2D(166f, 165f),
      BoxTopLimitZ: 152.399993896484f,
      ABSWarningDim: 65f,
      ZAfterProbing: 0.0f,
      ZAfterG33: 0.100000001490116f,
      G32ProbePoints: new SerializableDictionary<int, RectCoordinates>(){
        {0, new RectCoordinates(23.2f, 181f, 12.1f, 169.9f)}
      }
    )
    {}
  }
}
