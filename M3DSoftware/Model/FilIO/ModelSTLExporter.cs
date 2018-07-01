// Decompiled with JetBrains decompiler
// Type: M3D.Model.FilIO.ModelSTLExporter
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Model.Utils;
using M3D.Spooling.Common.Utils;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelSTLExporter : ModelExporter
  {
    public void Save(ModelData modelData, string filename)
    {
      byte[] buffer = new byte[80];
      short num = 0;
      using (FileStream fileStream = new FileStream(filename, FileMode.Create))
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
        {
          binaryWriter.Write(buffer, 0, 80);
          int faceCount = modelData.getFaceCount();
          binaryWriter.Write(faceCount);
          for (int index = 0; index < faceCount; ++index)
          {
            Vector3 _a = modelData[modelData.getFace(index).index1];
            Vector3 _b = modelData[modelData.getFace(index).index2];
            Vector3 _c = modelData[modelData.getFace(index).index3];
            Vector3 vector3 = ModelData.CalcNormal(_a, _b, _c);
            binaryWriter.Write(vector3.x);
            binaryWriter.Write(vector3.y);
            binaryWriter.Write(vector3.z);
            binaryWriter.Write(_a.x);
            binaryWriter.Write(_a.y);
            binaryWriter.Write(_a.z);
            binaryWriter.Write(_b.x);
            binaryWriter.Write(_b.y);
            binaryWriter.Write(_b.z);
            binaryWriter.Write(_c.x);
            binaryWriter.Write(_c.y);
            binaryWriter.Write(_c.z);
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
