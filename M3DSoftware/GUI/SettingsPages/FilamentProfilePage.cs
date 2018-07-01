// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.FilamentProfilePage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;

namespace M3D.GUI.SettingsPages
{
  public class FilamentProfilePage : SettingsPage
  {
    private int prevSelectedProfile = -1;
    private GUIHost host;
    private ListBoxWidget filamentprofile_list;
    private EditBoxWidget type_edit;
    private EditBoxWidget color_edit;
    private EditBoxWidget temperature_edit;
    private ButtonWidget add_button;
    private ButtonWidget remove_button;
    private ButtonWidget TemperatureEditButton;
    private ButtonWidget TemperatureSaveButton;
    private ButtonWidget TemperatureResetButton;
    private ButtonWidget track_filament;
    private ButtonWidget clean_nozzle;
    private AddFilamentProfileDialog addfilament_frame;
    private SettingsManager settingsManager;
    private SpoolerConnection spooler_connection;
    private PopupMessageBox messagebox;
    private XMLFrame filamentProfilesFrame;
    private XMLFrame filamentSettingsFrame;
    private Frame active_frame;

    public FilamentProfilePage(int ID, SettingsManager settingsManager, GUIHost host, SpoolerConnection spooler_connection, PopupMessageBox messagebox)
      : base(ID)
    {
      this.host = host;
      this.settingsManager = settingsManager;
      this.spooler_connection = spooler_connection;
      this.messagebox = messagebox;
      string filamentsettingsTabbuttons = Resources.filamentsettings_tabbuttons;
      this.Init(host, filamentsettingsTabbuttons, new ButtonCallback(this.tabsFrameButtonCallback));
      this.Visible = false;
      this.Enabled = false;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      this.CreateAdvancedFilamentSettingsFrame();
      this.CreateFilamentProfilesFrame();
      this.active_frame = (Frame) this.filamentSettingsFrame;
    }

    private void TurnOffActiveFrame()
    {
      if (this.active_frame == null)
        return;
      this.active_frame.Visible = false;
      this.active_frame.Enabled = false;
      this.active_frame = (Frame) null;
    }

