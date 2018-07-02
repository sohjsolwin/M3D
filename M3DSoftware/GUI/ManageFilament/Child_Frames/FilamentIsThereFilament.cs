// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentIsThereFilament
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
  internal class FilamentIsThereFilament : Manage3DInkChildWindow
  {
    private SettingsManager settingsManager;

    public FilamentIsThereFilament(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      switch (button.ID)
      {
        case 5:
          CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted;
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page11_CheatCodePage, CurrentDetails);
          break;
        case 6:
          CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
          if (settingsManager != null)
          {
            settingsManager.DisassociateFilamentFromPrinter(selectedPrinter.Info.serial_number);
            settingsManager.SaveSettings();
          }
          var none = (int) selectedPrinter.SetFilamentToNone(new AsyncCallback(MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(selectedPrinter, Manage3DInkMainWindow.PageID.Page8_WaitingPage, CurrentDetails));
          break;
        case 9:
          MainWindow.ResetToStartup();
          break;
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("3D Ink Details", "Is there 3D Ink in your printer?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }
  }
}
