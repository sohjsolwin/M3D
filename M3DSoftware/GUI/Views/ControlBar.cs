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
      m_oSpoolerConnection = spooler_connection;
      m_oSettingsManager = settingsManager;
      m_oModelLoadingManager = model_loading_manager;
      m_GUIHost = host;
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(OnSelectedPrinterChanged);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      X = 0;
      Y = 0;
      RelativeWidth = 1f;
      Height = 50;
      BGColor = new Color4(0.913725f, 0.905882f, 0.9098f, 1f);
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      var x1 = 10;
      m_owidgetFilamentButton = new ButtonWidget(8)
      {
        Text = "T_3DINK",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Left
      };
      m_owidgetFilamentButton.SetSize(170, 50);
      m_owidgetFilamentButton.SetPosition(x1, 0);
      m_owidgetFilamentButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetFilamentButton.Init(host, "guicontrols", 0.0f, 6f, 157f, 57f, 0.0f, 70f, 157f, 121f, 0.0f, 134f, 157f, 185f);
      m_owidgetFilamentButton.SetGrowableWidth(2, 2, 160);
      m_owidgetFilamentButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_3DINK");
      var x2 = x1 + m_owidgetFilamentButton.Width;
      m_owidgetAccessoriesButton = new ButtonWidget(13)
      {
        Text = "T_ACCESSORIES",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Justify
      };
      m_owidgetAccessoriesButton.SetPosition(x2, 8);
      m_owidgetAccessoriesButton.SetSize(200, 40);
      m_owidgetAccessoriesButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetAccessoriesButton.Init(host, "extendedcontrols3", 0.0f, 384f, 95f, 479f, 128f, 384f, 223f, 479f, 256f, 384f, 351f, 479f, 384f, 384f, 479f, 479f);
      m_owidgetAccessoriesButton.SetGrowableWidth(2, 2, 160);
      m_owidgetAccessoriesButton.ImageAreaWidth = 48;
      m_owidgetAccessoriesButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_ACCESSORIES");
      var x3 = x2 + m_owidgetAccessoriesButton.Width;
      m_owidgetOpenmodelButton = new ButtonWidget(0)
      {
        Text = "T_OPENFILE",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Left
      };
      m_owidgetOpenmodelButton.SetPosition(x3, 0);
      m_owidgetOpenmodelButton.SetSize(200, 50);
      m_owidgetOpenmodelButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetOpenmodelButton.Init(host, "guicontrols", 352f, 8f, 511f, 51f, 352f, 72f, 511f, 115f, 352f, 136f, 511f, 179f);
      m_owidgetOpenmodelButton.SetGrowableWidth(2, 2, 160);
      m_owidgetOpenmodelButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_OPENFILE");
      var num = x3 + m_owidgetOpenmodelButton.Width;
      m_owidgetHelpButton = new ButtonWidget(11)
      {
        Text = "",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Left
      };
      m_owidgetHelpButton.SetSize(52, 44);
      m_owidgetHelpButton.SetPosition(-m_owidgetHelpButton.Width, 3);
      m_owidgetHelpButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetHelpButton.Init(host, "extendedcontrols", 852f, 196f, 909f, 245f, 852f, 260f, 909f, 309f, 852f, 324f, 909f, 373f);
      m_owidgetHelpButton.SetGrowableWidth(2, 2, 160);
      m_owidgetHelpButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_HELP");
      m_owidgetSettingsButton = new ButtonWidget(1)
      {
        Text = "",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Left
      };
      m_owidgetSettingsButton.SetSize(58, 50);
      m_owidgetSettingsButton.SetPosition(-(m_owidgetSettingsButton.Width + m_owidgetHelpButton.Width), 0);
      m_owidgetSettingsButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetSettingsButton.Init(host, "guicontrols", 192f, 10f, 240f, 53f, 192f, 74f, 240f, 117f, 192f, 138f, 240f, 181f);
      m_owidgetSettingsButton.SetGrowableWidth(2, 2, 160);
      m_owidgetSettingsButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_SETTINGS");
      m_owidgetPrinterButton = new ButtonWidget(12)
      {
        Text = "NV-00-00-00-00-000-000",
        Size = FontSize.Large,
        TextColor = new Color4(0.5f, 0.5f, 0.5f, 1f),
        TextOverColor = new Color4(0.4392157f, 0.8392157f, 0.9372549f, 1f),
        TextDownColor = new Color4(0.2f, 0.6078432f, 0.7098039f, 1f),
        Alignment = QFontAlignment.Justify
      };
      m_owidgetPrinterButton.SetSize(250, 33);
      m_owidgetPrinterButton.ImageAreaWidth = 55;
      m_owidgetPrinterButton.SetPosition(-(m_owidgetSettingsButton.Width + m_owidgetHelpButton.Width + m_owidgetPrinterButton.Width), 9);
      m_owidgetPrinterButton.SetCallback(new ButtonCallback(MyButtonCallback));
      m_owidgetPrinterButton.Init(host, "guicontrols", 448f, 650f, 525f, 693f, 608f, 650f, 685f, 693f, 768f, 650f, 845f, 693f);
      m_owidgetPrinterButton.ToolTipMessage = host.Locale.T("T_TOOLTIP_MULTIPRINTER");
      AddChildElement((Element2D)m_owidgetPrinterButton);
      AddChildElement((Element2D)m_owidgetFilamentButton);
      AddChildElement((Element2D)m_owidgetAccessoriesButton);
      AddChildElement((Element2D)m_owidgetOpenmodelButton);
      AddChildElement((Element2D)m_owidgetSettingsButton);
      AddChildElement((Element2D)m_owidgetHelpButton);
      loading_frame = new Frame(10);
      loading_frame.SetSize(160, 200);
      loading_frame.CenterHorizontallyInParent = true;
      loading_frame.CenterVerticallyInParent = true;
      loading_frame.Visible = false;
      loading_progress = new SpriteAnimationWidget(10);
      loading_progress.Init(host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      loading_progress.SetSize(160, 135);
      loading_progress.SetPosition(0, 0);
      loading_progress.Visible = true;
      loading_text = new TextWidget(0)
      {
        Size = FontSize.VeryLarge
      };
      loading_text.SetPosition(0, 135);
      loading_text.SetSize(160, 65);
      loading_text.VAlignment = TextVerticalAlignment.Middle;
      loading_text.Alignment = QFontAlignment.Centre;
      loading_text.Text = "Loading....";
      loading_text.Color = new Color4(byte.MaxValue, (byte) 70, (byte) 0, byte.MaxValue);
      loading_frame.AddChildElement((Element2D)loading_text);
      loading_frame.AddChildElement((Element2D)loading_progress);
      driversInstalling_frame = new XMLFrame();
      driversInstalling_frame.Init(host, Resources.driverInstallDetected, (ButtonCallback) null);
      driversInstalling_frame.SetPosition(50, -50);
      driversInstalling_frame.SetSize(330, 48);
      driversInstalling_frame.Visible = false;
      m_GUIHost.AddControlElement((Element2D)driversInstalling_frame);
      diagnostics = new TextWidget(7)
      {
        Text = "",
        Size = FontSize.Medium,
        Color = new Color4(0.4f, 0.4f, 0.4f, 1f)
      };
      diagnostics.SetPosition(-530, 0);
      diagnostics.Alignment = QFontAlignment.Left;
      diagnostics.SetSize(150, 50);
      diagnostics.Visible = true;
      AddChildElement((Element2D)diagnostics);
      m_GUIHost.AddControlElement((Element2D) this);
      m_GUIHost.AddControlElement((Element2D)loading_frame);
      CreatePopUpDialogs(host, messagebox, infobox, spooler_connection, softwareUpdater);
      message_window = infobox;
      Sprite.pixel_perfect = true;
      information = new ButtonWidget(5)
      {
        Text = "",
        Size = FontSize.Medium
      };
      information.SetPosition(4, -36);
      information.SetSize(32, 32);
      information.SetCallback(new ButtonCallback(MyButtonCallback));
      information.Init(host, "guicontrols", 448f, 512f, 511f, 575f, 512f, 512f, 575f, 575f, 576f, 512f, 639f, 575f);
      information.ToolTipMessage = host.Locale.T("T_TOOLTIP_INFORMATION");
      m_GUIHost.AddControlElement((Element2D)information);
      Sprite.pixel_perfect = false;
      DisableAccessories();
    }

    private void CreatePopUpDialogs(GUIHost host, PopupMessageBox messagebox, MessagePopUp infobox, SpoolerConnection spooler_connection, Updater softwareUpdater)
    {
      PopUpDialogList = new List<Frame>();
      m_oManageFilamentDialog = new Manage3DInkMainWindow(8, host, m_oSettingsManager, messagebox, infobox, spooler_connection);
      m_oSettingsDialog = new SettingsDialogWidget(1, host, m_oSettingsManager, messagebox, spooler_connection, softwareUpdater);
      m_oMultiPrinterDialog = new MultiPrinterDialogWidget(12, host, m_oSettingsManager, messagebox, spooler_connection);
      m_oAccessoriesDialog = new MainWindow(13, host, m_oSettingsManager, messagebox, spooler_connection);
      m_oManageFilamentDialog.MinWidth = 750;
      m_oManageFilamentDialog.MinHeight = 480;
      m_oManageFilamentDialog.RelativeWidth = 0.5f;
      m_oManageFilamentDialog.RelativeHeight = 0.5f;
      m_oManageFilamentDialog.AutoCenterYOffset = 50;
      m_oManageFilamentDialog.CenterHorizontallyInParent = true;
      m_oManageFilamentDialog.CenterVerticallyInParent = true;
      m_oSettingsDialog.MinWidth = 850;
      m_oSettingsDialog.MinHeight = 480;
      m_oSettingsDialog.RelativeWidth = 0.5f;
      m_oSettingsDialog.RelativeHeight = 0.5f;
      m_oSettingsDialog.AutoCenterYOffset = 50;
      m_oSettingsDialog.CenterHorizontallyInParent = true;
      m_oSettingsDialog.CenterVerticallyInParent = true;
      m_oAccessoriesDialog.MinWidth = 850;
      m_oAccessoriesDialog.MinHeight = 480;
      m_oAccessoriesDialog.RelativeWidth = 0.5f;
      m_oAccessoriesDialog.RelativeHeight = 0.5f;
      m_oAccessoriesDialog.AutoCenterYOffset = 50;
      m_oAccessoriesDialog.CenterHorizontallyInParent = true;
      m_oAccessoriesDialog.CenterVerticallyInParent = true;
      m_oMultiPrinterDialog.MinWidth = 750;
      m_oMultiPrinterDialog.MinHeight = 440;
      m_oMultiPrinterDialog.RelativeWidth = 0.45f;
      m_oMultiPrinterDialog.RelativeHeight = 0.45f;
      m_oMultiPrinterDialog.AutoCenterYOffset = 50;
      m_oMultiPrinterDialog.CenterHorizontallyInParent = true;
      m_oMultiPrinterDialog.CenterVerticallyInParent = true;
      PopUpDialogList.Insert(0, (Frame)m_oSettingsDialog);
      PopUpDialogList.Insert(1, (Frame)m_oManageFilamentDialog);
      PopUpDialogList.Insert(2, (Frame)m_oMultiPrinterDialog);
      PopUpDialogList.Insert(3, (Frame)m_oAccessoriesDialog);
    }

    public override void OnUpdate()
    {
      if (m_oSettingsManager != null)
      {
        if (m_oModelLoadingManager.LoadingNewModel)
        {
          loading_frame.Visible = true;
          if (m_oModelLoadingManager.OptimizingModel)
          {
            loading_text.Text = "Optimizing...";
            loading_progress.Visible = false;
          }
          else
          {
            loading_text.Text = "Loading...";
            loading_progress.Visible = true;
          }
        }
        else
        {
          loading_frame.Visible = false;
        }
      }
      if (m_oSpoolerConnection != null)
      {
        PrinterObject selectedPrinter = m_oSpoolerConnection.SelectedPrinter;
        if (selectedPrinter != null && m_owidgetPrinterButton != null)
        {
          m_owidgetPrinterButton.Text = selectedPrinter.Info.serial_number.ToString();
        }
        else
        {
          m_owidgetPrinterButton.Text = "Not Connected";
        }

        driversInstalling_frame.Visible = m_oSpoolerConnection.PrintSpoolerClient.PrinterDriverInstallCount > 0;
      }
      base.OnUpdate();
    }

    public void OpenDialog(ControlBar.PopUpDialogTypes whichone)
    {
      Frame popUpDialog = PopUpDialogList[(int) whichone];
      if (m_GUIHost.HasChildDialog && whichone != ControlBar.PopUpDialogTypes.MultiPrinter || popUpDialog.Visible)
      {
        return;
      }

      Element2D element = m_GUIHost.GlobalChildDialog.Last();
      if (element != null && !IsOneOFOurs(element) && element.ID != 11001)
      {
        return;
      }

      m_GUIHost.GlobalChildDialog += (Element2D) popUpDialog;
      popUpDialog.Visible = true;
    }

    public void CloseDialog(ControlBar.PopUpDialogTypes whichone)
    {
      Frame popUpDialog = PopUpDialogList[(int) whichone];
      if (!m_GUIHost.HasChildDialog || !popUpDialog.Visible)
      {
        return;
      }

      popUpDialog.Visible = false;
      m_GUIHost.GlobalChildDialog -= (Element2D) popUpDialog;
    }

    private bool IsOneOFOurs(Element2D element)
    {
      foreach (Frame popUpDialog in PopUpDialogList)
      {
        if (popUpDialog == element)
        {
          return true;
        }
      }
      return false;
    }

    private void OnSelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      CheckAccessoriesAvailability(m_oSpoolerConnection.GetPrinterBySerialNumber(serial_number.ToString()));
    }

    private void CheckAccessoriesAvailability(PrinterObject printer)
    {
      var flag = false;
      if (printer != null && printer.MyPrinterProfile.AccessoriesConstants.NozzleConstants.bHasInterchangeableNozzle)
      {
        flag = true;
      }

      if (flag)
      {
        EnableAccessories();
      }
      else
      {
        DisableAccessories();
      }
    }

    private void EnableAccessories()
    {
      m_owidgetAccessoriesButton.Visible = true;
      m_owidgetAccessoriesButton.Enabled = true;
      m_owidgetOpenmodelButton.X = m_owidgetAccessoriesButton.X + m_owidgetAccessoriesButton.Width;
    }

    private void DisableAccessories()
    {
      m_owidgetAccessoriesButton.Visible = false;
      m_owidgetAccessoriesButton.Enabled = false;
      m_owidgetOpenmodelButton.X = m_owidgetAccessoriesButton.X;
      if (!m_oAccessoriesDialog.Visible)
      {
        return;
      }

      m_oAccessoriesDialog.Close();
    }

    public void OpenSettingsDialogCalibrationPage()
    {
      CloseDialog(ControlBar.PopUpDialogTypes.Filament);
      CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
      OpenDialog(ControlBar.PopUpDialogTypes.Settings);
      m_oSettingsDialog.ActivateAdvancedView(true);
    }

    public void OpenSettingsDialogFilamentManagement()
    {
      OpenDialog(ControlBar.PopUpDialogTypes.Filament);
    }

    public void OpenSettingsDialogAdvancedCalibrate(string printer_serial_number)
    {
      CloseDialog(ControlBar.PopUpDialogTypes.Filament);
      CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
      OpenDialog(ControlBar.PopUpDialogTypes.Settings);
      m_oSettingsDialog.ActivateAdvancedView(true);
    }

    public void OpenSettingsDialogChangeFilament()
    {
      OpenDialog(ControlBar.PopUpDialogTypes.Filament);
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          OpenModelClicked();
          break;
        case 1:
          if (!m_oSettingsDialog.Visible)
          {
            OpenDialog(ControlBar.PopUpDialogTypes.Settings);
            break;
          }
          CloseDialog(ControlBar.PopUpDialogTypes.Settings);
          break;
        case 5:
          message_window.Reshow();
          break;
        case 8:
          if (!m_oManageFilamentDialog.Visible)
          {
            OpenDialog(ControlBar.PopUpDialogTypes.Filament);
            break;
          }
          CloseDialog(ControlBar.PopUpDialogTypes.Filament);
          break;
        case 11:
          OpenM3DSupportWebsite();
          break;
        case 12:
          if (!m_oMultiPrinterDialog.Visible)
          {
            OpenDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
            break;
          }
          CloseDialog(ControlBar.PopUpDialogTypes.MultiPrinter);
          break;
        case 13:
          if (!m_oAccessoriesDialog.Visible)
          {
            OpenDialog(ControlBar.PopUpDialogTypes.Accessories);
            break;
          }
          CloseDialog(ControlBar.PopUpDialogTypes.Accessories);
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
      var filename = OpenModelFileDialog.RunOpenModelDialog(OpenModelFileDialog.FileType.Models);
      if (filename == null)
      {
        return;
      }

      m_oModelLoadingManager.LoadModelIntoPrinter(filename);
    }

    public void SaveSettings()
    {
      if (m_oSettingsDialog == null)
      {
        return;
      }

      m_oSettingsDialog.SaveSettings();
    }

    public void UpdateSettings()
    {
      m_oSettingsDialog.UpdateSettings();
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
