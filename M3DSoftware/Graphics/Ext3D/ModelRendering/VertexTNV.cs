// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.VertexTNV
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using OpenTK;
using System.Runtime.InteropServices;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  [StructLayout(LayoutKind.Sequential, Pack = 1)]
  public struct VertexTNV
  {
    public Vector2 TexCoord;
    public OpenTK.Vector3 Normal;
    public OpenTK.Vector3 Position;

    public VertexTNV(M3D.Model.Utils.Vector3 normal, M3D.Model.Utils.Vector3 position)
    {
      this.Normal = new OpenTK.Vector3(normal.x, normal.y, normal.z);
      this.Position = new OpenTK.Vector3(position.x, position.y, position.z);
      this.TexCoord = new Vector2(0.0f, 0.0f);
    }

    public VertexTNV(Vector2 texCoord, M3D.Model.Utils.Vector3 normal, M3D.Model.Utils.Vector3 position)
    {
      this.TexCoord = texCoord;
      this.Normal = new OpenTK.Vector3(normal.x, normal.y, normal.z);
      this.Position = new OpenTK.Vector3(position.x, position.y, position.z);
    }
  }
}
