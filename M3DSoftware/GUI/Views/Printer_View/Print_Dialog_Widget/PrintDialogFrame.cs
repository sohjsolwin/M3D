// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Print_Dialog_Widget.PrintDialogFrame
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
using M3D.Properties;
using M3D.Slicer.General;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System.Collections.Generic;
using System.Linq;

namespace M3D.GUI.Views.Printer_View.Print_Dialog_Widget
{
  internal class PrintDialogFrame : IPrintDialogFrame
  {
    private GUIHost host;
    private PrintJobDetails CurrentJobDetails;
    private Dictionary<PrintQuality, ButtonWidget> mPrintQualityButtons;
    private Dictionary<FillQuality, ButtonWidget> mFillDensityButtons;
    private bool mQualityButtonsSet;
    private PopupMessageBox message_box;
    private PrinterView printerview;
    private ModelLoadingManager modelloadingmanager;
    private SpoolerConnection spooler_connection;
    private SettingsManager settingsManager;
    private ButtonWidget print_button;
    private HorizontalLayoutScrollList quality_scroll_list;
    private HorizontalLayoutScrollList density_scroll_list;
    private ButtonWidget printQualityPrev_button;
    private ButtonWidget printQualityNext_button;
    private ButtonWidget fillDensityPrev_button;
    private ButtonWidget fillDensityNext_button;
    private ButtonWidget support_checkbutton;
    private ButtonWidget support_everywhere;
    private ButtonWidget support_printbedonly;
    private ButtonWidget UseWaveBonding;
    private ButtonWidget raft_checkbutton;
    private ButtonWidget verifybed_checkbutton;
    private TextWidget verifybed_text;
    private ButtonWidget enableskirt_checkbutton;
    private ButtonWidget heatedBedButton_checkbox;
    private TextWidget heatedBedButton_text;
    private XMLFrame sdCheckboxesFrame;
    private ButtonWidget untetheredButton_checkbox;
    private ButtonWidget sdOnlyButton_checkbox;
    private TextWidget sdOnlyButton_text;
    private EditBoxWidget printQuality_editbox;
    private EditBoxWidget fillDensity_editbox;
    private PrinterObject mPrevPrinter;
    private bool m_bNewPrinterSelected;
    private bool syncing;

