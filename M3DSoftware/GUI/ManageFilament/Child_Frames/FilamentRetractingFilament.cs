using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentRetractingFilament : Manage3DInkChildWindow
  {
    private ProgressBarWidget progressBar;

    public FilamentRetractingFilament(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
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
      CreateManageFilamentFrame("Retracting Current 3D Ink", "We're retracting your current 3D Ink.\n\nPull on the 3D Ink until it's released.", true, false, true, false, false, false);
      progressBar = (ProgressBarWidget)FindChildElement(4);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      var num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(DoNextRetractionStep), (object) new FilamentRetractingFilament.RetractionProcessData(progressBar, selectedPrinter, 0), "G90", "G92");
    }

    private void DoNextRetractionStep(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        MainWindow.ResetToStartup();
      }
      else
      {
        var asyncState = ar.AsyncState as FilamentRetractingFilament.RetractionProcessData;
        var num1 = (float) asyncState.amount_retracted / 80f;
        asyncState.progressBar.PercentComplete = num1;
        if ((double) asyncState.amount_retracted == 80.0)
        {
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page3_HasRetractedFilament, CurrentDetails);
        }
        else
        {
          asyncState.amount_retracted += 10;
          var num2 = (int) asyncState.printer.SendManualGCode(new AsyncCallback(DoNextRetractionStep), (object) asyncState, PrinterCompatibleString.Format("G0 F450 E-{0}", (object) asyncState.amount_retracted));
        }
      }
    }

    private class RetractionProcessData
    {
      public ProgressBarWidget progressBar;
      public PrinterObject printer;
      public int amount_retracted;

      public RetractionProcessData(ProgressBarWidget progressBar, PrinterObject printer, int amount_retracted)
      {
        this.progressBar = progressBar;
        this.printer = printer;
        this.amount_retracted = amount_retracted;
      }
    }
  }
}
