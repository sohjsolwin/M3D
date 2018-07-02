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
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
      if (selectedPrinter.IsPausedorPausing && currentFilament != null && currentFilament.filament_location == FilamentSpool.Location.Internal)
      {
        messagebox.AddMessageToQueue("Sorry, but changes cannot be made to internal spools while paused.");
      }
      else
      {
        selectedPrinter.MarkedAsBusy = true;
        switch (button.ID)
        {
          case 19:
            if (current_spool == null)
            {
              messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please insert filament"));
              break;
            }
            if (settingsManager.CurrentAppearanceSettings.ShowAllWarnings)
            {
              messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning. Changing these filament profiles can cause damage to your printer and are for advanced users only."));
            }

            MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page9_ChangeFilamentDetails, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.SetDetails, current_spool));
            break;
          case 20:
            MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page6_IsThereFilament, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.SetDetails));
            break;
          case 21:
            MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page11_CheatCodePage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.AddFilament));
            break;
          case 22:
            if (!(currentFilament != null))
            {
              break;
            }

            MainWindow.previous_spool = currentFilament;
            MainWindow.LockPrinterAndGotoPage(selectedPrinter, Manage3DInkMainWindow.PageID.Page12_RaisingExtruder, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.RemoveFilament, MainWindow.previous_spool));
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Filament Currently in use:", "", true, false, false, false, false, false);
      var childElement = (Frame)FindChildElement(2);
      if (childElement != null)
      {
        var textWidget1 = new TextWidget(11)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.0f,
          RelativeY = 0.1f,
          Alignment = QFontAlignment.Right,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "Color:"
        };
        childElement.AddChildElement(textWidget1);
        var textWidget2 = new TextWidget(12)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.55f,
          RelativeY = 0.1f,
          Alignment = QFontAlignment.Left,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "purple"
        };
        childElement.AddChildElement(textWidget2);
        var textWidget3 = new TextWidget(13)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.0f,
          RelativeY = 0.3f,
          Alignment = QFontAlignment.Right,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "Material:"
        };
        childElement.AddChildElement(textWidget3);
        var textWidget4 = new TextWidget(14)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.55f,
          RelativeY = 0.3f,
          Alignment = QFontAlignment.Left,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "PLA"
        };
        childElement.AddChildElement(textWidget4);
        var textWidget5 = new TextWidget(15)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.0f,
          RelativeY = 0.5f,
          Alignment = QFontAlignment.Right,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "Temperature:"
        };
        childElement.AddChildElement(textWidget5);
        var textWidget6 = new TextWidget(16)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.55f,
          RelativeY = 0.5f,
          Alignment = QFontAlignment.Left,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "9001"
        };
        childElement.AddChildElement(textWidget6);
        var textWidget7 = new TextWidget(17)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.0f,
          RelativeY = 0.7f,
          Alignment = QFontAlignment.Right,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "3D Ink Used (mm):"
        };
        childElement.AddChildElement(textWidget7);
        var textWidget8 = new TextWidget(18)
        {
          Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
          RelativeWidth = 0.45f,
          RelativeHeight = 0.2f,
          RelativeX = 0.55f,
          RelativeY = 0.7f,
          Alignment = QFontAlignment.Left,
          VAlignment = TextVerticalAlignment.Middle,
          Text = "0"
        };
        childElement.AddChildElement(textWidget8);
        var buttonWidget1 = new ButtonWidget(19);
        buttonWidget1.Init(Host, "guicontrols", 194f, 1f, 253f, 64f, 194f, 65f, 253f, 128f, 194f, 129f, 253f, 192f);
        buttonWidget1.Size = FontSize.Large;
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetSize(60, 60);
        buttonWidget1.SetPosition(-60, 0);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement(buttonWidget1);
        var buttonWidget2 = new ButtonWidget(20);
        buttonWidget2.Init(Host, "guicontrols", 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f, 200f, 705f, 220f, 725f);
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
        childElement.AddChildElement(buttonWidget2);
        var buttonWidget3 = new ButtonWidget(21);
        buttonWidget3.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget3.Size = FontSize.Medium;
        buttonWidget3.Text = "INSERT FILAMENT";
        buttonWidget3.SetGrowableWidth(4, 4, 32);
        buttonWidget3.SetGrowableHeight(4, 4, 32);
        buttonWidget3.SetSize(192, 60);
        buttonWidget3.SetPosition(-270, -100);
        buttonWidget3.CenterHorizontallyInParent = true;
        buttonWidget3.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        AddChildElement(buttonWidget3);
        var buttonWidget4 = new ButtonWidget(22);
        buttonWidget4.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget4.Size = FontSize.Medium;
        buttonWidget4.Text = "REMOVE FILAMENT";
        buttonWidget4.SetGrowableWidth(4, 4, 32);
        buttonWidget4.SetGrowableHeight(4, 4, 32);
        buttonWidget4.SetSize(192, 60);
        buttonWidget4.SetPosition(-270, -100);
        buttonWidget4.CenterHorizontallyInParent = true;
        buttonWidget4.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        AddChildElement(buttonWidget4);
        var spriteAnimationWidget = new SpriteAnimationWidget(23);
        spriteAnimationWidget.Init(Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
        spriteAnimationWidget.SetPosition(0, -100);
        spriteAnimationWidget.SetSize(96, 81);
        spriteAnimationWidget.CenterHorizontallyInParent = true;
        AddChildElement(spriteAnimationWidget);
      }
      PopulateStartupControlsList();
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      OnUpdate();
    }

    public override void OnUpdate()
    {
      if (Visible)
      {
        try
        {
          PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
          DisableAllControls();
          ShowCurrentDetails();
          if (selectedPrinter == null)
          {
            return;
          }

          if (!IsPrinterInErrorState)
          {
            FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
            current_spool = !(currentFilament != null) || currentFilament.filament_type == FilamentSpool.TypeEnum.NoFilament ? null : new FilamentSpool(currentFilament);
            if (!selectedPrinter.isBusy)
            {
              if (current_spool == null)
              {
                correct_button.Visible = true;
                correct_button.Enabled = true;
                add_button.Visible = true;
                add_button.Enabled = true;
                settings_button.Visible = true;
                settings_button.Enabled = true;
              }
              else
              {
                correct_button.Visible = true;
                remove_button.Enabled = true;
                remove_button.Visible = true;
                settings_button.Enabled = true;
                settings_button.Visible = true;
              }
            }
            else
            {
              progress_busy.Visible = true;
            }
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
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (ShowError(selectedPrinter))
      {
        return;
      }

      selectedPrinter.MarkedAsBusy = false;
      FilamentSpool filamentSpool = selectedPrinter.GetCurrentFilament();
      if (filamentSpool != null)
      {
        filamentSpool = new FilamentSpool(filamentSpool);
      }

      if (filamentSpool == null)
      {
        text_title.Text = "3D Ink Currently in use:";
        text_main.Text = selectedPrinter.isBusy || selectedPrinter.Info.Status == PrinterStatus.Connecting ? (selectedPrinter.Info.current_job == null ? "Unable to read information from the printer because it is working." : "Unable to read information from the printer because it is printing.") : "Looks like your printer doesn't have 3D Ink loaded.";
        DisableAllControls();
        text_main.Visible = true;
      }
      else
      {
        ShowCurrentFilament(selectedPrinter, filamentSpool);
      }
    }

    private bool ShowError(PrinterObject printer)
    {
      printerInErrorState = false;
      if (printer == null || printer.PrinterState == PrinterObject.State.IsUpdatingFirmware || (printer.PrinterState == PrinterObject.State.IsCalibrating || printer.PrinterState == PrinterObject.State.IsNotHealthy))
      {
        if (printer == null)
        {
          text_main.Text = "Sorry, but a printer has not been connected.";
          text_title.Text = "3D Ink Currently in use:";
        }
        else if (printer.PrinterState == PrinterObject.State.IsCalibrating)
        {
          text_main.Text = "Please wait while your printer calibrates.";
          text_title.Text = "";
        }
        else if (printer.PrinterState == PrinterObject.State.IsUpdatingFirmware)
        {
          text_main.Text = "Please wait while your firmware is updating.";
          text_title.Text = "";
        }
        else if (printer.PrinterState == PrinterObject.State.IsNotHealthy)
        {
          text_main.Text = "Your printer is having problems. You may have to disconnect.";
          text_title.Text = "";
        }
        else
        {
          printer.MarkedAsBusy = false;
        }

        DisableAllControls();
        text_main.Visible = true;
        printerInErrorState = true;
      }
      else
      {
        printerInErrorState = false;
      }

      return printerInErrorState;
    }

    private void ShowCurrentFilament(PrinterObject printer, FilamentSpool current_spool)
    {
      text_title.Text = "3D Ink Currently in use:\n\n" + FilamentProfile.GenerateSpoolName(current_spool, true);
      text_temperature_value.Text = current_spool.filament_temperature.ToString() + " Degrees C";
      FilamentConstants.Branding brandingFrom = FilamentConstants.GetBrandingFrom(current_spool.filament_type, (FilamentConstants.ColorsEnum) current_spool.filament_color_code);
      text_material_value.Text = brandingFrom == FilamentConstants.Branding.Other ? FilamentConstants.TypesToString(current_spool.filament_type) : FilamentConstants.TypesToString(current_spool.filament_type) + " " + FilamentConstants.BrandingToString(brandingFrom);
      text_color_value.Text = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) current_spool.filament_color_code);
      text_main.Text = "";
      text_3dInkAmount_value.Text = current_spool.estimated_filament_length_printed.ToString("0.00");
      text_temperature_value.Visible = true;
      text_material_value.Visible = true;
      text_color_value.Visible = true;
      text_temperature.Visible = true;
      text_material.Visible = true;
      text_color.Visible = true;
      if (!settingsManager.CurrentFilamentSettings.TrackFilament)
      {
        return;
      }

      text_3dInkAmount_value.Visible = true;
      text_3dInkAmount.Visible = true;
    }

    private void PopulateStartupControlsList()
    {
      text_color_value = (TextWidget)FindChildElement(12);
      text_material_value = (TextWidget)FindChildElement(14);
      text_temperature_value = (TextWidget)FindChildElement(16);
      text_color = (TextWidget)FindChildElement(11);
      text_material = (TextWidget)FindChildElement(13);
      text_temperature = (TextWidget)FindChildElement(15);
      text_3dInkAmount_value = (TextWidget)FindChildElement(18);
      text_3dInkAmount = (TextWidget)FindChildElement(17);
      text_main = (TextWidget)FindChildElement(3);
      text_title = (TextWidget)FindChildElement(1);
      cancel_button = (ButtonWidget)FindChildElement(9);
      progress_busy = (SpriteAnimationWidget)FindChildElement(23);
      add_button = (ButtonWidget)FindChildElement(21);
      remove_button = (ButtonWidget)FindChildElement(22);
      correct_button = (ButtonWidget)FindChildElement(20);
      settings_button = (ButtonWidget)FindChildElement(19);
    }

    public void DisableAllControls()
    {
      add_button.Visible = false;
      add_button.Enabled = false;
      remove_button.Enabled = false;
      remove_button.Visible = false;
      correct_button.Visible = false;
      settings_button.Visible = false;
      settings_button.Enabled = false;
      progress_busy.Visible = false;
      text_main.Visible = false;
      text_temperature_value.Visible = false;
      text_material_value.Visible = false;
      text_color_value.Visible = false;
      text_temperature.Visible = false;
      text_material.Visible = false;
      text_color.Visible = false;
      text_3dInkAmount_value.Visible = false;
      text_3dInkAmount.Visible = false;
      cancel_button.Visible = false;
      cancel_button.Enabled = false;
    }

    public bool IsPrinterInErrorState
    {
      get
      {
        return printerInErrorState;
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
