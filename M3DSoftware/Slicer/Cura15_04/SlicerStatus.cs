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
