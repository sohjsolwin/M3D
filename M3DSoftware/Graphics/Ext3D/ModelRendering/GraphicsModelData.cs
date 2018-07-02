using M3D.Model;
using M3D.Model.Utils;
using System;
using System.Collections.Generic;

namespace M3D.Graphics.Ext3D.ModelRendering
{
  public class GraphicsModelData
  {
    public VertexTNV[] dataTNV;
    public TriangleFace[] faces;
    private VertexNormalKey vertex;

    public GraphicsModelData(List<VertexTNV> vertex_list)
    {
      faces = new TriangleFace[vertex_list.Count / 3];
      dataTNV = new VertexTNV[vertex_list.Count];
      var num = 0;
      while (num < vertex_list.Count)
      {
        for (var index = num; index < num + 3; ++index)
        {
          dataTNV[index] = vertex_list[index];
        }

        faces[num / 3] = new TriangleFace((uint) num, (uint) (num + 1), (uint) (num + 2));
        num += 3;
      }
    }

    public GraphicsModelData(ModelData modelData, bool smoothing)
    {
      smoothing = true;
      var faceCount = modelData.GetFaceCount();
      faces = new TriangleFace[faceCount];
      dataTNV = new VertexTNV[faceCount * 3];
      for (var index = 0; index < faceCount; ++index)
      {
        ModelData.Face face = modelData.GetFace(index);
        Vector3 position1 = modelData[face.Index1];
        Vector3 position2 = modelData[face.Index2];
        Vector3 position3 = modelData[face.Index3];
        if (smoothing)
        {
          dataTNV[3 * index] = new VertexTNV(cheatSmoothing(modelData.GetVertex(face.Index1), face.Normal), position1);
          dataTNV[3 * index + 1] = new VertexTNV(cheatSmoothing(modelData.GetVertex(face.Index2), face.Normal), position2);
          dataTNV[3 * index + 2] = new VertexTNV(cheatSmoothing(modelData.GetVertex(face.Index3), face.Normal), position3);
        }
        else
        {
          dataTNV[3 * index] = new VertexTNV(face.Normal, position1);
          dataTNV[3 * index + 1] = new VertexTNV(face.Normal, position2);
          dataTNV[3 * index + 2] = new VertexTNV(face.Normal, position3);
        }
        faces[index] = new TriangleFace((uint) (3 * index), (uint) (3 * index + 1), (uint) (3 * index + 2));
      }
    }

    private Vector3 cheatSmoothing(ModelData.Vertex vertex, Vector3 faceNormal)
    {
      var vector3 = new Vector3(0.0f, 0.0f, 0.0f);
      uint num1 = 0;
      for (var index = 0; index < vertex.Faces.Count; ++index)
      {
        Vector3 normal = vertex.Faces[index].Normal;
        var num2 = Math.Abs(faceNormal.Dot(normal) / (faceNormal.Length() * normal.Length()));
        if (num2 >= 0.698131680488586)
        {
          vector3 += normal * num2;
          ++num1;
        }
      }
      return (vector3 * (1f / num1)).Normalize();
    }
  }
}