    public PrintDialogFrame(int ID, GUIHost host, PrinterView printerview, SpoolerConnection spooler_connection, PopupMessageBox message_box, ModelLoadingManager modelloadingmanager, SettingsManager settings, PrintDialogMainWindow printDialogWindow)
      : base(ID, printDialogWindow)
    {
      this.modelloadingmanager = modelloadingmanager;
      this.message_box = message_box;
      this.spooler_connection = spooler_connection;
      this.printerview = printerview;
      this.settingsManager = settings;
      this.host = host;
      this.CenterHorizontallyInParent = true;
      this.CenterVerticallyInParent = true;
      this.SetSize(750, 550);
      string printdialog = Resources.printdialog;
      XMLFrame xmlFrame = new XMLFrame(ID);
      xmlFrame.RelativeWidth = 1f;
      xmlFrame.RelativeHeight = 1f;
      this.AddChildElement((Element2D) xmlFrame);
      xmlFrame.Init(host, printdialog, new ButtonCallback(this.MyButtonCallback));
      this.mPrintQualityButtons = new Dictionary<PrintQuality, ButtonWidget>();
      this.mPrintQualityButtons.Add(PrintQuality.Expert, (ButtonWidget) this.FindChildElement(111));
      this.mPrintQualityButtons.Add(PrintQuality.VeryHighQuality, (ButtonWidget) this.FindChildElement(116));
      this.mPrintQualityButtons.Add(PrintQuality.HighQuality, (ButtonWidget) this.FindChildElement(112));
      this.mPrintQualityButtons.Add(PrintQuality.MediumQuality, (ButtonWidget) this.FindChildElement(113));
      this.mPrintQualityButtons.Add(PrintQuality.FastPrint, (ButtonWidget) this.FindChildElement(114));
      this.mPrintQualityButtons.Add(PrintQuality.VeryFastPrint, (ButtonWidget) this.FindChildElement(115));
      this.mPrintQualityButtons.Add(PrintQuality.Custom, (ButtonWidget) this.FindChildElement(118));
      this.mFillDensityButtons = new Dictionary<FillQuality, ButtonWidget>();
      this.mFillDensityButtons.Add(FillQuality.ExtraHigh, (ButtonWidget) this.FindChildElement(220));
      this.mFillDensityButtons.Add(FillQuality.High, (ButtonWidget) this.FindChildElement(221));
      this.mFillDensityButtons.Add(FillQuality.Medium, (ButtonWidget) this.FindChildElement(222));
      this.mFillDensityButtons.Add(FillQuality.Low, (ButtonWidget) this.FindChildElement(223));
      this.mFillDensityButtons.Add(FillQuality.HollowThickWalls, (ButtonWidget) this.FindChildElement(224));
      this.mFillDensityButtons.Add(FillQuality.HollowThinWalls, (ButtonWidget) this.FindChildElement(225));
      this.mFillDensityButtons.Add(FillQuality.Solid, (ButtonWidget) this.FindChildElement(227));
      this.mFillDensityButtons.Add(FillQuality.Custom, (ButtonWidget) this.FindChildElement(228));
      this.print_button = (ButtonWidget) this.FindChildElement(401);
      this.quality_scroll_list = (HorizontalLayoutScrollList) this.FindChildElement(110);
      this.density_scroll_list = (HorizontalLayoutScrollList) this.FindChildElement(219);
      this.printQualityPrev_button = (ButtonWidget) this.FindChildElement(109);
      this.printQualityPrev_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.printQualityNext_button = (ButtonWidget) this.FindChildElement(117);
      this.printQualityNext_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.fillDensityPrev_button = (ButtonWidget) this.FindChildElement(218);
      this.fillDensityPrev_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.fillDensityNext_button = (ButtonWidget) this.FindChildElement(226);
      this.fillDensityNext_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.support_checkbutton = (ButtonWidget) this.FindChildElement(301);
      this.support_everywhere = (ButtonWidget) this.FindChildElement(303);
      this.support_printbedonly = (ButtonWidget) this.FindChildElement(313);
      this.UseWaveBonding = (ButtonWidget) this.FindChildElement(305);
      this.raft_checkbutton = (ButtonWidget) this.FindChildElement(307);
      this.verifybed_checkbutton = (ButtonWidget) this.FindChildElement(309);
      this.verifybed_text = (TextWidget) this.FindChildElement(310);
      this.printQuality_editbox = (EditBoxWidget) this.FindChildElement(108);
      this.fillDensity_editbox = (EditBoxWidget) this.FindChildElement(217);
      this.enableskirt_checkbutton = (ButtonWidget) this.FindChildElement(311);
      this.heatedBedButton_checkbox = (ButtonWidget) this.FindChildElement(315);
      this.heatedBedButton_text = (TextWidget) this.FindChildElement(316);
      this.untetheredButton_checkbox = (ButtonWidget) this.FindChildElement(317);
      this.sdOnlyButton_checkbox = (ButtonWidget) this.FindChildElement(319);
      this.sdOnlyButton_text = (TextWidget) this.FindChildElement(320);
      this.sdCheckboxesFrame = (XMLFrame) this.FindChildElement(321);
      this.mPrintQualityButtons[PrintQuality.Custom].Visible = false;
      this.mFillDensityButtons[FillQuality.Custom].Visible = false;
      this.LoadSettings();
    }

    public override void OnActivate(PrintJobDetails details)
    {
      this.PrintDialogWindow.SetSize(750, 550);
      this.PrintDialogWindow.Refresh();
      this.CurrentJobDetails = details;
      this.LoadSettings();
      this.SlicerConnection.SlicerSettingStack.PushSettings();
      this.SetFillandQualityButtons();
      PrinterObject selectedPrinter = this.SelectedPrinter;
      this.CheckVerifyBedAvailability(selectedPrinter, true);
      this.CheckHeatedBedAvailability(selectedPrinter, true);
      this.CheckSDCardAvailability(selectedPrinter, true);
      this.SetSupportEnabledControls(false);
      this.mQualityButtonsSet = true;
    }

    public override void OnDeactivate()
    {
      this.SaveSettings();
    }

