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
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page11_CheatCodePage, this.CurrentDetails);
          break;
        case 8:
          FilamentConstants.ColorsEnum filamentColors = FilamentConstants.StringToFilamentColors(this.color_combobox.EditBox.Text);
          for (int index = 0; index < this.CurrentDetails.user_filaments.Count; ++index)
          {
            if (this.CurrentDetails.user_filaments[index].Color != filamentColors)
            {
              if (this.CurrentDetails.user_filaments[index].Color == FilamentConstants.ColorsEnum.Other)
                this.CurrentDetails.user_filaments[index] = new M3D.Spooling.Common.Filament(this.CurrentDetails.user_filaments[0].Type, filamentColors, this.CurrentDetails.user_filaments[0].CodeStr, this.CurrentDetails.user_filaments[0].Brand);
              else if (this.CurrentDetails.user_filaments.Count == 1)
              {
                this.CurrentDetails.user_filaments.RemoveAt(index);
                --index;
              }
            }
          }
          M3D.Spooling.Common.Filament filament;
          int temperature;
          if (!this.settingsManager.FilamentDictionary.ResolveToEncodedFilament(this.CurrentDetails.user_filaments[0].CodeStr, out filament, out temperature))
            temperature = this.settingsManager.GetFilamentTemperature(this.CurrentDetails.current_spool.filament_type, filamentColors);
          this.CurrentDetails.current_spool.filament_color_code = (uint) filamentColors;
          this.CurrentDetails.current_spool.filament_temperature = temperature;
          if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.AddFilament)
            this.CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew;
          else if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.SetDetails)
            this.CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationAlreadyInserted;
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page18_FilamentSpoolSize, this.CurrentDetails);
          break;
      }
    }

    public override void Init()
    {
      this.CreateManageFilamentFrame("Select 3D Ink Color", "", false, false, false, false, true, true);
      Frame childElement = (Frame) this.FindChildElement(2);
      if (childElement == null)
        return;
      TextWidget textWidget = new TextWidget(11);
      textWidget.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget.RelativeWidth = 1f;
      textWidget.RelativeHeight = 0.2f;
      textWidget.X = 0;
      textWidget.Y = 20;
      textWidget.Alignment = QFontAlignment.Centre;
      textWidget.VAlignment = TextVerticalAlignment.Top;
      textWidget.Text = "What Color is your filament?";
      childElement.AddChildElement((Element2D) textWidget);
      this.color_combobox = new ComboBoxWidget(12);
      this.color_combobox.Init(this.Host);
      this.color_combobox.Select = 0;
      this.color_combobox.SetPosition(30, 60);
      this.color_combobox.SetSize(336, 32);
      this.color_combobox.CenterHorizontallyInParent = true;
      childElement.AddChildElement((Element2D) this.color_combobox);
      this.color_combobox.ListBox.Items = this.settingsManager.FilamentDictionary.GenerateColors(FilamentSpool.TypeEnum.NoFilament).Cast<object>().ToList<object>();
      this.color_combobox.ListBox.Items.Sort();
      this.color_combobox.Select = 0;
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.color_combobox.ListBox.Items.Clear();
      this.color_combobox.ListBox.Items = this.settingsManager.FilamentDictionary.GenerateColors(FilamentSpool.TypeEnum.NoFilament).Cast<object>().ToList<object>();
      if (this.CurrentDetails.user_filaments.Count > 1)
      {
        foreach (M3D.Spooling.Common.Filament userFilament in this.CurrentDetails.user_filaments)
        {
          if (!this.color_combobox.ListBox.Items.Contains((object) userFilament.ColorStr))
            this.color_combobox.ListBox.Items.Add((object) userFilament.ColorStr);
        }
      }
      this.color_combobox.ListBox.Items.Sort();
      this.color_combobox.Select = 0;
    }

    public enum ControlIDs
    {
      TextColor = 11, // 0x0000000B
      ColorComboBox = 12, // 0x0000000C
    }
  }
}
