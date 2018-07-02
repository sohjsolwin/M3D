using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Views;
using M3D.Spooling.Common;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentIsNewSpoolPage : Manage3DInkChildWindow
  {
    private MessagePopUp infobox;
    private SettingsManager settingsManager;

    public FilamentIsNewSpoolPage(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, MessagePopUp infobox)
      : base(ID, host, mainWindow)
    {
      this.infobox = infobox;
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (MainWindow.GetSelectedPrinter() == null)
      {
        return;
      }

      if (button.ID == 9)
      {
        MainWindow.ResetToStartup();
      }
      else
      {
        CurrentDetails.current_spool.estimated_filament_length_printed = 0.0f;
        CurrentDetails.current_spool.filament_uid = FilamentSpool.GenerateUID();
        if (button.ID == 6)
        {
          FilamentSpool filamentSpool = SearchForFilamentDuplicate();
          if (filamentSpool != (FilamentSpool) null)
          {
            if ((double) filamentSpool.estimated_filament_length_printed > (double)CurrentDetails.current_spool.estimated_filament_length_printed)
            {
              CurrentDetails.current_spool.estimated_filament_length_printed = filamentSpool.estimated_filament_length_printed;
            }

            CurrentDetails.current_spool.filament_uid = filamentSpool.filament_uid;
            infobox.AddMessageToQueue("Using saved 3D Ink history.");
          }
          else
          {
            infobox.AddMessageToQueue("Matching 3D Ink history not found. Treating as a new spool.");
          }
        }
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page13_FilamentLocation, CurrentDetails);
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("3D Ink Details", "Is this a new filament spool?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }

    private FilamentSpool SearchForFilamentDuplicate()
    {
      return settingsManager.FindMatchingUsedSpool(CurrentDetails.current_spool);
    }
  }
}
