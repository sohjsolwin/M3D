// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.ProgressBarWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class ProgressBarWidget : Element2D
  {
    private string hexBarColor = "#FF808080";
    [XmlAttribute("src")]
    public string ImageSrc = "";
    [XmlAttribute("left-border-size")]
    public int left_border;
    [XmlAttribute("right-border-size")]
    public int right_border;
    [XmlAttribute("top-border-size")]
    public int top_border;
    [XmlAttribute("bottom-border-size")]
    public int bottom_border;
    [XmlAttribute("texture-u0")]
    public float u0;
    [XmlAttribute("texture-v0")]
    public float v0;
    [XmlAttribute("texture-u1")]
    public float u1;
    [XmlAttribute("texture-v1")]
    public float v1;
    [XmlAttribute("minimumwidth")]
    public int minimum_width_pixels;
    [XmlAttribute("minimumheight")]
    public int minimum_height_pixels;
    private Color4 bar_color;
    private Sprite drawable_sprite;
    private float complete;

    public ProgressBarWidget()
      : this(0)
    {
    }

    public ProgressBarWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public ProgressBarWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.drawable_sprite = new Sprite();
      this.bar_color = new Color4(0.5f, 0.9f, 1f, 1f);
      this.complete = 0.0f;
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth, int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      Sprite.pixel_perfect = false;
      this.drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
      this.drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      this.drawable_sprite.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      this.left_border = leftbordersize_pixels;
      this.right_border = rightbordersize_pixels;
      this.top_border = topbordersize_pixels;
      this.bottom_border = bottombordersize_pixels;
      this.minimum_width_pixels = minimumwidth;
      this.minimum_height_pixels = minimumheight;
    }

    public override void OnRender(GUIHost host)
    {
      int xAbs = this.X_Abs;
      int yAbs = this.Y_Abs;
      int width = this.Width;
      int height = this.Height;
      this.drawable_sprite.Render(host, State.Normal, xAbs, yAbs, width, height);
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      Simple2DRenderer.Quad quad1;
      quad1.x0 = (float) (xAbs + this.left_border);
      quad1.y0 = (float) (yAbs + this.top_border);
      quad1.x1 = (float) xAbs + (float) (width - this.right_border) * this.complete;
      quad1.y1 = (float) (yAbs + height - this.bottom_border);
      quad1.color = this.bar_color;
      Simple2DRenderer.Quad quad2 = quad1;
      simpleRenderer.DrawQuad(quad2);
      base.OnRender(host);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      this.Init(host, this.ImageSrc, this.u0, this.v0, this.u1, this.v1, this.left_border, this.right_border, this.minimum_width_pixels, this.top_border, this.bottom_border, this.minimum_height_pixels);
    }

    [XmlAttribute("percent")]
    public float PercentComplete
    {
      get
      {
        return this.complete;
      }
      set
      {
        this.complete = value;
        if ((double) this.complete < 0.0)
          this.complete = 0.0f;
        if ((double) this.complete <= 1.0)
          return;
        this.complete = 1f;
      }
    }

    [XmlAttribute("bar-color")]
    public string HexBarColor
    {
      get
      {
        return this.hexBarColor;
      }
      set
      {
        this.hexBarColor = value;
        this.bar_color = IElement.GenerateColorFromHtml(this.hexBarColor);
      }
    }

    [XmlIgnore]
    public Color4 BarColor
    {
      get
      {
        return this.bar_color;
      }
      set
      {
        this.bar_color = value;
      }
    }
  }
}
