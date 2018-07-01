// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentChangeFilamentDetails
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
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      switch (button.ID)
      {
        case 18:
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
          break;
        case 19:
          this.TemperatureEditEnterCallback(this.custom_temperature_edit);
          break;
        case 20:
          this.custom_temperature_edit.Text = FilamentConstants.Temperature.Default(selectedPrinter.GetCurrentFilament().filament_type).ToString();
          break;
      }
    }

    public override void Init()
    {
      Color4 color4_1 = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      Color4 color4_2 = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      Color4 color4_3 = new Color4(0.15f, 0.15f, 0.15f, 1f);
      Frame frame = new Frame(0);
      frame.BorderColor = color4_2;
      frame.BGColor = color4_1;
      TextWidget textWidget1 = new TextWidget(11);
      textWidget1.Color = color4_3;
      textWidget1.SetSize(500, 50);
      textWidget1.SetPosition(0, 25);
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.VAlignment = TextVerticalAlignment.Middle;
      textWidget1.CenterHorizontallyInParent = true;
      textWidget1.Text = "Change Current Temperature Settings:";
      this.AddChildElement((Element2D) textWidget1);
      TextWidget textWidget2 = new TextWidget(12);
      textWidget2.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget2.RelativeWidth = 0.45f;
      textWidget2.RelativeHeight = 0.2f;
      textWidget2.RelativeX = 0.0f;
      textWidget2.RelativeY = 0.1f;
      textWidget2.Alignment = QFontAlignment.Right;
      textWidget2.VAlignment = TextVerticalAlignment.Middle;
      textWidget2.Text = "Color:";
      frame.AddChildElement((Element2D) textWidget2);
      this.textColor = new TextWidget(14);
      this.textColor.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      this.textColor.RelativeWidth = 0.45f;
      this.textColor.RelativeHeight = 0.2f;
      this.textColor.RelativeX = 0.55f;
      this.textColor.RelativeY = 0.1f;
      this.textColor.Alignment = QFontAlignment.Left;
      this.textColor.VAlignment = TextVerticalAlignment.Middle;
      this.textColor.Text = "";
      frame.AddChildElement((Element2D) this.textColor);
      TextWidget textWidget3 = new TextWidget(15);
      textWidget3.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget3.RelativeWidth = 0.45f;
      textWidget3.RelativeHeight = 0.2f;
      textWidget3.RelativeX = 0.0f;
      textWidget3.RelativeY = 0.3f;
      textWidget3.Alignment = QFontAlignment.Right;
      textWidget3.VAlignment = TextVerticalAlignment.Middle;
      textWidget3.Text = "Material:";
      frame.AddChildElement((Element2D) textWidget3);
      this.textMaterial = new TextWidget(16);
      this.textMaterial.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      this.textMaterial.RelativeWidth = 0.45f;
      this.textMaterial.RelativeHeight = 0.2f;
      this.textMaterial.RelativeX = 0.55f;
      this.textMaterial.RelativeY = 0.3f;
      this.textMaterial.Alignment = QFontAlignment.Left;
      this.textMaterial.VAlignment = TextVerticalAlignment.Middle;
      this.textMaterial.Text = "PLA";
      frame.AddChildElement((Element2D) this.textMaterial);
      TextWidget textWidget4 = new TextWidget(17);
      textWidget4.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget4.RelativeWidth = 0.45f;
      textWidget4.RelativeHeight = 0.2f;
      textWidget4.RelativeX = 0.0f;
      textWidget4.RelativeY = 0.5f;
      textWidget4.Alignment = QFontAlignment.Right;
      textWidget4.VAlignment = TextVerticalAlignment.Middle;
      textWidget4.Text = "Temperature:";
      frame.AddChildElement((Element2D) textWidget4);
      this.custom_temperature_edit = new EditBoxWidget(13);
      this.custom_temperature_edit.Init(this.Host, "guicontrols", 898f, 104f, 941f, 135f);
      this.custom_temperature_edit.SetGrowableWidth(3, 3, 32);
      this.custom_temperature_edit.Text = "";
      this.custom_temperature_edit.Enabled = true;
      this.custom_temperature_edit.SetSize(100, 24);
      this.custom_temperature_edit.Color = color4_3;
      this.custom_temperature_edit.SetTextWindowBorders(4, 4, 4, 4);
      this.custom_temperature_edit.RelativeX = 0.55f;
      this.custom_temperature_edit.RelativeY = 0.55f;
      this.custom_temperature_edit.SetVisible(true);
      this.custom_temperature_edit.SetCallbackEnterKey(new EditBoxWidget.EditBoxCallback(this.TemperatureEditEnterCallback));
      frame.AddChildElement((Element2D) this.custom_temperature_edit);
      ButtonWidget buttonWidget1 = new ButtonWidget(20);
      buttonWidget1.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget1.Size = FontSize.Medium;
      buttonWidget1.Text = "Reset";
      buttonWidget1.SetSize(70, 24);
      buttonWidget1.RelativeX = 0.63f;
      buttonWidget1.RelativeY = 0.55f;
      buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      frame.AddChildElement((Element2D) buttonWidget1);
      ButtonWidget buttonWidget2 = new ButtonWidget(18, (Element2D) this);
      buttonWidget2.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget2.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget2.Size = FontSize.Large;
      buttonWidget2.SetSize(100, 32);
      buttonWidget2.RelativeX = 0.35f;
      buttonWidget2.RelativeY = 0.8f;
      buttonWidget2.SetGrowableHeight(4, 4, 32);
      buttonWidget2.SetGrowableWidth(4, 4, 32);
      buttonWidget2.Text = "Cancel";
      buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget2);
      ButtonWidget buttonWidget3 = new ButtonWidget(19, (Element2D) this);
      buttonWidget3.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget3.Color = color4_3;
      buttonWidget3.Size = FontSize.Large;
      buttonWidget3.SetSize(100, 32);
      buttonWidget3.RelativeX = 0.55f;
      buttonWidget3.RelativeY = 0.8f;
      buttonWidget3.SetGrowableHeight(4, 4, 32);
      buttonWidget3.SetGrowableWidth(4, 4, 32);
      buttonWidget3.Text = "Save";
      buttonWidget3.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget3);
      this.SetPosition(0, 0);
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      frame.RelativeX = 0.0f;
      frame.RelativeY = 0.2f;
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.5f;
      this.AddChildElement((Element2D) frame);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.filamentset.Value = false;
      this.Host.SetFocus((Element2D) this.custom_temperature_edit);
      this.custom_temperature_edit.Text = details.current_spool.filament_temperature.ToString();
      this.textColor.Text = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), this.CurrentDetails.current_spool.filament_color_code));
      this.textMaterial.Text = this.CurrentDetails.current_spool.filament_type.ToString();
    }

    public void TemperatureEditEnterCallback(EditBoxWidget edit)
    {
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      int result;
      if (int.TryParse(this.custom_temperature_edit.Text.ToString(), out result))
      {
        int min = (int) FilamentConstants.Temperature.MaxMinForFilamentType(this.CurrentDetails.current_spool.filament_type).Min;
        int num1 = 260;
        if (result >= min && result <= num1)
        {
          this.messagebox.AddMessageToQueue(string.Format("Changing the current temperature settings from {0} to {1}.", (object) this.CurrentDetails.current_spool.filament_temperature, (object) this.custom_temperature_edit.Text), PopupMessageBox.MessageBoxButtons.OK);
          FilamentSpool filamentSpool = new FilamentSpool(this.CurrentDetails.current_spool);
          filamentSpool.filament_temperature = result;
          this.CurrentDetails.current_spool = filamentSpool;
          this.CurrentDetails.waitCondition = new Mangage3DInkStageDetails.WaitCondition(this.WaitCondition);
          this.CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
          FilamentWaitingPage.CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
          this.settingsManager.FilamentDictionary.AddCustomTemperature(filamentSpool.filament_type, (FilamentConstants.ColorsEnum) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), filamentSpool.filament_color_code), filamentSpool.filament_temperature);
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, this.CurrentDetails);
          int num2 = (int) selectedPrinter.AcquireLock(new M3D.Spooling.Client.AsyncCallback(this.SetFilamentAfterLock), (object) selectedPrinter);
        }
        else
          this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a temperature from " + (object) min + " to " + (object) num1));
      }
      else
        this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a valid temperature value"));
    }

    private void SetFilamentAfterLock(IAsyncCallResult ar)
    {
      PrinterObject asyncState = ar.AsyncState as PrinterObject;
      if (asyncState.CheckForLockError(ar))
      {
        if (this.settingsManager != null)
        {
          this.settingsManager.AssociateFilamentToPrinter(asyncState.Info.serial_number, this.CurrentDetails.current_spool);
          this.settingsManager.SaveSettings();
        }
        int num = (int) asyncState.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(this.AfterFilamentSet), (object) asyncState, this.CurrentDetails.current_spool);
      }
      else
        this.MainWindow.ResetToStartup();
    }

    private void AfterFilamentSet(IAsyncCallResult ar)
    {
      (ar.AsyncState as PrinterObject).MarkedAsBusy = false;
      this.filamentset.Value = true;
    }

    private bool WaitCondition()
    {
      return this.filamentset.Value;
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
