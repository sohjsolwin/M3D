using M3D.Graphics.Widgets2D;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class BorderedImageFrame : Frame
  {
    [XmlAttribute("src")]
    private string texture;
    [XmlAttribute("texture-u0")]
    private float u0;
    [XmlAttribute("texture-v0")]
    private float v0;
    [XmlAttribute("texture-u1")]
    private float u1;
    [XmlAttribute("texture-v1")]
    private float v1;
    [XmlAttribute("leftbordersize-pixels")]
    private int leftbordersize_pixels;
    [XmlAttribute("rightbordersize-pixels")]
    private int rightbordersize_pixels;
    [XmlAttribute("minimumwidth")]
    private int minimumwidth;
    [XmlAttribute("topbordersize-pixels")]
    private int topbordersize_pixels;
    [XmlAttribute("bottombordersize-pixels")]
    private int bottombordersize_pixels;
    [XmlAttribute("minimumheight")]
    private int minimumheight;

    public BorderedImageFrame()
      : this(0)
    {
    }

    public BorderedImageFrame(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public BorderedImageFrame(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth, int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      var imageWidget = new ImageWidget(ID);
      imageWidget.SetPosition(0, 0);
      imageWidget.RelativeWidth = 1f;
      imageWidget.RelativeHeight = 1f;
      imageWidget.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1);
      imageWidget.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      imageWidget.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      AddFirstChild((Element2D) imageWidget);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      Parent = parent;
      base.InitChildren(parent, host, MyButtonCallback);
      if ((double)u0 == 0.0 && (double)v0 == 0.0 && ((double)u1 == 0.0 && (double)v1 == 0.0))
      {
        texture = "guicontrols";
        u0 = 640f;
        v0 = 320f;
        u1 = 704f;
        v1 = 383f;
        leftbordersize_pixels = 41;
        rightbordersize_pixels = 8;
        minimumwidth = 64;
        topbordersize_pixels = 35;
        bottombordersize_pixels = 8;
        minimumheight = 64;
      }
      Init(host, texture, u0, v0, u1, v1, leftbordersize_pixels, rightbordersize_pixels, minimumwidth, topbordersize_pixels, bottombordersize_pixels, minimumheight);
    }
  }
}
