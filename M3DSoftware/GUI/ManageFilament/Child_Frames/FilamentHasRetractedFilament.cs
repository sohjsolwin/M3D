// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentHasRetractedFilament
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.Spooling.Client;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentHasRetractedFilament : Manage3DInkChildWindow
  {
    private SettingsManager settingsManager;

    public FilamentHasRetractedFilament(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      switch (button.ID)
      {
        case 5:
          this.CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
          this.CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetDetails;
          FilamentWaitingPage.CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
          if (this.settingsManager != null)
          {
            this.settingsManager.DisassociateFilamentFromPrinter(selectedPrinter.Info.serial_number);
            this.settingsManager.SaveSettings();
          }
          int none = (int) selectedPrinter.SetFilamentToNone(new AsyncCallback(this.MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(selectedPrinter, Manage3DInkMainWindow.PageID.Page8_WaitingPage, this.CurrentDetails));
          break;
        case 6:
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page2_RetractingFilament, this.CurrentDetails);
          break;
        case 9:
          this.MainWindow.ResetToStartup();
          break;
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Retracting Current 3D Ink", "Has the current 3D Ink been released?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }
  }
}
