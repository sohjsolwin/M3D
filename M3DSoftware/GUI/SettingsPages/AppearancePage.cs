// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.AppearancePage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Ext3D.ModelRendering;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Interfaces;
using M3D.Properties;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;
using System.Windows.Forms;

namespace M3D.GUI.SettingsPages
{
  public class AppearancePage : SettingsPage
  {
    private XMLFrame appearanceFrame;
    private XMLFrame messagesFrame;
    private XMLFrame advancedFrame;
    private Frame active_frame;
    private bool initialized;
    private ButtonWidget pro_button;
    private ButtonWidget m1_button;
    private ButtonWidget mm_button;
    private ButtonWidget inches_button;
    private ComboBoxWidget printercolorCombobox;
    private ComboBoxWidget modelcolorCombobox;
    private ComboBoxWidget iconcolorCombobox;
    private ButtonWidget startFullScreenButton;
    private ButtonWidget showAllWarningsButton;
    private ButtonWidget printerMismatchButton;
    private ComboBoxWidget rendermode_combobox;
    private ComboBoxWidget softwareupdate_options;
    private ButtonWidget updateSoftware_button;
    private SpriteAnimationWidget download_progress;
    private TextWidget update_text;
    private ButtonWidget useMultipleModelsCheckbox;
    private ButtonWidget showRemoveModelWarningCheckbox;
    private ButtonWidget autoDetectUnitsCheckbox;
    private SettingsManager settings;
    private PopupMessageBox messagebox;
    private Updater softwareUpdater;

    public AppearancePage(int ID, GUIHost host, SettingsManager settings, Updater softwareUpdater, PopupMessageBox messagebox)
      : base(ID)
    {
      this.settings = settings;
      this.messagebox = messagebox;
      this.softwareUpdater = softwareUpdater;
      string interfaceframeTabbuttons = Resources.interfaceframe_tabbuttons;
      this.Init(host, interfaceframeTabbuttons, new ButtonCallback(this.tabsFrameButtonCallback));
      this.Visible = false;
      this.Enabled = false;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      this.CreateAppearanceFrame(host);
      this.CreateMessagesFrame(host);
      this.CreateAdvancedFrame(host);
      this.SyncSettings();
      this.active_frame = (Frame) this.appearanceFrame;
      this.initialized = true;
    }

    public void SetSelectedPrinterColor(string color)
    {
      int itemIndex = this.printercolorCombobox.GetItemIndex(color);
      if (itemIndex > -1)
        this.printercolorCombobox.Select = itemIndex;
      else
        this.printercolorCombobox.Select = 0;
    }

    public void SetSelectedModelColor(string color)
    {
      int itemIndex = this.modelcolorCombobox.GetItemIndex(color);
      if (itemIndex <= -1)
        return;
      this.modelcolorCombobox.Select = itemIndex;
    }

    public void SetSelectedIconColor(string color)
    {
      int itemIndex = this.iconcolorCombobox.GetItemIndex(color);
      if (itemIndex <= -1)
        return;
      this.iconcolorCombobox.Select = itemIndex;
    }

    public void SetStartFullScreen(bool startFullScreen)
    {
      this.startFullScreenButton.Checked = startFullScreen;
      this.settings.CurrentAppearanceSettings.StartFullScreen = startFullScreen;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible)
        return;
      this.SyncSettings();
    }

