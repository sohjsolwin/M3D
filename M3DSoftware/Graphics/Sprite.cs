using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using System.IO;

namespace M3D.Graphics
{
  public class Sprite
  {
    public static int texture_width_pixels = 1024;
    public static int texture_height_pixels = 1024;
    public static bool pixel_perfect = false;
    private SpriteType spritetype;
    private Simple2DRenderer.TexturedQuad normal_quad;
    private Simple2DRenderer.TexturedQuad over_quad;
    private Simple2DRenderer.TexturedQuad down_quad;
    private Simple2DRenderer.TexturedQuad disabled_quad;
    private Simple2DRenderer.TexturedQuad cur_quad;
    private Sprite.GrowableWidthParams growablewidthparams;
    private Sprite.GrowableHeightParams growableheightparams;
    private uint texture;

    public Sprite()
    {
      spritetype = SpriteType.Normal;
      texture = uint.MaxValue;
      Color = new Color4(1f, 1f, 1f, 1f);
    }

    public void CopyFrom(Sprite source)
    {
      spritetype = source.spritetype;
      normal_quad = source.normal_quad;
      over_quad = source.over_quad;
      down_quad = source.down_quad;
      disabled_quad = source.disabled_quad;
      cur_quad = source.cur_quad;
      growablewidthparams = source.growablewidthparams;
      growableheightparams = source.growableheightparams;
      texture = source.texture;
    }

    public void Render(GUIHost host, State element_state, int x, int y, int Width, int Height)
    {
      switch (element_state)
      {
        case State.Normal:
          cur_quad = normal_quad;
          break;
        case State.Highlighted:
          cur_quad = over_quad;
          break;
        case State.Down:
          cur_quad = down_quad;
          break;
        case State.Disabled:
          cur_quad = disabled_quad;
          break;
      }
      if (spritetype == SpriteType.Normal)
      {
        RenderNormalSprite(host, x, y, Width, Height);
      }
      else if (spritetype == SpriteType.GrowableWidth)
      {
        RenderGrowableWidth(host, x, y, Width, Height);
      }
      else if (spritetype == SpriteType.GrowableHeight)
      {
        RenderGrowableHeight(host, x, y, Width, Height);
      }
      else
      {
        if (spritetype != SpriteType.Growable)
        {
          return;
        }

        RenderGrowable(host, x, y, Width, Height);
      }
    }

    private void RenderNormalSprite(GUIHost host, int x, int y, int Width, int Height)
    {
      if (texture == uint.MaxValue)
      {
        return;
      }

      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      simpleRenderer.SetCurrentTexture(texture);
      cur_quad.x0 = (float) x;
      cur_quad.y0 = (float) y;
      cur_quad.x1 = (float) (x + Width);
      cur_quad.y1 = (float) (y + Height);
      simpleRenderer.DrawQuad(cur_quad);
    }

    private void RenderGrowableWidth(GUIHost host, int x, int y, int Width, int Height)
    {
      if (Width < growablewidthparams.minimum_width_pixels)
      {
        RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (texture == uint.MaxValue)
        {
          return;
        }

        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(texture);
        cur_quad.x0 = (float) x;
        cur_quad.y0 = (float) y;
        cur_quad.x1 = (float) (x + Width);
        cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = cur_quad;
        curQuad1.u1 = curQuad1.u0 + growablewidthparams.leftbordersize_texels;
        curQuad1.x1 = curQuad1.x0 + (float)growablewidthparams.leftbordersize_pixels;
        curQuad3.u0 = curQuad3.u1 - growablewidthparams.rightbordersize_texels;
        curQuad3.x0 = curQuad3.x1 - (float)growablewidthparams.rightbordersize_pixels;
        curQuad2.x0 = curQuad1.x1;
        curQuad2.u0 = curQuad1.u1;
        curQuad2.x1 = curQuad3.x0;
        curQuad2.u1 = curQuad3.u0;
        simpleRenderer.DrawQuad(curQuad1);
        simpleRenderer.DrawQuad(curQuad2);
        simpleRenderer.DrawQuad(curQuad3);
      }
    }

