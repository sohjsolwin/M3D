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
      drawable_sprite = new Sprite();
      state = State.Normal;
      text_area_height = 20;
      image_area_width = -1;
      sand_boxing = false;
      sandboxingratio = 1f;
      Text = "";
      sandboxcolor = new Color4(0.0f, 0.0f, 0.0f, 1f);
    }

    public void CopyImageData(ImageWidget source)
    {
      text_area_height = source.text_area_height;
      image_area_width = source.image_area_width;
      sand_boxing = source.sand_boxing;
      sandboxcolor = source.sandboxcolor;
      sandboxingratio = source.sandboxingratio;
      state = source.state;
      drawable_sprite.CopyFrom(source.drawable_sprite);
    }

    public override ElementType GetElementType()
    {
      return ElementType.ImageWidget;
    }

    public void Init(GUIHost host, string texture, float u0, float v0, float u1, float v1)
    {
      ImageSrc = texture;
      drawable_sprite.Init(host, texture, u0, v0, u1, v1, u0, v0, u1, v1, u0, v0, u1, v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      ImageSrc = texture;
      drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, float disabled_u0, float disabled_v0, float disabled_u1, float disabled_v1)
    {
      ImageSrc = texture;
      drawable_sprite.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, disabled_u0, disabled_v0, disabled_u1, disabled_v1);
    }

    public bool Init(GUIHost host, string texture)
    {
      ImageSrc = texture;
      return drawable_sprite.Init(host, texture);
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      drawable_sprite.SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimumwidth);
    }

    public void SetGrowableHeight(int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      drawable_sprite.SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimumheight);
    }

    public override void OnRender(GUIHost host)
    {
      int x;
      int y;
      int Width;
      int Height;
      if (VAlignment == TextVerticalAlignment.Top)
      {
        x = X_Abs + off_x;
        y = Y_Abs + off_y + text_area_height;
        Width = this.Width;
        Height = this.Height - text_area_height;
      }
      else if (VAlignment == TextVerticalAlignment.Bottom)
      {
        x = X_Abs + off_x;
        y = Y_Abs + off_y;
        Width = this.Width;
        Height = this.Height - text_area_height;
      }
      else
      {
        x = X_Abs + off_x;
        y = Y_Abs + off_y;
        Width = this.Width;
        Height = this.Height;
      }
      if (image_area_width > 0)
      {
        var num = Width - image_area_width;
        if (Alignment == QFontAlignment.Left)
        {
          x += num;
        }
        else if (Alignment == QFontAlignment.Centre)
        {
          x += num / 2;
        }
        else if (Alignment == QFontAlignment.Justify)
        {
          off_x += image_area_width;
        }

        Width = image_area_width;
      }
      if (sand_boxing)
      {
        Simple2DRenderer.Quad quad;
        quad.x0 = (float) x;
        quad.y0 = (float) y;
        quad.x1 = (float) (x + Width);
        quad.y1 = (float) (y + Height);
        quad.color = sandboxcolor;
        host.GetSimpleRenderer().DrawQuad(quad);
      }
      var sandboxingratio = (double) this.sandboxingratio;
      State element_state = Enabled || state == State.Down ? state : State.Disabled;
      if (Flashing && element_state != State.Highlighted && ImageWidget.FlashOn)
      {
        element_state = State.Highlighted;
      }

      drawable_sprite.Render(host, element_state, x, y, Width, Height);
      base.OnRender(host);
    }

    public Color4 SandBoxColor
    {
      get
      {
        return sandboxcolor;
      }
      set
      {
        sandboxcolor = value;
      }
    }

    [XmlAttribute("text-area-height")]
    public int TextAreaHeight
    {
      get
      {
        return text_area_height;
      }
      set
      {
        text_area_height = value;
      }
    }

    [XmlAttribute("image-area-width")]
    public int ImageAreaWidth
    {
      get
      {
        return image_area_width;
      }
      set
      {
        image_area_width = value;
      }
    }

    public bool SandBoxing
    {
      get
      {
        return sand_boxing;
      }
      set
      {
        sand_boxing = value;
      }
    }

    public float SandBoxingRatio
    {
      get
      {
        return sandboxingratio;
      }
      set
      {
        sandboxingratio = value;
      }
    }

    [XmlAttribute("src")]
    public string ImageSrc
    {
      set
      {
        imageSrc = value;
      }
      get
      {
        return imageSrc;
      }
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      if ((double)u0 != 0.0 || (double)v0 != 0.0 || ((double)u1 != 0.0 || (double)v1 != 0.0))
      {
        Init(host, ImageSrc, u0, v0, u1, v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, disabled_u0, disabled_v0, disabled_u1, disabled_v1);
        SetGrowableWidth(leftbordersize_pixels, rightbordersize_pixels, minimum_width_pixels);
        SetGrowableHeight(topbordersize_pixels, bottombordersize_pixels, minimum_height_pixels);
      }
      else
      {
        if (!parent.IsComboBoxElement() && !parent.IsListBoxElement())
        {
          u0 = 896f;
          v0 = 192f;
          u1 = 959f;
          v1 = (float) byte.MaxValue;
          over_u0 = 896f;
          over_v0 = 256f;
          over_u1 = 959f;
          over_v1 = 319f;
          down_u0 = 896f;
          down_v0 = 320f;
          down_u1 = 959f;
          down_v1 = 383f;
          disabled_u0 = 960f;
          disabled_v0 = 128f;
          disabled_u1 = 1023f;
          disabled_v1 = 191f;
          Init(host, "guicontrols", u0, v0, u1, v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, disabled_u0, disabled_v0, disabled_u1, disabled_v1);
        }
        SetGrowableWidth(4, 4, 12);
        SetGrowableHeight(4, 4, 12);
      }
    }

    [XmlAttribute("texture-u0")]
    public float u0
    {
      get
      {
        return _u0;
      }
      set
      {
        _u0 = value;
      }
    }

    [XmlAttribute("texture-v0")]
    public float v0
    {
      get
      {
        return _v0;
      }
      set
      {
        _v0 = value;
      }
    }

    [XmlAttribute("texture-u1")]
    public float u1
    {
      get
      {
        return _u1;
      }
      set
      {
        _u1 = value;
      }
    }

    [XmlAttribute("texture-v1")]
    public float v1
    {
      get
      {
        return _v1;
      }
      set
      {
        _v1 = value;
      }
    }

    [XmlAttribute("texture-over-u0")]
    public float over_u0
    {
      get
      {
        return _over_u0;
      }
      set
      {
        _over_u0 = value;
      }
    }

    [XmlAttribute("texture-over-v0")]
    public float over_v0
    {
      get
      {
        return _over_v0;
      }
      set
      {
        _over_v0 = value;
      }
    }

    [XmlAttribute("texture-over-u1")]
    public float over_u1
    {
      get
      {
        return _over_u1;
      }
      set
      {
        _over_u1 = value;
      }
    }

    [XmlAttribute("texture-over-v1")]
    public float over_v1
    {
      get
      {
        return _over_v1;
      }
      set
      {
        _over_v1 = value;
      }
    }

    [XmlAttribute("texture-down-u0")]
    public float down_u0
    {
      get
      {
        return _down_u0;
      }
      set
      {
        _down_u0 = value;
      }
    }

    [XmlAttribute("texture-down-v0")]
    public float down_v0
    {
      get
      {
        return _down_v0;
      }
      set
      {
        _down_v0 = value;
      }
    }

    [XmlAttribute("texture-down-u1")]
    public float down_u1
    {
      get
      {
        return _down_u1;
      }
      set
      {
        _down_u1 = value;
      }
    }

    [XmlAttribute("texture-down-v1")]
    public float down_v1
    {
      get
      {
        return _down_v1;
      }
      set
      {
        _down_v1 = value;
      }
    }

    [XmlAttribute("texture-disabled-u0")]
    public float disabled_u0
    {
      get
      {
        return _disabled_u0;
      }
      set
      {
        _disabled_u0 = value;
      }
    }

    [XmlAttribute("texture-disabled-v0")]
    public float disabled_v0
    {
      get
      {
        return _disabled_v0;
      }
      set
      {
        _disabled_v0 = value;
      }
    }

    [XmlAttribute("texture-disabled-u1")]
    public float disabled_u1
    {
      get
      {
        return _disabled_u1;
      }
      set
      {
        _disabled_u1 = value;
      }
    }

    [XmlAttribute("texture-disabled-v1")]
    public float disabled_v1
    {
      get
      {
        return _disabled_v1;
      }
      set
      {
        _disabled_v1 = value;
      }
    }

    [XmlIgnore]
    protected Color4 ImageColor
    {
      set
      {
        drawable_sprite.Color = value;
      }
      get
      {
        return drawable_sprite.Color;
      }
    }

    [XmlAttribute("default-image-color")]
    public string HexDefaultColor
    {
      set
      {
        drawable_sprite.DefaultColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DefaultColor
    {
      get
      {
        return drawable_sprite.DefaultColor;
      }
      set
      {
        drawable_sprite.DefaultColor = value;
      }
    }

    [XmlAttribute("down-image-color")]
    public string HexDownColor
    {
      set
      {
        drawable_sprite.DownColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DownColor
    {
      get
      {
        return drawable_sprite.DownColor;
      }
      set
      {
        drawable_sprite.DownColor = value;
      }
    }

    [XmlAttribute("over-image-color")]
    public string HexOverColor
    {
      set
      {
        drawable_sprite.OverColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 OverColor
    {
      get
      {
        return drawable_sprite.OverColor;
      }
      set
      {
        drawable_sprite.OverColor = value;
      }
    }

    [XmlAttribute("disabled-image-color")]
    public string HexDisabledColor
    {
      set
      {
        drawable_sprite.DisabledColor = IElement.GenerateColorFromHtml(value);
      }
    }

    [XmlIgnore]
    public Color4 DisabledColor
    {
      get
      {
        return drawable_sprite.DisabledColor;
      }
      set
      {
        drawable_sprite.DisabledColor = value;
      }
    }

    protected static bool FlashOn
    {
      get
      {
        if (!ImageWidget.flash_timer.IsRunning || ImageWidget.flash_timer.ElapsedMilliseconds > 1000L)
        {
          ImageWidget.flash_timer.Restart();
        }

        return ImageWidget.flash_timer.ElapsedMilliseconds >= 500L;
      }
    }
  }
}
