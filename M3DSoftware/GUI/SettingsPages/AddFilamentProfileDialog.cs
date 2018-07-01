// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.AddFilamentProfileDialog
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
using System;

namespace M3D.GUI.SettingsPages
{
  public class AddFilamentProfileDialog : BorderedImageFrame
  {
    private ComboBoxWidget type_combobox;
    private ComboBoxWidget color_combobox;
    private ButtonWidget add_button;
    private ButtonWidget cancel_button;
    private SettingsManager settings_manager;
    private GUIHost host;
    private FilamentProfilePage manageFilamentPage;

    public AddFilamentProfileDialog(int ID, SettingsManager mainLogicController)
      : base(ID, (Element2D) null)
    {
      this.settings_manager = mainLogicController;
      this.manageFilamentPage = (FilamentProfilePage) null;
    }

    public AddFilamentProfileDialog(int ID, SettingsManager mainLogicController, FilamentProfilePage manageFilamentPage)
      : base(ID, (Element2D) null)
    {
      this.settings_manager = mainLogicController;
      this.manageFilamentPage = manageFilamentPage;
    }

    public void Init(GUIHost host)
    {
      this.host = host;
      this.Init(host, "guicontrols", 640f, 320f, 704f, 383f, 41, 8, 64, 35, 8, 64);
      Sprite.pixel_perfect = false;
      TextWidget textWidget1 = new TextWidget(0);
      textWidget1.SetPosition(50, 2);
      textWidget1.SetSize(500, 35);
      textWidget1.Text = "Add Filament Profile";
      textWidget1.Alignment = QFontAlignment.Left;
      textWidget1.Size = FontSize.Medium;
      textWidget1.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget1);
      TextWidget textWidget2 = new TextWidget(0);
      textWidget2.Text = "Type";
      textWidget2.Size = FontSize.Medium;
      textWidget2.Alignment = QFontAlignment.Left;
      textWidget2.SetPosition(30, 50);
      textWidget2.SetSize(200, 24);
      textWidget2.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.AddChildElement((Element2D) textWidget2);
      TextWidget textWidget3 = new TextWidget(0);
      textWidget3.Text = "Color";
      textWidget3.Size = FontSize.Medium;
      textWidget3.Alignment = QFontAlignment.Left;
      textWidget3.SetPosition(30, 110);
      textWidget3.SetSize(50, 24);
      textWidget3.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      textWidget3.IgnoreMouse = true;
      this.AddChildElement((Element2D) textWidget3);
      this.type_combobox = new ComboBoxWidget(0);
      this.type_combobox.Init(host);
      this.type_combobox.ListBox.SetOnChangeCallback(new ListBoxWidget.OnChangeCallback(this.MyOnChangeFilamentTypeCallback));
      this.type_combobox.Select = 0;
      this.type_combobox.SetPosition(30, 80);
      this.type_combobox.SetSize(256, 24);
      foreach (FilamentSpool.TypeEnum typeEnum in (FilamentSpool.TypeEnum[]) Enum.GetValues(typeof (FilamentSpool.TypeEnum)))
        this.type_combobox.AddItem((object) typeEnum.ToString());
      this.type_combobox.Select = 0;
      this.type_combobox.tabIndex = 1;
      this.AddChildElement((Element2D) this.type_combobox);
      this.color_combobox = new ComboBoxWidget(0);
      this.color_combobox.Init(host);
      this.color_combobox.ListBox.SetOnChangeCallback(new ListBoxWidget.OnChangeCallback(this.MyOnChangeColorCallback));
      this.color_combobox.Select = 0;
      this.color_combobox.SetPosition(30, 140);
      this.color_combobox.SetSize(256, 24);
      this.color_combobox.tabIndex = 2;
      this.AddChildElement((Element2D) this.color_combobox);
      this.AddColorItems(FilamentSpool.TypeEnum.ABS);
      this.add_button = new ButtonWidget(3);
      this.add_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.add_button.Size = FontSize.Medium;
      this.add_button.Text = "Add";
      this.add_button.SetGrowableWidth(4, 4, 32);
      this.add_button.SetGrowableHeight(4, 4, 32);
      this.add_button.SetSize(80, 32);
      this.add_button.SetPosition(70, -50);
      this.add_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.add_button.tabIndex = 4;
      this.AddChildElement((Element2D) this.add_button);
      this.cancel_button = new ButtonWidget(4);
      this.cancel_button.Init(host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      this.cancel_button.Size = FontSize.Medium;
      this.cancel_button.Text = "Cancel";
      this.cancel_button.SetGrowableWidth(4, 4, 32);
      this.cancel_button.SetGrowableHeight(4, 4, 32);
      this.cancel_button.SetSize(100, 32);
      this.cancel_button.SetPosition(160, -50);
      this.cancel_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.cancel_button.tabIndex = 5;
      this.AddChildElement((Element2D) this.cancel_button);
      Sprite.pixel_perfect = false;
    }

    private void AddColorItems(FilamentSpool.TypeEnum type)
    {
      this.color_combobox.ClearItems();
      foreach (FilamentConstants.ColorsEnum color in (FilamentConstants.ColorsEnum[]) Enum.GetValues(typeof (FilamentConstants.ColorsEnum)))
        this.color_combobox.AddItem((object) FilamentConstants.ColorsToString(color));
      this.color_combobox.Select = 0;
    }

    public void MyOnChangeFilamentTypeCallback(ListBoxWidget listBox)
    {
      this.AddColorItems(((FilamentSpool.TypeEnum[]) Enum.GetValues(typeof (FilamentSpool.TypeEnum)))[listBox.Selected]);
    }

    public void MyOnChangeColorCallback(ListBoxWidget listBox)
    {
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 3:
          FilamentSpool.TypeEnum type = ((FilamentSpool.TypeEnum[]) Enum.GetValues(typeof (FilamentSpool.TypeEnum)))[this.type_combobox.ListBox.Selected];
          FilamentConstants.ColorsEnum filamentColors = FilamentConstants.StringToFilamentColors(this.color_combobox.EditBox.Text);
          this.settings_manager.FilamentDictionary.AddCustomTemperature(type, filamentColors, FilamentConstants.Temperature.Default(type));
          this.Enabled = false;
          this.Visible = false;
          this.host.GlobalChildDialog -= (Element2D) this;
          if (this.manageFilamentPage != null)
            this.manageFilamentPage.Enabled = true;
          if (this.manageFilamentPage == null)
            break;
          this.manageFilamentPage.UpdateProfileList(type, filamentColors);
          break;
        case 4:
          this.Enabled = false;
          this.Visible = false;
          if (this.manageFilamentPage != null)
            this.manageFilamentPage.Enabled = true;
          this.host.GlobalChildDialog -= (Element2D) this;
          break;
      }
    }

    private enum ControlIDs
    {
      TypeEdit,
      ColorEdit,
      RemainingEdit,
      AddButton,
      CancelButton,
    }
  }
}
