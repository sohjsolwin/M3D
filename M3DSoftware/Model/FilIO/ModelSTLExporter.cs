using M3D.Model.Utils;
using M3D.Spooling.Common.Utils;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelSTLExporter : IModelExporter
  {
    public void Save(ModelData modelData, string filename)
    {
      byte[] buffer = new byte[80];
      short num = 0;
      using (var fileStream = new FileStream(filename, FileMode.Create))
      {
        using (var binaryWriter = new BinaryWriter(fileStream))
        {
          binaryWriter.Write(buffer, 0, 80);
          var faceCount = modelData.GetFaceCount();
          binaryWriter.Write(faceCount);
          for (var index = 0; index < faceCount; ++index)
          {
            Vector3 _a = modelData[modelData.GetFace(index).Index1];
            Vector3 _b = modelData[modelData.GetFace(index).Index2];
            Vector3 _c = modelData[modelData.GetFace(index).Index3];
            Vector3 vector3 = ModelData.CalcNormal(_a, _b, _c);
            binaryWriter.Write(vector3.X);
            binaryWriter.Write(vector3.Y);
            binaryWriter.Write(vector3.Z);
            binaryWriter.Write(_a.X);
            binaryWriter.Write(_a.Y);
            binaryWriter.Write(_a.Z);
            binaryWriter.Write(_b.X);
            binaryWriter.Write(_b.Y);
            binaryWriter.Write(_b.Z);
            binaryWriter.Write(_c.X);
            binaryWriter.Write(_c.Y);
            binaryWriter.Write(_c.Z);
            binaryWriter.Write(num);
          }
          binaryWriter.Close();
        }
        fileStream.Close();
      }
      FileUtils.GrantAccess(filename);
    }
  }
}
