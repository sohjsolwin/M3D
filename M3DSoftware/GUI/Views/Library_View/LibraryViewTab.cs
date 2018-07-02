using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;

namespace M3D.GUI.Views.Library_View
{
  internal interface LibraryViewTab
  {
    void Show(GUIHost host, GridLayout LibraryGrid, string filter);

    void LoadRecord(LibraryRecord record);

    void RemoveRecord(LibraryRecord record);

    void SaveRecord(LibraryRecord record);

    bool CanSaveRecords { get; }

    bool CanRemoveRecords { get; }
  }
}
