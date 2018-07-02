using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.GUI.Views;
using M3D.Properties;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace M3D.GUI.SettingsPages.Manual_Controls_Tabs
{
  public class GCodeFrame : XMLFrame
  {
    private bool updatingfromprinter;
    private bool log_changed;
    private bool show_full_log;
    private ButtonWidget logwaits_checkbox;
    private ButtonWidget logfeedback_checkbox;
    private PrinterObject last_selected_printer;
    private PopupMessageBox messagebox;
    private SpoolerConnection spooler_connection;

    public GCodeFrame(int ID, GUIHost host, PopupMessageBox messagebox, SpoolerConnection spooler_connection)
      : base(ID)
    {
      this.messagebox = messagebox;
      this.spooler_connection = spooler_connection;
      var manualcontrolsframeGcodes = Resources.manualcontrolsframe_gcodes;
      Init(host, manualcontrolsframeGcodes, new ButtonCallback(gCodesFrameButtonCallback));
      CenterHorizontallyInParent = true;
      RelativeY = 0.1f;
      RelativeWidth = 0.95f;
      RelativeHeight = 0.9f;
      BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      Visible = false;
      Enabled = false;
      ((EditBoxWidget)FindChildElement(1030))?.SetCallbackEnterKey(new EditBoxWidget.EditBoxCallback(OnPressSendManualGCode));
      logwaits_checkbox = (ButtonWidget)FindChildElement(1034);
      logfeedback_checkbox = (ButtonWidget)FindChildElement(1035);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.isConnected() && last_selected_printer != selectedPrinter)
      {
        updatingfromprinter = true;
        if (logwaits_checkbox != null)
        {
          logwaits_checkbox.Checked = selectedPrinter.LogWaits;
        }

        if (logfeedback_checkbox != null)
        {
          logfeedback_checkbox.Checked = selectedPrinter.LogFeedback;
        }

        updatingfromprinter = false;
      }
      if (selectedPrinter == null || !selectedPrinter.isConnected() || show_full_log)
      {
        if (spooler_connection.LogUpdated || last_selected_printer != null || log_changed)
        {
          var childElement = (ListBoxWidget)FindChildElement(1031);
          if (childElement != null)
          {
            List<string> log = spooler_connection.GetLog();
            childElement.Items.Clear();
            foreach (var str in log)
            {
              childElement.Items.Add((object) str);
            }

            childElement.Refresh();
            childElement.SetTrackPositionToEnd();
          }
        }
      }
      else if (selectedPrinter.LogUpdated || last_selected_printer != selectedPrinter || log_changed)
      {
        var childElement = (ListBoxWidget)FindChildElement(1031);
        if (childElement != null)
        {
          List<string> log = selectedPrinter.GetLog();
          childElement.Items.Clear();
          foreach (var str in log)
          {
            childElement.Items.Add((object) str);
          }

          childElement.Refresh();
          childElement.SetTrackPositionToEnd();
        }
      }
      log_changed = false;
      last_selected_printer = selectedPrinter;
    }

    public void gCodesFrameButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if ((selectedPrinter == null || !selectedPrinter.isConnected()) && button.ID != 1033)
      {
        return;
      }

      switch (button.ID)
      {
        case 1000:
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M0");
          break;
        case 1032:
          var childElement = (EditBoxWidget)FindChildElement(1030);
          if (childElement == null)
          {
            break;
          }

          OnPressSendManualGCode(childElement);
          break;
        case 1033:
          if (selectedPrinter == null)
          {
            spooler_connection.ClearLog();
            break;
          }
          selectedPrinter.ClearLog();
          break;
        case 1034:
          if (updatingfromprinter || logwaits_checkbox == null)
          {
            break;
          }

          selectedPrinter.LogWaits = logwaits_checkbox.Checked;
          break;
        case 1035:
          if (updatingfromprinter || logfeedback_checkbox == null)
          {
            break;
          }

          selectedPrinter.LogFeedback = logfeedback_checkbox.Checked;
          break;
        case 1036:
          show_full_log = button.Checked;
          log_changed = true;
          break;
        case 1037:
          List<string> log = spooler_connection.GetLog();
          var text = "";
          foreach (var str in log)
          {
            text = text + str + "\n";
          }

          if (text.Length != 0)
          {
            Clipboard.SetText(text);
            messagebox.AddMessageToQueue("The Log has been copied to your clipboard.", PopupMessageBox.MessageBoxButtons.OK);
            break;
          }
          messagebox.AddMessageToQueue("There was nothing in your Log to copy.", PopupMessageBox.MessageBoxButtons.OK);
          break;
        case 1038:
          PrintGCodeFromFile();
          break;
      }
    }

    private void OnPressSendManualGCode(EditBoxWidget manual_g)
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected() || manual_g.Text == "")
      {
        return;
      }

      selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, manual_g.Text);
    }

    private void PrintGCodeFromFile()
    {
      PrinterObject selectedPrinter1 = spooler_connection.SelectedPrinter;
      if (selectedPrinter1 == null || !selectedPrinter1.isConnected())
      {
        return;
      }

      var sourceFileName = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.GCode);
      if (string.IsNullOrEmpty(sourceFileName))
      {
        return;
      }

      var str = Path.Combine(Paths.PublicDataFolder, "Working", "m3doutput.gcode");
      if (sourceFileName != str)
      {
        try
        {
          File.Copy(sourceFileName, str, true);
        }
        catch (Exception ex)
        {
          messagebox.AddMessageToQueue("Unable to send gcode file. " + ex.Message);
          return;
        }
      }
      var UserJob = new JobParams(str, "User Job", "", selectedPrinter1.GetCurrentFilament().filament_type, 0.0f, 0.0f);
      UserJob.options.autostart_ignorewarnings = true;
      UserJob.options.turn_on_fan_before_print = true;
      UserJob.preprocessor = selectedPrinter1.MyFilamentProfile.preprocessor;
      UserJob.filament_temperature = selectedPrinter1.MyFilamentProfile.Temperature;
      PrinterObject selectedPrinter2 = spooler_connection.SelectedPrinter;
      if (selectedPrinter2 == null || !selectedPrinter2.isConnected())
      {
        messagebox.AddMessageToQueue("Printer Disconnected", PopupMessageBox.MessageBoxButtons.OK);
      }
      else if (selectedPrinter2.PrinterState != PrinterObject.State.IsPrinting)
      {
        selectedPrinter2.AutoLockAndPrint(UserJob);
      }
      else
      {
        messagebox.AddMessageToQueue("Printer Busy", PopupMessageBox.MessageBoxButtons.OK);
      }
    }

    private enum GCodeID
    {
      Button_EmergencyStop = 1000, // 0x000003E8
      EditBox_ManualGCode = 1030, // 0x00000406
      ListBox_PrinterLog = 1031, // 0x00000407
      Button_SendGcode = 1032, // 0x00000408
      Button_ClearLog = 1033, // 0x00000409
      Button_LogWaits = 1034, // 0x0000040A
      Button_LogFeedback = 1035, // 0x0000040B
      Button_ShowFullLog = 1036, // 0x0000040C
      Button_CopyLogToClipboard = 1037, // 0x0000040D
      Button_PrintFromGCodeFile = 1038, // 0x0000040E
    }
  }
}