    public void tabsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.filamentSettingsFrame;
          break;
        case 2:
          this.TurnOffActiveFrame();
          this.active_frame = (Frame) this.filamentProfilesFrame;
          break;
      }
      if (this.active_frame != null)
      {
        this.active_frame.Enabled = true;
        this.active_frame.Visible = true;
        this.host.SetFocus((Element2D) this.active_frame);
      }
      this.Refresh();
    }

    public override void SetVisible(bool bVisible)
    {
      if (this.temperature_edit != null)
        this.temperature_edit.Enabled = false;
      base.SetVisible(bVisible);
      if (this.TemperatureEditButton != null)
        this.TemperatureEditButton.Visible = true;
      if (this.TemperatureSaveButton != null)
        this.TemperatureSaveButton.Visible = false;
      if (this.TemperatureResetButton == null)
        return;
      this.TemperatureResetButton.Visible = true;
    }

    private void UpdateProfileList()
    {
      this.UpdateProfileList(FilamentSpool.TypeEnum.OtherOrUnknown, FilamentConstants.ColorsEnum.Other);
    }

    private void UpdateProfileList(int selected)
    {
      this.filamentprofile_list.Items.Clear();
      foreach (KeyValuePair<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> customValue in this.settingsManager.FilamentDictionary.CustomValues)
        this.filamentprofile_list.Items.Add((object) new FilamentProfilePage.FilamentOptions(customValue.Key, customValue.Value));
      this.SelectProfile(selected);
    }

    public void UpdateProfileList(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
    {
      this.filamentprofile_list.Items.Clear();
      int index = 0;
      int num = 0;
      foreach (KeyValuePair<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> customValue in this.settingsManager.FilamentDictionary.CustomValues)
      {
        if (customValue.Key.type == type && customValue.Key.color == color)
          index = num;
        this.filamentprofile_list.Items.Add((object) new FilamentProfilePage.FilamentOptions(customValue.Key, customValue.Value));
        ++num;
      }
      this.SelectProfile(index);
    }

    protected override void OnHide()
    {
      if (this.prevSelectedProfile <= -1)
        return;
      this.UpdateProfileRemainingValue(this.prevSelectedProfile);
    }

    protected override void OnUnhide()
    {
      base.OnUnhide();
      this.UpdateProfileList(this.filamentprofile_list.Selected);
    }

    public static string GetColorString(string color)
    {
      if (color.Equals("DarkBlue"))
        return "Dark Blue";
      if (color.Equals("DarkGreen"))
        return "Dark Green";
      if (color.Equals("LightBlue"))
        return "Light Blue";
      if (color.Equals("LightGreen"))
        return "Light Green";
      if (color.Equals("NeonBlue"))
        return "Neon Blue";
      if (color.Equals("NeonOrange"))
        return "Neon Orange";
      if (color.Equals("NeonYellow"))
        return "Neon Yellow";
      return color;
    }

    public void SelectProfile(int index)
    {
      if (index >= 0 && index < this.filamentprofile_list.Items.Count)
      {
        FilamentProfilePage.FilamentOptions filamentOptions = (FilamentProfilePage.FilamentOptions) this.filamentprofile_list.Items[index];
        this.type_edit.Text = filamentOptions.Key.type.ToString();
        this.color_edit.Text = FilamentConstants.ColorsToString(filamentOptions.Key.color);
        this.temperature_edit.Text = filamentOptions.Options.temperature.ToString();
        this.TemperatureEditButton.Enabled = true;
        this.TemperatureResetButton.Enabled = true;
      }
      else
      {
        this.type_edit.Text = "";
        this.color_edit.Text = "";
        this.temperature_edit.Text = "";
        this.TemperatureEditButton.Enabled = false;
        this.TemperatureResetButton.Enabled = false;
      }
      this.temperature_edit.Enabled = false;
      this.TemperatureEditButton.Visible = true;
      this.TemperatureSaveButton.Visible = false;
      this.TemperatureSaveButton.Enabled = false;
      this.filamentprofile_list.Selected = index;
      this.prevSelectedProfile = index;
    }

    private void CreateFilamentProfilesFrame()
    {
      string filamentprofilesframe = Resources.filamentprofilesframe;
      this.filamentProfilesFrame = new XMLFrame();
      this.filamentProfilesFrame.Init(this.host, filamentprofilesframe, new ButtonCallback(this.CustomTempButtonCallback));
      this.type_edit = (EditBoxWidget) this.filamentProfilesFrame.FindChildElement(1001);
      this.color_edit = (EditBoxWidget) this.filamentProfilesFrame.FindChildElement(1002);
      this.temperature_edit = (EditBoxWidget) this.filamentProfilesFrame.FindChildElement(1003);
      this.add_button = (ButtonWidget) this.filamentProfilesFrame.FindChildElement(1005);
      this.remove_button = (ButtonWidget) this.filamentProfilesFrame.FindChildElement(1006);
      this.TemperatureEditButton = (ButtonWidget) this.filamentProfilesFrame.FindChildElement(1007);
      this.TemperatureSaveButton = (ButtonWidget) this.filamentProfilesFrame.FindChildElement(1008);
      this.TemperatureResetButton = (ButtonWidget) this.filamentProfilesFrame.FindChildElement(1010);
      this.filamentprofile_list = (ListBoxWidget) this.filamentProfilesFrame.FindChildElement(1009);
      this.filamentprofile_list.SetOnChangeCallback(new ListBoxWidget.OnChangeCallback(this.MyOnChangeProfileCallback));
      this.addfilament_frame = new AddFilamentProfileDialog(0, this.settingsManager, this);
      this.addfilament_frame.Init(this.host);
      this.addfilament_frame.SetSize(320, 300);
      this.addfilament_frame.CenterHorizontallyInParent = true;
      this.addfilament_frame.CenterVerticallyInParent = true;
      this.host.AddControlElement((Element2D) this.addfilament_frame);
      this.addfilament_frame.Visible = false;
      this.addfilament_frame.Enabled = false;
      this.filamentProfilesFrame.ID = 1001;
      this.filamentProfilesFrame.CenterHorizontallyInParent = true;
      this.filamentProfilesFrame.RelativeY = 0.1f;
      this.filamentProfilesFrame.RelativeWidth = 0.99f;
      this.filamentProfilesFrame.RelativeHeight = 0.9f;
      this.filamentProfilesFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.filamentProfilesFrame.Visible = false;
      this.filamentProfilesFrame.Enabled = false;
      this.childFrame.AddChildElement((Element2D) this.filamentProfilesFrame);
      this.filamentProfilesFrame.Refresh();
    }

    private void CreateAdvancedFilamentSettingsFrame()
    {
      string advancedfilamentsettings = Resources.advancedfilamentsettings;
      this.filamentSettingsFrame = new XMLFrame();
      this.filamentSettingsFrame.Init(this.host, advancedfilamentsettings, new ButtonCallback(this.CustomTempButtonCallback));
      this.filamentSettingsFrame.ID = 1002;
      this.filamentSettingsFrame.CenterHorizontallyInParent = true;
      this.filamentSettingsFrame.RelativeY = 0.1f;
      this.filamentSettingsFrame.RelativeWidth = 0.95f;
      this.filamentSettingsFrame.RelativeHeight = 0.9f;
      this.filamentSettingsFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      this.filamentSettingsFrame.Visible = true;
      this.filamentSettingsFrame.Enabled = true;
      this.childFrame.AddChildElement((Element2D) this.filamentSettingsFrame);
      this.filamentSettingsFrame.Refresh();
      this.track_filament = (ButtonWidget) this.filamentSettingsFrame.FindChildElement(1100);
      if (this.track_filament != null)
      {
        this.track_filament.SetCallback(new ButtonCallback(this.FilamentSettingsFrameButtonCallback));
        this.track_filament.Checked = this.settingsManager.CurrentFilamentSettings.TrackFilament;
      }
      this.clean_nozzle = (ButtonWidget) this.filamentSettingsFrame.FindChildElement(1102);
      if (this.clean_nozzle == null)
        return;
      this.clean_nozzle.SetCallback(new ButtonCallback(this.FilamentSettingsFrameButtonCallback));
      this.clean_nozzle.Checked = this.settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert;
    }

    public void UpdateProfileRemainingValue(int index)
    {
      if (index == -1)
        index = this.prevSelectedProfile;
      if (index <= -1)
        return;
      if (index >= this.filamentprofile_list.Items.Count)
        return;
      try
      {
        object obj = this.filamentprofile_list.Items[index];
        if (!(obj is FilamentProfilePage.FilamentOptions))
          return;
        FilamentProfilePage.FilamentOptions filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
        this.UpdateFilamentProfile(filamentOptions.Key, filamentOptions.Options);
      }
      catch (Exception ex)
      {
      }
    }

    public void MyOnChangeProfileCallback(ListBoxWidget listBox)
    {
      if (this.prevSelectedProfile > -1)
      {
        this.UpdateProfileRemainingValue(this.prevSelectedProfile);
        this.UpdateProfileList(listBox.Selected);
      }
      this.SelectProfile(listBox.Selected);
      this.prevSelectedProfile = listBox.Selected;
    }

    public void CustomTempButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1005:
          this.Enabled = false;
          this.addfilament_frame.Visible = true;
          this.addfilament_frame.Enabled = true;
          this.host.GlobalChildDialog += (Element2D) this.addfilament_frame;
          this.prevSelectedProfile = -1;
          break;
        case 1006:
          if (this.filamentprofile_list.Items.Count >= 0 && this.filamentprofile_list.Selected < this.filamentprofile_list.Items.Count)
          {
            object obj = this.filamentprofile_list.Items[this.filamentprofile_list.Selected];
            if (obj is FilamentProfilePage.FilamentOptions)
            {
              if (this.spooler_connection.FilamentSpoolLoaded(((FilamentProfilePage.FilamentOptions) obj).Key, new FilamentProfile.CustomOptions()) && this.settingsManager.ShowAllWarnings)
                this.messagebox.AddMessageToQueue("Warning: Filament profile in use. Deleting custom profile will not reset temperature to default..", PopupMessageBox.MessageBoxButtons.OKCANCEL, new PopupMessageBox.OnUserSelectionDel(this.OnUserSelection), (object) this.spooler_connection.SelectedPrinter);
              else
                this.RemoveFilamentProfile();
            }
          }
          this.prevSelectedProfile = -1;
          break;
        case 1007:
          this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning. Changing the temperature can cause permanent damage to your printer. This will change the temperature for all filaments of this type and color."));
          this.temperature_edit.Enabled = true;
          this.TemperatureEditButton.Visible = false;
          this.TemperatureSaveButton.Visible = true;
          this.TemperatureSaveButton.Enabled = true;
          this.TemperatureResetButton.Enabled = false;
          break;
        case 1008:
          try
          {
            if (this.filamentprofile_list.Selected >= 0 && this.filamentprofile_list.Selected < this.filamentprofile_list.Items.Count)
            {
              object obj = this.filamentprofile_list.Items[this.filamentprofile_list.Selected];
              if (obj is FilamentProfilePage.FilamentOptions)
              {
                FilamentProfilePage.FilamentOptions filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
                float num = float.Parse(this.temperature_edit.Text);
                FilamentConstants.Temperature.MaxMin maxMin = FilamentConstants.Temperature.MaxMinForFilamentType(filamentOptions.Key.type);
                if ((double) num >= (double) maxMin.Min && (double) num <= (double) maxMin.Max)
                {
                  this.UpdateTemperature((int) num);
                  this.temperature_edit.Enabled = false;
                  this.TemperatureEditButton.Visible = true;
                  this.TemperatureSaveButton.Visible = false;
                  this.TemperatureResetButton.Visible = true;
                }
                else
                  this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a temperature from " + (object) maxMin.Min + " to " + (object) maxMin.Max));
              }
            }
            this.UpdateProfileList(this.filamentprofile_list.Selected);
            break;
          }
          catch (Exception ex)
          {
            this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Sorry. The temperature you entered is invalid."));
            break;
          }
        case 1010:
          int temperature = FilamentConstants.Temperature.Default(this.spooler_connection.SelectedPrinter.GetCurrentFilament().filament_type);
          this.UpdateTemperature(temperature);
          this.temperature_edit.Text = temperature.ToString();
          break;
      }
    }

    public void FilamentSettingsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1100:
          this.settingsManager.CurrentFilamentSettings.TrackFilament = this.track_filament.Checked;
          break;
        case 1102:
          this.settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert = this.clean_nozzle.Checked;
          break;
      }
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
        return;
      this.RemoveFilamentProfile();
    }

    private void RemoveFilamentProfile()
    {
      object obj = this.filamentprofile_list.Items[this.filamentprofile_list.Selected];
      if (obj is FilamentProfilePage.FilamentOptions)
      {
        FilamentProfilePage.FilamentOptions filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
        this.settingsManager.FilamentDictionary.RemoveCustomTemperature(filamentOptions.Key.type, filamentOptions.Key.color);
      }
      this.UpdateProfileList();
    }

    private void UpdateTemperature(int temperature)
    {
      if (this.prevSelectedProfile < 0 || this.prevSelectedProfile >= this.filamentprofile_list.Items.Count)
        return;
      object obj = this.filamentprofile_list.Items[this.prevSelectedProfile];
      if (!(obj is FilamentProfilePage.FilamentOptions))
        return;
      this.UpdateFilamentProfile(((FilamentProfilePage.FilamentOptions) obj).Key, new FilamentProfile.CustomOptions()
      {
        temperature = temperature
      });
    }

    private void UpdateFilamentProfile(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions options)
    {
      this.settingsManager.FilamentDictionary.AddCustomTemperature(key.type, key.color, options.temperature);
      this.spooler_connection.CheckUpdatedFilamentProfile(key, options);
    }

    private enum ControlIDs
    {
      TypeEdit = 1001, // 0x000003E9
      ColorEdit = 1002, // 0x000003EA
      TemperatureEditBox = 1003, // 0x000003EB
      RemainingEdit = 1004, // 0x000003EC
      AddButton = 1005, // 0x000003ED
      RemoveButton = 1006, // 0x000003EE
      TemperatureEditButton = 1007, // 0x000003EF
      TemperatureSaveButton = 1008, // 0x000003F0
      FilamentProfileList = 1009, // 0x000003F1
      TemperatureResetButton = 1010, // 0x000003F2
    }

    private enum FilamentSettingIDs
    {
      TrackFilament = 1100, // 0x0000044C
      CleanNozzleDialog = 1102, // 0x0000044E
    }

    private struct FilamentOptions
    {
      public FilamentProfile.TypeColorKey Key;
      public FilamentProfile.CustomOptions Options;

      public FilamentOptions(FilamentProfile.TypeColorKey Key, FilamentProfile.CustomOptions Options)
      {
        this.Key = Key;
        this.Options = Options;
      }

      public override string ToString()
      {
        return ((int) this.Key.type).ToString() + " " + (object) this.Key.color;
      }
    }
  }
}
