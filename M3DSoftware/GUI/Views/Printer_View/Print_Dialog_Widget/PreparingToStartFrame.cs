// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.PreparingToStartFrame
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
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class PreparingToStartFrame : AbstractSliceAndPrintFrame
  {
    private PrintJobDetails CurrentJobDetails;
    private PopupMessageBox message_box;
    private ButtonWidget continue_button;
    private ButtonWidget cancel_button;
    private TextWidget estimated_time_label;
    private TextWidget estimated_time;
    private TextWidget estimated_filament_label;
    private TextWidget estimated_filament;
    private TextWidget autostart_text;
    private SpriteAnimationWidget timer_progress;
    private TextWidget timer_text;
    private Stopwatch countdown_timer;
    private List<PrinterInfo> printer_list;
    private PrinterView printerview;
    private RecentPrintsTab recentPrints;

    public PreparingToStartFrame(int ID, GUIHost host, PrinterView printer_view, PopupMessageBox message_box, RecentPrintsTab recentPrints, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      this.message_box = message_box;
      this.printerview = printer_view;
      this.recentPrints = recentPrints;
      this.printer_list = new List<PrinterInfo>();
      this.countdown_timer = new Stopwatch();
      this.Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.PrintDialogWindow.SetSize(480, 340);
      this.PrintDialogWindow.Refresh();
      this.CurrentJobDetails = details;
      int num1 = (int) this.CurrentJobDetails.Estimated_Print_Time + 1800;
      int num2 = num1 / 60;
      int num3 = num2 / 60;
      int num4 = num2 - num3 * 60;
      this.CurrentJobDetails.Estimated_Print_Time = (float) num1;
      this.estimated_time.Text = num3.ToString() + " hours, " + num4.ToString() + " minutes";
      this.estimated_filament.Text = ((float) (int) this.CurrentJobDetails.Estimated_Filament * 0.0393701f).ToString() + " inches";
      this.countdown_timer.Restart();
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
      textWidget.Text = "T_PrintDialog_PrintWillBeginShortly";
      textWidget.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement((Element2D) textWidget);
      this.autostart_text = new TextWidget(0);
      this.autostart_text.Size = FontSize.Medium;
      this.autostart_text.Alignment = QFontAlignment.Centre;
      this.autostart_text.VAlignment = TextVerticalAlignment.Middle;
      this.autostart_text.Text = "T_PrintDialog_AutoStartingIn";
      this.autostart_text.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      this.autostart_text.SetPosition(87, 193);
      this.autostart_text.SetSize(140, 30);
      borderedImageFrame.AddChildElement((Element2D) this.autostart_text);
      this.timer_progress = new SpriteAnimationWidget(1);
      this.timer_progress.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      this.timer_progress.SetSize(128, 108);
      this.timer_progress.SetPosition(238, 150);
      borderedImageFrame.AddChildElement((Element2D) this.timer_progress);
      this.timer_text = new TextWidget(0);
      this.timer_text.Size = FontSize.VeryLarge;
      this.timer_text.Alignment = QFontAlignment.Centre;
      this.timer_text.VAlignment = TextVerticalAlignment.Middle;
      this.timer_text.Text = "30";
      this.timer_text.SetSize(128, 108);
      this.timer_text.SetPosition(238, 150);
      this.timer_text.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      borderedImageFrame.AddChildElement((Element2D) this.timer_text);
      this.continue_button = new ButtonWidget(1);
      this.continue_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.continue_button.Size = FontSize.Medium;
      this.continue_button.Text = "T_PrintDialog_StartNow";
      this.continue_button.SetGrowableWidth(4, 4, 32);
      this.continue_button.SetGrowableHeight(4, 4, 32);
      this.continue_button.SetSize(100, 32);
      this.continue_button.SetPosition(100, -46);
      this.continue_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      borderedImageFrame.AddChildElement((Element2D) this.continue_button);
      this.cancel_button = new ButtonWidget(0);
      this.cancel_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.cancel_button.Size = FontSize.Medium;
      this.cancel_button.Text = "T_Cancel";
      this.cancel_button.SetGrowableWidth(4, 4, 32);
      this.cancel_button.SetGrowableHeight(4, 4, 32);
      this.cancel_button.SetSize(100, 32);
      this.cancel_button.SetPosition(-204, -46);
      this.cancel_button.CenterHorizontallyInParent = false;
      this.cancel_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      borderedImageFrame.AddChildElement((Element2D) this.cancel_button);
      this.estimated_time_label = new TextWidget(0);
      this.estimated_time_label.Text = "T_PrintDialog_EstimatedTime";
      this.estimated_time_label.Size = FontSize.Medium;
      this.estimated_time_label.Alignment = QFontAlignment.Left;
      this.estimated_time_label.SetPosition(54, 96);
      this.estimated_time_label.SetSize(164, 24);
      this.estimated_time_label.Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f);
      this.estimated_time = new TextWidget(0);
      this.estimated_time.Text = "The minions are working";
      this.estimated_time.Size = FontSize.Medium;
      this.estimated_time.Alignment = QFontAlignment.Left;
      this.estimated_time.SetPosition(220, 96);
      this.estimated_time.SetSize(275, 24);
      this.estimated_time.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.estimated_filament_label = new TextWidget(0);
      this.estimated_filament_label.Text = "T_PrintDialog_EstimatedFilament";
      this.estimated_filament_label.Size = FontSize.Medium;
      this.estimated_filament_label.Alignment = QFontAlignment.Left;
      this.estimated_filament_label.SetPosition(54, 122);
      this.estimated_filament_label.SetSize(164, 24);
      this.estimated_filament_label.Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f);
      this.estimated_filament = new TextWidget(0);
      this.estimated_filament.Text = "The minions are working";
      this.estimated_filament.Size = FontSize.Medium;
      this.estimated_filament.Alignment = QFontAlignment.Left;
      this.estimated_filament.SetPosition(220, 122);
      this.estimated_filament.SetSize(275, 24);
      this.estimated_filament.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      borderedImageFrame.AddChildElement((Element2D) this.estimated_time_label);
      borderedImageFrame.AddChildElement((Element2D) this.estimated_time);
      borderedImageFrame.AddChildElement((Element2D) this.estimated_filament_label);
      borderedImageFrame.AddChildElement((Element2D) this.estimated_filament);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          this.PrintDialogWindow.CloseWindow();
          if (this.CurrentJobDetails.printer == null)
            break;
          int num = (int) this.CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(this.ReleasePrinterAfterCommand), (object) this.CurrentJobDetails.printer, "M106 S0");
          break;
        case 1:
          this.CloseAndStart();
          break;
      }
    }

    private void ReleasePrinterAfterCommand(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState == null)
        return;
      int num = (int) asyncState.ReleaseLock((AsyncCallback) null, (object) null);
    }

    private void CloseAndStart()
    {
      this.countdown_timer.Stop();
      this.PrintSlicedModel(this.CurrentJobDetails, this.recentPrints, new AsyncCallback(this.OnPrintJobStarted));
      this.PrintDialogWindow.CloseWindow();
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
      if (!this.Visible || !this.countdown_timer.IsRunning)
        return;
      int num = 30 - this.countdown_timer.Elapsed.Seconds;
      if (num < 0)
      {
        num = 0;
        this.CloseAndStart();
      }
      this.timer_text.Text = num.ToString();
    }

    private enum PrintDialogControlID
    {
      CancelButton,
      ContinueButton,
    }
  }
}
