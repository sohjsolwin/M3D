// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Manual_Controls_Tabs.AdvancedFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using OpenTK.Graphics;

namespace M3D.GUI.SettingsPages.Manual_Controls_Tabs
{
  public class AdvancedFrame : XMLFrame
  {
    private TextWidget heater_text;
    private Frame heatedbedFrame;
    private PopupMessageBox messagebox;
    private SpoolerConnection spooler_connection;

    public AdvancedFrame(int ID, GUIHost host, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      var manualcontrolsframeAdvanced = Resources.manualcontrolsframe_advanced;
      Init(host, manualcontrolsframeAdvanced, new ButtonCallback(MyButtonCallback));
      CenterHorizontallyInParent = true;
      RelativeY = 0.1f;
      RelativeWidth = 0.95f;
      RelativeHeight = 0.9f;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      Visible = false;
      Enabled = false;
      heater_text = (TextWidget)FindChildElement(1020);
      heatedbedFrame = (Frame)FindChildElement(3000);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected() || !selectedPrinter.HasHeatedBed)
      {
        if (heater_text != null)
        {
          heater_text.Text = "OFF";
        }

        if (heatedbedFrame == null)
        {
          return;
        }

        heatedbedFrame.Enabled = false;
      }
      else
      {
        if (heatedbedFrame != null)
        {
          heatedbedFrame.Enabled = true;
        }

        if ((double) selectedPrinter.Info.accessories.BedStatus.BedTemperature == -1.0)
        {
          heater_text.Text = "ON";
        }
        else if ((double) selectedPrinter.Info.accessories.BedStatus.BedTemperature < 1.0)
        {
          heater_text.Text = "OFF";
        }
        else
        {
          heater_text.Text = selectedPrinter.Info.accessories.BedStatus.BedTemperature.ToString();
        }
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected())
      {
        return;
      }

      switch (button.ID)
      {
        case 1000:
          var num1 = (int) selectedPrinter.SendEmergencyStop((AsyncCallback) null, (object) null);
          break;
        case 1014:
          FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
          if (currentFilament == (FilamentSpool) null)
          {
            messagebox.AddMessageToQueue("Sorry, but you must insert filament first.");
            break;
          }
          var num2 = FilamentConstants.Temperature.BedDefault(currentFilament.filament_type);
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, PrinterCompatibleString.Format("M190 S{0}", (object) num2));
          break;
        case 1015:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M140 S0");
          break;
      }
    }

    private enum AdvancedControlsID
    {
      Button_EmergencyStop = 1000, // 0x000003E8
      Button_HeaterOn = 1014, // 0x000003F6
      Button_HeaterOff = 1015, // 0x000003F7
      Button_HeaterStatusText = 1020, // 0x000003FC
      HeatedBedFrame = 3000, // 0x00000BB8
    }
  }
}
