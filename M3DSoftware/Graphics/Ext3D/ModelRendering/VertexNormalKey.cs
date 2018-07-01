// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Ext3D.ModelRendering.VertexNormalKey
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model.Utils;
using System;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class VertexNormalKey : IEquatable<VertexNormalKey>
  {
    private Vector3 vertice;
    private Vector3 normal;
    private int hash;

    public VertexNormalKey(Vector3 vertice, Vector3 normal)
    {
      this.vertice = vertice;
      this.normal = normal;
      this.hash = vertice.x.GetHashCode();
      this.hash ^= vertice.y.GetHashCode();
      this.hash ^= vertice.z.GetHashCode();
      this.hash ^= normal.y.GetHashCode();
      this.hash ^= normal.z.GetHashCode();
      this.hash ^= normal.y.GetHashCode();
    }

    public override int GetHashCode()
    {
      return this.hash;
    }

    public bool Equals(VertexNormalKey other)
    {
      if (this.vertice == other.vertice)
        return this.normal == other.normal;
      return false;
    }
  }
}
