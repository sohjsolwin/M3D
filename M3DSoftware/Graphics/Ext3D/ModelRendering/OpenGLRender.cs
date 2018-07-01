﻿// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.OpenGLRender
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      if (!this.isInitalized())
        this.Create();
      this.DrawCallback();
    }

    public abstract void DrawCallback();

    public abstract void Create();

    public abstract void Distroy();

    public abstract bool isInitalized();

    public abstract OpenGLRendererObject.OpenGLRenderMode RenderMode { get; }

    public void Dispose()
    {
      if (this.isInitalized())
        return;
      this.Distroy();
    }
  }
}
