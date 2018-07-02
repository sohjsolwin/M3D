using M3D.Spooling.Common;
using System.Collections.Generic;

namespace M3D.GUI.ManageFilament
{
  public class Mangage3DInkStageDetails
  {
    public Manage3DInkMainWindow.Mode mode;
    public Manage3DInkMainWindow.PageID pageAfterWait;
    public FilamentSpool current_spool;
    public Mangage3DInkStageDetails.WaitCondition waitCondition;
    public List<Filament> user_filaments;

    public Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode mode)
      : this(mode, (FilamentSpool) null)
    {
    }

    public Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode mode, FilamentSpool current_spool)
      : this(mode, Manage3DInkMainWindow.PageID.Page0_StartupPage)
    {
      this.mode = mode;
      this.current_spool = current_spool;
      pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
    }

    public Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode mode, Manage3DInkMainWindow.PageID pageAfterWait)
    {
      this.mode = mode;
      this.pageAfterWait = pageAfterWait;
    }

    public delegate bool WaitCondition();
  }
}
