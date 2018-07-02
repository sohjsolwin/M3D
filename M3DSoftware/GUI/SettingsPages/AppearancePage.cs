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
      var interfaceframeTabbuttons = Resources.interfaceframe_tabbuttons;
      Init(host, interfaceframeTabbuttons, new ButtonCallback(tabsFrameButtonCallback));
      Visible = false;
      Enabled = false;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      CreateAppearanceFrame(host);
      CreateMessagesFrame(host);
      CreateAdvancedFrame(host);
      SyncSettings();
      active_frame = (Frame)appearanceFrame;
      initialized = true;
    }

    public void SetSelectedPrinterColor(string color)
    {
      var itemIndex = printercolorCombobox.GetItemIndex(color);
      if (itemIndex > -1)
      {
        printercolorCombobox.Select = itemIndex;
      }
      else
      {
        printercolorCombobox.Select = 0;
      }
    }

    public void SetSelectedModelColor(string color)
    {
      var itemIndex = modelcolorCombobox.GetItemIndex(color);
      if (itemIndex <= -1)
      {
        return;
      }

      modelcolorCombobox.Select = itemIndex;
    }

    public void SetSelectedIconColor(string color)
    {
      var itemIndex = iconcolorCombobox.GetItemIndex(color);
      if (itemIndex <= -1)
      {
        return;
      }

      iconcolorCombobox.Select = itemIndex;
    }

    public void SetStartFullScreen(bool startFullScreen)
    {
      startFullScreenButton.Checked = startFullScreen;
      settings.CurrentAppearanceSettings.StartFullScreen = startFullScreen;
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
      if (!bVisible)
      {
        return;
      }

      SyncSettings();
    }

    private void SyncSettings()
    {
      if (rendermode_combobox != null)
      {
        switch (OpenGLRendererObject.openGLRenderMode)
        {
          case OpenGLRendererObject.OpenGLRenderMode.VBOs:
            rendermode_combobox.Select = 0;
            break;
          case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
            rendermode_combobox.Select = 1;
            break;
          case OpenGLRendererObject.OpenGLRenderMode.ImmediateMode:
            rendermode_combobox.Select = 3;
            break;
        }
      }
      SetSelectedPrinterColor(settings.CurrentAppearanceSettings.PrinterColor);
      SetSelectedModelColor(settings.CurrentAppearanceSettings.ModelColor);
      SetSelectedIconColor(settings.CurrentAppearanceSettings.IconColor);
      SetStartFullScreen(settings.CurrentAppearanceSettings.StartFullScreen);
      if (showAllWarningsButton != null)
      {
        showAllWarningsButton.Checked = settings.CurrentAppearanceSettings.ShowAllWarnings;
      }

      if (printerMismatchButton != null)
      {
        printerMismatchButton.Checked = settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning;
      }

      if (useMultipleModelsCheckbox != null)
      {
        useMultipleModelsCheckbox.Checked = settings.CurrentAppearanceSettings.UseMultipleModels;
      }

      if (showRemoveModelWarningCheckbox != null)
      {
        showRemoveModelWarningCheckbox.Checked = settings.CurrentAppearanceSettings.ShowRemoveModelWarning;
      }

      if (autoDetectUnitsCheckbox != null)
      {
        autoDetectUnitsCheckbox.Checked = settings.CurrentAppearanceSettings.AutoDetectModelUnits;
      }

      if (settings.CurrentAppearanceSettings.Units == SettingsManager.GridUnit.Inches)
      {
        inches_button.Checked = true;
      }
      else if (settings.CurrentAppearanceSettings.Units == SettingsManager.GridUnit.MM)
      {
        mm_button.Checked = true;
      }

      if (settings.CurrentAppearanceSettings.CaseType == PrinterSizeProfile.CaseType.ProCase)
      {
        pro_button.Checked = true;
      }
      else
      {
        if (settings.CurrentAppearanceSettings.CaseType != PrinterSizeProfile.CaseType.Micro1Case)
        {
          return;
        }

        m1_button.Checked = true;
      }
    }

    public void SaveSettings()
    {
      settings.CurrentAppearanceSettings.ShowAllWarnings = showAllWarningsButton.Checked;
      settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = printerMismatchButton.Checked;
      settings.CurrentAppearanceSettings.PrinterColor = printercolorCombobox.Text;
      settings.CurrentAppearanceSettings.ModelColor = modelcolorCombobox.Text;
      settings.CurrentAppearanceSettings.IconColor = iconcolorCombobox.Text;
      settings.CurrentAppearanceSettings.StartFullScreen = startFullScreenButton.Checked;
      settings.CurrentAppearanceSettings.UseMultipleModels = useMultipleModelsCheckbox.Checked;
      settings.CurrentAppearanceSettings.ShowRemoveModelWarning = showRemoveModelWarningCheckbox.Checked;
      settings.CurrentAppearanceSettings.AutoDetectModelUnits = autoDetectUnitsCheckbox.Checked;
      settings.SaveSettings();
    }

    private void TurnOffActiveFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.Visible = false;
      active_frame.Enabled = false;
      active_frame = (Frame) null;
    }

    public void tabsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          TurnOffActiveFrame();
          active_frame = (Frame)appearanceFrame;
          break;
        case 2:
          TurnOffActiveFrame();
          active_frame = (Frame)messagesFrame;
          break;
        case 3:
          TurnOffActiveFrame();
          active_frame = (Frame)advancedFrame;
          break;
      }
      if (active_frame != null)
      {
        active_frame.Enabled = true;
        active_frame.Visible = true;
        Host.SetFocus((Element2D)active_frame);
      }
      Refresh();
    }

    private void CreateAppearanceFrame(GUIHost host)
    {
      var interfaceframeAppearance = Resources.interfaceframe_appearance;
      appearanceFrame = new XMLFrame();
      appearanceFrame.Init(host, interfaceframeAppearance, new ButtonCallback(AppearanceFrameButtonCallback));
      appearanceFrame.ID = 1001;
      appearanceFrame.CenterHorizontallyInParent = true;
      appearanceFrame.RelativeY = 0.1f;
      appearanceFrame.RelativeWidth = 0.95f;
      appearanceFrame.RelativeHeight = 0.9f;
      appearanceFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      appearanceFrame.Visible = true;
      appearanceFrame.Enabled = true;
      printercolorCombobox = (ComboBoxWidget)appearanceFrame.FindChildElement(104);
      printercolorCombobox.Select = 0;
      printercolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(ColorComboboxChanged);
      modelcolorCombobox = (ComboBoxWidget)appearanceFrame.FindChildElement(106);
      modelcolorCombobox.Select = 0;
      modelcolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(ColorComboboxChanged);
      iconcolorCombobox = (ComboBoxWidget)appearanceFrame.FindChildElement(108);
      iconcolorCombobox.Select = 0;
      iconcolorCombobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(ColorComboboxChanged);
      var childElement = (TextWidget)appearanceFrame.FindChildElement(100);
      mm_button = (ButtonWidget)appearanceFrame.FindChildElement(101);
      inches_button = (ButtonWidget)appearanceFrame.FindChildElement(102);
      pro_button = (ButtonWidget)appearanceFrame.FindChildElement(110);
      m1_button = (ButtonWidget)appearanceFrame.FindChildElement(111);
      rendermode_combobox = (ComboBoxWidget)appearanceFrame.FindChildElement(207);
      rendermode_combobox.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(RenderModeChanged);
      switch (OpenGLRendererObject.openGLRenderMode)
      {
        case OpenGLRendererObject.OpenGLRenderMode.VBOs:
          rendermode_combobox.Select = 0;
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
          rendermode_combobox.Select = 1;
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ImmediateMode:
          rendermode_combobox.Select = 1;
          break;
      }
      childFrame.AddChildElement((Element2D)appearanceFrame);
      appearanceFrame.Refresh();
    }

    private void AppearanceFrameButtonCallback(ButtonWidget button)
    {
      if (!initialized)
      {
        return;
      }

      switch (button.ID)
      {
        case 101:
          settings.CurrentAppearanceSettings.Units = SettingsManager.GridUnit.MM;
          break;
        case 102:
          settings.CurrentAppearanceSettings.Units = SettingsManager.GridUnit.Inches;
          break;
        case 110:
          settings.CurrentAppearanceSettings.CaseType = PrinterSizeProfile.CaseType.ProCase;
          break;
        case 111:
          settings.CurrentAppearanceSettings.CaseType = PrinterSizeProfile.CaseType.Micro1Case;
          break;
      }
    }

    private void ColorComboboxChanged(ComboBoxWidget combobox)
    {
      if (!initialized)
      {
        return;
      }

      if (combobox.ID == 104)
      {
        settings.CurrentAppearanceSettings.PrinterColor = printercolorCombobox.Text;
      }

      if (combobox.ID == 106)
      {
        settings.CurrentAppearanceSettings.ModelColor = modelcolorCombobox.Text;
      }

      if (combobox.ID != 108)
      {
        return;
      }

      settings.CurrentAppearanceSettings.IconColor = iconcolorCombobox.Text;
    }

    private void CreateMessagesFrame(GUIHost host)
    {
      var interfaceframeMessages = Resources.interfaceframe_messages;
      messagesFrame = new XMLFrame();
      messagesFrame.Init(host, interfaceframeMessages, new ButtonCallback(MessagesFrameButtonCallback));
      messagesFrame.ID = 1002;
      messagesFrame.CenterHorizontallyInParent = true;
      messagesFrame.RelativeY = 0.1f;
      messagesFrame.RelativeWidth = 0.95f;
      messagesFrame.RelativeHeight = 0.9f;
      messagesFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      messagesFrame.Visible = false;
      messagesFrame.Enabled = false;
      childFrame.AddChildElement((Element2D)messagesFrame);
      messagesFrame.Refresh();
      startFullScreenButton = (ButtonWidget)messagesFrame.FindChildElement(201);
      showAllWarningsButton = (ButtonWidget)messagesFrame.FindChildElement(203);
      printerMismatchButton = (ButtonWidget)messagesFrame.FindChildElement(209);
      var childElement1 = (ButtonWidget)messagesFrame.FindChildElement(205);
      var childElement2 = (TextWidget)messagesFrame.FindChildElement(204);
      IFileAssociations fileAssociations = settings.FileAssociations;
      if (fileAssociations == null)
      {
        return;
      }

      var str1 = fileAssociations.ExtensionOpenWith(".stl");
      var str2 = fileAssociations.ExtensionOpenWith(".obj");
      if (str1 != null && str2 != null && (str1.Contains(Application.ExecutablePath) && str2.Contains(Application.ExecutablePath)))
      {
        childElement1.Checked = true;
      }

      childElement1.ImageHasFocusColor = new Color4((byte) 100, (byte) 230, byte.MaxValue, byte.MaxValue);
    }

    private void MessagesFrameButtonCallback(ButtonWidget button)
    {
      if (!initialized)
      {
        return;
      }

      switch (button.ID)
      {
        case 201:
          settings.CurrentAppearanceSettings.StartFullScreen = button.Checked;
          break;
        case 203:
          settings.ShowAllWarnings = button.Checked;
          break;
        case 205:
          var startupPath = Application.StartupPath;
          IFileAssociations fileAssociations = settings.FileAssociations;
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
          settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = button.Checked;
          break;
      }
    }

    private void RenderModeChanged(ComboBoxWidget combobox)
    {
      if (!initialized)
      {
        return;
      }

      var itemIndex = combobox.GetItemIndex(combobox.EditBox.Text);
      if (itemIndex < 0)
      {
        return;
      }

      var str = (string) combobox.ListBox.Items[itemIndex];
      if (!(str == "Enhanced Graphics Mode"))
      {
        if (!(str == "ARB Vertex Buffer Objects"))
        {
          if (!(str == "Basic Graphics Mode"))
          {
            return;
          }

          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
        }
        else
        {
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ARBVBOs;
        }
      }
      else
      {
        OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.VBOs;
      }
    }

    private void CreateAdvancedFrame(GUIHost host)
    {
      var interfaceframeAdvanced = Resources.interfaceframe_advanced;
      advancedFrame = new XMLFrame();
      advancedFrame.Init(host, interfaceframeAdvanced, new ButtonCallback(AdvancedFrameButtonCallback));
      advancedFrame.ID = 1003;
      advancedFrame.CenterHorizontallyInParent = true;
      advancedFrame.RelativeY = 0.1f;
      advancedFrame.RelativeWidth = 0.95f;
      advancedFrame.RelativeHeight = 0.9f;
      advancedFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      advancedFrame.Visible = false;
      advancedFrame.Enabled = false;
      childFrame.AddChildElement((Element2D)advancedFrame);
      advancedFrame.Refresh();
      softwareupdate_options = (ComboBoxWidget)advancedFrame.FindChildElement(303);
      updateSoftware_button = (ButtonWidget)advancedFrame.FindChildElement(305);
      download_progress = (SpriteAnimationWidget)advancedFrame.FindChildElement(306);
      update_text = (TextWidget)advancedFrame.FindChildElement(304);
      softwareupdate_options.TextChangedCallback = new ComboBoxWidget.ComboBoxTextChangedCallback(AdvancedSettingsComboBoxCallback);
      softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_DownloadInstall"));
      softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_DownloadNoInstall"));
      softwareupdate_options.AddItem((object) Locale.GlobalLocale.T("T_UpdateOp_TakeNoAction"));
      softwareupdate_options.Select = softwareUpdater.UpdaterMode != Updater.UpdateSettings.DownloadNotInstall ? (softwareUpdater.UpdaterMode != Updater.UpdateSettings.NoAction ? 0 : 2) : 1;
      update_text.Visible = false;
      download_progress.Visible = false;
      updateSoftware_button.Enabled = true;
      updateSoftware_button.Visible = false;
      advancedFrame.DoOnUpdate = new ElementStandardDelegate(AdvancedSettingsOnUpdate);
      useMultipleModelsCheckbox = (ButtonWidget)advancedFrame.FindChildElement(313);
      showRemoveModelWarningCheckbox = (ButtonWidget)advancedFrame.FindChildElement(315);
      autoDetectUnitsCheckbox = (ButtonWidget)advancedFrame.FindChildElement(317);
    }

    private void AdvancedFrameButtonCallback(ButtonWidget button)
    {
      if (!initialized)
      {
        return;
      }

      switch (button.ID)
      {
        case 305:
          softwareUpdater.ForceDownloadAndUpdate();
          if (!softwareUpdater.isWorking)
          {
            break;
          }

          UpdateDownloadingDialog.Show(messagebox, softwareUpdater);
          break;
        case 313:
          settings.CurrentAppearanceSettings.UseMultipleModels = button.Checked;
          break;
        case 315:
          settings.CurrentAppearanceSettings.ShowRemoveModelWarning = button.Checked;
          break;
        case 317:
          settings.CurrentAppearanceSettings.AutoDetectModelUnits = button.Checked;
          break;
      }
    }

    public void AdvancedSettingsComboBoxCallback(ComboBoxWidget box)
    {
      if (!initialized || box.ID != 303)
      {
        return;
      }

      settings.CurrentAppearanceSettings.UpdaterMode = !(box.Text == Locale.GlobalLocale.T("T_UpdateOp_TakeNoAction")) ? (!(box.Text == Locale.GlobalLocale.T("T_UpdateOp_DownloadNoInstall")) ? Updater.UpdateSettings.DownloadInstall : Updater.UpdateSettings.DownloadNotInstall) : Updater.UpdateSettings.NoAction;
      SaveSettings();
    }

    public void AdvancedSettingsOnUpdate()
    {
      if (!Visible || !initialized)
      {
        return;
      }

      switch (softwareUpdater.CurrentStatus)
      {
        case Updater.Status.UptoDate:
          update_text.Text = "T_Updater_UpToDate";
          update_text.Visible = true;
          updateSoftware_button.Visible = false;
          download_progress.Visible = false;
          break;
        case Updater.Status.NewVersionAvailable:
        case Updater.Status.DownloadError:
          update_text.Text = "T_Updater_UpdateAvailable";
          updateSoftware_button.Text = "T_UpdateOp_DownloadInstall";
          updateSoftware_button.Visible = true;
          update_text.Visible = true;
          download_progress.Visible = false;
          break;
        case Updater.Status.Downloading:
          update_text.Text = "T_Updater_Downloading";
          update_text.Visible = true;
          updateSoftware_button.Visible = false;
          download_progress.Visible = true;
          break;
        case Updater.Status.Downloaded:
          update_text.Text = "T_Updater_UpdateDownloaded";
          updateSoftware_button.Text = "T_InstallNow";
          updateSoftware_button.Visible = true;
          update_text.Visible = true;
          download_progress.Visible = false;
          break;
        default:
          updateSoftware_button.Visible = false;
          update_text.Visible = false;
          download_progress.Visible = false;
          break;
      }
      if (!softwareUpdater.isReadyToInstall)
      {
        return;
      }

      updateSoftware_button.Visible = true;
      update_text.Visible = true;
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
