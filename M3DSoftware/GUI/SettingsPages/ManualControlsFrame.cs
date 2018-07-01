// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.ManualControlsFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.SettingsPages.Manual_Controls_Tabs;
using M3D.Properties;
using M3D.Spooling.Common;

namespace M3D.GUI.SettingsPages
{
  internal class ManualControlsFrame : SettingsPage
  {
    private bool controls_enabled = true;
    private GUIHost host;
    private SpoolerConnection spooler_connection;
    private XMLFrame basicControlsFrame;
    private XMLFrame diagnosticsFrame;
    private XMLFrame gCodesFrame;
    private XMLFrame advancedHeatedBedFrame;
    private XMLFrame sdCardFrame;
    private HorizontalLayout TabFrame;
    private ButtonWidget basicButton;
    private ButtonWidget gCodeButton;
    private ButtonWidget advancedHeatedBedButton;
    private ButtonWidget sdCardButton;
    private PopupMessageBox messagebox;
    private Frame active_frame;

    public ManualControlsFrame(GUIHost host, SpoolerConnection spooler_connection, PopupMessageBox messagebox, SettingsManager settingsManager)
    {
      this.host = host;
      this.spooler_connection = spooler_connection;
      this.messagebox = messagebox;
      string manualcontrolsframeTabbuttons = Resources.manualcontrolsframe_tabbuttons;
      this.Init(host, manualcontrolsframeTabbuttons, new ButtonCallback(this.tabsFrameButtonCallback));
      this.gCodeButton = (ButtonWidget) this.FindChildElement(3);
      this.advancedHeatedBedButton = (ButtonWidget) this.FindChildElement(4);
      this.basicButton = (ButtonWidget) this.FindChildElement(1);
      this.sdCardButton = (ButtonWidget) this.FindChildElement(5);
      this.TabFrame = (HorizontalLayout) this.FindChildElement(1004);
      this.Visible = false;
      this.Enabled = false;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      this.basicControlsFrame = (XMLFrame) new BasicControlsFrame(1001, host, messagebox, spooler_connection);
      this.AddChildElement((Element2D) this.basicControlsFrame);
      this.basicControlsFrame.Refresh();
      this.diagnosticsFrame = (XMLFrame) new DiagnosticsFrame(1002, host, spooler_connection);
      this.AddChildElement((Element2D) this.diagnosticsFrame);
      this.diagnosticsFrame.Refresh();
      this.advancedHeatedBedFrame = (XMLFrame) new AdvancedFrame(1004, host, messagebox, spooler_connection);
      this.AddChildElement((Element2D) this.advancedHeatedBedFrame);
      this.advancedHeatedBedFrame.Refresh();
      this.sdCardFrame = (XMLFrame) new SDCardFrame(1004, host, messagebox, spooler_connection, settingsManager);
      this.AddChildElement((Element2D) this.sdCardFrame);
      this.sdCardFrame.Refresh();
      this.gCodesFrame = (XMLFrame) new GCodeFrame(1003, host, messagebox, spooler_connection);
      this.AddChildElement((Element2D) this.gCodesFrame);
      this.gCodesFrame.Refresh();
      this.active_frame = (Frame) this.basicControlsFrame;
    }

    public override void SetVisible(bool bVisible)
    {
      if (!bVisible)
        this.host.SetFocus((Element2D) null);
      base.SetVisible(bVisible);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.isConnected())
      {
        if (selectedPrinter.isBusy && this.controls_enabled)
        {
          this.controls_enabled = false;
          this.DisableGroup(10001);
          if (!selectedPrinter.Info.FirmwareIsInvalid)
            this.DisableGroup(10003);
        }
        else if (!selectedPrinter.isBusy && !this.controls_enabled)
        {
          this.controls_enabled = true;
          this.EnableGroup(10001);
          this.EnableGroup(10003);
        }
        if (selectedPrinter.Info.FirmwareIsInvalid)
          this.EnableGroup(10003);
        if (selectedPrinter.Info.Status == PrinterStatus.Firmware_IsWaitingToPause || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused)
        {
          this.controls_enabled = false;
          this.DisableGroup(10001);
          this.DisableGroup(10003);
        }
        this.EnableGroup(10002);
        if (!selectedPrinter.HasHeatedBed)
        {
          if (this.TabFrame.ChildList.Contains((Element2D) this.advancedHeatedBedButton))
          {
            if (this.advancedHeatedBedButton.Checked)
              this.basicButton.Checked = true;
            this.TabFrame.RemoveChildElement((Element2D) this.advancedHeatedBedButton);
            this.advancedHeatedBedButton.Enabled = false;
          }
        }
        else if (selectedPrinter.HasHeatedBed && !this.TabFrame.ChildList.Contains((Element2D) this.advancedHeatedBedButton))
        {
          this.TabFrame.AddChildElement((Element2D) this.advancedHeatedBedButton);
          this.advancedHeatedBedButton.Enabled = true;
          this.TabFrame.RemoveChildElement((Element2D) this.gCodeButton);
          this.TabFrame.AddChildElement((Element2D) this.gCodeButton);
        }
        if (!selectedPrinter.SDCardExtension.Available)
        {
          if (!this.TabFrame.ChildList.Contains((Element2D) this.sdCardButton))
            return;
          if (this.sdCardButton.Checked)
            this.basicButton.Checked = true;
          this.TabFrame.RemoveChildElement((Element2D) this.sdCardButton);
          this.sdCardButton.Enabled = false;
        }
        else
        {
          if (!selectedPrinter.SDCardExtension.Available || this.TabFrame.ChildList.Contains((Element2D) this.sdCardButton))
            return;
          this.TabFrame.AddChildElement((Element2D) this.sdCardButton);
          this.sdCardButton.Enabled = true;
          this.TabFrame.RemoveChildElement((Element2D) this.gCodeButton);
          this.TabFrame.AddChildElement((Element2D) this.gCodeButton);
        }
      }
      else
      {
        this.controls_enabled = false;
        this.DisableGroup(10001);
        this.DisableGroup(10002);
        this.DisableGroup(10003);
      }
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
          this.active_frame = (Frame) this.basicControlsFrame;
          break;
        case 2:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.diagnosticsFrame;
          break;
        case 3:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.gCodesFrame;
          break;
        case 4:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.advancedHeatedBedFrame;
          break;
        case 5:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.sdCardFrame;
          break;
      }
      if (this.active_frame != null)
      {
        this.active_frame.Enabled = true;
        this.active_frame.Visible = true;
        this.host.SetFocus((Element2D) this.active_frame);
      }
      this.Refresh();
    }

    private enum TabButtons
    {
      Basic = 1,
      Diagnostics = 2,
      GCode = 3,
      HeatedBed = 4,
      SDCard = 5,
      TabFrame = 1004, // 0x000003EC
    }

    private enum ControlGroups
    {
      AutoDisableGroup = 10001, // 0x00002711
      EmergencyStopGroup = 10002, // 0x00002712
      UpdateFirmwareGroup = 10003, // 0x00002713
    }
  }
}
