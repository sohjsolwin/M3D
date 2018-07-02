using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentFilamentLocation : Manage3DInkChildWindow
  {
    private TextWidget text_main;
    private TextWidget text_title;
    private ButtonWidget add_button_external;
    private ButtonWidget add_button_internal;
    private ButtonWidget cancel_button;
    private bool insertwarn_sent;
    private PopupMessageBox messagebox;
    private SettingsManager settingsManager;

    public FilamentFilamentLocation(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
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
      switch (button.ID)
      {
        case 9:
          MainWindow.ResetToStartup();
          break;
        case 11:
        case 12:
          if (button.ID == 11)
          {
            CurrentDetails.current_spool.filament_location = FilamentSpool.Location.Internal;
          }
          else
          {
            CurrentDetails.current_spool.filament_location = FilamentSpool.Location.External;
          }

          SetFilamentAndStartNextProcess();
          break;
        default:
          throw new NotImplementedException();
      }
    }

    private void SetFilamentAndStartNextProcess()
    {
      PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
      if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew)
      {
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page12_RaisingExtruder, CurrentDetails);
      }
      else if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted)
      {
        CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetDetails;
        CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, CurrentDetails);
        if (settingsManager != null)
        {
          settingsManager.AssociateFilamentToPrinter(selectedPrinter.Info.serial_number, CurrentDetails.current_spool);
          settingsManager.SaveSettings();
        }
        var num = (int) selectedPrinter.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, CurrentDetails.current_spool);
      }
      else
      {
        selectedPrinter.MarkedAsBusy = false;
        MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Filament Currently in use:", "", true, false, false, false, false, false);
      var childElement = (Frame)FindChildElement(2);
      if (childElement != null)
      {
        var buttonWidget1 = new ButtonWidget(11);
        buttonWidget1.Init(Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget1.Size = FontSize.Medium;
        buttonWidget1.Text = "INSERT FILAMENT (INTERNAL)";
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetSize(192, 60);
        buttonWidget1.SetPosition(60, -100);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget1);
        var buttonWidget2 = new ButtonWidget(12);
        buttonWidget2.Init(Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "INSERT FILAMENT (EXTERNAL)";
        buttonWidget2.SetGrowableWidth(4, 4, 32);
        buttonWidget2.SetGrowableHeight(4, 4, 32);
        buttonWidget2.SetSize(192, 60);
        buttonWidget2.SetPosition(-252, -100);
        buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget2);
      }
      PopulateStartupControlsList();
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      insertwarn_sent = false;
      try
      {
        PrinterObject selectedPrinter = MainWindow.GetSelectedPrinter();
        if (selectedPrinter == null)
        {
          return;
        }

        OnUpdate();
        DisableAllControls();
        text_main.Visible = true;
        add_button_external.Visible = true;
        add_button_external.Enabled = true;
        add_button_internal.Visible = true;
        add_button_internal.Enabled = true;
        cancel_button.Visible = true;
        cancel_button.Enabled = true;
        if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew)
        {
          text_title.Text = "Currently inserting:\n\n" + FilamentProfile.GenerateSpoolName(CurrentDetails.current_spool, false);
          text_main.Text = "Please select filament location for insertion:";
          add_button_external.Text = "INSERT FILAMENT (EXTERNAL)";
          add_button_internal.Text = "INSERT FILAMENT (INTERNAL)";
        }
        else if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted)
        {
          text_title.Text = "Already inserted:\n\n" + FilamentProfile.GenerateSpoolName(CurrentDetails.current_spool, false);
          text_main.Text = "Please select filament location:";
          add_button_external.Text = "EXTERNAL";
          add_button_internal.Text = "INTERNAL";
        }
        if (selectedPrinter.IsPausedorPausing)
        {
          text_main.Text += "\n\n\nFilament cannot be loaded internally while the printer is paused.";
          if (add_button_internal.Visible)
          {
            add_button_internal.Enabled = false;
          }
        }
        else
        {
          if (CurrentDetails.current_spool.filament_type != FilamentSpool.TypeEnum.FLX)
          {
            if (CurrentDetails.current_spool.filament_type != FilamentSpool.TypeEnum.TGH)
            {
              goto label_17;
            }
          }
          if (!insertwarn_sent)
          {
            insertwarn_sent = true;
            messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning: Flexible filament can only be inserted externally."));
          }
          text_main.Text += "\n\n\nLoading flexible filaments internally can cause the filament to snag and in turn, cause prints to fail.";
          if (add_button_internal.Visible)
          {
            add_button_internal.Enabled = false;
          }
        }
      }
      catch (Exception ex)
      {
      }
label_17:
      OnUpdate();
    }

    private void PopulateStartupControlsList()
    {
      add_button_external = (ButtonWidget)FindChildElement(12);
      add_button_internal = (ButtonWidget)FindChildElement(11);
      cancel_button = (ButtonWidget)FindChildElement(9);
      text_main = (TextWidget)FindChildElement(3);
      text_title = (TextWidget)FindChildElement(1);
    }

    public void DisableAllControls()
    {
      text_main.Visible = false;
      add_button_external.Visible = false;
      add_button_external.Enabled = false;
      add_button_internal.Visible = false;
      add_button_internal.Enabled = false;
      cancel_button.Visible = false;
      cancel_button.Enabled = false;
    }

    public enum ControlIDs
    {
      AddFilamentInternal = 11, // 0x0000000B
      AddFilamentExternal = 12, // 0x0000000C
    }
  }
}
