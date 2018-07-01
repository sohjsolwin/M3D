// Decompiled with JetBrains decompiler
// Type: M3D.GUI.ManageFilament.Child_Frames.FilamentCleanNozzle
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;
using System.Diagnostics;

namespace M3D.GUI.ManageFilament.Child_Frames
{
  internal class FilamentCleanNozzle : Manage3DInkChildWindow
  {
    private Stopwatch _continueTimer = new Stopwatch();
    private TextWidget clean_nozzle_timer_text;

    public FilamentCleanNozzle(int ID, GUIHost host, Manage3DInkMainWindow mainWindow)
      : base(ID, host, mainWindow)
    {
    }

    public override void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID != 14)
        return;
      this.Continue();
    }

    public override void Init()
    {
      FontSize fontSize = FontSize.Medium;
      Color4 color4_1 = new Color4(0.35f, 0.35f, 0.35f, 1f);
      Color4 color4_2 = new Color4(0.35f, 0.35f, 0.35f, 1f);
      this.RelativeX = 0.0f;
      this.RelativeY = 0.0f;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      TextWidget textWidget1 = new TextWidget(11);
      textWidget1.Color = color4_1;
      textWidget1.Text = "Clean Nozzle Head";
      textWidget1.RelativeWidth = 1f;
      textWidget1.Size = fontSize;
      textWidget1.Alignment = QFontAlignment.Centre;
      textWidget1.SetPosition(0, 50);
      this.AddChildElement((Element2D) textWidget1);
      TextWidget textWidget2 = new TextWidget(12);
      textWidget2.Size = FontSize.Medium;
      textWidget2.Alignment = QFontAlignment.Centre;
      textWidget2.VAlignment = TextVerticalAlignment.Middle;
      textWidget2.Text = "The nozzle may contain unwanted filament.\nPlease remove any excess material from the nozzle.\nBe careful. The nozzle might be hot.";
      textWidget2.Color = color4_2;
      textWidget2.SetPosition(0, 75);
      textWidget2.SetSize(480, 80);
      textWidget2.CenterHorizontallyInParent = true;
      textWidget2.Visible = true;
      this.AddChildElement((Element2D) textWidget2);
      TextWidget textWidget3 = new TextWidget(0);
      textWidget3.Size = FontSize.Medium;
      textWidget3.Alignment = QFontAlignment.Centre;
      textWidget3.VAlignment = TextVerticalAlignment.Middle;
      textWidget3.Text = "Continuing in";
      textWidget3.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      textWidget3.SetSize(140, 30);
      textWidget3.SetPosition(327, 193);
      this.AddChildElement((Element2D) textWidget3);
      SpriteAnimationWidget spriteAnimationWidget = new SpriteAnimationWidget(1);
      spriteAnimationWidget.Init(this.Host, "guicontrols", 0.0f, 768f, 767f, 1023f, 6, 2, 12, 200U);
      spriteAnimationWidget.SetSize(128, 108);
      spriteAnimationWidget.SetPosition(488, 150);
      this.AddChildElement((Element2D) spriteAnimationWidget);
      this.clean_nozzle_timer_text = new TextWidget(13);
      this.clean_nozzle_timer_text.Size = FontSize.VeryLarge;
      this.clean_nozzle_timer_text.Alignment = QFontAlignment.Centre;
      this.clean_nozzle_timer_text.VAlignment = TextVerticalAlignment.Middle;
      this.clean_nozzle_timer_text.Text = "30";
      this.clean_nozzle_timer_text.SetSize(128, 108);
      this.clean_nozzle_timer_text.SetPosition(488, 150);
      this.clean_nozzle_timer_text.Color = new Color4((byte) 100, (byte) 100, (byte) 100, byte.MaxValue);
      this.AddChildElement((Element2D) this.clean_nozzle_timer_text);
      ButtonWidget buttonWidget = new ButtonWidget(14);
      buttonWidget.Init(this.Host, "guicontrols", 896f, 192f, 959f, (float) byte.MaxValue, 896f, 256f, 959f, 319f, 896f, 320f, 959f, 383f, 960f, 128f, 1023f, 191f);
      buttonWidget.Size = FontSize.Medium;
      buttonWidget.Text = "Continue";
      buttonWidget.SetGrowableWidth(4, 4, 32);
      buttonWidget.SetGrowableHeight(4, 4, 32);
      buttonWidget.SetSize(110, 40);
      buttonWidget.SetPosition(550, -50);
      buttonWidget.SetCallback(new ButtonCallback(((Manage3DInkChildWindow) this).MyButtonCallback));
      this.AddChildElement((Element2D) buttonWidget);
      ImageWidget imageWidget = new ImageWidget(0);
      imageWidget.Init(this.Host, "extendedcontrols2", 696f, 100f, 1020f, 350f, 696f, 100f, 1020f, 350f, 696f, 100f, 1020f, 350f);
      imageWidget.SetSize(320, 256);
      imageWidget.SetPosition(0, 140);
      imageWidget.Visible = true;
      this.AddChildElement((Element2D) imageWidget);
    }

    public override void OnActivate(Mangage3DInkStageDetails details)
    {
      base.OnActivate(details);
      this._continueTimer.Restart();
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (!this.Visible || !this._continueTimer.IsRunning)
        return;
      int num = 30 - this._continueTimer.Elapsed.Seconds;
      if (num < 0)
      {
        this._continueTimer.Stop();
        num = 0;
        this.Continue();
      }
      this.clean_nozzle_timer_text.Text = num.ToString();
    }

    private void Continue()
    {
      FilamentWaitingPage.CurrentWaitingText = "Please wait. The printer is busy perfoming the requested actions.";
      this.MainWindow.ActivateFrame(Manage3DInkMainWindow.PageID.Page8_WaitingPage, new Mangage3DInkStageDetails(Manage3DInkMainWindow.Mode.None, Manage3DInkMainWindow.PageID.Page0_StartupPage));
      this._continueTimer.Stop();
    }

    public enum ControlIDs
    {
      TitleText = 11, // 0x0000000B
      DirectionsText = 12, // 0x0000000C
      CleanNozzleTimerText = 13, // 0x0000000D
      ContinueButton = 14, // 0x0000000E
    }
  }
}
