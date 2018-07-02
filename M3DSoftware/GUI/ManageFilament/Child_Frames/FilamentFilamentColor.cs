// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentFilamentColor
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;
using System.Linq;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentFilamentColor : Manage3DInkChildWindow
  {
    private SettingsManager settingsManager;
    private ComboBoxWidget color_combobox;

    public FilamentFilamentColor(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 7:
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page11_CheatCodePage, CurrentDetails);
          break;
        case 8:
          FilamentConstants.ColorsEnum filamentColors = FilamentConstants.StringToFilamentColors(color_combobox.EditBox.Text);
          for (var index = 0; index < CurrentDetails.user_filaments.Count; ++index)
          {
            if (CurrentDetails.user_filaments[index].Color != filamentColors)
            {
              if (CurrentDetails.user_filaments[index].Color == FilamentConstants.ColorsEnum.Other)
              {
                CurrentDetails.user_filaments[index] = new M3D.Spooling.Common.Filament(CurrentDetails.user_filaments[0].Type, filamentColors, CurrentDetails.user_filaments[0].CodeStr, CurrentDetails.user_filaments[0].Brand);
              }
              else if (CurrentDetails.user_filaments.Count == 1)
              {
                CurrentDetails.user_filaments.RemoveAt(index);
                --index;
              }
            }
          }
          M3D.Spooling.Common.Filament filament;
          int temperature;
          if (!settingsManager.FilamentDictionary.ResolveToEncodedFilament(CurrentDetails.user_filaments[0].CodeStr, out filament, out temperature))
          {
            temperature = settingsManager.GetFilamentTemperature(CurrentDetails.current_spool.filament_type, filamentColors);
          }

          CurrentDetails.current_spool.filament_color_code = (uint) filamentColors;
          CurrentDetails.current_spool.filament_temperature = temperature;
          if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.AddFilament)
          {
            CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew;
          }
          else if (CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetDetails)
          {
            CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted;
          }

          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page18_FilamentSpoolSize, CurrentDetails);
          break;
      }
    }

    public override void Init()
    {
      CreateManageFilamentFrame("Select 3D Ink Color", "", false, false, false, false, true, true);
      var childElement = (Frame)FindChildElement(2);
      if (childElement == null)
      {
        return;
      }

      var textWidget = new TextWidget(11)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        RelativeWidth = 1f,
        RelativeHeight = 0.2f,
        X = 0,
        Y = 20,
        Alignment = QFontAlignment.Centre,
        VAlignment = TextVerticalAlignment.Top,
        Text = "What Color is your filament?"
      };
      childElement.AddChildElement((Element2D) textWidget);
      color_combobox = new ComboBoxWidget(12);
      color_combobox.Init(Host);
      color_combobox.Select = 0;
      color_combobox.SetPosition(30, 60);
      color_combobox.SetSize(336, 32);
      color_combobox.CenterHorizontallyInParent = true;
      childElement.AddChildElement((Element2D)color_combobox);
      color_combobox.ListBox.Items = settingsManager.FilamentDictionary.GenerateColors(FilamentSpool.TypeEnum.NoFilament).Cast<object>().ToList<object>();
      color_combobox.ListBox.Items.Sort();
      color_combobox.Select = 0;
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      color_combobox.ListBox.Items.Clear();
      color_combobox.ListBox.Items = settingsManager.FilamentDictionary.GenerateColors(FilamentSpool.TypeEnum.NoFilament).Cast<object>().ToList<object>();
      if (CurrentDetails.user_filaments.Count > 1)
      {
        foreach (M3D.Spooling.Common.Filament userFilament in CurrentDetails.user_filaments)
        {
          if (!color_combobox.ListBox.Items.Contains((object) userFilament.ColorStr))
          {
            color_combobox.ListBox.Items.Add((object) userFilament.ColorStr);
          }
        }
      }
      color_combobox.ListBox.Items.Sort();
      color_combobox.Select = 0;
    }

    public enum ControlIDs
    {
      TextColor = 11, // 0x0000000B
      ColorComboBox = 12, // 0x0000000C
    }
  }
}
