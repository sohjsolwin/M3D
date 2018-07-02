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
      : this(ID, null)
    {
    }

    public ProgressBarWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      drawable_sprite = new Sprite();
      bar_color = new Color4(0.5f, 0.9f, 1f, 1f);
      complete = 0.0f;
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth, int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      Sprite.pixel_perfect = false;
      drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1, normal_u0, normal_v0, normal_u1, normal_v1);
      drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
      drawable_sprite.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
      left_border = leftbordersize_pixels;
      right_border = rightbordersize_pixels;
      top_border = topbordersize_pixels;
      bottom_border = bottombordersize_pixels;
      minimum_width_pixels = minimumwidth;
      minimum_height_pixels = minimumheight;
    }

    public override void OnRender(GUIHost host)
    {
      var xAbs = X_Abs;
      var yAbs = Y_Abs;
      var width = Width;
      var height = Height;
      drawable_sprite.Render(host, State.Normal, xAbs, yAbs, width, height);
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      Simple2DRenderer.Quad quad1;
      quad1.x0 = xAbs + left_border;
      quad1.y0 = yAbs + top_border;
      quad1.x1 = xAbs + (width - right_border) * complete;
      quad1.y1 = yAbs + height - bottom_border;
      quad1.color = bar_color;
      Simple2DRenderer.Quad quad2 = quad1;
      simpleRenderer.DrawQuad(quad2);
      base.OnRender(host);
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      Init(host, ImageSrc, u0, v0, u1, v1, left_border, right_border, minimum_width_pixels, top_border, bottom_border, minimum_height_pixels);
    }

    [XmlAttribute("percent")]
    public float PercentComplete
    {
      get
      {
        return complete;
      }
      set
      {
        complete = value;
        if (complete < 0.0)
        {
          complete = 0.0f;
        }

        if (complete <= 1.0)
        {
          return;
        }

        complete = 1f;
      }
    }

    [XmlAttribute("bar-color")]
    public string HexBarColor
    {
      get
      {
        return hexBarColor;
      }
      set
      {
        hexBarColor = value;
        bar_color = IElement.GenerateColorFromHtml(hexBarColor);
      }
    }

    [XmlIgnore]
    public Color4 BarColor
    {
      get
      {
        return bar_color;
      }
      set
      {
        bar_color = value;
      }
    }
  }
}
