using System;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public abstract class OpenGLRender : IDisposable
  {
    public GraphicsModelData graphicsModelData;
    public bool useTextures;

    public OpenGLRender(GraphicsModelData graphicsModelData)
    {
      this.graphicsModelData = graphicsModelData;
    }

    public void Draw()
    {
      if (!isInitalized())
      {
        Create();
      }

      DrawCallback();
    }

    public abstract void DrawCallback();

    public abstract void Create();

    public abstract void Distroy();

    public abstract bool isInitalized();

    public abstract OpenGLRendererObject.OpenGLRenderMode RenderMode { get; }

    public void Dispose()
    {
      if (isInitalized())
      {
        return;
      }

      Distroy();
    }
  }
}
