// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentStartupPage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentStartupPage : Manage3DInkChildWindow
  {
    private SpriteAnimationWidget progress_busy;
    private ButtonWidget add_button;
    private ButtonWidget remove_button;
    private ButtonWidget correct_button;
    private ButtonWidget settings_button;
    private TextWidget text_color_value;
    private TextWidget text_material_value;
    private TextWidget text_temperature_value;
    private TextWidget text_color;
    private TextWidget text_material;
    private TextWidget text_temperature;
    private TextWidget text_main;
    private TextWidget text_title;
    private TextWidget text_3dInkAmount_value;
    private TextWidget text_3dInkAmount;
    private ButtonWidget cancel_button;
    private bool printerInErrorState;
    private FilamentSpool current_spool;
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;

    public FilamentStartupPage(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
      this.messagebox = messagebox;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
      if (selectedPrinter.IsPausedorPausing && currentFilament != (FilamentSpool) null && currentFilament.filament_location == FilamentSpool.Location.Internal)
      {
        this.messagebox.AddMessageToQueue("Sorry, but changes cannot be made to internal spools while paused.");
      }
      else
      {
        selectedPrinter.MarkedAsBusy = true;
        switch (button.ID)
        {
          case 19:
            if (this.current_spool == (FilamentSpool) null)
            {
              this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please insert filament"));
              break;
            }
            if (this.settingsManager.CurrentAppearanceSettings.ShowAllWarnings)
              this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning. Changing these filament profiles can cause damage to your printer and are for advanced users only."));
            this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page9_ChangeFilamentDetails, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.SetDetails, this.current_spool));
            break;
          case 20:
            this.MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page6_IsThereFilament, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.SetDetails));
            break;
          case 21:
            this.MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page11_CheatCodePage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.AddFilament));
            break;
          case 22:
            if (!(currentFilament != (FilamentSpool) null))
              break;
            this.MainWindow.previous_spool = currentFilament;
            this.MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page12_RaisingExtruder, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.RemoveFilament, this.MainWindow.previous_spool));
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Filament Currently in use:", "", true, false, false, false, false, false);
      Frame childElement = (Frame) this.FindChildElement(2);
      if (childElement != null)
      {
        TextWidget textWidget1 = new TextWidget(11);
        textWidget1.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget1.RelativeWidth = 0.45f;
        textWidget1.RelativeHeight = 0.2f;
        textWidget1.RelativeX = 0.0f;
        textWidget1.RelativeY = 0.1f;
        textWidget1.Alignment = QFontAlignment.Right;
        textWidget1.VAlignment = TextVerticalAlignment.Middle;
        textWidget1.Text = "Color:";
        childElement.AddChildElement((Element2D) textWidget1);
        TextWidget textWidget2 = new TextWidget(12);
        textWidget2.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget2.RelativeWidth = 0.45f;
        textWidget2.RelativeHeight = 0.2f;
        textWidget2.RelativeX = 0.55f;
        textWidget2.RelativeY = 0.1f;
        textWidget2.Alignment = QFontAlignment.Left;
        textWidget2.VAlignment = TextVerticalAlignment.Middle;
        textWidget2.Text = "purple";
        childElement.AddChildElement((Element2D) textWidget2);
        TextWidget textWidget3 = new TextWidget(13);
        textWidget3.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget3.RelativeWidth = 0.45f;
        textWidget3.RelativeHeight = 0.2f;
        textWidget3.RelativeX = 0.0f;
        textWidget3.RelativeY = 0.3f;
        textWidget3.Alignment = QFontAlignment.Right;
        textWidget3.VAlignment = TextVerticalAlignment.Middle;
        textWidget3.Text = "Material:";
        childElement.AddChildElement((Element2D) textWidget3);
        TextWidget textWidget4 = new TextWidget(14);
        textWidget4.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget4.RelativeWidth = 0.45f;
        textWidget4.RelativeHeight = 0.2f;
        textWidget4.RelativeX = 0.55f;
        textWidget4.RelativeY = 0.3f;
        textWidget4.Alignment = QFontAlignment.Left;
        textWidget4.VAlignment = TextVerticalAlignment.Middle;
        textWidget4.Text = "PLA";
        childElement.AddChildElement((Element2D) textWidget4);
        TextWidget textWidget5 = new TextWidget(15);
        textWidget5.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget5.RelativeWidth = 0.45f;
        textWidget5.RelativeHeight = 0.2f;
        textWidget5.RelativeX = 0.0f;
        textWidget5.RelativeY = 0.5f;
        textWidget5.Alignment = QFontAlignment.Right;
        textWidget5.VAlignment = TextVerticalAlignment.Middle;
        textWidget5.Text = "Temperature:";
        childElement.AddChildElement((Element2D) textWidget5);
        TextWidget textWidget6 = new TextWidget(16);
        textWidget6.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget6.RelativeWidth = 0.45f;
        textWidget6.RelativeHeight = 0.2f;
        textWidget6.RelativeX = 0.55f;
        textWidget6.RelativeY = 0.5f;
        textWidget6.Alignment = QFontAlignment.Left;
        textWidget6.VAlignment = TextVerticalAlignment.Middle;
        textWidget6.Text = "9001";
        childElement.AddChildElement((Element2D) textWidget6);
        TextWidget textWidget7 = new TextWidget(17);
        textWidget7.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget7.RelativeWidth = 0.45f;
        textWidget7.RelativeHeight = 0.2f;
        textWidget7.RelativeX = 0.0f;
        textWidget7.RelativeY = 0.7f;
        textWidget7.Alignment = QFontAlignment.Right;
        textWidget7.VAlignment = TextVerticalAlignment.Middle;
        textWidget7.Text = "3D Ink Used (mm):";
        childElement.AddChildElement((Element2D) textWidget7);
        TextWidget textWidget8 = new TextWidget(18);
        textWidget8.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
        textWidget8.RelativeWidth = 0.45f;
        textWidget8.RelativeHeight = 0.2f;
        textWidget8.RelativeX = 0.55f;
        textWidget8.RelativeY = 0.7f;
        textWidget8.Alignment = QFontAlignment.Left;
        textWidget8.VAlignment = TextVerticalAlignment.Middle;
        textWidget8.Text = "0";
        childElement.AddChildElement((Element2D) textWidget8);
        ButtonWidget buttonWidget1 = new ButtonWidget(19);
        buttonWidget1.Init(this.Host, "guicontrols", 194f, 1f, 253f, 64f, 194f, 65f, 253f, 128f, 194f, 129f, 253f, 192f);
        buttonWidget1.Size = FontSize.Large;
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetSize(60, 60);
        buttonWidget1.SetPosition(-60, 0);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget1);
        ButtonWidget buttonWidget2 = new ButtonWidget(20);
        buttonWidget2.Init(this.Host, "guicontrols", 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "CLICK HERE IF NOT CORRECT.";
        buttonWidget2.SetGrowableWidth(4, 4, 32);
        buttonWidget2.SetGrowableHeight(4, 4, 32);
        buttonWidget2.SetSize(300, 40);
        buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        buttonWidget2.SetPosition(-300, -40);
        buttonWidget2.TextColor = new Color4(0.3529412f, 0.7450981f, 0.8627451f, 1f);
        buttonWidget2.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
        buttonWidget2.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
        childElement.AddChildElement((Element2D) buttonWidget2);
        ButtonWidget buttonWidget3 = new ButtonWidget(21);
        buttonWidget3.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget3.Size = FontSize.Medium;
        buttonWidget3.Text = "INSERT FILAMENT";
        buttonWidget3.SetGrowableWidth(4, 4, 32);
        buttonWidget3.SetGrowableHeight(4, 4, 32);
        buttonWidget3.SetSize(192, 60);
        buttonWidget3.SetPosition(-270, -100);
        buttonWidget3.CenterHorizontallyInParent = true;
        buttonWidget3.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        this.AddChildElement((Element2D) buttonWidget3);
        ButtonWidget buttonWidget4 = new ButtonWidget(22);
        buttonWidget4.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget4.Size = FontSize.Medium;
        buttonWidget4.Text = "REMOVE FILAMENT";
        buttonWidget4.SetGrowableWidth(4, 4, 32);
        buttonWidget4.SetGrowableHeight(4, 4, 32);
        buttonWidget4.SetSize(192, 60);
        buttonWidget4.SetPosition(-270, -100);
        buttonWidget4.CenterHorizontallyInParent = true;
        buttonWidget4.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        this.AddChildElement((Element2D) buttonWidget4);
        SpriteAnimationWidget spriteAnimationWidget = new SpriteAnimationWidget(23);
        spriteAnimationWidget.Init(this.Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
        spriteAnimationWidget.SetPosition(0, -100);
        spriteAnimationWidget.SetSize(96, 81);
        spriteAnimationWidget.CenterHorizontallyInParent = true;
        this.AddChildElement((Element2D) spriteAnimationWidget);
      }
      this.PopulateStartupControlsList();
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.OnUpdate();
    }

    public override void OnUpdate()
    {
      if (this.Visible)
      {
        try
        {
          PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
          this.DisableAllControls();
          this.ShowCurrentDetails();
          if (selectedPrinter == null)
            return;
          if (!this.IsPrinterInErrorState)
          {
            FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
            this.current_spool = !(currentFilament != (FilamentSpool) null) || currentFilament.filament_type == FilamentSpool.TypeEnum.NoFilament ? (FilamentSpool) null : new FilamentSpool(currentFilament);
            if (!selectedPrinter.isBusy)
            {
              if (this.current_spool == (FilamentSpool) null)
              {
                this.correct_button.Visible = true;
                this.correct_button.Enabled = true;
                this.add_button.Visible = true;
                this.add_button.Enabled = true;
                this.settings_button.Visible = true;
                this.settings_button.Enabled = true;
              }
              else
              {
                this.correct_button.Visible = true;
                this.remove_button.Enabled = true;
                this.remove_button.Visible = true;
                this.settings_button.Enabled = true;
                this.settings_button.Visible = true;
              }
            }
            else
              this.progress_busy.Visible = true;
          }
        }
        catch (Exception ex)
        {
        }
      }
      base.OnUpdate();
    }

    private void ShowCurrentDetails()
    {
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (this.ShowError(selectedPrinter))
        return;
      selectedPrinter.MarkedAsBusy = false;
      FilamentSpool filamentSpool = selectedPrinter.GetCurrentFilament();
      if (filamentSpool != (FilamentSpool) null)
        filamentSpool = new FilamentSpool(filamentSpool);
      if (filamentSpool == (FilamentSpool) null)
      {
        this.text_title.Text = "3D Ink Currently in use:";
        this.text_main.Text = selectedPrinter.isBusy || selectedPrinter.Info.Status == PrinterStatus.Connecting ? (selectedPrinter.Info.current_job == null ? "Unable to read information from the printer because it is working." : "Unable to read information from the printer because it is printing.") : "Looks like your printer doesn't have 3D Ink loaded.";
        this.DisableAllControls();
        this.text_main.Visible = true;
      }
      else
        this.ShowCurrentFilament(selectedPrinter, filamentSpool);
    }

    private bool ShowError(PrinterObject printer)
    {
      this.printerInErrorState = false;
      if (printer == null || printer.PrinterState == PrinterObject.State.IsUpdatingFirmware || (printer.PrinterState == PrinterObject.State.IsCalibrating || printer.PrinterState == PrinterObject.State.IsNotHealthy))
      {
        if (printer == null)
        {
          this.text_main.Text = "Sorry, but a printer has not been connected.";
          this.text_title.Text = "3D Ink Currently in use:";
        }
        else if (printer.PrinterState == PrinterObject.State.IsCalibrating)
        {
          this.text_main.Text = "Please wait while your printer calibrates.";
          this.text_title.Text = "";
        }
        else if (printer.PrinterState == PrinterObject.State.IsUpdatingFirmware)
        {
          this.text_main.Text = "Please wait while your firmware is updating.";
          this.text_title.Text = "";
        }
        else if (printer.PrinterState == PrinterObject.State.IsNotHealthy)
        {
          this.text_main.Text = "Your printer is having problems. You may have to disconnect.";
          this.text_title.Text = "";
        }
        else
          printer.MarkedAsBusy = false;
        this.DisableAllControls();
        this.text_main.Visible = true;
        this.printerInErrorState = true;
      }
      else
        this.printerInErrorState = false;
      return this.printerInErrorState;
    }

    private void ShowCurrentFilament(PrinterObject printer, FilamentSpool current_spool)
    {
      this.text_title.Text = "3D Ink Currently in use:\n\n" + FilamentProfile.GenerateSpoolName(current_spool, true);
      this.text_temperature_value.Text = current_spool.filament_temperature.ToString() + " Degrees C";
      FilamentConstants.Branding brandingFrom = FilamentConstants.GetBrandingFrom(current_spool.filament_type, (FilamentConstants.ColorsEnum) current_spool.filament_color_code);
      this.text_material_value.Text = brandingFrom == FilamentConstants.Branding.Other ? FilamentConstants.TypesToString(current_spool.filament_type) : FilamentConstants.TypesToString(current_spool.filament_type) + " " + FilamentConstants.BrandingToString(brandingFrom);
      this.text_color_value.Text = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) current_spool.filament_color_code);
      this.text_main.Text = "";
      this.text_3dInkAmount_value.Text = current_spool.estimated_filament_length_printed.ToString("0.00");
      this.text_temperature_value.Visible = true;
      this.text_material_value.Visible = true;
      this.text_color_value.Visible = true;
      this.text_temperature.Visible = true;
      this.text_material.Visible = true;
      this.text_color.Visible = true;
      if (!this.settingsManager.CurrentFilamentSettings.TrackFilament)
        return;
      this.text_3dInkAmount_value.Visible = true;
      this.text_3dInkAmount.Visible = true;
    }

    private void PopulateStartupControlsList()
    {
      this.text_color_value = (TextWidget) this.FindChildElement(12);
      this.text_material_value = (TextWidget) this.FindChildElement(14);
      this.text_temperature_value = (TextWidget) this.FindChildElement(16);
      this.text_color = (TextWidget) this.FindChildElement(11);
      this.text_material = (TextWidget) this.FindChildElement(13);
      this.text_temperature = (TextWidget) this.FindChildElement(15);
      this.text_3dInkAmount_value = (TextWidget) this.FindChildElement(18);
      this.text_3dInkAmount = (TextWidget) this.FindChildElement(17);
      this.text_main = (TextWidget) this.FindChildElement(3);
      this.text_title = (TextWidget) this.FindChildElement(1);
      this.cancel_button = (ButtonWidget) this.FindChildElement(9);
      this.progress_busy = (SpriteAnimationWidget) this.FindChildElement(23);
      this.add_button = (ButtonWidget) this.FindChildElement(21);
      this.remove_button = (ButtonWidget) this.FindChildElement(22);
      this.correct_button = (ButtonWidget) this.FindChildElement(20);
      this.settings_button = (ButtonWidget) this.FindChildElement(19);
    }

    public void DisableAllControls()
    {
      this.add_button.Visible = false;
      this.add_button.Enabled = false;
      this.remove_button.Enabled = false;
      this.remove_button.Visible = false;
      this.correct_button.Visible = false;
      this.settings_button.Visible = false;
      this.settings_button.Enabled = false;
      this.progress_busy.Visible = false;
      this.text_main.Visible = false;
      this.text_temperature_value.Visible = false;
      this.text_material_value.Visible = false;
      this.text_color_value.Visible = false;
      this.text_temperature.Visible = false;
      this.text_material.Visible = false;
      this.text_color.Visible = false;
      this.text_3dInkAmount_value.Visible = false;
      this.text_3dInkAmount.Visible = false;
      this.cancel_button.Visible = false;
      this.cancel_button.Enabled = false;
    }

    public bool IsPrinterInErrorState
    {
      get
      {
        return this.printerInErrorState;
      }
    }

    public enum ControlIDs
    {
      TextColor = 11, // 0x0000000B
      TextColorValue = 12, // 0x0000000C
      TextMaterial = 13, // 0x0000000D
      TextMaterialValue = 14, // 0x0000000E
      TextTemperature = 15, // 0x0000000F
      TextTemperatureValue = 16, // 0x00000010
      Text3DInkUsed = 17, // 0x00000011
      Text3DInkUsedValue = 18, // 0x00000012
      Settings = 19, // 0x00000013
      CorrectButton = 20, // 0x00000014
      AddFilament = 21, // 0x00000015
      RemoveFilament = 22, // 0x00000016
      StartupBusyText = 23, // 0x00000017
    }
  }
}
