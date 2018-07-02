using M3D.Spooling.Common.Utils;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelM3DExporter : IModelExporter
  {
    public void Save(ModelData modelData, string filename)
    {
      using (var fileStream = new FileStream(filename, FileMode.Create))
      {
        using (var binaryWriter = new BinaryWriter(fileStream))
        {
          var vertexCount = modelData.GetVertexCount();
          binaryWriter.Write(vertexCount);
          foreach (ModelData.Vertex vertex in modelData.GetAllVertexs())
          {
            var num = -vertex.X;
            var y = vertex.Y;
            var z = vertex.Z;
            binaryWriter.Write(y);
            binaryWriter.Write(num);
            binaryWriter.Write(z);
          }
          binaryWriter.Write(vertexCount);
          foreach (ModelData.Vertex vertex in modelData.GetAllVertexs())
          {
            var num1 = 0.0f;
            var num2 = 1f;
            var num3 = 0.0f;
            binaryWriter.Write(num2);
            binaryWriter.Write(num1);
            binaryWriter.Write(num3);
          }
          var faceCount = modelData.GetFaceCount();
          binaryWriter.Write(faceCount);
          foreach (ModelData.Face allFace in modelData.GetAllFaces())
          {
            binaryWriter.Write(allFace.Index1);
            binaryWriter.Write(allFace.Index2);
            binaryWriter.Write(allFace.Index3);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
          }
          binaryWriter.Close();
        }
        fileStream.Close();
      }
      FileUtils.GrantAccess(filename);
    }
  }
}
