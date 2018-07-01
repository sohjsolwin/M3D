// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentHasFilamentExited
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentHasFilamentExited : Manage3DInkChildWindow
  {
    private ThreadSafeVariable<bool> finishedWaiting = new ThreadSafeVariable<bool>(false);
    private PopupMessageBox messagebox;
    private SettingsManager settingsManager;

    public FilamentHasFilamentExited(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.messagebox = messagebox;
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
          this.CurrentDetails.waitCondition = new Mangage3DInkStageDetails.WaitCondition(this.WaitCondition);
          FilamentWaitingPage.CurrentWaitingText = "Please wait. Some 3D Ink is being pulled back so it won't drip.";
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, this.CurrentDetails);
          AsyncCallback callback = this.CurrentDetails.current_spool.filament_location != FilamentSpool.Location.Internal ? new AsyncCallback(this.CleanNozzleAfterRetraction) : new AsyncCallback(this.CloseBedAfterRetraction);
          if (this.settingsManager != null)
          {
            this.settingsManager.AssociateFilamentToPrinter(selectedPrinter.Info.serial_number, this.CurrentDetails.current_spool);
            this.settingsManager.SaveSettings();
          }
          int num = (int) selectedPrinter.SetFilamentInfo(callback, (object) selectedPrinter, this.CurrentDetails.current_spool);
          break;
        case 6:
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page4_InsertNewFilament, this.CurrentDetails);
          break;
        case 9:
          this.MainWindow.ResetToStartup();
          break;
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Load New 3D Ink", "Has the new 3D Ink exited the nozzle?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.finishedWaiting.Value = false;
    }

    private void GotoCleanNozzlePageAfterCommand(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
        this.MainWindow.ResetToStartup();
      else if (this.settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert)
        this.MainWindow.TurnOnHeater(new AsyncCallback(this.MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(asyncState, Manage3DInkMainWindow.PageID.Page17_CleanNozzle, this.CurrentDetails), 150, this.CurrentDetails.current_spool.filament_type);
      else
        this.MainWindow.ResetToStartup();
    }

    private void CleanNozzleAfterRetraction(IAsyncCallResult ar)
    {
      PrinterObject asyncState = (PrinterObject) ar.AsyncState;
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          int num = (int) asyncState.SendManualGCode(new AsyncCallback(this.GotoCleanNozzlePageAfterCommand), (object) asyncState, "G91", PrinterCompatibleString.Format("G0 E-{0}", (object) asyncState.MyFilamentProfile.preprocessor.initialPrint.PrimeAmount), "G90");
          return;
        case CommandResult.Failed_PrinterAlreadyLocked:
          this.messagebox.AddMessageToQueue("Unable to connect to the printer because it is already in use.");
          break;
        default:
          this.messagebox.AddMessageToQueue("There was an error connecting to the printer. Please try again.");
          break;
      }
      this.MainWindow.ResetToStartup();
    }

    private void CloseBedAfterRetraction(IAsyncCallResult ar)
    {
      PrinterObject asyncState = (PrinterObject) ar.AsyncState;
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          int num = (int) asyncState.SendManualGCode(new AsyncCallback(this.MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(asyncState, Manage3DInkMainWindow.PageID.Page15_CloseBedInstructions, this.CurrentDetails), "G91", PrinterCompatibleString.Format("G0 E-{0}", (object) asyncState.MyFilamentProfile.preprocessor.initialPrint.PrimeAmount), "G90");
          return;
        case CommandResult.Failed_PrinterAlreadyLocked:
          this.messagebox.AddMessageToQueue("Unable to connect to the printer because it is already in use.");
          break;
        default:
          this.messagebox.AddMessageToQueue("There was an error connecting to the printer. Please try again.");
          break;
      }
      this.MainWindow.ResetToStartup();
    }

    private bool WaitCondition()
    {
      return this.finishedWaiting.Value;
    }
  }
}
