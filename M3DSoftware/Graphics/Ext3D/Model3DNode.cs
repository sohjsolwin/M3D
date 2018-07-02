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
      Brightness = 1f;
    }

    public override void Render3D()
    {
      if (Show)
      {
        if (Highlight)
        {
          DrawOutline();
        }

        var @params = new Color4(Diffuse.R * Brightness, Diffuse.G * Brightness, Diffuse.B * Brightness, Diffuse.A);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, Ambient);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, @params);
        GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, Specular);
        GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);
        GL.DepthMask(true);
        if (geometryData != null)
        {
          GL.BindTexture(TextureTarget.Texture2D, 0);
          geometryData.Draw();
        }
      }
      base.Render3D();
    }

    private void DrawOutline2()
    {
      GL.PushMatrix();
      var scale = Matrix4.CreateScale(1.06f);
      GL.MultMatrix(ref scale);
      var num = GL.IsEnabled(EnableCap.Lighting) ? 1 : 0;
      if (num != 0)
      {
        GL.Disable(EnableCap.Lighting);
      }

      GL.Color4(new Color4(byte.MaxValue, (byte) 128, (byte) 0, byte.MaxValue));
      GL.DepthMask(false);
      if (geometryData != null)
      {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        geometryData.Draw();
      }
      if (num != 0)
      {
        GL.Enable(EnableCap.Lighting);
      }

      GL.PopMatrix();
    }

    private void DrawOutline()
    {
      var num = GL.IsEnabled(EnableCap.Lighting) ? 1 : 0;
      if (num != 0)
      {
        GL.Disable(EnableCap.Lighting);
      }

      GL.Color4(new Color4(byte.MaxValue, (byte) 128, (byte) 0, byte.MaxValue));
      GL.LineWidth(4f);
      GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
      GL.DepthMask(false);
      if (geometryData != null)
      {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        geometryData.Draw();
      }
      GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
      if (num == 0)
      {
        return;
      }

      GL.Enable(EnableCap.Lighting);
    }

    public List<M3D.Model.Utils.Vector3> CalculateBoundingUsingTransformMatric(Matrix4 matrix)
    {
      return modelData.CalculateHullPointsUsingTransformMatrix(matrix);
    }

    public void TranslateModelData(float x, float y, float z)
    {
      modelData.Translate(new M3D.Model.Utils.Vector3(x, y, z));
      geometryData.Translate(x, y, z);
      geometryData.Reset();
    }

    public void Scale(float scalex, float scaley, float scalez)
    {
      modelData.Scale(new M3D.Model.Utils.Vector3(scalex, scaley, scalez));
      geometryData.Scale(scalex, scaley, scalez);
      geometryData.Reset();
    }

    public virtual void SetModel(M3D.Graphics.Ext3D.ModelRendering.Model model)
    {
      modelData = model.modelData;
      geometryData = model.geometryData;
      fileName = model.fileName;
      zipFileName = model.zipFileName;
    }

    public ModelSize CalculateMinMax()
    {
      return new ModelSize(modelData.Min, modelData.Max);
    }

    public ModelSize CalculateMinMax(Matrix4 matrix)
    {
      modelData.GetMinMaxWithTransform(matrix, out Model.Utils.Vector3 min, out Model.Utils.Vector3 max);
      return new ModelSize(min, max);
    }

    public M3D.Graphics.Ext3D.ModelRendering.Model GetModelInfo()
    {
      return new M3D.Graphics.Ext3D.ModelRendering.Model(modelData, geometryData, fileName, zipFileName);
    }

    public M3D.Model.Utils.Vector3 Ext
    {
      get
      {
        return CalculateMinMax().Ext;
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
        return modelData;
      }
    }

    public bool Highlight { get; set; }
  }
}
