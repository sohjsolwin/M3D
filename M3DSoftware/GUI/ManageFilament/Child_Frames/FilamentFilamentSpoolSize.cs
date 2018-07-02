using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Common;
using System;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentFilamentSpoolSize : Manage3DInkChildWindow
  {
    private TextWidget text_main;
    private TextWidget text_title;
    private ButtonWidget pro_filament_button;
    private ButtonWidget micro_filament_button;
    private ButtonWidget cancel_button;
    private PopupMessageBox messagebox;
    private SettingsManager settingsManager;

    public FilamentFilamentSpoolSize(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.messagebox = messagebox;
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
      {
        return;
      }

      selectedPrinter.MarkedAsBusy = true;
      var flag = false;
      switch (button.ID)
      {
        case 9:
          MainWindow.ResetToStartup();
          break;
        case 11:
          CurrentDetails.current_spool.filament_size = FilamentSpool.SizeEnum.Pro;
          flag = true;
          break;
        case 12:
          CurrentDetails.current_spool.filament_size = FilamentSpool.SizeEnum.Micro;
          flag = true;
          break;
        default:
          throw new NotImplementedException();
      }
      if (!flag)
      {
        return;
      }

      if (settingsManager.CurrentFilamentSettings.TrackFilament)
      {
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page19_FilamentIsNewSpoolPage, CurrentDetails);
      }
      else
      {
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page13_FilamentLocation, CurrentDetails);
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Size of Filament Spool Currently in use:", "", true, false, false, false, false, false);
      var childElement = (Frame)FindChildElement(2);
      if (childElement != null)
      {
        var buttonWidget1 = new ButtonWidget(11);
        buttonWidget1.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget1.Size = FontSize.Medium;
        buttonWidget1.Text = "PRO SPOOL";
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetSize(192, 60);
        buttonWidget1.SetPosition(60, -100);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement(buttonWidget1);
        var buttonWidget2 = new ButtonWidget(12);
        buttonWidget2.Init(Host, "guicontrols", 896f, 192f, 959f, byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "MICRO SPOOL";
        buttonWidget2.SetGrowableWidth(4, 4, 32);
        buttonWidget2.SetGrowableHeight(4, 4, 32);
        buttonWidget2.SetSize(192, 60);
        buttonWidget2.SetPosition(-252, -100);
        buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement(buttonWidget2);
      }
      PopulateStartupControlsList();
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      try
      {
        if (MainWindow.GetSelectedPrinter() == null)
        {
          return;
        }

        OnUpdate();
        DisableAllControls();
        text_main.Visible = true;
        pro_filament_button.Visible = true;
        pro_filament_button.Enabled = true;
        micro_filament_button.Visible = true;
        micro_filament_button.Enabled = true;
        cancel_button.Visible = true;
        cancel_button.Enabled = true;
        text_title.Text = "Currently inserting:\n\n" + FilamentProfile.GenerateSpoolName(CurrentDetails.current_spool, false);
        text_main.Text = "Please select filament spool size for insertion:";
      }
      catch (Exception ex)
      {
      }
      OnUpdate();
    }

    private void PopulateStartupControlsList()
    {
      pro_filament_button = (ButtonWidget)FindChildElement(11);
      micro_filament_button = (ButtonWidget)FindChildElement(12);
      cancel_button = (ButtonWidget)FindChildElement(9);
      text_main = (TextWidget)FindChildElement(3);
      text_title = (TextWidget)FindChildElement(1);
    }

    public void DisableAllControls()
    {
      text_main.Visible = false;
      pro_filament_button.Visible = false;
      pro_filament_button.Enabled = false;
      micro_filament_button.Visible = false;
      micro_filament_button.Enabled = false;
      cancel_button.Visible = false;
      cancel_button.Enabled = false;
    }

    public enum ControlIDs
    {
      ProFilamentSpoolSize = 11, // 0x0000000B
      MicroFilamentSpoolSize = 12, // 0x0000000C
    }
  }
}
