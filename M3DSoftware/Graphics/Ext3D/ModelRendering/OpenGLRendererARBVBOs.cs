// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.OpenGLRendererARBVBOs
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class OpenGLRendererARBVBOs : OpenGLRender
  {
    public uint VboId;
    public int Numelements;

    public OpenGLRendererARBVBOs(GraphicsModelData graphicsModelData)
      : base(graphicsModelData)
    {
      this.VboId = 0U;
      this.Numelements = 0;
    }

    public override OpenGLRendererObject.OpenGLRenderMode RenderMode
    {
      get
      {
        return OpenGLRendererObject.OpenGLRenderMode.ARBVBOs;
      }
    }

    public override void Create()
    {
      int length = this.graphicsModelData.dataTNV.Length;
      GL.Arb.GenBuffers(1, out this.VboId);
      if (GL.GetError() != ErrorCode.NoError && Enum.GetName(typeof (ErrorCode), (object) GL.GetError()) != "NoError")
        throw new ApplicationException("Error while creating VERTICES Buffer Object.\n\nERROR: " + Enum.GetName(typeof (ErrorCode), (object) GL.GetError()));
      GL.Arb.BindBuffer(BufferTargetArb.ArrayBuffer, this.VboId);
      GL.Arb.BufferData<VertexTNV>(BufferTargetArb.ArrayBuffer, (IntPtr) (length * (Vector2.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes)), this.graphicsModelData.dataTNV, BufferUsageArb.StaticDraw);
      int @params;
      GL.Arb.GetBufferParameter(BufferTargetArb.ArrayBuffer, BufferParameterNameArb.BufferSize, out @params);
      if (GL.GetError() != ErrorCode.NoError)
        throw new ApplicationException("Error while creating VERTICES Buffer Object.\n\nERROR: " + Enum.GetName(typeof (ErrorCode), (object) GL.GetError()));
      if (length * (Vector2.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes) != @params)
        throw new ApplicationException("Error while uploading VERTICES data.");
      this.Numelements = length;
    }

    public override void Distroy()
    {
      if (!this.isInitalized())
        return;
      GL.Arb.DeleteBuffer(this.VboId);
      this.VboId = 0U;
      this.Numelements = 0;
    }

    public override unsafe void DrawCallback()
    {
      GL.Arb.BindBuffer(BufferTargetArb.ArrayBuffer, this.VboId);
      GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, 0, (IntPtr) ((void*) null));
      GL.Arb.EnableVertexAttribArray(0);
      GL.Arb.VertexAttribPointer(0, 3, VertexAttribPointerTypeArb.Float, true, 32, (IntPtr) 20);
      GL.Arb.EnableVertexAttribArray(1);
      GL.Arb.VertexAttribPointer(1, 3, VertexAttribPointerTypeArb.Float, true, 32, (IntPtr) 8);
      GL.DrawArrays(PrimitiveType.Triangles, 0, this.Numelements);
    }

    public override bool isInitalized()
    {
      return this.VboId > 0U;
    }
  }
}
