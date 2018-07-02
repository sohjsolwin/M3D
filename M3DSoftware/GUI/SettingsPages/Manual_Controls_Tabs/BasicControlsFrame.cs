// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Manual_Controls_Tabs.BasicControlsFrame
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
  public class BasicControlsFrame : XMLFrame
  {
    private TextWidget heater_text;
    private PopupMessageBox messagebox;
    private SpoolerConnection spooler_connection;

    public BasicControlsFrame(int ID, GUIHost host, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      var manualcontrolsframeBasiccontrols = Resources.manualcontrolsframe_basiccontrols;
      Init(host, manualcontrolsframeBasiccontrols, new ButtonCallback(basicControlsFrameButtonCallback));
      CenterHorizontallyInParent = true;
      RelativeY = 0.1f;
      RelativeWidth = 0.95f;
      RelativeHeight = 0.9f;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      Visible = true;
      Enabled = true;
      heater_text = (TextWidget)FindChildElement(1020);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected())
      {
        if (heater_text == null)
        {
          return;
        }

        heater_text.Text = "OFF";
      }
      else if ((double) selectedPrinter.Info.extruder.Temperature == -1.0)
      {
        heater_text.Text = "ON";
      }
      else if ((double) selectedPrinter.Info.extruder.Temperature < 1.0)
      {
        heater_text.Text = "OFF";
      }
      else
      {
        heater_text.Text = selectedPrinter.Info.extruder.Temperature.ToString();
      }
    }

    public void basicControlsFrameButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected())
      {
        return;
      }

      switch (button.ID)
      {
        case 1000:
          var num = (int) selectedPrinter.SendEmergencyStop((AsyncCallback) null, (object) null);
          break;
        case 1001:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1016), "Z-", "The Z Value is not a number. Please correct and try again.", 90f);
          break;
        case 1002:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1016), "Z", "The Z Value is not a number. Please correct and try again.", 90f);
          break;
        case 1003:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1017), "X-", "The X Value is not a number. Please correct and try again.", 3000f);
          break;
        case 1004:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1017), "X", "The X Value is not a number. Please correct and try again.", 3000f);
          break;
        case 1005:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1018), "Y-", "The Y Value is not a number. Please correct and try again.", 3000f);
          break;
        case 1006:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1018), "Y", "The Y Value is not a number. Please correct and try again.", 3000f);
          break;
        case 1007:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1019), "E-", "The E Value is not a number. Please correct and try again.", 345f);
          break;
        case 1008:
          MovePrinterAxis(selectedPrinter, (EditBoxWidget)FindChildElement(1019), "E", "The E Value is not a number. Please correct and try again.", 345f);
          break;
        case 1010:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M17");
          break;
        case 1011:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M18");
          break;
        case 1012:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M106 S255");
          break;
        case 1013:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M106 S0");
          break;
        case 1014:
          FilamentSpool currentFilament = selectedPrinter.GetCurrentFilament();
          if (currentFilament == (FilamentSpool) null)
          {
            messagebox.AddMessageToQueue("Sorry, but you must insert filament first.");
            break;
          }
          var filamentTemperature = currentFilament.filament_temperature;
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, PrinterCompatibleString.Format("M109 S{0}", (object) filamentTemperature));
          break;
        case 1015:
          selectedPrinter.SendCommandAutoLockRelease(new AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M104 S0");
          break;
      }
    }

    private void MovePrinterAxis(PrinterObject selected_printer, EditBoxWidget amountEditBox, string axis, string errorString, float speed)
    {
      var text = amountEditBox.Text;
      if (!PrinterCompatibleString.VerifyNumber(text))
      {
        messagebox.AddMessageToQueue(errorString);
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(text);
        selected_printer.SendCommandAutoLockRelease(new AsyncCallback(selected_printer.ShowLockError), (object) selected_printer, "G91", PrinterCompatibleString.Format("G0 {0}{1} F{2}", (object) axis, (object) floatCurrentCulture, (object) speed));
      }
    }

    private enum BasicControlsID
    {
      Button_EmergencyStop = 1000, // 0x000003E8
      Button_ZDown = 1001, // 0x000003E9
      Button_ZUp = 1002, // 0x000003EA
      Button_XLeft = 1003, // 0x000003EB
      Button_XRight = 1004, // 0x000003EC
      Button_YForward = 1005, // 0x000003ED
      Button_YBack = 1006, // 0x000003EE
      Button_ERetract = 1007, // 0x000003EF
      Button_EExtrude = 1008, // 0x000003F0
      Button_MotorsOn = 1010, // 0x000003F2
      Button_MotorsOff = 1011, // 0x000003F3
      Button_FanOn = 1012, // 0x000003F4
      Button_FanOff = 1013, // 0x000003F5
      Button_HeaterOn = 1014, // 0x000003F6
      Button_HeaterOff = 1015, // 0x000003F7
      Button_ZAmountText = 1016, // 0x000003F8
      Button_XAmountText = 1017, // 0x000003F9
      Button_YAmountText = 1018, // 0x000003FA
      Button_EAmountText = 1019, // 0x000003FB
      Button_HeaterStatusText = 1020, // 0x000003FC
      HorizontalLayout = 2000, // 0x000007D0
    }
  }
}
