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
      printerview = printer_view;
      this.recentPrints = recentPrints;
      printer_list = new List<PrinterInfo>();
      countdown_timer = new Stopwatch();
      Init(host);
    }

    public override void OnActivate(PrintJobDetails details)
    {
      PrintDialogWindow.SetSize(480, 340);
      PrintDialogWindow.Refresh();
      CurrentJobDetails = details;
      var num1 = (int)CurrentJobDetails.Estimated_Print_Time + 1800;
      var num2 = num1 / 60;
      var num3 = num2 / 60;
      var num4 = num2 - num3 * 60;
      CurrentJobDetails.Estimated_Print_Time = num1;
      estimated_time.Text = num3.ToString() + " hours, " + num4.ToString() + " minutes";
      estimated_filament.Text = ((int)CurrentJobDetails.Estimated_Filament * 0.0393701f).ToString() + " inches";
      countdown_timer.Restart();
    }

    public override void OnDeactivate()
    {
    }

    public void Init(GUIHost host)
    {
      var borderedImageFrame = new BorderedImageFrame(ID, null);
      AddChildElement(borderedImageFrame);
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
        Text = "T_PrintDialog_PrintWillBeginShortly",
        Color = new Color4(100, 100, 100, byte.MaxValue)
      };
      textWidget.SetPosition(0, 10);
      textWidget.SetSize(480, 80);
      textWidget.CenterHorizontallyInParent = true;
      borderedImageFrame.AddChildElement(textWidget);
      autostart_text = new TextWidget(0)
      {
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "T_PrintDialog_AutoStartingIn",
        Color = new Color4(100, 100, 100, byte.MaxValue)
      };
      autostart_text.SetPosition(87, 193);
      autostart_text.SetSize(140, 30);
      borderedImageFrame.AddChildElement(autostart_text);
      timer_progress = new SpriteAnimationWidget(1);
      timer_progress.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      timer_progress.SetSize(128, 108);
      timer_progress.SetPosition(238, 150);
      borderedImageFrame.AddChildElement(timer_progress);
      timer_text = new TextWidget(0)
      {
        Size = FontSize.VeryLarge,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Middle,
        Text = "30"
      };
      timer_text.SetSize(128, 108);
      timer_text.SetPosition(238, 150);
      timer_text.Color = new Color4(100, 100, 100, byte.MaxValue);
      borderedImageFrame.AddChildElement(timer_text);
      continue_button = new ButtonWidget(1);
      continue_button.Init(host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      continue_button.Size = FontSize.Medium;
      continue_button.Text = "T_PrintDialog_StartNow";
      continue_button.SetGrowableWidth(4, 4, 32);
      continue_button.SetGrowableHeight(4, 4, 32);
      continue_button.SetSize(100, 32);
      continue_button.SetPosition(100, -46);
      continue_button.SetCallback(new ButtonCallback(MyButtonCallback));
      borderedImageFrame.AddChildElement(continue_button);
      cancel_button = new ButtonWidget(0);
      cancel_button.Init(host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      cancel_button.Size = FontSize.Medium;
      cancel_button.Text = "T_Cancel";
      cancel_button.SetGrowableWidth(4, 4, 32);
      cancel_button.SetGrowableHeight(4, 4, 32);
      cancel_button.SetSize(100, 32);
      cancel_button.SetPosition(-204, -46);
      cancel_button.CenterHorizontallyInParent = false;
      cancel_button.SetCallback(new ButtonCallback(MyButtonCallback));
      borderedImageFrame.AddChildElement(cancel_button);
      estimated_time_label = new TextWidget(0)
      {
        Text = "T_PrintDialog_EstimatedTime",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left
      };
      estimated_time_label.SetPosition(54, 96);
      estimated_time_label.SetSize(164, 24);
      estimated_time_label.Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f);
      estimated_time = new TextWidget(0)
      {
        Text = "The minions are working",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left
      };
      estimated_time.SetPosition(220, 96);
      estimated_time.SetSize(275, 24);
      estimated_time.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      estimated_filament_label = new TextWidget(0)
      {
        Text = "T_PrintDialog_EstimatedFilament",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left
      };
      estimated_filament_label.SetPosition(54, 122);
      estimated_filament_label.SetSize(164, 24);
      estimated_filament_label.Color = new Color4(0.9922f, 0.3765f, 0.2471f, 1f);
      estimated_filament = new TextWidget(0)
      {
        Text = "The minions are working",
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left
      };
      estimated_filament.SetPosition(220, 122);
      estimated_filament.SetSize(275, 24);
      estimated_filament.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      borderedImageFrame.AddChildElement(estimated_time_label);
      borderedImageFrame.AddChildElement(estimated_time);
      borderedImageFrame.AddChildElement(estimated_filament_label);
      borderedImageFrame.AddChildElement(estimated_filament);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          PrintDialogWindow.CloseWindow();
          if (CurrentJobDetails.printer == null)
          {
            break;
          }

          var num = (int)CurrentJobDetails.printer.SendManualGCode(new AsyncCallback(ReleasePrinterAfterCommand), CurrentJobDetails.printer, "M106 S0");
          break;
        case 1:
          CloseAndStart();
          break;
      }
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

    private void CloseAndStart()
    {
      countdown_timer.Stop();
      PrintSlicedModel(CurrentJobDetails, recentPrints, new AsyncCallback(OnPrintJobStarted));
      PrintDialogWindow.CloseWindow();
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

      var num = (int) asyncState.ReleaseLock(new AsyncCallback(FailedReleaseCallback), asyncState);
    }

    public override void OnUpdate()
    {
      if (!Visible || !countdown_timer.IsRunning)
      {
        return;
      }

      var num = 30 - countdown_timer.Elapsed.Seconds;
      if (num < 0)
      {
        num = 0;
        CloseAndStart();
      }
      timer_text.Text = num.ToString();
    }

    private enum PrintDialogControlID
    {
      CancelButton,
      ContinueButton,
    }
  }
}
