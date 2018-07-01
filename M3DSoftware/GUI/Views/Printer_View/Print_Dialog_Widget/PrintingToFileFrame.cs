// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.PrintingToFileFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.GUI.Views.Library_View;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System.Diagnostics;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class PrintingToFileFrame : AbstractSliceAndPrintFrame
  {
    private Stopwatch spoolerProcessingTimer = new Stopwatch();
    private PrintJobDetails CurrentJobDetails;
    private PopupMessageBox message_box;
    private ProgressBarWidget progressbar;
    private TextWidget status_text;
    private bool bProcessingAndSavingModel;
    private bool bCanCheckForNoJob;

    public PrintingToFileFrame(int ID, GUIHost host, PopupMessageBox message_box, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      this.message_box = message_box;
      this.ResetSlicerState();
      this.Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.PrintDialogWindow.SetSize(480, 340);
      this.PrintDialogWindow.Refresh();
      this.ResetSlicerState();
      this.bProcessingAndSavingModel = false;
      this.bCanCheckForNoJob = false;
      this.CurrentJobDetails = details;
      this.CurrentJobDetails.Estimated_Filament = -1f;
      this.CurrentJobDetails.Estimated_Print_Time = -1f;
      this.Enabled = true;
      this.status_text.Visible = true;
      this.SetSize(480, 340);
      this.StartSlicer(this.CurrentJobDetails.settings);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      string printToFileDialog = Resources.PrintToFileDialog;
      XMLFrame xmlFrame = new XMLFrame(this.ID);
      xmlFrame.RelativeWidth = 1f;
      xmlFrame.RelativeHeight = 1f;
      this.AddChildElement((Element2D) xmlFrame);
      xmlFrame.Init(host, printToFileDialog, new ButtonCallback(this.MyButtonCallback));
      this.status_text = (TextWidget) xmlFrame.FindChildElement("statustext");
      Frame childElement = (Frame) xmlFrame.FindChildElement(1005);
      this.progressbar = new ProgressBarWidget(0);
      this.progressbar.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 2, 2, 16, 2, 2, 16);
      this.progressbar.SetPosition(32, 64);
      this.progressbar.SetSize(280, 24);
      this.progressbar.PercentComplete = 0.0f;
      ProgressBarWidget progressbar = this.progressbar;
      childElement.AddChildElement((Element2D) progressbar);
      this.SetSize(480, 200);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        return;
      int num = (int) asyncState.ReleaseLock((AsyncCallback) null, (object) null);
    }

    private void FailedReleaseCallback(IAsyncCallResult ar)
    {
      this.PrintDialogWindow.CloseWindow();
      this.message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_Failed_ErrorSendingToPrinter"), PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnPrintJobStarted(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        return;
      if (ar.CallResult != CommandResult.Success && ar.CallResult != CommandResult.SuccessfullyReceived)
      {
        int num = (int) asyncState.ReleaseLock(new AsyncCallback(this.FailedReleaseCallback), (object) asyncState);
      }
      else
      {
        this.bProcessingAndSavingModel = true;
        this.bCanCheckForNoJob = false;
        this.progressbar.Visible = false;
        this.spoolerProcessingTimer.Restart();
      }
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!this.Visible)
        return;
      this.OnProcess();
    }

    public void OnProcess()
    {
      if (!this.bProcessingAndSavingModel)
        this.ProcessSlicing();
      else
        this.ProcessSpoolerProcessing();
    }

    private void ProcessSlicing()
    {
      this.status_text.Text = "Slicing Model ";
      for (string str = this.ProcessNextSlicerMessage(); str != null; str = this.ProcessNextSlicerMessage())
      {
        if (str == "Slicer Started")
          this.progressbar.PercentComplete = 0.0f;
        else if (str == "Slicer Finished")
          this.progressbar.PercentComplete = 1f;
      }
      if (this.bHasSlicingCompleted)
      {
        if ((double) this.CurrentJobDetails.Estimated_Print_Time < 0.0)
          this.CurrentJobDetails.Estimated_Print_Time = (float) this.SlicerConnection.EstimatedPrintTimeSeconds;
        else if ((double) this.CurrentJobDetails.Estimated_Filament < 0.0)
        {
          this.CurrentJobDetails.Estimated_Filament = this.SlicerConnection.EstimatedFilament;
        }
        else
        {
          this.ResetSlicerState();
          this.StartPrintingToFile();
        }
      }
      if (!this.bHasSlicerStarted)
        return;
      this.progressbar.PercentComplete = this.SlicerConnection.EstimatedPercentComplete;
      this.progressbar.Visible = true;
    }

    private void ProcessSpoolerProcessing()
    {
      this.status_text.Text = "Preparing gcode for printer and saving ";
      PrinterObject printer = this.CurrentJobDetails.printer;
      if (printer != null)
      {
        int num = printer.isHealthy ? 1 : 0;
      }
      if (printer.Info.current_job != null)
        this.bCanCheckForNoJob = true;
      if (!this.bCanCheckForNoJob && this.spoolerProcessingTimer.ElapsedMilliseconds <= 30000L || printer.Info.current_job != null)
        return;
      this.PrintDialogWindow.CloseWindow();
      this.message_box.AddMessageToQueue("File saved");
    }

    private void StartPrintingToFile()
    {
      this.PrintSlicedModel(this.CurrentJobDetails, (RecentPrintsTab) null, new AsyncCallback(this.OnPrintJobStarted));
    }

    private enum PrintDialogControlID
    {
      CancelButton,
    }
  }
}
