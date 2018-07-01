// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.OpenGLRendererImmediateMode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK.Graphics.OpenGL;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class OpenGLRendererImmediateMode : OpenGLRender
  {
    public OpenGLRendererImmediateMode(GraphicsModelData graphicsModelData)
      : base(graphicsModelData)
    {
    }

    public override OpenGLRendererObject.OpenGLRenderMode RenderMode
    {
      get
      {
        return OpenGLRendererObject.OpenGLRenderMode.ImmediateMode;
      }
    }

    public override void Create()
    {
    }

    public override void Distroy()
    {
    }

    public override void DrawCallback()
    {
      GL.Begin(PrimitiveType.Triangles);
      for (int index = 0; index < this.graphicsModelData.dataTNV.Length; ++index)
      {
        GL.TexCoord2(this.graphicsModelData.dataTNV[index].TexCoord.X, this.graphicsModelData.dataTNV[index].TexCoord.Y);
        GL.Normal3(this.graphicsModelData.dataTNV[index].Normal.X, this.graphicsModelData.dataTNV[index].Normal.Y, this.graphicsModelData.dataTNV[index].Normal.Z);
        GL.Vertex3(this.graphicsModelData.dataTNV[index].Position.X, this.graphicsModelData.dataTNV[index].Position.Y, this.graphicsModelData.dataTNV[index].Position.Z);
      }
      GL.End();
    }

    public override bool isInitalized()
    {
      return false;
    }
  }
}
