// Decompiled with JetBrains decompiler
// Type: M3D.Spooling.Printer_Profiles.PrinterSizeProfile
// Assembly: M3DSpooling, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D19DB185-E399-4809-A97E-0B15EB645090
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSpooling.dll

using M3D.Spooling.Common;
using M3D.Spooling.Common.Types;
using M3D.Spooling.Common.Utils;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Spooling.Printer_Profiles
{
  public class PrinterSizeProfile
  {
    [XmlAttribute]
    public PrinterSizeProfile.CaseType case_type;
    [XmlElement]
    public Vector3D shell_size;
    [XmlElement]
    public Vector2D printBedSize;
    [XmlAttribute]
    public float fluff_height;
    [XmlElement]
    public StackedBoundingBox WarningRegion;
    [XmlElement]
    public StackedBoundingBox PrintableRegion;
    [XmlElement]
    public Range UnhomedSafeZRange;
    [XmlElement]
    public Vector3D HomeLocation;
    [XmlElement]
    public Vector2D BackCornerPosition;
    [XmlElement]
    public Vector2D BackCornerPositionBoxTop;
    [XmlAttribute]
    public float BoxTopLimitZ;
    [XmlAttribute]
    public float ABSWarningDim;
    [XmlAttribute]
    public float ZAfterProbing;
    [XmlAttribute]
    public float ZAfterG33;
    [XmlElement]
    public SerializableDictionary<int, RectCoordinates> G32ProbePoints;

    public PrinterSizeProfile()
    {
      this.PrintableRegion = new StackedBoundingBox();
      this.WarningRegion = new StackedBoundingBox();
      this.G32ProbePoints = new SerializableDictionary<int, RectCoordinates>();
    }

    public PrinterSizeProfile(PrinterSizeProfile other)
    {
      this.case_type = other.case_type;
      this.shell_size = other.shell_size;
      this.printBedSize = other.printBedSize;
      this.fluff_height = other.fluff_height;
      this.PrintableRegion = new StackedBoundingBox(other.PrintableRegion);
      this.WarningRegion = new StackedBoundingBox(other.WarningRegion);
      this.UnhomedSafeZRange = other.UnhomedSafeZRange;
      this.HomeLocation = other.HomeLocation;
      this.ABSWarningDim = other.ABSWarningDim;
      this.ZAfterProbing = other.ZAfterProbing;
      this.ZAfterG33 = other.ZAfterG33;
      this.BackCornerPosition = other.BackCornerPosition;
      this.BackCornerPositionBoxTop = other.BackCornerPositionBoxTop;
      this.BoxTopLimitZ = other.BoxTopLimitZ;
      this.G32ProbePoints = new SerializableDictionary<int, RectCoordinates>((Dictionary<int, RectCoordinates>) other.G32ProbePoints);
    }

    public PrinterSizeProfile(PrinterSizeProfile.CaseType case_type, Vector3D shell_size, Vector2D printBedSize, float fluff_height, StackedBoundingBox WarningRegion, StackedBoundingBox PrintableRegion, Range UnhomedSafeZRange, Vector3D HomeLocation, Vector2D BackCornerPosition, Vector2D BackCornerPositionBoxTop, float BoxTopLimitZ, float ABSWarningDim, float ZAfterProbing, float ZAfterG33, SerializableDictionary<int, RectCoordinates> G32ProbePoints)
    {
      this.case_type = case_type;
      this.shell_size = shell_size;
      this.printBedSize = printBedSize;
      this.fluff_height = fluff_height;
      this.WarningRegion = WarningRegion;
      this.PrintableRegion = PrintableRegion;
      this.UnhomedSafeZRange = UnhomedSafeZRange;
      this.HomeLocation = HomeLocation;
      this.ABSWarningDim = ABSWarningDim;
      this.ZAfterProbing = ZAfterProbing;
      this.ZAfterG33 = ZAfterG33;
      this.BackCornerPosition = BackCornerPosition;
      this.BackCornerPositionBoxTop = BackCornerPositionBoxTop;
      this.BoxTopLimitZ = BoxTopLimitZ;
      this.G32ProbePoints = G32ProbePoints;
    }

    public enum CaseType
    {
      Micro1Case,
      ProCase,
    }
  }
}