    private void SyncFromSlicerSettings()
    {
      this.syncing = true;
      this.support_checkbutton.Checked = this.SlicerSettings.HasSupport;
      this.SetSupportEnabledControls(this.SlicerSettings.HasSupport && this.SlicerSettings.HasModelonModelSupport);
      this.raft_checkbutton.Checked = this.SlicerSettings.HasRaftEnabled;
      this.enableskirt_checkbutton.Checked = this.SlicerSettings.HasSkirt;
      PrintQuality index = this.SlicerConnection.SlicerSettings.CurrentPrintQuality;
      if (this.SlicerConnection.SlicerSettings.UsingCustomExtrusionWidth || !this.SlicerConnection.SlicerSettings.UsingAutoFanSettings)
        index = PrintQuality.Custom;
      if (index <= PrintQuality.HighQuality)
      {
        if (index != PrintQuality.Expert && index != PrintQuality.VeryHighQuality && index != PrintQuality.HighQuality)
          goto label_6;
      }
      else if (index != PrintQuality.MediumQuality && index != PrintQuality.FastPrint && index != PrintQuality.VeryFastPrint)
        goto label_6;
      this.mPrintQualityButtons[index].SetChecked(true);
      goto label_9;
label_6:
      if (!this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]))
        this.quality_scroll_list.AddChildElement((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]);
      this.mPrintQualityButtons[PrintQuality.Custom].SetChecked(true);
label_9:
      FillQuality currentFillQuality = this.SlicerConnection.SlicerSettings.CurrentFillQuality;
      switch (currentFillQuality)
      {
        case FillQuality.HollowThinWalls:
        case FillQuality.HollowThickWalls:
        case FillQuality.Solid:
        case FillQuality.ExtraHigh:
        case FillQuality.High:
        case FillQuality.Medium:
        case FillQuality.Low:
          this.mFillDensityButtons[currentFillQuality].SetChecked(true);
          break;
        default:
          if (!this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[FillQuality.Custom]))
            this.density_scroll_list.AddChildElement((Element2D) this.mFillDensityButtons[FillQuality.Custom]);
          this.mFillDensityButtons[FillQuality.Custom].SetChecked(true);
          break;
      }
      this.syncing = false;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
    }

    private void SetFillandQualityButtons()
    {
      if (this.SelectedPrinter == null)
        return;
      this.SyncFromSlicerSettings();
      foreach (System.Collections.Generic.KeyValuePair<PrintQuality, ButtonWidget> printQualityButton in this.mPrintQualityButtons)
      {
        if (this.quality_scroll_list.ChildList.Contains((Element2D) printQualityButton.Value))
        {
          this.quality_scroll_list.RemoveChildElement((Element2D) printQualityButton.Value);
          printQualityButton.Value.Visible = false;
        }
      }
      foreach (System.Collections.Generic.KeyValuePair<FillQuality, ButtonWidget> fillDensityButton in this.mFillDensityButtons)
      {
        if (this.density_scroll_list.ChildList.Contains((Element2D) fillDensityButton.Value))
        {
          this.density_scroll_list.RemoveChildElement((Element2D) fillDensityButton.Value);
          fillDensityButton.Value.Visible = false;
        }
      }
      this.quality_scroll_list.OnUpdate();
      this.density_scroll_list.OnUpdate();
      List<PrintQuality> printQualityList = new List<PrintQuality>();
      printQualityList.Add(PrintQuality.VeryFastPrint);
      printQualityList.Add(PrintQuality.FastPrint);
      printQualityList.Add(PrintQuality.MediumQuality);
      printQualityList.Add(PrintQuality.HighQuality);
      printQualityList.Add(PrintQuality.VeryHighQuality);
      printQualityList.Add(PrintQuality.Expert);
      List<FillQuality> fillQualityList = new List<FillQuality>()
      {
        FillQuality.HollowThinWalls,
        FillQuality.HollowThickWalls,
        FillQuality.Low,
        FillQuality.Medium,
        FillQuality.High,
        FillQuality.ExtraHigh,
        FillQuality.Solid
      };
      foreach (PrintQuality index in printQualityList)
      {
        if (this.SlicerSettings.SupportedPrintQualities.Contains(index) && index != PrintQuality.Custom && (index != PrintQuality.Expert && !this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[index])))
        {
          this.quality_scroll_list.AddChildElement((Element2D) this.mPrintQualityButtons[index]);
          this.mPrintQualityButtons[index].Visible = true;
        }
      }
      foreach (FillQuality index in fillQualityList)
      {
        if (this.SlicerSettings.SupportedFillQualities.Contains(index) && index != FillQuality.Custom && (index != FillQuality.Solid && !this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[index])))
        {
          this.density_scroll_list.AddChildElement((Element2D) this.mFillDensityButtons[index]);
          this.mFillDensityButtons[index].Visible = true;
        }
      }
      this.ValidatePrintQualityButtons();
      this.ValidatePrintFillDensityButtons();
      this.mQualityButtonsSet = true;
    }

    private void ValidatePrintQualityButtons()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || this.quality_scroll_list == null)
        return;
      if (this.SlicerSettings.SupportedPrintQualities.Contains(PrintQuality.Expert) && selectedPrinter.Info.filament_info.filament_type != FilamentSpool.TypeEnum.ABS && selectedPrinter.Info.filament_info.filament_type != FilamentSpool.TypeEnum.ABS_R)
      {
        this.mPrintQualityButtons[PrintQuality.Expert].Visible = true;
        if (!this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[PrintQuality.Expert]))
          this.quality_scroll_list.AddChildElement((Element2D) this.mPrintQualityButtons[PrintQuality.Expert]);
      }
      else
      {
        this.mPrintQualityButtons[PrintQuality.Expert].Visible = false;
        if (this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[PrintQuality.Expert]))
        {
          if (this.mPrintQualityButtons[PrintQuality.Expert].Checked)
            this.mPrintQualityButtons[this.SlicerSettings.SupportedPrintQualities.Last<PrintQuality>()].SetChecked(true);
          this.quality_scroll_list.RemoveChildElement((Element2D) this.mPrintQualityButtons[PrintQuality.Expert]);
        }
      }
      if (this.mPrintQualityButtons[PrintQuality.Custom].Checked)
      {
        this.mPrintQualityButtons[PrintQuality.Custom].Visible = true;
        if (!this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]))
          this.quality_scroll_list.AddChildElement((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]);
      }
      else
      {
        this.mPrintQualityButtons[PrintQuality.Custom].Visible = false;
        if (this.quality_scroll_list.ChildList.Contains((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]))
          this.quality_scroll_list.RemoveChildElement((Element2D) this.mPrintQualityButtons[PrintQuality.Custom]);
      }
      this.quality_scroll_list.OnParentResize();
    }

    private void ValidatePrintFillDensityButtons()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || this.density_scroll_list == null)
        return;
      if (this.SlicerSettings.SupportedFillQualities.Contains(FillQuality.Solid) && (selectedPrinter.Info.filament_info.filament_type == FilamentSpool.TypeEnum.FLX || selectedPrinter.Info.filament_info.filament_type == FilamentSpool.TypeEnum.TGH))
      {
        this.mFillDensityButtons[FillQuality.Solid].Visible = true;
        if (!this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[FillQuality.Solid]))
          this.density_scroll_list.AddChildElement((Element2D) this.mFillDensityButtons[FillQuality.Solid]);
      }
      else
      {
        this.mFillDensityButtons[FillQuality.Solid].Visible = false;
        if (this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[FillQuality.Solid]))
        {
          if (this.mFillDensityButtons[FillQuality.Solid].Checked)
            this.mFillDensityButtons[this.SlicerSettings.SupportedFillQualities.Last<FillQuality>()].SetChecked(true);
          this.density_scroll_list.RemoveChildElement((Element2D) this.mFillDensityButtons[FillQuality.Solid]);
        }
      }
      if (this.mFillDensityButtons[FillQuality.Custom].Checked)
      {
        this.mFillDensityButtons[FillQuality.Custom].Visible = true;
        if (!this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[FillQuality.Custom]))
          this.density_scroll_list.AddChildElement((Element2D) this.mFillDensityButtons[FillQuality.Custom]);
      }
      else
      {
        this.mFillDensityButtons[FillQuality.Custom].Visible = false;
        if (this.density_scroll_list.ChildList.Contains((Element2D) this.mFillDensityButtons[FillQuality.Custom]))
          this.density_scroll_list.RemoveChildElement((Element2D) this.mFillDensityButtons[FillQuality.Custom]);
      }
      this.density_scroll_list.OnParentResize();
    }

    private void CheckSDCardAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
        return;
      if (selected_printer.SDCardExtension.Available)
      {
        this.untetheredButton_checkbox.Checked = this.settingsManager.Settings.GetPrintSettingsSafe(selected_printer.MyPrinterProfile.ProfileName).AutoUntetheredSupport;
        this.sdCheckboxesFrame.Enabled = true;
        this.sdCheckboxesFrame.Visible = true;
      }
      else
      {
        this.untetheredButton_checkbox.Checked = false;
        this.sdCheckboxesFrame.Enabled = false;
        this.sdCheckboxesFrame.Visible = false;
      }
      if (this.settingsManager.CurrentAppearanceSettings.AllowSDOnlyPrinting)
      {
        this.sdOnlyButton_checkbox.Visible = true;
        this.sdOnlyButton_text.Visible = true;
      }
      else
      {
        this.sdOnlyButton_checkbox.Visible = false;
        this.sdOnlyButton_text.Visible = false;
      }
    }

    private void CheckHeatedBedAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
        return;
      bool flag = false;
      if (selected_printer.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed && selected_printer.Info.supportedFeatures.UsesSupportedFeatures)
        flag = selected_printer.Info.supportedFeatures.Available("Heated Bed Control", selected_printer.MyPrinterProfile.SupportedFeaturesConstants);
      if (flag)
      {
        this.heatedBedButton_checkbox.Checked = this.settingsManager.Settings.GetPrintSettingsSafe(selected_printer.MyPrinterProfile.ProfileName).UseHeatedBed;
        this.heatedBedButton_checkbox.Enabled = true;
        this.heatedBedButton_checkbox.Visible = true;
        this.heatedBedButton_text.Visible = true;
      }
      else
      {
        this.heatedBedButton_checkbox.Checked = false;
        this.heatedBedButton_checkbox.Enabled = false;
        this.heatedBedButton_checkbox.Visible = false;
        this.heatedBedButton_text.Visible = false;
      }
    }

    private void CheckVerifyBedAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
        return;
      SupportedFeatures.Status status = SupportedFeatures.Status.Available;
      if (selected_printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        int featureSlot = selected_printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
        if (featureSlot >= 0)
          status = selected_printer.Info.supportedFeatures.GetStatus(featureSlot);
      }
      if (status == SupportedFeatures.Status.Unavailable)
      {
        if (!this.verifybed_checkbutton.Enabled)
          return;
        this.verifybed_checkbutton.Checked = false;
        this.verifybed_checkbutton.Enabled = false;
        this.verifybed_checkbutton.Visible = false;
        this.verifybed_text.Visible = false;
      }
      else if (SupportedFeatures.Status.Available == status)
      {
        if (!this.verifybed_checkbutton.Enabled)
        {
          this.verifybed_checkbutton.Enabled = true;
          this.verifybed_checkbutton.Visible = true;
          this.verifybed_text.Visible = true;
        }
        this.verifybed_text.Text = "T_PrintDialog_VerifyBed";
      }
      else
      {
        if (!this.verifybed_checkbutton.Enabled)
        {
          this.verifybed_checkbutton.Enabled = true;
          this.verifybed_checkbutton.Visible = true;
          this.verifybed_text.Visible = true;
        }
        this.verifybed_checkbutton.Checked = false;
        this.verifybed_text.Text = "T_PrintDialog_VerifyBedNotRec";
      }
    }

    private void SetSupportEnabledControls(bool bUseSupportEverywhere)
    {
      if (this.support_checkbutton.Checked)
      {
        this.support_everywhere.Enabled = true;
        this.support_everywhere.Checked = bUseSupportEverywhere;
        this.support_printbedonly.Enabled = true;
        this.support_printbedonly.Checked = !bUseSupportEverywhere;
      }
      else
      {
        this.support_everywhere.Checked = false;
        this.support_everywhere.Enabled = false;
        this.support_printbedonly.Checked = false;
        this.support_printbedonly.Enabled = false;
      }
    }

    public void OnPrintButtonPressed(bool bPrintToFile)
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null)
        return;
      JobOptions jobOptions = new JobOptions(false);
      this.CurrentJobDetails.print_to_file = bPrintToFile;
      if (bPrintToFile)
      {
        this.CurrentJobDetails.printToFileOutputFile = SaveModelFileDialog.RunSaveFileDialog(SaveModelFileDialog.FileType.GCode);
        if (string.IsNullOrEmpty(this.CurrentJobDetails.printToFileOutputFile))
        {
          this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, this.CurrentJobDetails);
          return;
        }
      }
      jobOptions.autostart_ignorewarnings = true;
      jobOptions.use_raft_DetailOnly = this.SlicerSettings.HasRaftEnabled;
      jobOptions.use_wave_bonding = this.UseWaveBonding.Checked;
      jobOptions.use_fan_preprocessor = this.SlicerSettings.UsingAutoFanSettings;
      jobOptions.use_support_DetailOnly = this.support_checkbutton.Checked;
      jobOptions.use_support_everywhere_DetailOnly = this.support_everywhere.Checked;
      jobOptions.calibrate_before_print = this.verifybed_checkbutton.Checked;
      jobOptions.calibrate_before_print_z = 0.4f;
      if (selectedPrinter.Info.calibration.UsesCalibrationOffset)
        jobOptions.calibrate_before_print_z += selectedPrinter.Info.calibration.CALIBRATION_OFFSET;
      jobOptions.use_heated_bed = this.heatedBedButton_checkbox.Checked;
      jobOptions.quality_layer_resolution_DetailOnly = (int) this.GetPrintQuality();
      jobOptions.fill_density_DetailOnly = (int) this.GetFillDensity();
      this.CurrentJobDetails.GenerateSlicerSettings(selectedPrinter, this.printerview);
      this.CurrentJobDetails.auto_untethered_print = this.untetheredButton_checkbox.Checked;
      this.CurrentJobDetails.sdSaveOnly_print = this.sdOnlyButton_checkbox.Checked;
      this.SlicerConnection.SlicerSettingStack.SaveSettingsDown();
      this.CurrentJobDetails.printer = selectedPrinter;
      this.CurrentJobDetails.jobOptions = jobOptions;
      this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PreSlicingFrame, this.CurrentJobDetails);
    }

    private void ReleasePrinterLock(PrinterObject printer)
    {
      int num = (int) printer.ReleaseLock((AsyncCallback) null, (object) null);
    }

    private PrintQuality GetPrintQuality()
    {
      PrintQuality printQuality = PrintQuality.FastPrint;
      if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY2"))
        printQuality = PrintQuality.MediumQuality;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY3"))
        printQuality = PrintQuality.FastPrint;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY1"))
        printQuality = PrintQuality.HighQuality;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY4"))
        printQuality = PrintQuality.VeryFastPrint;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY6"))
        printQuality = PrintQuality.VeryHighQuality;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY5"))
        printQuality = PrintQuality.Expert;
      else if (this.printQuality_editbox.Text == this.host.Locale.T("T_PRINTQUALITY7"))
        printQuality = PrintQuality.Custom;
      return printQuality;
    }

    private FillQuality GetFillDensity()
    {
      FillQuality fillQuality = FillQuality.HollowThinWalls;
      if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY3"))
        fillQuality = FillQuality.Medium;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY4"))
        fillQuality = FillQuality.Low;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY2"))
        fillQuality = FillQuality.High;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY6"))
        fillQuality = FillQuality.HollowThinWalls;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY5"))
        fillQuality = FillQuality.HollowThickWalls;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY1"))
        fillQuality = FillQuality.ExtraHigh;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY7"))
        fillQuality = FillQuality.Solid;
      else if (this.fillDensity_editbox.Text == this.host.Locale.T("T_FILLDENSITY8"))
        fillQuality = FillQuality.Custom;
      return fillQuality;
    }

    public override void OnUpdate()
    {
      if (!this.Visible)
        return;
      base.OnUpdate();
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter != null && this.m_bNewPrinterSelected)
      {
        if (this.print_button != null)
          this.print_button.Enabled = true;
        if (!this.mQualityButtonsSet)
        {
          this.SetFillandQualityButtons();
        }
        else
        {
          this.ValidatePrintQualityButtons();
          this.ValidatePrintFillDensityButtons();
        }
        this.CheckVerifyBedAvailability(selectedPrinter, this.m_bNewPrinterSelected);
        this.CheckHeatedBedAvailability(selectedPrinter, this.m_bNewPrinterSelected);
        this.CheckSDCardAvailability(selectedPrinter, this.m_bNewPrinterSelected);
        this.m_bNewPrinterSelected = false;
        if (this.quality_scroll_list != null && this.printQualityPrev_button != null && this.printQualityNext_button != null)
        {
          if (this.quality_scroll_list.OnLastElement)
            this.printQualityNext_button.Enabled = false;
          else
            this.printQualityNext_button.Enabled = true;
          if (this.quality_scroll_list.OnFirstElement)
            this.printQualityPrev_button.Enabled = false;
          else
            this.printQualityPrev_button.Enabled = true;
        }
        if (this.density_scroll_list == null || this.fillDensityPrev_button == null || this.fillDensityNext_button == null)
          return;
        if (this.density_scroll_list.OnLastElement)
          this.fillDensityNext_button.Enabled = false;
        else
          this.fillDensityNext_button.Enabled = true;
        if (this.density_scroll_list.OnFirstElement)
          this.fillDensityPrev_button.Enabled = false;
        else
          this.fillDensityPrev_button.Enabled = true;
      }
      else
        this.mQualityButtonsSet = false;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      FilamentSpool filament = (FilamentSpool) null;
      if (selectedPrinter != null)
        filament = selectedPrinter.GetCurrentFilament();
      switch (button.ID)
      {
        case 109:
          --this.quality_scroll_list.StartIndex;
          break;
        case 111:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY5");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.Expert, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 112:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY1");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.HighQuality, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 113:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY2");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.MediumQuality, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 114:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY3");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.FastPrint, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 115:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY4");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.VeryFastPrint, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 116:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY6");
          if (!this.syncing)
            this.SlicerSettings.SetPrintQuality(PrintQuality.VeryHighQuality, filament, this.CurrentJobDetails.slicer_objects.Count);
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 117:
          ++this.quality_scroll_list.StartIndex;
          break;
        case 118:
          this.printQuality_editbox.Text = this.host.Locale.T("T_PRINTQUALITY7");
          if (string.IsNullOrEmpty(this.printQuality_editbox.Text))
            this.printQuality_editbox.Text = "Custom";
          this.quality_scroll_list.GotoChild(button.ID);
          break;
        case 218:
          --this.density_scroll_list.StartIndex;
          break;
        case 220:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY1");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.ExtraHigh);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 221:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY2");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.High);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 222:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY3");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.Medium);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 223:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY4");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.Low);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 224:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY5");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.HollowThickWalls);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 225:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY6");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.HollowThinWalls);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 226:
          ++this.density_scroll_list.StartIndex;
          break;
        case 227:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY7");
          if (!this.syncing)
            this.SlicerSettings.SetFillQuality(FillQuality.Solid);
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 228:
          this.fillDensity_editbox.Text = this.host.Locale.T("T_FILLDENSITY8");
          if (string.IsNullOrEmpty(this.fillDensity_editbox.Text))
            this.fillDensity_editbox.Text = "Custom";
          this.density_scroll_list.GotoChild(button.ID);
          break;
        case 301:
          if (this.syncing)
            break;
          this.SetSupportEnabledControls(false);
          if (this.support_checkbutton.Checked)
          {
            this.support_everywhere.Enabled = true;
            this.support_printbedonly.Enabled = true;
            this.support_printbedonly.Checked = true;
            this.SlicerSettings.EnableSupport(SupportType.LineSupport);
            break;
          }
          this.SlicerSettings.DisableSupport();
          break;
        case 303:
          if (this.syncing || !this.support_everywhere.Checked)
            break;
          this.SlicerSettings.EnableSupport(SupportType.LineSupportEveryWhere);
          break;
        case 307:
          if (this.syncing)
            break;
          if (button.Checked)
          {
            this.SlicerSettings.EnableRaft(filament);
            break;
          }
          this.SlicerSettings.DisableRaft();
          break;
        case 311:
          if (this.syncing)
            break;
          if (button.Checked)
          {
            this.SlicerSettings.EnableSkirt();
            break;
          }
          this.SlicerSettings.DisableSkirt();
          break;
        case 313:
          if (this.syncing || !this.support_printbedonly.Checked)
            break;
          this.SlicerSettings.EnableSupport(SupportType.LineSupport);
          break;
        case 401:
        case 404:
          if (this.printerview.ModelLoaded)
          {
            this.SaveSettings();
            this.OnPrintButtonPressed(button.ID == 404);
            break;
          }
          this.message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoModel"));
          break;
        case 402:
          this.PrintDialogWindow.CloseWindow();
          break;
        case 403:
          this.ResetSettings();
          this.LoadSettings();
          break;
        case 501:
          this.OpenAdvancedPrintSettingsDialog();
          break;
      }
    }

    private void OpenAdvancedPrintSettingsDialog()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null)
        return;
      this.CurrentJobDetails.printer = selectedPrinter;
      this.CurrentJobDetails.jobOptions = new JobOptions(false)
      {
        autostart_ignorewarnings = true
      };
      this.CurrentJobDetails.GenerateSlicerSettings(selectedPrinter, this.printerview);
      this.PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.AdvancedPrintSettingsFrame, this.CurrentJobDetails);
    }

    private bool LoadSettings()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.MyPrinterProfile != null)
      {
        this.settingsManager.LoadSettings();
        SettingsManager.PrintSettings printSettingsSafe = this.settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
        ButtonWidget childElement1 = (ButtonWidget) this.FindChildElement(305);
        if (childElement1 != null)
          childElement1.Checked = printSettingsSafe.WaveBonding;
        ButtonWidget childElement2 = (ButtonWidget) this.FindChildElement(309);
        if (childElement2 != null)
          childElement2.Checked = printSettingsSafe.VerifyBed;
        ButtonWidget childElement3 = (ButtonWidget) this.FindChildElement(315);
        if (childElement3 != null)
          childElement3.Checked = printSettingsSafe.UseHeatedBed;
        ButtonWidget childElement4 = (ButtonWidget) this.FindChildElement(317);
        if (childElement4 != null)
          childElement4.Checked = printSettingsSafe.AutoUntetheredSupport;
      }
      return true;
    }

    private void SaveSettings()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.MyPrinterProfile != null)
      {
        SettingsManager.PrintSettings printSettingsSafe = this.settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
        ButtonWidget childElement1 = (ButtonWidget) this.FindChildElement(305);
        if (childElement1 != null)
          printSettingsSafe.WaveBonding = childElement1.Checked;
        ButtonWidget childElement2 = (ButtonWidget) this.FindChildElement(309);
        if (childElement2 != null)
          printSettingsSafe.VerifyBed = childElement2.Checked;
        ButtonWidget childElement3 = (ButtonWidget) this.FindChildElement(315);
        if (childElement3 != null && childElement3.Enabled)
          printSettingsSafe.UseHeatedBed = childElement3.Checked;
        ButtonWidget childElement4 = (ButtonWidget) this.FindChildElement(317);
        if (childElement4 != null && childElement4.Enabled)
          printSettingsSafe.AutoUntetheredSupport = childElement4.Checked;
      }
      this.settingsManager.SaveSettings();
    }

    private void ResetSettings()
    {
      PrinterObject selectedPrinter = this.SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter.MyPrinterProfile == null)
        return;
      this.settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName).Reset();
      this.settingsManager.SaveSettings();
      this.SlicerSettings.SetToDefault();
      this.SyncFromSlicerSettings();
    }

    public override PrinterObject SelectedPrinter
    {
      get
      {
        PrinterObject selectedPrinter = base.SelectedPrinter;
        if (selectedPrinter == null || selectedPrinter != this.mPrevPrinter)
        {
          this.m_bNewPrinterSelected = true;
          this.mPrevPrinter = selectedPrinter;
          this.mQualityButtonsSet = false;
        }
        else
          this.m_bNewPrinterSelected = false;
        return selectedPrinter;
      }
    }
  }
}
