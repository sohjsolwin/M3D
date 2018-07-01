// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.PrinterStatusDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      : base((IPrinter) base_printer)
    {
      this.found = true;
      this.previous_jobs_count = 0;
      this.previous_job_image = "";
      base_printer.OnUpdateData += new OnUpdateDataDel(this.OnUpdateData);
      this.mainform = mainform;
      this.messagebox = messagebox;
      this.current_job = this.Info.current_job == null ? (JobInfo) null : this.Info.current_job;
      this.settings = settings;
      this.InitGUIElement(host, parent_element);
      this.m_host = host;
    }

    public void OnDisconnect()
    {
      this.RefreshParent();
      this.status_dialog_frame.Visible = false;
      this.OnRemoval(this.status_dialog_frame.Parent);
    }

    private void RefreshParent()
    {
      if (this.status_dialog_frame == null || this.status_dialog_frame.Parent == null)
        return;
      this.status_dialog_frame.Parent.Refresh();
    }

    private string ExpandName(string str)
    {
      for (int startIndex = 1; startIndex < str.Length; ++startIndex)
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
      if (info.supportedFeatures.UsesSupportedFeatures && !this.bSupportedFeaturesProcessed)
        this.ProcessSupportedFeatures(info);
      if (info.current_job != null && this.previous_jobs_count > 0)
      {
        if (info.current_job.PreviewImageFileName != this.previous_job_image)
          this.DestroyPreviewImageWidget();
        this.previous_job_image = info.current_job.PreviewImageFileName;
      }
      this.previous_jobs_count = info.current_job != null ? 1 : 0;
      if (this.Info.current_job != null)
      {
        this.current_job = this.Info.current_job;
        if (this.modelName_text != null)
          this.modelName_text.Text = "Model Name: " + this.current_job.JobName;
        string str1 = this.ExpandName(((FillQuality) this.current_job.Params.options.fill_density_DetailOnly).ToString());
        int resolutionDetailOnly = this.current_job.Params.options.quality_layer_resolution_DetailOnly;
        string str2 = this.ExpandName(((PrintQuality) resolutionDetailOnly).ToString()) + " - " + resolutionDetailOnly.ToString() + " microns";
        if (this.infill_text != null)
          this.infill_text.Text = "Fill: " + str1;
        if (this.quality_text != null)
          this.quality_text.Text = "Quality: " + str2;
        if (this.options_text != null)
        {
          this.options_text.Text = "-" + this.current_job.Params.filament_type.ToString() + "-   ";
          if (this.current_job.Params.options.use_raft_DetailOnly)
            this.options_text.Text += "-Raft-   ";
          if (this.current_job.Params.options.use_wave_bonding)
            this.options_text.Text += "-Wave bonding-   ";
          if (this.current_job.Params.options.use_support_DetailOnly)
            this.options_text.Text += "-Support-   ";
        }
        if (this.current_job.Params.autoprint)
          this.more_info.Visible = false;
        else
          this.more_info.Visible = true;
      }
      else
      {
        this.current_job = (JobInfo) null;
        if (this.modelName_text != null)
          this.modelName_text.Text = "Model Name: ";
        if (this.infill_text != null)
          this.infill_text.Text = "Fill: ";
        if (this.quality_text != null)
          this.quality_text.Text = "Quality: ";
        if (this.options_text != null)
          this.options_text.Text = "";
        if (PrinterStatus.Firmware_PowerRecovery == this.Info.Status)
        {
          this.status_text.Text = "Recovering from\npower outage.";
          this.more_info.Visible = false;
          this.remain_text.Visible = false;
          this.abort_button.Visible = false;
          this.pause_print.Visible = false;
          this.continue_print.Visible = false;
          this.progress_indicator.Visible = true;
          if (!this.status_dialog_frame.Visible)
          {
            this.status_dialog_frame.Visible = true;
            this.RefreshParent();
          }
        }
      }
      if (this.HasJob && this.Info.current_job.Status != JobStatus.SavingToFile)
      {
        if (this.preview_image == null && !string.IsNullOrEmpty(this.current_job.PreviewImageFileName) && this.current_job.PreviewImageFileName != "null")
          this.mainform.AddTask(new Form1.Task(this.CreatePreviewImageWidget), (object) this.current_job.PreviewImageFileName);
        float percentComplete = this.current_job.PercentComplete;
        float timeRemaining = this.current_job.TimeRemaining;
        float num1 = float.IsNaN(percentComplete) || (double) percentComplete < -1.0 ? 0.0f : percentComplete * 100f;
        if ((double) timeRemaining > 0.0)
        {
          int num2 = (int) ((double) timeRemaining / 60.0);
          int num3 = num2 / 60;
          int num4 = num2 - num3 * 60;
          this.remain_text.Text = "Remaining: " + num3.ToString() + " hours, " + num4.ToString() + " minutes";
          this.remain_text.Visible = true;
        }
        else
          this.remain_text.Visible = false;
        if (!this.status_dialog_frame.Visible)
        {
          this.status_dialog_frame.Visible = true;
          this.RefreshParent();
        }
        if (this.quad_overlay != null)
        {
          if (this.current_job.Status == JobStatus.SavingToSD || this.current_job.Status == JobStatus.Buffering)
            this.quad_overlay.Height = 80;
          else
            this.quad_overlay.Height = 80 - (int) (80.0 * (double) this.current_job.PercentComplete);
          this.quad_overlay.Visible = true;
        }
        if (!this.aborted)
        {
          this.status_text.Visible = true;
          this.progress_indicator.Visible = false;
          if (this.Info.Status == PrinterStatus.Firmware_IsWaitingToPause)
          {
            this.status_text.Text = "Pausing: Waiting to pause.";
            this.remain_text.Visible = false;
            this.pause_print.Visible = false;
          }
          else if (this.Info.Status == PrinterStatus.Firmware_PrintingPaused || this.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing || this.current_job.Status == JobStatus.Paused)
          {
            this.status_text.Text = "Paused";
            this.continue_print.Visible = true;
          }
          else if (info.Status == PrinterStatus.Firmware_Calibrating)
          {
            this.status_text.Text = "Checking Height";
            this.remain_text.Visible = false;
            this.abort_button.Visible = false;
            this.pause_print.Visible = false;
            this.continue_print.Visible = false;
            this.progress_indicator.Visible = true;
          }
          else if ((double) num1 == 0.0 && !this.aborted && this.Info.Status != PrinterStatus.Firmware_IsWaitingToPause)
          {
            this.status_text.Text = "Processing";
            this.remain_text.Visible = false;
            this.abort_button.Visible = false;
            this.pause_print.Visible = false;
            this.continue_print.Visible = false;
            this.progress_indicator.Visible = true;
          }
          else if (this.current_job.Status == JobStatus.Heating)
          {
            this.status_text.Text = "Warming up";
            this.remain_text.Visible = false;
            this.progress_indicator.Visible = true;
            this.continue_print.Visible = false;
          }
          else
          {
            this.status_text.Visible = true;
            this.continue_print.Visible = false;
            this.pause_print.Visible = true;
            this.abort_button.Visible = true;
            switch (this.current_job.Status)
            {
              case JobStatus.SavingToSD:
                this.status_text.Text = "Saving to printer: ";
                this.pause_print.Visible = false;
                break;
              case JobStatus.Buffering:
                this.status_text.Text = "Buffering to printer: ";
                this.pause_print.Visible = false;
                break;
              default:
                this.status_text.Text = "Printing: ";
                break;
            }
            this.status_text.Text = this.status_text.Text + num1.ToString("F2") + "%";
          }
        }
        else
        {
          this.remain_text.Visible = false;
          this.abort_button.Visible = false;
          this.pause_print.Visible = false;
          this.continue_print.Visible = false;
        }
      }
      else if (this.aborted)
      {
        this.pause_print.Visible = false;
        this.continue_print.Visible = false;
        if (info.Status == PrinterStatus.Firmware_Idle)
        {
          this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Finished aborting the print."));
          this.aborted = false;
        }
      }
      else
      {
        if (PrinterStatus.Firmware_PowerRecovery != this.Info.Status && this.status_dialog_frame.Visible)
        {
          this.status_dialog_frame.Visible = false;
          this.RefreshParent();
        }
        if (this.preview_image != null)
          this.DestroyPreviewImageWidget();
      }
      if (this.heater_temp_text != null)
        this.heater_temp_text.Text = "Heater Temperature: " + ((double) info.extruder.Temperature > 0.0 ? info.extruder.Temperature.ToString("0.0") + " C" : "OFF");
      if (this.heatedbed_temp_text == null)
        return;
      this.heatedbed_temp_text.Text = "Heated Bed Temperature: " + ((double) info.accessories.BedStatus.BedTemperature > 0.0 ? info.accessories.BedStatus.BedTemperature.ToString("0.0") + " C" : "OFF");
    }

    public void ProcessSupportedFeatures(PrinterInfo info)
    {
      if (this.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed)
        this.bHasHeatedbed = info.supportedFeatures.Available("Heated Bed Control", this.MyPrinterProfile.SupportedFeaturesConstants);
      this.bSupportedFeaturesProcessed = true;
    }

    public JobInfo CurrentJob
    {
      get
      {
        return this.Info.current_job;
      }
    }

    public void OnRemoval(Element2D parent_element)
    {
      this.DestroyPreviewImageWidget();
      parent_element.RemoveChildElement((Element2D) this.status_dialog_frame);
      this.preview_image = (ImageWidget) null;
    }

    public bool HasJob
    {
      get
      {
        return this.Info.current_job != null;
      }
    }

    private void InitGUIElement(GUIHost host, Element2D parent_element)
    {
      this.status_dialog_frame = new BorderedImageFrame(2340);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      this.status_dialog_frame.Init(host, "guicontrols", 768f, 384f, 895f, 511f, 14, 14, 64, 14, 14, 64);
      this.status_dialog_frame.SetSize(420, 160);
      this.status_dialog_frame.SetPosition(-400, 64);
      this.status_dialog_frame.Visible = false;
      Sprite.pixel_perfect = false;
      this.progress_indicator = new SpriteAnimationWidget(3);
      this.progress_indicator.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      this.progress_indicator.SetSize(100, 100);
      this.progress_indicator.SetPosition(-110, 10);
      this.progress_indicator.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.progress_indicator);
      this.printer_name_text = new TextWidget(2);
      this.printer_name_text.SetPosition(140, 10);
      this.printer_name_text.SetSize(250, 30);
      this.printer_name_text.Text = this.Info.serial_number.ToString();
      this.printer_name_text.Size = FontSize.Medium;
      this.printer_name_text.Alignment = QFontAlignment.Left;
      this.printer_name_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.status_dialog_frame.AddChildElement((Element2D) this.printer_name_text);
      this.status_text = new TextWidget(3);
      this.status_text.SetPosition(140, 40);
      this.status_text.SetSize(250, 30);
      this.status_text.Text = "Please Wait...";
      this.status_text.Size = FontSize.Medium;
      this.status_text.Alignment = QFontAlignment.Left;
      this.status_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.status_dialog_frame.AddChildElement((Element2D) this.status_text);
      this.remain_text = new TextWidget(4);
      this.remain_text.SetPosition(140, 70);
      this.remain_text.SetSize(350, 30);
      this.remain_text.Text = "Remaining: ...Calculating...";
      this.remain_text.Size = FontSize.Medium;
      this.remain_text.Alignment = QFontAlignment.Left;
      this.remain_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.status_dialog_frame.AddChildElement((Element2D) this.remain_text);
      this.abort_button = new ButtonWidget(0);
      this.abort_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.abort_button.Size = FontSize.Medium;
      this.abort_button.Text = "Abort";
      this.abort_button.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      this.abort_button.SetGrowableWidth(4, 4, 32);
      this.abort_button.SetGrowableHeight(4, 4, 32);
      this.abort_button.SetSize(90, 32);
      this.abort_button.SetPosition(140, 110);
      this.abort_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.status_dialog_frame.AddChildElement((Element2D) this.abort_button);
      this.pause_print = new ButtonWidget(1);
      this.pause_print.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.pause_print.Size = FontSize.Medium;
      this.pause_print.Text = "Pause";
      this.pause_print.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      this.pause_print.SetGrowableWidth(4, 4, 32);
      this.pause_print.SetGrowableHeight(4, 4, 32);
      this.pause_print.SetSize(90, 32);
      this.pause_print.SetPosition(250, 110);
      this.pause_print.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.pause_print.Enabled = true;
      this.status_dialog_frame.AddChildElement((Element2D) this.pause_print);
      this.continue_print = new ButtonWidget(2);
      this.continue_print.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.continue_print.Size = FontSize.Medium;
      this.continue_print.Text = "Continue";
      this.continue_print.TextColor = new Color4(0.95f, 0.5f, 0.0f, 1f);
      this.continue_print.SetGrowableWidth(4, 4, 32);
      this.continue_print.SetGrowableHeight(4, 4, 32);
      this.continue_print.SetSize(100, 32);
      this.continue_print.SetPosition(250, 110);
      this.continue_print.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.continue_print.Enabled = true;
      this.status_dialog_frame.AddChildElement((Element2D) this.continue_print);
      this.modelName_text = new TextWidget();
      this.modelName_text.SetPosition(10, 150);
      this.modelName_text.SetSize(380, 30);
      this.modelName_text.Text = "modelName_text";
      this.modelName_text.Size = FontSize.Medium;
      this.modelName_text.Alignment = QFontAlignment.Left;
      this.modelName_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.modelName_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.modelName_text);
      this.quality_text = new TextWidget();
      this.quality_text.SetPosition(10, 180);
      this.quality_text.SetSize(380, 30);
      this.quality_text.Text = "quality_text";
      this.quality_text.Size = FontSize.Medium;
      this.quality_text.Alignment = QFontAlignment.Left;
      this.quality_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.quality_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.quality_text);
      this.infill_text = new TextWidget();
      this.infill_text.SetPosition(10, 210);
      this.infill_text.SetSize(380, 30);
      this.infill_text.Text = "infill_text";
      this.infill_text.Size = FontSize.Medium;
      this.infill_text.Alignment = QFontAlignment.Left;
      this.infill_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.infill_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.infill_text);
      this.options_text = new TextWidget();
      this.options_text.SetPosition(10, 240);
      this.options_text.SetSize(380, 30);
      this.options_text.Text = "";
      this.options_text.Size = FontSize.Medium;
      this.options_text.Alignment = QFontAlignment.Left;
      this.options_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.options_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.options_text);
      this.heater_temp_text = new TextWidget();
      this.heater_temp_text.SetPosition(10, 270);
      this.heater_temp_text.SetSize(380, 30);
      this.heater_temp_text.Text = "Heater Temperature:";
      this.heater_temp_text.Size = FontSize.Medium;
      this.heater_temp_text.Alignment = QFontAlignment.Left;
      this.heater_temp_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.heater_temp_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.heater_temp_text);
      this.heatedbed_temp_text = new TextWidget();
      this.heatedbed_temp_text.SetPosition(10, 300);
      this.heatedbed_temp_text.SetSize(380, 30);
      this.heatedbed_temp_text.Text = "Heated Bed Temperature:";
      this.heatedbed_temp_text.Size = FontSize.Medium;
      this.heatedbed_temp_text.Alignment = QFontAlignment.Left;
      this.heatedbed_temp_text.Color = new Color4(0.25f, 0.25f, 0.25f, 1f);
      this.heatedbed_temp_text.Visible = false;
      this.status_dialog_frame.AddChildElement((Element2D) this.heatedbed_temp_text);
      this.more_info = new ButtonWidget(3);
      this.more_info.Text = "";
      this.more_info.Size = FontSize.Medium;
      this.more_info.SetPosition(355, 112);
      this.more_info.SetSize(30, 30);
      this.more_info.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.more_info.Enabled = true;
      this.more_info.Init(host, "guicontrols", 448f, 512f, 511f, 575f, 512f, 512f, 575f, 575f, 576f, 512f, 639f, 575f);
      this.more_info.ToolTipMessage = host.Locale.T("T_TOOLTIP_INFORMATION");
      this.status_dialog_frame.AddChildElement((Element2D) this.more_info);
      parent_element.AddChildElement((Element2D) this.status_dialog_frame);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          if (this.messagebox == null)
            break;
          this.messagebox.AddMessageToQueue("Are you sure you want to abort this print?", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.AbortCallBack));
          break;
        case 1:
          if (this.messagebox == null)
            break;
          this.messagebox.AddMessageToQueue("Are you sure you want to pause?\r\n\r\nA print can be paused 5 minutes\r\nafter the start of the print.", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.PauseCallBack));
          break;
        case 2:
          if (this.GetCurrentFilament() == (FilamentSpool) null)
          {
            this.messagebox.AddMessageToQueue("Please load 3D ink into your printer before resuming the print.");
            break;
          }
          if (this.settings.ShowAllWarnings && this.messagebox != null)
          {
            this.messagebox.AddMessageToQueue("Are you sure you want to continue this print?", PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.ContinueCallBack));
            break;
          }
          this.ContinuePrint();
          break;
        case 3:
          this.showing_details = !this.showing_details;
          if (this.showing_details)
          {
            if (this.bHasHeatedbed)
            {
              this.heatedbed_temp_text.Visible = true;
              this.status_dialog_frame.Height = 340;
            }
            else
              this.status_dialog_frame.Height = 310;
            this.modelName_text.Visible = true;
            this.infill_text.Visible = true;
            this.quality_text.Visible = true;
            this.options_text.Visible = true;
            this.heater_temp_text.Visible = true;
          }
          else
          {
            this.status_dialog_frame.Height = 160;
            this.modelName_text.Visible = false;
            this.infill_text.Visible = false;
            this.quality_text.Visible = false;
            this.options_text.Visible = false;
            this.heater_temp_text.Visible = false;
            this.heatedbed_temp_text.Visible = false;
          }
          this.RefreshParent();
          break;
      }
    }

    private void AbortCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
        return;
      if (this.settings.ShowAllWarnings)
        this.messagebox.AddMessageToQueue("Aborting print. Please do not unplug your printer while it is aborting. If you unplug your printer, you will have to recalibrate it.");
      int num = (int) this.AbortPrint((AsyncCallback) null, (object) null);
      this.aborted = true;
      this.status_text.Visible = false;
      this.remain_text.Visible = false;
      this.progress_indicator.Visible = true;
      this.abort_button.Visible = false;
    }

    private void PauseCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
        return;
      int num = (int) this.PausePrint();
    }

    public SpoolerResult PausePrint()
    {
      if (this.settings.ShowAllWarnings)
        this.messagebox.AddMessageToQueue("Please do not unplug your printer while it is Moving. If you unplug your printer, you will have to recalibrate it.");
      int num = (int) this.PausePrint(new AsyncCallback(this.ShowLockError), (object) this.base_obj);
      this.paused = true;
      this.continued = false;
      this.pause_print.Visible = false;
      return (SpoolerResult) num;
    }

    private void ContinueCallBack(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
        return;
      this.ContinuePrint();
    }

    public void ContinuePrint()
    {
      int num = (int) this.AcquireLock(new AsyncCallback(this.DoPrinterTask), (object) new PrinterStatusDialog.PrinterTaskData((IPrinter) this, PrinterStatusDialog.PrinterTask.ContinuePrint));
      this.continued = true;
      this.paused = false;
      this.pause_print.Visible = true;
      this.continue_print.Visible = false;
    }

    private void CreatePreviewImageWidget(object data)
    {
      string texture = (string) data;
      this.preview_image = new ImageWidget(0);
      if (!this.preview_image.Init(this.m_host, texture))
      {
        this.preview_image = (ImageWidget) null;
      }
      else
      {
        if (this.preview_image == null)
          return;
        this.preview_image.SetSize(120, 80);
        this.preview_image.SetPosition(10, 10);
        this.status_dialog_frame.AddChildElement((Element2D) this.preview_image);
        this.quad_overlay = new QuadWidget(1);
        this.quad_overlay.SetSize(120, 80);
        this.quad_overlay.SetPosition(10, 10);
        this.quad_overlay.Color = new Color4(0.99f, 0.54f, 0.35f, 0.75f);
        this.quad_overlay.Visible = false;
        this.status_dialog_frame.AddChildElement((Element2D) this.quad_overlay);
        this.status_dialog_frame.X = this.status_dialog_frame.X;
      }
    }

    private void DestroyPreviewImageWidget()
    {
      if (this.preview_image != null)
      {
        this.status_dialog_frame.RemoveChildElement((Element2D) this.preview_image);
        this.preview_image = (ImageWidget) null;
      }
      if (this.quad_overlay == null)
        return;
      this.status_dialog_frame.RemoveChildElement((Element2D) this.quad_overlay);
      this.quad_overlay = (QuadWidget) null;
    }

    public bool ValidZ
    {
      get
      {
        return this.Info.extruder.Z_Valid;
      }
    }

    private void DoPrinterTask(IAsyncCallResult ar)
    {
      string lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (!string.IsNullOrEmpty(lockErrorMessage))
      {
        this.messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
      }
      else
      {
        PrinterStatusDialog.PrinterTaskData asyncState = (PrinterStatusDialog.PrinterTaskData) ar.AsyncState;
        switch (asyncState.task)
        {
          case PrinterStatusDialog.PrinterTask.ContinuePrint:
            int num1 = (int) asyncState.printer.ContinuePrint(new AsyncCallback(this.ShowLockError), (object) asyncState.printer);
            break;
          case PrinterStatusDialog.PrinterTask.ReleasePrinter:
            int num2 = (int) asyncState.printer.ReleaseLock(new AsyncCallback(this.ShowLockError), (object) asyncState.printer);
            break;
        }
      }
    }

    public void ShowLockError(IAsyncCallResult ar)
    {
      string lockErrorMessage = PrinterObject.GetLockErrorMessage(ar);
      if (string.IsNullOrEmpty(lockErrorMessage))
        return;
      this.messagebox.AddMessageToQueue(lockErrorMessage, PopupMessageBox.MessageBoxButtons.OK);
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
