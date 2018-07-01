// Decompiled with JetBrains decompiler
// Type: M3D.Model.FilIO.ModelM3DExporter
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Spooling.Common.Utils;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelM3DExporter : ModelExporter
  {
    public void Save(ModelData modelData, string filename)
    {
      using (FileStream fileStream = new FileStream(filename, FileMode.Create))
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
        {
          int vertexCount = modelData.getVertexCount();
          binaryWriter.Write(vertexCount);
          foreach (ModelData.Vertex vertex in modelData.GetAllVertexs())
          {
            float num = -vertex.x;
            float y = vertex.y;
            float z = vertex.z;
            binaryWriter.Write(y);
            binaryWriter.Write(num);
            binaryWriter.Write(z);
          }
          binaryWriter.Write(vertexCount);
          foreach (ModelData.Vertex vertex in modelData.GetAllVertexs())
          {
            float num1 = 0.0f;
            float num2 = 1f;
            float num3 = 0.0f;
            binaryWriter.Write(num2);
            binaryWriter.Write(num1);
            binaryWriter.Write(num3);
          }
          int faceCount = modelData.getFaceCount();
          binaryWriter.Write(faceCount);
          foreach (ModelData.Face allFace in modelData.GetAllFaces())
          {
            binaryWriter.Write(allFace.index1);
            binaryWriter.Write(allFace.index2);
            binaryWriter.Write(allFace.index3);
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
