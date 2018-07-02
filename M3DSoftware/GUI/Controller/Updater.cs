using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;

namespace M3D.GUI.Controller
{
  public class Updater
  {
    private ThreadSafeVariable<Updater.Status> internalState = new ThreadSafeVariable<Updater.Status>(Updater.Status.Unknown);
    private object ThreadWatcherLock = new object();
    private Form1 form;
    private PopupMessageBox messagebox;
    private SpoolerConnection spooler_connection;
    private SettingsManager settingsManager;
    private Package m_oOnlineVersionInfo;
    private bool m_bRememberUserDecision;
    private Thread WorkerCheckForUpdateThread;
    private Thread WorkerDownloaderThread;

    public Updater(Form1 mainForm, PopupMessageBox Box, SpoolerConnection connection, SettingsManager settings)
    {
      form = mainForm;
      messagebox = Box;
      spooler_connection = connection;
      settingsManager = settings;
    }

    public Updater.UpdateSettings UpdaterMode
    {
      get
      {
        return settingsManager.CurrentAppearanceSettings.UpdaterMode;
      }
      set
      {
        settingsManager.CurrentAppearanceSettings.UpdaterMode = value;
      }
    }

    public Updater.Status CurrentStatus
    {
      get
      {
        return internalState.Value;
      }
    }

    public bool isReadyToInstall
    {
      get
      {
        return internalState.Value == Updater.Status.Downloaded;
      }
    }

    public void CheckForUpdate(bool bCommandedByUser = false)
    {
      lock (ThreadWatcherLock)
      {
        if (WorkerCheckForUpdateThread != null)
        {
          return;
        }

        WorkerCheckForUpdateThread = new Thread(() => CheckForUpdate_Thread(bCommandedByUser))
        {
          IsBackground = true
        };
        WorkerCheckForUpdateThread.Start();
      }
    }

    public void DownloadUpdate(bool bCommandedByUser = false)
    {
      lock (ThreadWatcherLock)
      {
        if (WorkerDownloaderThread != null)
        {
          return;
        }

        WorkerDownloaderThread = new Thread(() => DownloadUpdate_Thread(bCommandedByUser))
        {
          IsBackground = true
        };
        WorkerDownloaderThread.Start();
      }
    }

    public void CancelDownloadUpdate()
    {
      if (WorkerDownloaderThread != null)
      {
        try
        {
          lock (ThreadWatcherLock)
          {
            WorkerDownloaderThread.Abort();
          }

          WorkerDownloaderThread.Join();
          WorkerCheckForUpdateThread = null;
        }
        catch (Exception ex)
        {
        }
      }
      if (WorkerCheckForUpdateThread == null)
      {
        return;
      }

      try
      {
        lock (ThreadWatcherLock)
        {
          WorkerCheckForUpdateThread.Abort();
        }

        WorkerCheckForUpdateThread.Join();
        WorkerCheckForUpdateThread = null;
      }
      catch (Exception ex)
      {
      }
    }

