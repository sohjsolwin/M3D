// Decompiled with JetBrains decompiler
// Type: M3D.GUI.SettingsPages.AboutPage
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.Spooling;
using OpenTK.Graphics;
using QuickFont;

namespace M3D.GUI.SettingsPages
{
  public class AboutPage : SettingsPage
  {
    public AboutPage(int ID)
      : base(ID)
    {
    }

    public void Init(GUIHost host)
    {
      ScrollFrame scrollFrame = new ScrollFrame(0, (Element2D) this);
      scrollFrame.Init(host);
      scrollFrame.RelativeWidth = 1f;
      scrollFrame.RelativeHeight = 1f;
      this.AddChildElement((Element2D) scrollFrame);
      ImageWidget imageWidget = new ImageWidget(0, (Element2D) scrollFrame);
      imageWidget.Init(host, "guicontrols", 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f);
      imageWidget.Width = 129;
      imageWidget.Height = 31;
      imageWidget.Y = 10;
      imageWidget.X = 10;
      imageWidget.SandBoxing = false;
      scrollFrame.AddChildElement((Element2D) imageWidget);
      string versionText = Version.VersionText;
      TextWidget textWidget = new TextWidget(0, (Element2D) scrollFrame);
      textWidget.Text = Locale.GlobalLocale.T("T_AboutText") + versionText;
      textWidget.Size = FontSize.Medium;
      textWidget.Alignment = QFontAlignment.Left;
      textWidget.VAlignment = TextVerticalAlignment.Top;
      textWidget.Color = new Color4(0.5f, 0.5f, 0.5f, 1f);
      textWidget.SetPosition(10, 60);
      textWidget.Height = 300;
      textWidget.RelativeWidth = 0.8f;
      scrollFrame.AddChildElement((Element2D) textWidget);
      textWidget.IgnoreMouse = false;
    }
  }
}
