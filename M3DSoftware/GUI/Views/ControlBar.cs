// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.ControlBar
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.AccessoriesDialog;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.GUI.ManageFilament;
using M3D.GUI.SettingsPages;
using M3D.Properties;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System.Collections.Generic;
using System.Diagnostics;

namespace M3D.GUI.Views
{
  public class ControlBar : Frame
  {
    private MessagePopUp message_window;
    private ModelLoadingManager m_oModelLoadingManager;
    private SettingsManager m_oSettingsManager;
    private SpoolerConnection m_oSpoolerConnection;
    private GUIHost m_GUIHost;
    private SettingsDialogWidget m_oSettingsDialog;
    private MultiPrinterDialogWidget m_oMultiPrinterDialog;
    private Manage3DInkMainWindow m_oManageFilamentDialog;
    private MainWindow m_oAccessoriesDialog;
    private ButtonWidget information;
    private Frame loading_frame;
    private TextWidget loading_text;
    private SpriteAnimationWidget loading_progress;
    private ButtonWidget m_owidgetFilamentButton;
    private ButtonWidget m_owidgetAccessoriesButton;
    private ButtonWidget m_owidgetOpenmodelButton;
    private ButtonWidget m_owidgetHelpButton;
    private ButtonWidget m_owidgetSettingsButton;
    private ButtonWidget m_owidgetPrinterButton;
    private XMLFrame driversInstalling_frame;
    public TextWidget diagnostics;
    public List<Frame> PopUpDialogList;

