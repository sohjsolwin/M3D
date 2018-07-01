﻿// Decompiled with JetBrains decompiler
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
      this.Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.PrintDialogWindow.SetSize(480, 340);
      this.PrintDialogWindow.Refresh();
      this.ResetSlicerState();
      this.CurrentJobDetails = details;
      this.CurrentJobDetails.Estimated_Filament = -1f;
      this.CurrentJobDetails.Estimated_Print_Time = -1f;
      this.Enabled = true;
      this.cancel_button.Visible = true;
      this.progressbar.Visible = true;
      this.pleasewait_text.Visible = true;
      this.cancel_button.CenterHorizontallyInParent = true;
      this.SetSize(480, 340);
      this.StartSlicer(this.CurrentJobDetails.settings);
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      BorderedImageFrame borderedImageFrame = new BorderedImageFrame(this.ID, (Element2D) null);
      this.AddChildElement((Element2D) borderedImageFrame);
      this.SetSize(480, 340);
      borderedImageFrame.Init(host, "guicontrols", 640f, 256f, 703f, 319f, 8, 8, 64, 8, 8, 64);
      borderedImageFrame.SetSize(480, 340);
      borderedImageFrame.CenterHorizontallyInParent = true;
      borderedImageFrame.CenterVerticallyInParent = true;
      TextWidget textWidget = new TextWidget(0);
      textWidget.Size = FontSize.Medium;
      textWidget.Alignment = QFontAlignment.Centre;
      textWidget.VAlignment = TextVerticalAlignment.Middle;
      textWidget.Text = "T_PrintDialog_SlicingWarning";
      textWidget.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) textWidget);
      this.pleasewait_text = new TextWidget(0);
      this.pleasewait_text.Size = FontSize.Medium;
      this.pleasewait_text.Alignment = QFontAlignment.Centre;
      this.pleasewait_text.VAlignment = TextVerticalAlignment.Middle;
      this.pleasewait_text.Text = "T_PrintDialog_SlicingPleaseWait";
      this.pleasewait_text.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      this.pleasewait_text.SetPosition(0, 109);
      this.pleasewait_text.SetSize(247, 50);
      this.pleasewait_text.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) this.pleasewait_text);
      this.progressbar = new ProgressBarWidget(0);
      this.progressbar.Init(host, "guicontrols", 944f, 96f, 960f, 144f, 2, 2, 16, 2, 2, 16);
      this.progressbar.SetPosition(42, 185);
      this.progressbar.SetSize(401, 24);
      this.progressbar.PercentComplete = 0.0f;
      borderedImageFrame.AddChildElement((Element2D) this.progressbar);
      this.cancel_button = new ButtonWidget(0);
      this.cancel_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.cancel_button.Size = FontSize.Medium;
      this.cancel_button.Text = "Cancel";
      this.cancel_button.SetGrowableWidth(4, 4, 32);
      this.cancel_button.SetGrowableHeight(4, 4, 32);
      this.cancel_button.SetSize(100, 32);
      this.cancel_button.SetPosition(0, -46);
      this.cancel_button.CenterHorizontallyInParent = true;
      this.cancel_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      borderedImageFrame.AddChildElement((Element2D) this.cancel_button);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 0)
        return;
      this.PrintDialogWindow.CloseWindow();
      if (this.SlicerConnection != null)
        this.SlicerConnection.Cancel();
      if (this.CurrentJobDetails.printer == null)
        return;
      int num = (int) this.CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(this.ReleasePrinterAfterCommand), (object) this.CurrentJobDetails.printer, "M106 S0");
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
      this.message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_Failed_ErrorSendingToPrinter"), PopupMessageBox.MessageBoxButtons.OK);
    }

    private void OnPrintJobStarted(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null || ar.CallResult == CommandResult.Success || ar.CallResult == CommandResult.SuccessfullyReceived)
        return;
      int num = (int) asyncState.ReleaseLock(new AsyncCallback(this.FailedReleaseCallback), (object) asyncState);
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
      if (this.progressbar == null)
        return;
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
          this.EvaluateRemainingFilament();
        }
      }
      if (!this.bHasSlicerStarted)
        return;
      this.progressbar.PercentComplete = this.SlicerConnection.EstimatedPercentComplete;
    }

    private void PrepareToStartPrint()
    {
      this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PreparingToStartFrame, this.CurrentJobDetails);
    }

    private void EvaluateRemainingFilament()
    {
      float num = 0.75f * this.CurrentJobDetails.settings.filament_info.GetMaxFilamentBySpoolSize();
      FilamentSpool filamentInfo = this.CurrentJobDetails.settings.filament_info;
      if (this.settings_manager.CurrentFilamentSettings.TrackFilament && (double) filamentInfo.estimated_filament_length_printed >= (double) num && (double) this.CurrentJobDetails.Estimated_Filament < (double) filamentInfo.GetMaxFilamentBySpoolSize() - (double) filamentInfo.estimated_filament_length_printed)
        this.message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrintDialog_LowFilament"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.OrderFilamentCallBack));
      else if (this.settings_manager.CurrentFilamentSettings.TrackFilament && (double) this.CurrentJobDetails.Estimated_Filament > (double) filamentInfo.GetMaxFilamentBySpoolSize() - (double) filamentInfo.estimated_filament_length_printed)
        this.message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrintDialog_LowFilamentContinue"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.ContinuePrintCallBack));
      else
        this.PrepareToStartPrint();
    }

    private void OrderFilamentCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
        Process.Start("https://printm3d.com/3d-printer-filaments");
      this.PrepareToStartPrint();
    }

    private void ContinuePrintCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result == PopupMessageBox.PopupResult.Button1_YesOK)
      {
        this.PrepareToStartPrint();
      }
      else
      {
        this.PrintDialogWindow.CloseWindow();
        if (this.CurrentJobDetails.printer == null)
          return;
        int num = (int) this.CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(this.ReleasePrinterAfterCommand), (object) this.CurrentJobDetails.printer, "M106 S0");
      }
    }

    private enum PrintDialogControlID
    {
      CancelButton,
    }
  }
}
