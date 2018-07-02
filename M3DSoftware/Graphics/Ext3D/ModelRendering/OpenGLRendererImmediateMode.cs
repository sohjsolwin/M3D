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
      for (var index = 0; index < graphicsModelData.dataTNV.Length; ++index)
      {
        GL.TexCoord2(graphicsModelData.dataTNV[index].TexCoord.X, graphicsModelData.dataTNV[index].TexCoord.Y);
        GL.Normal3(graphicsModelData.dataTNV[index].Normal.X, graphicsModelData.dataTNV[index].Normal.Y, graphicsModelData.dataTNV[index].Normal.Z);
        GL.Vertex3(graphicsModelData.dataTNV[index].Position.X, graphicsModelData.dataTNV[index].Position.Y, graphicsModelData.dataTNV[index].Position.Z);
      }
      GL.End();
    }

    public override bool isInitalized()
    {
      return false;
    }
  }
}
