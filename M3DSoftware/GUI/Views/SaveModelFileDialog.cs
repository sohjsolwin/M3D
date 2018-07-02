using System.Windows.Forms;

namespace M3D.GUI.Views
{
  public static class SaveModelFileDialog
  {
    public static string RunSaveFileDialog(SaveModelFileDialog.FileType fileType)
    {
      var str1 = "";
      var saveFileDialog = new SaveFileDialog();
      switch (fileType)
      {
        case SaveModelFileDialog.FileType.Models:
          var str2 = str1 + "Supported Files|*.stl;*.obj;|" + "STL File (.stl)|*.stl|" + "OBJ File (.obj)|*.obj|";
          saveFileDialog.Filter = str2;
          saveFileDialog.FilterIndex = 1;
          break;
        case SaveModelFileDialog.FileType.GCode:
          saveFileDialog.Filter = "Gcode File (.gcode)|*.gcode|Text File (.txt)|*.txt|All Files (*.*)|*.*";
          saveFileDialog.FilterIndex = 1;
          break;
      }
      if (saveFileDialog.ShowDialog() == DialogResult.OK)
      {
        return saveFileDialog.FileName;
      }

      return (string) null;
    }

    public enum FileType
    {
      Models,
      GCode,
    }
  }
}
