// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentRetractingFilament
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
        return;
      this.MainWindow.ResetToStartup();
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Retracting Current 3D Ink", "We're retracting your current 3D Ink.\n\nPull on the 3D Ink until it's released.", true, false, true, false, false, false);
      this.progressBar = (ProgressBarWidget) this.FindChildElement(4);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      int num = (int) selectedPrinter.SendManualGCode(new AsyncCallback(this.DoNextRetractionStep), (object) new FilamentRetractingFilament.RetractionProcessData(this.progressBar, selectedPrinter, 0), "G90", "G92");
    }

    private void DoNextRetractionStep(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        this.MainWindow.ResetToStartup();
      }
      else
      {
        FilamentRetractingFilament.RetractionProcessData asyncState = ar.AsyncState as FilamentRetractingFilament.RetractionProcessData;
        float num1 = (float) asyncState.amount_retracted / 80f;
        asyncState.progressBar.PercentComplete = num1;
        if ((double) asyncState.amount_retracted == 80.0)
        {
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page3_HasRetractedFilament, this.CurrentDetails);
        }
        else
        {
          asyncState.amount_retracted += 10;
          int num2 = (int) asyncState.printer.SendManualGCode(new AsyncCallback(this.DoNextRetractionStep), (object) asyncState, PrinterCompatibleString.Format("G0 F450 E-{0}", (object) asyncState.amount_retracted));
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
