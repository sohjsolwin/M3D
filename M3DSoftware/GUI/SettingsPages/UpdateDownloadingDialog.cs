using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Dialogs;
using M3D.Properties;
using M3D.Spooling.Client;

namespace M3D.GUI.SettingsPages
{
  public static class UpdateDownloadingDialog
  {
    private static Updater updater;
    private static PopupMessageBox messagebox;

    public static void Show(PopupMessageBox messagebox, Updater updater)
    {
      UpdateDownloadingDialog.updater = updater;
      UpdateDownloadingDialog.messagebox = messagebox;
      var updateDownloading = Resources.updateDownloading;
      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(MessageType.UserDefined, ""), updateDownloading, new PopupMessageBox.XMLButtonCallback(UpdateDownloadingDialog.ButtonCallback), (object) updater, new ElementStandardDelegate(UpdateDownloadingDialog.OnUpdate)));
    }

    private static void ButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID == 101)
      {
        parentFrame.CloseCurrent();
      }
      else
      {
        if (button.ID != 102)
        {
          return;
        }

        UpdateDownloadingDialog.updater.CancelDownloadUpdate();
        parentFrame.CloseCurrent();
      }
    }

    private static void OnUpdate()
    {
      if (UpdateDownloadingDialog.updater.isWorking)
      {
        return;
      }

      UpdateDownloadingDialog.messagebox.CloseCurrent();
    }
  }
}
