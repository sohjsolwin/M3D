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
      hash = vertice.X.GetHashCode();
      hash ^= vertice.Y.GetHashCode();
      hash ^= vertice.Z.GetHashCode();
      hash ^= normal.Y.GetHashCode();
      hash ^= normal.Z.GetHashCode();
      hash ^= normal.Y.GetHashCode();
    }

    public override int GetHashCode()
    {
      return hash;
    }

    public bool Equals(VertexNormalKey other)
    {
      if (vertice == other.vertice)
      {
        return normal == other.normal;
      }

      return false;
    }
  }
}
