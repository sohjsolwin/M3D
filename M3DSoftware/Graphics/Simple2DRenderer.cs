using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace M3D.Graphics
{
  public class Simple2DRenderer
  {
    private string texturePath = "";
    private float lineWidth = 2f;
    private Stack<Simple2DRenderer.Region> regions = new Stack<Simple2DRenderer.Region>();
    private List<Texture> textureList;
    private uint currentTexture;
    private int glwindow_width;
    private int glwindow_height;
    private int originalViewportX;
    private int originalViewportY;
    private int originalViewportWidth;
    private int originalViewportHeight;
    public const uint undefined_texture_handle = 4294967295;
    private bool lighting;
    private bool zbuffer;

    public void PushClipping(Simple2DRenderer.Region region)
    {
      if (regions.Count == 0)
      {
        GL.Enable(EnableCap.ScissorTest);
      }

      if (regions.Count != 0)
      {
        Simple2DRenderer.Region region1 = regions.Peek();
        if (region1.Equals(region))
        {
          ++region1.count;
          return;
        }
      }
      regions.Push(region);
      ScissorClippingRegion();
    }

    public void PopClipping()
    {
      if (regions.Count != 0)
      {
        Simple2DRenderer.Region region = regions.Peek();
        if (region.count > 0)
        {
          --region.count;
          return;
        }
        regions.Pop();
      }
      if (regions.Count == 0)
      {
        GL.Disable(EnableCap.ScissorTest);
      }
      else
      {
        ScissorClippingRegion();
      }
    }

    private Simple2DRenderer.Region ClippingRegion
    {
      get
      {
        if (regions.Count == 0)
        {
          return new Simple2DRenderer.Region(0, 0, glwindow_width, glwindow_height);
        }

        var x0 = regions.Max<Simple2DRenderer.Region>(a => a.x0);
        var num1 = regions.Max<Simple2DRenderer.Region>(a => a.y0);
        var num2 = regions.Min<Simple2DRenderer.Region>(a => a.x1);
        var num3 = regions.Min<Simple2DRenderer.Region>(a => a.y1);
        var y0 = num1;
        var x1 = num2;
        var y1 = num3;
        var region = new Simple2DRenderer.Region(x0, y0, x1, y1);
        if (region.x0 > region.x1 || region.y0 > region.y1)
        {
          region.x0 = 0;
          region.y0 = 0;
          region.x1 = 0;
          region.y1 = 0;
        }
        region.Equals(regions.Peek());
        return region;
      }
    }

    public int ClippingWidth
    {
      get
      {
        if (regions.Count == 0)
        {
          return glwindow_width;
        }

        var num1 = regions.Max<Simple2DRenderer.Region>(a => a.x0);
        var num2 = regions.Min<Simple2DRenderer.Region>(a => a.x1);
        if (num1 > num2)
        {
          num1 = 0;
          num2 = 0;
        }
        return num2 - num1;
      }
    }

    public int ClippingHeight
    {
      get
      {
        if (regions.Count == 0)
        {
          return glwindow_height;
        }

        var num1 = regions.Max<Simple2DRenderer.Region>(a => a.y0);
        var num2 = regions.Min<Simple2DRenderer.Region>(a => a.y1);
        if (num1 > num2)
        {
          num1 = 0;
          num2 = 0;
        }
        return num2 - num1;
      }
    }

    public Simple2DRenderer(int glwindow_width, int glwindow_height)
    {
      textureList = new List<Texture>();
      this.glwindow_width = glwindow_width;
      this.glwindow_height = glwindow_height;
    }

    public void OnGLWindowResize(int width, int height)
    {
      glwindow_width = width;
      glwindow_height = height;
    }

    public void SetCameraOrthographic(double left, double right, double bottom, double top, double zNear, double zFar)
    {
      GL.MatrixMode(MatrixMode.Projection);
      GL.LoadIdentity();
      GL.Ortho(left, right, bottom, top, zNear, zFar);
      GL.MatrixMode(MatrixMode.Modelview);
    }

    private void ScissorClippingRegion()
    {
      Simple2DRenderer.Region clippingRegion = ClippingRegion;
      var x0 = clippingRegion.x0;
      var y0 = clippingRegion.y0;
      var x1 = clippingRegion.x1;
      var y1 = clippingRegion.y1;
      GL.Scissor(x0, glwindow_height - y1, x1 - x0, y1 - y0);
    }

    public void Begin2D()
    {
      GL.Viewport(0, 0, GLWindowWidth(), GLWindowHeight());
      SetCameraOrthographic(0.0, GLWindowWidth(), 0.0, GLWindowHeight(), 1.0, 1000.0);
      lighting = GL.IsEnabled(EnableCap.Lighting);
      if (lighting)
      {
        GL.Disable(EnableCap.Lighting);
      }

      zbuffer = GL.IsEnabled(EnableCap.DepthTest);
      if (zbuffer)
      {
        GL.Disable(EnableCap.DepthTest);
      }

      int[] data = new int[4];
      GL.GetInteger(GetPName.Viewport, data);
      originalViewportX = data[1];
      originalViewportY = data[0];
      originalViewportWidth = data[2] - data[1];
      originalViewportHeight = data[3] - data[0];
      GL.Viewport(0, 0, glwindow_width, glwindow_height);
      SetCameraOrthographic(0.0, glwindow_width, 0.0, glwindow_height, 1.0, 1000.0);
      GL.MatrixMode(MatrixMode.Modelview);
      GL.LoadIdentity();
      GL.Enable(EnableCap.ColorMaterial);
      GL.BindTexture(TextureTarget.Texture2D, 0);
      GL.Translate(0.0f, 0.0f, -999f);
    }

    public void End2D()
    {
      GL.Viewport(originalViewportX, originalViewportY, originalViewportWidth, originalViewportHeight);
      GL.Disable(EnableCap.ColorMaterial);
      GL.Color4(1f, 1f, 1f, 1f);
      if (lighting)
      {
        GL.Enable(EnableCap.Lighting);
      }

      if (!zbuffer)
      {
        return;
      }

      GL.Enable(EnableCap.DepthTest);
    }

    public void SetTexturePath(string path)
    {
      texturePath = path + Path.DirectorySeparatorChar.ToString();
    }

    public uint LoadTextureFromFile(string filename)
    {
      try
      {
        return LoadTextureFromFile(filename, true);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public uint LoadTextureFromFile(string filename, bool default_path)
    {
      var textureByName = GetTextureByName(filename);
      if (textureByName != uint.MaxValue)
      {
        return textureByName;
      }

      if (default_path)
      {
        filename = texturePath + filename;
      }

      return LoadTextureFromBitmap(new Bitmap(filename), Path.GetFileName(filename));
    }

    public uint LoadTextureFromBitmap(Bitmap bitmap, string name)
    {
      uint glTexture;
      try
      {
        glTexture = CreateGLTexture(bitmap);
      }
      catch (Exception ex)
      {
        return uint.MaxValue;
      }
      textureList.Add(new Texture(name, glTexture));
      return glTexture;
    }

    private uint CreateGLTexture(Bitmap bitmap)
    {
      GL.GenTextures(1, out uint textures);
      GL.BindTexture(TextureTarget.Texture2D, textures);
      BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
      if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
      {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bitmapdata.Width, bitmapdata.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bitmapdata.Scan0);
      }
      else
      {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapdata.Width, bitmapdata.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapdata.Scan0);
      }

      bitmap.UnlockBits(bitmapdata);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, 9729);
      GL.BindTexture(TextureTarget.Texture2D, 0);
      bitmap.Dispose();
      return textures;
    }

    public void UnLoadTexture(uint handle)
    {
      for (var index = 0; index < textureList.Count; ++index)
      {
        if ((int)textureList[index].handle == (int) handle)
        {
          GL.DeleteTexture(handle);
          textureList.RemoveAt(index);
          break;
        }
      }
    }

    public uint GetTextureByName(string name)
    {
      foreach (Texture texture in textureList)
      {
        if (texture.name == name)
        {
          return texture.handle;
        }
      }
      return uint.MaxValue;
    }

    public void SetCurrentTexture(uint texture_handle)
    {
      currentTexture = texture_handle;
    }

    public void SetLineWidth(float width)
    {
      if (width > 10.0)
      {
        lineWidth = 10f;
      }
      else
      {
        lineWidth = width;
      }
    }

    public void DrawQuad(Simple2DRenderer.TexturedQuad textured_quad)
    {
      var num = textured_quad.y1 - textured_quad.y0;
      var y = glwindow_height - textured_quad.y0;
      GL.BindTexture(TextureTarget.Texture2D, currentTexture);
      GL.Translate(0.0f, 0.0f, 1f);
      GL.Color4(textured_quad.color.R, textured_quad.color.G, textured_quad.color.B, textured_quad.color.A);
      GL.Begin(PrimitiveType.Triangles);
      GL.TexCoord2(textured_quad.u1, textured_quad.v0);
      GL.Vertex3(textured_quad.x1, y, 0.0f);
      GL.TexCoord2(textured_quad.u0, textured_quad.v0);
      GL.Vertex3(textured_quad.x0, y, 0.0f);
      GL.TexCoord2(textured_quad.u1, textured_quad.v1);
      GL.Vertex3(textured_quad.x1, y - num, 0.0f);
      GL.Vertex3(textured_quad.x1, y - num, 0.0f);
      GL.TexCoord2(textured_quad.u0, textured_quad.v0);
      GL.Vertex3(textured_quad.x0, y, 0.0f);
      GL.TexCoord2(textured_quad.u0, textured_quad.v1);
      GL.Vertex3(textured_quad.x0, y - num, 0.0f);
      GL.End();
      GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void DrawQuad(Simple2DRenderer.Quad quad)
    {
      var num = quad.y1 - quad.y0;
      var y = glwindow_height - quad.y0;
      GL.BindTexture(TextureTarget.Texture2D, 0);
      GL.Translate(0.0f, 0.0f, 1f);
      GL.Color4(quad.color.R, quad.color.G, quad.color.B, quad.color.A);
      GL.Begin(PrimitiveType.Triangles);
      GL.Vertex3(quad.x1, y, 0.0f);
      GL.Vertex3(quad.x0, y, 0.0f);
      GL.Vertex3(quad.x1, y - num, 0.0f);
      GL.Vertex3(quad.x1, y - num, 0.0f);
      GL.Vertex3(quad.x0, y, 0.0f);
      GL.Vertex3(quad.x0, y - num, 0.0f);
      GL.End();
    }

    public void DrawQuadLine(Simple2DRenderer.Quad quad)
    {
      var num = quad.y1 - quad.y0;
      var y = glwindow_height - quad.y0;
      GL.BindTexture(TextureTarget.Texture2D, 0);
      GL.Translate(0.0f, 0.0f, 1f);
      GL.Color4(quad.color.R, quad.color.G, quad.color.B, quad.color.A);
      GL.LineWidth(lineWidth);
      GL.Begin(PrimitiveType.Lines);
      GL.Vertex3(quad.x1, y, 0.0f);
      GL.Vertex3(quad.x0, y, 0.0f);
      GL.Vertex3(quad.x0, y, 0.0f);
      GL.Vertex3(quad.x0, y - num, 0.0f);
      GL.Vertex3(quad.x0, y - num, 0.0f);
      GL.Vertex3(quad.x1, y - num, 0.0f);
      GL.Vertex3(quad.x1, y - num, 0.0f);
      GL.Vertex3(quad.x1, y, 0.0f);
      GL.End();
    }

    public void Close()
    {
      foreach (Texture texture in textureList)
      {
        GL.DeleteTexture(texture.handle);
      }

      textureList.Clear();
    }

    public int WindowWidth
    {
      get
      {
        return glwindow_width;
      }
    }

    public int WindowHeight
    {
      get
      {
        return glwindow_height;
      }
    }

    public int GLWindowWidth()
    {
      return glwindow_width;
    }

    public int GLWindowHeight()
    {
      return glwindow_height;
    }

    public class Region : IEquatable<Simple2DRenderer.Region>
    {
      public int x0;
      public int y0;
      public int x1;
      public int y1;
      public int count;

      public Region(int x0, int y0, int x1, int y1)
      {
        this.x0 = x0;
        this.x1 = x1;
        this.y0 = y0;
        this.y1 = y1;
        count = 0;
      }

      public bool Equals(Simple2DRenderer.Region other)
      {
        if (x0 == other.x0 && x1 == other.x1 && y0 == other.y0)
        {
          return y1 == other.y1;
        }

        return false;
      }
    }

    public struct TexturedQuad
    {
      public float x0;
      public float y0;
      public float x1;
      public float y1;
      public float u0;
      public float v0;
      public float u1;
      public float v1;
      public Color4 color;

      public TexturedQuad(float x0, float y0, float x1, float y1, float u0, float v0, float u1, float v1)
      {
        this.x0 = x0;
        this.x1 = x1;
        this.y0 = y0;
        this.y1 = y1;
        this.u0 = u0;
        this.u1 = u1;
        this.v0 = v0;
        this.v1 = v1;
        color = new Color4(1f, 1f, 1f, 1f);
      }
    }

    public struct Quad
    {
      public float x0;
      public float y0;
      public float x1;
      public float y1;
      public Color4 color;

      public Quad(float x0, float y0, float x1, float y1, Color4 color)
      {
        this.x0 = x0;
        this.x1 = x1;
        this.y0 = y0;
        this.y1 = y1;
        this.color = color;
      }
    }
  }
}
