using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.Slicer.General;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.Views.Printer_View
{
  public class PrinterStatusDialog : PrinterDecorator
  {
    public bool found;
    public JobInfo current_job;
    private BorderedImageFrame status_dialog_frame;
    private ButtonWidget more_info;
    private ImageWidget preview_image;
    private QuadWidget quad_overlay;
    private TextWidget printer_name_text;
    private TextWidget status_text;
    private TextWidget remain_text;
    private ButtonWidget abort_button;
    private ButtonWidget pause_print;
    private ButtonWidget continue_print;
    private SpriteAnimationWidget progress_indicator;
    private TextWidget modelName_text;
    private TextWidget infill_text;
    private TextWidget quality_text;
    private TextWidget options_text;
    private TextWidget heater_temp_text;
    private TextWidget heatedbed_temp_text;
    public bool aborted;
    public bool paused;
    public bool continued;
    private int previous_jobs_count;
    private string previous_job_image;
    private PopupMessageBox messagebox;
    private SettingsManager settings;
    private Form1 mainform;
    private bool showing_details;
    private bool bHasHeatedbed;
    private bool bSupportedFeaturesProcessed;
    private GUIHost m_host;

    public PrinterStatusDialog(Printer base_printer, GUIHost host, Element2D parent_element, PopupMessageBox messagebox, Form1 mainform, SettingsManager settings)
      : base(base_printer)
    {
      found = true;
      previous_jobs_count = 0;
      previous_job_image = "";
      base_printer.OnUpdateData += new OnUpdateDataDel(OnUpdateData);
      this.mainform = mainform;
      this.messagebox = messagebox;
      current_job = Info.current_job ?? null;
      this.settings = settings;
      InitGUIElement(host, parent_element);
      m_host = host;
    }

    public void OnDisconnect()
    {
      RefreshParent();
      status_dialog_frame.Visible = false;
      OnRemoval(status_dialog_frame.Parent);
    }

    private void RefreshParent()
    {
      if (status_dialog_frame == null || status_dialog_frame.Parent == null)
      {
        return;
      }

      status_dialog_frame.Parent.Refresh();
    }

    private string ExpandName(string str)
    {
      for (var startIndex = 1; startIndex < str.Length; ++startIndex)
      {
        if (char.IsUpper(str[startIndex]))
        {
          str = str.Insert(startIndex, " ");
          ++startIndex;
        }
      }
      return str;
    }

    public void OnUpdateData(PrinterInfo info)
    {
      if (info.supportedFeatures.UsesSupportedFeatures && !bSupportedFeaturesProcessed)
      {
        ProcessSupportedFeatures(info);
      }

      if (info.current_job != null && previous_jobs_count > 0)
      {
        if (info.current_job.PreviewImageFileName != previous_job_image)
        {
          DestroyPreviewImageWidget();
        }

        previous_job_image = info.current_job.PreviewImageFileName;
      }
      previous_jobs_count = info.current_job != null ? 1 : 0;
      if (Info.current_job != null)
      {
        current_job = Info.current_job;
        if (modelName_text != null)
        {
          modelName_text.Text = "Model Name: " + current_job.JobName;
        }

        var str1 = ExpandName(((FillQuality)current_job.Params.options.fill_density_DetailOnly).ToString());
        var resolutionDetailOnly = current_job.Params.options.quality_layer_resolution_DetailOnly;
        var str2 = ExpandName(((PrintQuality) resolutionDetailOnly).ToString()) + " - " + resolutionDetailOnly.ToString() + " microns";
        if (infill_text != null)
        {
          infill_text.Text = "Fill: " + str1;
        }

        if (quality_text != null)
        {
          quality_text.Text = "Quality: " + str2;
        }

        if (options_text != null)
        {
          options_text.Text = "-" + current_job.Params.filament_type.ToString() + "-   ";
          if (current_job.Params.options.use_raft_DetailOnly)
          {
            options_text.Text += "-Raft-   ";
          }

          if (current_job.Params.options.use_wave_bonding)
          {
            options_text.Text += "-Wave bonding-   ";
          }

          if (current_job.Params.options.use_support_DetailOnly)
          {
            options_text.Text += "-Support-   ";
          }
        }
        if (current_job.Params.autoprint)
        {
          more_info.Visible = false;
        }
        else
        {
          more_info.Visible = true;
        }
      }
      else
      {
        current_job = null;
        if (modelName_text != null)
        {
          modelName_text.Text = "Model Name: ";
        }

        if (infill_text != null)
        {
          infill_text.Text = "Fill: ";
        }

        if (quality_text != null)
        {
          quality_text.Text = "Quality: ";
        }

        if (options_text != null)
        {
          options_text.Text = "";
        }

        if (PrinterStatus.Firmware_PowerRecovery == Info.Status)
        {
          status_text.Text = "Recovering from\npower outage.";
          more_info.Visible = false;
          remain_text.Visible = false;
          abort_button.Visible = false;
          pause_print.Visible = false;
          continue_print.Visible = false;
          progress_indicator.Visible = true;
          if (!status_dialog_frame.Visible)
          {
            status_dialog_frame.Visible = true;
            RefreshParent();
          }
        }
      }
      if (HasJob && Info.current_job.Status != JobStatus.SavingToFile)
      {
        if (preview_image == null && !string.IsNullOrEmpty(current_job.PreviewImageFileName) && current_job.PreviewImageFileName != "null")
        {
          mainform.AddTask(new Form1.Task(CreatePreviewImageWidget), current_job.PreviewImageFileName);
        }

        var percentComplete = current_job.PercentComplete;
        var timeRemaining = current_job.TimeRemaining;
        var num1 = float.IsNaN(percentComplete) || percentComplete < -1.0 ? 0.0f : percentComplete * 100f;
        if (timeRemaining > 0.0)
        {
          var num2 = (int)(timeRemaining / 60.0);
          var num3 = num2 / 60;
          var num4 = num2 - num3 * 60;
          remain_text.Text = "Remaining: " + num3.ToString() + " hours, " + num4.ToString() + " minutes";
          remain_text.Visible = true;
        }
        else
        {
          remain_text.Visible = false;
        }

        if (!status_dialog_frame.Visible)
        {
          status_dialog_frame.Visible = true;
          RefreshParent();
        }
        if (quad_overlay != null)
        {
          if (current_job.Status == JobStatus.SavingToSD || current_job.Status == JobStatus.Buffering)
          {
            quad_overlay.Height = 80;
          }
          else
          {
            quad_overlay.Height = 80 - (int) (80.0 * current_job.PercentComplete);
          }

          quad_overlay.Visible = true;
        }
        if (!aborted)
        {
          status_text.Visible = true;
          progress_indicator.Visible = false;
          if (Info.Status == PrinterStatus.Firmware_IsWaitingToPause)
          {
            status_text.Text = "Pausing: Waiting to pause.";
            remain_text.Visible = false;
            pause_print.Visible = false;
          }
          else if (Info.Status == PrinterStatus.Firmware_PrintingPaused || Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing || current_job.Status == JobStatus.Paused)
          {
            status_text.Text = "Paused";
            continue_print.Visible = true;
          }
          else if (info.Status == PrinterStatus.Firmware_Calibrating)
          {
            status_text.Text = "Checking Height";
            remain_text.Visible = false;
            abort_button.Visible = false;
            pause_print.Visible = false;
            continue_print.Visible = false;
            progress_indicator.Visible = true;
          }
          else if (num1 == 0.0 && !aborted && Info.Status != PrinterStatus.Firmware_IsWaitingToPause)
          {
            status_text.Text = "Processing";
            remain_text.Visible = false;
            abort_button.Visible = false;
            pause_print.Visible = false;
            continue_print.Visible = false;
            progress_indicator.Visible = true;
          }
          else if (current_job.Status == JobStatus.Heating)
          {
            status_text.Text = "Warming up";
            remain_text.Visible = false;
            progress_indicator.Visible = true;
            continue_print.Visible = false;
          }
          else
          {
            status_text.Visible = true;
            continue_print.Visible = false;
            pause_print.Visible = true;
            abort_button.Visible = true;
            switch (current_job.Status)
            {
              case JobStatus.SavingToSD:
                status_text.Text = "Saving to printer: ";
                pause_print.Visible = false;
                break;
              case JobStatus.Buffering:
                status_text.Text = "Buffering to printer: ";
                pause_print.Visible = false;
                break;
              default:
                status_text.Text = "Printing: ";
                break;
            }
            status_text.Text = status_text.Text + num1.ToString("F2") + "%";
          }
        }
        else
        {
          remain_text.Visible = false;
          abort_button.Visible = false;
          pause_print.Visible = false;
          continue_print.Visible = false;
        }
      }
      else if (aborted)
      {
        pause_print.Visible = false;
        continue_print.Visible = false;
        if (info.Status == PrinterStatus.Firmware_Idle)
        {
          messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Finished aborting the print."));
          aborted = false;
        }
      }
      else
      {
        if (PrinterStatus.Firmware_PowerRecovery != Info.Status && status_dialog_frame.Visible)
        {
          status_dialog_frame.Visible = false;
          RefreshParent();
        }
        if (preview_image != null)
        {
          DestroyPreviewImageWidget();
        }
      }
      if (heater_temp_text != null)
      {
        heater_temp_text.Text = "Heater Temperature: " + (info.extruder.Temperature > 0.0 ? info.extruder.Temperature.ToString("0.0") + " C" : "OFF");
      }

      if (heatedbed_temp_text == null)
      {
        return;
      }

      heatedbed_temp_text.Text = "Heated Bed Temperature: " + (info.accessories.BedStatus.BedTemperature > 0.0 ? info.accessories.BedStatus.BedTemperature.ToString("0.0") + " C" : "OFF");
    }

    public void ProcessSupportedFeatures(PrinterInfo info)
    {
      if (MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
      {
        bHasHeatedbed = info.supportedFeatures.Available("Heated Bed Control", MyPrinterProfile.SupportedFeaturesConstants);
      }

      bSupportedFeaturesProcessed = true;
    }

    public JobInfo CurrentJob
    {
      get
      {
        return Info.current_job;
      }
    }

    public void OnRemoval(Element2D parent_element)
    {
      DestroyPreviewImageWidget();
      parent_element.RemoveChildElement(status_dialog_frame);
      preview_image = null;
    }

    public bool HasJob
    {
      get
      {
        return Info.current_job != null;
      }
    }

    private void InitGUIElement(GUIHost host, Element2D parent_element)
    {
      status_dialog_frame = new BorderedImageFrame(2340);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      status_dialog_frame.Init(host, "guicontrols", 768f, 384f, 895f, 511f, 14, 14, 64, 14, 14, 64);
      status_dialog_frame.SetSize(420, 160);
      status_dialog_frame.SetPosition(-400, 64);
      status_dialog_frame.Visible = false;
      Sprite.pixel_perfect = false;
      progress_indicator = new SpriteAnimationWidget(3);
      progress_indicator.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      progress_indicator.SetSize(100, 100);
      progress_indicator.SetPosition(-110, 10);
      progress_indicator.Visible = false;
      status_dialog_frame.AddChildElement(progress_indicator);
      printer_name_text = new TextWidget(2);
      printer_name_text.SetPosition(140, 10);
      printer_name_text.SetSize(250, 30);
      printer_name_text.Text = Info.serial_number.ToString();
      printer_name_text.Size = FontSize.Medium;
      printer_name_text.Alignment = QFontAlignment.Left;
      printer_name_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      status_dialog_frame.AddChildElement(printer_name_text);
      status_text = new TextWidget(3);
      status_text.SetPosition(140, 40);
      status_text.SetSize(250, 30);
      status_text.Text = "Please Wait...";
      status_text.Size = FontSize.Medium;
      status_text.Alignment = QFontAlignment.Left;
      status_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      status_dialog_frame.AddChildElement(status_text);
      remain_text = new TextWidget(4);
      remain_text.SetPosition(140, 70);
      remain_text.SetSize(350, 30);
      remain_text.Text = "Remaining: ...Calculating...";
      remain_text.Size = FontSize.Medium;
      remain_text.Alignment = QFontAlignment.Left;
      remain_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      status_dialog_frame.AddChildElement(remain_text);
      abort_button = new ButtonWidget(0);
      abort_button.Init(host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      abort_button.Size = FontSize.Medium;
      abort_button.Text = "Abort";
      abort_button.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      abort_button.SetGrowableWidth(4, 4, 32);
      abort_button.SetGrowableHeight(4, 4, 32);
      abort_button.SetSize(90, 32);
      abort_button.SetPosition(140, 110);
      abort_button.SetCallback(new ButtonCallback(MyButtonCallback));
      status_dialog_frame.AddChildElement(abort_button);
      pause_print = new ButtonWidget(1);
      pause_print.Init(host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      pause_print.Size = FontSize.Medium;
      pause_print.Text = "Pause";
      pause_print.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      pause_print.SetGrowableWidth(4, 4, 32);
      pause_print.SetGrowableHeight(4, 4, 32);
      pause_print.SetSize(90, 32);
      pause_print.SetPosition(250, 110);
      pause_print.SetCallback(new ButtonCallback(MyButtonCallback));
      pause_print.Enabled = true;
      status_dialog_frame.AddChildElement(pause_print);
      continue_print = new ButtonWidget(2);
      continue_print.Init(host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      continue_print.Size = FontSize.Medium;
      continue_print.Text = "Continue";
      continue_print.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      continue_print.SetGrowableWidth(4, 4, 32);
      continue_print.SetGrowableHeight(4, 4, 32);
      continue_print.SetSize(100, 32);
      continue_print.SetPosition(250, 110);
      continue_print.SetCallback(new ButtonCallback(MyButtonCallback));
      continue_print.Enabled = true;
      status_dialog_frame.AddChildElement(continue_print);
      modelName_text = new TextWidget();
      modelName_text.SetPosition(10, 150);
      modelName_text.SetSize(380, 30);
      modelName_text.Text = "modelName_text";
      modelName_text.Size = FontSize.Medium;
      modelName_text.Alignment = QFontAlignment.Left;
      modelName_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      modelName_text.Visible = false;
      status_dialog_frame.AddChildElement(modelName_text);
      quality_text = new TextWidget();
      quality_text.SetPosition(10, 180);
      quality_text.SetSize(380, 30);
      quality_text.Text = "quality_text";
      quality_text.Size = FontSize.Medium;
      quality_text.Alignment = QFontAlignment.Left;
      quality_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      quality_text.Visible = false;
      status_dialog_frame.AddChildElement(quality_text);
      infill_text = new TextWidget();
      infill_text.SetPosition(10, 210);
      infill_text.SetSize(380, 30);
      infill_text.Text = "infill_text";
      infill_text.Size = FontSize.Medium;
      infill_text.Alignment = QFontAlignment.Left;
      infill_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      infill_text.Visible = false;
      status_dialog_frame.AddChildElement(infill_text);
      options_text = new TextWidget();
      options_text.SetPosition(10, 240);
      options_text.SetSize(380, 30);
      options_text.Text = "";
      options_text.Size = FontSize.Medium;
      options_text.Alignment = QFontAlignment.Left;
      options_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      options_text.Visible = false;
      status_dialog_frame.AddChildElement(options_text);
      heater_temp_text = new TextWidget();
      heater_temp_text.SetPosition(10, 270);
      heater_temp_text.SetSize(380, 30);
      heater_temp_text.Text = "Heater Temperature:";
      heater_temp_text.Size = FontSize.Medium;
      heater_temp_text.Alignment = QFontAlignment.Left;
      heater_temp_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      heater_temp_text.Visible = false;
      status_dialog_frame.AddChildElement(heater_temp_text);
      heatedbed_temp_text = new TextWidget();
      heatedbed_temp_text.SetPosition(10, 300);
      heatedbed_temp_text.SetSize(380, 30);
      heatedbed_temp_text.Text = "Heated Bed Temperature:";
      heatedbed_temp_text.Size = FontSize.Medium;
      heatedbed_temp_text.Alignment = QFontAlignment.Left;
      heatedbed_temp_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      heatedbed_temp_text.Visible = false;
      status_dialog_frame.AddChildElement(heatedbed_temp_text);
      more_info = new ButtonWidget(3)
      {
        Text = "",
        Size = FontSize.Medium
      };
      more_info.SetPosition(355, 112);
      more_info.SetSize(30, 30);
      more_info.SetCallback(new ButtonCallback(MyButtonCallback));
      more_info.Enabled = true;
      more_info.Init(host, "guicontrols", 448f, 512f, 511f, 575f, 512f, 512f, 575f, 575f, 576f, 512f, 639f, 575f);
      more_info.ToolTipMessage = host.Locale.T("T_TOOLTIP_INFORMATION");
      status_dialog_frame.AddChildElement(more_info);
      parent_element.AddChildElement(status_dialog_frame);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          if (messagebox == null)
          {
            break;
          }

          messagebox.AddMessageToQueue("Are you sure you want to abort this print?", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(AbortCallBack));
          break;
        case 1:
          if (messagebox == null)
          {
            break;
          }

          messagebox.AddMessageToQueue("Are you sure you want to pause?\r\n\r\nA print can be paused 5 minutes\r\nafter the start of the print.", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(PauseCallBack));
          break;
        case 2:
          if (GetCurrentFilament() == null)
          {
            messagebox.AddMessageToQueue("Please load 3D ink into your printer before resuming the print.");
            break;
          }
          if (settings.ShowAllWarnings && messagebox != null)
          {
            messagebox.AddMessageToQueue("Are you sure you want to continue this print?", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(ContinueCallBack));
            break;
          }
          ContinuePrint();
          break;
        case 3:
          showing_details = !showing_details;
          if (showing_details)
          {
            if (bHasHeatedbed)
            {
              heatedbed_temp_text.Visible = true;
              status_dialog_frame.Height = 340;
            }
            else
            {
              status_dialog_frame.Height = 310;
            }

            modelName_text.Visible = true;
            infill_text.Visible = true;
            quality_text.Visible = true;
            options_text.Visible = true;
            heater_temp_text.Visible = true;
          }
          else
          {
            status_dialog_frame.Height = 160;
            modelName_text.Visible = false;
            infill_text.Visible = false;
            quality_text.Visible = false;
            options_text.Visible = false;
            heater_temp_text.Visible = false;
            heatedbed_temp_text.Visible = false;
          }
          RefreshParent();
          break;
      }
    }

    private void AbortCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
      {
        return;
      }

      if (settings.ShowAllWarnings)
      {
        messagebox.AddMessageToQueue("Aborting print. Please do not unplug your printer while it is aborting. If you unplug your printer, you will have to recalibrate it.");
      }

      var num = (int)AbortPrint(null, null);
      aborted = true;
      status_text.Visible = false;
      remain_text.Visible = false;
      progress_indicator.Visible = true;
      abort_button.Visible = false;
    }

    private void PauseCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
      {
        return;
      }

      var num = (int)PausePrint();
    }

    public SpoolerResult PausePrint()
    {
      if (settings.ShowAllWarnings)
      {
        messagebox.AddMessageToQueue("Please do not unplug your printer while it is Moving. If you unplug your printer, you will have to recalibrate it.");
      }

      var num = (int)PausePrint(new AsyncCallback(ShowLockError), base_obj);
      paused = true;
      continued = false;
      pause_print.Visible = false;
      return (SpoolerResult) num;
    }

    private void ContinueCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
      {
        return;
      }

      ContinuePrint();
    }

    public void ContinuePrint()
    {
      var num = (int)AcquireLock(new AsyncCallback(DoPrinterTask), new PrinterStatusDialog.PrinterTaskData((IPrinter)this, PrinterStatusDialog.PrinterTask.ContinuePrint));
      continued = true;
      paused = false;
      pause_print.Visible = true;
      continue_print.Visible = false;
    }

    private void CreatePreviewImageWidget(object data)
    {
      var texture = (string) data;
      preview_image = new ImageWidget(0);
      if (!preview_image.Init(m_host, texture))
      {
        preview_image = null;
      }
      else
      {
        if (preview_image == null)
        {
          return;
        }

        preview_image.SetSize(120, 80);
        preview_image.SetPosition(10, 10);
        status_dialog_frame.AddChildElement(preview_image);
        quad_overlay = new QuadWidget(1);
        quad_overlay.SetSize(120, 80);
        quad_overlay.SetPosition(10, 10);
        quad_overlay.Color = new Color4(0.99f, 0.54f, 0.35f, 0.75f);
        quad_overlay.Visible = false;
        status_dialog_frame.AddChildElement(quad_overlay);
        status_dialog_frame.X = status_dialog_frame.X;
      }
    }

    private void DestroyPreviewImageWidget()
    {
      if (preview_image != null)
      {
        status_dialog_frame.RemoveChildElement(preview_image);
        preview_image = null;
      }
      if (quad_overlay == null)
      {
        return;
      }

      status_dialog_frame.RemoveChildElement(quad_overlay);
      quad_overlay = null;
    }

    public bool ValidZ
    {
      get
      {
        return Info.extruder.Z_Valid;
      }
    }

    private void DoPrinterTask(IAsyncCallResult ar)
    {
      var lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (!string.IsNullOrEmpty(lockErrorMessage))
      {
        messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
      }
      else
      {
        var asyncState = (PrinterStatusDialog.PrinterTaskData) ar.AsyncState;
        switch (asyncState.task)
        {
          case PrinterStatusDialog.PrinterTask.ContinuePrint:
            var num1 = (int) asyncState.printer.ContinuePrint(new AsyncCallback(ShowLockError), asyncState.printer);
            break;
          case PrinterStatusDialog.PrinterTask.ReleasePrinter:
            var num2 = (int) asyncState.printer.ReleaseLock(new AsyncCallback(ShowLockError), asyncState.printer);
            break;
        }
      }
    }

    public void ShowLockError(IAsyncCallResult ar)
    {
      var lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
      {
        return;
      }

      messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
    }

    private enum PrintInfoControlID
    {
      AbortButton,
      PauseButton,
      ContinueButton,
      MoreInfo,
    }

    private enum PrinterTask
    {
      ContinuePrint,
      ReleasePrinter,
    }

    private struct PrinterTaskData
    {
      public IPrinter printer;
      public PrinterStatusDialog.PrinterTask task;

      public PrinterTaskData(IPrinter printer, PrinterStatusDialog.PrinterTask task)
      {
        this.printer = printer;
        this.task = task;
      }
    }
  }
}
