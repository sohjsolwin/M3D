using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentInsertNewFilament : Manage3DInkChildWindow
  {
    private ProgressBarWidget progressBar;
    private TextWidget insert_filament_text;

    public FilamentInsertNewFilament(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 9)
      {
        return;
      }

      MainWindow.ResetToStartup();
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Load New 3D Ink", "Insert your new 3D Ink into the external feed port.\n\nWait a few moments until the new 3D Ink begins\n exiting the nozzle.", true, false, true, false, false, false);
      insert_filament_text = (TextWidget)FindChildElement(3);
      progressBar = (ProgressBarWidget)FindChildElement(4);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      insert_filament_text.Text = CurrentDetails.current_spool.filament_location != FilamentSpool.Location.Internal ? "Insert your new 3D Ink into the external feed port.\n\nWait a few moments until the new 3D Ink begins\n exiting the nozzle." : "The printer is now loading 3D Ink through it's internal port.\nYou may need to continue to push the 3D Ink into the 3D Ink tube to make sure it is in all of the way.\n\nWait a few moments until the new 3D Ink begins exiting the nozzle.";
      if (CurrentDetails.current_spool.filament_type == FilamentSpool.TypeEnum.CAM)
      {
        insert_filament_text.Text = "\n\nWarning: Chameleon Ink may appear white when heated and exiting nozzle.";
      }

      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      var num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(DoNextExtrusionStep), (object) new FilamentInsertNewFilament.ExtrusionProcessData(progressBar, selectedPrinter, 0), "G90", "G92");
    }

    private void DoNextExtrusionStep(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        MainWindow.ResetToStartup();
      }
      else
      {
        var asyncState = ar.AsyncState as FilamentInsertNewFilament.ExtrusionProcessData;
        var num1 = (float) asyncState.amount_extruded / 80f;
        asyncState.progressBar.PercentComplete = num1;
        if ((double) asyncState.amount_extruded == 80.0)
        {
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page5_HasFilamentExited, CurrentDetails);
        }
        else
        {
          asyncState.amount_extruded += 10;
          var num2 = (int) asyncState.printer.SendManualGCode(new AsyncCallback(DoNextExtrusionStep), (object) asyncState, PrinterCompatibleString.Format("G0 F450 E{0}", (object) asyncState.amount_extruded));
        }
      }
    }

    private class ExtrusionProcessData
    {
      public ProgressBarWidget progressBar;
      public PrinterObject printer;
      public int amount_extruded;

      public ExtrusionProcessData(ProgressBarWidget progressBar, PrinterObject printer, int amount_extruded)
      {
        this.progressBar = progressBar;
        this.printer = printer;
        this.amount_extruded = amount_extruded;
      }
    }
  }
}
