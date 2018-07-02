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
  internal class FilamentChangeFilamentDetails : Manage3DInkChildWindow
  {
    private ThreadSafeVariable<bool> filamentset = new ThreadSafeVariable<bool>(false);
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;
    private EditBoxWidget custom_temperature_edit;
    private TextWidget textColor;
    private TextWidget textMaterial;

    public FilamentChangeFilamentDetails(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
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

      switch (button.ID)
      {
        case 18:
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          break;
        case 19:
          TemperatureEditEnterCallback(custom_temperature_edit);
          break;
        case 20:
          custom_temperature_edit.Text = FilamentConstants.Temperature.Default(selectedPrinter.GetCurrentFilament().filament_type).ToString();
          break;
      }
    }

    public override void Init()
    {
      var color4_1 = new Color4(246, 246, 246, byte.MaxValue);
      var color4_2 = new Color4(220, 220, 220, byte.MaxValue);
      var color4_3 = new Color4(0.15f, 0.15f, 0.15f, 1f);
      var frame = new Frame(0)
      {
        BorderColor = color4_2,
        BGColor = color4_1
      };
      var textWidget1 = new TextWidget(11)
      {
        Color = color4_3
      };
      textWidget1.SetSize(500, 50);
      textWidget1.SetPosition(0, 25);
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.VAlignment = TextVerticalAlignment.Middle;
      textWidget1.CenterHorizontallyInParent = true;
      textWidget1.Text = "Change Current Temperature Settings:";
      AddChildElement(textWidget1);
      var textWidget2 = new TextWidget(12)
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
      frame.AddChildElement(textWidget2);
      textColor = new TextWidget(14)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        RelativeWidth = 0.45f,
        RelativeHeight = 0.2f,
        RelativeX = 0.55f,
        RelativeY = 0.1f,
        Alignment = QFontAlignment.Left,
        VAlignment = TextVerticalAlignment.Middle,
        Text = ""
      };
      frame.AddChildElement(textColor);
      var textWidget3 = new TextWidget(15)
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
      frame.AddChildElement(textWidget3);
      textMaterial = new TextWidget(16)
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
      frame.AddChildElement(textMaterial);
      var textWidget4 = new TextWidget(17)
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
      frame.AddChildElement(textWidget4);
      custom_temperature_edit = new EditBoxWidget(13);
      custom_temperature_edit.Init(Host, "guicontrols", 898f, 104f, 941f, 135f);
      custom_temperature_edit.SetGrowableWidth(3, 3, 32);
      custom_temperature_edit.Text = "";
      custom_temperature_edit.Enabled = true;
      custom_temperature_edit.SetSize(100, 24);
      custom_temperature_edit.Color = color4_3;
      custom_temperature_edit.SetTextWindowBorders(4, 4, 4, 4);
      custom_temperature_edit.RelativeX = 0.55f;
      custom_temperature_edit.RelativeY = 0.55f;
      custom_temperature_edit.SetVisible(true);
      custom_temperature_edit.SetCallbackEnterKey(new EditBoxWidget.EditBoxCallback(TemperatureEditEnterCallback));
      frame.AddChildElement(custom_temperature_edit);
      var buttonWidget1 = new ButtonWidget(20);
      buttonWidget1.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget1.Size = FontSize.Medium;
      buttonWidget1.Text = "Reset";
      buttonWidget1.SetSize(70, 24);
      buttonWidget1.RelativeX = 0.63f;
      buttonWidget1.RelativeY = 0.55f;
      buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      frame.AddChildElement(buttonWidget1);
      var buttonWidget2 = new ButtonWidget(18, this);
      buttonWidget2.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget2.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget2.Size = FontSize.Large;
      buttonWidget2.SetSize(100, 32);
      buttonWidget2.RelativeX = 0.35f;
      buttonWidget2.RelativeY = 0.8f;
      buttonWidget2.SetGrowableHeight(4, 4, 32);
      buttonWidget2.SetGrowableWidth(4, 4, 32);
      buttonWidget2.Text = "Cancel";
      buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      AddChildElement(buttonWidget2);
      var buttonWidget3 = new ButtonWidget(19, this);
      buttonWidget3.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget3.Color = color4_3;
      buttonWidget3.Size = FontSize.Large;
      buttonWidget3.SetSize(100, 32);
      buttonWidget3.RelativeX = 0.55f;
      buttonWidget3.RelativeY = 0.8f;
      buttonWidget3.SetGrowableHeight(4, 4, 32);
      buttonWidget3.SetGrowableWidth(4, 4, 32);
      buttonWidget3.Text = "Save";
      buttonWidget3.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      AddChildElement(buttonWidget3);
      SetPosition(0, 0);
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      frame.RelativeX = 0.0f;
      frame.RelativeY = 0.2f;
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.5f;
      AddChildElement(frame);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      filamentset.Value = false;
      Host.SetFocus(custom_temperature_edit);
      custom_temperature_edit.Text = details.current_spool.filament_temperature.ToString();
      textColor.Text = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), CurrentDetails.current_spool.filament_color_code));
      textMaterial.Text = CurrentDetails.current_spool.filament_type.ToString();
    }

    public void TemperatureEditEnterCallback(EditBoxWidget edit)
    {
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      if (int.TryParse(custom_temperature_edit.Text.ToString(), out var result))
      {
        var min = (int)FilamentConstants.Temperature.MaxMinForFilamentType(CurrentDetails.current_spool.filament_type).Min;
        var num1 = 260;
        if (result >= min && result <= num1)
        {
          messagebox.AddMessageToQueue(string.Format("Changing the current temperature settings from {0} to {1}.", CurrentDetails.current_spool.filament_temperature, custom_temperature_edit.Text), PopupMessageBox.MessageBoxButtons.OK);
          var filamentSpool = new FilamentSpool(CurrentDetails.current_spool)
          {
            filament_temperature = result
          };
          CurrentDetails.current_spool = filamentSpool;
          CurrentDetails.waitCondition = new Mangage3DInkStageDetails.WaitCondition(WaitCondition);
          CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
          FilamentWaitingPage.CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
          settingsManager.FilamentDictionary.AddCustomTemperature(filamentSpool.filament_type, (FilamentConstants.ColorsEnum)Enum.ToObject(typeof(FilamentConstants.ColorsEnum), filamentSpool.filament_color_code), filamentSpool.filament_temperature);
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, CurrentDetails);
          var num2 = (int)selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(SetFilamentAfterLock), selectedPrinter);
        }
        else
        {
          messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a temperature from " + min + " to " + num1));
        }
      }
      else
      {
        messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a valid temperature value"));
      }
    }

    private void SetFilamentAfterLock(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as PrinterObject;
      if (asyncState.CheckForLockError(ar))
      {
        if (settingsManager != null)
        {
          settingsManager.AssociateFilamentToPrinter(asyncState.Info.serial_number, CurrentDetails.current_spool);
          settingsManager.SaveSettings();
        }
        var num = (int) asyncState.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(AfterFilamentSet), asyncState, CurrentDetails.current_spool);
      }
      else
      {
        MainWindow.ResetToStartup();
      }
    }

    private void AfterFilamentSet(IAsyncCallResult ar)
    {
      (ar.AsyncState as PrinterObject).MarkedAsBusy = false;
      filamentset.Value = true;
    }

    private bool WaitCondition()
    {
      return filamentset.Value;
    }

    public enum ControlIDs
    {
      TextTitle = 11, // 0x0000000B
      TextColor = 12, // 0x0000000C
      TemperatureEdit = 13, // 0x0000000D
      TextColorValue = 14, // 0x0000000E
      TextMaterial = 15, // 0x0000000F
      TextMaterialValue = 16, // 0x00000010
      TextTemperature = 17, // 0x00000011
      BackButton = 18, // 0x00000012
      Apply = 19, // 0x00000013
      DefaultTemp = 20, // 0x00000014
    }
  }
}
