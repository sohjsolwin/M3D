using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.SettingsPages
{
  public class SettingsDialogWidget : BorderedImageFrame
  {
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;
    private GUIHost host;
    public int window_width;
    public int window_height;
    private Frame tab_frame;
    private AppearancePage appearanceFrame;
    private ManualControlsFrame manualControlsFrame;
    private FilamentProfilePage filamentprofiles_page;
    private CalibrationSettingsFrame calibrationsettings_page;
    private FeaturePanel proFeaturePanelFrame;
    private AboutPage about_frame;
    private SettingsPage active_frame;

    public SettingsDialogWidget(int ID, GUIHost host, SettingsManager main_controller, PopupMessageBox messagebox, SpoolerConnection spooler_connection, Updater softwareUpdater)
      : base(ID, (Element2D) null)
    {
      settingsManager = main_controller;
      this.messagebox = messagebox;
      Init(host, spooler_connection, softwareUpdater);
    }

    private void Init(GUIHost host, SpoolerConnection spooler_connection, Updater softwareUpdater)
    {
      this.host = host;
      Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 37, 8, 64);
      SetSize(792, 356);
      var textWidget = new TextWidget(0);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Settings";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      AddChildElement((Element2D) textWidget);
      var yposition1 = 35;
      Sprite.pixel_perfect = true;
      CreateTabButton(2, "T_SettingsTab_UserInterfaceOptions", yposition1);
      var yposition2 = yposition1 + 64;
      CreateTabButton(5, "T_SettingsTab_Calibration", yposition2);
      var yposition3 = yposition2 + 64;
      CreateTabButton(3, "T_SettingsTab_ExpertControls", yposition3);
      var yposition4 = yposition3 + 64;
      CreateTabButton(4, "T_SettingsTab_FilamentOptions", yposition4);
      var yposition5 = yposition4 + 64;
      CreateTabButton(6, "  Pro/Micro+\n  Features", yposition5);
      CreateTabButton(7, "T_SettingsTab_About", yposition5 + 64);
      var buttonWidget = new ButtonWidget(8)
      {
        X = -40,
        Y = 4
      };
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      AddChildElement((Element2D) buttonWidget);
      tab_frame = new Frame(9)
      {
        X = 191,
        Y = 35
      };
      AddChildElement((Element2D)tab_frame);
      CreateAppearanceFrame(host, softwareUpdater, messagebox);
      CreateManualControlsFrame(host, spooler_connection, settingsManager);
      CreateFilamentProfilesFrame(host, spooler_connection);
      CreateProFeaturesFrame(host, spooler_connection);
      CreateCalibrationFrame(host, spooler_connection);
      CreateAboutFrame(host);
      about_frame.Visible = true;
      about_frame.Enabled = true;
      active_frame = (SettingsPage)about_frame;
      Sprite.pixel_perfect = false;
      Visible = false;
    }

    private ButtonWidget CreateTabButton(int ID, string text, int yposition)
    {
      var buttonWidget = new ButtonWidget(ID);
      buttonWidget.SetPosition(0, yposition);
      buttonWidget.SetSize(181, 64);
      buttonWidget.Text = text;
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 18302;
      buttonWidget.Checked = false;
      buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
      AddChildElement((Element2D) buttonWidget);
      return buttonWidget;
    }

    public override void OnParentResize()
    {
      if (tab_frame != null)
      {
        tab_frame.SetSize(Width - tab_frame.X - 10, Height - tab_frame.Y - 10);
      }

      base.OnParentResize();
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void ActivateAdvancedView()
    {
      ActivateAdvancedView(false);
    }

    public void ActivateAdvancedView(bool calibrate)
    {
      if (!calibrate)
      {
        return;
      }

      var childElement = (ButtonWidget)FindChildElement(5);
      if (childElement != null)
      {
        childElement.Checked = true;
      }

      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
    }

    public void ActivateFilamentView()
    {
      var childElement = (ButtonWidget)FindChildElement(1);
      if (childElement != null)
      {
        childElement.Checked = true;
      }

      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
    }

    public void ActivateChangeFilamentView()
    {
      var childElement = (ButtonWidget)FindChildElement(1);
      if (childElement == null)
      {
        return;
      }

      childElement.Checked = true;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 2:
          TurnOffActiveFrame();
          active_frame = (SettingsPage)appearanceFrame;
          break;
        case 3:
          if (settingsManager.ShowAllWarnings)
          {
            messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_ManualControlsWarning"));
          }

          TurnOffActiveFrame();
          active_frame = (SettingsPage)manualControlsFrame;
          break;
        case 4:
          if (settingsManager.ShowAllWarnings)
          {
            messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, Locale.GlobalLocale.T("T_FilamentProfilesWarning")));
          }

          TurnOffActiveFrame();
          active_frame = (SettingsPage)filamentprofiles_page;
          break;
        case 5:
          TurnOffActiveFrame();
          active_frame = (SettingsPage)calibrationsettings_page;
          break;
        case 6:
          TurnOffActiveFrame();
          active_frame = (SettingsPage)proFeaturePanelFrame;
          break;
        case 7:
          TurnOffActiveFrame();
          active_frame = (SettingsPage)about_frame;
          break;
        case 8:
          Close();
          break;
      }
      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
      active_frame.Refresh();
      active_frame.OnOpen();
    }

    private void CreateAppearanceFrame(GUIHost host, Updater softwareUpdater, PopupMessageBox messagebox)
    {
      appearanceFrame = new AppearancePage(2, host, settingsManager, softwareUpdater, messagebox);
      tab_frame.AddChildElement((Element2D)appearanceFrame);
      appearanceFrame.Refresh();
    }

    private void CreateManualControlsFrame(GUIHost host, SpoolerConnection spooler_connection, SettingsManager settingsManager)
    {
      manualControlsFrame = new ManualControlsFrame(host, spooler_connection, messagebox, settingsManager);
      tab_frame.AddChildElement((Element2D)manualControlsFrame);
      manualControlsFrame.Refresh();
    }

    private void CreateFilamentProfilesFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      filamentprofiles_page = new FilamentProfilePage(4, settingsManager, host, spooler_connection, messagebox);
      tab_frame.AddChildElement((Element2D)filamentprofiles_page);
      filamentprofiles_page.Refresh();
    }

    private void CreateProFeaturesFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      proFeaturePanelFrame = new FeaturePanel(6, host, spooler_connection);
      tab_frame.AddChildElement((Element2D)proFeaturePanelFrame);
      proFeaturePanelFrame.Refresh();
    }

    private void CreateCalibrationFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      calibrationsettings_page = new CalibrationSettingsFrame(5, settingsManager, messagebox, spooler_connection)
      {
        X = 0,
        Y = 0,
        RelativeWidth = 1f,
        RelativeHeight = 1f,
        Visible = false,
        Enabled = false
      };
      calibrationsettings_page.Init(host);
      tab_frame.AddChildElement((Element2D)calibrationsettings_page);
    }

    private void CreateAboutFrame(GUIHost host)
    {
      about_frame = new AboutPage(7)
      {
        X = 0,
        Y = 0,
        RelativeWidth = 1f,
        RelativeHeight = 1f,
        Visible = false,
        Enabled = false
      };
      about_frame.Init(host);
      tab_frame.AddChildElement((Element2D)about_frame);
    }

    private void TurnOffActiveFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.OnClose();
      active_frame.Visible = false;
      active_frame.Enabled = false;
      active_frame = (SettingsPage) null;
    }

    public void UpdateSettings()
    {
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
    }

    public void SaveSettings()
    {
      if (appearanceFrame != null)
      {
        appearanceFrame.SaveSettings();
      }

      if (settingsManager == null)
      {
        return;
      }

      settingsManager.SaveSettings();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      Close();
      return true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      Visible = false;
      if (host.HasChildDialog)
      {
        host.GlobalChildDialog -= (Element2D) this;
      }

      if (active_frame != null)
      {
        active_frame.OnClose();
      }

      SaveSettings();
    }

    private enum SettingsButtons
    {
      Title,
      ManageFilament,
      Appearance,
      ManualControls,
      FilamentProfiles,
      CalibrationSettings,
      ProFeatures,
      About,
      Close,
      TabFrame,
    }
  }
}
