// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.SaveModelFileDialog
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Windows.Forms;

namespace M3D.GUI.Views
{
  public static class SaveModelFileDialog
  {
    public static string RunSaveFileDialog(SaveModelFileDialog.FileType fileType)
    {
      string str1 = "";
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      switch (fileType)
      {
        case SaveModelFileDialog.FileType.Models:
          string str2 = str1 + "Supported Files|*.stl;*.obj;|" + "STL File (.stl)|*.stl|" + "OBJ File (.obj)|*.obj|";
          saveFileDialog.Filter = str2;
          saveFileDialog.FilterIndex = 1;
          break;
        case SaveModelFileDialog.FileType.GCode:
          saveFileDialog.Filter = "Gcode File (.gcode)|*.gcode|Text File (.txt)|*.txt|All Files (*.*)|*.*";
          saveFileDialog.FilterIndex = 1;
          break;
      }
      if (saveFileDialog.ShowDialog() == DialogResult.OK)
        return saveFileDialog.FileName;
      return (string) null;
    }

    public enum FileType
    {
      Models,
      GCode,
    }
  }
}
