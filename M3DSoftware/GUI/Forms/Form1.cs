using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms.Splash;
using M3D.GUI.Interfaces;
using M3D.GUI.OpenGL;
using M3D.GUI.Views;
using M3D.GUI.Views.Library_View;
using M3D.GUI.Views.Printer_View;
using M3D.Slicer.Cura15_04;
using M3D.Slicer.General;
using M3D.Spooling.Common.Utils;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace M3D.GUI.Forms
{
  public class Form1 : Form
  {
    public static DebugLogger debugLogger = new DebugLogger(Paths.StartupLogPath, 500U);
    public bool zoomed;
    public bool AskForUpdate;
    private SplashForm splashForm;
    private Updater SoftwareUpdater;
    private double elapsed;
    private double elapsed_frame;
    private Stopwatch fps_lock_watch;
    private Stopwatch fps_frame_counter;
    private string[] args;
    private System.Windows.Forms.Timer timer1;
    private int TEMPORARY_run_count;
    private IFileAssociations myFileAssociations;
    private IStopShutdown myStopShutdown;
    private Stopwatch keypress_stopwatch;
    private bool shift;
    private bool alt;
    private bool ctrl;
    private bool tab;
    private bool infocus;
    private SettingsManager settingsManager;
    private ModelLoadingManager model_loading_manager;
    private SpoolerConnection spooler_connection;
    private PrinterStatusDialogOrganizer printer_status_dialog_organizer;
    public GUIHost m_gui_host;
    private ControlBar controlbar;
    private PopupMessageBox messagebox;
    private MessagePopUp informationbox;
    public LibraryView libraryview;
    private OpenGLConnection OpenGLConnection;
    private bool resized;
    private PrinterView printerView;
    private bool force_close;
    private Queue<Form1.MainThreadTask> mainThreadTaskQueue;
    private IContainer components;
    public GLControl glControl1;
    private ContextMenuStrip contextMenuStrip1;

    public Form1(SplashForm splashForm, string[] args)
    {
      var str = (string) null;
      try
      {
        str = "InitializePlatformSpecificObjects";
        InitializePlatformSpecificObjects();
        this.args = args;
        AllowDrop = true;
        DragEnter += new DragEventHandler(Form1_DragEnter);
        DragDrop += new DragEventHandler(Form1_DragDrop);
        mainThreadTaskQueue = new Queue<Form1.MainThreadTask>();
        Form1.debugLogger.Add("Form1() Constructor", "Constructor for the main Form.", DebugLogger.LogType.Secondary);
        ExceptionForm.form1 = this;
        str = "splashForm.Show";
        this.splashForm = splashForm;
        splashForm.Show();
        str = "InitializeComponent";
        InitializeComponent();
        settingsManager = new SettingsManager(FileAssociations);
        Form1.debugLogger.Add("Form1() Constructor", "SettingsManager created.", DebugLogger.LogType.Secondary);
        timer1 = new System.Windows.Forms.Timer
        {
          Interval = 16
        };
        timer1.Tick += new EventHandler(on_timerTick);
        timer1.Start();
        MouseWheel += new MouseEventHandler(Form1_MouseWheel);
        fps_lock_watch = new Stopwatch();
        fps_frame_counter = new Stopwatch();
      }
      catch (Exception ex)
      {
        var extra_info = str;
        ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
    }

    private void Form1_DragEnter(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
      {
        return;
      }

      e.Effect = DragDropEffects.Copy;
    }

    private void Form1_DragDrop(object sender, DragEventArgs e)
    {
      var data = (string[]) e.Data.GetData(DataFormats.FileDrop);
      if (data != null && data.Length != 0 && data[0] != null)
      {
        model_loading_manager.LoadModelIntoPrinter(data[0]);
      }
      else
      {
        informationbox.AddMessageToQueue("Unable to find model.");
      }
    }

    public void StopTimers()
    {
      if (timer1 == null)
      {
        return;
      }

      timer1.Stop();
    }

    private void on_timerTick(object sender, EventArgs e)
    {
      timer1.Stop();
      var str = (string) null;
      ++TEMPORARY_run_count;
      try
      {
        str = "on_timerTick::views.Process";
        if (TEMPORARY_run_count <= 3)
        {
          Form1.debugLogger.Add(nameof (on_timerTick), str + TEMPORARY_run_count, DebugLogger.LogType.Primary);
        }

        str = "on_timerTick::Refresh";
        if (!contextMenuStrip1.Visible)
        {
          Refresh();
        }

        if (TEMPORARY_run_count <= 3)
        {
          Form1.debugLogger.Add(nameof (on_timerTick), str + TEMPORARY_run_count, DebugLogger.LogType.Primary);
        }

        str = "on_timerTick::RefreshViews";
        if (m_gui_host != null)
        {
          if (resized)
          {
            m_gui_host.RefreshViews();
            resized = false;
          }
          str = "on_timerTick::m_gui_host.OnUpdate()";
          m_gui_host.OnUpdate();
        }
        if (TEMPORARY_run_count <= 3)
        {
          Form1.debugLogger.Add(nameof (on_timerTick), str + TEMPORARY_run_count, DebugLogger.LogType.Primary);
        }
      }
      catch (Exception ex)
      {
        var extra_info = str;
        ExceptionForm.ShowExceptionForm(ex, extra_info);
      }
      lock (mainThreadTaskQueue)
      {
        while (mainThreadTaskQueue.Count > 0)
        {
          Form1.MainThreadTask mainThreadTask = mainThreadTaskQueue.Dequeue();
          mainThreadTask.the_task(mainThreadTask.data);
        }
      }
      timer1.Start();
    }

    private void glControl1_Load(object sender, EventArgs e)
    {
      OpenGLConnection = new OpenGLConnection();
      try
      {
        splashForm.Hide();
        splashForm.Show();
        splashForm.TopMost = true;
        OpenGLConnection.OnLoad(glControl1, Form1.debugLogger);
        InitGUI();
        var slicer_connection = (SlicerConnectionBase) new M3D.Slicer.Cura15_04.SlicerConnectionCura(Paths.WorkingFolder, Paths.ResourceFolder);
        model_loading_manager = new ModelLoadingManager();
        spooler_connection = new SpoolerConnection(messagebox, informationbox, settingsManager);
        SoftwareUpdater = new Updater(this, messagebox, spooler_connection, settingsManager);
        controlbar = new ControlBar(this, m_gui_host, settingsManager, messagebox, informationbox, spooler_connection, model_loading_manager, SoftwareUpdater);
        var frame = new Frame(24680);
        frame.SetPosition(0, 0);
        frame.RelativeWidth = 1f;
        frame.RelativeHeight = 1f;
        frame.BGColor = new Color4(0.913725f, 0.905882f, 0.9098f, 1f);
        m_gui_host.AddElement(frame);
        libraryview = new LibraryView(10001, frame, glControl1, m_gui_host, informationbox, model_loading_manager);
        m_gui_host.SetFocus(1001);
        m_gui_host.Refresh();
        Form1.debugLogger.Add("glControl1_Load()", "LibraryView created.", DebugLogger.LogType.Secondary);
        printerView = new PrinterView(this, m_gui_host, OpenGLConnection, spooler_connection, slicer_connection, model_loading_manager, messagebox, informationbox, settingsManager, libraryview);
        printerView.SetViewPointPos(0.0f, 100f, 400f);
        Form1.debugLogger.Add("glControl1_Load()", "GLPrinterView created.", DebugLogger.LogType.Secondary);
        frame.AddChildElement(printerView);
        frame.AddChildElement(libraryview);
        Form1.debugLogger.Add("glControl1_Load()", "Views added to background view.", DebugLogger.LogType.Secondary);
        model_loading_manager.Init(settingsManager, libraryview, printerView, messagebox, informationbox);
        Form1.debugLogger.Add("glControl1_Load()", "Model Loading Manager Initialized.", DebugLogger.LogType.Secondary);
        printer_status_dialog_organizer = new PrinterStatusDialogOrganizer(spooler_connection, model_loading_manager, settingsManager, this, m_gui_host, printerView, messagebox);
        Form1.debugLogger.Add("glControl1_Load()", "PrinterStatusDialogOrganizer Initialized.", DebugLogger.LogType.Secondary);
        spooler_connection.SpoolerStartUp(Form1.debugLogger);
        Form1.debugLogger.Add("glControl1_Load()", "spooler_connection.SpoolerStartUp() completed.", DebugLogger.LogType.Secondary);
        controlbar.UpdateSettings();
        Form1.debugLogger.Add("glControl1_Load()", "controlbar.UpdateSettings() completed.", DebugLogger.LogType.Secondary);
        if (settingsManager.CurrentAppearanceSettings.StartFullScreen)
        {
          WindowState = FormWindowState.Maximized;
        }
        else
        {
          WindowState = FormWindowState.Normal;
        }

        splashForm.Close();
        Form1.debugLogger.Add("glControl1_Load()", "splash form closed.", DebugLogger.LogType.Secondary);
        glControl1.MakeCurrent();
        glControl1.VSync = false;
        Form1.debugLogger.Add("glControl1_Load()", "glcontrol sync", DebugLogger.LogType.Secondary);
        if (SplashFormFirstRun.WasRunForTheFirstTime)
        {
          var welcomeDialog = new WelcomeDialog(1209, messagebox);
          welcomeDialog.Init(m_gui_host);
          m_gui_host.GlobalChildDialog += welcomeDialog;
        }
        else
        {
          messagebox.AllowMessages = true;
        }

        Form1.debugLogger.Add("glControl1_Load()", "Welcome Initialized", DebugLogger.LogType.Secondary);
        CheckFileAssociations();
        Form1.debugLogger.Add("glControl1_Load()", "File Associations Checked", DebugLogger.LogType.Secondary);
        var num = spooler_connection.PrintSpoolerClient.IsPrinting ? 0 : (!Program.runfirst_start ? 1 : 0);
        SoftwareUpdater.CheckForUpdate(false);
        Form1.debugLogger.Add("glControl1_Load()", "Checked for updates", DebugLogger.LogType.Secondary);
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
      if (args.Length != 0)
      {
        model_loading_manager.LoadModelIntoPrinter(args[0]);
      }

      FileAssociationSingleInstance.OnNewInstance += new NewInstanceEvent(OnNewInstanceEvent);
    }

    public void SetToEditView()
    {
      if (printerView != null && printerView.TransitionViewState(ViewState.Active))
      {
        Form1.debugLogger.Add("SetToEditView()", "Setting to edit view.", DebugLogger.LogType.Secondary);
      }

      libraryview.TransitionViewState(ViewState.Hidden);
    }

    public void SetToLibraryView()
    {
      if (printerView.TransitionViewState(ViewState.Hidden))
      {
        Form1.debugLogger.Add("SetToLibraryView()", "Setting to library view.", DebugLogger.LogType.Secondary);
      }

      libraryview.TransitionViewState(ViewState.Active);
    }

    private void OnNewInstanceEvent(string[] args)
    {
      try
      {
        if (printerView.IsModelLoaded() && !settingsManager.CurrentAppearanceSettings.UseMultipleModels && !settingsManager.CurrentAppearanceSettings.ShowRemoveModelWarning)
        {
          if (new LoadNewModelForm().ShowDialog() != DialogResult.Yes || args.Length == 0)
          {
            return;
          }

          model_loading_manager.LoadModelIntoPrinter(args[0]);
        }
        else
        {
          model_loading_manager.LoadModelIntoPrinter(args[0]);
        }
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    private void CheckFileAssociations()
    {
      try
      {
        if (FileAssociations == null)
        {
          return;
        }

        var startupPath = Application.StartupPath;
        var str1 = FileAssociations.ExtensionOpenWith(".stl");
        var str2 = FileAssociations.ExtensionOpenWith(".obj");
        var associationsDialog = settingsManager.Settings.miscSettings.FileAssociations.ShowFileAssociationsDialog;
        if (str1 == null || str1 != null && !str1.Contains(Application.ExecutablePath) || (str2 == null || str2 != null && !str2.Contains(Application.ExecutablePath)))
        {
          if (!associationsDialog && !SplashFormFirstRun.WasRunForTheFirstTime)
          {
            return;
          }

          var associationsForm = new AssociationsForm(settingsManager, messagebox, FileAssociations, Application.ExecutablePath, startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
        }
        else
        {
          FileAssociations.Set3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.stl)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
          FileAssociations.Set3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file", Application.ExecutablePath, "M3D file (.obj)", startupPath + "/Resources/Data\\GUIImages\\M3D32x32Icon.ico");
        }
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    public IFileAssociations FileAssociations
    {
      get
      {
        return myFileAssociations;
      }
    }

    private void InitializePlatformSpecificObjects()
    {
      myFileAssociations = null;
      myStopShutdown = new WinStopShutdown(Handle);
      myFileAssociations = new WinFileAssociations();
    }

    private void glControl1_Paint(object sender, PaintEventArgs e)
    {
      try
      {
        if (!fps_lock_watch.IsRunning)
        {
          fps_lock_watch.Start();
        }
        else
        {
          elapsed = fps_lock_watch.ElapsedTicks / (double)Stopwatch.Frequency;
          if (elapsed + elapsed_frame >= 1.0 / 60.0)
          {
            fps_lock_watch.Reset();
            fps_lock_watch.Start();
            fps_frame_counter.Reset();
            fps_frame_counter.Start();
            OpenGLConnection.OnPaint(() =>
           {
             if (resized)
             {
               return;
             }

             m_gui_host.Render();
           });
            fps_frame_counter.Stop();
            elapsed_frame = fps_frame_counter.ElapsedTicks / (double)Stopwatch.Frequency;
          }
          else
          {
            Thread.Sleep(5);
          }
        }
      }
      catch (Exception ex)
      {
        ExceptionForm.ShowExceptionForm(ex);
      }
    }

    private void Form1_MouseDown(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Down;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      m_gui_host.OnMouseCommand(mouseevent);
    }

    private void Form1_MouseMove(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Move;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      try
      {
        m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseUp(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Up;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.button = e.Button != MouseButtons.Middle ? (e.Button != MouseButtons.Left ? (e.Button != MouseButtons.Right ? MouseButton.None : MouseButton.Right) : MouseButton.Left) : MouseButton.Middle;
      mouseevent.num_clicks = e.Clicks;
      mouseevent.delta = 0;
      try
      {
        m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseLeave(object sender, EventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.Leave;
      mouseevent.pos.x = 0;
      mouseevent.pos.y = 0;
      mouseevent.button = MouseButton.None;
      mouseevent.num_clicks = 0;
      mouseevent.delta = 0;
      try
      {
        m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
    }

    private void Form1_MouseWheel(object sender, MouseEventArgs e)
    {
      MouseEvent mouseevent;
      mouseevent.type = MouseEventType.MouseWheel;
      mouseevent.pos.x = e.X;
      mouseevent.pos.y = e.Y;
      mouseevent.num_clicks = 0;
      mouseevent.button = MouseButton.None;
      mouseevent.delta = e.Delta;
      contextMenuStrip1.Visible = false;
      try
      {
        m_gui_host.OnMouseCommand(mouseevent);
      }
      catch (Exception ex)
      {
      }
      Refresh();
    }

    private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
    {
      if (!infocus)
      {
        return;
      }

      try
      {
        m_gui_host.OnKeyboardEvent(new InputKeyEvent(e.KeyChar, shift, alt, ctrl));
      }
      catch (Exception ex)
      {
      }
    }

    private void OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
      switch (e.KeyData & Keys.KeyCode)
      {
        case Keys.ShiftKey:
        case Keys.ControlKey:
        case Keys.Menu:
        case Keys.End:
        case Keys.Home:
        case Keys.Left:
        case Keys.Up:
        case Keys.Right:
        case Keys.Down:
          e.IsInputKey = true;
          break;
      }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      try
      {
        if (keyData == Keys.Tab)
        {
          tab = true;
          m_gui_host.OnKeyboardEvent(new InputKeyEvent(' ', shift, alt, ctrl, tab));
        }
      }
      catch (Exception ex)
      {
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (!infocus)
      {
        return;
      }

      try
      {
        if (keypress_stopwatch == null)
        {
          keypress_stopwatch = new Stopwatch();
        }

        switch (e.KeyCode & Keys.KeyCode)
        {
          case Keys.ShiftKey:
            shift = true;
            break;
          case Keys.ControlKey:
            ctrl = true;
            break;
          case Keys.Menu:
            alt = true;
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Alt, shift, alt, ctrl));
            break;
          case Keys.End:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.End, shift, alt, ctrl));
            break;
          case Keys.Home:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Home, shift, alt, ctrl));
            break;
          case Keys.Left:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Left, shift, alt, ctrl));
            break;
          case Keys.Up:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Up, shift, alt, ctrl));
            break;
          case Keys.Right:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Right, shift, alt, ctrl));
            break;
          case Keys.Down:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Down, shift, alt, ctrl));
            break;
          case Keys.Delete:
            m_gui_host.OnKeyboardEvent(new CommandKeyEvent(KeyboardCommandKey.Delete, shift, alt, ctrl));
            break;
          case Keys.C:
            if (!alt && !ctrl)
            {
              break;
            }

            m_gui_host.OnKeyboardEvent(new InputKeyEvent('C', shift, alt, ctrl));
            break;
          case Keys.V:
            if (!alt && !ctrl)
            {
              break;
            }

            m_gui_host.OnKeyboardEvent(new InputKeyEvent('V', shift, alt, ctrl));
            break;
          case Keys.X:
            if (!alt && !ctrl)
            {
              break;
            }

            m_gui_host.OnKeyboardEvent(new InputKeyEvent('X', shift, alt, ctrl));
            break;
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (!infocus)
      {
        return;
      }

      switch (e.KeyCode & Keys.KeyCode)
      {
        case Keys.ShiftKey:
          shift = false;
          break;
        case Keys.ControlKey:
          ctrl = false;
          break;
        case Keys.Menu:
          alt = false;
          break;
      }
    }

    private void OnLeaveFocus(object sender, EventArgs e)
    {
      infocus = false;
      shift = false;
      alt = false;
      ctrl = false;
    }

    private void OnEnterFocus(object sender, EventArgs e)
    {
      infocus = true;
    }

    private void glControl1_Resize(object sender, EventArgs e)
    {
      if (m_gui_host != null)
      {
        m_gui_host.OnResize(glControl1.Width, glControl1.Height);
      }

      resized = true;
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
      FileAssociationSingleInstance.UnRegisterAsSingleInstance();
      if (controlbar != null)
      {
        controlbar.SaveSettings();
      }

      if (model_loading_manager != null)
      {
        model_loading_manager.OnShutdown();
      }

      if (settingsManager != null)
      {
        settingsManager.OnShutdown();
      }

      if (spooler_connection == null)
      {
        return;
      }

      spooler_connection.OnShutdown();
    }

    private void InitGUI()
    {
      Locale.GlobalLocale = new Locale(Path.Combine(Paths.ReadOnlyDataFolder, "locales", "English.locale.xml"));
      m_gui_host = new GUIHost(Locale.GlobalLocale, 11f, Paths.PublicDataFolder, glControl1);
      m_gui_host.OnResize(glControl1.Width, glControl1.Height);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      messagebox = new PopupMessageBox(0);
      messagebox.Init(m_gui_host);
      informationbox = new MessagePopUp(0, settingsManager);
      informationbox.Init(m_gui_host);
    }

    public void OpenSettingsDialogCalibrationPage()
    {
      if (controlbar == null)
      {
        return;
      }

      controlbar.OpenSettingsDialogCalibrationPage();
    }

    public void OpenSettingsDialogAdvancedCalibrate(string printer_serial_number)
    {
      if (controlbar == null)
      {
        return;
      }

      controlbar.OpenSettingsDialogAdvancedCalibrate(printer_serial_number);
    }

    public void OpenSettingsDialogFilamentManagement()
    {
      if (controlbar == null)
      {
        return;
      }

      controlbar.OpenSettingsDialogFilamentManagement();
    }

    public void OpenSettingsDialogChangeFilament()
    {
      if (controlbar == null)
      {
        return;
      }

      controlbar.OpenSettingsDialogChangeFilament();
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (force_close || spooler_connection == null || (myStopShutdown == null || e.CloseReason != CloseReason.WindowsShutDown) || (spooler_connection.PrintSpoolerClient == null || !spooler_connection.PrintSpoolerClient.IsPrinting))
      {
        return;
      }

      myStopShutdown.CreateShutdownMessage("This app is preventing shutdown because this will cancel print jobs.");
      var confirmShutdownForm = new ConfirmShutdownForm();
      var num1 = (int) confirmShutdownForm.ShowDialog();
      if (confirmShutdownForm.shutdown)
      {
        force_close = true;
        var num2 = (int)spooler_connection.PrintSpoolerClient.ForceSpoolerShutdown();
      }
      else
      {
        e.Cancel = true;
      }

      myStopShutdown.DestroyShutdownMessage();
    }

    public void AddTask(Form1.Task task, object data)
    {
      lock (mainThreadTaskQueue)
      {
        mainThreadTaskQueue.Enqueue(new Form1.MainThreadTask(task, data));
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && components != null)
      {
        components.Dispose();
      }

      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      components = new Container();
      var componentResourceManager = new ComponentResourceManager(typeof (Form1));
      glControl1 = new GLControl(new GraphicsMode(new ColorFormat(32), 24, 0, 8), 3, 0, GraphicsContextFlags.ForwardCompatible);
      contextMenuStrip1 = new ContextMenuStrip(components);
      contextMenuStrip1.SuspendLayout();
      SuspendLayout();
      glControl1.BackColor = Color.Black;
      componentResourceManager.ApplyResources(glControl1, "glControl1");
      glControl1.Name = "glControl1";
      glControl1.VSync = false;
      glControl1.Load += new EventHandler(glControl1_Load);
      glControl1.Paint += new PaintEventHandler(glControl1_Paint);
      glControl1.Enter += new EventHandler(OnEnterFocus);
      glControl1.KeyDown += new KeyEventHandler(OnKeyDown);
      glControl1.KeyPress += new KeyPressEventHandler(OnKeyPress);
      glControl1.KeyUp += new KeyEventHandler(OnKeyUp);
      glControl1.Leave += new EventHandler(OnLeaveFocus);
      glControl1.MouseDown += new MouseEventHandler(Form1_MouseDown);
      glControl1.MouseLeave += new EventHandler(Form1_MouseLeave);
      glControl1.MouseMove += new MouseEventHandler(Form1_MouseMove);
      glControl1.MouseUp += new MouseEventHandler(Form1_MouseUp);
      glControl1.PreviewKeyDown += new PreviewKeyDownEventHandler(OnPreviewKeyDown);
      glControl1.Resize += new EventHandler(glControl1_Resize);
      componentResourceManager.ApplyResources(this, "$this");
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add(glControl1);
      Name = nameof (Form1);
      FormClosing += new FormClosingEventHandler(Form1_FormClosing);
      FormClosed += new FormClosedEventHandler(Form1_FormClosed);
      Load += new EventHandler(Form1_Load);
      MouseDown += new MouseEventHandler(Form1_MouseDown);
      MouseLeave += new EventHandler(Form1_MouseLeave);
      MouseMove += new MouseEventHandler(Form1_MouseMove);
      MouseUp += new MouseEventHandler(Form1_MouseUp);
      contextMenuStrip1.ResumeLayout(false);
      ResumeLayout(false);
    }

    public struct MainThreadTask
    {
      public Form1.Task the_task;
      public object data;

      public MainThreadTask(Form1.Task task, object data)
      {
        the_task = task;
        this.data = data;
      }
    }

    public delegate void Task(object data);
  }
}
