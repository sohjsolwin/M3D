// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.Updater
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.form = mainForm;
      this.messagebox = Box;
      this.spooler_connection = connection;
      this.settingsManager = settings;
    }

    public Updater.UpdateSettings UpdaterMode
    {
      get
      {
        return this.settingsManager.CurrentAppearanceSettings.UpdaterMode;
      }
      set
      {
        this.settingsManager.CurrentAppearanceSettings.UpdaterMode = value;
      }
    }

    public Updater.Status CurrentStatus
    {
      get
      {
        return this.internalState.Value;
      }
    }

    public bool isReadyToInstall
    {
      get
      {
        return this.internalState.Value == Updater.Status.Downloaded;
      }
    }

    public void CheckForUpdate(bool bCommandedByUser = false)
    {
      lock (this.ThreadWatcherLock)
      {
        if (this.WorkerCheckForUpdateThread != null)
          return;
        this.WorkerCheckForUpdateThread = new Thread((ThreadStart) (() => this.CheckForUpdate_Thread(bCommandedByUser)));
        this.WorkerCheckForUpdateThread.IsBackground = true;
        this.WorkerCheckForUpdateThread.Start();
      }
    }

    public void DownloadUpdate(bool bCommandedByUser = false)
    {
      lock (this.ThreadWatcherLock)
      {
        if (this.WorkerDownloaderThread != null)
          return;
        this.WorkerDownloaderThread = new Thread((ThreadStart) (() => this.DownloadUpdate_Thread(bCommandedByUser)));
        this.WorkerDownloaderThread.IsBackground = true;
        this.WorkerDownloaderThread.Start();
      }
    }

    public void CancelDownloadUpdate()
    {
      if (this.WorkerDownloaderThread != null)
      {
        try
        {
          lock (this.ThreadWatcherLock)
            this.WorkerDownloaderThread.Abort();
          this.WorkerDownloaderThread.Join();
          this.WorkerCheckForUpdateThread = (Thread) null;
        }
        catch (Exception ex)
        {
        }
      }
      if (this.WorkerCheckForUpdateThread == null)
        return;
      try
      {
        lock (this.ThreadWatcherLock)
          this.WorkerCheckForUpdateThread.Abort();
        this.WorkerCheckForUpdateThread.Join();
        this.WorkerCheckForUpdateThread = (Thread) null;
      }
      catch (Exception ex)
      {
      }
    }

    private void CheckForUpdate_Thread(bool bCommandedByUser)
    {
      try
      {
        this.m_oOnlineVersionInfo = this.FetchUpdateInfo();
        bool flag;
        if (this.m_oOnlineVersionInfo != null)
        {
          if (this.m_oOnlineVersionInfo.Version > M3D.Spooling.Version.Client_Version)
          {
            this.internalState.Value = Updater.Status.NewVersionAvailable;
            flag = this.UpdaterMode != Updater.UpdateSettings.NoAction | bCommandedByUser;
          }
          else
          {
            flag = false;
            if (this.m_oOnlineVersionInfo != null)
              this.internalState.Value = Updater.Status.UptoDate;
          }
        }
        else
        {
          if (bCommandedByUser)
            this.messagebox.AddMessageToQueue("Failed to get information about update", PopupMessageBox.MessageBoxButtons.OK);
          flag = false;
          this.internalState.Value = Updater.Status.Unknown;
        }
        if (!flag)
          return;
        this.DownloadUpdate(false);
      }
      catch (ThreadAbortException ex)
      {
        throw ex;
      }
      finally
      {
        lock (this.ThreadWatcherLock)
          this.WorkerCheckForUpdateThread = (Thread) null;
      }
    }

    private static bool bCheckFileAgainstHash(string sFileName, BigInteger? n128CheckValue)
    {
      bool flag = false;
      BigInteger? md5Hash = Updater.CalculateMD5Hash(sFileName);
      if (md5Hash.HasValue)
      {
        BigInteger? nullable1 = md5Hash;
        BigInteger? nullable2 = n128CheckValue;
        if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
          flag = true;
      }
      return flag;
    }

    public void DownloadUpdate_Thread(bool bCommandedByUser)
    {
      if (this.m_oOnlineVersionInfo == null)
        return;
      try
      {
        bool flag1 = false;
        bool flag2 = false;
        if (System.IO.File.Exists(Paths.DownloadedExecutableFile))
        {
          flag1 = Updater.bCheckFileAgainstHash(Paths.DownloadedExecutableFile, this.m_oOnlineVersionInfo.n128FileHash);
          if (flag1)
            flag2 = true;
          else
            this.DeleteDownloadedFile();
        }
        if (!flag2)
        {
          this.internalState.Value = Updater.Status.Downloading;
          if (this.DownloadFile(this.m_oOnlineVersionInfo.Address_Str, Paths.DownloadedExecutableFile))
            flag1 = Updater.bCheckFileAgainstHash(Paths.DownloadedExecutableFile, this.m_oOnlineVersionInfo.n128FileHash);
        }
        if (flag1)
        {
          this.internalState.Value = Updater.Status.Downloaded;
          if (bCommandedByUser)
          {
            this.StartInstall();
          }
          else
          {
            if (!this.allPrintersIdleCheck())
              return;
            if (this.UpdaterMode == Updater.UpdateSettings.DownloadInstall)
            {
              this.StartInstall();
            }
            else
            {
              if (this.UpdaterMode != Updater.UpdateSettings.DownloadNotInstall)
                return;
              this.AskToInstall();
            }
          }
        }
        else
        {
          this.internalState.Value = Updater.Status.DownloadError;
          if (!bCommandedByUser)
            return;
          this.messagebox.AddMessageToQueue("Failed to download update", PopupMessageBox.MessageBoxButtons.OK);
        }
      }
      catch (ThreadAbortException ex)
      {
        if (Updater.Status.Downloading == this.internalState.Value)
          this.internalState.Value = Updater.Status.DownloadError;
        throw ex;
      }
      finally
      {
        lock (this.ThreadWatcherLock)
          this.WorkerDownloaderThread = (Thread) null;
      }
    }

    public void AskToInstall()
    {
      if (!this.isReadyToInstall)
        return;
      this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), Resources.updateInstallAskDialog, new PopupMessageBox.XMLButtonCallback(this.Callback), (object) null));
    }

    private void Callback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.ID == 301)
      {
        this.m_bRememberUserDecision = button.Checked;
      }
      else
      {
        if (button.ID != 101 && button.ID != 102)
          return;
        parentFrame.CloseCurrent();
        if (button.ID == 101)
        {
          if (this.m_bRememberUserDecision)
            this.UpdaterMode = Updater.UpdateSettings.DownloadInstall;
          this.StartInstall();
        }
        else
        {
          if (!this.m_bRememberUserDecision)
            return;
          this.UpdaterMode = Updater.UpdateSettings.NoAction;
          parentFrame.AddMessageToQueue("You can check for future updates in the \"Settings\" menu under \"User Interface Options\".", "Software Update", PopupMessageBox.MessageBoxButtons.OK, (PopupMessageBox.OnUserSelectionDel) null);
        }
      }
    }

    public void ForceDownloadAndUpdate()
    {
      switch (this.internalState.Value)
      {
        case Updater.Status.Unknown:
          this.CheckForUpdate(true);
          break;
        case Updater.Status.NewVersionAvailable:
        case Updater.Status.DownloadError:
          this.DownloadUpdate(true);
          break;
        case Updater.Status.Downloaded:
          this.StartInstall();
          break;
      }
    }

    public bool isWorking
    {
      get
      {
        lock (this.ThreadWatcherLock)
          return this.WorkerCheckForUpdateThread != null || this.WorkerDownloaderThread != null;
      }
    }

    private void StartInstall()
    {
      if (this.allPrintersIdleCheck())
      {
        Process process = new Process() { StartInfo = new ProcessStartInfo(Paths.DownloadedExecutableFile) };
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
          this.form.Close();
        else
          this.messagebox.AddMessageToQueue("Problem encountered while starting installer", PopupMessageBox.MessageBoxButtons.OK);
      }
      else
        this.messagebox.AddMessageToQueue("Please allow all print jobs to complete and then try again.");
    }

    private bool allPrintersIdleCheck()
    {
      bool flag = false;
      List<PrinterInfo> printer_list = new List<PrinterInfo>();
      this.spooler_connection.CopyPrinterList(ref printer_list);
      for (int index = 0; index < printer_list.Count && !flag; ++index)
        flag = printer_list[index].Status != PrinterStatus.Firmware_Idle || printer_list[index].current_job != null;
      return !flag;
    }

    private bool DownloadFile(string URL, string FilePath)
    {
      WebClient webClient = new WebClient();
      try
      {
        webClient.DownloadFile(URL, FilePath);
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
          throw ex;
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
          throw ex;
      }
    }

    private Package FetchUpdateInfo()
    {
      Package package = (Package) null;
      try
      {
        PackageManager packageManager = PackageManager.Load(Paths.DownloadedHashXML_URL);
        if (packageManager != null)
        {
          Package.DistributionType thisDistro = Package.DistributionType.Windows;
          IEnumerable<Package> source = (IEnumerable<Package>) packageManager.items.Where<Package>((Func<Package, bool>) (s => s.Distribution == thisDistro)).OrderBy<Package, VersionNumber>((Func<Package, VersionNumber>) (s => s.Version));
          if (source.Count<Package>() > 0)
            package = source.Last<Package>();
        }
      }
      catch (Exception ex)
      {
        package = (Package) null;
        if (ex is ThreadAbortException)
          throw ex;
      }
      return package;
    }

    private bool VersionNumberTryCreate(string str, out VersionNumber Version)
    {
      Version = (VersionNumber) null;
      try
      {
        Version = new VersionNumber(str);
      }
      catch (Exception ex)
      {
        if (ex is ThreadAbortException)
          throw ex;
        return false;
      }
      return true;
    }

    private static BigInteger? CalculateMD5Hash(string fileName)
    {
      BigInteger? nullable1 = new BigInteger?();
      BigInteger? nullable2;
      try
      {
        byte[] numArray = (byte[]) null;
        using (MD5 md5 = MD5.Create())
        {
          using (FileStream fileStream = System.IO.File.OpenRead(fileName))
            numArray = md5.ComputeHash((Stream) fileStream);
        }
        Array.Reverse((Array) numArray);
        nullable2 = new BigInteger?(new BigInteger(numArray));
      }
      catch (Exception ex)
      {
        nullable2 = new BigInteger?();
        if (ex is ThreadAbortException)
          throw ex;
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
