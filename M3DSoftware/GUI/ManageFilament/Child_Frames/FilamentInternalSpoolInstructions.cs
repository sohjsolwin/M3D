// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentInternalSpoolInstructions
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
  internal class FilamentInternalSpoolInstructions : Manage3DInkChildWindow
  {
    public FilamentInternalSpoolInstructions(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 8:
          MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page1_HeatingNozzle, CurrentDetails);
          break;
        case 9:
          MainWindow.ResetToStartup();
          break;
      }
    }

    public override void Init()
    {
      RelativeX = 0.0f;
      RelativeY = 0.0f;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      var textWidget1 = new TextWidget(1)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        Text = "Load Filament into Internal Port",
        RelativeWidth = 1f,
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Centre
      };
      textWidget1.SetPosition(0, 25);
      AddChildElement((Element2D) textWidget1);
      var frame = new Frame(2);
      frame.SetPosition(0, 50);
      frame.RelativeWidth = 1f;
      frame.RelativeHeight = 0.75f;
      frame.BGColor = new Color4((byte) 246, (byte) 246, (byte) 246, byte.MaxValue);
      frame.BorderColor = new Color4((byte) 220, (byte) 220, (byte) 220, byte.MaxValue);
      AddChildElement((Element2D) frame);
      Sprite.pixel_perfect = true;
      var imageWidget1 = new ImageWidget(0);
      imageWidget1.Init(Host, "extendedcontrols", 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget1.SetSize(120, 102);
      imageWidget1.SetPosition(10, 5);
      frame.AddChildElement((Element2D) imageWidget1);
      var textWidget2 = new TextWidget(1)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        Text = "1. Push the print bed backwards to unlock it."
      };
      textWidget2.SetSize(380, 100);
      textWidget2.Size = FontSize.Medium;
      textWidget2.Alignment = QFontAlignment.Left;
      textWidget2.VAlignment = TextVerticalAlignment.Top;
      textWidget2.SetPosition(140, 5);
      frame.AddChildElement((Element2D) textWidget2);
      var imageWidget2 = new ImageWidget(0);
      imageWidget2.Init(Host, "extendedcontrols", 0.0f, 613f, 119f, 713f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget2.SetSize(120, 102);
      imageWidget2.SetPosition(10, 110);
      frame.AddChildElement((Element2D) imageWidget2);
      var textWidget3 = new TextWidget(1)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        Text = "2. Lift the print bed up to reveal the compartment underneath."
      };
      textWidget3.SetSize(380, 100);
      textWidget3.Size = FontSize.Medium;
      textWidget3.Alignment = QFontAlignment.Left;
      textWidget3.VAlignment = TextVerticalAlignment.Top;
      textWidget3.SetPosition(140, 110);
      frame.AddChildElement((Element2D) textWidget3);
      var imageWidget3 = new ImageWidget(0);
      imageWidget3.Init(Host, "extendedcontrols", 0.0f, 714f, 119f, 814f, 0.0f, 512f, 119f, 612f, 0.0f, 512f, 119f, 612f);
      imageWidget3.SetSize(120, 102);
      imageWidget3.SetPosition(10, 215);
      frame.AddChildElement((Element2D) imageWidget3);
      var textWidget4 = new TextWidget(1)
      {
        Color = new Color4(0.35f, 0.35f, 0.35f, 1f),
        Text = "3. Unwrap one meter of filament from a genuine M3D spool so that it comes out counter clockwise and feed it into the filament tube all the way."
      };
      textWidget4.SetSize(380, 100);
      textWidget4.Size = FontSize.Medium;
      textWidget4.Alignment = QFontAlignment.Left;
      textWidget4.VAlignment = TextVerticalAlignment.Top;
      textWidget4.SetPosition(140, 215);
      frame.AddChildElement((Element2D) textWidget4);
      Sprite.pixel_perfect = false;
      var buttonWidget1 = new ButtonWidget(8);
      buttonWidget1.Init(Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget1.Size = FontSize.Medium;
      buttonWidget1.Text = "Next";
      buttonWidget1.SetGrowableWidth(4, 4, 32);
      buttonWidget1.SetGrowableHeight(4, 4, 32);
      buttonWidget1.SetSize(100, 32);
      buttonWidget1.SetPosition(400, -50);
      buttonWidget1.RelativeX = 0.8f;
      buttonWidget1.RelativeY = -1000f;
      buttonWidget1.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      AddChildElement((Element2D) buttonWidget1);
      var buttonWidget2 = new ButtonWidget(9);
      buttonWidget2.Init(Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget2.Size = FontSize.Medium;
      buttonWidget2.Text = "Cancel";
      buttonWidget2.SetGrowableWidth(4, 4, 32);
      buttonWidget2.SetGrowableHeight(4, 4, 32);
      buttonWidget2.SetSize(110, 40);
      buttonWidget2.SetPosition(20, -50);
      buttonWidget2.RelativeX = 0.025f;
      buttonWidget2.RelativeY = -1000f;
      buttonWidget2.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      AddChildElement((Element2D) buttonWidget2);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
    }
  }
}
