using System.IO;

namespace M3D.GUI.Tools
{
  internal class SplitFileName
  {
    public string name;
    public string ext;
    public string path;

    public SplitFileName(string filepath)
    {
      ext = Path.GetExtension(filepath);
      ext = ext.TrimStart('.');
      name = Path.GetFileNameWithoutExtension(filepath);
      path = Path.GetDirectoryName(filepath);
      path += !path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : "";
    }
  }
}
