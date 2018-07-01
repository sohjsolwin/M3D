// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentFilamentSpoolSize
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      PrinterObject selectedPrinter = this.MainWindow.GetSelectedPrinter();
      if (selectedPrinter == null)
        return;
      selectedPrinter.MarkedAsBusy = true;
      bool flag = false;
      switch (button.ID)
      {
        case 9:
          this.MainWindow.ResetToStartup();
          break;
        case 11:
          this.CurrentDetails.current_spool.filament_size = FilamentSpool.SizeEnum.Pro;
          flag = true;
          break;
        case 12:
          this.CurrentDetails.current_spool.filament_size = FilamentSpool.SizeEnum.Micro;
          flag = true;
          break;
        default:
          throw new NotImplementedException();
      }
      if (!flag)
        return;
      if (this.settingsManager.CurrentFilamentSettings.TrackFilament)
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page19_FilamentIsNewSpoolPage, this.CurrentDetails);
      else
        this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page13_FilamentLocation, this.CurrentDetails);
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Size of Filament Spool Currently in use:", "", true, false, false, false, false, false);
      Frame childElement = (Frame) this.FindChildElement(2);
      if (childElement != null)
      {
        ButtonWidget buttonWidget1 = new ButtonWidget(11);
        buttonWidget1.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget1.Size = FontSize.Medium;
        buttonWidget1.Text = "PRO SPOOL";
        buttonWidget1.SetGrowableWidth(4, 4, 32);
        buttonWidget1.SetGrowableHeight(4, 4, 32);
        buttonWidget1.SetSize(192, 60);
        buttonWidget1.SetPosition(60, -100);
        buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
        childElement.AddChildElement((Element2D) buttonWidget1);
        ButtonWidget buttonWidget2 = new ButtonWidget(12);
        buttonWidget2.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
        buttonWidget2.Size = FontSize.Medium;
        buttonWidget2.Text = "MICRO SPOOL";
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
      try
      {
        if (this.MainWindow.GetSelectedPrinter() == null)
          return;
        this.OnUpdate();
        this.DisableAllControls();
        this.text_main.Visible = true;
        this.pro_filament_button.Visible = true;
        this.pro_filament_button.Enabled = true;
        this.micro_filament_button.Visible = true;
        this.micro_filament_button.Enabled = true;
        this.cancel_button.Visible = true;
        this.cancel_button.Enabled = true;
        this.text_title.Text = "Currently inserting:\n\n" + FilamentProfile.GenerateSpoolName(this.CurrentDetails.current_spool, false);
        this.text_main.Text = "Please select filament spool size for insertion:";
      }
      catch (Exception ex)
      {
      }
      this.OnUpdate();
    }

    private void PopulateStartupControlsList()
    {
      this.pro_filament_button = (ButtonWidget) this.FindChildElement(11);
      this.micro_filament_button = (ButtonWidget) this.FindChildElement(12);
      this.cancel_button = (ButtonWidget) this.FindChildElement(9);
      this.text_main = (TextWidget) this.FindChildElement(3);
      this.text_title = (TextWidget) this.FindChildElement(1);
    }

    public void DisableAllControls()
    {
      this.text_main.Visible = false;
      this.pro_filament_button.Visible = false;
      this.pro_filament_button.Enabled = false;
      this.micro_filament_button.Visible = false;
      this.micro_filament_button.Enabled = false;
      this.cancel_button.Visible = false;
      this.cancel_button.Enabled = false;
    }

    public enum ControlIDs
    {
      ProFilamentSpoolSize = 11, // 0x0000000B
      MicroFilamentSpoolSize = 12, // 0x0000000C
    }
  }
}
