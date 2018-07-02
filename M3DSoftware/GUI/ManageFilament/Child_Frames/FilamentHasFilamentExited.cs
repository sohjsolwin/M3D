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
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      switch (button.ID)
      {
        case 5:
          CurrentDetails.waitCondition = new Mangage3DInkStageDetails.WaitCondition(WaitCondition);
          FilamentWaitingPage.CurrentWaitingText = "Please wait. Some 3D Ink is being pulled back so it won't drip.";
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, CurrentDetails);
          AsyncCallback callback = CurrentDetails.current_spool.filament_location != FilamentSpool.Location.Internal ? new AsyncCallback(CleanNozzleAfterRetraction) : new AsyncCallback(CloseBedAfterRetraction);
          if (settingsManager != null)
          {
            settingsManager.AssociateFilamentToPrinter(selectedPrinter.Info.serial_number, CurrentDetails.current_spool);
            settingsManager.SaveSettings();
          }
          var num = (int) selectedPrinter.SetFilamentInfo(callback, (object) selectedPrinter, CurrentDetails.current_spool);
          break;
        case 6:
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page4_InsertNewFilament, CurrentDetails);
          break;
        case 9:
          MainWindow.ResetToStartup();
          break;
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Load New 3D Ink", "Has the new 3D Ink exited the nozzle?", true, true, false, false, false, false);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      finishedWaiting.Value = false;
    }

    private void GotoCleanNozzlePageAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (ar.CallResult != CommandResult.Success)
      {
        MainWindow.ResetToStartup();
      }
      else if (settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert)
      {
        MainWindow.TurnOnHeater(new AsyncCallback(MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(asyncState, Manage3DInkMainWindow.PageID.Page17_CleanNozzle, CurrentDetails), 150, CurrentDetails.current_spool.filament_type);
      }
      else
      {
        MainWindow.ResetToStartup();
      }
    }

    private void CleanNozzleAfterRetraction(IAsyncCallResult ar)
    {
      var asyncState = (PrinterObject) ar.AsyncState;
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          var num = (int) asyncState.SendManualGCode(new AsyncCallback(GotoCleanNozzlePageAfterCommand), (object) asyncState, "G91", PrinterCompatibleString.Format("G0 E-{0}", (object) asyncState.MyFilamentProfile.preprocessor.initialPrint.PrimeAmount), "G90");
          return;
        case CommandResult.Failed_PrinterAlreadyLocked:
          messagebox.AddMessageToQueue("Unable to connect to the printer because it is already in use.");
          break;
        default:
          messagebox.AddMessageToQueue("There was an error connecting to the printer. Please try again.");
          break;
      }
      MainWindow.ResetToStartup();
    }

    private void CloseBedAfterRetraction(IAsyncCallResult ar)
    {
      var asyncState = (PrinterObject) ar.AsyncState;
      switch (ar.CallResult)
      {
        case CommandResult.Success:
        case CommandResult.Success_LockReleased:
        case CommandResult.Success_LockAcquired:
          var num = (int) asyncState.SendManualGCode(new AsyncCallback(MainWindow.GotoPageAfterOperation), (object) new Manage3DInkMainWindow.PageAfterLockDetails(asyncState, Manage3DInkMainWindow.PageID.Page15_CloseBedInstructions, CurrentDetails), "G91", PrinterCompatibleString.Format("G0 E-{0}", (object) asyncState.MyFilamentProfile.preprocessor.initialPrint.PrimeAmount), "G90");
          return;
        case CommandResult.Failed_PrinterAlreadyLocked:
          messagebox.AddMessageToQueue("Unable to connect to the printer because it is already in use.");
          break;
        default:
          messagebox.AddMessageToQueue("There was an error connecting to the printer. Please try again.");
          break;
      }
      MainWindow.ResetToStartup();
    }

    private bool WaitCondition()
    {
      return finishedWaiting.Value;
    }
  }
}
