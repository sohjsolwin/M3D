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
      var scrollFrame = new ScrollFrame(0, (Element2D) this);
      scrollFrame.Init(host);
      scrollFrame.RelativeWidth = 1f;
      scrollFrame.RelativeHeight = 1f;
      AddChildElement((Element2D) scrollFrame);
      var imageWidget = new ImageWidget(0, (Element2D) scrollFrame);
      imageWidget.Init(host, "guicontrols", 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f, 0.0f, 737f, 128f, 767f);
      imageWidget.Width = 129;
      imageWidget.Height = 31;
      imageWidget.Y = 10;
      imageWidget.X = 10;
      imageWidget.SandBoxing = false;
      scrollFrame.AddChildElement((Element2D) imageWidget);
      var versionText = Version.VersionText;
      var textWidget = new TextWidget(0, (Element2D)scrollFrame)
      {
        Text = Locale.GlobalLocale.T("T_AboutText") + versionText,
        Size = FontSize.Medium,
        Alignment = QFontAlignment.Left,
        VAlignment = TextVerticalAlignment.Top,
        Color = new Color4(0.5f, 0.5f, 0.5f, 1f)
      };
      textWidget.SetPosition(10, 60);
      textWidget.Height = 300;
      textWidget.RelativeWidth = 0.8f;
      scrollFrame.AddChildElement((Element2D) textWidget);
      textWidget.IgnoreMouse = false;
    }
  }
}
