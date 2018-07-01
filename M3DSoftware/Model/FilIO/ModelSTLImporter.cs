// Decompiled with JetBrains decompiler
// Type: M3D.Model.FilIO.ModelSTLImporter
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
        bool flag = false;
        using (StreamReader streamReader = new StreamReader(filename))
        {
          for (int index = 0; index < 4; ++index)
          {
            string str = streamReader.ReadLine();
            if (str != null && str != null)
            {
              if (str.Contains("facet"))
              {
                flag = true;
                break;
              }
            }
            else
              break;
          }
          streamReader.Close();
        }
        if (flag)
          return this.LoadSTLASCII(filename);
        return this.LoadSTLBinary(filename);
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
        LinkedList<Vector3> verticies = new LinkedList<Vector3>();
        using (StreamReader streamReader = new StreamReader(filename))
        {
          int num1 = 0;
          float[] numArray = new float[3];
          string str;
          while ((str = streamReader.ReadLine()) != null)
          {
            if (str.Contains("vertex"))
            {
              string[] strArray = str.Substring(str.IndexOf("vertex") + 7).Split(' ');
              int num2 = 0;
              for (int index = 0; index < strArray.Length; ++index)
              {
                if (strArray[index].Length > 0)
                  numArray[num2++] = float.Parse(strArray[index], (IFormatProvider) M3DGlobalization.SYSTEM_CULTURE);
              }
              verticies.AddLast(new Vector3(numArray[0], numArray[1], numArray[2]));
            }
            else if (str.Contains("endfacet"))
              ++num1;
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
        LinkedList<Vector3> verticies = new LinkedList<Vector3>();
        byte[] buffer = new byte[80];
        using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
          using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
          {
            binaryReader.Read(buffer, 0, 80);
            int num1 = binaryReader.ReadInt32();
            for (int index = 0; index < num1; ++index)
            {
              binaryReader.BaseStream.Seek(12L, SeekOrigin.Current);
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              verticies.AddLast(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
              int num2 = (int) binaryReader.ReadInt16();
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
