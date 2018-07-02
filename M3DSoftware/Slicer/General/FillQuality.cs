namespace M3D.Slicer.General
{
  public enum FillQuality
  {
    Custom = -1,
    HollowThinWalls = 0,
    HollowThickWalls = 1,
    Solid = 350, // 0x0000015E
    ExtraHigh = 1500, // 0x000005DC
    High = 2500, // 0x000009C4
    Medium = 4000, // 0x00000FA0
    Low = 5500, // 0x0000157C
  }
}
