// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Dialogs.AssociationsForm
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.allow_messages = messagebox.AllowMessages;
      this.settings = settings;
      this.FileAssociations = FileAssociations;
      this.ExecutablePath = ExecutablePath;
      this.IconPath = IconPath;
      messagebox.AllowMessages = true;
      string associationsDialog = Resources.fileAssociationsDialog;
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), associationsDialog, new PopupMessageBox.XMLButtonCallback(this.XMLButtonCallback), (object) null));
    }

    private void XMLButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      ButtonWidget childElement = (ButtonWidget) childFrame.FindChildElement(301);
      if (button.ID == 301)
        return;
      this.settings.Settings.miscSettings.FileAssociations.ShowFileAssociationsDialog = !childElement.Checked;
      switch (button.ID)
      {
        case 101:
          this.FileAssociations.Set3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file", this.ExecutablePath, "M3D file (.stl)", this.IconPath);
          this.FileAssociations.Set3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file", this.ExecutablePath, "M3D file (.obj)", this.IconPath);
          break;
        case 102:
          this.FileAssociations.Delete3DFileAssociation(".stl", "STL_M3D_Printer_GUI_file");
          this.FileAssociations.Delete3DFileAssociation(".obj", "OBJ_M3D_Printer_GUI_file");
          break;
      }
      parentFrame.AllowMessages = this.allow_messages;
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
