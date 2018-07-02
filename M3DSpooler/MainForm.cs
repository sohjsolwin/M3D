using M3D.Spooler.Core;
using M3D.Spooler.Dialogs;
using M3D.Spooler.Forms;
using M3D.Spooler.Properties;
using M3D.Spooling;
using M3D.Spooling.Client;
using M3D.Spooling.Client.Extensions;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using M3D.Spooling.Core;
using M3D.Spooling.Preprocessors;
using M3D.Spooling.Preprocessors.Foundation;
using M3D.Spooling.Printer_Profiles;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace M3D.Spooler
{
  public class MainForm : Form
  {
    private string lastManualCommand = string.Empty;
    private ThreadSafeVariable<bool> PrinterLocked = new ThreadSafeVariable<bool>(false);
    private SampleSet m_sampleSetTemperatureSamples = new SampleSet(100);
    private ThreadSafeVariable<MainForm.PrinterLockChanging> LockChanging = new ThreadSafeVariable<MainForm.PrinterLockChanging>(MainForm.PrinterLockChanging.No);
    private bool printer_controls_open = true;
    private object _myCurrentPrinterObjectSync = new object();
    private static MainForm global_form;
    private const int DefaultLockTimeOut = 120;
    private SpoolerSettings settings;
    private ThreadSafeVariable<bool> shared_shutdown;
    private SpoolerClientBuiltIn spooler_client;
    private AdvancedStatistics advancedStatisticsDialog;
    private string Selected_Printer_Serial;
    public static bool AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT;
    private bool has_had_a_connected_client;
    private bool m_bSDTabSelected;
    private Stopwatch restart_counter;
    private bool in_check_printers;
    private ConcurrentDictionary<PrinterSerialNumber, MainForm.PrinterData> m_oPrinterData;
    private ThreadSafeVariable<RequestedFormTask> form_task;
    private bool m_bDontHideSpoolerJustShutDown;
    public bool m_bShutdownByUser;
    private bool force_shutdown;
    private bool job_stopped;
    private Timer timerLogger;
    private int firmwareUpdateStatusIndex;
    private Queue<string> logqueue;
    private ThreadSafeVariable<bool> logToScreen;
    private StayAwakeAndShutdown stay_awake;
    private SpoolerMessageHandler message_handler;
    private Printer MyCurrentPrinterObject;
    private const float XY_SPEED = 3000f;
    private const float Z_SPEED = 90f;
    private const float E_SPEED = 345f;
    private IContainer components;
    private Timer timer1;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStripMenuItem viewToolStripMenuItem;
    private GroupBox groupBoxPrinterList;
    private Button buttonStandAlone;
    private CheckBox checkBoxAutoCheckFirmware;
    private ListView listViewPrinterInfo;
    private ColumnHeader columnSerialNumber;
    private ColumnHeader columnStatus;
    private ColumnHeader columnTemp;
    public ColumnHeader columnZValid;
    private ColumnHeader columnJobStatus;
    private ColumnHeader columnFile;
    private ColumnHeader columnPerComplete;
    private ColumnHeader columnUser;
    private Label labelSpoolerVersion;
    private GroupBox groupBoxPrinterControls;
    private Button buttonLock;
    private GroupBox groupBoxControls;
    private TabControl tabControl1;
    private TabPage tabPageBasicOptions;
    private Button buttonHeaterOff;
    private Button buttonFanOff;
    private Button buttonFanOn;
    private TextBox textBoxEVal;
    private Button buttonPLATemp;
    private Button buttonDownE;
    private Button buttonEmergencyStop;
    private Button buttonUpE;
    private Button buttonABSTemp;
    private Button buttonAddJob;
    private Button buttonMotorsOff;
    private Button buttonMotorsOn;
    private TextBox textBoxYVal;
    private TextBox textBoxXVal;
    private Button buttonPrintToFile;
    private TextBox textBoxZVal;
    private Button button2;
    private Button buttonUpX;
    private Button button1;
    private Button buttonDownX;
    private Button buttonFilamentInfo;
    private Button buttonDownZ;
    private Button buttonDownY;
    private Button buttonUpZ;
    private Button buttonUpY;
    private TabPage tabPageDiagnostics;
    private Button buttonUpdateFirmware;
    private Button buttonPreprocess;
    private GroupBox groupBox4;
    private Button buttonBedPointTest;
    private ComboBox comboBoxBedTestPoint;
    private Button buttonBacklashPrint;
    private Button buttonGetGCode;
    private Button buttonReCenter;
    private Button buttonHome;
    private Button buttonSpeedTest;
    private Button buttonXSpeedTest;
    private Button buttonYSkipTestBack;
    private Button buttonXSkipTestRight;
    private Button buttonYSkipTest;
    private Button buttonXSkipTest;
    private Button buttonRepeatLast;
    private Label label3;
    private TextBox textBoxManualGCode;
    private Button buttonSendGCode;
    private Button buttonClearLog;
    private CheckBox checkBoxLogToScreen;
    private RichTextBox richTextBoxLoggedItems;
    private Label label5;
    private ListBox listBoxManualHistory;
    private Label label4;
    private CheckBox checkBoxAutoScroll;
    private ToolStripMenuItem showAdvancedToolStripMenuItem;
    private Label label1;
    private ComboBox selectedPrinterComboBox;
    private ToolStripMenuItem optionsToolStripMenuItem;
    private ToolStripMenuItem showAdvancedOptionsAtStartupToolStripMenuItem;
    private Button buttonTGHTemp;
    private Button buttonResumePrint;
    private Button buttonPausePrint;
    private ToolStripMenuItem showAdvancedStatisticsToolStripMenuItem;
    private Button buttonLoadEepromData;
    private Button buttonSaveEepromData;
    private ColumnHeader columnType;
    private Button buttonAbortPrint;
    private CheckBox logWaitsCheckBox;
    private GroupBox groupBoxBootloaderOptions;
    private GroupBox groupBoxFirmwareControls;
    private Button button3;
    private Label label2;
    private Button buttonQuitBootloader;
    private ColumnHeader columnBedTemp;
    private TabPage tabPageHeatedBedControl;
    private Button buttonTurnOfHeatedbed;
    private Button buttonBedHeatToABR;
    private Button buttonBedHeatToABS;
    private Button buttonBedHeatToPLA;
    private TabPage tabPageSDCard;
    private Button buttonSDDeleteFile;
    private Button buttonSDPrint;
    private Button buttonSDSaveGcode;
    private Button buttonSDRefresh;
    private ListBox listBoxSDFiles;
    private Label label6;
    private CheckBox checkBoxCalibrateBeforeSDPrint;
    private GroupBox groupBoxPrinting;
    private GroupBox groupBox1;
    private Label textBoxMeanTemperature;
    private Label label7;
    private GroupBox groupBox2;
    private Button buttonSetTemp;
    private TextBox textBoxTempEdit;
    private Label label8;
    private GroupBox groupBox3;

    public MainForm(SpoolerClientBuiltIn spooler_client)
    {
      m_oPrinterData = new ConcurrentDictionary<PrinterSerialNumber, MainForm.PrinterData>();
      form_task = new ThreadSafeVariable<RequestedFormTask>(RequestedFormTask.None);
      spooler_client.OnReceivedSpoolerShutdownMessage += new EventHandler<EventArgs>(OnReceivedSpoolerShutdownMessage);
      spooler_client.OnReceivedSpoolerShowMessage += new EventHandler<EventArgs>(OnReceivedSpoolerShowMessage);
      spooler_client.OnReceivedSpoolerHideMessage += new EventHandler<EventArgs>(OnReceivedSpoolerHideMessage);
      SpoolerClientBuiltIn spoolerClientBuiltIn1 = spooler_client;
      spoolerClientBuiltIn1.OnGotNewPrinter = spoolerClientBuiltIn1.OnGotNewPrinter + new SpoolerClient.OnGotNewPrinterDel(OnGotNewPrinter);
      SpoolerClientBuiltIn spoolerClientBuiltIn2 = spooler_client;
      spoolerClientBuiltIn2.OnPrinterDisconnected = spoolerClientBuiltIn2.OnPrinterDisconnected + new SpoolerClient.OnPrinterDisconnectedDel(OnPrinterDisconnected);
      stay_awake = new StayAwakeAndShutdown();
      stay_awake.StartUp(Handle);
      shared_shutdown = new ThreadSafeVariable<bool>(false);
      MainForm.global_form = this;
      InitializeComponent();
      InitializeTimers();
      logToScreen = new ThreadSafeVariable<bool>(false);
      logqueue = new Queue<string>();
      advancedStatisticsDialog = new AdvancedStatistics();
      buttonStandAlone.Visible = false;
      restart_counter = new Stopwatch();
      restart_counter.Reset();
      message_handler = new SpoolerMessageHandler(spooler_client);
      if (!MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT)
      {
        m_bDontHideSpoolerJustShutDown = true;
      }

      this.spooler_client = spooler_client;
    }

    private void OnLoad(object sender, EventArgs e)
    {
      SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(MainForm.SystemEvents_PowerModeChanged);
      settings = SpoolerSettings.LoadSettings();
      if (settings == null)
      {
        settings = new SpoolerSettings();
      }

      SpoolerClientBuiltIn spoolerClient1 = spooler_client;
      spoolerClient1.OnGotNewPrinter = spoolerClient1.OnGotNewPrinter + new SpoolerClient.OnGotNewPrinterDel(message_handler.OnGotNewPrinter);
      SpoolerClientBuiltIn spoolerClient2 = spooler_client;
      spoolerClient2.OnReceivedMessage = spoolerClient2.OnReceivedMessage + new OnReceivedMessageDel(message_handler.OnSpoolerMessage);
      spooler_client.ConnectInternalSpoolerToWindow(Handle);
      StartTimers();
      checkBoxAutoCheckFirmware.Checked = true;
      checkBoxAutoCheckFirmware.Enabled = false;
      OnAutoCheckFirmwareChanged((object) null, new EventArgs());
      buttonLoadEepromData.Visible = false;
      viewToolStripMenuItem.DropDownItems.Remove((ToolStripItem)showAdvancedStatisticsToolStripMenuItem);
      labelSpoolerVersion.Text = "Spooler Version: " + M3D.Spooling.Version.VersionText;
      showAdvancedOptionsAtStartupToolStripMenuItem.Checked = settings.StartAdvanced;
      if (settings.StartAdvanced)
      {
        return;
      }

      ClosePrinterControls();
    }

    private void ClosePrinterControls()
    {
      if (!printer_controls_open)
      {
        return;
      }

      groupBoxPrinterControls.Visible = false;
      groupBoxPrinterList.Location = new Point(1, 22);
      Height = 190;
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject != null && MyCurrentPrinterObject.HasLock)
        {
          var num = (int)MyCurrentPrinterObject.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        }
        MyCurrentPrinterObject = (Printer) null;
      }
      LockChanging.Value = MainForm.PrinterLockChanging.No;
      PrinterLocked.Value = false;
      printer_controls_open = false;
      showAdvancedToolStripMenuItem.Text = "Show Advanced Options";
    }

    private void OpenPrinterControls()
    {
      if (printer_controls_open)
      {
        return;
      }

      Height = 585;
      groupBoxPrinterControls.Visible = true;
      buttonPausePrint.Enabled = false;
      buttonResumePrint.Enabled = false;
      buttonAbortPrint.Enabled = false;
      groupBoxControls.Enabled = false;
      checkBoxLogToScreen.Checked = false;
      groupBoxPrinterList.Location = new Point(1, 420);
      printer_controls_open = true;
      LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      showAdvancedToolStripMenuItem.Text = "Hide Advanced Options";
    }

    private void CheckLockState()
    {
      if (LockChanging.Value == MainForm.PrinterLockChanging.YesToLocked)
      {
        PrinterLocked.Value = true;
        groupBoxControls.Enabled = true;
        buttonLock.Text = "Release Printer";
        buttonLock.Enabled = true;
        LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      else if (LockChanging.Value == MainForm.PrinterLockChanging.YesToUnlocked)
      {
        PrinterLocked.Value = false;
        groupBoxControls.Enabled = false;
        buttonLock.Text = "Control This Printer";
        buttonLock.Enabled = true;
        LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      else if (LockChanging.Value == MainForm.PrinterLockChanging.NoFailed)
      {
        buttonLock.Enabled = true;
        LockChanging.Value = MainForm.PrinterLockChanging.No;
      }
      lock (_myCurrentPrinterObjectSync)
      {
        try
        {
          if (MyCurrentPrinterObject != null)
          {
            if (MyCurrentPrinterObject.InBootloaderMode)
            {
              groupBoxFirmwareControls.Visible = false;
              groupBoxPrinting.Visible = false;
              groupBoxBootloaderOptions.Visible = true;
            }
            else
            {
              if (MyCurrentPrinterObject.MyPrinterProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed && MyCurrentPrinterObject.Info.supportedFeatures.Available("Heated Bed Control", MyCurrentPrinterObject.MyPrinterProfile.SupportedFeaturesConstants))
              {
                if (!tabControl1.TabPages.Contains(tabPageHeatedBedControl))
                {
                  tabControl1.TabPages.Add(tabPageHeatedBedControl);
                }
              }
              else if (tabControl1.TabPages.Contains(tabPageHeatedBedControl))
              {
                tabControl1.TabPages.Remove(tabPageHeatedBedControl);
              }

              if (MyCurrentPrinterObject.Info.supportedFeatures.UsesSupportedFeatures && MyCurrentPrinterObject.Info.supportedFeatures.Available("Untethered Printing", MyCurrentPrinterObject.MyPrinterProfile.SupportedFeaturesConstants))
              {
                if (!tabControl1.TabPages.Contains(tabPageSDCard))
                {
                  tabControl1.TabPages.Add(tabPageSDCard);
                }
              }
              else if (tabControl1.TabPages.Contains(tabPageSDCard))
              {
                tabControl1.TabPages.Remove(tabPageSDCard);
              }

              groupBoxFirmwareControls.Visible = true;
              groupBoxBootloaderOptions.Visible = false;
              groupBoxPrinting.Visible = false;
            }
          }
          else
          {
            groupBoxControls.Enabled = false;
          }
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void OnClosing(object sender, FormClosingEventArgs e)
    {
      if (spooler_client == null)
      {
        return;
      }

      if (m_bShutdownByUser || force_shutdown || (e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing) || !printer_controls_open && m_bDontHideSpoolerJustShutDown)
      {
        if (!CanShutdownImmediately() && !force_shutdown && e.CloseReason != CloseReason.TaskManagerClosing)
        {
          if ((!m_bShutdownByUser || e.CloseReason == CloseReason.WindowsShutDown) && spooler_client.ClientCount > 0)
          {
            e.Cancel = true;
            return;
          }
          var num1 = (int) MessageBox.Show("Shutting down now will stop all print jobs and may cause you printer to lose calibration. Shut down anyway.", "M3D Print Spooler", MessageBoxButtons.YesNo);
          ClearShutdownMessage();
          var num2 = 7;
          if (num1 == num2)
          {
            e.Cancel = true;
            return;
          }
        }
        SpoolerSettings.SaveSettings(settings, true);
        StopTimers();
        MainForm.global_form = (MainForm) null;
        spooler_client.CloseSession();
        shared_shutdown.Value = true;
        if (stay_awake == null)
        {
          return;
        }

        stay_awake.Shutdown();
      }
      else if (printer_controls_open)
      {
        ClosePrinterControls();
        e.Cancel = true;
      }
      else
      {
        TopMost = false;
        Visible = false;
        ShowInTaskbar = false;
        Hide();
        e.Cancel = true;
      }
    }

    public bool CanShutdownImmediately()
    {
      if (spooler_client.CanShutdown())
      {
        return true;
      }

      StayAwakeMethods.CreateShutdownMessage("This app is preventing shutdown because this will cancel print jobs.");
      return false;
    }

    public void ClearShutdownMessage()
    {
      StayAwakeMethods.DestroyShutdownMessage();
    }

    private void RefreshPrinters(bool redo_list = false)
    {
      ++firmwareUpdateStatusIndex;
      firmwareUpdateStatusIndex %= 5;
      try
      {
        if (spooler_client.IsBusy && !StayAwakeMethods.InStayAwakeMode())
        {
          StayAwakeMethods.NeverSleep();
        }
        else if (!spooler_client.IsBusy && StayAwakeMethods.InStayAwakeMode())
        {
          StayAwakeMethods.AllowSleep();
        }

        List<PrinterSerialNumber> serialNumbers = spooler_client.GetSerialNumbers();
        var flag = false;
        var str = (string) null;
        var driverInstallCount = spooler_client.PrinterDriverInstallCount;
        var num = driverInstallCount;
        foreach (ListViewItem listViewItem in listViewPrinterInfo.Items)
        {
          foreach (PrinterSerialNumber printerSerialNumber in serialNumbers)
          {
            if (listViewItem.Text == printerSerialNumber.ToString())
            {
              ++num;
              break;
            }
          }
        }
        if (listViewPrinterInfo.SelectedItems.Count > 0)
        {
          str = listViewPrinterInfo.SelectedItems[0].Text;
        }

        if (num != listViewPrinterInfo.Items.Count | redo_list)
        {
          listViewPrinterInfo.Items.Clear();
          selectedPrinterComboBox.Items.Clear();
          advancedStatisticsDialog.ClearList();
          flag = true;
        }
        List<PrinterInfo> printerInfo = spooler_client.GetPrinterInfo();
        AddPrinterInfoToListView(printerInfo, driverInstallCount);
        advancedStatisticsDialog.RefreshList(printerInfo);
        CheckForPrintFailureOnPowerOutage(printerInfo);
        if (!string.IsNullOrEmpty(str) & flag)
        {
          ListViewItem listViewItem = FindItem(listViewPrinterInfo, Selected_Printer_Serial);
          if (listViewItem != null && !listViewItem.Selected)
          {
            listViewItem.Selected = true;
          }
        }
        lock (_myCurrentPrinterObjectSync)
        {
          if (string.IsNullOrEmpty(Selected_Printer_Serial) && selectedPrinterComboBox.Items.Count > 0)
          {
            SetNewPrinter(selectedPrinterComboBox.Items[0].ToString());
          }
          else if (MyCurrentPrinterObject == null)
          {
            var selectedPrinterSerial = Selected_Printer_Serial;
            Selected_Printer_Serial = (string) null;
            SetNewPrinter(selectedPrinterSerial);
          }
        }
        if (MyCurrentPrinterObject != null)
        {
          if (MyCurrentPrinterObject.Info.current_job != null)
          {
            RefreshPrintingTemperatureStats(MyCurrentPrinterObject.Info);
          }
        }
      }
      catch (Exception ex)
      {
        var num = (int) MessageBox.Show("MainForm.UpdatePrinters" + ex.Message, "Exception");
        Selected_Printer_Serial = (string) null;
      }
      RefreshCurrentPrinter();
      if (MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT && has_had_a_connected_client)
      {
        if (spooler_client.ClientCount == 0 && !spooler_client.IsBusy)
        {
          force_shutdown = true;
          Close();
        }
      }
      else if (!MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT && has_had_a_connected_client && spooler_client.ClientCount == 0)
      {
        has_had_a_connected_client = false;
        m_bDontHideSpoolerJustShutDown = true;
      }
      if (has_had_a_connected_client || spooler_client.ClientCount <= 0)
      {
        return;
      }

      m_bDontHideSpoolerJustShutDown = false;
      has_had_a_connected_client = true;
    }

    private void AddPrinterInfoToListView(List<PrinterInfo> connected_printers, int numPrintersInstalling)
    {
      foreach (PrinterInfo connectedPrinter in connected_printers)
      {
        PrinterSerialNumber serialNumber = connectedPrinter.serial_number;
        PrinterProfile printerProfile = spooler_client.GetPrinterProfile(connectedPrinter.ProfileName);
        if (!(serialNumber == PrinterSerialNumber.Undefined))
        {
          if (!selectedPrinterComboBox.Items.Contains((object) serialNumber.ToString()))
          {
            selectedPrinterComboBox.Items.Add((object) serialNumber.ToString());
          }

          if (connectedPrinter != null)
          {
            ListViewItem listViewItem = FindItem(listViewPrinterInfo, connectedPrinter.serial_number.ToString());
            if (listViewItem == null)
            {
              listViewItem = listViewPrinterInfo.Items.Add(connectedPrinter.serial_number.ToString());
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("?");
              listViewItem.SubItems.Add("None");
              listViewItem.SubItems.Add(" ");
              listViewItem.SubItems.Add(" ");
              listViewItem.SubItems.Add(" ");
            }
            var profileName = connectedPrinter.ProfileName;
            if (listViewItem.SubItems[1].Text != profileName)
            {
              listViewItem.SubItems[1].Text = profileName;
            }

            var str1 = connectedPrinter.Status.ToString();
            if (connectedPrinter.Status == PrinterStatus.Bootloader_UpdatingFirmware)
            {
              str1 += " ";
              for (var index = 0; index < firmwareUpdateStatusIndex; ++index)
              {
                str1 += "-";
              }
            }
            if (listViewItem.SubItems[2].Text != str1)
            {
              listViewItem.SubItems[2].Text = str1;
            }

            var str2 = (double) connectedPrinter.extruder.Temperature != -1.0 ? ((double) connectedPrinter.extruder.Temperature >= 1.0 ? connectedPrinter.extruder.Temperature.ToString() : "OFF") : "ON";
            if (str2 != listViewItem.SubItems[3].Text)
            {
              listViewItem.SubItems[3].Text = str2;
            }

            var str3 = !printerProfile.AccessoriesConstants.HeatedBedConstants.HasBuiltinHeatedBed ? "N/A" : ((double) connectedPrinter.accessories.BedStatus.BedTemperature != -1.0 ? ((double) connectedPrinter.accessories.BedStatus.BedTemperature >= 1.0 ? connectedPrinter.accessories.BedStatus.BedTemperature.ToString() : "OFF") : "ON");
            if (str3 != listViewItem.SubItems[4].Text)
            {
              listViewItem.SubItems[4].Text = str3;
            }

            var str4 = !connectedPrinter.extruder.Z_Valid ? "Invalid" : "Valid";
            if (str4 != listViewItem.SubItems[5].Text)
            {
              listViewItem.SubItems[5].Text = str4;
            }

            JobInfo currentJob = connectedPrinter.current_job;
            string str5;
            string str6;
            string str7;
            string str8;
            if (currentJob != null)
            {
              str5 = currentJob.Status.ToString();
              str6 = currentJob.JobName;
              var f = currentJob.PercentComplete * 100f;
              str7 = float.IsNaN(f) || (double) f < 0.0 || (double) f > 100.0 ? "Processing" : f.ToString();
              str8 = currentJob.User;
            }
            else
            {
              str5 = "None";
              str6 = " ";
              str7 = " ";
              str8 = " ";
            }
            if (str5 != listViewItem.SubItems[6].Text)
            {
              listViewItem.SubItems[6].Text = str5;
            }

            if (listViewItem.SubItems[7].Text != str6)
            {
              listViewItem.SubItems[7].Text = str6;
            }

            if (listViewItem.SubItems[8].Text != str7)
            {
              listViewItem.SubItems[8].Text = str7;
            }

            if (listViewItem.SubItems[9].Text != str8)
            {
              listViewItem.SubItems[9].Text = str8;
            }
          }
        }
      }
      for (var index = 0; index < numPrintersInstalling; ++index)
      {
        var printerSerialNumber = new PrinterSerialNumber(index.ToString("X16"));
        if (FindItem(listViewPrinterInfo, printerSerialNumber.ToString()) == null)
        {
          ListViewItem listViewItem = listViewPrinterInfo.Items.Add(printerSerialNumber.ToString());
          listViewItem.SubItems.Add("Driver Installing");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("?");
          listViewItem.SubItems.Add("None");
          listViewItem.SubItems.Add(" ");
          listViewItem.SubItems.Add(" ");
          listViewItem.SubItems.Add(" ");
        }
      }
    }

    private void CheckForPrintFailureOnPowerOutage(List<PrinterInfo> connected_printers)
    {
      foreach (PrinterInfo connectedPrinter in connected_printers)
      {
        if (connectedPrinter.powerRecovery.bPowerOutageWhilePrinting)
        {
          var flag = false;
          if (m_oPrinterData.ContainsKey(connectedPrinter.serial_number))
          {
            flag = m_oPrinterData[connectedPrinter.serial_number].bPowerOutageHandled;
          }
          else
          {
            m_oPrinterData.TryAdd(connectedPrinter.serial_number, new MainForm.PrinterData());
          }

          m_oPrinterData[connectedPrinter.serial_number].bPowerOutageHandled = true;
          if (!flag && message_handler != null)
          {
            message_handler.OnSpoolerMessage(new SpoolerMessage(MessageType.PowerOutageWhilePrinting, connectedPrinter.serial_number, (string) null));
          }
        }
      }
    }

    private void RefreshCurrentPrinter()
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject != null)
        {
          if (!MyCurrentPrinterObject.HasLock && PrinterLocked.Value)
          {
            LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
          }

          CheckLockState();
          var flag1 = true;
          if (!MyCurrentPrinterObject.Connected && !MyCurrentPrinterObject.Switching)
          {
            SetNewPrinter((string) null);
          }

          try
          {
            if (MyCurrentPrinterObject.Info != null)
            {
              var flag2 = MyCurrentPrinterObject.Info.current_job != null;
              var flag3 = MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_PrintingPaused;
              if (flag2)
              {
                if (MyCurrentPrinterObject.Info.current_job.Status != JobStatus.SavingToSD)
                {
                  if (MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_Printing)
                  {
                    buttonResumePrint.Enabled = false;
                    buttonResumePrint.Visible = false;
                    buttonPausePrint.Enabled = true;
                    buttonPausePrint.Visible = true;
                    flag1 = false;
                  }
                  else if (MyCurrentPrinterObject.Info.Status == PrinterStatus.Firmware_PrintingPaused)
                  {
                    if (groupBoxControls.Enabled)
                    {
                      buttonResumePrint.Enabled = true;
                    }

                    buttonResumePrint.Visible = true;
                    buttonPausePrint.Enabled = false;
                    buttonPausePrint.Visible = false;
                    flag1 = false;
                  }
                }
                buttonAbortPrint.Enabled = true;
              }
              else
              {
                buttonAbortPrint.Enabled = false;
              }

              var flag4 = MyCurrentPrinterObject.Info.Status == PrinterStatus.Bootloader_UpdatingFirmware;
              ControlsEnable(MyCurrentPrinterObject.Connected && !(flag2 | flag4) | flag3);
            }
          }
          catch (Exception ex)
          {
          }
          if (!flag1)
          {
            return;
          }

          buttonResumePrint.Enabled = false;
          buttonResumePrint.Visible = false;
          buttonPausePrint.Enabled = false;
          buttonPausePrint.Visible = true;
        }
        else
        {
          groupBoxControls.Enabled = false;
        }
      }
    }

    private void RefreshPrintingTemperatureStats(PrinterInfo printerInfo)
    {
      var num1 = (double) printerInfo.extruder.Temperature > 0.0 ? (double) printerInfo.extruder.Temperature : 0.0;
      m_sampleSetTemperatureSamples.Add(num1);
      var sampleMean = (double)m_sampleSetTemperatureSamples.SampleMean;
      var num2 = num1 - sampleMean;
      byte num3 = 0;
      byte num4 = 0;
      byte num5 = 0;
      if (num2 < 0.0)
      {
        if (num2 < -5.0)
        {
          num2 = -5.0;
        }

        num4 = (byte) (-num2 * (double) byte.MaxValue / 5.0);
      }
      else if (num2 > 0.0)
      {
        if (num2 > 5.0)
        {
          num2 = 5.0;
        }

        num3 = (byte) (num2 * (double) byte.MaxValue / 5.0);
      }
      if (num2 > 2.0 || num2 < -2.0 || num1 < 150.0)
      {
        textBoxTempEdit.Enabled = false;
        buttonSetTemp.Enabled = false;
      }
      else
      {
        textBoxTempEdit.Enabled = true;
        buttonSetTemp.Enabled = true;
      }
      textBoxMeanTemperature.ForeColor = System.Drawing.Color.FromArgb((int) num3, (int) num5, (int) num4);
      textBoxMeanTemperature.Text = num1.ToString("0.00");
    }

    private void ControlsEnable(bool flag)
    {
      GroupBox boxPrinterControls = groupBoxPrinterControls;
      foreach (Control control in GetAll((Control) boxPrinterControls, new HashSet<System.Type>()
      {
        typeof (TextBox),
        typeof (Button)
      }))
      {
        if (!(control.Name == "buttonEmergencyStop") && !(control.Name == "buttonStandAlone") && (!(control.Name == "buttonClearLog") && !(control.Name == "buttonFanOff")) && (!(control.Name == "buttonFanOn") && !(control.Name == "buttonSetClear") && (!(control.Name == "buttonLock") && !(control.Name == "buttonResumePrint"))) && (!(control.Name == "buttonPausePrint") && !(control.Name == "buttonAbortPrint") && (!(control.Name == "textBoxTempEdit") && !(control.Name == "buttonSetTemp"))))
        {
          control.Enabled = flag;
        }
      }
    }

    public IEnumerable<Control> GetAll(Control control, HashSet<System.Type> types)
    {
      IEnumerable<Control> controls = control.Controls.Cast<Control>();
      return controls.SelectMany<Control, Control>((Func<Control, IEnumerable<Control>>) (ctrl => GetAll(ctrl, types))).Concat<Control>(controls).Where<Control>((Func<Control, bool>) (c => types.Contains(c.GetType())));
    }

    private Printer SelectedPrinter
    {
      get
      {
        if (spooler_client != null && !string.IsNullOrEmpty(Selected_Printer_Serial))
        {
          return spooler_client.GetPrinter(new PrinterSerialNumber(Selected_Printer_Serial));
        }

        return (Printer) null;
      }
    }

    private void DoUpdates(object sender, EventArgs e)
    {
      if (in_check_printers)
      {
        return;
      }

      in_check_printers = true;
      if (restart_counter.IsRunning)
      {
        if (restart_counter.ElapsedMilliseconds > 10000L)
        {
          restart_counter.Stop();
          if (job_stopped)
          {
            job_stopped = false;
            if (message_handler != null)
            {
              spooler_client.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, "Print jobs cancelled because your computer went into sleep mode. Please restart your printer by disconnecting it from power and then reconnecting it to power.").Serialize());
            }
          }
          else if (message_handler != null)
          {
            spooler_client.BroadcastMessage(new SpoolerMessage(MessageType.UserDefined, "Your computer went into sleep mode. Please restart your printer by disconnecting it from power and then reconnecting it to power.").Serialize());
          }
        }
        else if (restart_counter.ElapsedMilliseconds > 5000L)
        {
          spooler_client.DisconnectAllPrinters();
        }
      }
      else
      {
        DoFormTask();
        RefreshPrinters(false);
      }
      in_check_printers = false;
    }

    private ListViewItem FindItem(ListView view, string text)
    {
      for (var index = 0; index < view.Items.Count; ++index)
      {
        if (view.Items[index].Text == text)
        {
          return view.Items[index];
        }
      }
      return (ListViewItem) null;
    }

    private static void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      if (MainForm.global_form == null)
      {
        return;
      }

      SpoolerClientBuiltIn spoolerClient = MainForm.global_form.spooler_client;
      if (spoolerClient != null)
      {
        return;
      }

      if (e.Mode == PowerModes.Suspend)
      {
        MainForm.global_form.job_stopped = spoolerClient.IsBusy;
        spoolerClient.DisconnectAllPrinters();
      }
      else
      {
        if (e.Mode != PowerModes.Resume)
        {
          return;
        }

        spoolerClient.DisconnectAllPrinters();
        MainForm.global_form.restart_counter.Reset();
        MainForm.global_form.restart_counter.Start();
      }
    }

    protected void ShowSpooler()
    {
      ShowInTaskbar = true;
      WindowState = FormWindowState.Normal;
      ShowInTaskbar = true;
      Show();
      BringToFront();
      Show();
    }

    private void DoFormTask()
    {
      RequestedFormTask requestedFormTask = form_task.Value;
      if (requestedFormTask == RequestedFormTask.Shutdown)
      {
        force_shutdown = true;
        Close();
      }
      else if (requestedFormTask == RequestedFormTask.Hide)
      {
        MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT = true;
        m_bShutdownByUser = false;
        Close();
      }
      else if (requestedFormTask == RequestedFormTask.Show)
      {
        MainForm.AUTO_SHUTDOWN_IF_ALL_CLIENTS_DISCONNECT = false;
        ShowSpooler();
      }
      if (requestedFormTask == RequestedFormTask.None)
      {
        return;
      }

      form_task.Value = RequestedFormTask.None;
    }

    private void OnReceivedSpoolerShutdownMessage(object sender, EventArgs e)
    {
      form_task.Value = RequestedFormTask.Shutdown;
    }

    private void OnReceivedSpoolerShowMessage(object sender, EventArgs e)
    {
      form_task.Value = RequestedFormTask.Show;
    }

    private void OnReceivedSpoolerHideMessage(object sender, EventArgs e)
    {
      form_task.Value = RequestedFormTask.Hide;
    }

    private void OnGotNewPrinter(Printer new_printer)
    {
      new_printer.RegisterPlugin(SDCardExtensions.ID, (IPrinterPlugin) new SDCardExtensions((IPrinter) new_printer));
      if (m_oPrinterData.ContainsKey(new_printer.Info.serial_number))
      {
        return;
      }

      m_oPrinterData.TryAdd(new_printer.Info.serial_number, new MainForm.PrinterData());
    }

    private void OnPrinterDisconnected(Printer new_printer)
    {
      if (!m_oPrinterData.ContainsKey(new_printer.Info.serial_number))
      {
        return;
      }

      m_oPrinterData.TryRemove(new_printer.Info.serial_number, out PrinterData printerData);
    }

    private void InitializeTimers()
    {
      timerLogger = new Timer(components);
      timerLogger.Tick += new EventHandler(timerLogger_tick);
    }

    private void StartTimers()
    {
      timer1.Start();
      timerLogger.Start();
    }

    private void StopTimers()
    {
      timer1.Stop();
      timerLogger.Stop();
    }

    private void OnLog(string message, string printer_serial)
    {
      lock (logqueue)
      {
        logqueue.Enqueue(string.Format("{0}::{1}", (object) printer_serial, (object) message));
      }
    }

    private void timerLogger_tick(object sender, EventArgs e)
    {
      timerLogger.Stop();
      lock (_myCurrentPrinterObjectSync)
      {
        lock (logqueue)
        {
          while (logqueue.Count > 0)
          {
            richTextBoxLoggedItems.AppendText(logqueue.Dequeue() + "\r\n");
          }
        }
        if (MyCurrentPrinterObject != null)
        {
          if (MyCurrentPrinterObject.LogUpdated)
          {
            if (logToScreen.Value)
            {
              List<string> nextLogItems = MyCurrentPrinterObject.GetNextLogItems();
              var num = 0;
              foreach (var str in nextLogItems)
              {
                if (logWaitsCheckBox.Checked || !(str == ">> wait"))
                {
                  richTextBoxLoggedItems.AppendText(str + "\r\n");
                  ++num;
                }
              }
              if (num > 0)
              {
                if (checkBoxAutoScroll.Checked)
                {
                  richTextBoxLoggedItems.SelectionStart = richTextBoxLoggedItems.Text.Length;
                  richTextBoxLoggedItems.ScrollToCaret();
                }
              }
            }
          }
        }
      }
      if (message_handler != null)
      {
        message_handler.ShowMessage();
      }

      timerLogger.Start();
    }

    private void richTextBoxLoggedItems_MouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Right)
      {
        return;
      }

      var contextMenu = new ContextMenu();
      var menuItem1 = new MenuItem("Select All");
      menuItem1.Click += new EventHandler(SelectAllAction);
      contextMenu.MenuItems.Add(menuItem1);
      var menuItem2 = new MenuItem("Copy");
      menuItem2.Click += new EventHandler(CopyAction);
      contextMenu.MenuItems.Add(menuItem2);
      richTextBoxLoggedItems.ContextMenu = contextMenu;
    }

    private void SelectAllAction(object sender, EventArgs e)
    {
      richTextBoxLoggedItems.SelectAll();
    }

    private void CopyAction(object sender, EventArgs e)
    {
      Clipboard.SetText(richTextBoxLoggedItems.SelectedText);
    }

    public StayAwakeAndShutdown StayAwakeMethods
    {
      get
      {
        return stay_awake;
      }
    }

    private void listViewPrinterInfo_DoubleClick(object sender, EventArgs e)
    {
      if (listViewPrinterInfo.SelectedItems.Count <= 0)
      {
        return;
      }

      var text = listViewPrinterInfo.SelectedItems[0].Text;
      if (text.StartsWith("00"))
      {
        return;
      }

      SetNewPrinter(text);
      OpenPrinterControls();
    }

    private void OnSelectedPrintComboBoxChanged(object sender, EventArgs e)
    {
      SetNewPrinter(selectedPrinterComboBox.Text);
    }

    private void showAdvancedStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!printer_controls_open)
      {
        OpenPrinterControls();
      }
      else
      {
        ClosePrinterControls();
      }
    }

    private void SetNewPrinter(string serial_number)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (Selected_Printer_Serial != serial_number)
        {
          Printer selectedPrinter1 = SelectedPrinter;
          if (selectedPrinter1 != null)
          {
            var num1 = (int) selectedPrinter1.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
          }
          ResetSDFileList();
          Selected_Printer_Serial = serial_number;
          Printer selectedPrinter2 = SelectedPrinter;
          if (selectedPrinter2 != null)
          {
            if (MyCurrentPrinterObject == null || selectedPrinter2.Info.MySerialNumber != MyCurrentPrinterObject.Info.MySerialNumber)
            {
              groupBoxPrinterControls.Text = selectedPrinter2.Info.MySerialNumber;
              var num2 = (int) selectedPrinter2.ReleaseLock((M3D.Spooling.Client.AsyncCallback) null, (object) null);
              MyCurrentPrinterObject = selectedPrinter2;
              MyCurrentPrinterObject.LockStepMode = false;
              LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
            }
            selectedPrinterComboBox.SelectedIndex = selectedPrinterComboBox.Items.IndexOf((object) serial_number);
            buttonLock.Enabled = true;
            buttonEmergencyStop.Enabled = true;
          }
          else
          {
            MyCurrentPrinterObject = (Printer) null;
            groupBoxPrinterControls.Text = "00-00-00-00-00-000-000";
            selectedPrinterComboBox.SelectedIndex = -1;
            buttonLock.Enabled = false;
            buttonEmergencyStop.Enabled = false;
          }
        }
        m_sampleSetTemperatureSamples.Clear();
      }
    }

    private void buttonStandAlone_Click(object sender, EventArgs e)
    {
    }

    private void OnAutoCheckFirmwareChanged(object sender, EventArgs e)
    {
      spooler_client.CheckFirmware = checkBoxAutoCheckFirmware.Checked;
    }

    private void buttonSetTemp_Click(object sender, EventArgs e)
    {
      Printer currentPrinterObject = MyCurrentPrinterObject;
      if (currentPrinterObject == null)
      {
        return;
      }

      if (int.TryParse(textBoxTempEdit.Text, out var result))
      {
        if ((double)Math.Abs((float)result - m_sampleSetTemperatureSamples.SampleMean) > 15.0)
        {
          var num1 = (int)MessageBox.Show("The new temperature must be with 15 degrees of the current temperature. Please try again");
        }
        else if (result < 180)
        {
          var num2 = (int)MessageBox.Show("The new temperature must be over 180 degrees. Please try again");
        }
        else
        {
          var num3 = (int)currentPrinterObject.SetTemperatureWhilePrinting(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)currentPrinterObject, result);
        }
      }
      else
      {
        var num4 = (int)MessageBox.Show("The temperature entered is not an integer. Please try again");
      }
    }

    private void buttonAddJob_Click(object sender, EventArgs e)
    {
      if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
      {
        return;
      }

      AddJobFromGCodeFile(false, JobParams.Mode.DirectPrinting);
    }

    private void AddJobFromGCodeFile(bool allow_untethered, JobParams.Mode jobMode)
    {
      var openFileDialog = new OpenFileDialog
      {
        Filter = "G-Code (*.gcode)|*.gcode|Binary G-Code (*.bgcode)|*.bgcode|Text Files (.txt)|*.txt|All Files (*.*)|*.*",
        FilterIndex = 1,
        Multiselect = false,
        CheckFileExists = true
      };
      if (openFileDialog.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      var fileName = openFileDialog.FileName;
      PrintOptions options = ManualPrintOptions.GetOptions(MyCurrentPrinterObject.GetCurrentFilament(), allow_untethered, jobMode);
      var flag = !options.use_preprocessors;
      var calibrateZ = options.calibrateZ;
      if (options.type == FilamentSpool.TypeEnum.OtherOrUnknown)
      {
        return;
      }

      Printer currentPrinterObject = MyCurrentPrinterObject;
      var jobParams = new JobParams(fileName, Path.GetFileNameWithoutExtension(fileName), "null", options.type, 0.0f, 0.0f)
      {
        jobMode = options.jobMode
      };
      jobParams.options.autostart_ignorewarnings = true;
      jobParams.options.dont_use_preprocessors = flag;
      jobParams.options.calibrate_before_print = calibrateZ;
      if (calibrateZ)
      {
        jobParams.options.calibrate_before_print_z = 0.4f;
        if (MyCurrentPrinterObject.Info.calibration.UsesCalibrationOffset)
        {
          jobParams.options.calibrate_before_print_z += MyCurrentPrinterObject.Info.calibration.CALIBRATION_OFFSET;
        }
      }
      var filamentProfile = FilamentProfile.CreateFilamentProfile(new FilamentSpool()
      {
        filament_type = options.type,
        filament_temperature = options.temperature
      }, currentPrinterObject.MyPrinterProfile);
      jobParams.preprocessor = filamentProfile.preprocessor;
      jobParams.filament_temperature = filamentProfile.Temperature;
      if (jobMode == JobParams.Mode.SaveToBinaryGCodeFile)
      {
        var saveFileDialog = new SaveFileDialog
        {
          Filter = "G-Code (*.gcode)|*.gcode|Binary G-Code (*.bgcode)|*.bgcode",
          FilterIndex = 1
        };
        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
          return;
        }

        jobParams.jobMode = jobMode;
        jobParams.outputfile = saveFileDialog.FileName;
      }
      var num = (int)MyCurrentPrinterObject.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
    }

    private void comboBoxActivePrinter_TextUpdate(object sender, EventArgs e)
    {
    }

    private void OnLogToScreenChanged(object sender, EventArgs e)
    {
      logToScreen.Value = checkBoxLogToScreen.Checked;
    }

    private void buttonClearLog_Click(object sender, EventArgs e)
    {
      richTextBoxLoggedItems.Clear();
    }

    private void OnEnterInsertGCode(object sender, EventArgs e)
    {
      AcceptButton = (IButtonControl)buttonSendGCode;
    }

    private void OnLeaveInsertGCode(object sender, EventArgs e)
    {
      AcceptButton = (IButtonControl) null;
    }

    private void buttonPreprocess_Click(object sender, EventArgs e)
    {
      var initial_x = 0.0f;
      var initial_y = 0.0f;
      var initial_speed = 0.0f;
      var default_speed = 0.0f;
      var max_speed = 0.0f;
      var flag = false;
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject != null)
        {
          if (MyCurrentPrinterObject.Connected)
          {
            initial_x = MyCurrentPrinterObject.Info.calibration.BACKLASH_X;
            initial_y = MyCurrentPrinterObject.Info.calibration.BACKLASH_Y;
            initial_speed = MyCurrentPrinterObject.Info.calibration.BACKLASH_SPEED;
            default_speed = MyCurrentPrinterObject.MyPrinterProfile.SpeedLimitConstants.DefaultBacklashSpeed;
            max_speed = MyCurrentPrinterObject.MyPrinterProfile.SpeedLimitConstants.FastestPossible;
            flag = true;
          }
        }
      }
      if (!flag)
      {
        return;
      }

      var backlashSettingsDialog = new BacklashSettingsDialog(initial_x, initial_y, initial_speed, default_speed, max_speed);
      var num1 = (int) backlashSettingsDialog.ShowDialog();
      if (!backlashSettingsDialog.ok)
      {
        return;
      }

      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null)
        {
          return;
        }

        if (!MyCurrentPrinterObject.HasLock)
        {
          var num2 = (int) MessageBox.Show("Unable to send command to the printer because the lock on the printer has timed out.");
        }
        else if (MyCurrentPrinterObject.Connected)
        {
          var num3 = (int)MyCurrentPrinterObject.SetBacklash((M3D.Spooling.Client.AsyncCallback) null, (object) null, new BacklashSettings(backlashSettingsDialog.X_BACKLASH, backlashSettingsDialog.Y_BACKLASH, backlashSettingsDialog.BACKLASH_SPEED));
        }
        else
        {
          var num4 = (int) MessageBox.Show("Unable to send command to the printer because the printer has disconnected.");
        }
      }
    }

    private void buttonPrintToFile_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        AddJobFromGCodeFile(false, JobParams.Mode.SaveToBinaryGCodeFile);
      }
    }

    private void buttonBacklashPrint_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.PrintBacklashPrint(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
      }
    }

    private void buttonGoToBootloader_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null)
        {
          return;
        }

        var num = MyCurrentPrinterObject.Connected ? 1 : 0;
      }
    }

    private void buttonQuitBootloader_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "Q");
      }
    }

    private void buttonUpdateFirmware_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
        var num = (int)MyCurrentPrinterObject.DoFirmwareUpdate((M3D.Spooling.Client.AsyncCallback) null, (object) null);
        SetNewPrinter((string) null);
      }
    }

    private void ProcessCallErrors(IAsyncCallResult ar)
    {
      var asyncState = ar.AsyncState as Printer;
      if (asyncState == null)
      {
        return;
      }

      switch (ar.CallResult)
      {
        case CommandResult.Success:
          break;
        case CommandResult.SuccessfullyReceived:
          break;
        case CommandResult.OverridedByNonLockStepCall:
          if (spooler_client.ClientCount != 0)
          {
            break;
          }

          OnLog("The previous command is still running, so your request has been queued.", asyncState.Info.MySerialNumber);
          break;
        case CommandResult.Failed_CannotPauseSavingToSD:
          var num1 = (int) MessageBox.Show("Sorry, but saving to the printer can't be paused.");
          break;
        case CommandResult.Failed_G28_G30_G32_NotAllowedWhilePaused:
          if (spooler_client.ClientCount != 0)
          {
            break;
          }

          var num2 = (int) MessageBox.Show("Homing and calibration are disabled while paused.");
          break;
        case CommandResult.CommandInterruptedByM0:
          if (spooler_client.ClientCount != 0)
          {
            break;
          }

          OnLog("The previous command was interrupted.", asyncState.Info.MySerialNumber);
          break;
        default:
          OnLog(ar.CallResult.ToString(), asyncState.Info.MySerialNumber);
          break;
      }
    }

    private void buttonSendGCode_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected || string.IsNullOrEmpty(textBoxManualGCode.Text))
        {
          return;
        }

        if (textBoxManualGCode.Text.IndexOf("backlash") == 0)
        {
          var num1 = (int)MyCurrentPrinterObject.PrintBacklashPrint(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
        }
        else
        {
          checkBoxLogToScreen.Checked = true;
          lastManualCommand = textBoxManualGCode.Text;
          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, textBoxManualGCode.Text);
          listBoxManualHistory.Items.Add((object) ("(" + MyCurrentPrinterObject.Info.MySerialNumber + ")->" + textBoxManualGCode.Text));
          textBoxManualGCode.Text = "";
        }
      }
    }

    private void buttonEmergencyStop_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendEmergencyStop(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
        listBoxManualHistory.Items.Add((object) ("(" + MyCurrentPrinterObject.Info.MySerialNumber + ")->Emergency Stop"));
      }
    }

    private void buttonPausePrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
      {
        return;
      }

      var num = (int) selectedPrinter.PausePrint(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
      listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Pause Print"));
    }

    private void buttonResumePrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected)
      {
        return;
      }

      var num = (int) selectedPrinter.ContinuePrint(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
      listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Resume Print"));
    }

    private void buttonAbortPrint_Click(object sender, EventArgs e)
    {
      Printer selectedPrinter = SelectedPrinter;
      if (selectedPrinter == null || !selectedPrinter.Connected || MessageBox.Show("Are you sure you want to abort this print?", "M3D Print Spooler", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
      {
        return;
      }

      var num = (int) selectedPrinter.AbortPrint(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject);
      listBoxManualHistory.Items.Add((object) ("(" + selectedPrinter.Info.MySerialNumber + ")->Abort Print"));
    }

    private void buttonUpZ_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxZVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the Z value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxZVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Z{1}", (object) 90f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpY_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxYVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the Y value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxYVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Y{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownX_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxXVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the X value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxXVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} X-{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpX_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxXVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the X value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxXVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} X{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownY_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxYVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the Y value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxYVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Y-{1}", (object) 3000f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownZ_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxZVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the Z value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxZVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} Z-{1}", (object) 90f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonDownE_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxEVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the E value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxEVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} E-{1}", (object) 345f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonUpE_Click(object sender, EventArgs e)
    {
      if (!PrinterCompatibleString.VerifyNumber(textBoxEVal.Text))
      {
        var num1 = (int) MessageBox.Show("Sorry, but the E value is not a number");
      }
      else
      {
        var floatCurrentCulture = PrinterCompatibleString.ToFloatCurrentCulture(textBoxEVal.Text);
        lock (_myCurrentPrinterObjectSync)
        {
          if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
          {
            return;
          }

          var num2 = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G91", PrinterCompatibleString.Format("G0 F{0} E{1}", (object) 345f, (object) floatCurrentCulture));
        }
      }
    }

    private void buttonABSTemp_Click(object sender, EventArgs e)
    {
      SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.ABS));
    }

    private void buttonTGHTemp_Click(object sender, EventArgs e)
    {
      SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.TGH));
    }

    private void buttonPLATemp_Click(object sender, EventArgs e)
    {
      SetTemperature(FilamentConstants.Temperature.Default(FilamentSpool.TypeEnum.PLA));
    }

    private void buttonHeaterOff_Click(object sender, EventArgs e)
    {
      SetTemperature(0);
    }

    private void SetTemperature(int temperature)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, PrinterCompatibleString.Format("M109 S{0}", (object) temperature));
      }
    }

    private void buttonBedHeatToPLA_Click(object sender, EventArgs e)
    {
      SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.PLA));
    }

    private void buttonBedHeatToABS_Click(object sender, EventArgs e)
    {
      SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.ABS));
    }

    private void buttonBedHeatToABR_Click(object sender, EventArgs e)
    {
      SetBedTemperature(FilamentConstants.Temperature.BedDefault(FilamentSpool.TypeEnum.ABS_R));
    }

    private void buttonTurnOfHeatedbed_Click(object sender, EventArgs e)
    {
      SetBedTemperature(0);
    }

    private void SetBedTemperature(int temperature)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, PrinterCompatibleString.Format("M190 S{0}", (object) temperature));
      }
    }

    private void buttonMotorsOn_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "M17");
      }
    }

    private void buttonMotorsOff_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "M18");
      }
    }

    private void buttonFanOn_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "M106 S255");
      }
    }

    private void buttonFanOff_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "M106 S0");
      }
    }

    private void button1_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G90");
      }
    }

    private void buttonRepeatLast_Click(object sender, EventArgs e)
    {
      checkBoxLogToScreen.Checked = true;
      textBoxManualGCode.Text = lastManualCommand;
      buttonSendGCode_Click(sender, e);
    }

    private void buttonChangeFilamentInfo_Click(object sender, EventArgs e)
    {
    }

    private void buttonLoadFilament_Click(object sender, EventArgs e)
    {
    }

    private void buttonEjectFilament_Click(object sender, EventArgs e)
    {
    }

    private void button2_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "G91");
      }
    }

    private void buttonXSkipTest_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateXSkipTestMinus(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSkipTest_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateYSkipTestMinus(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonXSkipTestRight_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateXSkipTestPlus(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSkipTestBack_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateYSkipTestPlus(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonXSpeedTest_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateXSpeedTest(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonYSpeedTest_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.CreateYSpeedTest(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void buttonHome_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "G28");
      }
    }

    private void buttonReCenter_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, TestGeneration.FastRecenter(MyCurrentPrinterObject.MyPrinterProfile).ToArray());
      }
    }

    private void OnPrinterLockCallback(IAsyncCallResult ar)
    {
      var flag = PrinterLocked.Value;
      if (ar.CallResult == CommandResult.Success_LockAcquired && !flag)
      {
        LockChanging.Value = MainForm.PrinterLockChanging.YesToLocked;
        SDCardExtensions sdPluginExtension = GetCurrentSDPluginExtension();
        if (sdPluginExtension == null || !sdPluginExtension.Available)
        {
          return;
        }

        sdPluginExtension.OnReceivedFileList = new EventHandler(ReceivedUpdatedSDCardList);
        if (!m_bSDTabSelected)
        {
          return;
        }

        var num = (int) sdPluginExtension.RefreshSDCardList((M3D.Spooling.Client.AsyncCallback) null, (object) null);
      }
      else if (ar.CallResult == CommandResult.Success_LockReleased & flag)
      {
        LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      }
      else if (ar.CallResult == CommandResult.Failed_PrinterDoesNotHaveLock)
      {
        if (!flag)
        {
          return;
        }

        LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
      }
      else if (ar.CallResult == CommandResult.Failed_PrinterAlreadyLocked)
      {
        var num = (int) MessageBox.Show("Can not lock this printer because it is being used by another program.");
        LockChanging.Value = MainForm.PrinterLockChanging.NoFailed;
      }
      else
      {
        if (ar.CallResult != CommandResult.Failed_CannotLockWhilePrinting)
        {
          return;
        }

        var num = (int) MessageBox.Show("Can not lock this printer because it is printing.");
        LockChanging.Value = MainForm.PrinterLockChanging.NoFailed;
      }
    }

    private void buttonGetGCode_Click(object sender, EventArgs e)
    {
      var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "M3D", "Spooler", "queue");
      var destFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "M3D - Last used G-code.gcode");
      try
      {
        var directoryInfo = new DirectoryInfo(path);
        var fileInfo = (FileInfo) null;
        var searchPattern = "*_processed.gcode";
        foreach (FileInfo file in directoryInfo.GetFiles(searchPattern))
        {
          if (fileInfo == null)
          {
            fileInfo = file;
          }
          else if (fileInfo.CreationTimeUtc < file.CreationTimeUtc)
          {
            fileInfo = file;
          }
        }
        if (fileInfo != null)
        {
          File.Copy(fileInfo.FullName, destFileName, true);
          var num = (int) MessageBox.Show("Successfully moved \"M3D - Last used G-code.gcode\" to the Desktop", "Info");
        }
        else
        {
          var num1 = (int) MessageBox.Show("Sorry, no gcode files could be found", "Info");
        }
      }
      catch (Exception ex)
      {
        var num = (int) MessageBox.Show("Sorry, there was a problem copying the gcode to the Desktop", "Info");
      }
    }

    private void buttonBedPointTest_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        PrinterInfo info = MyCurrentPrinterObject.Info;
        if (info == null)
        {
          return;
        }

        var compensationPreprocessor = new BedCompensationPreprocessor();
        compensationPreprocessor.UpdateConfigurations(info.calibration, MyCurrentPrinterObject.MyPrinterProfile.PrinterSizeConstants);
        var selectedItem = (string)comboBoxBedTestPoint.SelectedItem;
        Vector vector;
        if (!(selectedItem == "Center"))
        {
          if (!(selectedItem == "Front Right"))
          {
            if (!(selectedItem == "Front Left"))
            {
              if (!(selectedItem == "Back Right"))
              {
                if (!(selectedItem == "Back Left"))
                {
                  return;
                }

                vector = compensationPreprocessor.BackLeft;
              }
              else
              {
                vector = compensationPreprocessor.BackRight;
              }
            }
            else
            {
              vector = compensationPreprocessor.FrontLeft;
            }
          }
          else
          {
            vector = compensationPreprocessor.FrontRight;
          }
        }
        else
        {
          vector = compensationPreprocessor.Center;
        }

        vector.z = 0.1f + compensationPreprocessor.GetHeightAdjustmentRequired(vector.x, vector.y) + compensationPreprocessor.entire_z_height_offset;
        var num = (int)MyCurrentPrinterObject.SendManualGCode(new M3D.Spooling.Client.AsyncCallback(ProcessCallErrors), (object)MyCurrentPrinterObject, "M1012", "G91", PrinterCompatibleString.Format("G0 Z{0} F100", (object) 2), "G90", PrinterCompatibleString.Format("G0 X{0} Y{1} Z{2} F3000", (object) vector.x, (object) vector.y, (object) 5f), PrinterCompatibleString.Format("G0 Z{0} F100", (object) vector.z), "M1011");
      }
    }

    private void buttonLock_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        buttonLock.Enabled = false;
        if (!PrinterLocked.Value)
        {
          MyCurrentPrinterObject.LockStepMode = false;
          var num1 = (int)MyCurrentPrinterObject.AcquireLock(new M3D.Spooling.Client.AsyncCallback(OnPrinterLockCallback), (object)MyCurrentPrinterObject, new EventLockTimeOutCallBack(OnLockTimedOut), 120);
          if (settings.DoNotShowPrinterLockOutWarning)
          {
            return;
          }

          var printerLockWarning = new PrinterLockWarning();
          var num2 = (int) printerLockWarning.ShowDialog((IWin32Window) this);
          settings.DoNotShowPrinterLockOutWarning = printerLockWarning.DoNotShowAgain;
          if (!printerLockWarning.DoNotShowAgain)
          {
            return;
          }

          SpoolerSettings.SaveSettings(settings, shared_shutdown.Value);
        }
        else
        {
          MyCurrentPrinterObject.LockStepMode = true;
          var num = (int)MyCurrentPrinterObject.ReleaseLock(new M3D.Spooling.Client.AsyncCallback(OnPrinterLockCallback), (object)MyCurrentPrinterObject);
        }
      }
    }

    private void OnLockTimedOut(IPrinter printer)
    {
      OnLog(" >> The lock on the printer has timed out", printer.Info.MySerialNumber);
      if (!PrinterLocked.Value)
      {
        return;
      }

      LockChanging.Value = MainForm.PrinterLockChanging.YesToUnlocked;
    }

    private void buttonSaveEepromData_Click(object sender, EventArgs e)
    {
      lock (_myCurrentPrinterObjectSync)
      {
        if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
        {
          return;
        }

        PrinterInfo info = MyCurrentPrinterObject.Info;
        if (info == null)
        {
          return;
        }

        PublicPrinterConnection printerConnection = spooler_client.UnsafeFindPrinterConnection(info.serial_number);
        if (printerConnection == null)
        {
          var num1 = (int) MessageBox.Show((IWin32Window) this, "Unable to modify PrinterConnection", "M3D Print Spooler");
        }
        else
        {
          var saveFileDialog = new SaveFileDialog
          {
            Filter = "EEPROM XML Files (.EEPROM.xml)|*.EEPROM.xml|All Files (*.*)|*.*",
            FilterIndex = 1,
            FileName = info.serial_number.ToString() + ".EEPROM.xml"
          };
          if (saveFileDialog.ShowDialog() != DialogResult.OK)
          {
            return;
          }

          switch (printerConnection.SaveEEPROMDataToXMLFile(saveFileDialog.FileName))
          {
            case CommandResult.Success:
              var num2 = (int) MessageBox.Show("EEPROM Data Saved to XML file.", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
              break;
            case CommandResult.Failed_NotInFirmware:
              var num3 = (int) MessageBox.Show("Printer must be in firmware mode", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Hand);
              break;
            case CommandResult.Failed_Argument:
              var num4 = (int) MessageBox.Show("Unable to save xml file.", "M3D Print Spooler", MessageBoxButtons.OK, MessageBoxIcon.Hand);
              break;
          }
        }
      }
    }

    private void buttonLoadEepromData_Click(object sender, EventArgs e)
    {
    }

    private void buttonSDRefresh_Click(object sender, EventArgs e)
    {
      SDCardExtensions sdPluginExtension = GetCurrentSDPluginExtension();
      if (sdPluginExtension == null || !sdPluginExtension.Available)
      {
        return;
      }

      sdPluginExtension.OnReceivedFileList = new EventHandler(ReceivedUpdatedSDCardList);
      var num = (int) sdPluginExtension.RefreshSDCardList((M3D.Spooling.Client.AsyncCallback) null, (object) null);
    }

    private void buttonSDSaveGcode_Click(object sender, EventArgs e)
    {
      if (MyCurrentPrinterObject == null || !MyCurrentPrinterObject.Connected)
      {
        return;
      }

      AddJobFromGCodeFile(true, JobParams.Mode.SavingToSDCard);
    }

    private void buttonSDPrint_Click(object sender, EventArgs e)
    {
      if (listBoxSDFiles.SelectedIndex < 0)
      {
        var num1 = (int) MessageBox.Show("Please select a file on your 3D printer to print.");
      }
      else
      {
        try
        {
          var gcodefile = listBoxSDFiles.Items[listBoxSDFiles.SelectedIndex].ToString();
          if (MyCurrentPrinterObject != null && MyCurrentPrinterObject.Connected)
          {
            Printer currentPrinterObject = MyCurrentPrinterObject;
            var jobParams = new JobParams(gcodefile, "Spooler Inserted Job", "null", FilamentSpool.TypeEnum.OtherOrUnknown, 0.0f, 0.0f)
            {
              jobMode = JobParams.Mode.FirmwarePrintingFromSDCard
            };
            jobParams.options.autostart_ignorewarnings = true;
            jobParams.options.dont_use_preprocessors = true;
            if (checkBoxCalibrateBeforeSDPrint.Checked)
            {
              jobParams.options.calibrate_before_print = true;
              jobParams.options.calibrate_before_print_z = 0.4f;
              if (MyCurrentPrinterObject.Info.calibration.UsesCalibrationOffset)
              {
                jobParams.options.calibrate_before_print_z += MyCurrentPrinterObject.Info.calibration.CALIBRATION_OFFSET;
              }
            }
            var filamentSpool = new FilamentSpool()
            {
              filament_type = FilamentSpool.TypeEnum.OtherOrUnknown,
              filament_temperature = 0
            };
            jobParams.preprocessor = (FilamentPreprocessorData) null;
            jobParams.filament_temperature = 0;
            var num2 = (int)MyCurrentPrinterObject.PrintModel((M3D.Spooling.Client.AsyncCallback) null, (object) null, jobParams);
          }
          else
          {
            var num3 = (int) MessageBox.Show("Sorry but the printer has disconnected.");
          }
        }
        catch (Exception ex)
        {
          var num2 = (int) MessageBox.Show("There was an error selecting the file. Please try again.");
        }
      }
    }

    private void buttonSDDelete_Click(object sender, EventArgs e)
    {
      if (listBoxSDFiles.SelectedIndex < 0)
      {
        var num1 = (int) MessageBox.Show("Please select a file on your 3D printer to print.");
      }
      else
      {
        try
        {
          var filename = listBoxSDFiles.Items[listBoxSDFiles.SelectedIndex].ToString();
          SDCardExtensions sdPluginExtension = GetCurrentSDPluginExtension();
          if (sdPluginExtension == null || !sdPluginExtension.Available)
          {
            return;
          }

          var num2 = (int) sdPluginExtension.DeleteFileFromSDCard(new M3D.Spooling.Client.AsyncCallback(RefreshAfterCommand), (object) sdPluginExtension, filename);
        }
        catch (Exception ex)
        {
        }
      }
    }

    private void RefreshAfterCommand(IAsyncCallResult ar)
    {
      if (ar.CallResult != CommandResult.Success)
      {
        return;
      }

      buttonSDRefresh_Click((object) null, (EventArgs) null);
    }

    private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var num = (int) new About(spooler_client.ConnectedSpoolerInfo).ShowDialog((IWin32Window) this);
    }

    private void AdvancedOptionsAtStartup(object sender, EventArgs e)
    {
      var flag = !showAdvancedOptionsAtStartupToolStripMenuItem.Checked;
      showAdvancedOptionsAtStartupToolStripMenuItem.Checked = flag;
      settings.StartAdvanced = flag;
      SpoolerSettings.SaveSettings(settings, shared_shutdown.Value);
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (tabPageSDCard == tabControl1.SelectedTab)
      {
        m_bSDTabSelected = true;
        Printer currentPrinterObject = MyCurrentPrinterObject;
        if (currentPrinterObject == null)
        {
          return;
        } (currentPrinterObject.GetPrinterPlugin(SDCardExtensions.ID) as SDCardExtensions).OnReceivedFileList = new EventHandler(ReceivedUpdatedSDCardList);
        var num = (int) currentPrinterObject.SendManualGCode((M3D.Spooling.Client.AsyncCallback) null, (object) null, "M20");
      }
      else
      {
        m_bSDTabSelected = false;
      }
    }

    private void ResetSDFileList()
    {
      listBoxSDFiles.Items.Clear();
    }

    private void ReceivedUpdatedSDCardList(object sender, EventArgs args)
    {
      SDCardExtensions sdplugin = GetCurrentSDPluginExtension();
      if (sdplugin == null || sdplugin != sender as SDCardExtensions)
      {
        return;
      }

      BeginInvoke((Action) (() =>
      {
        listBoxSDFiles.Items.Clear();
        foreach (var sdCardFile in sdplugin.GetSDCardFileList())
        {
          listBoxSDFiles.Items.Add(sdCardFile);
        }
      }));
    }

    private SDCardExtensions GetCurrentSDPluginExtension()
    {
      Printer currentPrinterObject = MyCurrentPrinterObject;
      if (currentPrinterObject != null)
      {
        return currentPrinterObject.GetPrinterPlugin(SDCardExtensions.ID) as SDCardExtensions;
      }

      return (SDCardExtensions) null;
    }

    private void showAdvancedStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var flag = !showAdvancedStatisticsToolStripMenuItem.Checked;
      showAdvancedStatisticsToolStripMenuItem.Checked = flag;
      if (flag)
      {
        advancedStatisticsDialog.Show((IWin32Window) this);
      }
      else
      {
        advancedStatisticsDialog.Hide();
      }
    }

    protected override void Dispose(bool bIsDisposing)
    {
      if (bIsDisposing && components != null)
      {
        components.Dispose();
      }

      base.Dispose(bIsDisposing);
    }

    private void InitializeComponent()
    {
      components = (IContainer) new Container();
      var componentResourceManager = new ComponentResourceManager(typeof (MainForm));
      timer1 = new Timer(components);
      menuStrip1 = new MenuStrip();
      viewToolStripMenuItem = new ToolStripMenuItem();
      showAdvancedToolStripMenuItem = new ToolStripMenuItem();
      showAdvancedStatisticsToolStripMenuItem = new ToolStripMenuItem();
      optionsToolStripMenuItem = new ToolStripMenuItem();
      showAdvancedOptionsAtStartupToolStripMenuItem = new ToolStripMenuItem();
      helpToolStripMenuItem = new ToolStripMenuItem();
      aboutToolStripMenuItem = new ToolStripMenuItem();
      groupBoxPrinterList = new GroupBox();
      buttonStandAlone = new Button();
      checkBoxAutoCheckFirmware = new CheckBox();
      listViewPrinterInfo = new ListView();
      columnSerialNumber = new ColumnHeader();
      columnType = new ColumnHeader();
      columnStatus = new ColumnHeader();
      columnTemp = new ColumnHeader();
      columnBedTemp = new ColumnHeader();
      columnZValid = new ColumnHeader();
      columnJobStatus = new ColumnHeader();
      columnFile = new ColumnHeader();
      columnPerComplete = new ColumnHeader();
      columnUser = new ColumnHeader();
      labelSpoolerVersion = new Label();
      groupBoxPrinterControls = new GroupBox();
      logWaitsCheckBox = new CheckBox();
      buttonAbortPrint = new Button();
      buttonResumePrint = new Button();
      buttonPausePrint = new Button();
      label1 = new Label();
      selectedPrinterComboBox = new ComboBox();
      buttonLock = new Button();
      groupBoxPrinting = new GroupBox();
      groupBox2 = new GroupBox();
      buttonSetTemp = new Button();
      textBoxTempEdit = new TextBox();
      label8 = new Label();
      label7 = new Label();
      groupBox1 = new GroupBox();
      textBoxMeanTemperature = new Label();
      groupBoxControls = new GroupBox();
      groupBoxBootloaderOptions = new GroupBox();
      label2 = new Label();
      buttonQuitBootloader = new Button();
      button3 = new Button();
      groupBoxFirmwareControls = new GroupBox();
      tabControl1 = new TabControl();
      tabPageBasicOptions = new TabPage();
      buttonTGHTemp = new Button();
      buttonHeaterOff = new Button();
      buttonFanOff = new Button();
      buttonFanOn = new Button();
      textBoxEVal = new TextBox();
      buttonPLATemp = new Button();
      buttonDownE = new Button();
      buttonUpE = new Button();
      buttonABSTemp = new Button();
      buttonAddJob = new Button();
      buttonMotorsOff = new Button();
      buttonMotorsOn = new Button();
      textBoxYVal = new TextBox();
      textBoxXVal = new TextBox();
      buttonPrintToFile = new Button();
      textBoxZVal = new TextBox();
      button2 = new Button();
      buttonUpX = new Button();
      button1 = new Button();
      buttonDownX = new Button();
      buttonFilamentInfo = new Button();
      buttonDownZ = new Button();
      buttonDownY = new Button();
      buttonUpZ = new Button();
      buttonUpY = new Button();
      tabPageDiagnostics = new TabPage();
      buttonLoadEepromData = new Button();
      buttonSaveEepromData = new Button();
      buttonUpdateFirmware = new Button();
      buttonPreprocess = new Button();
      groupBox4 = new GroupBox();
      buttonBedPointTest = new Button();
      comboBoxBedTestPoint = new ComboBox();
      buttonBacklashPrint = new Button();
      buttonGetGCode = new Button();
      buttonReCenter = new Button();
      buttonHome = new Button();
      buttonSpeedTest = new Button();
      buttonXSpeedTest = new Button();
      buttonYSkipTestBack = new Button();
      buttonXSkipTestRight = new Button();
      buttonYSkipTest = new Button();
      buttonXSkipTest = new Button();
      tabPageSDCard = new TabPage();
      checkBoxCalibrateBeforeSDPrint = new CheckBox();
      buttonSDDeleteFile = new Button();
      buttonSDPrint = new Button();
      buttonSDSaveGcode = new Button();
      buttonSDRefresh = new Button();
      listBoxSDFiles = new ListBox();
      label6 = new Label();
      tabPageHeatedBedControl = new TabPage();
      buttonTurnOfHeatedbed = new Button();
      buttonBedHeatToABR = new Button();
      buttonBedHeatToABS = new Button();
      buttonBedHeatToPLA = new Button();
      buttonRepeatLast = new Button();
      label3 = new Label();
      textBoxManualGCode = new TextBox();
      buttonSendGCode = new Button();
      buttonClearLog = new Button();
      checkBoxLogToScreen = new CheckBox();
      richTextBoxLoggedItems = new RichTextBox();
      buttonEmergencyStop = new Button();
      label5 = new Label();
      listBoxManualHistory = new ListBox();
      label4 = new Label();
      checkBoxAutoScroll = new CheckBox();
      groupBox3 = new GroupBox();
      menuStrip1.SuspendLayout();
      groupBoxPrinterList.SuspendLayout();
      groupBoxPrinterControls.SuspendLayout();
      groupBoxPrinting.SuspendLayout();
      groupBox2.SuspendLayout();
      groupBox1.SuspendLayout();
      groupBoxControls.SuspendLayout();
      groupBoxBootloaderOptions.SuspendLayout();
      groupBoxFirmwareControls.SuspendLayout();
      tabControl1.SuspendLayout();
      tabPageBasicOptions.SuspendLayout();
      tabPageDiagnostics.SuspendLayout();
      groupBox4.SuspendLayout();
      tabPageSDCard.SuspendLayout();
      tabPageHeatedBedControl.SuspendLayout();
      groupBox3.SuspendLayout();
      SuspendLayout();
      timer1.Interval = 300;
      timer1.Tick += new EventHandler(DoUpdates);
      menuStrip1.Items.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) viewToolStripMenuItem,
        (ToolStripItem) optionsToolStripMenuItem,
        (ToolStripItem) helpToolStripMenuItem
      });
      menuStrip1.Location = new Point(0, 0);
      menuStrip1.Name = "menuStrip1";
      menuStrip1.Size = new Size(1040, 24);
      menuStrip1.TabIndex = 49;
      menuStrip1.Text = "menuStrip1";
      viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) showAdvancedToolStripMenuItem,
        (ToolStripItem) showAdvancedStatisticsToolStripMenuItem
      });
      viewToolStripMenuItem.Name = "viewToolStripMenuItem";
      viewToolStripMenuItem.Size = new Size(45, 20);
      viewToolStripMenuItem.Text = "View";
      showAdvancedToolStripMenuItem.Name = "showAdvancedToolStripMenuItem";
      showAdvancedToolStripMenuItem.Size = new Size(212, 22);
      showAdvancedToolStripMenuItem.Text = "Show Advanced Options";
      showAdvancedToolStripMenuItem.Click += new EventHandler(showAdvancedStripMenuItem_Click);
      showAdvancedStatisticsToolStripMenuItem.Name = "showAdvancedStatisticsToolStripMenuItem";
      showAdvancedStatisticsToolStripMenuItem.Size = new Size(212, 22);
      showAdvancedStatisticsToolStripMenuItem.Text = "Show Advanced Statistics";
      showAdvancedStatisticsToolStripMenuItem.Click += new EventHandler(showAdvancedStatisticsToolStripMenuItem_Click);
      optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) showAdvancedOptionsAtStartupToolStripMenuItem
      });
      optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
      optionsToolStripMenuItem.Size = new Size(61, 20);
      optionsToolStripMenuItem.Text = "Options";
      showAdvancedOptionsAtStartupToolStripMenuItem.Name = "showAdvancedOptionsAtStartupToolStripMenuItem";
      showAdvancedOptionsAtStartupToolStripMenuItem.Size = new Size(264, 22);
      showAdvancedOptionsAtStartupToolStripMenuItem.Text = "Show Advanced Options at Startup";
      showAdvancedOptionsAtStartupToolStripMenuItem.Click += new EventHandler(AdvancedOptionsAtStartup);
      helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[1]
      {
        (ToolStripItem) aboutToolStripMenuItem
      });
      helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      helpToolStripMenuItem.Size = new Size(44, 20);
      helpToolStripMenuItem.Text = "Help";
      aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
      aboutToolStripMenuItem.Size = new Size(107, 22);
      aboutToolStripMenuItem.Text = "About";
      aboutToolStripMenuItem.Click += new EventHandler(aboutToolStripMenuItem_Click);
      groupBoxPrinterList.Controls.Add((Control)buttonStandAlone);
      groupBoxPrinterList.Controls.Add((Control)checkBoxAutoCheckFirmware);
      groupBoxPrinterList.Controls.Add((Control)listViewPrinterInfo);
      groupBoxPrinterList.Controls.Add((Control)labelSpoolerVersion);
      groupBoxPrinterList.Location = new Point(1, 420);
      groupBoxPrinterList.Name = "groupBoxPrinterList";
      groupBoxPrinterList.Size = new Size(1039, 124);
      groupBoxPrinterList.TabIndex = 50;
      groupBoxPrinterList.TabStop = false;
      buttonStandAlone.Location = new Point(208, 98);
      buttonStandAlone.Name = "buttonStandAlone";
      buttonStandAlone.Size = new Size(153, 18);
      buttonStandAlone.TabIndex = 51;
      buttonStandAlone.Text = "Make Stand-alone";
      buttonStandAlone.UseVisualStyleBackColor = true;
      buttonStandAlone.Click += new EventHandler(buttonStandAlone_Click);
      checkBoxAutoCheckFirmware.AutoSize = true;
      checkBoxAutoCheckFirmware.Checked = true;
      checkBoxAutoCheckFirmware.CheckState = CheckState.Checked;
      checkBoxAutoCheckFirmware.Location = new Point(15, 101);
      checkBoxAutoCheckFirmware.Name = "checkBoxAutoCheckFirmware";
      checkBoxAutoCheckFirmware.Size = new Size(146, 16);
      checkBoxAutoCheckFirmware.TabIndex = 50;
      checkBoxAutoCheckFirmware.Text = "Auto-check Firmware";
      checkBoxAutoCheckFirmware.UseVisualStyleBackColor = true;
      checkBoxAutoCheckFirmware.CheckedChanged += new EventHandler(OnAutoCheckFirmwareChanged);
      listViewPrinterInfo.Activation = ItemActivation.OneClick;
      listViewPrinterInfo.Columns.AddRange(new ColumnHeader[10]
      {
        columnSerialNumber,
        columnType,
        columnStatus,
        columnTemp,
        columnBedTemp,
        columnZValid,
        columnJobStatus,
        columnFile,
        columnPerComplete,
        columnUser
      });
      listViewPrinterInfo.FullRowSelect = true;
      listViewPrinterInfo.HideSelection = false;
      listViewPrinterInfo.LabelWrap = false;
      listViewPrinterInfo.Location = new Point(9, 12);
      listViewPrinterInfo.MultiSelect = false;
      listViewPrinterInfo.Name = "listViewPrinterInfo";
      listViewPrinterInfo.Size = new Size(1018, 80);
      listViewPrinterInfo.TabIndex = 49;
      listViewPrinterInfo.UseCompatibleStateImageBehavior = false;
      listViewPrinterInfo.View = View.Details;
      listViewPrinterInfo.DoubleClick += new EventHandler(listViewPrinterInfo_DoubleClick);
      columnSerialNumber.Text = "Serial Number";
      columnSerialNumber.Width = 149;
      columnType.Text = "Type";
      columnStatus.Text = "Status";
      columnStatus.Width = 213;
      columnTemp.Text = "Temp. (C)";
      columnTemp.Width = 72;
      columnBedTemp.Text = "Bed Temp. (C)";
      columnBedTemp.Width = 99;
      columnZValid.Text = "Z Valid";
      columnZValid.Width = 53;
      columnJobStatus.Text = "Job Status";
      columnJobStatus.Width = 81;
      columnFile.Text = "File";
      columnFile.Width = 109;
      columnPerComplete.Text = "% Done";
      columnPerComplete.Width = 58;
      columnUser.Text = "User";
      columnUser.Width = 117;
      labelSpoolerVersion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      labelSpoolerVersion.AutoSize = true;
      labelSpoolerVersion.Location = new Point(731, 96);
      labelSpoolerVersion.Name = "labelSpoolerVersion";
      labelSpoolerVersion.Size = new Size(181, 12);
      labelSpoolerVersion.TabIndex = 53;
      labelSpoolerVersion.Text = "Spooler Version: ????-??-??-??";
      groupBoxPrinterControls.Controls.Add((Control)logWaitsCheckBox);
      groupBoxPrinterControls.Controls.Add((Control)buttonAbortPrint);
      groupBoxPrinterControls.Controls.Add((Control)buttonResumePrint);
      groupBoxPrinterControls.Controls.Add((Control)buttonPausePrint);
      groupBoxPrinterControls.Controls.Add((Control)label1);
      groupBoxPrinterControls.Controls.Add((Control)selectedPrinterComboBox);
      groupBoxPrinterControls.Controls.Add((Control)buttonLock);
      groupBoxPrinterControls.Controls.Add((Control)groupBoxPrinting);
      groupBoxPrinterControls.Controls.Add((Control)groupBoxControls);
      groupBoxPrinterControls.Controls.Add((Control)buttonClearLog);
      groupBoxPrinterControls.Controls.Add((Control)checkBoxLogToScreen);
      groupBoxPrinterControls.Controls.Add((Control)richTextBoxLoggedItems);
      groupBoxPrinterControls.Controls.Add((Control)buttonEmergencyStop);
      groupBoxPrinterControls.Controls.Add((Control)label5);
      groupBoxPrinterControls.Controls.Add((Control)listBoxManualHistory);
      groupBoxPrinterControls.Controls.Add((Control)label4);
      groupBoxPrinterControls.Controls.Add((Control)checkBoxAutoScroll);
      groupBoxPrinterControls.Location = new Point(10, 25);
      groupBoxPrinterControls.Name = "groupBoxPrinterControls";
      groupBoxPrinterControls.Size = new Size(1018, 393);
      groupBoxPrinterControls.TabIndex = 51;
      groupBoxPrinterControls.TabStop = false;
      groupBoxPrinterControls.Text = "00-00-00-00-00-000-000";
      logWaitsCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      logWaitsCheckBox.AutoSize = true;
      logWaitsCheckBox.Location = new Point(129, 278);
      logWaitsCheckBox.Name = "logWaitsCheckBox";
      logWaitsCheckBox.Size = new Size(79, 16);
      logWaitsCheckBox.TabIndex = 51;
      logWaitsCheckBox.Text = "Log Waits";
      logWaitsCheckBox.UseVisualStyleBackColor = true;
      buttonAbortPrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonAbortPrint.BackColor = SystemColors.Control;
      buttonAbortPrint.ForeColor = System.Drawing.Color.Black;
      buttonAbortPrint.Location = new Point(6, 85);
      buttonAbortPrint.Name = "buttonAbortPrint";
      buttonAbortPrint.Size = new Size(184, 30);
      buttonAbortPrint.TabIndex = 50;
      buttonAbortPrint.Text = "Abort Print";
      buttonAbortPrint.UseVisualStyleBackColor = false;
      buttonAbortPrint.Click += new EventHandler(buttonAbortPrint_Click);
      buttonResumePrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonResumePrint.BackColor = SystemColors.Control;
      buttonResumePrint.ForeColor = SystemColors.ControlText;
      buttonResumePrint.Location = new Point(199, 85);
      buttonResumePrint.Name = "buttonResumePrint";
      buttonResumePrint.Size = new Size(184, 30);
      buttonResumePrint.TabIndex = 49;
      buttonResumePrint.Text = "Resume";
      buttonResumePrint.UseVisualStyleBackColor = false;
      buttonResumePrint.Visible = false;
      buttonResumePrint.Click += new EventHandler(buttonResumePrint_Click);
      buttonPausePrint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonPausePrint.BackColor = SystemColors.Control;
      buttonPausePrint.ForeColor = SystemColors.ControlText;
      buttonPausePrint.Location = new Point(199, 85);
      buttonPausePrint.Name = "buttonPausePrint";
      buttonPausePrint.Size = new Size(184, 30);
      buttonPausePrint.TabIndex = 48;
      buttonPausePrint.Text = "Pause";
      buttonPausePrint.UseVisualStyleBackColor = false;
      buttonPausePrint.Click += new EventHandler(buttonPausePrint_Click);
      label1.AutoSize = true;
      label1.Location = new Point(10, 21);
      label1.Name = "label1";
      label1.Size = new Size(90, 12);
      label1.TabIndex = 47;
      label1.Text = "Current Printer:";
      selectedPrinterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
      selectedPrinterComboBox.FormattingEnabled = true;
      selectedPrinterComboBox.Location = new Point(103, 18);
      selectedPrinterComboBox.Name = "selectedPrinterComboBox";
      selectedPrinterComboBox.Size = new Size(280, 20);
      selectedPrinterComboBox.TabIndex = 46;
      selectedPrinterComboBox.SelectedIndexChanged += new EventHandler(OnSelectedPrintComboBoxChanged);
      buttonLock.Location = new Point(8, 49);
      buttonLock.Name = "buttonLock";
      buttonLock.Size = new Size(184, 30);
      buttonLock.TabIndex = 45;
      buttonLock.Text = "Control This Printer";
      buttonLock.UseVisualStyleBackColor = true;
      buttonLock.Click += new EventHandler(buttonLock_Click);
      groupBoxPrinting.Controls.Add((Control)groupBox3);
      groupBoxPrinting.Location = new Point(389, 11);
      groupBoxPrinting.Name = "groupBoxPrinting";
      groupBoxPrinting.Size = new Size(619, 382);
      groupBoxPrinting.TabIndex = 52;
      groupBoxPrinting.TabStop = false;
      groupBoxPrinting.Text = "Now Printing";
      groupBoxPrinting.Visible = false;
      groupBox2.Controls.Add((Control)buttonSetTemp);
      groupBox2.Controls.Add((Control)textBoxTempEdit);
      groupBox2.Controls.Add((Control)label8);
      groupBox2.Location = new Point(4, 90);
      groupBox2.Name = "groupBox2";
      groupBox2.Size = new Size(435, 109);
      groupBox2.TabIndex = 3;
      groupBox2.TabStop = false;
      groupBox2.Text = "Tempeature Adjustments";
      buttonSetTemp.Location = new Point(158, 71);
      buttonSetTemp.Name = "buttonSetTemp";
      buttonSetTemp.Size = new Size(124, 24);
      buttonSetTemp.TabIndex = 4;
      buttonSetTemp.Text = "Set Temperature";
      buttonSetTemp.UseVisualStyleBackColor = true;
      buttonSetTemp.Click += new EventHandler(buttonSetTemp_Click);
      textBoxTempEdit.Location = new Point(14, 72);
      textBoxTempEdit.Name = "textBoxTempEdit";
      textBoxTempEdit.Size = new Size(131, 21);
      textBoxTempEdit.TabIndex = 3;
      label8.Location = new Point(7, 23);
      label8.Name = "label8";
      label8.Size = new Size(401, 40);
      label8.TabIndex = 2;
      label8.Text = "The current temperature can be set while printing to improve print results. Only temperature values within 15 degees of the current average temperature are allowed.";
      label7.AutoSize = true;
      label7.Location = new Point(95, 40);
      label7.Name = "label7";
      label7.Size = new Size(178, 12);
      label7.TabIndex = 1;
      label7.Text = "Average Extruder Temperature";
      groupBox1.Controls.Add((Control)textBoxMeanTemperature);
      groupBox1.Location = new Point(3, 20);
      groupBox1.Name = "groupBox1";
      groupBox1.Size = new Size(83, 49);
      groupBox1.TabIndex = 0;
      groupBox1.TabStop = false;
      textBoxMeanTemperature.AutoSize = true;
      textBoxMeanTemperature.Font = new Font("Gulim", 14.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 129);
      textBoxMeanTemperature.Location = new Point(8, 18);
      textBoxMeanTemperature.Name = "textBoxMeanTemperature";
      textBoxMeanTemperature.Size = new Size(65, 19);
      textBoxMeanTemperature.TabIndex = 0;
      textBoxMeanTemperature.Text = "000.00";
      groupBoxControls.BackgroundImageLayout = ImageLayout.Center;
      groupBoxControls.Controls.Add((Control)groupBoxBootloaderOptions);
      groupBoxControls.Controls.Add((Control)groupBoxFirmwareControls);
      groupBoxControls.Location = new Point(389, 8);
      groupBoxControls.Name = "groupBoxControls";
      groupBoxControls.Size = new Size(624, 396);
      groupBoxControls.TabIndex = 43;
      groupBoxControls.TabStop = false;
      groupBoxBootloaderOptions.Controls.Add((Control)label2);
      groupBoxBootloaderOptions.Controls.Add((Control)buttonQuitBootloader);
      groupBoxBootloaderOptions.Controls.Add((Control)button3);
      groupBoxBootloaderOptions.Location = new Point(8, 11);
      groupBoxBootloaderOptions.Name = "groupBoxBootloaderOptions";
      groupBoxBootloaderOptions.Size = new Size(499, 162);
      groupBoxBootloaderOptions.TabIndex = 51;
      groupBoxBootloaderOptions.TabStop = false;
      groupBoxBootloaderOptions.Text = "Printer Boot Options";
      groupBoxBootloaderOptions.Visible = false;
      label2.AutoSize = true;
      label2.Location = new Point(19, 74);
      label2.Name = "label2";
      label2.Size = new Size(241, 12);
      label2.TabIndex = 2;
      label2.Text = "The main printer program has not started.";
      buttonQuitBootloader.Location = new Point(11, 20);
      buttonQuitBootloader.Name = "buttonQuitBootloader";
      buttonQuitBootloader.Size = new Size(200, 36);
      buttonQuitBootloader.TabIndex = 1;
      buttonQuitBootloader.Text = "Start Printer Firmware";
      buttonQuitBootloader.UseVisualStyleBackColor = true;
      buttonQuitBootloader.UseWaitCursor = true;
      buttonQuitBootloader.Click += new EventHandler(buttonQuitBootloader_Click);
      button3.Location = new Point(229, 20);
      button3.Name = "button3";
      button3.Size = new Size(200, 36);
      button3.TabIndex = 0;
      button3.Text = "Update Firmware";
      button3.UseVisualStyleBackColor = true;
      button3.Click += new EventHandler(buttonUpdateFirmware_Click);
      groupBoxFirmwareControls.Controls.Add((Control)tabControl1);
      groupBoxFirmwareControls.Controls.Add((Control)buttonRepeatLast);
      groupBoxFirmwareControls.Controls.Add((Control)label3);
      groupBoxFirmwareControls.Controls.Add((Control)textBoxManualGCode);
      groupBoxFirmwareControls.Controls.Add((Control)buttonSendGCode);
      groupBoxFirmwareControls.Location = new Point(8, 11);
      groupBoxFirmwareControls.Name = "groupBoxFirmwareControls";
      groupBoxFirmwareControls.Size = new Size(617, 387);
      groupBoxFirmwareControls.TabIndex = 0;
      groupBoxFirmwareControls.TabStop = false;
      tabControl1.Controls.Add((Control)tabPageBasicOptions);
      tabControl1.Controls.Add((Control)tabPageDiagnostics);
      tabControl1.Controls.Add((Control)tabPageSDCard);
      tabControl1.Controls.Add((Control)tabPageHeatedBedControl);
      tabControl1.Location = new Point(4, 49);
      tabControl1.Name = "tabControl1";
      tabControl1.SelectedIndex = 0;
      tabControl1.Size = new Size(609, 329);
      tabControl1.TabIndex = 50;
      tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);
      tabPageBasicOptions.BackColor = SystemColors.Control;
      tabPageBasicOptions.Controls.Add((Control)buttonTGHTemp);
      tabPageBasicOptions.Controls.Add((Control)buttonHeaterOff);
      tabPageBasicOptions.Controls.Add((Control)buttonFanOff);
      tabPageBasicOptions.Controls.Add((Control)buttonFanOn);
      tabPageBasicOptions.Controls.Add((Control)textBoxEVal);
      tabPageBasicOptions.Controls.Add((Control)buttonPLATemp);
      tabPageBasicOptions.Controls.Add((Control)buttonDownE);
      tabPageBasicOptions.Controls.Add((Control)buttonUpE);
      tabPageBasicOptions.Controls.Add((Control)buttonABSTemp);
      tabPageBasicOptions.Controls.Add((Control)buttonAddJob);
      tabPageBasicOptions.Controls.Add((Control)buttonMotorsOff);
      tabPageBasicOptions.Controls.Add((Control)buttonMotorsOn);
      tabPageBasicOptions.Controls.Add((Control)textBoxYVal);
      tabPageBasicOptions.Controls.Add((Control)textBoxXVal);
      tabPageBasicOptions.Controls.Add((Control)buttonPrintToFile);
      tabPageBasicOptions.Controls.Add((Control)textBoxZVal);
      tabPageBasicOptions.Controls.Add((Control)button2);
      tabPageBasicOptions.Controls.Add((Control)buttonUpX);
      tabPageBasicOptions.Controls.Add((Control)button1);
      tabPageBasicOptions.Controls.Add((Control)buttonDownX);
      tabPageBasicOptions.Controls.Add((Control)buttonFilamentInfo);
      tabPageBasicOptions.Controls.Add((Control)buttonDownZ);
      tabPageBasicOptions.Controls.Add((Control)buttonDownY);
      tabPageBasicOptions.Controls.Add((Control)buttonUpZ);
      tabPageBasicOptions.Controls.Add((Control)buttonUpY);
      tabPageBasicOptions.Location = new Point(4, 22);
      tabPageBasicOptions.Name = "tabPageBasicOptions";
      tabPageBasicOptions.Padding = new Padding(3);
      tabPageBasicOptions.Size = new Size(601, 303);
      tabPageBasicOptions.TabIndex = 0;
      tabPageBasicOptions.Text = "Basic Options";
      buttonTGHTemp.Location = new Point(13, 177);
      buttonTGHTemp.Name = "buttonTGHTemp";
      buttonTGHTemp.Size = new Size(129, 32);
      buttonTGHTemp.TabIndex = 59;
      buttonTGHTemp.Text = "Heat to TGH Temp";
      buttonTGHTemp.UseVisualStyleBackColor = true;
      buttonTGHTemp.Click += new EventHandler(buttonTGHTemp_Click);
      buttonHeaterOff.Location = new Point(149, 178);
      buttonHeaterOff.Name = "buttonHeaterOff";
      buttonHeaterOff.Size = new Size(129, 32);
      buttonHeaterOff.TabIndex = 58;
      buttonHeaterOff.Text = "Heater Off";
      buttonHeaterOff.UseVisualStyleBackColor = true;
      buttonHeaterOff.Click += new EventHandler(buttonHeaterOff_Click);
      buttonFanOff.Location = new Point(149, 109);
      buttonFanOff.Name = "buttonFanOff";
      buttonFanOff.Size = new Size(129, 32);
      buttonFanOff.TabIndex = 57;
      buttonFanOff.Text = "Fan Off";
      buttonFanOff.UseVisualStyleBackColor = true;
      buttonFanOff.Click += new EventHandler(buttonFanOff_Click);
      buttonFanOn.Location = new Point(13, 109);
      buttonFanOn.Name = "buttonFanOn";
      buttonFanOn.Size = new Size(129, 32);
      buttonFanOn.TabIndex = 56;
      buttonFanOn.Text = "Fan On";
      buttonFanOn.UseVisualStyleBackColor = true;
      buttonFanOn.Click += new EventHandler(buttonFanOn_Click);
      textBoxEVal.Location = new Point(408, 251);
      textBoxEVal.Name = "textBoxEVal";
      textBoxEVal.Size = new Size(49, 21);
      textBoxEVal.TabIndex = 53;
      textBoxEVal.Text = "1.0";
      buttonPLATemp.Location = new Point(13, 143);
      buttonPLATemp.Name = "buttonPLATemp";
      buttonPLATemp.Size = new Size(129, 32);
      buttonPLATemp.TabIndex = 55;
      buttonPLATemp.Text = "Heat to PLA Temp";
      buttonPLATemp.UseVisualStyleBackColor = true;
      buttonPLATemp.Click += new EventHandler(buttonPLATemp_Click);
      buttonDownE.BackgroundImage = (Image) Resources.arrowsEMinus;
      buttonDownE.BackgroundImageLayout = ImageLayout.Stretch;
      buttonDownE.Location = new Point(285, 223);
      buttonDownE.Name = "buttonDownE";
      buttonDownE.Size = new Size(101, 73);
      buttonDownE.TabIndex = 52;
      buttonDownE.UseVisualStyleBackColor = true;
      buttonDownE.Click += new EventHandler(buttonDownE_Click);
      buttonUpE.BackgroundImage = (Image) Resources.arrowsEPlus;
      buttonUpE.BackgroundImageLayout = ImageLayout.Stretch;
      buttonUpE.Location = new Point(477, 223);
      buttonUpE.Name = "buttonUpE";
      buttonUpE.Size = new Size(101, 73);
      buttonUpE.TabIndex = 51;
      buttonUpE.UseVisualStyleBackColor = true;
      buttonUpE.Click += new EventHandler(buttonUpE_Click);
      buttonABSTemp.Location = new Point(149, 143);
      buttonABSTemp.Name = "buttonABSTemp";
      buttonABSTemp.Size = new Size(129, 32);
      buttonABSTemp.TabIndex = 54;
      buttonABSTemp.Text = "Heat to ABS Temp";
      buttonABSTemp.UseVisualStyleBackColor = true;
      buttonABSTemp.Click += new EventHandler(buttonABSTemp_Click);
      buttonAddJob.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonAddJob.ForeColor = SystemColors.ControlText;
      buttonAddJob.Location = new Point(13, 75);
      buttonAddJob.Name = "buttonAddJob";
      buttonAddJob.Size = new Size(129, 32);
      buttonAddJob.TabIndex = 5;
      buttonAddJob.Text = "Add Print Job";
      buttonAddJob.UseVisualStyleBackColor = true;
      buttonAddJob.Click += new EventHandler(buttonAddJob_Click);
      buttonMotorsOff.Location = new Point(149, 6);
      buttonMotorsOff.Name = "buttonMotorsOff";
      buttonMotorsOff.Size = new Size(129, 32);
      buttonMotorsOff.TabIndex = 41;
      buttonMotorsOff.Text = "Motors Off";
      buttonMotorsOff.UseVisualStyleBackColor = true;
      buttonMotorsOff.Click += new EventHandler(buttonMotorsOff_Click);
      buttonMotorsOn.Location = new Point(13, 6);
      buttonMotorsOn.Name = "buttonMotorsOn";
      buttonMotorsOn.Size = new Size(129, 32);
      buttonMotorsOn.TabIndex = 40;
      buttonMotorsOn.Text = "Motors On";
      buttonMotorsOn.UseVisualStyleBackColor = true;
      buttonMotorsOn.Click += new EventHandler(buttonMotorsOn_Click);
      textBoxYVal.Location = new Point(408, 178);
      textBoxYVal.Name = "textBoxYVal";
      textBoxYVal.Size = new Size(49, 21);
      textBoxYVal.TabIndex = 39;
      textBoxYVal.Text = "10.0";
      textBoxXVal.Location = new Point(408, 105);
      textBoxXVal.Name = "textBoxXVal";
      textBoxXVal.Size = new Size(49, 21);
      textBoxXVal.TabIndex = 38;
      textBoxXVal.Text = "10.0";
      buttonPrintToFile.Location = new Point(149, 75);
      buttonPrintToFile.Name = "buttonPrintToFile";
      buttonPrintToFile.Size = new Size(129, 32);
      buttonPrintToFile.TabIndex = 28;
      buttonPrintToFile.Text = "Print to Binary File";
      buttonPrintToFile.UseVisualStyleBackColor = true;
      buttonPrintToFile.Click += new EventHandler(buttonPrintToFile_Click);
      textBoxZVal.Location = new Point(408, 34);
      textBoxZVal.Name = "textBoxZVal";
      textBoxZVal.Size = new Size(49, 21);
      textBoxZVal.TabIndex = 37;
      textBoxZVal.Text = "1.0";
      button2.Location = new Point(13, 41);
      button2.Name = "button2";
      button2.Size = new Size(129, 32);
      button2.TabIndex = 50;
      button2.Text = "Relative Mode";
      button2.UseVisualStyleBackColor = true;
      button2.Click += new EventHandler(button2_Click);
      buttonUpX.BackgroundImage = (Image) Resources.arrowsXPlus;
      buttonUpX.BackgroundImageLayout = ImageLayout.Stretch;
      buttonUpX.Location = new Point(477, 78);
      buttonUpX.Name = "buttonUpX";
      buttonUpX.Size = new Size(101, 73);
      buttonUpX.TabIndex = 36;
      buttonUpX.UseVisualStyleBackColor = true;
      buttonUpX.Click += new EventHandler(buttonUpX_Click);
      button1.Location = new Point(149, 41);
      button1.Name = "button1";
      button1.Size = new Size(129, 32);
      button1.TabIndex = 42;
      button1.Text = "Absolute Mode";
      button1.UseVisualStyleBackColor = true;
      button1.Click += new EventHandler(button1_Click);
      buttonDownX.BackgroundImage = (Image) Resources.arrowsXMinus;
      buttonDownX.BackgroundImageLayout = ImageLayout.Stretch;
      buttonDownX.Location = new Point(285, 78);
      buttonDownX.Name = "buttonDownX";
      buttonDownX.Size = new Size(101, 73);
      buttonDownX.TabIndex = 35;
      buttonDownX.UseVisualStyleBackColor = true;
      buttonDownX.Click += new EventHandler(buttonDownX_Click);
      buttonFilamentInfo.ForeColor = SystemColors.ControlText;
      buttonFilamentInfo.Location = new Point(13, 211);
      buttonFilamentInfo.Name = "buttonFilamentInfo";
      buttonFilamentInfo.Size = new Size(129, 32);
      buttonFilamentInfo.TabIndex = 44;
      buttonFilamentInfo.Text = "Set Filament Info";
      buttonFilamentInfo.UseVisualStyleBackColor = true;
      buttonFilamentInfo.Visible = false;
      buttonFilamentInfo.Click += new EventHandler(buttonChangeFilamentInfo_Click);
      buttonDownZ.BackgroundImage = (Image) Resources.arrowsZMinus;
      buttonDownZ.BackgroundImageLayout = ImageLayout.Stretch;
      buttonDownZ.Location = new Point(285, 5);
      buttonDownZ.Name = "buttonDownZ";
      buttonDownZ.Size = new Size(101, 73);
      buttonDownZ.TabIndex = 34;
      buttonDownZ.UseVisualStyleBackColor = true;
      buttonDownZ.Click += new EventHandler(buttonDownZ_Click);
      buttonDownY.BackgroundImage = (Image) Resources.arrowsYMinus;
      buttonDownY.BackgroundImageLayout = ImageLayout.Stretch;
      buttonDownY.Location = new Point(285, 150);
      buttonDownY.Name = "buttonDownY";
      buttonDownY.Size = new Size(101, 73);
      buttonDownY.TabIndex = 32;
      buttonDownY.UseVisualStyleBackColor = true;
      buttonDownY.Click += new EventHandler(buttonDownY_Click);
      buttonUpZ.BackgroundImage = (Image) Resources.arrowsZPlus;
      buttonUpZ.BackgroundImageLayout = ImageLayout.Stretch;
      buttonUpZ.Location = new Point(477, 5);
      buttonUpZ.Name = "buttonUpZ";
      buttonUpZ.Size = new Size(101, 73);
      buttonUpZ.TabIndex = 33;
      buttonUpZ.UseVisualStyleBackColor = true;
      buttonUpZ.Click += new EventHandler(buttonUpZ_Click);
      buttonUpY.BackgroundImage = (Image) Resources.arrowsYPlus;
      buttonUpY.BackgroundImageLayout = ImageLayout.Stretch;
      buttonUpY.Location = new Point(477, 150);
      buttonUpY.Name = "buttonUpY";
      buttonUpY.Size = new Size(101, 73);
      buttonUpY.TabIndex = 31;
      buttonUpY.UseVisualStyleBackColor = true;
      buttonUpY.Click += new EventHandler(buttonUpY_Click);
      tabPageDiagnostics.BackColor = SystemColors.Control;
      tabPageDiagnostics.Controls.Add((Control)buttonLoadEepromData);
      tabPageDiagnostics.Controls.Add((Control)buttonSaveEepromData);
      tabPageDiagnostics.Controls.Add((Control)buttonUpdateFirmware);
      tabPageDiagnostics.Controls.Add((Control)buttonPreprocess);
      tabPageDiagnostics.Controls.Add((Control)groupBox4);
      tabPageDiagnostics.Controls.Add((Control)buttonBacklashPrint);
      tabPageDiagnostics.Controls.Add((Control)buttonGetGCode);
      tabPageDiagnostics.Controls.Add((Control)buttonReCenter);
      tabPageDiagnostics.Controls.Add((Control)buttonHome);
      tabPageDiagnostics.Controls.Add((Control)buttonSpeedTest);
      tabPageDiagnostics.Controls.Add((Control)buttonXSpeedTest);
      tabPageDiagnostics.Controls.Add((Control)buttonYSkipTestBack);
      tabPageDiagnostics.Controls.Add((Control)buttonXSkipTestRight);
      tabPageDiagnostics.Controls.Add((Control)buttonYSkipTest);
      tabPageDiagnostics.Controls.Add((Control)buttonXSkipTest);
      tabPageDiagnostics.Location = new Point(4, 22);
      tabPageDiagnostics.Name = "tabPageDiagnostics";
      tabPageDiagnostics.Padding = new Padding(3);
      tabPageDiagnostics.Size = new Size(601, 303);
      tabPageDiagnostics.TabIndex = 1;
      tabPageDiagnostics.Text = "Diagnostics";
      buttonLoadEepromData.Location = new Point(448, 58);
      buttonLoadEepromData.Name = "buttonLoadEepromData";
      buttonLoadEepromData.Size = new Size(138, 36);
      buttonLoadEepromData.TabIndex = 29;
      buttonLoadEepromData.Text = "Load EEPROM Data";
      buttonLoadEepromData.UseVisualStyleBackColor = true;
      buttonLoadEepromData.Click += new EventHandler(buttonLoadEepromData_Click);
      buttonSaveEepromData.Location = new Point(448, 11);
      buttonSaveEepromData.Name = "buttonSaveEepromData";
      buttonSaveEepromData.Size = new Size(138, 36);
      buttonSaveEepromData.TabIndex = 28;
      buttonSaveEepromData.Text = "Save EEPROM Data";
      buttonSaveEepromData.UseVisualStyleBackColor = true;
      buttonSaveEepromData.Click += new EventHandler(buttonSaveEepromData_Click);
      buttonUpdateFirmware.Location = new Point(301, 157);
      buttonUpdateFirmware.Name = "buttonUpdateFirmware";
      buttonUpdateFirmware.Size = new Size(138, 36);
      buttonUpdateFirmware.TabIndex = 26;
      buttonUpdateFirmware.Text = "Update Firmware";
      buttonUpdateFirmware.UseVisualStyleBackColor = true;
      buttonUpdateFirmware.Click += new EventHandler(buttonUpdateFirmware_Click);
      buttonPreprocess.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonPreprocess.Location = new Point(155, 157);
      buttonPreprocess.Name = "buttonPreprocess";
      buttonPreprocess.Size = new Size(138, 36);
      buttonPreprocess.TabIndex = 27;
      buttonPreprocess.Text = "Backlash Settings";
      buttonPreprocess.UseVisualStyleBackColor = true;
      buttonPreprocess.Click += new EventHandler(buttonPreprocess_Click);
      groupBox4.Controls.Add((Control)buttonBedPointTest);
      groupBox4.Controls.Add((Control)comboBoxBedTestPoint);
      groupBox4.Location = new Point(9, 203);
      groupBox4.Name = "groupBox4";
      groupBox4.Size = new Size(297, 45);
      groupBox4.TabIndex = 10;
      groupBox4.TabStop = false;
      groupBox4.Text = "Bed Point Test";
      buttonBedPointTest.Location = new Point(7, 17);
      buttonBedPointTest.Name = "buttonBedPointTest";
      buttonBedPointTest.Size = new Size(131, 21);
      buttonBedPointTest.TabIndex = 1;
      buttonBedPointTest.Text = "Move to Point";
      buttonBedPointTest.UseVisualStyleBackColor = true;
      buttonBedPointTest.Click += new EventHandler(buttonBedPointTest_Click);
      comboBoxBedTestPoint.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxBedTestPoint.FormattingEnabled = true;
      comboBoxBedTestPoint.Items.AddRange(new object[5]
      {
        (object) "Center",
        (object) "Front Right",
        (object) "Front Left",
        (object) "Back Left",
        (object) "Back Right"
      });
      comboBoxBedTestPoint.Location = new Point(160, 18);
      comboBoxBedTestPoint.Name = "comboBoxBedTestPoint";
      comboBoxBedTestPoint.Size = new Size(129, 20);
      comboBoxBedTestPoint.TabIndex = 0;
      buttonBacklashPrint.Location = new Point(9, 157);
      buttonBacklashPrint.Name = "buttonBacklashPrint";
      buttonBacklashPrint.Size = new Size(138, 36);
      buttonBacklashPrint.TabIndex = 9;
      buttonBacklashPrint.Text = "Start Backlash Print";
      buttonBacklashPrint.UseVisualStyleBackColor = true;
      buttonBacklashPrint.Click += new EventHandler(buttonBacklashPrint_Click);
      buttonGetGCode.Location = new Point(301, 106);
      buttonGetGCode.Name = "buttonGetGCode";
      buttonGetGCode.Size = new Size(138, 36);
      buttonGetGCode.TabIndex = 8;
      buttonGetGCode.Text = "Get G-Code";
      buttonGetGCode.UseVisualStyleBackColor = true;
      buttonGetGCode.Click += new EventHandler(buttonGetGCode_Click);
      buttonReCenter.Location = new Point(301, 58);
      buttonReCenter.Name = "buttonReCenter";
      buttonReCenter.Size = new Size(138, 36);
      buttonReCenter.TabIndex = 7;
      buttonReCenter.Text = "Fast Re-Center";
      buttonReCenter.UseVisualStyleBackColor = true;
      buttonReCenter.Click += new EventHandler(buttonReCenter_Click);
      buttonHome.Location = new Point(301, 11);
      buttonHome.Name = "buttonHome";
      buttonHome.Size = new Size(138, 36);
      buttonHome.TabIndex = 6;
      buttonHome.Text = "Home";
      buttonHome.UseVisualStyleBackColor = true;
      buttonHome.Click += new EventHandler(buttonHome_Click);
      buttonSpeedTest.Location = new Point(155, 106);
      buttonSpeedTest.Name = "buttonSpeedTest";
      buttonSpeedTest.Size = new Size(138, 36);
      buttonSpeedTest.TabIndex = 5;
      buttonSpeedTest.Text = "Y Speed Test";
      buttonSpeedTest.UseVisualStyleBackColor = true;
      buttonSpeedTest.Click += new EventHandler(buttonYSpeedTest_Click);
      buttonXSpeedTest.Location = new Point(9, 106);
      buttonXSpeedTest.Name = "buttonXSpeedTest";
      buttonXSpeedTest.Size = new Size(138, 36);
      buttonXSpeedTest.TabIndex = 4;
      buttonXSpeedTest.Text = "X Speed Test";
      buttonXSpeedTest.UseVisualStyleBackColor = true;
      buttonXSpeedTest.Click += new EventHandler(buttonXSpeedTest_Click);
      buttonYSkipTestBack.Location = new Point(155, 58);
      buttonYSkipTestBack.Name = "buttonYSkipTestBack";
      buttonYSkipTestBack.Size = new Size(138, 36);
      buttonYSkipTestBack.TabIndex = 3;
      buttonYSkipTestBack.Text = "Y Skip Test /\\ (Back)";
      buttonYSkipTestBack.UseVisualStyleBackColor = true;
      buttonYSkipTestBack.Click += new EventHandler(buttonYSkipTestBack_Click);
      buttonXSkipTestRight.Location = new Point(9, 58);
      buttonXSkipTestRight.Name = "buttonXSkipTestRight";
      buttonXSkipTestRight.Size = new Size(138, 36);
      buttonXSkipTestRight.TabIndex = 2;
      buttonXSkipTestRight.Text = "X Skip Test -->(Right)";
      buttonXSkipTestRight.UseVisualStyleBackColor = true;
      buttonXSkipTestRight.Click += new EventHandler(buttonXSkipTestRight_Click);
      buttonYSkipTest.Location = new Point(155, 10);
      buttonYSkipTest.Name = "buttonYSkipTest";
      buttonYSkipTest.Size = new Size(138, 36);
      buttonYSkipTest.TabIndex = 1;
      buttonYSkipTest.Text = "Y Skip Test \\/ (Front)";
      buttonYSkipTest.UseVisualStyleBackColor = true;
      buttonYSkipTest.Click += new EventHandler(buttonYSkipTest_Click);
      buttonXSkipTest.Location = new Point(9, 10);
      buttonXSkipTest.Name = "buttonXSkipTest";
      buttonXSkipTest.Size = new Size(138, 36);
      buttonXSkipTest.TabIndex = 0;
      buttonXSkipTest.Text = "X Skip Test <--  (Left)";
      buttonXSkipTest.UseVisualStyleBackColor = true;
      buttonXSkipTest.Click += new EventHandler(buttonXSkipTest_Click);
      tabPageSDCard.BackColor = SystemColors.Control;
      tabPageSDCard.Controls.Add((Control)checkBoxCalibrateBeforeSDPrint);
      tabPageSDCard.Controls.Add((Control)buttonSDDeleteFile);
      tabPageSDCard.Controls.Add((Control)buttonSDPrint);
      tabPageSDCard.Controls.Add((Control)buttonSDSaveGcode);
      tabPageSDCard.Controls.Add((Control)buttonSDRefresh);
      tabPageSDCard.Controls.Add((Control)listBoxSDFiles);
      tabPageSDCard.Controls.Add((Control)label6);
      tabPageSDCard.Location = new Point(4, 22);
      tabPageSDCard.Name = "tabPageSDCard";
      tabPageSDCard.Size = new Size(601, 303);
      tabPageSDCard.TabIndex = 3;
      tabPageSDCard.Text = "Untethered Printing";
      checkBoxCalibrateBeforeSDPrint.AutoSize = true;
      checkBoxCalibrateBeforeSDPrint.Location = new Point(20, 217);
      checkBoxCalibrateBeforeSDPrint.Name = "checkBoxCalibrateBeforeSDPrint";
      checkBoxCalibrateBeforeSDPrint.Size = new Size(267, 16);
      checkBoxCalibrateBeforeSDPrint.TabIndex = 11;
      checkBoxCalibrateBeforeSDPrint.Text = "Calibrate printer before printing saved jobs.";
      checkBoxCalibrateBeforeSDPrint.UseVisualStyleBackColor = true;
      buttonSDDeleteFile.Location = new Point(359, 173);
      buttonSDDeleteFile.Name = "buttonSDDeleteFile";
      buttonSDDeleteFile.Size = new Size(138, 36);
      buttonSDDeleteFile.TabIndex = 10;
      buttonSDDeleteFile.Text = "Delete Saved Job";
      buttonSDDeleteFile.UseVisualStyleBackColor = true;
      buttonSDDeleteFile.Click += new EventHandler(buttonSDDelete_Click);
      buttonSDPrint.Location = new Point(359, (int) sbyte.MaxValue);
      buttonSDPrint.Name = "buttonSDPrint";
      buttonSDPrint.Size = new Size(138, 36);
      buttonSDPrint.TabIndex = 9;
      buttonSDPrint.Text = "Print Saved Job";
      buttonSDPrint.UseVisualStyleBackColor = true;
      buttonSDPrint.Click += new EventHandler(buttonSDPrint_Click);
      buttonSDSaveGcode.Location = new Point(359, 81);
      buttonSDSaveGcode.Name = "buttonSDSaveGcode";
      buttonSDSaveGcode.Size = new Size(138, 36);
      buttonSDSaveGcode.TabIndex = 8;
      buttonSDSaveGcode.Text = "Save Job to Printer";
      buttonSDSaveGcode.UseVisualStyleBackColor = true;
      buttonSDSaveGcode.Click += new EventHandler(buttonSDSaveGcode_Click);
      buttonSDRefresh.Location = new Point(359, 35);
      buttonSDRefresh.Name = "buttonSDRefresh";
      buttonSDRefresh.Size = new Size(138, 36);
      buttonSDRefresh.TabIndex = 7;
      buttonSDRefresh.Text = "Refresh";
      buttonSDRefresh.UseVisualStyleBackColor = true;
      buttonSDRefresh.Click += new EventHandler(buttonSDRefresh_Click);
      listBoxSDFiles.FormattingEnabled = true;
      listBoxSDFiles.ItemHeight = 12;
      listBoxSDFiles.Location = new Point(15, 35);
      listBoxSDFiles.Name = "listBoxSDFiles";
      listBoxSDFiles.Size = new Size(330, 172);
      listBoxSDFiles.TabIndex = 1;
      label6.AutoSize = true;
      label6.Location = new Point(10, 18);
      label6.Name = "label6";
      label6.Size = new Size(214, 12);
      label6.TabIndex = 0;
      label6.Text = "Print Jobs Saved to Internal Memory:";
      tabPageHeatedBedControl.BackColor = SystemColors.Control;
      tabPageHeatedBedControl.Controls.Add((Control)buttonTurnOfHeatedbed);
      tabPageHeatedBedControl.Controls.Add((Control)buttonBedHeatToABR);
      tabPageHeatedBedControl.Controls.Add((Control)buttonBedHeatToABS);
      tabPageHeatedBedControl.Controls.Add((Control)buttonBedHeatToPLA);
      tabPageHeatedBedControl.Location = new Point(4, 22);
      tabPageHeatedBedControl.Name = "tabPageHeatedBedControl";
      tabPageHeatedBedControl.Padding = new Padding(3);
      tabPageHeatedBedControl.Size = new Size(601, 303);
      tabPageHeatedBedControl.TabIndex = 2;
      tabPageHeatedBedControl.Text = "Heated Bed Control";
      buttonTurnOfHeatedbed.Location = new Point(6, 52);
      buttonTurnOfHeatedbed.Name = "buttonTurnOfHeatedbed";
      buttonTurnOfHeatedbed.Size = new Size(141, 32);
      buttonTurnOfHeatedbed.TabIndex = 63;
      buttonTurnOfHeatedbed.Text = "Turn off Heated Bed";
      buttonTurnOfHeatedbed.UseVisualStyleBackColor = true;
      buttonTurnOfHeatedbed.Click += new EventHandler(buttonTurnOfHeatedbed_Click);
      buttonBedHeatToABR.Location = new Point(304, 6);
      buttonBedHeatToABR.Name = "buttonBedHeatToABR";
      buttonBedHeatToABR.Size = new Size(141, 32);
      buttonBedHeatToABR.TabIndex = 61;
      buttonBedHeatToABR.Text = "Heat to ABS-R Temp";
      buttonBedHeatToABR.UseVisualStyleBackColor = true;
      buttonBedHeatToABR.Click += new EventHandler(buttonBedHeatToABR_Click);
      buttonBedHeatToABS.Location = new Point(155, 6);
      buttonBedHeatToABS.Name = "buttonBedHeatToABS";
      buttonBedHeatToABS.Size = new Size(141, 32);
      buttonBedHeatToABS.TabIndex = 60;
      buttonBedHeatToABS.Text = "Heat to ABS Temp";
      buttonBedHeatToABS.UseVisualStyleBackColor = true;
      buttonBedHeatToABS.Click += new EventHandler(buttonBedHeatToABS_Click);
      buttonBedHeatToPLA.Location = new Point(6, 6);
      buttonBedHeatToPLA.Name = "buttonBedHeatToPLA";
      buttonBedHeatToPLA.Size = new Size(141, 32);
      buttonBedHeatToPLA.TabIndex = 59;
      buttonBedHeatToPLA.Text = "Heat to PLA Temp";
      buttonBedHeatToPLA.UseVisualStyleBackColor = true;
      buttonBedHeatToPLA.Click += new EventHandler(buttonBedHeatToPLA_Click);
      buttonRepeatLast.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonRepeatLast.Location = new Point(383, 24);
      buttonRepeatLast.Name = "buttonRepeatLast";
      buttonRepeatLast.Size = new Size(78, 18);
      buttonRepeatLast.TabIndex = 43;
      buttonRepeatLast.Text = "Repeat";
      buttonRepeatLast.UseVisualStyleBackColor = true;
      buttonRepeatLast.Click += new EventHandler(buttonRepeatLast_Click);
      label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      label3.AutoSize = true;
      label3.Location = new Point(4, 8);
      label3.Name = "label3";
      label3.Size = new Size(131, 12);
      label3.TabIndex = 7;
      label3.Text = "Manual Insert G-Code";
      textBoxManualGCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      textBoxManualGCode.Location = new Point(8, 24);
      textBoxManualGCode.Name = "textBoxManualGCode";
      textBoxManualGCode.Size = new Size(278, 21);
      textBoxManualGCode.TabIndex = 8;
      textBoxManualGCode.Enter += new EventHandler(OnEnterInsertGCode);
      textBoxManualGCode.Leave += new EventHandler(OnLeaveInsertGCode);
      buttonSendGCode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonSendGCode.Location = new Point(297, 24);
      buttonSendGCode.Name = "buttonSendGCode";
      buttonSendGCode.Size = new Size(78, 18);
      buttonSendGCode.TabIndex = 9;
      buttonSendGCode.Text = "Send";
      buttonSendGCode.UseVisualStyleBackColor = true;
      buttonSendGCode.Click += new EventHandler(buttonSendGCode_Click);
      buttonClearLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      buttonClearLog.Location = new Point(9, 252);
      buttonClearLog.Name = "buttonClearLog";
      buttonClearLog.Size = new Size(87, 21);
      buttonClearLog.TabIndex = 41;
      buttonClearLog.Text = "Clear Log";
      buttonClearLog.UseVisualStyleBackColor = true;
      buttonClearLog.Click += new EventHandler(buttonClearLog_Click);
      checkBoxLogToScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      checkBoxLogToScreen.AutoSize = true;
      checkBoxLogToScreen.Location = new Point(240, 254);
      checkBoxLogToScreen.Name = "checkBoxLogToScreen";
      checkBoxLogToScreen.Size = new Size(103, 16);
      checkBoxLogToScreen.TabIndex = 40;
      checkBoxLogToScreen.Text = "Log to Screen";
      checkBoxLogToScreen.UseVisualStyleBackColor = true;
      checkBoxLogToScreen.CheckedChanged += new EventHandler(OnLogToScreenChanged);
      richTextBoxLoggedItems.Location = new Point(8, 144);
      richTextBoxLoggedItems.Name = "richTextBoxLoggedItems";
      richTextBoxLoggedItems.Size = new Size(375, 104);
      richTextBoxLoggedItems.TabIndex = 42;
      richTextBoxLoggedItems.Text = "";
      richTextBoxLoggedItems.MouseUp += new MouseEventHandler(richTextBoxLoggedItems_MouseUp);
      buttonEmergencyStop.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      buttonEmergencyStop.BackColor = SystemColors.Control;
      buttonEmergencyStop.ForeColor = System.Drawing.Color.Red;
      buttonEmergencyStop.Location = new Point(200, 49);
      buttonEmergencyStop.Name = "buttonEmergencyStop";
      buttonEmergencyStop.Size = new Size(184, 30);
      buttonEmergencyStop.TabIndex = 0;
      buttonEmergencyStop.Text = "EMERGENCY STOP";
      buttonEmergencyStop.UseVisualStyleBackColor = false;
      buttonEmergencyStop.Click += new EventHandler(buttonEmergencyStop_Click);
      label5.AutoSize = true;
      label5.Location = new Point(7, 126);
      label5.Name = "label5";
      label5.Size = new Size(89, 12);
      label5.TabIndex = 39;
      label5.Tag = (object) "";
      label5.Text = "Command Log";
      listBoxManualHistory.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      listBoxManualHistory.FormattingEnabled = true;
      listBoxManualHistory.ItemHeight = 12;
      listBoxManualHistory.Location = new Point(13, 320);
      listBoxManualHistory.Name = "listBoxManualHistory";
      listBoxManualHistory.Size = new Size(370, 64);
      listBoxManualHistory.TabIndex = 38;
      label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      label4.AutoSize = true;
      label4.Location = new Point(10, 304);
      label4.Name = "label4";
      label4.Size = new Size(103, 12);
      label4.TabIndex = 37;
      label4.Text = "Manual G-Codes";
      checkBoxAutoScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      checkBoxAutoScroll.AutoSize = true;
      checkBoxAutoScroll.Checked = true;
      checkBoxAutoScroll.CheckState = CheckState.Checked;
      checkBoxAutoScroll.Location = new Point(129, 254);
      checkBoxAutoScroll.Name = "checkBoxAutoScroll";
      checkBoxAutoScroll.Size = new Size(85, 16);
      checkBoxAutoScroll.TabIndex = 44;
      checkBoxAutoScroll.Text = "Auto Scroll";
      checkBoxAutoScroll.UseVisualStyleBackColor = true;
      groupBox3.Controls.Add((Control)groupBox1);
      groupBox3.Controls.Add((Control)groupBox2);
      groupBox3.Controls.Add((Control)label7);
      groupBox3.Location = new Point(18, 28);
      groupBox3.Name = "groupBox3";
      groupBox3.Size = new Size(461, 226);
      groupBox3.TabIndex = 4;
      groupBox3.TabStop = false;
      groupBox3.Text = "Temperature Controls";
      AutoScaleDimensions = new SizeF(7f, 12f);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = System.Drawing.Color.WhiteSmoke;
      ClientSize = new Size(1040, 547);
      Controls.Add((Control)groupBoxPrinterControls);
      Controls.Add((Control)groupBoxPrinterList);
      Controls.Add((Control)menuStrip1);
      FormBorderStyle = FormBorderStyle.FixedSingle;
      Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      MainMenuStrip = menuStrip1;
      MaximizeBox = false;
      Name = nameof (MainForm);
      Text = "M3D Print Spooler";
      FormClosing += new FormClosingEventHandler(OnClosing);
      Load += new EventHandler(OnLoad);
      menuStrip1.ResumeLayout(false);
      menuStrip1.PerformLayout();
      groupBoxPrinterList.ResumeLayout(false);
      groupBoxPrinterList.PerformLayout();
      groupBoxPrinterControls.ResumeLayout(false);
      groupBoxPrinterControls.PerformLayout();
      groupBoxPrinting.ResumeLayout(false);
      groupBox2.ResumeLayout(false);
      groupBox2.PerformLayout();
      groupBox1.ResumeLayout(false);
      groupBox1.PerformLayout();
      groupBoxControls.ResumeLayout(false);
      groupBoxBootloaderOptions.ResumeLayout(false);
      groupBoxBootloaderOptions.PerformLayout();
      groupBoxFirmwareControls.ResumeLayout(false);
      groupBoxFirmwareControls.PerformLayout();
      tabControl1.ResumeLayout(false);
      tabPageBasicOptions.ResumeLayout(false);
      tabPageBasicOptions.PerformLayout();
      tabPageDiagnostics.ResumeLayout(false);
      groupBox4.ResumeLayout(false);
      tabPageSDCard.ResumeLayout(false);
      tabPageSDCard.PerformLayout();
      tabPageHeatedBedControl.ResumeLayout(false);
      groupBox3.ResumeLayout(false);
      groupBox3.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    private enum PrinterLockChanging
    {
      No,
      NoFailed,
      YesToLocked,
      YesToUnlocked,
    }

    private class PrinterData
    {
      public bool bPowerOutageHandled;

      public PrinterData()
      {
        bPowerOutageHandled = false;
      }
    }
  }
}
