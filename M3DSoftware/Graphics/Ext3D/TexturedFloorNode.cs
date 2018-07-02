using M3D.Graphics.Ext3D.ModelRendering;
using OpenTK;
using System.Collections.Generic;

namespace M3D.Graphics.Ext3D
{
  public class TexturedFloorNode : CustomShape
  {
    public TexturedFloorNode()
      : this(0, null)
    {
    }

    public TexturedFloorNode(int ID)
      : this(ID, null)
    {
    }

    public TexturedFloorNode(int ID, Element3D parent)
      : base(ID, parent)
    {
    }

    public void Create(M3D.Model.Utils.Vector3 pos, float xWidth, float yWidth, int opengl_texture_handle)
    {
      var num1 = xWidth / 2f;
      var num2 = yWidth / 2f;
      var vector3_1 = new M3D.Model.Utils.Vector3(pos.X + num1, pos.Y + num2, pos.Z);
      var vector3_2 = new M3D.Model.Utils.Vector3(pos.X - num1, pos.Y - num2, pos.Z);
      Create(new List<VertexTNV>()
      {
        new VertexTNV(new Vector2(0.0f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z)),
        new VertexTNV(new Vector2(1f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_1.Z)),
        new VertexTNV(new Vector2(1f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)),
        new VertexTNV(new Vector2(1f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)),
        new VertexTNV(new Vector2(0.0f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_1.Z)),
        new VertexTNV(new Vector2(0.0f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z))
      }, opengl_texture_handle);
    }
  }
}
