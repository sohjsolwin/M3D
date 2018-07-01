// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentRemoveInternalSpoolInstructions
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentRemoveInternalSpoolInstructions : Manage3DInkChildWindow
  {
    public FilamentRemoveInternalSpoolInstructions(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 8)
        return;
      this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page1_HeatingNozzle, this.CurrentDetails);
    }

    public override void Init()
    {
      this.RelativeX = 0.0f;
      this.RelativeY = 0.0f;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      TextWidget textWidget1 = new TextWidget(1);
      textWidget1.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget1.Text = "Open Print Bed to Remove Filament";
      textWidget1.RelativeWidth = 1f;
      textWidget1.Size = FontSize.Medium;
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.SetPosition(0, 25);
      this.AddChildElement((Element2D) textWidget1);
      Frame frame = new Frame(2);
      frame.SetPosition(0, 50);
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.75f;
      frame.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      frame.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      this.AddChildElement((Element2D) frame);
      Sprite.pixel_perfect = true;
      ImageWidget imageWidget1 = new ImageWidget(0);
      imageWidget1.Init(this.Host, "extendedcontrols", 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget1.SetSize(120, 102);
      imageWidget1.SetPosition(10, 5);
      frame.AddChildElement((Element2D) imageWidget1);
      TextWidget textWidget2 = new TextWidget(1);
      textWidget2.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget2.Text = "1. Push the print bed backwards to unlock it.";
      textWidget2.SetSize(380, 100);
      textWidget2.Size = FontSize.Medium;
      textWidget2.Alignment = QFontAlignment.Left;
      textWidget2.VAlignment = TextVerticalAlignment.Top;
      textWidget2.SetPosition(140, 5);
      frame.AddChildElement((Element2D) textWidget2);
      ImageWidget imageWidget2 = new ImageWidget(0);
      imageWidget2.Init(this.Host, "extendedcontrols", 0.0f, 613f, 119f, 713f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget2.SetSize(120, 102);
      imageWidget2.SetPosition(10, 110);
      frame.AddChildElement((Element2D) imageWidget2);
      TextWidget textWidget3 = new TextWidget(1);
      textWidget3.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget3.Text = "2. Lift the print bed up to reveal the compartment underneath.";
      textWidget3.SetSize(380, 100);
      textWidget3.Size = FontSize.Medium;
      textWidget3.Alignment = QFontAlignment.Left;
      textWidget3.VAlignment = TextVerticalAlignment.Top;
      textWidget3.SetPosition(140, 110);
      frame.AddChildElement((Element2D) textWidget3);
      ImageWidget imageWidget3 = new ImageWidget(0);
      imageWidget3.Init(this.Host, "extendedcontrols", 120f, 714f, 239f, 814f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget3.SetSize(120, 102);
      imageWidget3.SetPosition(10, 215);
      frame.AddChildElement((Element2D) imageWidget3);
      TextWidget textWidget4 = new TextWidget(1);
      textWidget4.Color = new Color4(0.35f, 0.35f, 0.35f, 1f);
      textWidget4.Text = "3. Remove the filament spool from the compartment, but do not pull the filament from the filament tube yet.";
      textWidget4.SetSize(380, 100);
      textWidget4.Size = FontSize.Medium;
      textWidget4.Alignment = QFontAlignment.Left;
      textWidget4.VAlignment = TextVerticalAlignment.Top;
      textWidget4.SetPosition(140, 215);
      frame.AddChildElement((Element2D) textWidget4);
      Sprite.pixel_perfect = false;
      ButtonWidget buttonWidget = new ButtonWidget(8);
      buttonWidget.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "Next";
      buttonWidget.SetGrowableWidth(4, 4, 32);
      buttonWidget.SetGrowableHeight(4, 4, 32);
      buttonWidget.SetSize(100, 32);
      buttonWidget.SetPosition(400, -50);
      buttonWidget.RelativeX = 0.8f;
      buttonWidget.RelativeY = -1000f;
      buttonWidget.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }
  }
}
