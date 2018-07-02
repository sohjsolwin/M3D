using M3D.GUI.Controller;
using M3D.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelSTLImporter : ModelImporterInterface
  {
    public ModelData Load(string filename)
    {
      try
      {
        var flag = false;
        using (var streamReader = new StreamReader(filename))
        {
          for (var index = 0; index < 4; ++index)
          {
            var str = streamReader.ReadLine();
            if (str != null && str != null)
            {
              if (str.Contains("facet"))
              {
                flag = true;
                break;
              }
            }
            else
            {
              break;
            }
          }
          streamReader.Close();
        }
        if (flag)
        {
          return LoadSTLASCII(filename);
        }

        return LoadSTLBinary(filename);
      }
      catch (Exception ex)
      {
      }
      return (ModelData) null;
    }

    public ModelData LoadSTLASCII(string filename)
    {
      try
      {
        var verticies = new LinkedList<Vector3>();
        using (var streamReader = new StreamReader(filename))
        {
          var num1 = 0;
          float[] numArray = new float[3];
          string str;
          while ((str = streamReader.ReadLine()) != null)
          {
            if (str.Contains("vertex"))
            {
              string[] strArray = str.Substring(str.IndexOf("vertex") + 7).Split(' ');
              var num2 = 0;
              for (var index = 0; index < strArray.Length; ++index)
              {
                if (strArray[index].Length > 0)
                {
                  numArray[num2++] = float.Parse(strArray[index], (IFormatProvider) M3DGlobalization.SYSTEM_CULTURE);
                }
              }
              verticies.AddLast(new Vector3(numArray[0], numArray[1], numArray[2]));
            }
            else if (str.Contains("endfacet"))
            {
              ++num1;
            }
          }
          streamReader.Close();
        }
        return ModelData.Create(verticies, (LinkedList<int[]>) null, (ProgressHelper.PercentageDelagate) null);
      }
      catch (Exception ex)
      {
      }
      return (ModelData) null;
    }

    public ModelData LoadSTLBinary(string filename)
    {
      try
      {
        var verticies = new LinkedList<Vector3>();
        byte[] buffer = new byte[80];
        using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
          using (var binaryReader = new BinaryReader((Stream) fileStream))
          {
            binaryReader.Read(buffer, 0, 80);
            var num1 = binaryReader.ReadInt32();
            for (var index = 0; index < num1; ++index)
            {
              binaryReader.BaseStream.Seek(12L, SeekOrigin.Current);
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              var num2 = (int) binaryReader.ReadInt16();
            }
            binaryReader.Close();
          }
          fileStream.Close();
        }
        return ModelData.Create(verticies, (LinkedList<int[]>) null, (ProgressHelper.PercentageDelagate) null);
      }
      catch (Exception ex)
      {
      }
      return (ModelData) null;
    }
  }
}
