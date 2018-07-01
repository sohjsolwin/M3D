// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.BorderedImageFrame
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      ImageWidget imageWidget = new ImageWidget(this.ID);
      imageWidget.SetPosition(0, 0);
      imageWidget.RelativeWidth = 1f;
      imageWidget.RelativeHeight = 1f;
      imageWidget.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1);
      imageWidget.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      imageWidget.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      this.AddFirstChild((Element2D) imageWidget);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      this.Parent = parent;
      base.InitChildren(parent, host, MyButtonCallback);
      if ((double) this.u0 == 0.0 && (double) this.v0 == 0.0 && ((double) this.u1 == 0.0 && (double) this.v1 == 0.0))
      {
        this.texture = "guicontrols";
        this.u0 = 640f;
        this.v0 = 320f;
        this.u1 = 704f;
        this.v1 = 383f;
        this.leftbordersize_pixels = 41;
        this.rightbordersize_pixels = 8;
        this.minimumwidth = 64;
        this.topbordersize_pixels = 35;
        this.bottombordersize_pixels = 8;
        this.minimumheight = 64;
      }
      this.Init(host, this.texture, this.u0, this.v0, this.u1, this.v1, this.leftbordersize_pixels, this.rightbordersize_pixels, this.minimumwidth, this.topbordersize_pixels, this.bottombordersize_pixels, this.minimumheight);
    }
  }
}
