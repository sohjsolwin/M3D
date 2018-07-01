// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentCheatCodePage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentCheatCodePage : Manage3DInkChildWindow
  {
    private MultiBoxEditBoxWidget CheatEdit;
    private SettingsManager settingsManager;
    private PopupMessageBox messagebox;

    public FilamentCheatCodePage(int ID, GUIHost host, Manage3DInkMainWindow mainWindow, SettingsManager settingsManager, PopupMessageBox messagebox)
      : base(ID, host, mainWindow)
    {
      this.settingsManager = settingsManager;
      this.messagebox = messagebox;
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (this.MainWindow.GetSelectedPrinter() == null)
        return;
      switch (button.ID)
      {
        case 11:
          this.MainWindow.ResetToStartup();
          break;
        case 12:
          this.CheatCodeEnterCallBack(this.CheatEdit);
          break;
      }
    }

    public override void Init()
    {
      Frame frame = new Frame(1);
      Color4 color4_1 = new Color4(0.35f, 0.35f, 0.35f, 1f);
      Color4 color4_2 = new Color4(1f, 1f, 1f, 1f);
      Color4 color4_3 = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      Color4 color4_4 = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      TextWidget textWidget1 = new TextWidget(0);
      textWidget1.Color = color4_1;
      textWidget1.Text = "Set new 3D Ink information by cheat code:";
      textWidget1.RelativeWidth = 1f;
      textWidget1.Size = FontSize.Medium;
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.SetPosition(0, 40);
      this.AddChildElement((Element2D) textWidget1);
      ImageWidget imageWidget = new ImageWidget(0);
      imageWidget.Init(this.Host, "extendedcontrols", 0.0f, 256f, 290f, 381f, 0.0f, 256f, 448f, 511f, 0.0f, 256f, 448f, 511f);
      imageWidget.Width = 290;
      imageWidget.Height = 125;
      imageWidget.X = 100;
      imageWidget.Y = 40;
      imageWidget.CenterHorizontallyInParent = true;
      frame.AddChildElement((Element2D) imageWidget);
      TextWidget textWidget2 = new TextWidget(1);
      textWidget2.Text = "Enter cheat code or filament type:";
      textWidget2.SetSize(150, 100);
      textWidget2.SetPositionRelative(0.25f, 0.6f);
      textWidget2.Color = color4_1;
      frame.AddChildElement((Element2D) textWidget2);
      this.CheatEdit = new MultiBoxEditBoxWidget(13, (Element2D) null);
      this.CheatEdit.Init(this.Host, 3, 1);
      this.CheatEdit.SetSize(150, 32);
      this.CheatEdit.SetPositionRelative(0.55f, 0.75f);
      this.CheatEdit.Color = color4_1;
      this.CheatEdit.SetCallbackEnterKey(new MultiBoxEditBoxWidget.EditBoxCallback(this.CheatCodeEnterCallBack));
      frame.AddChildElement((Element2D) this.CheatEdit);
      ButtonWidget buttonWidget1 = new ButtonWidget(11);
      buttonWidget1.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget1.Size = FontSize.Medium;
      buttonWidget1.Text = "Cancel";
      buttonWidget1.SetGrowableWidth(4, 4, 32);
      buttonWidget1.SetGrowableHeight(4, 4, 32);
      buttonWidget1.SetSize(110, 40);
      buttonWidget1.SetPosition(20, -50);
      buttonWidget1.SetPositionRelative(0.025f, -1000f);
      buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget1);
      ButtonWidget buttonWidget2 = new ButtonWidget(12);
      buttonWidget2.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget2.Size = FontSize.Medium;
      buttonWidget2.Text = "Next";
      buttonWidget2.SetGrowableWidth(4, 4, 32);
      buttonWidget2.SetGrowableHeight(4, 4, 32);
      buttonWidget2.SetSize(100, 32);
      buttonWidget2.SetPosition(400, -50);
      buttonWidget2.SetPositionRelative(0.8f, -1000f);
      buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget2);
      frame.BGColor = color4_3;
      frame.BorderColor = color4_4;
      frame.SetSizeRelative(1f, 0.6f);
      frame.SetPositionRelative(0.0f, 0.15f);
      this.BGColor = color4_2;
      this.SetSizeRelative(1f, 0.9f);
      this.SetPositionRelative(0.0f, 0.05f);
      this.AddChildElement((Element2D) frame);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this.CheatEdit.Text = "";
      this.Host.SetFocus((Element2D) this.CheatEdit);
    }

    public void CheatCodeEnterCallBack(MultiBoxEditBoxWidget edit)
    {
      if (this.MainWindow.GetSelectedPrinter() == null)
        return;
      FilamentSpool filamentSpool = new FilamentSpool();
      this.CurrentDetails.user_filaments = this.settingsManager.FilamentDictionary.GetFromCheatCode(edit.Text.ToUpperInvariant());
      if (this.CurrentDetails.user_filaments == null || this.CurrentDetails.user_filaments.Count == 0)
      {
        this.messagebox.AddMessageToQueue("Please enter a valid cheat code", PopupMessageBox.MessageBoxButtons.OK);
      }
      else
      {
        FilamentSpool spool = this.CurrentDetails.user_filaments[0].ToSpool();
        spool.filament_location = FilamentSpool.Location.External;
        this.CurrentDetails.current_spool = spool;
        if (this.CurrentDetails.user_filaments.Count == 1 && this.CurrentDetails.user_filaments[0].Color == FilamentConstants.ColorsEnum.Other || this.CurrentDetails.user_filaments.Count > 1)
        {
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page7_FilamentColor, this.CurrentDetails);
        }
        else
        {
          spool.filament_temperature = this.settingsManager.GetFilamentTemperature(spool.filament_type, this.CurrentDetails.user_filaments[0].Color);
          if (this.CurrentDetails.mode == Manage3DInkMainWindow.Mode.AddFilament)
            this.CurrentDetails.mode = Manage3DInkMainWindow.Mode.SetFilamentLocationInsertingNew;
          this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page18_FilamentSpoolSize, this.CurrentDetails);
        }
      }
    }

    public enum ControlIDs
    {
      CodeBack = 11, // 0x0000000B
      CodeButton = 12, // 0x0000000C
      CheatEdit = 13, // 0x0000000D
    }
  }
}
