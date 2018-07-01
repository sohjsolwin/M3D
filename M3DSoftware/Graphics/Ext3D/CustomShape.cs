// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.CustomShape
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D.ModelRendering;
using M3D.Model.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace M3D.Graphics.Ext3D
{
  public class CustomShape : Element3D
  {
    public bool CullBackFaces = true;
    private Color4 ambient;
    private Color4 diffuse;
    private Color4 emission;
    private Color4 specular;
    private float shininess;
    private Vector3 ext;
    private Vector3 max;
    private Vector3 min;
    protected int texture_handle;
    protected OpenGLRendererObject texturedGeometry;

    public CustomShape()
      : this(0, (Element3D) null)
    {
    }

    public CustomShape(int ID)
      : this(ID, (Element3D) null)
    {
    }

    public CustomShape(int ID, Element3D parent)
      : base(ID, parent)
    {
      this.ambient = new Color4(0.05f, 0.05f, 0.05f, 1f);
      this.diffuse = new Color4(1f, 1f, 1f, 1f);
      this.specular = new Color4(1f, 1f, 1f, 1f);
      this.shininess = 100f;
    }

    public override void Render3D()
    {
      GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, this.ambient);
      GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, this.diffuse);
      GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, this.specular);
      GL.Material(MaterialFace.Front, MaterialParameter.Shininess, this.shininess);
      GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, this.emission);
      GL.DepthMask(true);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
      if (!this.CullBackFaces)
      {
        GL.DepthMask(false);
        GL.Disable(EnableCap.CullFace);
      }
      GL.BindTexture(TextureTarget.Texture2D, this.texture_handle);
      if (this.texturedGeometry != null)
        this.texturedGeometry.Draw();
      GL.DepthMask(true);
      GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Color4(0.0f, 0.0f, 0.0f, 1f));
      GL.BindTexture(TextureTarget.Texture2D, 0);
      if (!this.CullBackFaces)
        GL.Enable(EnableCap.CullFace);
      base.Render3D();
    }

    public void Rescale(float x, float y, float z)
    {
      this.texturedGeometry.Scale(x, y, z);
    }

    public void Create(List<VertexTNV> vertex_list, int opengl_texture_handle)
    {
      if (vertex_list == null)
        throw new ArgumentNullException("vertex_list cannot be null");
      if (vertex_list.Count < 3)
        throw new ArgumentException("vertex_list must have at least 3 vertices.");
      if (vertex_list.Count % 3 != 0)
        throw new ArgumentException("vertex_list.Count must be a multiple of 3.");
      this.max = new Vector3(vertex_list[0].Position.X, vertex_list[0].Position.Y, vertex_list[0].Position.Z);
      this.min = new Vector3(vertex_list[0].Position.X, vertex_list[0].Position.Y, vertex_list[0].Position.Z);
      foreach (VertexTNV vertex in vertex_list)
      {
        if ((double) vertex.Position.X < (double) this.min.x)
          this.min.x = vertex.Position.X;
        if ((double) vertex.Position.Y < (double) this.min.y)
          this.min.y = vertex.Position.Y;
        if ((double) vertex.Position.Z < (double) this.min.z)
          this.min.z = vertex.Position.Z;
        if ((double) vertex.Position.X > (double) this.max.x)
          this.max.x = vertex.Position.X;
        if ((double) vertex.Position.Y > (double) this.max.y)
          this.max.y = vertex.Position.Y;
        if ((double) vertex.Position.Z > (double) this.max.z)
          this.max.z = vertex.Position.Z;
      }
      this.ext = new Vector3(this.max.x - this.min.x, this.max.y - this.min.y, this.max.z - this.min.z);
      this.texturedGeometry = new OpenGLRendererObject(new GraphicsModelData(vertex_list), true);
      this.texture_handle = opengl_texture_handle;
    }

    public int TextureHandle
    {
      get
      {
        return this.texture_handle;
      }
      set
      {
        this.texture_handle = value;
      }
    }

    public Color4 Ambient
    {
      get
      {
        return this.ambient;
      }
      set
      {
        this.ambient = value;
      }
    }

    public Color4 Diffuse
    {
      get
      {
        return this.diffuse;
      }
      set
      {
        this.diffuse = value;
      }
    }

    public Color4 Specular
    {
      get
      {
        return this.specular;
      }
      set
      {
        this.specular = value;
      }
    }

    public float Shininess
    {
      get
      {
        return this.shininess;
      }
      set
      {
        this.shininess = value;
      }
    }

    public Color4 Emission
    {
      get
      {
        return this.emission;
      }
      set
      {
        this.emission = value;
      }
    }

    public Vector3 Ext
    {
      get
      {
        return this.ext;
      }
    }

    public Vector3 Max
    {
      get
      {
        return this.max;
      }
    }

    public Vector3 Min
    {
      get
      {
        return this.min;
      }
    }
  }
}
