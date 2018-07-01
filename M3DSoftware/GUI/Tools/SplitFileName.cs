// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Tools.SplitFileName
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.ext = Path.GetExtension(filepath);
      this.ext = this.ext.TrimStart('.');
      this.name = Path.GetFileNameWithoutExtension(filepath);
      this.path = Path.GetDirectoryName(filepath);
      this.path += !this.path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? Path.DirectorySeparatorChar.ToString() : "";
    }
  }
}
