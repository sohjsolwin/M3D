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
      : this(new GraphicsModelData(model, model.GetVertexCount() < 10000), false)
    {
    }

    public OpenGLRendererObject(GraphicsModelData model, bool useTextures)
    {
      this.useTextures = useTextures;
      graphicsModelData = model;
    }

    public void Reset()
    {
      try
      {
        Delete();
      }
      catch (Exception ex)
      {
      }
      try
      {
        Create();
      }
      catch (Exception ex)
      {
      }
    }

    public void Delete()
    {
      try
      {
        if (openGLRender == null)
        {
          return;
        }

        openGLRender.Dispose();
        openGLRender = (OpenGLRender) null;
      }
      catch (Exception ex)
      {
      }
    }

    public void Create()
    {
      Delete();
      switch (OpenGLRendererObject.openGLRenderMode)
      {
        case OpenGLRendererObject.OpenGLRenderMode.VBOs:
          openGLRender = (OpenGLRender) new OpenGLRendererVBOs(graphicsModelData);
          break;
        case OpenGLRendererObject.OpenGLRenderMode.ARBVBOs:
          openGLRender = (OpenGLRender) new OpenGLRendererARBVBOs(graphicsModelData);
          break;
        default:
          openGLRender = (OpenGLRender) new OpenGLRendererImmediateMode(graphicsModelData);
          break;
      }
      openGLRender.Create();
    }

    public void Draw()
    {
      try
      {
        if (openGLRender != null && openGLRender.RenderMode != OpenGLRendererObject.openGLRenderMode)
        {
          Delete();
        }

        if (openGLRender == null)
        {
          Create();
        }

        openGLRender.Draw();
      }
      catch (Exception ex)
      {
        Reset();
        if (OpenGLRendererObject.openGLRenderMode == OpenGLRendererObject.OpenGLRenderMode.ImmediateMode)
        {
          ++try_count;
          if (try_count <= 2)
          {
            return;
          }

          ExceptionForm.ShowExceptionForm(new Exception("VBOObject::Draw::Failure", ex));
        }
        else
        {
          Reset();
          OpenGLRendererObject.openGLRenderMode = OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
        }
      }
    }

    public void Translate(float x, float y, float z)
    {
      for (var index = 0; index < graphicsModelData.dataTNV.Length; ++index)
      {
        graphicsModelData.dataTNV[index].Position.X += x;
        graphicsModelData.dataTNV[index].Position.Y += y;
        graphicsModelData.dataTNV[index].Position.Z += z;
      }
    }

    public void Scale(float x, float y, float z)
    {
      for (var index = 0; index < graphicsModelData.dataTNV.Length; ++index)
      {
        graphicsModelData.dataTNV[index].Position.X *= x;
        graphicsModelData.dataTNV[index].Position.Y *= y;
        graphicsModelData.dataTNV[index].Position.Z *= z;
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
