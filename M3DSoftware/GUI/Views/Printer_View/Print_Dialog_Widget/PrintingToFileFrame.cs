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
      ResetSlicerState();
      Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      PrintDialogWindow.SetSize(480, 340);
      PrintDialogWindow.Refresh();
      ResetSlicerState();
      bProcessingAndSavingModel = false;
      bCanCheckForNoJob = false;
      CurrentJobDetails = details;
      CurrentJobDetails.Estimated_Filament = -1f;
      CurrentJobDetails.Estimated_Print_Time = -1f;
      Enabled = true;
      status_text.Visible = true;
      SetSize(480, 340);
      StartSlicer(CurrentJobDetails.settings);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      var printToFileDialog = Resources.PrintToFileDialog;
      var xmlFrame = new XMLFrame(ID)
      {
        RelativeWidth = 1f,
        RelativeHeight = 1f
      };
      AddChildElement(xmlFrame);
      xmlFrame.Init(host, printToFileDialog, new ButtonCallback(MyButtonCallback));
      status_text = (TextWidget) xmlFrame.FindChildElement("statustext");
      var childElement = (Frame) xmlFrame.FindChildElement(1005);
      this.progressbar = new ProgressBarWidget(0);
      this.progressbar.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 2, 2, 16, 2, 2, 16);
      this.progressbar.SetPosition(32, 64);
      this.progressbar.SetSize(280, 24);
      this.progressbar.PercentComplete = 0.0f;
      ProgressBarWidget progressbar = this.progressbar;
      childElement.AddChildElement(progressbar);
      SetSize(480, 200);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock(null, null);
    }

    private void FailedReleaseCallback(IAsyncCallResult ar)
    {
      PrintDialogWindow.CloseWindow();
      message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_Failed_ErrorSendingToPrinter"), PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnPrintJobStarted(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        return;
      }

      if (ar.CallResult != CommandResult.Success && ar.CallResult != CommandResult.SuccessfullyReceived)
      {
        var num = (int) asyncState.ReleaseLock(new AsyncCallback(FailedReleaseCallback), asyncState);
      }
      else
      {
        bProcessingAndSavingModel = true;
        bCanCheckForNoJob = false;
        progressbar.Visible = false;
        spoolerProcessingTimer.Restart();
      }
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!Visible)
      {
        return;
      }

      OnProcess();
    }

    public void OnProcess()
    {
      if (!bProcessingAndSavingModel)
      {
        ProcessSlicing();
      }
      else
      {
        ProcessSpoolerProcessing();
      }
    }

    private void ProcessSlicing()
    {
      status_text.Text = "Slicing Model ";
      for (var str = ProcessNextSlicerMessage(); str != null; str = ProcessNextSlicerMessage())
      {
        if (str == "Slicer Started")
        {
          progressbar.PercentComplete = 0.0f;
        }
        else if (str == "Slicer Finished")
        {
          progressbar.PercentComplete = 1f;
        }
      }
      if (BHasSlicingCompleted)
      {
        if (CurrentJobDetails.Estimated_Print_Time < 0.0)
        {
          CurrentJobDetails.Estimated_Print_Time = SlicerConnection.EstimatedPrintTimeSeconds;
        }
        else if (CurrentJobDetails.Estimated_Filament < 0.0)
        {
          CurrentJobDetails.Estimated_Filament = SlicerConnection.EstimatedFilament;
        }
        else
        {
          ResetSlicerState();
          StartPrintingToFile();
        }
      }
      if (!BHasSlicerStarted)
      {
        return;
      }

      progressbar.PercentComplete = SlicerConnection.EstimatedPercentComplete;
      progressbar.Visible = true;
    }

    private void ProcessSpoolerProcessing()
    {
      status_text.Text = "Preparing gcode for printer and saving ";
      PrinterObject printer = CurrentJobDetails.printer;
      if (printer != null)
      {
        var num = printer.isHealthy ? 1 : 0;
      }
      if (printer.Info.current_job != null)
      {
        bCanCheckForNoJob = true;
      }

      if (!bCanCheckForNoJob && spoolerProcessingTimer.ElapsedMilliseconds <= 30000L || printer.Info.current_job != null)
      {
        return;
      }

      PrintDialogWindow.CloseWindow();
      message_box.AddMessageToQueue("File saved");
    }

    private void StartPrintingToFile()
    {
      PrintSlicedModel(CurrentJobDetails, null, new AsyncCallback(OnPrintJobStarted));
    }

    private enum PrintDialogControlID
    {
      CancelButton,
    }
  }
}
