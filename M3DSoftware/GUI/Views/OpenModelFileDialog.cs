using System.Windows.Forms;

namespace M3D.GUI.Views
{
  public static class OpenModelFileDialog
  {
    public static string RunOpenModelDialog(OpenModelFileDialog.FileType fileType)
    {
      var str1 = "";
      var openFileDialog = new OpenFileDialog();
      switch (fileType)
      {
        case OpenModelFileDialog.FileType.Models:
          var str2 = str1 + "Supported Files|*.stl;*.obj;*.zip|" + "STL File (.stl)|*.stl|" + "OBJ File (.obj)|*.obj|" + "ZIP File (.zip)|*.zip|" + "All Files (*.*)|*.*";
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
      {
        return openFileDialog.FileName;
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
