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
      settingsManager = settings;
      this.host = host;
      CenterHorizontallyInParent = true;
      CenterVerticallyInParent = true;
      SetSize(750, 550);
      var printdialog = Resources.printdialog;
      var xmlFrame = new XMLFrame(ID)
      {
        RelativeWidth = 1f,
        RelativeHeight = 1f
      };
      AddChildElement(xmlFrame);
      xmlFrame.Init(host, printdialog, new ButtonCallback(MyButtonCallback));
      mPrintQualityButtons = new Dictionary<PrintQuality, ButtonWidget>
      {
        { PrintQuality.Expert, (ButtonWidget)FindChildElement(111) },
        { PrintQuality.VeryHighQuality, (ButtonWidget)FindChildElement(116) },
        { PrintQuality.HighQuality, (ButtonWidget)FindChildElement(112) },
        { PrintQuality.MediumQuality, (ButtonWidget)FindChildElement(113) },
        { PrintQuality.FastPrint, (ButtonWidget)FindChildElement(114) },
        { PrintQuality.VeryFastPrint, (ButtonWidget)FindChildElement(115) },
        { PrintQuality.Custom, (ButtonWidget)FindChildElement(118) }
      };
      mFillDensityButtons = new Dictionary<FillQuality, ButtonWidget>
      {
        { FillQuality.ExtraHigh, (ButtonWidget)FindChildElement(220) },
        { FillQuality.High, (ButtonWidget)FindChildElement(221) },
        { FillQuality.Medium, (ButtonWidget)FindChildElement(222) },
        { FillQuality.Low, (ButtonWidget)FindChildElement(223) },
        { FillQuality.HollowThickWalls, (ButtonWidget)FindChildElement(224) },
        { FillQuality.HollowThinWalls, (ButtonWidget)FindChildElement(225) },
        { FillQuality.Solid, (ButtonWidget)FindChildElement(227) },
        { FillQuality.Custom, (ButtonWidget)FindChildElement(228) }
      };
      print_button = (ButtonWidget)FindChildElement(401);
      quality_scroll_list = (HorizontalLayoutScrollList)FindChildElement(110);
      density_scroll_list = (HorizontalLayoutScrollList)FindChildElement(219);
      printQualityPrev_button = (ButtonWidget)FindChildElement(109);
      printQualityPrev_button.SetCallback(new ButtonCallback(MyButtonCallback));
      printQualityNext_button = (ButtonWidget)FindChildElement(117);
      printQualityNext_button.SetCallback(new ButtonCallback(MyButtonCallback));
      fillDensityPrev_button = (ButtonWidget)FindChildElement(218);
      fillDensityPrev_button.SetCallback(new ButtonCallback(MyButtonCallback));
      fillDensityNext_button = (ButtonWidget)FindChildElement(226);
      fillDensityNext_button.SetCallback(new ButtonCallback(MyButtonCallback));
      support_checkbutton = (ButtonWidget)FindChildElement(301);
      support_everywhere = (ButtonWidget)FindChildElement(303);
      support_printbedonly = (ButtonWidget)FindChildElement(313);
      UseWaveBonding = (ButtonWidget)FindChildElement(305);
      raft_checkbutton = (ButtonWidget)FindChildElement(307);
      verifybed_checkbutton = (ButtonWidget)FindChildElement(309);
      verifybed_text = (TextWidget)FindChildElement(310);
      printQuality_editbox = (EditBoxWidget)FindChildElement(108);
      fillDensity_editbox = (EditBoxWidget)FindChildElement(217);
      enableskirt_checkbutton = (ButtonWidget)FindChildElement(311);
      heatedBedButton_checkbox = (ButtonWidget)FindChildElement(315);
      heatedBedButton_text = (TextWidget)FindChildElement(316);
      untetheredButton_checkbox = (ButtonWidget)FindChildElement(317);
      sdOnlyButton_checkbox = (ButtonWidget)FindChildElement(319);
      sdOnlyButton_text = (TextWidget)FindChildElement(320);
      sdCheckboxesFrame = (XMLFrame)FindChildElement(321);
      mPrintQualityButtons[PrintQuality.Custom].Visible = false;
      mFillDensityButtons[FillQuality.Custom].Visible = false;
      LoadSettings();
    }

    public override void OnActivate(PrintJobDetails details)
    {
      PrintDialogWindow.SetSize(750, 550);
      PrintDialogWindow.Refresh();
      CurrentJobDetails = details;
      LoadSettings();
      SlicerConnection.SlicerSettingStack.PushSettings();
      SetFillandQualityButtons();
      PrinterObject selectedPrinter = SelectedPrinter;
      CheckVerifyBedAvailability(selectedPrinter, true);
      CheckHeatedBedAvailability(selectedPrinter, true);
      CheckSDCardAvailability(selectedPrinter, true);
      SetSupportEnabledControls(false);
      mQualityButtonsSet = true;
    }

    public override void OnDeactivate()
    {
      SaveSettings();
    }

    private void SyncFromSlicerSettings()
    {
      syncing = true;
      support_checkbutton.Checked = SlicerSettings.HasSupport;
      SetSupportEnabledControls(SlicerSettings.HasSupport && SlicerSettings.HasModelonModelSupport);
      raft_checkbutton.Checked = SlicerSettings.HasRaftEnabled;
      enableskirt_checkbutton.Checked = SlicerSettings.HasSkirt;
      PrintQuality index = SlicerConnection.SlicerSettings.CurrentPrintQuality;
      if (SlicerConnection.SlicerSettings.UsingCustomExtrusionWidth || !SlicerConnection.SlicerSettings.UsingAutoFanSettings)
      {
        index = PrintQuality.Custom;
      }

      if (index <= PrintQuality.HighQuality)
      {
        if (index != PrintQuality.Expert && index != PrintQuality.VeryHighQuality && index != PrintQuality.HighQuality)
        {
          goto label_6;
        }
      }
      else if (index != PrintQuality.MediumQuality && index != PrintQuality.FastPrint && index != PrintQuality.VeryFastPrint)
      {
        goto label_6;
      }

      mPrintQualityButtons[index].SetChecked(true);
      goto label_9;
label_6:
      if (!quality_scroll_list.ChildList.Contains(mPrintQualityButtons[PrintQuality.Custom]))
      {
        quality_scroll_list.AddChildElement(mPrintQualityButtons[PrintQuality.Custom]);
      }

      mPrintQualityButtons[PrintQuality.Custom].SetChecked(true);
label_9:
      FillQuality currentFillQuality = SlicerConnection.SlicerSettings.CurrentFillQuality;
      switch (currentFillQuality)
      {
        case FillQuality.HollowThinWalls:
        case FillQuality.HollowThickWalls:
        case FillQuality.Solid:
        case FillQuality.ExtraHigh:
        case FillQuality.High:
        case FillQuality.Medium:
        case FillQuality.Low:
          mFillDensityButtons[currentFillQuality].SetChecked(true);
          break;
        default:
          if (!density_scroll_list.ChildList.Contains(mFillDensityButtons[FillQuality.Custom]))
          {
            density_scroll_list.AddChildElement(mFillDensityButtons[FillQuality.Custom]);
          }

          mFillDensityButtons[FillQuality.Custom].SetChecked(true);
          break;
      }
      syncing = false;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
    }

    private void SetFillandQualityButtons()
    {
      if (SelectedPrinter == null)
      {
        return;
      }

      SyncFromSlicerSettings();
      foreach (System.Collections.Generic.KeyValuePair<PrintQuality, ButtonWidget> printQualityButton in mPrintQualityButtons)
      {
        if (quality_scroll_list.ChildList.Contains(printQualityButton.Value))
        {
          quality_scroll_list.RemoveChildElement(printQualityButton.Value);
          printQualityButton.Value.Visible = false;
        }
      }
      foreach (System.Collections.Generic.KeyValuePair<FillQuality, ButtonWidget> fillDensityButton in mFillDensityButtons)
      {
        if (density_scroll_list.ChildList.Contains(fillDensityButton.Value))
        {
          density_scroll_list.RemoveChildElement(fillDensityButton.Value);
          fillDensityButton.Value.Visible = false;
        }
      }
      quality_scroll_list.OnUpdate();
      density_scroll_list.OnUpdate();
      var printQualityList = new List<PrintQuality>
      {
        PrintQuality.VeryFastPrint,
        PrintQuality.FastPrint,
        PrintQuality.MediumQuality,
        PrintQuality.HighQuality,
        PrintQuality.VeryHighQuality,
        PrintQuality.Expert
      };
      var fillQualityList = new List<FillQuality>()
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
        if (SlicerSettings.SupportedPrintQualities.Contains(index) && index != PrintQuality.Custom && (index != PrintQuality.Expert && !quality_scroll_list.ChildList.Contains(mPrintQualityButtons[index])))
        {
          quality_scroll_list.AddChildElement(mPrintQualityButtons[index]);
          mPrintQualityButtons[index].Visible = true;
        }
      }
      foreach (FillQuality index in fillQualityList)
      {
        if (SlicerSettings.SupportedFillQualities.Contains(index) && index != FillQuality.Custom && (index != FillQuality.Solid && !density_scroll_list.ChildList.Contains(mFillDensityButtons[index])))
        {
          density_scroll_list.AddChildElement(mFillDensityButtons[index]);
          mFillDensityButtons[index].Visible = true;
        }
      }
      ValidatePrintQualityButtons();
      ValidatePrintFillDensityButtons();
      mQualityButtonsSet = true;
    }

    private void ValidatePrintQualityButtons()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || quality_scroll_list == null)
      {
        return;
      }

      if (SlicerSettings.SupportedPrintQualities.Contains(PrintQuality.Expert) && selectedPrinter.Info.filament_info.filament_type != FilamentSpool.TypeEnum.ABS && selectedPrinter.Info.filament_info.filament_type != FilamentSpool.TypeEnum.ABS_R)
      {
        mPrintQualityButtons[PrintQuality.Expert].Visible = true;
        if (!quality_scroll_list.ChildList.Contains(mPrintQualityButtons[PrintQuality.Expert]))
        {
          quality_scroll_list.AddChildElement(mPrintQualityButtons[PrintQuality.Expert]);
        }
      }
      else
      {
        mPrintQualityButtons[PrintQuality.Expert].Visible = false;
        if (quality_scroll_list.ChildList.Contains(mPrintQualityButtons[PrintQuality.Expert]))
        {
          if (mPrintQualityButtons[PrintQuality.Expert].Checked)
          {
            mPrintQualityButtons[SlicerSettings.SupportedPrintQualities.Last<PrintQuality>()].SetChecked(true);
          }

          quality_scroll_list.RemoveChildElement(mPrintQualityButtons[PrintQuality.Expert]);
        }
      }
      if (mPrintQualityButtons[PrintQuality.Custom].Checked)
      {
        mPrintQualityButtons[PrintQuality.Custom].Visible = true;
        if (!quality_scroll_list.ChildList.Contains(mPrintQualityButtons[PrintQuality.Custom]))
        {
          quality_scroll_list.AddChildElement(mPrintQualityButtons[PrintQuality.Custom]);
        }
      }
      else
      {
        mPrintQualityButtons[PrintQuality.Custom].Visible = false;
        if (quality_scroll_list.ChildList.Contains(mPrintQualityButtons[PrintQuality.Custom]))
        {
          quality_scroll_list.RemoveChildElement(mPrintQualityButtons[PrintQuality.Custom]);
        }
      }
      quality_scroll_list.OnParentResize();
    }

    private void ValidatePrintFillDensityButtons()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || density_scroll_list == null)
      {
        return;
      }

      if (SlicerSettings.SupportedFillQualities.Contains(FillQuality.Solid) && (selectedPrinter.Info.filament_info.filament_type == FilamentSpool.TypeEnum.FLX || selectedPrinter.Info.filament_info.filament_type == FilamentSpool.TypeEnum.TGH))
      {
        mFillDensityButtons[FillQuality.Solid].Visible = true;
        if (!density_scroll_list.ChildList.Contains(mFillDensityButtons[FillQuality.Solid]))
        {
          density_scroll_list.AddChildElement(mFillDensityButtons[FillQuality.Solid]);
        }
      }
      else
      {
        mFillDensityButtons[FillQuality.Solid].Visible = false;
        if (density_scroll_list.ChildList.Contains(mFillDensityButtons[FillQuality.Solid]))
        {
          if (mFillDensityButtons[FillQuality.Solid].Checked)
          {
            mFillDensityButtons[SlicerSettings.SupportedFillQualities.Last<FillQuality>()].SetChecked(true);
          }

          density_scroll_list.RemoveChildElement(mFillDensityButtons[FillQuality.Solid]);
        }
      }
      if (mFillDensityButtons[FillQuality.Custom].Checked)
      {
        mFillDensityButtons[FillQuality.Custom].Visible = true;
        if (!density_scroll_list.ChildList.Contains(mFillDensityButtons[FillQuality.Custom]))
        {
          density_scroll_list.AddChildElement(mFillDensityButtons[FillQuality.Custom]);
        }
      }
      else
      {
        mFillDensityButtons[FillQuality.Custom].Visible = false;
        if (density_scroll_list.ChildList.Contains(mFillDensityButtons[FillQuality.Custom]))
        {
          density_scroll_list.RemoveChildElement(mFillDensityButtons[FillQuality.Custom]);
        }
      }
      density_scroll_list.OnParentResize();
    }

    private void CheckSDCardAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
      {
        return;
      }

      if (selected_printer.SDCardExtension.Available)
      {
        untetheredButton_checkbox.Checked = settingsManager.Settings.GetPrintSettingsSafe(selected_printer.MyPrinterProfile.ProfileName).AutoUntetheredSupport;
        sdCheckboxesFrame.Enabled = true;
        sdCheckboxesFrame.Visible = true;
      }
      else
      {
        untetheredButton_checkbox.Checked = false;
        sdCheckboxesFrame.Enabled = false;
        sdCheckboxesFrame.Visible = false;
      }
      if (settingsManager.CurrentAppearanceSettings.AllowSDOnlyPrinting)
      {
        sdOnlyButton_checkbox.Visible = true;
        sdOnlyButton_text.Visible = true;
      }
      else
      {
        sdOnlyButton_checkbox.Visible = false;
        sdOnlyButton_text.Visible = false;
      }
    }

    private void CheckHeatedBedAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
      {
        return;
      }

      var flag = false;
      if (selected_printer.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed && selected_printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        flag = selected_printer.Info.supportedFeatures.Available("Heated Bed Control", selected_printer.MyPrinterProfile.SupportedFeaturesConstants);
      }

      if (flag)
      {
        heatedBedButton_checkbox.Checked = settingsManager.Settings.GetPrintSettingsSafe(selected_printer.MyPrinterProfile.ProfileName).UseHeatedBed;
        heatedBedButton_checkbox.Enabled = true;
        heatedBedButton_checkbox.Visible = true;
        heatedBedButton_text.Visible = true;
      }
      else
      {
        heatedBedButton_checkbox.Checked = false;
        heatedBedButton_checkbox.Enabled = false;
        heatedBedButton_checkbox.Visible = false;
        heatedBedButton_text.Visible = false;
      }
    }

    private void CheckVerifyBedAvailability(PrinterObject selected_printer, bool bSwitchingPrinters)
    {
      if (selected_printer == null || !bSwitchingPrinters)
      {
        return;
      }

      SupportedFeatures.Status status = SupportedFeatures.Status.Available;
      if (selected_printer.Info.supportedFeatures.UsesSupportedFeatures)
      {
        var featureSlot = selected_printer.MyPrinterProfile.SupportedFeaturesConstants.GetFeatureSlot("Single Point Bed Height Calibration");
        if (featureSlot >= 0)
        {
          status = selected_printer.Info.supportedFeatures.GetStatus(featureSlot);
        }
      }
      if (status == SupportedFeatures.Status.Unavailable)
      {
        if (!verifybed_checkbutton.Enabled)
        {
          return;
        }

        verifybed_checkbutton.Checked = false;
        verifybed_checkbutton.Enabled = false;
        verifybed_checkbutton.Visible = false;
        verifybed_text.Visible = false;
      }
      else if (SupportedFeatures.Status.Available == status)
      {
        if (!verifybed_checkbutton.Enabled)
        {
          verifybed_checkbutton.Enabled = true;
          verifybed_checkbutton.Visible = true;
          verifybed_text.Visible = true;
        }
        verifybed_text.Text = "T_PrintDialog_VerifyBed";
      }
      else
      {
        if (!verifybed_checkbutton.Enabled)
        {
          verifybed_checkbutton.Enabled = true;
          verifybed_checkbutton.Visible = true;
          verifybed_text.Visible = true;
        }
        verifybed_checkbutton.Checked = false;
        verifybed_text.Text = "T_PrintDialog_VerifyBedNotRec";
      }
    }

    private void SetSupportEnabledControls(bool bUseSupportEverywhere)
    {
      if (support_checkbutton.Checked)
      {
        support_everywhere.Enabled = true;
        support_everywhere.Checked = bUseSupportEverywhere;
        support_printbedonly.Enabled = true;
        support_printbedonly.Checked = !bUseSupportEverywhere;
      }
      else
      {
        support_everywhere.Checked = false;
        support_everywhere.Enabled = false;
        support_printbedonly.Checked = false;
        support_printbedonly.Enabled = false;
      }
    }

    public void OnPrintButtonPressed(bool bPrintToFile)
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null)
      {
        return;
      }

      var jobOptions = new JobOptions(false);
      CurrentJobDetails.print_to_file = bPrintToFile;
      if (bPrintToFile)
      {
        CurrentJobDetails.printToFileOutputFile = SaveModelFileDialog.RunSaveFileDialog(SaveModelFileDialog.FileType.GCode);
        if (string.IsNullOrEmpty(CurrentJobDetails.printToFileOutputFile))
        {
          PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PrintDialogFrame, CurrentJobDetails);
          return;
        }
      }
      jobOptions.autostart_ignorewarnings = true;
      jobOptions.use_raft_DetailOnly = SlicerSettings.HasRaftEnabled;
      jobOptions.use_wave_bonding = UseWaveBonding.Checked;
      jobOptions.use_fan_preprocessor = SlicerSettings.UsingAutoFanSettings;
      jobOptions.use_support_DetailOnly = support_checkbutton.Checked;
      jobOptions.use_support_everywhere_DetailOnly = support_everywhere.Checked;
      jobOptions.calibrate_before_print = verifybed_checkbutton.Checked;
      jobOptions.calibrate_before_print_z = 0.4f;
      if (selectedPrinter.Info.calibration.UsesCalibrationOffset)
      {
        jobOptions.calibrate_before_print_z += selectedPrinter.Info.calibration.CALIBRATION_OFFSET;
      }

      jobOptions.use_heated_bed = heatedBedButton_checkbox.Checked;
      jobOptions.quality_layer_resolution_DetailOnly = (int)GetPrintQuality();
      jobOptions.fill_density_DetailOnly = (int)GetFillDensity();
      CurrentJobDetails.GenerateSlicerSettings(selectedPrinter, printerview);
      CurrentJobDetails.auto_untethered_print = untetheredButton_checkbox.Checked;
      CurrentJobDetails.sdSaveOnly_print = sdOnlyButton_checkbox.Checked;
      SlicerConnection.SlicerSettingStack.SaveSettingsDown();
      CurrentJobDetails.printer = selectedPrinter;
      CurrentJobDetails.jobOptions = jobOptions;
      PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.PreSlicingFrame, CurrentJobDetails);
    }

    private void ReleasePrinterLock(PrinterObject printer)
    {
      var num = (int) printer.ReleaseLock(null, null);
    }

    private PrintQuality GetPrintQuality()
    {
      PrintQuality printQuality = PrintQuality.FastPrint;
      if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY2"))
      {
        printQuality = PrintQuality.MediumQuality;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY3"))
      {
        printQuality = PrintQuality.FastPrint;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY1"))
      {
        printQuality = PrintQuality.HighQuality;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY4"))
      {
        printQuality = PrintQuality.VeryFastPrint;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY6"))
      {
        printQuality = PrintQuality.VeryHighQuality;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY5"))
      {
        printQuality = PrintQuality.Expert;
      }
      else if (printQuality_editbox.Text == host.Locale.T("T_PRINTQUALITY7"))
      {
        printQuality = PrintQuality.Custom;
      }

      return printQuality;
    }

    private FillQuality GetFillDensity()
    {
      FillQuality fillQuality = FillQuality.HollowThinWalls;
      if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY3"))
      {
        fillQuality = FillQuality.Medium;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY4"))
      {
        fillQuality = FillQuality.Low;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY2"))
      {
        fillQuality = FillQuality.High;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY6"))
      {
        fillQuality = FillQuality.HollowThinWalls;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY5"))
      {
        fillQuality = FillQuality.HollowThickWalls;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY1"))
      {
        fillQuality = FillQuality.ExtraHigh;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY7"))
      {
        fillQuality = FillQuality.Solid;
      }
      else if (fillDensity_editbox.Text == host.Locale.T("T_FILLDENSITY8"))
      {
        fillQuality = FillQuality.Custom;
      }

      return fillQuality;
    }

    public override void OnUpdate()
    {
      if (!Visible)
      {
        return;
      }

      base.OnUpdate();
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter != null && m_bNewPrinterSelected)
      {
        if (print_button != null)
        {
          print_button.Enabled = true;
        }

        if (!mQualityButtonsSet)
        {
          SetFillandQualityButtons();
        }
        else
        {
          ValidatePrintQualityButtons();
          ValidatePrintFillDensityButtons();
        }
        CheckVerifyBedAvailability(selectedPrinter, m_bNewPrinterSelected);
        CheckHeatedBedAvailability(selectedPrinter, m_bNewPrinterSelected);
        CheckSDCardAvailability(selectedPrinter, m_bNewPrinterSelected);
        m_bNewPrinterSelected = false;
        if (quality_scroll_list != null && printQualityPrev_button != null && printQualityNext_button != null)
        {
          if (quality_scroll_list.OnLastElement)
          {
            printQualityNext_button.Enabled = false;
          }
          else
          {
            printQualityNext_button.Enabled = true;
          }

          if (quality_scroll_list.OnFirstElement)
          {
            printQualityPrev_button.Enabled = false;
          }
          else
          {
            printQualityPrev_button.Enabled = true;
          }
        }
        if (density_scroll_list == null || fillDensityPrev_button == null || fillDensityNext_button == null)
        {
          return;
        }

        if (density_scroll_list.OnLastElement)
        {
          fillDensityNext_button.Enabled = false;
        }
        else
        {
          fillDensityNext_button.Enabled = true;
        }

        if (density_scroll_list.OnFirstElement)
        {
          fillDensityPrev_button.Enabled = false;
        }
        else
        {
          fillDensityPrev_button.Enabled = true;
        }
      }
      else
      {
        mQualityButtonsSet = false;
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      var filament = (FilamentSpool) null;
      if (selectedPrinter != null)
      {
        filament = selectedPrinter.GetCurrentFilament();
      }

      switch (button.ID)
      {
        case 109:
          --quality_scroll_list.StartIndex;
          break;
        case 111:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY5");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.Expert, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 112:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY1");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.HighQuality, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 113:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY2");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.MediumQuality, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 114:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY3");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.FastPrint, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 115:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY4");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.VeryFastPrint, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 116:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY6");
          if (!syncing)
          {
            SlicerSettings.SetPrintQuality(PrintQuality.VeryHighQuality, filament, CurrentJobDetails.slicer_objects.Count);
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 117:
          ++quality_scroll_list.StartIndex;
          break;
        case 118:
          printQuality_editbox.Text = host.Locale.T("T_PRINTQUALITY7");
          if (string.IsNullOrEmpty(printQuality_editbox.Text))
          {
            printQuality_editbox.Text = "Custom";
          }

          quality_scroll_list.GotoChild(button.ID);
          break;
        case 218:
          --density_scroll_list.StartIndex;
          break;
        case 220:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY1");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.ExtraHigh);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 221:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY2");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.High);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 222:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY3");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.Medium);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 223:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY4");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.Low);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 224:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY5");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.HollowThickWalls);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 225:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY6");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.HollowThinWalls);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 226:
          ++density_scroll_list.StartIndex;
          break;
        case 227:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY7");
          if (!syncing)
          {
            SlicerSettings.SetFillQuality(FillQuality.Solid);
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 228:
          fillDensity_editbox.Text = host.Locale.T("T_FILLDENSITY8");
          if (string.IsNullOrEmpty(fillDensity_editbox.Text))
          {
            fillDensity_editbox.Text = "Custom";
          }

          density_scroll_list.GotoChild(button.ID);
          break;
        case 301:
          if (syncing)
          {
            break;
          }

          SetSupportEnabledControls(false);
          if (support_checkbutton.Checked)
          {
            support_everywhere.Enabled = true;
            support_printbedonly.Enabled = true;
            support_printbedonly.Checked = true;
            SlicerSettings.EnableSupport(SupportType.LineSupport);
            break;
          }
          SlicerSettings.DisableSupport();
          break;
        case 303:
          if (syncing || !support_everywhere.Checked)
          {
            break;
          }

          SlicerSettings.EnableSupport(SupportType.LineSupportEveryWhere);
          break;
        case 307:
          if (syncing)
          {
            break;
          }

          if (button.Checked)
          {
            SlicerSettings.EnableRaft(filament);
            break;
          }
          SlicerSettings.DisableRaft();
          break;
        case 311:
          if (syncing)
          {
            break;
          }

          if (button.Checked)
          {
            SlicerSettings.EnableSkirt();
            break;
          }
          SlicerSettings.DisableSkirt();
          break;
        case 313:
          if (syncing || !support_printbedonly.Checked)
          {
            break;
          }

          SlicerSettings.EnableSupport(SupportType.LineSupport);
          break;
        case 401:
        case 404:
          if (printerview.ModelLoaded)
          {
            SaveSettings();
            OnPrintButtonPressed(button.ID == 404);
            break;
          }
          message_box.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoModel"));
          break;
        case 402:
          PrintDialogWindow.CloseWindow();
          break;
        case 403:
          ResetSettings();
          LoadSettings();
          break;
        case 501:
          OpenAdvancedPrintSettingsDialog();
          break;
      }
    }

    private void OpenAdvancedPrintSettingsDialog()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null)
      {
        return;
      }

      CurrentJobDetails.printer = selectedPrinter;
      CurrentJobDetails.jobOptions = new JobOptions(false)
      {
        autostart_ignorewarnings = true
      };
      CurrentJobDetails.GenerateSlicerSettings(selectedPrinter, printerview);
      PrintDialogWindow.ActivateFrame(PrintDialogWidgetFrames.AdvancedPrintSettingsFrame, CurrentJobDetails);
    }

    private bool LoadSettings()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.MyPrinterProfile != null)
      {
        settingsManager.LoadSettings();
        SettingsManager.PrintSettings printSettingsSafe = settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
        var childElement1 = (ButtonWidget)FindChildElement(305);
        if (childElement1 != null)
        {
          childElement1.Checked = printSettingsSafe.WaveBonding;
        }

        var childElement2 = (ButtonWidget)FindChildElement(309);
        if (childElement2 != null)
        {
          childElement2.Checked = printSettingsSafe.VerifyBed;
        }

        var childElement3 = (ButtonWidget)FindChildElement(315);
        if (childElement3 != null)
        {
          childElement3.Checked = printSettingsSafe.UseHeatedBed;
        }

        var childElement4 = (ButtonWidget)FindChildElement(317);
        if (childElement4 != null)
        {
          childElement4.Checked = printSettingsSafe.AutoUntetheredSupport;
        }
      }
      return true;
    }

    private void SaveSettings()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.MyPrinterProfile != null)
      {
        SettingsManager.PrintSettings printSettingsSafe = settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
        var childElement1 = (ButtonWidget)FindChildElement(305);
        if (childElement1 != null)
        {
          printSettingsSafe.WaveBonding = childElement1.Checked;
        }

        var childElement2 = (ButtonWidget)FindChildElement(309);
        if (childElement2 != null)
        {
          printSettingsSafe.VerifyBed = childElement2.Checked;
        }

        var childElement3 = (ButtonWidget)FindChildElement(315);
        if (childElement3 != null && childElement3.Enabled)
        {
          printSettingsSafe.UseHeatedBed = childElement3.Checked;
        }

        var childElement4 = (ButtonWidget)FindChildElement(317);
        if (childElement4 != null && childElement4.Enabled)
        {
          printSettingsSafe.AutoUntetheredSupport = childElement4.Checked;
        }
      }
      settingsManager.SaveSettings();
    }

    private void ResetSettings()
    {
      PrinterObject selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter.MyPrinterProfile == null)
      {
        return;
      }

      settingsManager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName).Reset();
      settingsManager.SaveSettings();
      SlicerSettings.SetToDefault();
      SyncFromSlicerSettings();
    }

    public override PrinterObject SelectedPrinter
    {
      get
      {
        PrinterObject selectedPrinter = base.SelectedPrinter;
        if (selectedPrinter == null || selectedPrinter != mPrevPrinter)
        {
          m_bNewPrinterSelected = true;
          mPrevPrinter = selectedPrinter;
          mQualityButtonsSet = false;
        }
        else
        {
          m_bNewPrinterSelected = false;
        }

        return selectedPrinter;
      }
    }
  }
}
