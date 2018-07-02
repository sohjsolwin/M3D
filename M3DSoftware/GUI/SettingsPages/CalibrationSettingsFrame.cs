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
      calibration_settings.SetChecked(true);
      base.OnUnhide();
    }

    public void Init(GUIHost host)
    {
      X = 0;
      Y = 0;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      calibration_settings = new ButtonWidget(0, (Element2D)null)
      {
        Text = "Calibration",
        TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f),
        TextOverColor = new Color4(1f, 1f, 1f, 1f),
        TextDownColor = new Color4(1f, 1f, 1f, 1f),
        Size = FontSize.Medium
      };
      calibration_settings.SetCallback(new ButtonCallback(MyButtonCallback));
      calibration_settings.Init(host, "guicontrols", 513f, 64f, 575f, (float) sbyte.MaxValue, 513f, 128f, 575f, 191f, 513f, 192f, 575f, (float) byte.MaxValue);
      calibration_settings.SetGrowableWidth(16, 16, 48);
      calibration_settings.DontMove = true;
      calibration_settings.SetPosition(10, 10);
      calibration_settings.SetSize(200, 32);
      calibration_settings.ClickType = ButtonType.Checkable;
      calibration_settings.GroupID = 1234;
      advancedcalibration_button = new ButtonWidget(1, (Element2D)null)
      {
        Text = "Advanced Calibration",
        TextColor = new Color4(0.71f, 0.71f, 0.71f, 1f),
        TextOverColor = new Color4(1f, 1f, 1f, 1f),
        TextDownColor = new Color4(1f, 1f, 1f, 1f),
        Size = FontSize.Medium
      };
      advancedcalibration_button.SetCallback(new ButtonCallback(MyButtonCallback));
      advancedcalibration_button.Init(host, "guicontrols", 576f, 64f, 639f, (float) sbyte.MaxValue, 576f, 128f, 639f, 191f, 576f, 192f, 639f, (float) byte.MaxValue);
      advancedcalibration_button.SetGrowableWidth(16, 16, 48);
      advancedcalibration_button.DontMove = true;
      advancedcalibration_button.ClickType = ButtonType.Checkable;
      advancedcalibration_button.GroupID = 1234;
      advancedcalibration_button.SetPosition(210, 10);
      advancedcalibration_button.SetSize(200, 32);
      AddChildElement((Element2D)calibration_settings);
      AddChildElement((Element2D)advancedcalibration_button);
      var frame = new Frame(3)
      {
        X = 0,
        Y = 50,
        RelativeWidth = 1f,
        RelativeHeight = 0.85f,
        Enabled = true,
        IgnoreMouse = false
      };
      advanced_calibration_tab = new AdvancedCalibrationTab(1001, main_controller, messagebox, spooler_connection);
      advanced_calibration_tab.Init(host);
      advanced_calibration_tab.Visible = false;
      advanced_calibration_tab.Enabled = false;
      advanced_calibration_tab.RelativeWidth = 1f;
      advanced_calibration_tab.RelativeHeight = 1f;
      advanced_calibration_tab.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      advanced_calibration_tab.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      frame.AddChildElement((Element2D)advanced_calibration_tab);
      calibration_tab = new CatScreenTab(1002, main_controller, spooler_connection, messagebox);
      calibration_tab.Init(host);
      calibration_tab.Visible = true;
      calibration_tab.Enabled = true;
      calibration_tab.RelativeWidth = 1f;
      calibration_tab.RelativeHeight = 1f;
      frame.AddChildElement((Element2D)calibration_tab);
      calibration_tab.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      calibration_tab.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      AddChildElement((Element2D) frame);
      calibration_settings.SetChecked(true);
      active_frame = (SettingsPage)calibration_tab;
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          DeactivateFrame();
          active_frame = (SettingsPage)calibration_tab;
          ActivateFrame();
          break;
        case 1:
          DeactivateFrame();
          active_frame = (SettingsPage)advanced_calibration_tab;
          ActivateFrame();
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
      if (active_frame == null)
      {
        return;
      }

      active_frame.OnClose();
      active_frame.Enabled = false;
      active_frame.Visible = false;
      active_frame = (SettingsPage) null;
    }

    private void ActivateFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.Enabled = true;
      active_frame.Visible = true;
      active_frame.OnOpen();
    }

    public void SendCalibrateBedLocationMessage()
    {
      if (active_frame != advanced_calibration_tab)
      {
        DeactivateFrame();
        active_frame = (SettingsPage)advanced_calibration_tab;
        ActivateFrame();
      }
      advancedcalibration_button.Checked = true;
    }

    public override void OnOpen()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.OnOpen();
    }

    public override void OnClose()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.OnClose();
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
