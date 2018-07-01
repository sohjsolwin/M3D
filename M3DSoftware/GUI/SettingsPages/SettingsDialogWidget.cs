// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.SettingsDialogWidget
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
      this.settingsManager = main_controller;
      this.messagebox = messagebox;
      this.Init(host, spooler_connection, softwareUpdater);
    }

    private void Init(GUIHost host, SpoolerConnection spooler_connection, Updater softwareUpdater)
    {
      this.host = host;
      this.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 37, 8, 64);
      this.SetSize(792, 356);
      TextWidget textWidget = new TextWidget(0);
      textWidget.SetPosition(50, 2);
      textWidget.SetSize(500, 35);
      textWidget.Text = "Settings";
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.Size = FontSize.Large;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget);
      int yposition1 = 35;
      Sprite.pixel_perfect = true;
      this.CreateTabButton(2, "T_SettingsTab_UserInterfaceOptions", yposition1);
      int yposition2 = yposition1 + 64;
      this.CreateTabButton(5, "T_SettingsTab_Calibration", yposition2);
      int yposition3 = yposition2 + 64;
      this.CreateTabButton(3, "T_SettingsTab_ExpertControls", yposition3);
      int yposition4 = yposition3 + 64;
      this.CreateTabButton(4, "T_SettingsTab_FilamentOptions", yposition4);
      int yposition5 = yposition4 + 64;
      this.CreateTabButton(6, "  Pro/Micro+\n  Features", yposition5);
      this.CreateTabButton(7, "T_SettingsTab_About", yposition5 + 64);
      ButtonWidget buttonWidget = new ButtonWidget(8);
      buttonWidget.X = -40;
      buttonWidget.Y = 4;
      buttonWidget.SetSize(32, 32);
      buttonWidget.Text = "";
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(host, "guicontrols", 704f, 320f, 735f, 351f, 736f, 320f, 767f, 351f, 704f, 352f, 735f, 383f);
      buttonWidget.DontMove = true;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      this.tab_frame = new Frame(9);
      this.tab_frame.X = 191;
      this.tab_frame.Y = 35;
      this.AddChildElement((Element2D) this.tab_frame);
      this.CreateAppearanceFrame(host, softwareUpdater, this.messagebox);
      this.CreateManualControlsFrame(host, spooler_connection, this.settingsManager);
      this.CreateFilamentProfilesFrame(host, spooler_connection);
      this.CreateProFeaturesFrame(host, spooler_connection);
      this.CreateCalibrationFrame(host, spooler_connection);
      this.CreateAboutFrame(host);
      this.about_frame.Visible = true;
      this.about_frame.Enabled = true;
      this.active_frame = (SettingsPage) this.about_frame;
      Sprite.pixel_perfect = false;
      this.Visible = false;
    }

    private ButtonWidget CreateTabButton(int ID, string text, int yposition)
    {
      ButtonWidget buttonWidget = new ButtonWidget(ID);
      buttonWidget.SetPosition(0, yposition);
      buttonWidget.SetSize(181, 64);
      buttonWidget.Text = text;
      buttonWidget.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      buttonWidget.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      buttonWidget.TextOverColor = new Color4(0.161f, 0.79f, 0.95f, 1f);
      buttonWidget.Alignment = QFontAlignment.Left;
      buttonWidget.Init(this.host, "guicontrols", 448f, 256f, 628f, 319f, 448f, 256f, 628f, 319f, 448f, 384f, 628f, 447f);
      buttonWidget.DontMove = true;
      buttonWidget.ClickType = ButtonType.Checkable;
      buttonWidget.GroupID = 18302;
      buttonWidget.Checked = false;
      buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      return buttonWidget;
    }

    public override void OnParentResize()
    {
      if (this.tab_frame != null)
        this.tab_frame.SetSize(this.Width - this.tab_frame.X - 10, this.Height - this.tab_frame.Y - 10);
      base.OnParentResize();
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public void ActivateAdvancedView()
    {
      this.ActivateAdvancedView(false);
    }

    public void ActivateAdvancedView(bool calibrate)
    {
      if (!calibrate)
        return;
      ButtonWidget childElement = (ButtonWidget) this.FindChildElement(5);
      if (childElement != null)
        childElement.Checked = true;
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
    }

    public void ActivateFilamentView()
    {
      ButtonWidget childElement = (ButtonWidget) this.FindChildElement(1);
      if (childElement != null)
        childElement.Checked = true;
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
    }

    public void ActivateChangeFilamentView()
    {
      ButtonWidget childElement = (ButtonWidget) this.FindChildElement(1);
      if (childElement == null)
        return;
      childElement.Checked = true;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 2:
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.appearanceFrame;
          break;
        case 3:
          if (this.settingsManager.ShowAllWarnings)
            this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_ManualControlsWarning"));
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.manualControlsFrame;
          break;
        case 4:
          if (this.settingsManager.ShowAllWarnings)
            this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, Locale.GlobalLocale.T("T_FilamentProfilesWarning")));
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.filamentprofiles_page;
          break;
        case 5:
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.calibrationsettings_page;
          break;
        case 6:
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.proFeaturePanelFrame;
          break;
        case 7:
          this.TurnOffActiveFrame();
          this.active_frame = (SettingsPage) this.about_frame;
          break;
        case 8:
          this.Close();
          break;
      }
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
      this.active_frame.Refresh();
      this.active_frame.OnOpen();
    }

    private void CreateAppearanceFrame(GUIHost host, Updater softwareUpdater, PopupMessageBox messagebox)
    {
      this.appearanceFrame = new AppearancePage(2, host, this.settingsManager, softwareUpdater, messagebox);
      this.tab_frame.AddChildElement((Element2D) this.appearanceFrame);
      this.appearanceFrame.Refresh();
    }

    private void CreateManualControlsFrame(GUIHost host, SpoolerConnection spooler_connection, SettingsManager settingsManager)
    {
      this.manualControlsFrame = new ManualControlsFrame(host, spooler_connection, this.messagebox, settingsManager);
      this.tab_frame.AddChildElement((Element2D) this.manualControlsFrame);
      this.manualControlsFrame.Refresh();
    }

    private void CreateFilamentProfilesFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      this.filamentprofiles_page = new FilamentProfilePage(4, this.settingsManager, host, spooler_connection, this.messagebox);
      this.tab_frame.AddChildElement((Element2D) this.filamentprofiles_page);
      this.filamentprofiles_page.Refresh();
    }

    private void CreateProFeaturesFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      this.proFeaturePanelFrame = new FeaturePanel(6, host, spooler_connection);
      this.tab_frame.AddChildElement((Element2D) this.proFeaturePanelFrame);
      this.proFeaturePanelFrame.Refresh();
    }

    private void CreateCalibrationFrame(GUIHost host, SpoolerConnection spooler_connection)
    {
      this.calibrationsettings_page = new CalibrationSettingsFrame(5, this.settingsManager, this.messagebox, spooler_connection);
      this.calibrationsettings_page.X = 0;
      this.calibrationsettings_page.Y = 0;
      this.calibrationsettings_page.RelativeWidth = 1f;
      this.calibrationsettings_page.RelativeHeight = 1f;
      this.calibrationsettings_page.Visible = false;
      this.calibrationsettings_page.Enabled = false;
      this.calibrationsettings_page.Init(host);
      this.tab_frame.AddChildElement((Element2D) this.calibrationsettings_page);
    }

    private void CreateAboutFrame(GUIHost host)
    {
      this.about_frame = new AboutPage(7);
      this.about_frame.X = 0;
      this.about_frame.Y = 0;
      this.about_frame.RelativeWidth = 1f;
      this.about_frame.RelativeHeight = 1f;
      this.about_frame.Visible = false;
      this.about_frame.Enabled = false;
      this.about_frame.Init(host);
      this.tab_frame.AddChildElement((Element2D) this.about_frame);
    }

    private void TurnOffActiveFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.OnClose();
      this.active_frame.Visible = false;
      this.active_frame.Enabled = false;
      this.active_frame = (SettingsPage) null;
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
      if (this.appearanceFrame != null)
        this.appearanceFrame.SaveSettings();
      if (this.settingsManager == null)
        return;
      this.settingsManager.SaveSettings();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      this.Close();
      return true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.SettingsDialog;
    }

    public override void Close()
    {
      this.Visible = false;
      if (this.host.HasChildDialog)
        this.host.GlobalChildDialog -= (Element2D) this;
      if (this.active_frame != null)
        this.active_frame.OnClose();
      this.SaveSettings();
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
