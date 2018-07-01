// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentFilamentLocation
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      selectedPrinter.MarkedAsBusy = true;
      switch (button.ID)
      {
        case 9:
          this.MainWindow.ResetToStartup();
          break;
        case 11:
        case 12:
          if (button.ID == 11)
            this.CurrentDetails.current_spool.filament_location = FilamentSpool.Location.Internal;
          else
            this.CurrentDetails.current_spool.filament_location = FilamentSpool.Location.External;
          this.SetFilamentAndStartNextProcess();
          break;
        default:
          throw new NotImplementedException();
      }
    }

    private void SetFilamentAndStartNextProcess()
    {
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew)
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page12_RaisingExtruder, this.CurrentDetails);
      else if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted)
      {
        this.CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetDetails;
        this.CurrentDetails.pageAfterWait = Manage3DInkMainWindow.PageID.Page0_StartupPage;
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, this.CurrentDetails);
        if (this.settingsManager != null)
        {
          this.settingsManager.AssociateFilamentToPrinter(selectedPrinter.Info.serial_number, this.CurrentDetails.current_spool);
          this.settingsManager.SaveSettings();
        }
        int num = (int) selectedPrinter.SetFilamentInfo(new M3D.Spooling.Client.AsyncCallback(selectedPrinter.ShowLockError), (object) selectedPrinter, this.CurrentDetails.current_spool);
      }
      else
      {
        selectedPrinter.MarkedAsBusy = false;
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page0_StartupPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None));
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Filament Currently in use:", "", true, false, false, false, false, false);
      Frame childElement = (Frame) this.FindChildElement(2);
      if (childElement != null)
      {
        ButtonWidget buttonWidget1 = new ButtonWidget(11);
        buttonWidget1.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget1.Size = FontSize.Medium;
        buttonWidget1.Text = "INSERT FILAMENT (INTERNAL)";
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetSize(192, 60);
        buttonWidget1.SetPosition(60, -100);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget1);
        ButtonWidget buttonWidget2 = new ButtonWidget(12);
        buttonWidget2.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "INSERT FILAMENT (EXTERNAL)";
        buttonWidget2.SetGrowableWidth(4, 4, 32);
        buttonWidget2.SetGrowableHeight(4, 4, 32);
        buttonWidget2.SetSize(192, 60);
        buttonWidget2.SetPosition(-252, -100);
        buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget2);
      }
      this.PopulateStartupControlsList();
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.insertwarn_sent = false;
      try
      {
        PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
        if (selectedPrinter == null)
          return;
        this.OnUpdate();
        this.DisableAllControls();
        this.text_main.Visible = true;
        this.add_button_external.Visible = true;
        this.add_button_external.Enabled = true;
        this.add_button_internal.Visible = true;
        this.add_button_internal.Enabled = true;
        this.cancel_button.Visible = true;
        this.cancel_button.Enabled = true;
        if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew)
        {
          this.text_title.Text = "Currently inserting:\n\n" + FilamentProfile.GenerateSpoolName(this.CurrentDetails.current_spool, false);
          this.text_main.Text = "Please select filament location for insertion:";
          this.add_button_external.Text = "INSERT FILAMENT (EXTERNAL)";
          this.add_button_internal.Text = "INSERT FILAMENT (INTERNAL)";
        }
        else if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted)
        {
          this.text_title.Text = "Already inserted:\n\n" + FilamentProfile.GenerateSpoolName(this.CurrentDetails.current_spool, false);
          this.text_main.Text = "Please select filament location:";
          this.add_button_external.Text = "EXTERNAL";
          this.add_button_internal.Text = "INTERNAL";
        }
        if (selectedPrinter.IsPausedorPausing)
        {
          this.text_main.Text += "\n\n\nFilament cannot be loaded internally while the printer is paused.";
          if (this.add_button_internal.Visible)
            this.add_button_internal.Enabled = false;
        }
        else
        {
          if (this.CurrentDetails.current_spool.filament_type != FilamentSpool.TypeEnum.FLX)
          {
            if (this.CurrentDetails.current_spool.filament_type != FilamentSpool.TypeEnum.TGH)
              goto label_17;
          }
          if (!this.insertwarn_sent)
          {
            this.insertwarn_sent = true;
            this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning: Flexible filament can only be inserted externally."));
          }
          this.text_main.Text += "\n\n\nLoading flexible filaments internally can cause the filament to snag and in turn, cause prints to fail.";
          if (this.add_button_internal.Visible)
            this.add_button_internal.Enabled = false;
        }
      }
      catch (Exception ex)
      {
      }
label_17:
      this.OnUpdate();
    }

    private void PopulateStartupControlsList()
    {
      this.add_button_external = (ButtonWidget) this.FindChildElement(12);
      this.add_button_internal = (ButtonWidget) this.FindChildElement(11);
      this.cancel_button = (ButtonWidget) this.FindChildElement(9);
      this.text_main = (TextWidget) this.FindChildElement(3);
      this.text_title = (TextWidget) this.FindChildElement(1);
    }

    public void DisableAllControls()
    {
      this.text_main.Visible = false;
      this.add_button_external.Visible = false;
      this.add_button_external.Enabled = false;
      this.add_button_internal.Visible = false;
      this.add_button_internal.Enabled = false;
      this.cancel_button.Visible = false;
      this.cancel_button.Enabled = false;
    }

    public enum ControlIDs
    {
      AddFilamentInternal = 11, // 0x0000000B
      AddFilamentExternal = 12, // 0x0000000C
    }
  }
}
