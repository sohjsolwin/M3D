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
      hash = vertice.x.GetHashCode();
      hash ^= vertice.y.GetHashCode();
      hash ^= vertice.z.GetHashCode();
      hash ^= normal.y.GetHashCode();
      hash ^= normal.z.GetHashCode();
      hash ^= normal.y.GetHashCode();
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
