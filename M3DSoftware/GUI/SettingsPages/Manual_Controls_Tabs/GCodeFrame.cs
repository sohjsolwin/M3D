// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.Manual_Controls_Tabs.GCodeFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      string manualcontrolsframeGcodes = Resources.manualcontrolsframe_gcodes;
      this.Init(host, manualcontrolsframeGcodes, new ButtonCallback(this.gCodesFrameButtonCallback));
      this.CenterHorizontallyInParent = true;
      this.RelativeY = 0.1f;
      this.RelativeWidth = 0.95f;
      this.RelativeHeight = 0.9f;
      this.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.Visible = false;
      this.Enabled = false;
      ((EditBoxWidget) this.FindChildElement(1030))?.SetCallbackEnterKey(new EditBoxWidget.EditBoxCallback(this.OnPressSendManualGCode));
      this.logwaits_checkbox = (ButtonWidget) this.FindChildElement(1034);
      this.logfeedback_checkbox = (ButtonWidget) this.FindChildElement(1035);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter != null && selectedPrinter.isConnected() && this.last_selected_printer != selectedPrinter)
      {
        this.updatingfromprinter = true;
        if (this.logwaits_checkbox != null)
          this.logwaits_checkbox.Checked = selectedPrinter.LogWaits;
        if (this.logfeedback_checkbox != null)
          this.logfeedback_checkbox.Checked = selectedPrinter.LogFeedback;
        this.updatingfromprinter = false;
      }
      if (selectedPrinter == null || !selectedPrinter.isConnected() || this.show_full_log)
      {
        if (this.spooler_connection.LogUpdated || this.last_selected_printer != null || this.log_changed)
        {
          ListBoxWidget childElement = (ListBoxWidget) this.FindChildElement(1031);
          if (childElement != null)
          {
            List<string> log = this.spooler_connection.GetLog();
            childElement.Items.Clear();
            foreach (string str in log)
              childElement.Items.Add((object) str);
            childElement.Refresh();
            childElement.SetTrackPositionToEnd();
          }
        }
      }
      else if (selectedPrinter.LogUpdated || this.last_selected_printer != selectedPrinter || this.log_changed)
      {
        ListBoxWidget childElement = (ListBoxWidget) this.FindChildElement(1031);
        if (childElement != null)
        {
          List<string> log = selectedPrinter.GetLog();
          childElement.Items.Clear();
          foreach (string str in log)
            childElement.Items.Add((object) str);
          childElement.Refresh();
          childElement.SetTrackPositionToEnd();
        }
      }
      this.log_changed = false;
      this.last_selected_printer = selectedPrinter;
    }

    public void gCodesFrameButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if ((selectedPrinter == null || !selectedPrinter.isConnected()) && button.ID != 1033)
        return;
      switch (button.ID)
      {
        case 1000:
          selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, "M0");
          break;
        case 1032:
          EditBoxWidget childElement = (EditBoxWidget) this.FindChildElement(1030);
          if (childElement == null)
            break;
          this.OnPressSendManualGCode(childElement);
          break;
        case 1033:
          if (selectedPrinter == null)
          {
            this.spooler_connection.ClearLog();
            break;
          }
          selectedPrinter.ClearLog();
          break;
        case 1034:
          if (this.updatingfromprinter || this.logwaits_checkbox == null)
            break;
          selectedPrinter.LogWaits = this.logwaits_checkbox.Checked;
          break;
        case 1035:
          if (this.updatingfromprinter || this.logfeedback_checkbox == null)
            break;
          selectedPrinter.LogFeedback = this.logfeedback_checkbox.Checked;
          break;
        case 1036:
          this.show_full_log = button.Checked;
          this.log_changed = true;
          break;
        case 1037:
          List<string> log = this.spooler_connection.GetLog();
          string text = "";
          foreach (string str in log)
            text = text + str + "\n";
          if (text.Length != 0)
          {
            Clipboard.SetText(text);
            this.messagebox.AddMessageToQueue("The Log has been copied to your clipboard.", PopupMessageBox.MessageBoxButtons.OK);
            break;
          }
          this.messagebox.AddMessageToQueue("There was nothing in your Log to copy.", PopupMessageBox.MessageBoxButtons.OK);
          break;
        case 1038:
          this.PrintGCodeFromFile();
          break;
      }
    }

    private void OnPressSendManualGCode(EditBoxWidget manual_g)
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.isConnected() || manual_g.Text == "")
        return;
      selectedPrinter.SendCommandAutoLockRelease(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, manual_g.Text);
    }

    private void PrintGCodeFromFile()
    {
      PrinterObject selectedPrinter1 = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter1 == null || !selectedPrinter1.isConnected())
        return;
      string sourceFileName = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.GCode);
      if (string.IsNullOrEmpty(sourceFileName))
        return;
      string str = Path.Combine(Paths.PublicDataFolder, "Working", "m3doutput.gcode");
      if (sourceFileName != str)
      {
        try
        {
          File.Copy(sourceFileName, str, true);
        }
        catch (Exception ex)
        {
          this.messagebox.AddMessageToQueue("Unable to send gcode file. " + ex.Message);
          return;
        }
      }
      JobParams UserJob = new JobParams(str, "User Job", "", selectedPrinter1.GetCurrentFilament().filament_type, 0.0f, 0.0f);
      UserJob.options.autostart_ignorewarnings = true;
      UserJob.options.turn_on_fan_before_print = true;
      UserJob.preprocessor = selectedPrinter1.MyFilamentProfile.preprocessor;
      UserJob.filament_temperature = selectedPrinter1.MyFilamentProfile.Temperature;
      PrinterObject selectedPrinter2 = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter2 == null || !selectedPrinter2.isConnected())
        this.messagebox.AddMessageToQueue("Printer Disconnected", PopupMessageBox.MessageBoxButtons.OK);
      else if (selectedPrinter2.PrinterState != PrinterObject.State.IsPrinting)
        selectedPrinter2.AutoLockAndPrint(UserJob);
      else
        this.messagebox.AddMessageToQueue("Printer Busy", PopupMessageBox.MessageBoxButtons.OK);
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
