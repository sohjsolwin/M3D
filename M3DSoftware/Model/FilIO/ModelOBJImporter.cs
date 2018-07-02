using M3D.GUI.Controller;
using M3D.Model.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.Model.FilIO
{
  public class ModelOBJImporter : ModelImporterInterface
  {
    private static readonly char[] mVecSeparator = new char[1]
    {
      ' '
    };
    private static readonly char[] mFaceSeparator = new char[2]
    {
      ' ',
      '/'
    };

    public ModelData Load(string filename)
    {
      try
      {
        var verticies = new LinkedList<Vector3>();
        var triangleIndecies = new LinkedList<int[]>();
        using (var streamReader = new StreamReader(filename))
        {
          string str;
          while ((str = streamReader.ReadLine()) != null)
          {
            if (str.Length >= 2)
            {
              if (str[0] == 'v' && str[1] == ' ')
              {
                string[] strArray = str.Split(ModelOBJImporter.mVecSeparator, StringSplitOptions.RemoveEmptyEntries);
                verticies.AddLast(new Vector3((float) Convert.ToDouble(strArray[1], M3DGlobalization.SYSTEM_CULTURE), (float) Convert.ToDouble(strArray[2], M3DGlobalization.SYSTEM_CULTURE), (float) Convert.ToDouble(strArray[3], M3DGlobalization.SYSTEM_CULTURE)));
              }
              if (str[0] == 'f' && str[1] == ' ')
              {
                string[] strArray = str.Split(ModelOBJImporter.mFaceSeparator);
                switch (strArray.Length)
                {
                  case 4:
                    var num1 = Convert.ToInt32(strArray[1], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num2 = Convert.ToInt32(strArray[2], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num3 = Convert.ToInt32(strArray[3], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    triangleIndecies.AddLast(new int[3]
                    {
                      num1,
                      num2,
                      num3
                    });
                    continue;
                  case 7:
                  case 8:
                    var num4 = Convert.ToInt32(strArray[1], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num5 = Convert.ToInt32(strArray[3], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num6 = Convert.ToInt32(strArray[5], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    triangleIndecies.AddLast(new int[3]
                    {
                      num4,
                      num5,
                      num6
                    });
                    continue;
                  case 10:
                  case 11:
                    var num7 = Convert.ToInt32(strArray[1], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num8 = Convert.ToInt32(strArray[4], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    var num9 = Convert.ToInt32(strArray[7], M3DGlobalization.SYSTEM_CULTURE) - 1;
                    triangleIndecies.AddLast(new int[3]
                    {
                      num7,
                      num8,
                      num9
                    });
                    continue;
                  default:
                    continue;
                }
              }
            }
          }
          streamReader.Close();
        }
        return ModelData.Create(verticies, triangleIndecies, null);
      }
      catch (Exception ex)
      {
      }
      return null;
    }
  }
}
