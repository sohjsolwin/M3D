// Decompiled with JetBrains decompiler
// Type: M3D.Slicer.Cura15_04.SlicerStatus
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

namespace M3D.Slicer.Cura15_04
{
  public struct SlicerStatus
  {
    public bool is_slicing;
    public bool is_done;
    public float estimatedpercentcomplete;
    public SlicerStatus.SlicerResultsInfo results;

    public struct SlicerResultsInfo
    {
      public int print_time_s;
      public int filament;
    }
  }
}
