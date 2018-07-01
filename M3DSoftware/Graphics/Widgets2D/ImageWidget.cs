// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.ImageWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics;
using QuickFont;
using System.Diagnostics;
using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public class ImageWidget : TextWidget
  {
    private static Stopwatch flash_timer = new Stopwatch();
    private string imageSrc = "";
    [XmlAttribute("leftbordersize-pixels")]
    public int leftbordersize_pixels;
    [XmlAttribute("rightbordersize-pixels")]
    public int rightbordersize_pixels;
    [XmlAttribute("minimumwidth")]
    public int minimum_width_pixels;
    [XmlAttribute("topbordersize-pixels")]
    public int topbordersize_pixels;
    [XmlAttribute("bottombordersize-pixels")]
    public int bottombordersize_pixels;
    [XmlAttribute("minimumheight")]
    public int minimum_height_pixels;
    private int text_area_height;
    private int image_area_width;
    private bool sand_boxing;
    private Color4 sandboxcolor;
    private float sandboxingratio;
    public State state;
    private Sprite drawable_sprite;
    private float _u0;
    private float _u1;
    private float _v0;
    private float _v1;
    private float _over_u0;
    private float _over_u1;
    private float _over_v0;
    private float _over_v1;
    private float _down_u0;
    private float _down_u1;
    private float _down_v0;
    private float _down_v1;
    private float _disabled_u0;
    private float _disabled_u1;
    private float _disabled_v0;
    private float _disabled_v1;
    [XmlIgnore]
    public bool Flashing;

    public ImageWidget()
      : this(0)
    {
    }

    public ImageWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public ImageWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.drawable_sprite = new Sprite();
      this.state = State.Normal;
      this.text_area_height = 20;
      this.image_area_width = -1;
      this.sand_boxing = false;
      this.sandboxingratio = 1f;
      this.Text = "";
      this.sandboxcolor = new Color4(0.0f, 0.0f, 0.0f, 1f);
    }

    public void CopyImageData(ImageWidget source)
    {
      this.text_area_height = source.text_area_height;
      this.image_area_width = source.image_area_width;
      this.sand_boxing = source.sand_boxing;
      this.sandboxcolor = source.sandboxcolor;
      this.sandboxingratio = source.sandboxingratio;
      this.state = source.state;
      this.drawable_sprite.CopyFrom(source.drawable_sprite);
    }

    public override ElementType GetElementType()
    {
      return ElementType.ImageWidget;
    }

    public void Init(GUIHost host, string texture, float u0, float v0, float u1, float v1)
    {
      this.ImageSrc = texture;
      this.drawable_sprite.Init(host, texture, u0, v0, u1, v1, u0, v0, u1, v1, u0, v0, u1, v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      this.ImageSrc = texture;
      this.drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, float disabled_u0, float disabled_v0, float disabled_u1, float disabled_v1)
    {
      this.ImageSrc = texture;
      this.drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, disabled_u0, disabled_v0, disabled_u1, disabled_v1);
    }

    public bool Init(GUIHost host, string texture)
    {
      this.ImageSrc = texture;
      return this.drawable_sprite.Init(host, texture);
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      this.drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
    }

    public void SetGrowableHeight(int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      this.drawable_sprite.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
    }

    public override void OnRender(GUIHost host)
    {
      int x;
      int y;
      int Width;
      int Height;
      if (this.VAlignment == TextVerticalAlignment.Top)
      {
        x = this.X_Abs + this.off_x;
        y = this.Y_Abs + this.off_y + this.text_area_height;
        Width = this.Width;
        Height = this.Height - this.text_area_height;
      }
      else if (this.VAlignment == TextVerticalAlignment.Bottom)
      {
        x = this.X_Abs + this.off_x;
        y = this.Y_Abs + this.off_y;
        Width = this.Width;
        Height = this.Height - this.text_area_height;
      }
      else
      {
        x = this.X_Abs + this.off_x;
        y = this.Y_Abs + this.off_y;
        Width = this.Width;
        Height = this.Height;
      }
      if (this.image_area_width > 0)
      {
        int num = Width - this.image_area_width;
        if (this.Alignment == QFontAlignment.Left)
          x += num;
        else if (this.Alignment == QFontAlignment.Centre)
          x += num / 2;
        else if (this.Alignment == QFontAlignment.Justify)
          this.off_x += this.image_area_width;
        Width = this.image_area_width;
      }
      if (this.sand_boxing)
      {
        Simple2DRenderer.Quad quad;
        quad.x0 = (float) x;
        quad.y0 = (float) y;
        quad.x1 = (float) (x + Width);
        quad.y1 = (float) (y + Height);
        quad.color = this.sandboxcolor;
        host.GetSimpleRenderer().DrawQuad(quad);
      }
      double sandboxingratio = (double) this.sandboxingratio;
      State element_state = this.Enabled || this.state == State.Down ? this.state : State.Disabled;
      if (this.Flashing && element_state != State.Highlighted && ImageWidget.FlashOn)
        element_state = State.Highlighted;
      this.drawable_sprite.Render(host, element_state, x, y, Width, Height);
      base.OnRender(host);
    }

    public Color4 SandBoxColor
    {
      get
      {
        return this.sandboxcolor;
      }
      set
      {
        this.sandboxcolor = value;
      }
    }

    [XmlAttribute("text-area-height")]
    public int TextAreaHeight
    {
      get
      {
        return this.text_area_height;
      }
      set
      {
        this.text_area_height = value;
      }
    }

    [XmlAttribute("image-area-width")]
    public int ImageAreaWidth
    {
      get
      {
        return this.image_area_width;
      }
      set
      {
        this.image_area_width = value;
      }
    }

    public bool SandBoxing
    {
      get
      {
        return this.sand_boxing;
      }
      set
      {
        this.sand_boxing = value;
      }
    }

    public float SandBoxingRatio
    {
      get
      {
        return this.sandboxingratio;
      }
      set
      {
        this.sandboxingratio = value;
      }
    }

    [XmlAttribute("src")]
    public string ImageSrc
    {
      set
      {
        this.imageSrc = value;
      }
      get
      {
        return this.imageSrc;
      }
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      if ((double) this.u0 != 0.0 || (double) this.v0 != 0.0 || ((double) this.u1 != 0.0 || (double) this.v1 != 0.0))
      {
        this.Init(host, this.ImageSrc, this.u0, this.v0, this.u1, this.v1, this.over_u0, this.over_v0, this.over_u1, this.over_v1, this.down_u0, this.down_v0, this.down_u1, this.down_v1, this.disabled_u0, this.disabled_v0, this.disabled_u1, this.disabled_v1);
        this.SetGrowableWidth(this.leftbordersize_pixels, this.rightbordersize_pixels, this.minimum_width_pixels);
        this.SetGrowableHeight(this.topbordersize_pixels, this.bottombordersize_pixels, this.minimum_height_pixels);
      }
      else
      {
        if (!parent.IsComboBoxElement() && !parent.IsListBoxElement())
        {
          this.u0 = 896f;
          this.v0 = 192f;
          this.u1 = 959f;
          this.v1 = (float) byte.MaxValue;
          this.over_u0 = 896f;
          this.over_v0 = 256f;
          this.over_u1 = 959f;
          this.over_v1 = 319f;
          this.down_u0 = 896f;
          this.down_v0 = 320f;
          this.down_u1 = 959f;
          this.down_v1 = 383f;
          this.disabled_u0 = 960f;
          this.disabled_v0 = 128f;
          this.disabled_u1 = 1023f;
          this.disabled_v1 = 191f;
          this.Init(host, "guicontrols", this.u0, this.v0, this.u1, this.v1, this.over_u0, this.over_v0, this.over_u1, this.over_v1, this.down_u0, this.down_v0, this.down_u1, this.down_v1, this.disabled_u0, this.disabled_v0, this.disabled_u1, this.disabled_v1);
        }
        this.SetGrowableWidth(4, 4, 12);
        this.SetGrowableHeight(4, 4, 12);
      }
    }

    [XmlAttribute("texture-u0")]
    public float u0
    {
      get
      {
        return this._u0;
      }
      set
      {
        this._u0 = value;
      }
    }

    [XmlAttribute("texture-v0")]
    public float v0
    {
      get
      {
        return this._v0;
      }
      set
      {
        this._v0 = value;
      }
    }

    [XmlAttribute("texture-u1")]
    public float u1
    {
      get
      {
        return this._u1;
      }
      set
      {
        this._u1 = value;
      }
    }

    [XmlAttribute("texture-v1")]
    public float v1
    {
      get
      {
        return this._v1;
      }
      set
      {
        this._v1 = value;
      }
    }

    [XmlAttribute("texture-over-u0")]
    public float over_u0
    {
      get
      {
        return this._over_u0;
      }
      set
      {
        this._over_u0 = value;
      }
    }

    [XmlAttribute("texture-over-v0")]
    public float over_v0
    {
      get
      {
        return this._over_v0;
      }
      set
      {
        this._over_v0 = value;
      }
    }

    [XmlAttribute("texture-over-u1")]
    public float over_u1
    {
      get
      {
        return this._over_u1;
      }
      set
      {
        this._over_u1 = value;
      }
    }

    [XmlAttribute("texture-over-v1")]
    public float over_v1
    {
      get
      {
        return this._over_v1;
      }
      set
      {
        this._over_v1 = value;
      }
    }

    [XmlAttribute("texture-down-u0")]
    public float down_u0
    {
      get
      {
        return this._down_u0;
      }
      set
      {
        this._down_u0 = value;
      }
    }

    [XmlAttribute("texture-down-v0")]
    public float down_v0
    {
      get
      {
        return this._down_v0;
      }
      set
      {
        this._down_v0 = value;
      }
    }

    [XmlAttribute("texture-down-u1")]
    public float down_u1
    {
      get
      {
        return this._down_u1;
      }
      set
      {
        this._down_u1 = value;
      }
    }

    [XmlAttribute("texture-down-v1")]
    public float down_v1
    {
      get
      {
        return this._down_v1;
      }
      set
      {
        this._down_v1 = value;
      }
    }

    [XmlAttribute("texture-disabled-u0")]
    public float disabled_u0
    {
      get
      {
        return this._disabled_u0;
      }
      set
      {
        this._disabled_u0 = value;
      }
    }

    [XmlAttribute("texture-disabled-v0")]
    public float disabled_v0
    {
      get
      {
        return this._disabled_v0;
      }
      set
      {
        this._disabled_v0 = value;
      }
    }

    [XmlAttribute("texture-disabled-u1")]
    public float disabled_u1
    {
      get
      {
        return this._disabled_u1;
      }
      set
      {
        this._disabled_u1 = value;
      }
    }

    [XmlAttribute("texture-disabled-v1")]
    public float disabled_v1
    {
      get
      {
        return this._disabled_v1;
      }
      set
      {
        this._disabled_v1 = value;
      }
    }

    [XmlIgnore]
    protected Color4 ImageColor
    {
      set
      {
        this.drawable_sprite.Color = value;
      }
      get
      {
        return this.drawable_sprite.Color;
      }
    }

    [XmlAttribute("default-image-color")]
    public string HexDefaultColor
    {
      set
      {
        this.drawable_sprite.DefaultColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DefaultColor
    {
      get
      {
        return this.drawable_sprite.DefaultColor;
      }
      set
      {
        this.drawable_sprite.DefaultColor = value;
      }
    }

    [XmlAttribute("down-image-color")]
    public string HexDownColor
    {
      set
      {
        this.drawable_sprite.DownColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DownColor
    {
      get
      {
        return this.drawable_sprite.DownColor;
      }
      set
      {
        this.drawable_sprite.DownColor = value;
      }
    }

    [XmlAttribute("over-image-color")]
    public string HexOverColor
    {
      set
      {
        this.drawable_sprite.OverColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 OverColor
    {
      get
      {
        return this.drawable_sprite.OverColor;
      }
      set
      {
        this.drawable_sprite.OverColor = value;
      }
    }

    [XmlAttribute("disabled-image-color")]
    public string HexDisabledColor
    {
      set
      {
        this.drawable_sprite.DisabledColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DisabledColor
    {
      get
      {
        return this.drawable_sprite.DisabledColor;
      }
      set
      {
        this.drawable_sprite.DisabledColor = value;
      }
    }

    protected static bool FlashOn
    {
      get
      {
        if (!ImageWidget.flash_timer.IsRunning || ImageWidget.flash_timer.ElapsedMilliseconds > 1000L)
          ImageWidget.flash_timer.Restart();
        return ImageWidget.flash_timer.ElapsedMilliseconds >= 500L;
      }
    }
  }
}
