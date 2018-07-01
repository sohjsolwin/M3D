// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.CalibrationSettingsFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.SettingsPages.Calibration_Tabs;
using OpenTK.Graphics;

namespace M3D.GUI.SettingsPages
{
  public class CalibrationSettingsFrame : SettingsPage
  {
    private ButtonWidget calibration_settings;
    private SettingsManager main_controller;
    private SpoolerConnection spooler_connection;
    private AdvancedCalibrationTab advanced_calibration_tab;
    private ButtonWidget advancedcalibration_button;
    private CatScreenTab calibration_tab;
    private SettingsPage active_frame;
    private PopupMessageBox messagebox;

    public CalibrationSettingsFrame(int ID, SettingsManager main_controller, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.main_controller = main_controller;
      this.spooler_connection = spooler_connection;
      this.messagebox = messagebox;
    }

    public override void OnParentResize()
    {
      base.OnParentResize();
    }

    protected override void OnUnhide()
    {
      this.calibration_settings.SetChecked(true);
      base.OnUnhide();
    }

    public void Init(GUIHost host)
    {
      this.X = 0;
      this.Y = 0;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      this.calibration_settings = new ButtonWidget(0, (Element2D) null);
      this.calibration_settings.Text = "Calibration";
      this.calibration_settings.TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f);
      this.calibration_settings.TextOverColor = new Color4(1f, 1f, 1f, 1f);
      this.calibration_settings.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      this.calibration_settings.Size = FontSize.Medium;
      this.calibration_settings.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.calibration_settings.Init(host, "guicontrols", 513f, 64f, 575f, (float) sbyte.MaxValue, 513f, 128f, 575f, 191f, 513f, 192f, 575f, (float) byte.MaxValue);
      this.calibration_settings.SetGrowableWidth(16, 16, 48);
      this.calibration_settings.DontMove = true;
      this.calibration_settings.SetPosition(10, 10);
      this.calibration_settings.SetSize(200, 32);
      this.calibration_settings.ClickType = ButtonType.Checkable;
      this.calibration_settings.GroupID = 1234;
      this.advancedcalibration_button = new ButtonWidget(1, (Element2D) null);
      this.advancedcalibration_button.Text = "Advanced Calibration";
      this.advancedcalibration_button.TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f);
      this.advancedcalibration_button.TextOverColor = new Color4(1f, 1f, 1f, 1f);
      this.advancedcalibration_button.TextDownColor = new Color4(1f, 1f, 1f, 1f);
      this.advancedcalibration_button.Size = FontSize.Medium;
      this.advancedcalibration_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.advancedcalibration_button.Init(host, "guicontrols", 576f, 64f, 639f, (float) sbyte.MaxValue, 576f, 128f, 639f, 191f, 576f, 192f, 639f, (float) byte.MaxValue);
      this.advancedcalibration_button.SetGrowableWidth(16, 16, 48);
      this.advancedcalibration_button.DontMove = true;
      this.advancedcalibration_button.ClickType = ButtonType.Checkable;
      this.advancedcalibration_button.GroupID = 1234;
      this.advancedcalibration_button.SetPosition(210, 10);
      this.advancedcalibration_button.SetSize(200, 32);
      this.AddChildElement((Element2D) this.calibration_settings);
      this.AddChildElement((Element2D) this.advancedcalibration_button);
      Frame frame = new Frame(3);
      frame.X = 0;
      frame.Y = 50;
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.85f;
      frame.Enabled = true;
      frame.IgnoreMouse = false;
      this.advanced_calibration_tab = new AdvancedCalibrationTab(1001, this.main_controller, this.messagebox, this.spooler_connection);
      this.advanced_calibration_tab.Init(host);
      this.advanced_calibration_tab.Visible = false;
      this.advanced_calibration_tab.Enabled = false;
      this.advanced_calibration_tab.RelativeWidth = 1f;
      this.advanced_calibration_tab.RelativeHeight = 1f;
      this.advanced_calibration_tab.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      this.advanced_calibration_tab.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      frame.AddChildElement((Element2D) this.advanced_calibration_tab);
      this.calibration_tab = new CatScreenTab(1002, this.main_controller, this.spooler_connection, this.messagebox);
      this.calibration_tab.Init(host);
      this.calibration_tab.Visible = true;
      this.calibration_tab.Enabled = true;
      this.calibration_tab.RelativeWidth = 1f;
      this.calibration_tab.RelativeHeight = 1f;
      frame.AddChildElement((Element2D) this.calibration_tab);
      this.calibration_tab.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      this.calibration_tab.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      this.AddChildElement((Element2D) frame);
      this.calibration_settings.SetChecked(true);
      this.active_frame = (SettingsPage) this.calibration_tab;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          this.DeactivateFrame();
          this.active_frame = (SettingsPage) this.calibration_tab;
          this.ActivateFrame();
          break;
        case 1:
          this.DeactivateFrame();
          this.active_frame = (SettingsPage) this.advanced_calibration_tab;
          this.ActivateFrame();
          break;
      }
    }

    public override void OnRender(GUIHost host)
    {
      base.OnRender(host);
    }

    public override void SetVisible(bool bVisible)
    {
      base.SetVisible(bVisible);
    }

    private void DeactivateFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.OnClose();
      this.active_frame.Enabled = false;
      this.active_frame.Visible = false;
      this.active_frame = (SettingsPage) null;
    }

    private void ActivateFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.Enabled = true;
      this.active_frame.Visible = true;
      this.active_frame.OnOpen();
    }

    public void SendCalibrateBedLocationMessage()
    {
      if (this.active_frame != this.advanced_calibration_tab)
      {
        this.DeactivateFrame();
        this.active_frame = (SettingsPage) this.advanced_calibration_tab;
        this.ActivateFrame();
      }
      this.advancedcalibration_button.Checked = true;
    }

    public override void OnOpen()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.OnOpen();
    }

    public override void OnClose()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.OnClose();
    }

    private enum ControlIDs
    {
      CalibrationTab = 0,
      AdvancedCalibrationTab = 1,
      TabFrame = 2,
      MainFrame = 3,
      TabsGroup = 1234, // 0x000004D2
    }
  }
}
