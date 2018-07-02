using M3D.Graphics.Ext3D.ModelRendering;
using OpenTK;
using System.Collections.Generic;

namespace M3D.Graphics.Ext3D
{
  public class FrontFacingPlaneNode : CustomShape
  {
    public FrontFacingPlaneNode()
      : this(0, (Element3D) null)
    {
    }

    public FrontFacingPlaneNode(int ID)
      : this(ID, (Element3D) null)
    {
    }

    public FrontFacingPlaneNode(int ID, Element3D parent)
      : base(ID, parent)
    {
    }

    public void Create(M3D.Model.Utils.Vector3 pos, float xWidth, float zWidth, int opengl_texture_handle)
    {
      var num1 = xWidth / 2f;
      var num2 = zWidth / 2f;
      var vector3_1 = new M3D.Model.Utils.Vector3(pos.x + num1, pos.y, pos.z + num2);
      var vector3_2 = new M3D.Model.Utils.Vector3(pos.x - num1, pos.y, pos.z - num2);
      Create(new List<VertexTNV>()
      {
        new VertexTNV(new Vector2(0.0f, 1f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)),
        new VertexTNV(new Vector2(1f, 1f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)),
        new VertexTNV(new Vector2(1f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)),
        new VertexTNV(new Vector2(1f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)),
        new VertexTNV(new Vector2(0.0f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)),
        new VertexTNV(new Vector2(0.0f, 1f), new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z))
      }, opengl_texture_handle);
    }
  }
}
