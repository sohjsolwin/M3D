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
      Normal = new OpenTK.Vector3(normal.x, normal.y, normal.z);
      Position = new OpenTK.Vector3(position.x, position.y, position.z);
      TexCoord = new Vector2(0.0f, 0.0f);
    }

    public VertexTNV(Vector2 texCoord, M3D.Model.Utils.Vector3 normal, M3D.Model.Utils.Vector3 position)
    {
      TexCoord = texCoord;
      Normal = new OpenTK.Vector3(normal.x, normal.y, normal.z);
      Position = new OpenTK.Vector3(position.x, position.y, position.z);
    }
  }
}
