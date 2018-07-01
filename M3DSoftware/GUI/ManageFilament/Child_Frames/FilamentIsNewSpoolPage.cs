// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentIsNewSpoolPage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      if (this.MainWindow.GetSelectedPrinter() == null)
        return;
      if (button.ID == 9)
      {
        this.MainWindow.ResetToStartup();
      }
      else
      {
        this.CurrentDetails.current_spool.estimated_filament_length_printed = 0.0f;
        this.CurrentDetails.current_spool.filament_uid = FilamentSpool.GenerateUID();
        if (button.ID == 6)
        {
          FilamentSpool filamentSpool = this.SearchForFilamentDuplicate();
          if (filamentSpool != (FilamentSpool) null)
          {
            if ((double) filamentSpool.estimated_filament_length_printed > (double) this.CurrentDetails.current_spool.estimated_filament_length_printed)
              this.CurrentDetails.current_spool.estimated_filament_length_printed = filamentSpool.estimated_filament_length_printed;
            this.CurrentDetails.current_spool.filament_uid = filamentSpool.filament_uid;
            this.infobox.AddMessageToQueue("Using saved 3D Ink history.");
          }
          else
            this.infobox.AddMessageToQueue("Matching 3D Ink history not found. Treating as a new spool.");
        }
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page13_FilamentLocation, this.CurrentDetails);
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("3D Ink Details", "Is this a new filament spool?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }

    private FilamentSpool SearchForFilamentDuplicate()
    {
      return this.settingsManager.FindMatchingUsedSpool(this.CurrentDetails.current_spool);
    }
  }
}
