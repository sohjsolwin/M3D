using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Interfaces;
using M3D.Properties;
using M3D.Spooling.Client;

namespace M3D.GUI.Dialogs
{
  internal class AssociationsForm
  {
    private bool allow_messages;
    private SettingsManager settings;
    private IFileAssociations FileAssociations;
    private string ExecutablePath;
    private string IconPath;

    public AssociationsForm(SettingsManager settings, PopupMessageBox messagebox, IFileAssociations FileAssociations, string ExecutablePath, string IconPath)
    {
      allow_messages = messagebox.AllowMessages;
      this.settings = settings;
      this.FileAssociations = FileAssociations;
      this.ExecutablePath = ExecutablePath;
      this.IconPath = IconPath;
      messagebox.AllowMessages = true;
      var associationsDialog = Resources.fileAssociationsDialog;
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), associationsDialog, new PopupMessageBox.XMLButtonCallback(XMLButtonCallback), null));
    }

    private void XMLButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      var childElement = (ButtonWidget) childFrame.FindChildElement(301);
      if (button.ID == 301)
      {
        return;
      }

      settings.Settings.miscSettings.FileAssociations.ShowFileAssociationsDialog = !childElement.Checked;
      switch (button.ID)
      {
        case 101:
          FileAssociations.Set3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file", ExecutablePath, "M3D file (.stl)", IconPath);
          FileAssociations.Set3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file", ExecutablePath, "M3D file (.obj)", IconPath);
          break;
        case 102:
          FileAssociations.Delete3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file");
          FileAssociations.Delete3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file");
          break;
      }
      parentFrame.AllowMessages = allow_messages;
      parentFrame.CloseCurrent();
    }

    private enum ControlIDs
    {
      YesButton = 101, // 0x00000065
      NoButton = 102, // 0x00000066
      DontShowCheckbox = 301, // 0x0000012D
    }
  }
}