    private void SyncSettings()
    {
      if (this.rendermode_combobox != null)
      {
        switch (OpenGLRendererObject.openGLRenderMode)
        {
          case OpenGLRendererObject.OpenGLRenderMode.VBOs:
            this.rendermode_combobox.Select = 0;
            break;
          case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
            this.rendermode_combobox.Select = 1;
            break;
          case OpenGLRendererObject.OpenGLRenderMode.ImmediateMode:
            this.rendermode_combobox.Select = 3;
            break;
        }
      }
      this.SetSelectedPrinterColor(this.settings.CurrentAppearanceSettings.PrinterColor);
      this.SetSelectedModelColor(this.settings.CurrentAppearanceSettings.ModelColor);
      this.SetSelectedIconColor(this.settings.CurrentAppearanceSettings.IconColor);
      this.SetStartFullScreen(this.settings.CurrentAppearanceSettings.StartFullScreen);
      if (this.showAllWarningsButton != null)
        this.showAllWarningsButton.Checked = this.settings.CurrentAppearanceSettings.ShowAllWarnings;
      if (this.printerMismatchButton != null)
        this.printerMismatchButton.Checked = this.settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning;
      if (this.useMultipleModelsCheckbox != null)
        this.useMultipleModelsCheckbox.Checked = this.settings.CurrentAppearanceSettings.UseMultipleModels;
      if (this.showRemoveModelWarningCheckbox != null)
        this.showRemoveModelWarningCheckbox.Checked = this.settings.CurrentAppearanceSettings.ShowRemoveModelWarning;
      if (this.autoDetectUnitsCheckbox != null)
        this.autoDetectUnitsCheckbox.Checked = this.settings.CurrentAppearanceSettings.AutoDetectModelUnits;
      if (this.settings.CurrentAppearanceSettings.Units == SettingsManager.GridUnit.Inches)
        this.inches_button.Checked = true;
      else if (this.settings.CurrentAppearanceSettings.Units == SettingsManager.GridUnit.MM)
        this.mm_button.Checked = true;
      if (this.settings.CurrentAppearanceSettings.CaseType == PrinterSizeProfile.CaseType.ProCase)
      {
        this.pro_button.Checked = true;
      }
      else
      {
        if (this.settings.CurrentAppearanceSettings.CaseType != PrinterSizeProfile.CaseType.Micro1Case)
          return;
        this.m1_button.Checked = true;
      }
    }

    public void SaveSettings()
    {
      this.settings.CurrentAppearanceSettings.ShowAllWarnings = this.showAllWarningsButton.Checked;
      this.settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = this.printerMismatchButton.Checked;
      this.settings.CurrentAppearanceSettings.PrinterColor = this.printercolorCombobox.Text;
      this.settings.CurrentAppearanceSettings.ModelColor = this.modelcolorCombobox.Text;
      this.settings.CurrentAppearanceSettings.IconColor = this.iconcolorCombobox.Text;
      this.settings.CurrentAppearanceSettings.StartFullScreen = this.startFullScreenButton.Checked;
      this.settings.CurrentAppearanceSettings.UseMultipleModels = this.useMultipleModelsCheckbox.Checked;
      this.settings.CurrentAppearanceSettings.ShowRemoveModelWarning = this.showRemoveModelWarningCheckbox.Checked;
      this.settings.CurrentAppearanceSettings.AutoDetectModelUnits = this.autoDetectUnitsCheckbox.Checked;
      this.settings.SaveSettings();
    }