    private void RenderGrowable(GUIHost host, int x, int y, int Width, int Height)
    {
      if (Width < growablewidthparams.minimum_width_pixels || Height < growableheightparams.minimum_height_pixels)
      {
        RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (texture == uint.MaxValue)
        {
          return;
        }

        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(texture);
        cur_quad.x0 = (float) x;
        cur_quad.y0 = (float) y;
        cur_quad.x1 = (float) (x + Width);
        cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = cur_quad;
        curQuad1.u1 = curQuad1.u0 + growablewidthparams.leftbordersize_texels;
        curQuad1.x1 = curQuad1.x0 + (float)growablewidthparams.leftbordersize_pixels;
        curQuad3.u0 = curQuad3.u1 - growablewidthparams.rightbordersize_texels;
        curQuad3.x0 = curQuad3.x1 - (float)growablewidthparams.rightbordersize_pixels;
        curQuad2.x0 = curQuad1.x1;
        curQuad2.u0 = curQuad1.u1;
        curQuad2.x1 = curQuad3.x0;
        curQuad2.u1 = curQuad3.u0;
        Simple2DRenderer.TexturedQuad curQuad4 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad5 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad6 = cur_quad;
        curQuad4.v1 = curQuad4.v0 + growableheightparams.topbordersize_texels;
        curQuad4.y1 = curQuad4.y0 + (float)growableheightparams.topbordersize_pixels;
        curQuad6.v0 = curQuad6.v1 - growableheightparams.bottombordersize_texels;
        curQuad6.y0 = curQuad6.y1 - (float)growableheightparams.bottombordersize_pixels;
        curQuad5.y0 = curQuad4.y1;
        curQuad5.v0 = curQuad4.v1;
        curQuad5.y1 = curQuad6.y0;
        curQuad5.v1 = curQuad6.v0;
        Simple2DRenderer.TexturedQuad textured_quad1 = curQuad1;
        Simple2DRenderer.TexturedQuad textured_quad2 = curQuad2;
        Simple2DRenderer.TexturedQuad textured_quad3 = curQuad3;
        textured_quad1.v0 = textured_quad2.v0 = textured_quad3.v0 = curQuad4.v0;
        textured_quad1.v1 = textured_quad2.v1 = textured_quad3.v1 = curQuad4.v1;
        textured_quad1.y0 = textured_quad2.y0 = textured_quad3.y0 = curQuad4.y0;
        textured_quad1.y1 = textured_quad2.y1 = textured_quad3.y1 = curQuad4.y1;
        Simple2DRenderer.TexturedQuad textured_quad4 = curQuad1;
        Simple2DRenderer.TexturedQuad textured_quad5 = curQuad2;
        Simple2DRenderer.TexturedQuad textured_quad6 = curQuad3;
        textured_quad4.v0 = textured_quad5.v0 = textured_quad6.v0 = curQuad6.v0;
        textured_quad4.v1 = textured_quad5.v1 = textured_quad6.v1 = curQuad6.v1;
        textured_quad4.y0 = textured_quad5.y0 = textured_quad6.y0 = curQuad6.y0;
        textured_quad4.y1 = textured_quad5.y1 = textured_quad6.y1 = curQuad6.y1;
        curQuad1.v0 = curQuad2.v0 = curQuad3.v0 = curQuad5.v0;
        curQuad1.v1 = curQuad2.v1 = curQuad3.v1 = curQuad5.v1;
        curQuad1.y0 = curQuad2.y0 = curQuad3.y0 = curQuad5.y0;
        curQuad1.y1 = curQuad2.y1 = curQuad3.y1 = curQuad5.y1;
        curQuad1.color = cur_quad.color;
        curQuad2.color = cur_quad.color;
        curQuad3.color = cur_quad.color;
        textured_quad1.color = cur_quad.color;
        textured_quad2.color = cur_quad.color;
        textured_quad3.color = cur_quad.color;
        textured_quad4.color = cur_quad.color;
        textured_quad5.color = cur_quad.color;
        textured_quad6.color = cur_quad.color;
        simpleRenderer.DrawQuad(curQuad1);
        simpleRenderer.DrawQuad(curQuad2);
        simpleRenderer.DrawQuad(curQuad3);
        simpleRenderer.DrawQuad(textured_quad1);
        simpleRenderer.DrawQuad(textured_quad2);
        simpleRenderer.DrawQuad(textured_quad3);
        simpleRenderer.DrawQuad(textured_quad4);
        simpleRenderer.DrawQuad(textured_quad5);
        simpleRenderer.DrawQuad(textured_quad6);
      }
    }