    private void CheckForUpdate_Thread(bool bCommandedByUser)
    {
      try
      {
        m_oOnlineVersionInfo = FetchUpdateInfo();
        bool flag;
        if (m_oOnlineVersionInfo != null)
        {
          if (m_oOnlineVersionInfo.Version > M3D.Spooling.Version.Client_Version)
          {
            internalState.Value = Updater.Status.NewVersionAvailable;
            flag = UpdaterMode != Updater.UpdateSettings.NoAction | bCommandedByUser;
          }
          else
          {
            flag = false;
            if (m_oOnlineVersionInfo != null)
            {
              internalState.Value = Updater.Status.UptoDate;
            }
          }
        }
        else
        {
          if (bCommandedByUser)
          {
            messagebox.AddMessageToQueue("Failed to get information about update", PopupMessageBox.MessageBoxButtons.OK);
          }

          flag = false;
          internalState.Value = Updater.Status.Unknown;
        }
        if (!flag)
        {
          return;
        }

        DownloadUpdate(false);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      finally
      {
        lock (ThreadWatcherLock)
        {
          WorkerCheckForUpdateThread = null;
        }
      }
    }

    private static bool bCheckFileAgainstHash(string sFileName, BigInteger? n128CheckValue)
    {
      var flag = false;
      BigInteger? md5Hash = Updater.CalculateMD5Hash(sFileName);
      if (md5Hash.HasValue)
      {
        BigInteger? nullable1 = md5Hash;
        BigInteger? nullable2 = n128CheckValue;
        if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
        {
          flag = true;
        }
      }
      return flag;
    }

    public void DownloadUpdate_Thread(bool bCommandedByUser)
    {
      if (m_oOnlineVersionInfo == null)
      {
        return;
      }

      try
      {
        var flag1 = false;
        var flag2 = false;
        if (System.IO.File.Exists(Paths.DownloadedExecutableFile))
        {
          flag1 = Updater.bCheckFileAgainstHash(Paths.DownloadedExecutableFile, m_oOnlineVersionInfo.n128FileHash);
          if (flag1)
          {
            flag2 = true;
          }
          else
          {
            DeleteDownloadedFile();
          }
        }
        if (!flag2)
        {
          internalState.Value = Updater.Status.Downloading;
          if (DownloadFile(m_oOnlineVersionInfo.Address_Str, Paths.DownloadedExecutableFile))
          {
            flag1 = Updater.bCheckFileAgainstHash(Paths.DownloadedExecutableFile, m_oOnlineVersionInfo.n128FileHash);
          }
        }
        if (flag1)
        {
          internalState.Value = Updater.Status.Downloaded;
          if (bCommandedByUser)
          {
            StartInstall();
          }
          else
          {
            if (!allPrintersIdleCheck())
            {
              return;
            }

            if (UpdaterMode == Updater.UpdateSettings.DownloadInstall)
            {
              StartInstall();
            }
            else
            {
              if (UpdaterMode != Updater.UpdateSettings.DownloadNotInstall)
              {
                return;
              }

              AskToInstall();
            }
          }
        }
        else
        {
          internalState.Value = Updater.Status.DownloadError;
          if (!bCommandedByUser)
          {
            return;
          }

          messagebox.AddMessageToQueue("Failed to download update", PopupMessageBox.MessageBoxButtons.OK);
        }
      }
      catch (ThreadAbortException ex)
      {
        if (Updater.Status.Downloading == internalState.Value)
        {
          internalState.Value = Updater.Status.DownloadError;
        }

        throw ex;
      }
      finally
      {
        lock (ThreadWatcherLock)
        {
          WorkerDownloaderThread = null;
        }
      }
    }

    public void AskToInstall()
    {
      if (!isReadyToInstall)
      {
        return;
      }

      messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), Resources.updateInstallAskDialog, new PopupMessageBox.XMLButtonCallback(Callback), null));
    }

    private void Callback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID == 301)
      {
        m_bRememberUserDecision = button.Checked;
      }
      else
      {
        if (button.ID != 101 && button.ID != 102)
        {
          return;
        }

        parentFrame.CloseCurrent();
        if (button.ID == 101)
        {
          if (m_bRememberUserDecision)
          {
            UpdaterMode = Updater.UpdateSettings.DownloadInstall;
          }

          StartInstall();
        }
        else
        {
          if (!m_bRememberUserDecision)
          {
            return;
          }

          UpdaterMode = Updater.UpdateSettings.NoAction;
          parentFrame.AddMessageToQueue("You can check for future updates in the \"Settings\" menu under \"User Interface Options\".", "Software Update", PopupMessageBox.MessageBoxButtons.OK, null);
        }
      }
    }

    public void ForceDownloadAndUpdate()
    {
      switch (internalState.Value)
      {
        case Updater.Status.Unknown:
          CheckForUpdate(true);
          break;
        case Updater.Status.NewVersionAvailable:
        case Updater.Status.DownloadError:
          DownloadUpdate(true);
          break;
        case Updater.Status.Downloaded:
          StartInstall();
          break;
      }
    }

    public bool isWorking
    {
      get
      {
        lock (ThreadWatcherLock)
        {
          return WorkerCheckForUpdateThread != null || WorkerDownloaderThread != null;
        }
      }
    }

    private void StartInstall()
    {
      if (allPrintersIdleCheck())
      {
        var process = new Process() { StartInfo = new ProcessStartInfo(Paths.DownloadedExecutableFile) };
        bool flag;
        try
        {
          flag = process.Start();
        }
        catch (Exception ex)
        {
          flag = false;
        }
        if (flag)
        {
          form.Close();
        }
        else
        {
          messagebox.AddMessageToQueue("Problem encountered while starting installer", PopupMessageBox.MessageBoxButtons.OK);
        }
      }
      else
      {
        messagebox.AddMessageToQueue("Please allow all print jobs to complete and then try again.");
      }
    }

    private bool allPrintersIdleCheck()
    {
      var flag = false;
      var printer_list = new List<PrinterInfo>();
      spooler_connection.CopyPrinterList(ref printer_list);
      for (var index = 0; index < printer_list.Count && !flag; ++index)
      {
        flag = printer_list[index].Status != PrinterStatus.Firmware_Idle || printer_list[index].current_job != null;
      }

      return !flag;
    }

    private bool DownloadFile(string URL, string FilePath)
    {
      var webClient = new WebClient();
      try
      {
        webClient.DownloadFile(URL, FilePath);
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }

        return false;
      }
      return true;
    }

    private void DeleteDownloadedFile()
    {
      try
      {
        System.IO.File.Delete(Paths.DownloadedExecutableFile);
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }
      }
    }

    private Package FetchUpdateInfo()
    {
      var package = (Package) null;
      try
      {
        var packageManager = PackageManager.Load(Paths.DownloadedHashXML_URL);
        if (packageManager != null)
        {
          Package.DistributionType thisDistro = Package.DistributionType.Windows;
          var source = (IEnumerable<Package>) packageManager.items.Where<Package>(s => s.Distribution == thisDistro).OrderBy<Package, VersionNumber>(s => s.Version);
          if (source.Count<Package>() > 0)
          {
            package = source.Last<Package>();
          }
        }
      }
      catch (Exception ex)
      {
        package = null;
        if (ex is ThreadAbortException)
        {
          throw ex;
        }
      }
      return package;
    }

    private bool VersionNumberTryCreate(string str, out VersionNumber Version)
    {
      Version = null;
      try
      {
        Version = new VersionNumber(str);
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
        {
          throw ex;
        }

        return false;
      }
      return true;
    }

    private static BigInteger? CalculateMD5Hash(string fileName)
    {
      var nullable1 = new BigInteger?();
      BigInteger? nullable2;
      try
      {
        var numArray = (byte[]) null;
        using (var md5 = MD5.Create())
        {
          using (FileStream fileStream = System.IO.File.OpenRead(fileName))
          {
            numArray = md5.ComputeHash(fileStream);
          }
        }
        Array.Reverse(numArray);
        nullable2 = new BigInteger?(new BigInteger(numArray));
      }
      catch (Exception ex)
      {
        nullable2 = new BigInteger?();
        if (ex is ThreadAbortException)
        {
          throw ex;
        }
      }
      return nullable2;
    }

    private enum XMLDialogControlIDs
    {
      YesButton = 101, // 0x00000065
      NoButton = 102, // 0x00000066
      DontShowCheckbox = 301, // 0x0000012D
    }

    public enum UpdateSettings
    {
      DownloadInstall,
      DownloadNotInstall,
      NoAction,
    }

    public enum Status
    {
      Unknown,
      UptoDate,
      NewVersionAvailable,
      Downloading,
      Downloaded,
      DownloadError,
    }
  }
}
