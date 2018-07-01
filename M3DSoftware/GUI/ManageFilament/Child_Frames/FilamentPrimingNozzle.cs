// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentPrimingNozzle
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentPrimingNozzle : Manage3DInkChildWindow
  {
    private TextWidget text_main;

    public FilamentPrimingNozzle(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Remove 3D Ink", "Please wait. \n\nRemove filament is extruding a small amount of filament first to prevent clogs.", false, false, false, false, false, false);
      this.text_main = (TextWidget) this.FindChildElement(3);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.text_main.Text = "";
      this.text_main.Text = this.CurrentDetails.current_spool.filament_type != FilamentSpool.TypeEnum.CAM ? "Please wait. \n\nRemove filament is extruding a small amount of filament first to prevent clogs." : "Please wait. \n\nRemove filament is extruding a small amount of filament first to prevent clogs.\n\nWarning: Chameleon Ink may appear white when heated and exiting nozzle.";
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      if (this.CurrentDetails.current_spool == (FilamentSpool) null)
      {
        this.MainWindow.ResetToStartup();
      }
      else
      {
        int num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(this.MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(selectedPrinter, Manage3DInkMainWindow.PageID.Page2_RetractingFilament, this.CurrentDetails), "G4 S5", "G91", PrinterCompatibleString.Format("G0 E{0} F{1}", (object) 50f, (object) 90.0));
      }
    }
  }
}