    private void RenderGrowableHeight(GUIHost host, int x, int y, int Width, int Height)
    {
      if (Height < growableheightparams.minimum_height_pixels)
      {
        RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (texture == uint.MaxValue)
        {
          return;
        }

        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(texture);
        cur_quad.x0 = (float) x;
        cur_quad.y0 = (float) y;
        cur_quad.x1 = (float) (x + Width);
        cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = cur_quad;
        curQuad1.v1 = curQuad1.v0 + growableheightparams.topbordersize_texels;
        curQuad1.y1 = curQuad1.y0 + (float)growableheightparams.topbordersize_pixels;
        curQuad3.v0 = curQuad3.v1 - growableheightparams.bottombordersize_texels;
        curQuad3.y0 = curQuad3.y1 - (float)growableheightparams.bottombordersize_pixels;
        curQuad2.y0 = curQuad1.y1;
        curQuad2.v0 = curQuad1.v1;
        curQuad2.y1 = curQuad3.y0;
        curQuad2.v1 = curQuad3.v0;
        simpleRenderer.DrawQuad(curQuad1);
        simpleRenderer.DrawQuad(curQuad2);
        simpleRenderer.DrawQuad(curQuad3);
      }
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1)
    {
      Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, float disabled_u0, float disabled_v0, float disabled_u1, float disabled_v1)
    {
      spritetype = SpriteType.Normal;
      var num1 = 0.0f;
      var num2 = 0.0f;
      float num3;
      float num4;
      if (Sprite.pixel_perfect)
      {
        num1 = 1f / (float) Sprite.texture_width_pixels;
        num2 = 1f / (float) Sprite.texture_height_pixels;
        num3 = 1f / (float) Sprite.texture_width_pixels;
        num4 = 1f / (float) Sprite.texture_height_pixels;
      }
      else
      {
        num3 = 1f / (float) Sprite.texture_width_pixels;
        num4 = 1f / (float) Sprite.texture_height_pixels;
      }
      normal_quad.x0 = 0.0f;
      normal_quad.y0 = 0.0f;
      normal_quad.x1 = 1f;
      normal_quad.y1 = 1f;
      over_quad = down_quad = normal_quad;
      if ((double) over_u0 == 0.0 && (double) over_v0 == 0.0 && ((double) over_u1 == 0.0 && (double) over_v1 == 0.0))
      {
        over_u0 = normal_u0;
        over_v0 = normal_v0;
        over_u1 = normal_u1;
        over_v1 = normal_v1;
      }
      if ((double) down_u0 == 0.0 && (double) down_v0 == 0.0 && ((double) down_u1 == 0.0 && (double) down_v1 == 0.0))
      {
        down_u0 = normal_u0;
        down_v0 = normal_v0;
        down_u1 = normal_u1;
        down_v1 = normal_v1;
      }
      if ((double) disabled_u0 == 0.0 && (double) disabled_v0 == 0.0 && ((double) disabled_u1 == 0.0 && (double) disabled_v1 == 0.0))
      {
        disabled_u0 = normal_u0;
        disabled_v0 = normal_v0;
        disabled_u1 = normal_u1;
        disabled_v1 = normal_v1;
      }
      normal_quad.u0 = normal_u0 * num3;
      normal_quad.v0 = normal_v0 * num4;
      normal_quad.u1 = normal_u1 * num3 + num1;
      normal_quad.v1 = normal_v1 * num4 + num2;
      normal_quad.color = new Color4(1f, 1f, 1f, 1f);
      over_quad.u0 = over_u0 * num3;
      over_quad.v0 = over_v0 * num4;
      over_quad.u1 = over_u1 * num3 + num1;
      over_quad.v1 = over_v1 * num4 + num2;
      over_quad.color = new Color4(1f, 1f, 1f, 1f);
      down_quad.u0 = down_u0 * num3;
      down_quad.v0 = down_v0 * num4;
      down_quad.u1 = down_u1 * num3 + num1;
      down_quad.v1 = down_v1 * num4 + num2;
      down_quad.color = new Color4(1f, 1f, 1f, 1f);
      disabled_quad.u0 = disabled_u0 * num3;
      disabled_quad.v0 = disabled_v0 * num4;
      disabled_quad.u1 = disabled_u1 * num3 + num1;
      disabled_quad.v1 = disabled_v1 * num4 + num2;
      disabled_quad.color = new Color4(1f, 1f, 1f, 1f);
      if (Path.IsPathRooted(texture))
      {
        this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture, false);
      }
      else
      {
        this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture);
      }
    }

    public bool Init(GUIHost host, string texture)
    {
      spritetype = SpriteType.Normal;
      normal_quad.x0 = 0.0f;
      normal_quad.y0 = 0.0f;
      normal_quad.x1 = 1f;
      normal_quad.y1 = 1f;
      normal_quad.u0 = 0.0f;
      normal_quad.v0 = 0.0f;
      normal_quad.u1 = 1f;
      normal_quad.v1 = 1f;
      disabled_quad = over_quad = down_quad = normal_quad;
      this.texture = !Path.IsPathRooted(texture) ? host.GetSimpleRenderer().LoadTextureFromFile(texture) : host.GetSimpleRenderer().LoadTextureFromFile(texture, false);
      return this.texture != uint.MaxValue;
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      growablewidthparams.leftbordersize_pixels = leftbordersize_pixels;
      growablewidthparams.rightbordersize_pixels = rightbordersize_pixels;
      growablewidthparams.minimum_width_pixels = minimumwidth;
      growablewidthparams.leftbordersize_texels = (float) leftbordersize_pixels / (float) Sprite.texture_width_pixels;
      growablewidthparams.rightbordersize_texels = (float) rightbordersize_pixels / (float) Sprite.texture_width_pixels;
      if (spritetype == SpriteType.Normal || spritetype == SpriteType.GrowableWidth)
      {
        spritetype = SpriteType.GrowableWidth;
      }
      else
      {
        spritetype = SpriteType.Growable;
      }
    }

    public void SetGrowableHeight(int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      growableheightparams.topbordersize_pixels = topbordersize_pixels;
      growableheightparams.bottombordersize_pixels = bottombordersize_pixels;
      growableheightparams.minimum_height_pixels = minimumheight;
      growableheightparams.topbordersize_texels = (float) topbordersize_pixels / (float) Sprite.texture_height_pixels;
      growableheightparams.bottombordersize_texels = (float) bottombordersize_pixels / (float) Sprite.texture_height_pixels;
      if (spritetype == SpriteType.Normal || spritetype == SpriteType.GrowableHeight)
      {
        spritetype = SpriteType.GrowableHeight;
      }
      else
      {
        spritetype = SpriteType.Growable;
      }
    }

    public Color4 Color
    {
      set
      {
        normal_quad.color = value;
        down_quad.color = value;
        over_quad.color = value;
        disabled_quad.color = value;
      }
      get
      {
        return normal_quad.color;
      }
    }

    public Color4 DefaultColor
    {
      set
      {
        normal_quad.color = value;
      }
      get
      {
        return normal_quad.color;
      }
    }

    public Color4 DownColor
    {
      set
      {
        down_quad.color = value;
      }
      get
      {
        return down_quad.color;
      }
    }

    public Color4 OverColor
    {
      set
      {
        over_quad.color = value;
      }
      get
      {
        return over_quad.color;
      }
    }

    public Color4 DisabledColor
    {
      set
      {
        disabled_quad.color = value;
      }
      get
      {
        return disabled_quad.color;
      }
    }

    private struct GrowableWidthParams
    {
      public int leftbordersize_pixels;
      public int rightbordersize_pixels;
      public float leftbordersize_texels;
      public float rightbordersize_texels;
      public int minimum_width_pixels;
    }

    private struct GrowableHeightParams
    {
      public int topbordersize_pixels;
      public int bottombordersize_pixels;
      public float topbordersize_texels;
      public float bottombordersize_texels;
      public int minimum_height_pixels;
    }
  }
}
