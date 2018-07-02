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
      var manualcontrolsframeTabbuttons = Resources.manualcontrolsframe_tabbuttons;
      Init(host, manualcontrolsframeTabbuttons, new ButtonCallback(tabsFrameButtonCallback));
      gCodeButton = (ButtonWidget)FindChildElement(3);
      advancedHeatedBedButton = (ButtonWidget)FindChildElement(4);
      basicButton = (ButtonWidget)FindChildElement(1);
      sdCardButton = (ButtonWidget)FindChildElement(5);
      TabFrame = (HorizontalLayout)FindChildElement(1004);
      Visible = false;
      Enabled = false;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      basicControlsFrame = new BasicControlsFrame(1001, host, messagebox, spooler_connection);
      AddChildElement(basicControlsFrame);
      basicControlsFrame.Refresh();
      diagnosticsFrame = new DiagnosticsFrame(1002, host, spooler_connection);
      AddChildElement(diagnosticsFrame);
      diagnosticsFrame.Refresh();
      advancedHeatedBedFrame = new AdvancedFrame(1004, host, messagebox, spooler_connection);
      AddChildElement(advancedHeatedBedFrame);
      advancedHeatedBedFrame.Refresh();
      sdCardFrame = new SDCardFrame(1004, host, messagebox, spooler_connection, settingsManager);
      AddChildElement(sdCardFrame);
      sdCardFrame.Refresh();
      gCodesFrame = new GCodeFrame(1003, host, messagebox, spooler_connection);
      AddChildElement(gCodesFrame);
      gCodesFrame.Refresh();
      active_frame = basicControlsFrame;
    }

    public override void SetVisible(bool bVisible)
    {
      if (!bVisible)
      {
        host.SetFocus(null);
      }

      base.SetVisible(bVisible);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.IsConnected())
      {
        if (selectedPrinter.isBusy && controls_enabled)
        {
          controls_enabled = false;
          DisableGroup(10001);
          if (!selectedPrinter.Info.FirmwareIsInvalid)
          {
            DisableGroup(10003);
          }
        }
        else if (!selectedPrinter.isBusy && !controls_enabled)
        {
          controls_enabled = true;
          EnableGroup(10001);
          EnableGroup(10003);
        }
        if (selectedPrinter.Info.FirmwareIsInvalid)
        {
          EnableGroup(10003);
        }

        if (selectedPrinter.Info.Status == PrinterStatus.Firmware_IsWaitingToPause || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPausedProcessing || selectedPrinter.Info.Status == PrinterStatus.Firmware_PrintingPaused)
        {
          controls_enabled = false;
          DisableGroup(10001);
          DisableGroup(10003);
        }
        EnableGroup(10002);
        if (!selectedPrinter.HasHeatedBed)
        {
          if (TabFrame.ChildList.Contains(advancedHeatedBedButton))
          {
            if (advancedHeatedBedButton.Checked)
            {
              basicButton.Checked = true;
            }

            TabFrame.RemoveChildElement(advancedHeatedBedButton);
            advancedHeatedBedButton.Enabled = false;
          }
        }
        else if (selectedPrinter.HasHeatedBed && !TabFrame.ChildList.Contains(advancedHeatedBedButton))
        {
          TabFrame.AddChildElement(advancedHeatedBedButton);
          advancedHeatedBedButton.Enabled = true;
          TabFrame.RemoveChildElement(gCodeButton);
          TabFrame.AddChildElement(gCodeButton);
        }
        if (!selectedPrinter.SDCardExtension.Available)
        {
          if (!TabFrame.ChildList.Contains(sdCardButton))
          {
            return;
          }

          if (sdCardButton.Checked)
          {
            basicButton.Checked = true;
          }

          TabFrame.RemoveChildElement(sdCardButton);
          sdCardButton.Enabled = false;
        }
        else
        {
          if (!selectedPrinter.SDCardExtension.Available || TabFrame.ChildList.Contains(sdCardButton))
          {
            return;
          }

          TabFrame.AddChildElement(sdCardButton);
          sdCardButton.Enabled = true;
          TabFrame.RemoveChildElement(gCodeButton);
          TabFrame.AddChildElement(gCodeButton);
        }
      }
      else
      {
        controls_enabled = false;
        DisableGroup(10001);
        DisableGroup(10002);
        DisableGroup(10003);
      }
    }

    private void TurnOffActiveFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.Visible = false;
      active_frame.Enabled = false;
      active_frame = null;
    }

    public void tabsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          TurnOffActiveFrame();
          active_frame = basicControlsFrame;
          break;
        case 2:
          TurnOffActiveFrame();
          active_frame = diagnosticsFrame;
          break;
        case 3:
          TurnOffActiveFrame();
          active_frame = gCodesFrame;
          break;
        case 4:
          TurnOffActiveFrame();
          active_frame = advancedHeatedBedFrame;
          break;
        case 5:
          TurnOffActiveFrame();
          active_frame = sdCardFrame;
          break;
      }
      if (active_frame != null)
      {
        active_frame.Enabled = true;
        active_frame.Visible = true;
        host.SetFocus(active_frame);
      }
      Refresh();
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
