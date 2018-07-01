// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Sprite
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.spritetype = SpriteType.Normal;
      this.texture = uint.MaxValue;
      this.Color = new Color4(1f, 1f, 1f, 1f);
    }

    public void CopyFrom(Sprite source)
    {
      this.spritetype = source.spritetype;
      this.normal_quad = source.normal_quad;
      this.over_quad = source.over_quad;
      this.down_quad = source.down_quad;
      this.disabled_quad = source.disabled_quad;
      this.cur_quad = source.cur_quad;
      this.growablewidthparams = source.growablewidthparams;
      this.growableheightparams = source.growableheightparams;
      this.texture = source.texture;
    }

    public void Render(GUIHost host, State element_state, int x, int y, int Width, int Height)
    {
      switch (element_state)
      {
        case State.Normal:
          this.cur_quad = this.normal_quad;
          break;
        case State.Highlighted:
          this.cur_quad = this.over_quad;
          break;
        case State.Down:
          this.cur_quad = this.down_quad;
          break;
        case State.Disabled:
          this.cur_quad = this.disabled_quad;
          break;
      }
      if (this.spritetype == SpriteType.Normal)
        this.RenderNormalSprite(host, x, y, Width, Height);
      else if (this.spritetype == SpriteType.GrowableWidth)
        this.RenderGrowableWidth(host, x, y, Width, Height);
      else if (this.spritetype == SpriteType.GrowableHeight)
      {
        this.RenderGrowableHeight(host, x, y, Width, Height);
      }
      else
      {
        if (this.spritetype != SpriteType.Growable)
          return;
        this.RenderGrowable(host, x, y, Width, Height);
      }
    }

    private void RenderNormalSprite(GUIHost host, int x, int y, int Width, int Height)
    {
      if (this.texture == uint.MaxValue)
        return;
      Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
      simpleRenderer.SetCurrentTexture(this.texture);
      this.cur_quad.x0 = (float) x;
      this.cur_quad.y0 = (float) y;
      this.cur_quad.x1 = (float) (x + Width);
      this.cur_quad.y1 = (float) (y + Height);
      simpleRenderer.DrawQuad(this.cur_quad);
    }

    private void RenderGrowableWidth(GUIHost host, int x, int y, int Width, int Height)
    {
      if (Width < this.growablewidthparams.minimum_width_pixels)
      {
        this.RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (this.texture == uint.MaxValue)
          return;
        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(this.texture);
        this.cur_quad.x0 = (float) x;
        this.cur_quad.y0 = (float) y;
        this.cur_quad.x1 = (float) (x + Width);
        this.cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = this.cur_quad;
        curQuad1.u1 = curQuad1.u0 + this.growablewidthparams.leftbordersize_texels;
        curQuad1.x1 = curQuad1.x0 + (float) this.growablewidthparams.leftbordersize_pixels;
        curQuad3.u0 = curQuad3.u1 - this.growablewidthparams.rightbordersize_texels;
        curQuad3.x0 = curQuad3.x1 - (float) this.growablewidthparams.rightbordersize_pixels;
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
      if (Width < this.growablewidthparams.minimum_width_pixels || Height < this.growableheightparams.minimum_height_pixels)
      {
        this.RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (this.texture == uint.MaxValue)
          return;
        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(this.texture);
        this.cur_quad.x0 = (float) x;
        this.cur_quad.y0 = (float) y;
        this.cur_quad.x1 = (float) (x + Width);
        this.cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = this.cur_quad;
        curQuad1.u1 = curQuad1.u0 + this.growablewidthparams.leftbordersize_texels;
        curQuad1.x1 = curQuad1.x0 + (float) this.growablewidthparams.leftbordersize_pixels;
        curQuad3.u0 = curQuad3.u1 - this.growablewidthparams.rightbordersize_texels;
        curQuad3.x0 = curQuad3.x1 - (float) this.growablewidthparams.rightbordersize_pixels;
        curQuad2.x0 = curQuad1.x1;
        curQuad2.u0 = curQuad1.u1;
        curQuad2.x1 = curQuad3.x0;
        curQuad2.u1 = curQuad3.u0;
        Simple2DRenderer.TexturedQuad curQuad4 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad5 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad6 = this.cur_quad;
        curQuad4.v1 = curQuad4.v0 + this.growableheightparams.topbordersize_texels;
        curQuad4.y1 = curQuad4.y0 + (float) this.growableheightparams.topbordersize_pixels;
        curQuad6.v0 = curQuad6.v1 - this.growableheightparams.bottombordersize_texels;
        curQuad6.y0 = curQuad6.y1 - (float) this.growableheightparams.bottombordersize_pixels;
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
        curQuad1.color = this.cur_quad.color;
        curQuad2.color = this.cur_quad.color;
        curQuad3.color = this.cur_quad.color;
        textured_quad1.color = this.cur_quad.color;
        textured_quad2.color = this.cur_quad.color;
        textured_quad3.color = this.cur_quad.color;
        textured_quad4.color = this.cur_quad.color;
        textured_quad5.color = this.cur_quad.color;
        textured_quad6.color = this.cur_quad.color;
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
      if (Height < this.growableheightparams.minimum_height_pixels)
      {
        this.RenderNormalSprite(host, x, y, Width, Height);
      }
      else
      {
        if (this.texture == uint.MaxValue)
          return;
        Simple2DRenderer simpleRenderer = host.GetSimpleRenderer();
        simpleRenderer.SetCurrentTexture(this.texture);
        this.cur_quad.x0 = (float) x;
        this.cur_quad.y0 = (float) y;
        this.cur_quad.x1 = (float) (x + Width);
        this.cur_quad.y1 = (float) (y + Height);
        Simple2DRenderer.TexturedQuad curQuad1 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad2 = this.cur_quad;
        Simple2DRenderer.TexturedQuad curQuad3 = this.cur_quad;
        curQuad1.v1 = curQuad1.v0 + this.growableheightparams.topbordersize_texels;
        curQuad1.y1 = curQuad1.y0 + (float) this.growableheightparams.topbordersize_pixels;
        curQuad3.v0 = curQuad3.v1 - this.growableheightparams.bottombordersize_texels;
        curQuad3.y0 = curQuad3.y1 - (float) this.growableheightparams.bottombordersize_pixels;
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
      this.Init(host, texture, normal_u0, normal_v0, normal_u1, normal_v1, over_u0, over_v0, over_u1, over_v1, down_u0, down_v0, down_u1, down_v1, down_u0, down_v0, down_u1, down_v1);
    }

    public void Init(GUIHost host, string texture, float normal_u0, float normal_v0, float normal_u1, float normal_v1, float over_u0, float over_v0, float over_u1, float over_v1, float down_u0, float down_v0, float down_u1, float down_v1, float disabled_u0, float disabled_v0, float disabled_u1, float disabled_v1)
    {
      this.spritetype = SpriteType.Normal;
      float num1 = 0.0f;
      float num2 = 0.0f;
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
      this.normal_quad.x0 = 0.0f;
      this.normal_quad.y0 = 0.0f;
      this.normal_quad.x1 = 1f;
      this.normal_quad.y1 = 1f;
      this.over_quad = this.down_quad = this.normal_quad;
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
      this.normal_quad.u0 = normal_u0 * num3;
      this.normal_quad.v0 = normal_v0 * num4;
      this.normal_quad.u1 = normal_u1 * num3 + num1;
      this.normal_quad.v1 = normal_v1 * num4 + num2;
      this.normal_quad.color = new Color4(1f, 1f, 1f, 1f);
      this.over_quad.u0 = over_u0 * num3;
      this.over_quad.v0 = over_v0 * num4;
      this.over_quad.u1 = over_u1 * num3 + num1;
      this.over_quad.v1 = over_v1 * num4 + num2;
      this.over_quad.color = new Color4(1f, 1f, 1f, 1f);
      this.down_quad.u0 = down_u0 * num3;
      this.down_quad.v0 = down_v0 * num4;
      this.down_quad.u1 = down_u1 * num3 + num1;
      this.down_quad.v1 = down_v1 * num4 + num2;
      this.down_quad.color = new Color4(1f, 1f, 1f, 1f);
      this.disabled_quad.u0 = disabled_u0 * num3;
      this.disabled_quad.v0 = disabled_v0 * num4;
      this.disabled_quad.u1 = disabled_u1 * num3 + num1;
      this.disabled_quad.v1 = disabled_v1 * num4 + num2;
      this.disabled_quad.color = new Color4(1f, 1f, 1f, 1f);
      if (Path.IsPathRooted(texture))
        this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture, false);
      else
        this.texture = host.GetSimpleRenderer().LoadTextureFromFile(texture);
    }

    public bool Init(GUIHost host, string texture)
    {
      this.spritetype = SpriteType.Normal;
      this.normal_quad.x0 = 0.0f;
      this.normal_quad.y0 = 0.0f;
      this.normal_quad.x1 = 1f;
      this.normal_quad.y1 = 1f;
      this.normal_quad.u0 = 0.0f;
      this.normal_quad.v0 = 0.0f;
      this.normal_quad.u1 = 1f;
      this.normal_quad.v1 = 1f;
      this.disabled_quad = this.over_quad = this.down_quad = this.normal_quad;
      this.texture = !Path.IsPathRooted(texture) ? host.GetSimpleRenderer().LoadTextureFromFile(texture) : host.GetSimpleRenderer().LoadTextureFromFile(texture, false);
      return this.texture != uint.MaxValue;
    }

    public void SetGrowableWidth(int leftbordersize_pixels, int rightbordersize_pixels, int minimumwidth)
    {
      this.growablewidthparams.leftbordersize_pixels = leftbordersize_pixels;
      this.growablewidthparams.rightbordersize_pixels = rightbordersize_pixels;
      this.growablewidthparams.minimum_width_pixels = minimumwidth;
      this.growablewidthparams.leftbordersize_texels = (float) leftbordersize_pixels / (float) Sprite.texture_width_pixels;
      this.growablewidthparams.rightbordersize_texels = (float) rightbordersize_pixels / (float) Sprite.texture_width_pixels;
      if (this.spritetype == SpriteType.Normal || this.spritetype == SpriteType.GrowableWidth)
        this.spritetype = SpriteType.GrowableWidth;
      else
        this.spritetype = SpriteType.Growable;
    }

    public void SetGrowableHeight(int topbordersize_pixels, int bottombordersize_pixels, int minimumheight)
    {
      this.growableheightparams.topbordersize_pixels = topbordersize_pixels;
      this.growableheightparams.bottombordersize_pixels = bottombordersize_pixels;
      this.growableheightparams.minimum_height_pixels = minimumheight;
      this.growableheightparams.topbordersize_texels = (float) topbordersize_pixels / (float) Sprite.texture_height_pixels;
      this.growableheightparams.bottombordersize_texels = (float) bottombordersize_pixels / (float) Sprite.texture_height_pixels;
      if (this.spritetype == SpriteType.Normal || this.spritetype == SpriteType.GrowableHeight)
        this.spritetype = SpriteType.GrowableHeight;
      else
        this.spritetype = SpriteType.Growable;
    }

    public Color4 Color
    {
      set
      {
        this.normal_quad.color = value;
        this.down_quad.color = value;
        this.over_quad.color = value;
        this.disabled_quad.color = value;
      }
      get
      {
        return this.normal_quad.color;
      }
    }

    public Color4 DefaultColor
    {
      set
      {
        this.normal_quad.color = value;
      }
      get
      {
        return this.normal_quad.color;
      }
    }

    public Color4 DownColor
    {
      set
      {
        this.down_quad.color = value;
      }
      get
      {
        return this.down_quad.color;
      }
    }

    public Color4 OverColor
    {
      set
      {
        this.over_quad.color = value;
      }
      get
      {
        return this.over_quad.color;
      }
    }

    public Color4 DisabledColor
    {
      set
      {
        this.disabled_quad.color = value;
      }
      get
      {
        return this.disabled_quad.color;
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