    public ControlBar(Form1 form1, GUIHost host, SettingsManager settingsManager, PopupMessageBox messagebox, MessagePopUp infobox, SpoolerConnection spooler_connection, ModelLoadingManager model_loading_manager, Updater softwareUpdater)
      : base(0, (Element2D) null)
    {
      this.m_oSpoolerConnection = spooler_connection;
      this.m_oSettingsManager = settingsManager;
      this.m_oModelLoadingManager = model_loading_manager;
      this.m_GUIHost = host;
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(this.OnSelectedPrinterChanged);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      this.X = 0;
      this.Y = 0;
      this.RelativeWidth = 1f;
      this.Height = 50;
      this.BGColor = new Color4(0.913725f, 0.905882f, 0.9098f, 1f);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      int x1 = 10;
      this.m_owidgetFilamentButton = new ButtonWidget(8);
      this.m_owidgetFilamentButton.Text = "T_3DINK";
      this.m_owidgetFilamentButton.Size = FontSize.Large;
      this.m_owidgetFilamentButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetFilamentButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetFilamentButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetFilamentButton.Alignment = QFontAlignment.Left;
      this.m_owidgetFilamentButton.SetSize(170, 50);
      this.m_owidgetFilamentButton.SetPosition(x1, 0);
      this.m_owidgetFilamentButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetFilamentButton.Init(host, "guicontrols", 0.0f, 6f, 157f, 57f, 0.0f, 70f, 157f, 121f, 0.0f, 134f, 157f, 185f);
      this.m_owidgetFilamentButton.SetGrowableWidth(2, 2, 160);
      this.m_owidgetFilamentButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_3DINK");
      int x2 = x1 + this.m_owidgetFilamentButton.Width;
      this.m_owidgetAccessoriesButton = new ButtonWidget(13);
      this.m_owidgetAccessoriesButton.Text = "T_ACCESSORIES";
      this.m_owidgetAccessoriesButton.Size = FontSize.Large;
      this.m_owidgetAccessoriesButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetAccessoriesButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetAccessoriesButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetAccessoriesButton.Alignment = QFontAlignment.Justify;
      this.m_owidgetAccessoriesButton.SetPosition(x2, 8);
      this.m_owidgetAccessoriesButton.SetSize(200, 40);
      this.m_owidgetAccessoriesButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetAccessoriesButton.Init(host, "extendedcontrols3", 0.0f, 384f, 95f, 479f, 128f, 384f, 223f, 479f, 256f, 384f, 351f, 479f, 384f, 384f, 479f, 479f);
      this.m_owidgetAccessoriesButton.SetGrowableWidth(2, 2, 160);
      this.m_owidgetAccessoriesButton.ImageAreaWidth = 48;
      this.m_owidgetAccessoriesButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_ACCESSORIES");
      int x3 = x2 + this.m_owidgetAccessoriesButton.Width;
      this.m_owidgetOpenmodelButton = new ButtonWidget(0);
      this.m_owidgetOpenmodelButton.Text = "T_OPENFILE";
      this.m_owidgetOpenmodelButton.Size = FontSize.Large;
      this.m_owidgetOpenmodelButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetOpenmodelButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetOpenmodelButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetOpenmodelButton.Alignment = QFontAlignment.Left;
      this.m_owidgetOpenmodelButton.SetPosition(x3, 0);
      this.m_owidgetOpenmodelButton.SetSize(200, 50);
      this.m_owidgetOpenmodelButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetOpenmodelButton.Init(host, "guicontrols", 352f, 8f, 511f, 51f, 352f, 72f, 511f, 115f, 352f, 136f, 511f, 179f);
      this.m_owidgetOpenmodelButton.SetGrowableWidth(2, 2, 160);
      this.m_owidgetOpenmodelButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_OPENFILE");
      int num = x3 + this.m_owidgetOpenmodelButton.Width;
      this.m_owidgetHelpButton = new ButtonWidget(11);
      this.m_owidgetHelpButton.Text = "";
      this.m_owidgetHelpButton.Size = FontSize.Large;
      this.m_owidgetHelpButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetHelpButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetHelpButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetHelpButton.Alignment = QFontAlignment.Left;
      this.m_owidgetHelpButton.SetSize(52, 44);
      this.m_owidgetHelpButton.SetPosition(-this.m_owidgetHelpButton.Width, 3);
      this.m_owidgetHelpButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetHelpButton.Init(host, "extendedcontrols", 852f, 196f, 909f, 245f, 852f, 260f, 909f, 309f, 852f, 324f, 909f, 373f);
      this.m_owidgetHelpButton.SetGrowableWidth(2, 2, 160);
      this.m_owidgetHelpButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_HELP");
      this.m_owidgetSettingsButton = new ButtonWidget(1);
      this.m_owidgetSettingsButton.Text = "";
      this.m_owidgetSettingsButton.Size = FontSize.Large;
      this.m_owidgetSettingsButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetSettingsButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetSettingsButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetSettingsButton.Alignment = QFontAlignment.Left;
      this.m_owidgetSettingsButton.SetSize(58, 50);
      this.m_owidgetSettingsButton.SetPosition(-(this.m_owidgetSettingsButton.Width + this.m_owidgetHelpButton.Width), 0);
      this.m_owidgetSettingsButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetSettingsButton.Init(host, "guicontrols", 192f, 10f, 240f, 53f, 192f, 74f, 240f, 117f, 192f, 138f, 240f, 181f);
      this.m_owidgetSettingsButton.SetGrowableWidth(2, 2, 160);
      this.m_owidgetSettingsButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_SETTINGS");
      this.m_owidgetPrinterButton = new ButtonWidget(12);
      this.m_owidgetPrinterButton.Text = "NV-00-00-00-00-000-000";
      this.m_owidgetPrinterButton.Size = FontSize.Large;
      this.m_owidgetPrinterButton.TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.m_owidgetPrinterButton.TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f);
      this.m_owidgetPrinterButton.TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f);
      this.m_owidgetPrinterButton.Alignment = QFontAlignment.Justify;
      this.m_owidgetPrinterButton.SetSize(250, 33);
      this.m_owidgetPrinterButton.ImageAreaWidth = 55;
      this.m_owidgetPrinterButton.SetPosition(-(this.m_owidgetSettingsButton.Width + this.m_owidgetHelpButton.Width + this.m_owidgetPrinterButton.Width), 9);
      this.m_owidgetPrinterButton.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.m_owidgetPrinterButton.Init(host, "guicontrols", 448f, 650f, 525f, 693f, 608f, 650f, 685f, 693f, 768f, 650f, 845f, 693f);
      this.m_owidgetPrinterButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_MULTIPRINTER");
      this.AddChildElement((Element2D) this.m_owidgetPrinterButton);
      this.AddChildElement((Element2D) this.m_owidgetFilamentButton);
      this.AddChildElement((Element2D) this.m_owidgetAccessoriesButton);
      this.AddChildElement((Element2D) this.m_owidgetOpenmodelButton);
      this.AddChildElement((Element2D) this.m_owidgetSettingsButton);
      this.AddChildElement((Element2D) this.m_owidgetHelpButton);
      this.loading_frame = new Frame(10);
      this.loading_frame.SetSize(160, 200);
      this.loading_frame.CenterHorizontallyInParent = true;
      this.loading_frame.CenterVerticallyInParent = true;
      this.loading_frame.Visible = false;
      this.loading_progress = new SpriteAnimationWidget(10);
      this.loading_progress.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      this.loading_progress.SetSize(160, 135);
      this.loading_progress.SetPosition(0, 0);
      this.loading_progress.Visible = true;
      this.loading_text = new TextWidget(0);
      this.loading_text.Size = FontSize.VeryLarge;
      this.loading_text.SetPosition(0, 135);
      this.loading_text.SetSize(160, 65);
      this.loading_text.VAlignment = TextVerticalAlignment.Middle;
      this.loading_text.Alignment = QFontAlignment.Centre;
      this.loading_text.Text = "Loading....";
      this.loading_text.Color = new Color4(byte.MaxValue, (byte) 70, (byte) 0, byte.MaxValue);
      this.loading_frame.AddChildElement((Element2D) this.loading_text);
      this.loading_frame.AddChildElement((Element2D) this.loading_progress);
      this.driversInstalling_frame = new XMLFrame();
      this.driversInstalling_frame.Init(host, Resources.driverInstallDetected, (ButtonCallback) null);
      this.driversInstalling_frame.SetPosition(50, -50);
      this.driversInstalling_frame.SetSize(330, 48);
      this.driversInstalling_frame.Visible = false;
      this.m_GUIHost.AddControlElement((Element2D) this.driversInstalling_frame);
      this.diagnostics = new TextWidget(7);
      this.diagnostics.Text = "";
      this.diagnostics.Size = FontSize.Medium;
      this.diagnostics.Color = new Color4(0.4f, 0.4f, 0.4f, 1f);
      this.diagnostics.SetPosition(-530, 0);
      this.diagnostics.Alignment = QFontAlignment.Left;
      this.diagnostics.SetSize(150, 50);
      this.diagnostics.Visible = true;
      this.AddChildElement((Element2D) this.diagnostics);
      this.m_GUIHost.AddControlElement((Element2D) this);
      this.m_GUIHost.AddControlElement((Element2D) this.loading_frame);
      this.CreatePopUpDialogs(host, messagebox, infobox, spooler_connection, softwareUpdater);
      this.message_window = infobox;
      Sprite.pixel_perfect = true;
      this.information = new ButtonWidget(5);
      this.information.Text = "";
      this.information.Size = FontSize.Medium;
      this.information.SetPosition(4, -36);
      this.information.SetSize(32, 32);
      this.information.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.information.Init(host, "guicontrols", 448f, 512f, 511f, 575f, 512f, 512f, 575f, 575f, 576f, 512f, 639f, 575f);
      this.information.ToolTipMessage = host.Locale.T("T_TOOLTIP_INFORMATION");
      this.m_GUIHost.AddControlElement((Element2D) this.information);
      Sprite.pixel_perfect = false;
      this.DisableAccessories();
    }

    private void CreatePopUpDialogs(GUIHost host, PopupMessageBox messagebox, MessagePopUp infobox, SpoolerConnection spooler_connection, Updater softwareUpdater)
    {
      this.PopUpDialogList = new List<Frame>();
      this.m_oManageFilamentDialog = new Manage3DInkMainWindow(8, host, this.m_oSettingsManager, messagebox, infobox, spooler_connection);
      this.m_oSettingsDialog = new SettingsDialogWidget(1, host, this.m_oSettingsManager, messagebox, spooler_connection, softwareUpdater);
      this.m_oMultiPrinterDialog = new MultiPrinterDialogWidget(12, host, this.m_oSettingsManager, messagebox, spooler_connection);
      this.m_oAccessoriesDialog = new MainWindow(13, host, this.m_oSettingsManager, messagebox, spooler_connection);
      this.m_oManageFilamentDialog.MinWidth = 750;
      this.m_oManageFilamentDialog.MinHeight = 480;
      this.m_oManageFilamentDialog.RelativeWidth = 0.5f;
      this.m_oManageFilamentDialog.RelativeHeight = 0.5f;
      this.m_oManageFilamentDialog.AutoCenterYOffset = 50;
      this.m_oManageFilamentDialog.CenterHorizontallyInParent = true;
      this.m_oManageFilamentDialog.CenterVerticallyInParent = true;
      this.m_oSettingsDialog.MinWidth = 850;
      this.m_oSettingsDialog.MinHeight = 480;
      this.m_oSettingsDialog.RelativeWidth = 0.5f;
      this.m_oSettingsDialog.RelativeHeight = 0.5f;
      this.m_oSettingsDialog.AutoCenterYOffset = 50;
      this.m_oSettingsDialog.CenterHorizontallyInParent = true;
      this.m_oSettingsDialog.CenterVerticallyInParent = true;
      this.m_oAccessoriesDialog.MinWidth = 850;
      this.m_oAccessoriesDialog.MinHeight = 480;
      this.m_oAccessoriesDialog.RelativeWidth = 0.5f;
      this.m_oAccessoriesDialog.RelativeHeight = 0.5f;
      this.m_oAccessoriesDialog.AutoCenterYOffset = 50;
      this.m_oAccessoriesDialog.CenterHorizontallyInParent = true;
      this.m_oAccessoriesDialog.CenterVerticallyInParent = true;
      this.m_oMultiPrinterDialog.MinWidth = 750;
      this.m_oMultiPrinterDialog.MinHeight = 440;
      this.m_oMultiPrinterDialog.RelativeWidth = 0.45f;
      this.m_oMultiPrinterDialog.RelativeHeight = 0.45f;
      this.m_oMultiPrinterDialog.AutoCenterYOffset = 50;
      this.m_oMultiPrinterDialog.CenterHorizontallyInParent = true;
      this.m_oMultiPrinterDialog.CenterVerticallyInParent = true;
      this.PopUpDialogList.Insert(0, (Frame) this.m_oSettingsDialog);
      this.PopUpDialogList.Insert(1, (Frame) this.m_oManageFilamentDialog);
      this.PopUpDialogList.Insert(2, (Frame) this.m_oMultiPrinterDialog);
      this.PopUpDialogList.Insert(3, (Frame) this.m_oAccessoriesDialog);
    }

    public override void OnUpdate()
    {
      if (this.m_oSettingsManager != null)
      {
        if (this.m_oModelLoadingManager.LoadingNewModel)
        {
          this.loading_frame.Visible = true;
          if (this.m_oModelLoadingManager.OptimizingModel)
          {
            this.loading_text.Text = "Optimizing...";
            this.loading_progress.Visible = false;
          }
          else
          {
            this.loading_text.Text = "Loading...";
            this.loading_progress.Visible = true;
          }
        }
        else
          this.loading_frame.Visible = false;
      }
      if (this.m_oSpoolerConnection != null)
      {
        PrinterObject selectedPrinter = this.m_oSpoolerConnection.SelectedPrinter;
        if (selectedPrinter != null && this.m_owidgetPrinterButton != null)
          this.m_owidgetPrinterButton.Text = selectedPrinter.Info.serial_number.ToString();
        else
          this.m_owidgetPrinterButton.Text = "Not Connected";
        this.driversInstalling_frame.Visible = this.m_oSpoolerConnection.PrintSpoolerClient.PrinterDriverInstallCount > 0;
      }
      base.OnUpdate();
    }

    public void OpenDialog(ControlBar.PopUpDialogTypes whichone)
    {
      Frame popUpDialog = this.PopUpDialogList[(int) whichone];
      if (this.m_GUIHost.HasChildDialog && whichone != ControlBar.PopUpDialogTypes.MultiPrinter || popUpDialog.Visible)
        return;
      Element2D element = this.m_GUIHost.GlobalChildDialog.Last();
      if (element != null && !this.IsOneOFOurs(element) && element.ID != 11001)
        return;
      this.m_GUIHost.GlobalChildDialog += (Element2D) popUpDialog;
      popUpDialog.Visible = true;
    }

    public void CloseDialog(ControlBar.PopUpDialogTypes whichone)
    {
      Frame popUpDialog = this.PopUpDialogList[(int) whichone];
      if (!this.m_GUIHost.HasChildDialog || !popUpDialog.Visible)
        return;
      popUpDialog.Visible = false;
      this.m_GUIHost.GlobalChildDialog -= (Element2D) popUpDialog;
    }

    private bool IsOneOFOurs(Element2D element)
    {
      foreach (Frame popUpDialog in this.PopUpDialogList)
      {
        if (popUpDialog == element)
          return true;
      }
      return false;
    }

    private void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      this.CheckAccessoriesAvailability(this.m_oSpoolerConnection.GetPrinterBySerialNumber(serial_number.ToString()));
    }

    private void CheckAccessoriesAvailability(PrinterObject printer)
    {
      bool flag = false;
      if (printer != null && printer.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
        flag = true;
      if (flag)
        this.EnableAccessories();
      else
        this.DisableAccessories();
    }

    private void EnableAccessories()
    {
      this.m_owidgetAccessoriesButton.Visible = true;
      this.m_owidgetAccessoriesButton.Enabled = true;
      this.m_owidgetOpenmodelButton.X = this.m_owidgetAccessoriesButton.X + this.m_owidgetAccessoriesButton.Width;
    }

    private void DisableAccessories()
    {
      this.m_owidgetAccessoriesButton.Visible = false;
      this.m_owidgetAccessoriesButton.Enabled = false;
      this.m_owidgetOpenmodelButton.X = this.m_owidgetAccessoriesButton.X;
      if (!this.m_oAccessoriesDialog.Visible)
        return;
      this.m_oAccessoriesDialog.Close();
    }

    public void OpenSettingsDialogCalibrationPage()
    {
      this.CloseDialog(ControlBar.PopUpDialogTypes.Filament);
      this.CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
      this.OpenDialog(ControlBar.PopUpDialogTypes.Settings);
      this.m_oSettingsDialog.ActivateAdvancedView(true);
    }

    public void OpenSettingsDialogFilamentManagement()
    {
      this.OpenDialog(ControlBar.PopUpDialogTypes.Filament);
    }

    public void OpenSettingsDialogAdvancedCalibrate(string printer_serial_number)
    {
      this.CloseDialog(ControlBar.PopUpDialogTypes.Filament);
      this.CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
      this.OpenDialog(ControlBar.PopUpDialogTypes.Settings);
      this.m_oSettingsDialog.ActivateAdvancedView(true);
    }

    public void OpenSettingsDialogChangeFilament()
    {
      this.OpenDialog(ControlBar.PopUpDialogTypes.Filament);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          this.OpenModelClicked();
          break;
        case 1:
          if (!this.m_oSettingsDialog.Visible)
          {
            this.OpenDialog(ControlBar.PopUpDialogTypes.Settings);
            break;
          }
          this.CloseDialog(ControlBar.PopUpDialogTypes.Settings);
          break;
        case 5:
          this.message_window.Reshow();
          break;
        case 8:
          if (!this.m_oManageFilamentDialog.Visible)
          {
            this.OpenDialog(ControlBar.PopUpDialogTypes.Filament);
            break;
          }
          this.CloseDialog(ControlBar.PopUpDialogTypes.Filament);
          break;
        case 11:
          this.OpenM3DSupportWebsite();
          break;
        case 12:
          if (!this.m_oMultiPrinterDialog.Visible)
          {
            this.OpenDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
            break;
          }
          this.CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
          break;
        case 13:
          if (!this.m_oAccessoriesDialog.Visible)
          {
            this.OpenDialog(ControlBar.PopUpDialogTypes.Accessories);
            break;
          }
          this.CloseDialog(ControlBar.PopUpDialogTypes.Accessories);
          break;
      }
    }

    public override ElementType GetElementType()
    {
      return ElementType.ControlBar;
    }

    public override void OnParentMove()
    {
      base.OnParentMove();
    }

    public void OpenModelClicked()
    {
      string filename = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.Models);
      if (filename == null)
        return;
      this.m_oModelLoadingManager.LoadModelIntoPrinter(filename);
    }

    public void SaveSettings()
    {
      if (this.m_oSettingsDialog == null)
        return;
      this.m_oSettingsDialog.SaveSettings();
    }

    public void UpdateSettings()
    {
      this.m_oSettingsDialog.UpdateSettings();
    }

    private void OpenM3DSupportWebsite()
    {
      Process.Start("https://printm3d.com/support");
    }

    public enum PopUpDialogTypes
    {
      Settings,
      Filament,
      MultiPrinter,
      Accessories,
    }
  }
}
