using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class OpenGLRendererVBOs : OpenGLRender
  {
    public uint VboId;
    public uint IboId;
    public int Numelements;
    public int Numfaces;

    public OpenGLRendererVBOs(GraphicsModelData graphicsModelData)
      : base(graphicsModelData)
    {
    }

    public override OpenGLRendererObject.OpenGLRenderMode RenderMode
    {
      get
      {
        return OpenGLRendererObject.OpenGLRenderMode.VBOs;
      }
    }

    public override void Create()
    {
      var length1 = graphicsModelData.dataTNV.Length;
      var length2 = graphicsModelData.faces.Length;
      GL.GenBuffers(1, out VboId);
      switch (GL.GetError())
      {
        case ErrorCode.NoError:
          GL.BindBuffer(BufferTarget.ArrayBuffer, VboId);
          GL.BufferData<VertexTNV>(BufferTarget.ArrayBuffer, (IntPtr) (length1 * (Vector2.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes)), graphicsModelData.dataTNV, BufferUsageHint.StaticDraw);
          int @params;
          GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out @params);
          ErrorCode error = GL.GetError();
          switch (error)
          {
            case ErrorCode.NoError:
              if (length1 * (Vector2.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes) != @params)
              {
                throw new ApplicationException("Error while uploading VERTICES data.");
              }

              if (length2 > 0)
              {
                GL.GenBuffers(1, out IboId);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IboId);
                GL.BufferData<TriangleFace>(BufferTarget.ElementArrayBuffer, (IntPtr) (length2 * 3 * 4), graphicsModelData.faces, BufferUsageHint.StaticDraw);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
              }
              Numelements = length1;
              Numfaces = length2;
              return;
            case ErrorCode.OutOfMemory:
              throw new OutOfMemoryException("Out of GPU memory");
            default:
              throw new ApplicationException("Error while creating VERTICES Buffer Object.\n\nERROR: " + Enum.GetName(typeof (ErrorCode), (object) error));
          }
        case ErrorCode.OutOfMemory:
          throw new OutOfMemoryException("Out of GPU memory");
        default:
          throw new ApplicationException("Error while creating VERTICES Buffer Object.\n\nERROR: " + Enum.GetName(typeof (ErrorCode), (object) GL.GetError()));
      }
    }

    public override void Distroy()
    {
      if (!isInitalized())
      {
        return;
      }

      GL.DeleteBuffer(IboId);
      GL.DeleteBuffer(VboId);
      IboId = 0U;
      VboId = 0U;
      Numelements = 0;
      Numfaces = 0;
    }

    public override unsafe void DrawCallback()
    {
      GL.BindBuffer(BufferTarget.ArrayBuffer, VboId);
      GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, 0, (IntPtr) ((void*) null));
      GL.EnableVertexAttribArray(0);
      GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, true, 32, 20);
      GL.EnableVertexAttribArray(1);
      GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, 32, 8);
      if (Numfaces > 0)
      {
        GL.EnableClientState(ArrayCap.IndexArray);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, IboId);
        GL.DrawElements(PrimitiveType.Triangles, 3 * Numfaces, DrawElementsType.UnsignedInt, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.DisableClientState(ArrayCap.IndexArray);
      }
      else
      {
        GL.DrawArrays(PrimitiveType.Triangles, 0, Numelements);
      }

      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public override bool isInitalized()
    {
      return VboId > 0U;
    }
  }
}
