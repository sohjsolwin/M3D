// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.OpenModelFileDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Windows.Forms;

namespace M3D.GUI.Views
{
  public static class OpenModelFileDialog
  {
    public static string RunOpenModelDialog(OpenModelFileDialog.FileType fileType)
    {
      string str1 = "";
      OpenFileDialog openFileDialog = new OpenFileDialog();
      switch (fileType)
      {
        case OpenModelFileDialog.FileType.Models:
          string str2 = str1 + "Supported Files|*.stl;*.obj;*.zip|" + "STL File (.stl)|*.stl|" + "OBJ File (.obj)|*.obj|" + "ZIP File (.zip)|*.zip|" + "All Files (*.*)|*.*";
          openFileDialog.Filter = str2;
          openFileDialog.FilterIndex = 1;
          break;
        case OpenModelFileDialog.FileType.GCode:
          openFileDialog.Filter = "Gcode File (.gcode)|*.gcode|Text File (.txt)|*.txt|All Files (*.*)|*.*";
          openFileDialog.FilterIndex = 1;
          break;
      }
      openFileDialog.Multiselect = false;
      if (openFileDialog.ShowDialog() == DialogResult.OK)
        return openFileDialog.FileName;
      return (string) null;
    }

    public enum FileType
    {
      Models,
      GCode,
    }
  }
}
