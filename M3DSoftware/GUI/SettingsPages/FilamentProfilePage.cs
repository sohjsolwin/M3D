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
      var filamentsettingsTabbuttons = Resources.filamentsettings_tabbuttons;
      Init(host, filamentsettingsTabbuttons, new ButtonCallback(tabsFrameButtonCallback));
      Visible = false;
      Enabled = false;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      CreateAdvancedFilamentSettingsFrame();
      CreateFilamentProfilesFrame();
      active_frame = (Frame)filamentSettingsFrame;
    }

    private void TurnOffActiveFrame()
    {
      if (active_frame == null)
      {
        return;
      }

      active_frame.Visible = false;
      active_frame.Enabled = false;
      active_frame = (Frame) null;
    }

    public void tabsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1:
          TurnOffActiveFrame();
          active_frame = (Frame)filamentSettingsFrame;
          break;
        case 2:
          TurnOffActiveFrame();
          active_frame = (Frame)filamentProfilesFrame;
          break;
      }
      if (active_frame != null)
      {
        active_frame.Enabled = true;
        active_frame.Visible = true;
        host.SetFocus((Element2D)active_frame);
      }
      Refresh();
    }

    public override void SetVisible(bool bVisible)
    {
      if (temperature_edit != null)
      {
        temperature_edit.Enabled = false;
      }

      base.SetVisible(bVisible);
      if (TemperatureEditButton != null)
      {
        TemperatureEditButton.Visible = true;
      }

      if (TemperatureSaveButton != null)
      {
        TemperatureSaveButton.Visible = false;
      }

      if (TemperatureResetButton == null)
      {
        return;
      }

      TemperatureResetButton.Visible = true;
    }

    private void UpdateProfileList()
    {
      UpdateProfileList(FilamentSpool.TypeEnum.OtherOrUnknown, FilamentConstants.ColorsEnum.Other);
    }

    private void UpdateProfileList(int selected)
    {
      filamentprofile_list.Items.Clear();
      foreach (KeyValuePair<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> customValue in settingsManager.FilamentDictionary.CustomValues)
      {
        filamentprofile_list.Items.Add((object) new FilamentProfilePage.FilamentOptions(customValue.Key, customValue.Value));
      }

      SelectProfile(selected);
    }

    public void UpdateProfileList(FilamentSpool.TypeEnum type, FilamentConstants.ColorsEnum color)
    {
      filamentprofile_list.Items.Clear();
      var index = 0;
      var num = 0;
      foreach (KeyValuePair<FilamentProfile.TypeColorKey, FilamentProfile.CustomOptions> customValue in settingsManager.FilamentDictionary.CustomValues)
      {
        if (customValue.Key.type == type && customValue.Key.color == color)
        {
          index = num;
        }

        filamentprofile_list.Items.Add((object) new FilamentProfilePage.FilamentOptions(customValue.Key, customValue.Value));
        ++num;
      }
      SelectProfile(index);
    }

    protected override void OnHide()
    {
      if (prevSelectedProfile <= -1)
      {
        return;
      }

      UpdateProfileRemainingValue(prevSelectedProfile);
    }

    protected override void OnUnhide()
    {
      base.OnUnhide();
      UpdateProfileList(filamentprofile_list.Selected);
    }

    public static string GetColorString(string color)
    {
      if (color.Equals("DarkBlue"))
      {
        return "Dark Blue";
      }

      if (color.Equals("DarkGreen"))
      {
        return "Dark Green";
      }

      if (color.Equals("LightBlue"))
      {
        return "Light Blue";
      }

      if (color.Equals("LightGreen"))
      {
        return "Light Green";
      }

      if (color.Equals("NeonBlue"))
      {
        return "Neon Blue";
      }

      if (color.Equals("NeonOrange"))
      {
        return "Neon Orange";
      }

      if (color.Equals("NeonYellow"))
      {
        return "Neon Yellow";
      }

      return color;
    }

    public void SelectProfile(int index)
    {
      if (index >= 0 && index < filamentprofile_list.Items.Count)
      {
        var filamentOptions = (FilamentProfilePage.FilamentOptions)filamentprofile_list.Items[index];
        type_edit.Text = filamentOptions.Key.type.ToString();
        color_edit.Text = FilamentConstants.ColorsToString(filamentOptions.Key.color);
        temperature_edit.Text = filamentOptions.Options.temperature.ToString();
        TemperatureEditButton.Enabled = true;
        TemperatureResetButton.Enabled = true;
      }
      else
      {
        type_edit.Text = "";
        color_edit.Text = "";
        temperature_edit.Text = "";
        TemperatureEditButton.Enabled = false;
        TemperatureResetButton.Enabled = false;
      }
      temperature_edit.Enabled = false;
      TemperatureEditButton.Visible = true;
      TemperatureSaveButton.Visible = false;
      TemperatureSaveButton.Enabled = false;
      filamentprofile_list.Selected = index;
      prevSelectedProfile = index;
    }

    private void CreateFilamentProfilesFrame()
    {
      var filamentprofilesframe = Resources.filamentprofilesframe;
      filamentProfilesFrame = new XMLFrame();
      filamentProfilesFrame.Init(host, filamentprofilesframe, new ButtonCallback(CustomTempButtonCallback));
      type_edit = (EditBoxWidget)filamentProfilesFrame.FindChildElement(1001);
      color_edit = (EditBoxWidget)filamentProfilesFrame.FindChildElement(1002);
      temperature_edit = (EditBoxWidget)filamentProfilesFrame.FindChildElement(1003);
      add_button = (ButtonWidget)filamentProfilesFrame.FindChildElement(1005);
      remove_button = (ButtonWidget)filamentProfilesFrame.FindChildElement(1006);
      TemperatureEditButton = (ButtonWidget)filamentProfilesFrame.FindChildElement(1007);
      TemperatureSaveButton = (ButtonWidget)filamentProfilesFrame.FindChildElement(1008);
      TemperatureResetButton = (ButtonWidget)filamentProfilesFrame.FindChildElement(1010);
      filamentprofile_list = (ListBoxWidget)filamentProfilesFrame.FindChildElement(1009);
      filamentprofile_list.SetOnChangeCallback(new ListBoxWidget.OnChangeCallback(MyOnChangeProfileCallback));
      addfilament_frame = new AddFilamentProfileDialog(0, settingsManager, this);
      addfilament_frame.Init(host);
      addfilament_frame.SetSize(320, 300);
      addfilament_frame.CenterHorizontallyInParent = true;
      addfilament_frame.CenterVerticallyInParent = true;
      host.AddControlElement((Element2D)addfilament_frame);
      addfilament_frame.Visible = false;
      addfilament_frame.Enabled = false;
      filamentProfilesFrame.ID = 1001;
      filamentProfilesFrame.CenterHorizontallyInParent = true;
      filamentProfilesFrame.RelativeY = 0.1f;
      filamentProfilesFrame.RelativeWidth = 0.99f;
      filamentProfilesFrame.RelativeHeight = 0.9f;
      filamentProfilesFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      filamentProfilesFrame.Visible = false;
      filamentProfilesFrame.Enabled = false;
      childFrame.AddChildElement((Element2D)filamentProfilesFrame);
      filamentProfilesFrame.Refresh();
    }

    private void CreateAdvancedFilamentSettingsFrame()
    {
      var advancedfilamentsettings = Resources.advancedfilamentsettings;
      filamentSettingsFrame = new XMLFrame();
      filamentSettingsFrame.Init(host, advancedfilamentsettings, new ButtonCallback(CustomTempButtonCallback));
      filamentSettingsFrame.ID = 1002;
      filamentSettingsFrame.CenterHorizontallyInParent = true;
      filamentSettingsFrame.RelativeY = 0.1f;
      filamentSettingsFrame.RelativeWidth = 0.95f;
      filamentSettingsFrame.RelativeHeight = 0.9f;
      filamentSettingsFrame.BGColor = new Color4(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
      filamentSettingsFrame.Visible = true;
      filamentSettingsFrame.Enabled = true;
      childFrame.AddChildElement((Element2D)filamentSettingsFrame);
      filamentSettingsFrame.Refresh();
      track_filament = (ButtonWidget)filamentSettingsFrame.FindChildElement(1100);
      if (track_filament != null)
      {
        track_filament.SetCallback(new ButtonCallback(FilamentSettingsFrameButtonCallback));
        track_filament.Checked = settingsManager.CurrentFilamentSettings.TrackFilament;
      }
      clean_nozzle = (ButtonWidget)filamentSettingsFrame.FindChildElement(1102);
      if (clean_nozzle == null)
      {
        return;
      }

      clean_nozzle.SetCallback(new ButtonCallback(FilamentSettingsFrameButtonCallback));
      clean_nozzle.Checked = settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert;
    }

    public void UpdateProfileRemainingValue(int index)
    {
      if (index == -1)
      {
        index = prevSelectedProfile;
      }

      if (index <= -1)
      {
        return;
      }

      if (index >= filamentprofile_list.Items.Count)
      {
        return;
      }

      try
      {
        var obj = filamentprofile_list.Items[index];
        if (!(obj is FilamentProfilePage.FilamentOptions))
        {
          return;
        }

        var filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
        UpdateFilamentProfile(filamentOptions.Key, filamentOptions.Options);
      }
      catch (Exception ex)
      {
      }
    }

    public void MyOnChangeProfileCallback(ListBoxWidget listBox)
    {
      if (prevSelectedProfile > -1)
      {
        UpdateProfileRemainingValue(prevSelectedProfile);
        UpdateProfileList(listBox.Selected);
      }
      SelectProfile(listBox.Selected);
      prevSelectedProfile = listBox.Selected;
    }

    public void CustomTempButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1005:
          Enabled = false;
          addfilament_frame.Visible = true;
          addfilament_frame.Enabled = true;
          host.GlobalChildDialog += (Element2D)addfilament_frame;
          prevSelectedProfile = -1;
          break;
        case 1006:
          if (filamentprofile_list.Items.Count >= 0 && filamentprofile_list.Selected < filamentprofile_list.Items.Count)
          {
            var obj = filamentprofile_list.Items[filamentprofile_list.Selected];
            if (obj is FilamentProfilePage.FilamentOptions)
            {
              if (spooler_connection.FilamentSpoolLoaded(((FilamentProfilePage.FilamentOptions) obj).Key, new FilamentProfile.CustomOptions()) && settingsManager.ShowAllWarnings)
              {
                messagebox.AddMessageToQueue("Warning: Filament profile in use. Deleting custom profile will not reset temperature to default..", PopupMessageBox.MessageBoxButtons.OKCANCEL, new PopupMessageBox.OnUserSelectionDel(OnUserSelection), (object)spooler_connection.SelectedPrinter);
              }
              else
              {
                RemoveFilamentProfile();
              }
            }
          }
          prevSelectedProfile = -1;
          break;
        case 1007:
          messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Warning. Changing the temperature can cause permanent damage to your printer. This will change the temperature for all filaments of this type and color."));
          temperature_edit.Enabled = true;
          TemperatureEditButton.Visible = false;
          TemperatureSaveButton.Visible = true;
          TemperatureSaveButton.Enabled = true;
          TemperatureResetButton.Enabled = false;
          break;
        case 1008:
          try
          {
            if (filamentprofile_list.Selected >= 0 && filamentprofile_list.Selected < filamentprofile_list.Items.Count)
            {
              var obj = filamentprofile_list.Items[filamentprofile_list.Selected];
              if (obj is FilamentProfilePage.FilamentOptions)
              {
                var filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
                var num = float.Parse(temperature_edit.Text);
                FilamentConstants.Temperature.MaxMin maxMin = FilamentConstants.Temperature.MaxMinForFilamentType(filamentOptions.Key.type);
                if ((double) num >= (double) maxMin.Min && (double) num <= (double) maxMin.Max)
                {
                  UpdateTemperature((int) num);
                  temperature_edit.Enabled = false;
                  TemperatureEditButton.Visible = true;
                  TemperatureSaveButton.Visible = false;
                  TemperatureResetButton.Visible = true;
                }
                else
                {
                  messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Please enter a temperature from " + (object) maxMin.Min + " to " + (object) maxMin.Max));
                }
              }
            }
            UpdateProfileList(filamentprofile_list.Selected);
            break;
          }
          catch (Exception ex)
          {
            messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, "Sorry. The temperature you entered is invalid."));
            break;
          }
        case 1010:
          var temperature = FilamentConstants.Temperature.Default(spooler_connection.SelectedPrinter.GetCurrentFilament().filament_type);
          UpdateTemperature(temperature);
          temperature_edit.Text = temperature.ToString();
          break;
      }
    }

    public void FilamentSettingsFrameButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 1100:
          settingsManager.CurrentFilamentSettings.TrackFilament = track_filament.Checked;
          break;
        case 1102:
          settingsManager.CurrentFilamentSettings.CleanNozzleAfterInsert = clean_nozzle.Checked;
          break;
      }
    }

    public void OnUserSelection(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
      {
        return;
      }

      RemoveFilamentProfile();
    }

    private void RemoveFilamentProfile()
    {
      var obj = filamentprofile_list.Items[filamentprofile_list.Selected];
      if (obj is FilamentProfilePage.FilamentOptions)
      {
        var filamentOptions = (FilamentProfilePage.FilamentOptions) obj;
        settingsManager.FilamentDictionary.RemoveCustomTemperature(filamentOptions.Key.type, filamentOptions.Key.color);
      }
      UpdateProfileList();
    }

    private void UpdateTemperature(int temperature)
    {
      if (prevSelectedProfile < 0 || prevSelectedProfile >= filamentprofile_list.Items.Count)
      {
        return;
      }

      var obj = filamentprofile_list.Items[prevSelectedProfile];
      if (!(obj is FilamentProfilePage.FilamentOptions))
      {
        return;
      }

      UpdateFilamentProfile(((FilamentProfilePage.FilamentOptions) obj).Key, new FilamentProfile.CustomOptions()
      {
        temperature = temperature
      });
    }

    private void UpdateFilamentProfile(FilamentProfile.TypeColorKey key, FilamentProfile.CustomOptions options)
    {
      settingsManager.FilamentDictionary.AddCustomTemperature(key.type, key.color, options.temperature);
      spooler_connection.CheckUpdatedFilamentProfile(key, options);
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
        return ((int)Key.type).ToString() + " " + (object)Key.color;
      }
    }
  }
}
