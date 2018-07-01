// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.Model3DNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D.ModelRendering;
using M3D.Model;
using M3D.Spooling.Common;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace M3D.Graphics.Ext3D
{
  public class Model3DNode : Element3D
  {
    public bool Show = true;
    private ThreadSafeVariable<bool> highlight = new ThreadSafeVariable<bool>(false);
    public float Brightness;
    public string fileName;
    public string zipFileName;
    private ModelData modelData;
    private OpenGLRendererObject geometryData;

    public Model3DNode()
      : this(0, (Element3D) null)
    {
    }

    public Model3DNode(int ID)
      : this(ID, (Element3D) null)
    {
    }

    public Model3DNode(int ID, Element3D parent)
      : base(ID, parent)
    {
      this.Brightness = 1f;
    }

    public override void Render3D()
    {
      if (this.Show)
      {
        if (this.Highlight)
          this.DrawOutline();
        Color4 @params = new Color4(this.Diffuse.R * this.Brightness, this.Diffuse.G * this.Brightness, this.Diffuse.B * this.Brightness, this.Diffuse.A);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, this.Ambient);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, @params);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, this.Specular);
        GL.Material(MaterialFace.Front, MaterialParameter.Shininess, this.Shininess);
        GL.DepthMask(true);
        if (this.geometryData != null)
        {
          GL.BindTexture(TextureTarget.Texture2D, 0);
          this.geometryData.Draw();
        }
      }
      base.Render3D();
    }

    private void DrawOutline2()
    {
      GL.PushMatrix();
      Matrix4 scale = Matrix4.CreateScale(1.06f);
      GL.MultMatrix(ref scale);
      int num = GL.IsEnabled(EnableCap.Lighting) ? 1 : 0;
      if (num != 0)
        GL.Disable(EnableCap.Lighting);
      GL.Color4(new Color4(byte.MaxValue, (byte) 128, (byte) 0, byte.MaxValue));
      GL.DepthMask(false);
      if (this.geometryData != null)
      {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        this.geometryData.Draw();
      }
      if (num != 0)
        GL.Enable(EnableCap.Lighting);
      GL.PopMatrix();
    }

    private void DrawOutline()
    {
      int num = GL.IsEnabled(EnableCap.Lighting) ? 1 : 0;
      if (num != 0)
        GL.Disable(EnableCap.Lighting);
      GL.Color4(new Color4(byte.MaxValue, (byte) 128, (byte) 0, byte.MaxValue));
      GL.LineWidth(4f);
      GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
      GL.DepthMask(false);
      if (this.geometryData != null)
      {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        this.geometryData.Draw();
      }
      GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
      if (num == 0)
        return;
      GL.Enable(EnableCap.Lighting);
    }

    public List<M3D.Model.Utils.Vector3> CalculateBoundingUsingTransformMatric(Matrix4 matrix)
    {
      return this.modelData.CalculateHullPointsUsingTransformMatrix(matrix);
    }

    public void TranslateModelData(float x, float y, float z)
    {
      this.modelData.Translate(new M3D.Model.Utils.Vector3(x, y, z));
      this.geometryData.Translate(x, y, z);
      this.geometryData.Reset();
    }

    public void Scale(float scalex, float scaley, float scalez)
    {
      this.modelData.Scale(new M3D.Model.Utils.Vector3(scalex, scaley, scalez));
      this.geometryData.Scale(scalex, scaley, scalez);
      this.geometryData.Reset();
    }

    public virtual void SetModel(M3D.Graphics.Ext3D.ModelRendering.Model model)
    {
      this.modelData = model.modelData;
      this.geometryData = model.geometryData;
      this.fileName = model.fileName;
      this.zipFileName = model.zipFileName;
    }

    public ModelSize CalculateMinMax()
    {
      return new ModelSize(this.modelData.Min, this.modelData.Max);
    }

    public ModelSize CalculateMinMax(Matrix4 matrix)
    {
      M3D.Model.Utils.Vector3 min;
      M3D.Model.Utils.Vector3 max;
      this.modelData.GetMinMaxWithTransform(matrix, out min, out max);
      return new ModelSize(min, max);
    }

    public M3D.Graphics.Ext3D.ModelRendering.Model GetModelInfo()
    {
      return new M3D.Graphics.Ext3D.ModelRendering.Model(this.modelData, this.geometryData, this.fileName, this.zipFileName);
    }

    public M3D.Model.Utils.Vector3 Ext
    {
      get
      {
        return this.CalculateMinMax().Ext;
      }
    }

    public Color4 Ambient { get; set; }

    public Color4 Diffuse { get; set; }

    public Color4 Specular { get; set; }

    public float Shininess { get; set; }

    public ModelData ModelData
    {
      get
      {
        return this.modelData;
      }
    }

    public bool Highlight { get; set; }
  }
}
