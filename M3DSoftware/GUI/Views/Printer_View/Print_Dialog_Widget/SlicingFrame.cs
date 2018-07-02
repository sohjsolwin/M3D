// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.SlicingFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System.Diagnostics;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class SlicingFrame : AbstractSliceAndPrintFrame
  {
    private PrintJobDetails CurrentJobDetails;
    private PopupMessageBox message_box;
    private PrinterView printerview;
    private SettingsManager settings_manager;
    private ButtonWidget cancel_button;
    private ProgressBarWidget progressbar;
    private TextWidget pleasewait_text;

    public SlicingFrame(int ID, GUIHost host, PrinterView printerview, PopupMessageBox message_box, SettingsManager settings_manager, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      this.message_box = message_box;
      this.printerview = printerview;
      this.settings_manager = settings_manager;
      Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      PrintDialogWindow.SetSize(480, 340);
      PrintDialogWindow.Refresh();
      ResetSlicerState();
      CurrentJobDetails = details;
      CurrentJobDetails.Estimated_Filament = -1f;
      CurrentJobDetails.Estimated_Print_Time = -1f;
      Enabled = true;
      cancel_button.Visible = true;
      progressbar.Visible = true;
      pleasewait_text.Visible = true;
      cancel_button.CenterHorizontallyInParent = true;
      SetSize(480, 340);
      StartSlicer(CurrentJobDetails.settings);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      var borderedImageFrame = new BorderedImageFrame(ID, (Element2D) null);
      AddChildElement((Element2D) borderedImageFrame);
      SetSize(480, 340);
      borderedImageFrame.Init(host, "guicontrols", 640f, 256f, 703f, 319f, 8, 8, 64, 8, 8, 64);
      borderedImageFrame.SetSize(480, 340);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      var textWidget = new TextWidget(0)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "T_PrintDialog_SlicingWarning",
        Color = new Color4((byte)100, (byte)100, (byte)100, byte.MaxValue)
      };
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) textWidget);
      pleasewait_text = new TextWidget(0)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "T_PrintDialog_SlicingPleaseWait",
        Color = new Color4((byte)100, (byte)100, (byte)100, byte.MaxValue)
      };
      pleasewait_text.SetPosition(0, 109);
      pleasewait_text.SetSize(247, 50);
      pleasewait_text.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D)pleasewait_text);
      progressbar = new ProgressBarWidget(0);
      progressbar.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 2, 2, 16, 2, 2, 16);
      progressbar.SetPosition(42, 185);
      progressbar.SetSize(401, 24);
      progressbar.PercentComplete = 0.0f;
      borderedImageFrame.AddChildElement((Element2D)progressbar);
      cancel_button = new ButtonWidget(0);
      cancel_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      cancel_button.Size = FontSize.Medium;
      cancel_button.Text = "Cancel";
      cancel_button.SetGrowableWidth(4, 4, 32);
      cancel_button.SetGrowableHeight(4, 4, 32);
      cancel_button.SetSize(100, 32);
      cancel_button.SetPosition(0, -46);
      cancel_button.CenterHorizontallyInParent = true;
      cancel_button.SetCallback(new ButtonCallback(MyButtonCallback));
      borderedImageFrame.AddChildElement((Element2D)cancel_button);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 0)
      {
        return;
      }

      PrintDialogWindow.CloseWindow();
      if (SlicerConnection != null)
      {
        SlicerConnection.Cancel();
      }

      if (CurrentJobDetails.printer == null)
      {
        return;
      }

      var num = (int)CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(ReleasePrinterAfterCommand), (object)CurrentJobDetails.printer, "M106 S0");
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock((AsyncCallback) null, (object) null);
    }

    private void FailedReleaseCallback(IAsyncCallResult ar)
    {
      message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_Failed_ErrorSendingToPrinter"), PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnPrintJobStarted(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null || ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived)
      {
        return;
      }

      var num = (int) asyncState.ReleaseLock(new AsyncCallback(FailedReleaseCallback), (object) asyncState);
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
      if (progressbar == null)
      {
        return;
      }

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
      if (bHasSlicingCompleted)
      {
        if ((double)CurrentJobDetails.Estimated_Print_Time < 0.0)
        {
          CurrentJobDetails.Estimated_Print_Time = (float)SlicerConnection.EstimatedPrintTimeSeconds;
        }
        else if ((double)CurrentJobDetails.Estimated_Filament < 0.0)
        {
          CurrentJobDetails.Estimated_Filament = SlicerConnection.EstimatedFilament;
        }
        else
        {
          ResetSlicerState();
          EvaluateRemainingFilament();
        }
      }
      if (!bHasSlicerStarted)
      {
        return;
      }

      progressbar.PercentComplete = SlicerConnection.EstimatedPercentComplete;
    }

    private void PrepareToStartPrint()
    {
      PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PreparingToStartFrame, CurrentJobDetails);
    }

    private void EvaluateRemainingFilament()
    {
      var num = 0.75f * CurrentJobDetails.settings.filament_info.GetMaxFilamentBySpoolSize();
      FilamentSpool filamentInfo = CurrentJobDetails.settings.filament_info;
      if (settings_manager.CurrentFilamentSettings.TrackFilament && (double) filamentInfo.estimated_filament_length_printed >= (double) num && (double)CurrentJobDetails.Estimated_Filament < (double) filamentInfo.GetMaxFilamentBySpoolSize() - (double) filamentInfo.estimated_filament_length_printed)
      {
        message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrintDialog_LowFilament"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(OrderFilamentCallBack));
      }
      else if (settings_manager.CurrentFilamentSettings.TrackFilament && (double)CurrentJobDetails.Estimated_Filament > (double) filamentInfo.GetMaxFilamentBySpoolSize() - (double) filamentInfo.estimated_filament_length_printed)
      {
        message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrintDialog_LowFilamentContinue"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(ContinuePrintCallBack));
      }
      else
      {
        PrepareToStartPrint();
      }
    }

    private void OrderFilamentCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        Process.Start("https://printm3d.com/3d-printer-filaments");
      }

      PrepareToStartPrint();
    }

    private void ContinuePrintCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        PrepareToStartPrint();
      }
      else
      {
        PrintDialogWindow.CloseWindow();
        if (CurrentJobDetails.printer == null)
        {
          return;
        }

        var num = (int)CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(ReleasePrinterAfterCommand), (object)CurrentJobDetails.printer, "M106 S0");
      }
    }

    private enum PrintDialogControlID
    {
      CancelButton,
    }
  }
}
