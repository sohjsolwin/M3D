using M3D.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelM3DImporter : ModelImporterInterface
  {
    public ModelData Load(string filename)
    {
      try
      {
        var verticies = new LinkedList<Vector3>();
        var triangleIndecies = new LinkedList<int[]>();
        using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
          using (var binaryReader = new BinaryReader((Stream) fileStream))
          {
            var num1 = binaryReader.ReadInt32();
            for (var index = 0; index < num1; ++index)
            {
              var y = binaryReader.ReadSingle();
              var x = -binaryReader.ReadSingle();
              var z = binaryReader.ReadSingle();
              verticies.AddLast(new Vector3(x, y, z));
            }
            var num2 = binaryReader.ReadInt32();
            binaryReader.BaseStream.Seek((long) (12 * num2), SeekOrigin.Current);
            var num3 = binaryReader.ReadInt32();
            for (var index = 0; index < num3; ++index)
            {
              var num4 = binaryReader.ReadInt32();
              var num5 = binaryReader.ReadInt32();
              var num6 = binaryReader.ReadInt32();
              triangleIndecies.AddLast(new int[3]
              {
                num4,
                num5,
                num6
              });
              binaryReader.BaseStream.Seek(12L, SeekOrigin.Current);
            }
            binaryReader.Close();
          }
          fileStream.Close();
        }
        return ModelData.Create(verticies, triangleIndecies, (ProgressHelper.PercentageDelagate) null);
      }
      catch (Exception ex)
      {
      }
      return (ModelData) null;
    }
  }
}