    private void TurnOffActiveFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.Visible = false;
      this.active_frame.Enabled = false;
      this.active_frame = (Frame) null;
    }

    public void tabsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.appearanceFrame;
          break;
        case 2:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.messagesFrame;
          break;
        case 3:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.advancedFrame;
          break;
      }
      if (this.active_frame != null)
      {
        this.active_frame.Enabled = true;
        this.active_frame.Visible = true;
        this.Host.SetFocus((Element2D) this.active_frame);
      }
      this.Refresh();
    }

    private void CreateAppearanceFrame(GUIHost host)
    {
      string interfaceframeAppearance = Resources.interfaceframe_appearance;
      this.appearanceFrame = new XMLFrame();
      this.appearanceFrame.Init(host, interfaceframeAppearance, new ButtonCallback(this.AppearanceFrameButtonCallback));
      this.appearanceFrame.ID = 1001;
      this.appearanceFrame.CenterHorizontallyInParent = true;
      this.appearanceFrame.RelativeY = 0.1f;
      this.appearanceFrame.RelativeWidth = 0.95f;
      this.appearanceFrame.RelativeHeight = 0.9f;
      this.appearanceFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.appearanceFrame.Visible = true;
      this.appearanceFrame.Enabled = true;
      this.printercolorCombobox = (ComboBoxWidget) this.appearanceFrame.FindChildElement(104);
      this.printercolorCombobox.Select = 0;
      this.printercolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(this.ColorComboboxChanged);
      this.modelcolorCombobox = (ComboBoxWidget) this.appearanceFrame.FindChildElement(106);
      this.modelcolorCombobox.Select = 0;
      this.modelcolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(this.ColorComboboxChanged);
      this.iconcolorCombobox = (ComboBoxWidget) this.appearanceFrame.FindChildElement(108);
      this.iconcolorCombobox.Select = 0;
      this.iconcolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(this.ColorComboboxChanged);
      TextWidget childElement = (TextWidget) this.appearanceFrame.FindChildElement(100);
      this.mm_button = (ButtonWidget) this.appearanceFrame.FindChildElement(101);
      this.inches_button = (ButtonWidget) this.appearanceFrame.FindChildElement(102);
      this.pro_button = (ButtonWidget) this.appearanceFrame.FindChildElement(110);
      this.m1_button = (ButtonWidget) this.appearanceFrame.FindChildElement(111);
      this.rendermode_combobox = (ComboBoxWidget) this.appearanceFrame.FindChildElement(207);
      this.rendermode_combobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(this.RenderModeChanged);
      switch (OpenGLRendererObject.openGLRenderMode)
      {
        case OpenGLRendererObject.OpenGLRenderMode.VBOs:
          this.rendermode_combobox.Select = 0;
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
          this.rendermode_combobox.Select = 1;
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ImmediateMode:
          this.rendermode_combobox.Select = 1;
          break;
      }
      this.childFrame.AddChildElement((Element2D) this.appearanceFrame);
      this.appearanceFrame.Refresh();
    }

    private void AppearanceFrameButtonCallback(ButtonWidget button)
    {
      if (!this.initialized)
        return;
      switch (button.ID)
      {
        case 101:
          this.settings.CurrentAppearanceSettings.Units = SettingsManager.GridUnit.MM;
          break;
        case 102:
          this.settings.CurrentAppearanceSettings.Units = SettingsManager.GridUnit.Inches;
          break;
        case 110:
          this.settings.CurrentAppearanceSettings.CaseType = PrinterSizeProfile.CaseType.ProCase;
          break;
        case 111:
          this.settings.CurrentAppearanceSettings.CaseType = PrinterSizeProfile.CaseType.Micro1Case;
          break;
      }
    }

    private void ColorComboboxChanged(ComboBoxWidget combobox)
    {
      if (!this.initialized)
        return;
      if (combobox.ID == 104)
        this.settings.CurrentAppearanceSettings.PrinterColor = this.printercolorCombobox.Text;
      if (combobox.ID == 106)
        this.settings.CurrentAppearanceSettings.ModelColor = this.modelcolorCombobox.Text;
      if (combobox.ID != 108)
        return;
      this.settings.CurrentAppearanceSettings.IconColor = this.iconcolorCombobox.Text;
    }

    private void CreateMessagesFrame(GUIHost host)
    {
      string interfaceframeMessages = Resources.interfaceframe_messages;
      this.messagesFrame = new XMLFrame();
      this.messagesFrame.Init(host, interfaceframeMessages, new ButtonCallback(this.MessagesFrameButtonCallback));
      this.messagesFrame.ID = 1002;
      this.messagesFrame.CenterHorizontallyInParent = true;
      this.messagesFrame.RelativeY = 0.1f;
      this.messagesFrame.RelativeWidth = 0.95f;
      this.messagesFrame.RelativeHeight = 0.9f;
      this.messagesFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.messagesFrame.Visible = false;
      this.messagesFrame.Enabled = false;
      this.childFrame.AddChildElement((Element2D) this.messagesFrame);
      this.messagesFrame.Refresh();
      this.startFullScreenButton = (ButtonWidget) this.messagesFrame.FindChildElement(201);
      this.showAllWarningsButton = (ButtonWidget) this.messagesFrame.FindChildElement(203);
      this.printerMismatchButton = (ButtonWidget) this.messagesFrame.FindChildElement(209);
      ButtonWidget childElement1 = (ButtonWidget) this.messagesFrame.FindChildElement(205);
      TextWidget childElement2 = (TextWidget) this.messagesFrame.FindChildElement(204);
      IFileAssociations fileAssociations = this.settings.FileAssociations;
      if (fileAssociations == null)
        return;
      string str1 = fileAssociations.ExtensionOpenWith(".stl");
      string str2 = fileAssociations.ExtensionOpenWith(".obj");
      if (str1 != null && str2 != null && (str1.Contains(Application.ExecutablePath) && str2.Contains(Application.ExecutablePath)))
        childElement1.Checked = true;
      childElement1.ImageHasFocusColor = new Color4((byte) 100, (byte) 230, byte.MaxValue, byte.MaxValue);
    }

    private void MessagesFrameButtonCallback(ButtonWidget button)
    {
      if (!this.initialized)
        return;
      switch (button.ID)
      {
        case 201:
          this.settings.CurrentAppearanceSettings.StartFullScreen = button.Checked;
          break;
        case 203:
          this.settings.ShowAllWarnings = button.Checked;
          break;
        case 205:
          string startupPath = Application.StartupPath;
          IFileAssociations fileAssociations = this.settings.FileAssociations;
          if (button.Checked)
          {
            fileAssociations.Set3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.stl)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
            fileAssociations.Set3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.obj)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
            break;
          }
          fileAssociations.Delete3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file");
          fileAssociations.Delete3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file");
          break;
        case 209:
          this.settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = button.Checked;
          break;
      }
    }

    private void RenderModeChanged(ComboBoxWidget combobox)
    {
      if (!this.initialized)
        return;
      int itemIndex = combobox.GetItemIndex(combobox.EditBox.Text);
      if (itemIndex < 0)
        return;
      string str = (string) combobox.ListBox.Items[itemIndex];
      if (!(str == "Enhanced Graphics Mode"))
      {
        if (!(str == "ARB Vertex Buffer Objects"))
        {
          if (!(str == "Basic Graphics Mode"))
            return;
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
        }
        else
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ARBVBOs;
      }
      else
        OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.VBOs;
    }

    private void CreateAdvancedFrame(GUIHost host)
    {
      string interfaceframeAdvanced = Resources.interfaceframe_advanced;
      this.advancedFrame = new XMLFrame();
      this.advancedFrame.Init(host, interfaceframeAdvanced, new ButtonCallback(this.AdvancedFrameButtonCallback));
      this.advancedFrame.ID = 1003;
      this.advancedFrame.CenterHorizontallyInParent = true;
      this.advancedFrame.RelativeY = 0.1f;
      this.advancedFrame.RelativeWidth = 0.95f;
      this.advancedFrame.RelativeHeight = 0.9f;
      this.advancedFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.advancedFrame.Visible = false;
      this.advancedFrame.Enabled = false;
      this.childFrame.AddChildElement((Element2D) this.advancedFrame);
      this.advancedFrame.Refresh();
      this.softwareupdate_options = (ComboBoxWidget) this.advancedFrame.FindChildElement(303);
      this.updateSoftware_button = (ButtonWidget) this.advancedFrame.FindChildElement(305);
      this.download_progress = (SpriteAnimationWidget) this.advancedFrame.FindChildElement(306);
      this.update_text = (TextWidget) this.advancedFrame.FindChildElement(304);
      this.softwareupdate_options.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(this.AdvancedSettingsComboBoxCallback);
      this.softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_DownloadInstall"));
      this.softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_DownloadNoInstall"));
      this.softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_TakeNoAction"));
      this.softwareupdate_options.Select = this.softwareUpdater.UpdaterMode != Updater.UpdateSettings.DownloadNotInstall ? (this.softwareUpdater.UpdaterMode != Updater.UpdateSettings.NoAction ? 0 : 2) : 1;
      this.update_text.Visible = false;
      this.download_progress.Visible = false;
      this.updateSoftware_button.Enabled = true;
      this.updateSoftware_button.Visible = false;
      this.advancedFrame.DoOnUpdate = new ElementStandardDelegate(this.AdvancedSettingsOnUpdate);
      this.useMultipleModelsCheckbox = (ButtonWidget) this.advancedFrame.FindChildElement(313);
      this.showRemoveModelWarningCheckbox = (ButtonWidget) this.advancedFrame.FindChildElement(315);
      this.autoDetectUnitsCheckbox = (ButtonWidget) this.advancedFrame.FindChildElement(317);
    }

    private void AdvancedFrameButtonCallback(ButtonWidget button)
    {
      if (!this.initialized)
        return;
      switch (button.ID)
      {
        case 305:
          this.softwareUpdater.ForceDownloadAndUpdate();
          if (!this.softwareUpdater.isWorking)
            break;
          UpdateDownloadingDialog.Show(this.messagebox, this.softwareUpdater);
          break;
        case 313:
          this.settings.CurrentAppearanceSettings.UseMultipleModels = button.Checked;
          break;
        case 315:
          this.settings.CurrentAppearanceSettings.ShowRemoveModelWarning = button.Checked;
          break;
        case 317:
          this.settings.CurrentAppearanceSettings.AutoDetectModelUnits = button.Checked;
          break;
      }
    }

    public void AdvancedSettingsComboBoxCallback(ComboBoxWidget box)
    {
      if (!this.initialized || box.ID != 303)
        return;
      this.settings.CurrentAppearanceSettings.UpdaterMode = !(box.Text == Locale.GlobalLocale.T("T_UpdateOp_TakeNoAction")) ? (!(box.Text == Locale.GlobalLocale.T("T_UpdateOp_DownloadNoInstall")) ? Updater.UpdateSettings.DownloadInstall : Updater.UpdateSettings.DownloadNotInstall) : Updater.UpdateSettings.NoAction;
      this.SaveSettings();
    }

    public void AdvancedSettingsOnUpdate()
    {
      if (!this.Visible || !this.initialized)
        return;
      switch (this.softwareUpdater.CurrentStatus)
      {
        case Updater.Status.UptoDate:
          this.update_text.Text = "T_Updater_UpToDate";
          this.update_text.Visible = true;
          this.updateSoftware_button.Visible = false;
          this.download_progress.Visible = false;
          break;
        case Updater.Status.NewVersionAvailable:
        case Updater.Status.DownloadError:
          this.update_text.Text = "T_Updater_UpdateAvailable";
          this.updateSoftware_button.Text = "T_UpdateOp_DownloadInstall";
          this.updateSoftware_button.Visible = true;
          this.update_text.Visible = true;
          this.download_progress.Visible = false;
          break;
        case Updater.Status.Downloading:
          this.update_text.Text = "T_Updater_Downloading";
          this.update_text.Visible = true;
          this.updateSoftware_button.Visible = false;
          this.download_progress.Visible = true;
          break;
        case Updater.Status.Downloaded:
          this.update_text.Text = "T_Updater_UpdateDownloaded";
          this.updateSoftware_button.Text = "T_InstallNow";
          this.updateSoftware_button.Visible = true;
          this.update_text.Visible = true;
          this.download_progress.Visible = false;
          break;
        default:
          this.updateSoftware_button.Visible = false;
          this.update_text.Visible = false;
          this.download_progress.Visible = false;
          break;
      }
      if (!this.softwareUpdater.isReadyToInstall)
        return;
      this.updateSoftware_button.Visible = true;
      this.update_text.Visible = true;
    }

    private enum AppearanceFrameIDs
    {
      UnitsText = 100, // 0x00000064
      MMButton = 101, // 0x00000065
      InchesButton = 102, // 0x00000066
      PrinterColorText = 103, // 0x00000067
      PrinterColorCombobox = 104, // 0x00000068
      ModelColorText = 105, // 0x00000069
      ModelColorCombobox = 106, // 0x0000006A
      IconColorText = 107, // 0x0000006B
      IconColorCombobox = 108, // 0x0000006C
      CaseText = 109, // 0x0000006D
      ProCaseButton = 110, // 0x0000006E
      M1CaseButton = 111, // 0x0000006F
      RenderingModeText = 206, // 0x000000CE
      RenderingModeCombobox = 207, // 0x000000CF
    }

    private enum MessageFrameIDs
    {
      StartFullScreenText = 200, // 0x000000C8
      StartFullScreenCheckbox = 201, // 0x000000C9
      ShowAllWarningsText = 202, // 0x000000CA
      ShowAllWarningsCheckbox = 203, // 0x000000CB
      MakeDefaultProgramText = 204, // 0x000000CC
      MakeDefaultProgramCheckbox = 205, // 0x000000CD
      ShowImproveHelpText = 206, // 0x000000CE
      ShowImproveHelpCheckbox = 207, // 0x000000CF
      PrintMismatchText = 208, // 0x000000D0
      PrintMismatchCheckbox = 209, // 0x000000D1
    }

    private enum AdvancedFrameIDs
    {
      UpdateSettingsText = 302, // 0x0000012E
      SoftwareupdateOptionsCombobox = 303, // 0x0000012F
      UpdateText = 304, // 0x00000130
      UpdateSoftwareButton = 305, // 0x00000131
      DownloadProgressAnimation = 306, // 0x00000132
      UseMultipleModelsCheckbox = 313, // 0x00000139
      ShowRemoveModelWarningCheckbox = 315, // 0x0000013B
      AutoDetectUnitsCheckbox = 317, // 0x0000013D
    }
  }
}
