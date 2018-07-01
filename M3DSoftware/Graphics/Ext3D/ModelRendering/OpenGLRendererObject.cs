// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.OpenGLRendererObject
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.GUI.Forms;
using M3D.Model;
using System;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class OpenGLRendererObject
  {
    private int try_count;
    private GraphicsModelData graphicsModelData;
    private OpenGLRender openGLRender;
    public static OpenGLRendererObject.OpenGLRenderMode openGLRenderMode;
    private bool useTextures;

    public OpenGLRendererObject(ModelData model)
      : this(new GraphicsModelData(model, model.getVertexCount() < 10000), false)
    {
    }

    public OpenGLRendererObject(GraphicsModelData model, bool useTextures)
    {
      this.useTextures = useTextures;
      this.graphicsModelData = model;
    }

    public void Reset()
    {
      try
      {
        this.Delete();
      }
      catch (Exception ex)
      {
      }
      try
      {
        this.Create();
      }
      catch (Exception ex)
      {
      }
    }

    public void Delete()
    {
      try
      {
        if (this.openGLRender == null)
          return;
        this.openGLRender.Dispose();
        this.openGLRender = (OpenGLRender) null;
      }
      catch (Exception ex)
      {
      }
    }

    public void Create()
    {
      this.Delete();
      switch (OpenGLRendererObject.openGLRenderMode)
      {
        case OpenGLRendererObject.OpenGLRenderMode.VBOs:
          this.openGLRender = (OpenGLRender) new OpenGLRendererVBOs(this.graphicsModelData);
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
          this.openGLRender = (OpenGLRender) new OpenGLRendererARBVBOs(this.graphicsModelData);
          break;
        default:
          this.openGLRender = (OpenGLRender) new OpenGLRendererImmediateMode(this.graphicsModelData);
          break;
      }
      this.openGLRender.Create();
    }

    public void Draw()
    {
      try
      {
        if (this.openGLRender != null && this.openGLRender.RenderMode != OpenGLRendererObject.openGLRenderMode)
          this.Delete();
        if (this.openGLRender == null)
          this.Create();
        this.openGLRender.Draw();
      }
      catch (Exception ex)
      {
        this.Reset();
        if (OpenGLRendererObject.openGLRenderMode == OpenGLRendererObject.OpenGLRenderMode.ImmediateMode)
        {
          ++this.try_count;
          if (this.try_count <= 2)
            return;
          ExceptionForm.ShowExceptionForm(new Exception("VBOObject::Draw::Failure", ex));
        }
        else
        {
          this.Reset();
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
        }
      }
    }

    public void Translate(float x, float y, float z)
    {
      for (int index = 0; index < this.graphicsModelData.dataTNV.Length; ++index)
      {
        this.graphicsModelData.dataTNV[index].Position.X += x;
        this.graphicsModelData.dataTNV[index].Position.Y += y;
        this.graphicsModelData.dataTNV[index].Position.Z += z;
      }
    }

    public void Scale(float x, float y, float z)
    {
      for (int index = 0; index < this.graphicsModelData.dataTNV.Length; ++index)
      {
        this.graphicsModelData.dataTNV[index].Position.X *= x;
        this.graphicsModelData.dataTNV[index].Position.Y *= y;
        this.graphicsModelData.dataTNV[index].Position.Z *= z;
      }
    }

    public enum OpenGLRenderMode
    {
      VBOs,
      ARBVBOs,
      ImmediateMode,
    }
  }
}
