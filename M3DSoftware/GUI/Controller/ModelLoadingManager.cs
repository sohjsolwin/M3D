// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Controller.ModelLoadingManager
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.GUI.Tools;
using M3D.GUI.Views;
using M3D.GUI.Views.Library_View;
using M3D.GUI.Views.Printer_View;
using M3D.Model.FilIO;
using M3D.Properties;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Common.Utils;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace M3D.GUI.Controller
{
  public class ModelLoadingManager
  {
    private ThreadSafeVariable<bool> shutdown = new ThreadSafeVariable<bool>(false);
    private object filesLoadingSync = new object();
    private object filesOptimizingSync = new object();
    private SettingsManager settings;
    private LibraryView libraryview;
    public PrinterView printerview;
    private PopupMessageBox messagebox;
    private MessagePopUp informationbox;
    private System.Windows.Forms.Timer update_timer;
    private bool xmlPrinterSettingsZipFileLoaded;
    private ConcurrentQueue<ModelLoadingManager.ModelLoadDetails> modelLoadedQueue;
    private int filesLoading;
    private int filesOptimizing;
    internal const string sViewFileName = "printerview.xml";
    internal const string sSettingsFileName = "printersettings.xml";

    public ModelLoadingManager()
    {
      this.libraryview = (LibraryView) null;
      this.printerview = (PrinterView) null;
      this.messagebox = (PopupMessageBox) null;
      this.informationbox = (MessagePopUp) null;
      this.modelLoadedQueue = new ConcurrentQueue<ModelLoadingManager.ModelLoadDetails>();
    }

    public void Init(SettingsManager settings, LibraryView libraryview, PrinterView printerview, PopupMessageBox messagebox, MessagePopUp informationbox)
    {
      this.libraryview = libraryview;
      this.printerview = printerview;
      this.settings = settings;
      this.messagebox = messagebox;
      this.informationbox = informationbox;
      this.update_timer = new System.Windows.Forms.Timer();
      this.update_timer.Interval = 200;
      this.update_timer.Tick += new EventHandler(this.on_UpdateTimerTick);
      this.update_timer.Start();
    }

    public void OnShutdown()
    {
      this.shutdown.Value = true;
      this.update_timer.Stop();
    }

    private void on_UpdateTimerTick(object sender, EventArgs e)
    {
      ModelLoadingManager.ModelLoadDetails result;
      while (this.modelLoadedQueue.TryDequeue(out result) && !this.shutdown.Value)
      {
        M3D.Graphics.Ext3D.ModelRendering.Model model = result.model;
        if (model != null)
          this.printerview.AddModel(model, result.details);
        this.DecFilesOptimizing();
        this.DecFilesLoading();
      }
    }

    public bool LoadModelIntoPrinter(string filename)
    {
      return this.LoadModelIntoPrinter(filename, new ModelLoadingManager.LoadFailedCallback(this.OnFileLoadFailure));
    }

    public bool LoadModelIntoPrinter(string filename, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      try
      {
        if (!this.OptimizingModel && !this.LoadingNewModel)
          return this.LoadModelIntoPrinter(filename, new ModelLoadingManager.OnModelLoadedDel(this.OnModelLoadedCallback), onFailCallback, (PrintDetails.ObjectDetails) null);
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool LoadModelIntoPrinter(PrintDetails.ObjectDetails objectDetails)
    {
      return this.LoadModelIntoPrinter(objectDetails.filename, new ModelLoadingManager.OnModelLoadedDel(this.OnModelLoadedCallback), new ModelLoadingManager.LoadFailedCallback(this.OnFileLoadFailure), objectDetails);
    }

    private bool LoadModelIntoPrinter(string filename, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback, PrintDetails.ObjectDetails objectDetails)
    {
      if (new SplitFileName(filename).ext.ToLowerInvariant() == "zip")
        return this.LoadZip(filename, loadedCallback, onFailCallback);
      if (this.xmlPrinterSettingsZipFileLoaded)
      {
        this.printerview.ResetControlState();
        this.xmlPrinterSettingsZipFileLoaded = false;
      }
      return this.ImportModel(filename, loadedCallback, onFailCallback, objectDetails);
    }

    private bool LoadZip(string zipFileName, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      this.IncFilesLoading();
      ModelLoadingManager.AsyncZipLoadData asyncZipLoadData = new ModelLoadingManager.AsyncZipLoadData(zipFileName, loadedCallback);
      FileStream file;
      try
      {
        file = File.OpenRead(zipFileName);
      }
      catch (IOException ex)
      {
        this.DecFilesLoading();
        this.ShowFileLoadingExeption((Exception) ex, zipFileName, onFailCallback);
        return false;
      }
      try
      {
        asyncZipLoadData.zf = new ZipFile(file);
        asyncZipLoadData.iconFile = (string) null;
        asyncZipLoadData.extractTo = Path.Combine(Paths.PublicDataFolder, "ExtractedZipFiles", Path.GetFileNameWithoutExtension(zipFileName));
        foreach (ZipEntry zipEntry in asyncZipLoadData.zf)
        {
          if (zipEntry.IsFile)
          {
            string str = Path.Combine(asyncZipLoadData.extractTo, Path.GetFileName(zipEntry.Name));
            if (ModelLoadingManager.GetModelLoader(zipEntry.Name) != null)
              asyncZipLoadData.modelFiles.Add(str);
            else if (zipEntry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
              asyncZipLoadData.xmlFiles.Add(str);
            else if (zipEntry.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || zipEntry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || zipEntry.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
              asyncZipLoadData.iconFile = str;
          }
        }
        if (asyncZipLoadData.modelFiles.Count > 0)
        {
          if (asyncZipLoadData.xmlFiles.Count > 0)
          {
            this.printerview.RemovePrintableModels();
            this.xmlPrinterSettingsZipFileLoaded = true;
          }
          if (!Directory.Exists(asyncZipLoadData.extractTo))
            Directory.CreateDirectory(asyncZipLoadData.extractTo);
        }
      }
      catch (Exception ex)
      {
        file.Close();
        this.DecFilesLoading();
        return false;
      }
      if (asyncZipLoadData.modelFiles.Count > 0)
      {
        this.libraryview.RecentModels.CopyAndAssignIconForLibrary(zipFileName, asyncZipLoadData.iconFile);
        ThreadPool.QueueUserWorkItem(new WaitCallback(this.LoadZipWorkerThread), (object) asyncZipLoadData);
        return true;
      }
      file.Close();
      this.DecFilesLoading();
      return false;
    }

    private void LoadZipWorkerThread(object state)
    {
      ModelLoadingManager.AsyncZipLoadData asyncZipLoadData = state as ModelLoadingManager.AsyncZipLoadData;
      string zipFileName = asyncZipLoadData.zipFileName;
      ModelLoadingManager.OnModelLoadedDel loadedCallback = asyncZipLoadData.loadedCallback;
      try
      {
        foreach (ZipEntry entry in asyncZipLoadData.zf)
        {
          if (entry.IsFile)
          {
            byte[] buffer = new byte[4096];
            using (FileStream fileStream = File.Create(Path.Combine(asyncZipLoadData.extractTo, Path.GetFileName(entry.Name))))
              StreamUtils.Copy(asyncZipLoadData.zf.GetInputStream(entry), (Stream) fileStream, buffer);
          }
        }
        string settingsFromList1 = this.FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, (string) null, "printerview.xml");
        string settingsFromList2 = this.FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, (string) null, "printersettings.xml");
        if (asyncZipLoadData.xmlFiles.Count == 2 && !string.IsNullOrEmpty(settingsFromList1) && !string.IsNullOrEmpty(settingsFromList2))
        {
          this.LoadPrinterView(settingsFromList1, settingsFromList2, asyncZipLoadData.extractTo, zipFileName);
        }
        else
        {
          foreach (string modelFile in asyncZipLoadData.modelFiles)
          {
            string withoutExtension = Path.GetFileNameWithoutExtension(modelFile);
            string settingsFromList3 = this.FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, withoutExtension, "printerview.xml");
            string settingsFromList4 = this.FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, withoutExtension, "printersettings.xml");
            this.ImportModel(modelFile, loadedCallback, new ModelLoadingManager.LoadFailedCallback(this.OnPrinterViewModelLoadFailed), new PrintDetails.ObjectDetails(modelFile, settingsFromList3, settingsFromList4, zipFileName));
          }
        }
      }
      catch (Exception ex)
      {
      }
      asyncZipLoadData.zf.Close();
      this.DecFilesLoading();
    }

    public void LoadPrinterView(string printerViewFile, string printerSettingsFile, string directory, string zipfile)
    {
      this.printerview.RemovePrintableModels();
      PrintDetails.PrintJobObjectViewDetails printerview_settings;
      if (!SettingsManager.LoadPrinterViewFile(printerViewFile, out printerview_settings))
        return;
      this.LoadPrinterView(printerview_settings, printerSettingsFile, directory, zipfile);
    }

    public void LoadPrinterView(PrintDetails.PrintJobObjectViewDetails printerview_settings)
    {
      this.LoadPrinterView(printerview_settings, "", "", "");
    }

    private void LoadPrinterView(PrintDetails.PrintJobObjectViewDetails printerview_settings, string printerSettingsXMLFile, string directory, string zipfile)
    {
      PrintDetails.M3DSettings? printSettings = new PrintDetails.M3DSettings?();
      if (!string.IsNullOrEmpty(printerSettingsXMLFile))
        printSettings = SettingsManager.LoadPrintJobInfoFile(printerSettingsXMLFile);
      ModelLoadingManager.PrinterViewLoadData data = new ModelLoadingManager.PrinterViewLoadData(printerview_settings, printSettings, directory, zipfile);
      if (printSettings.HasValue && this.settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning && !PrinterObject.IsProfileMemberOfFamily(printSettings.Value.profileName, this.printerview.CurrentFamilyName))
      {
        string profileMismatchDialog = Resources.profileMismatchDialog;
        string profileName = printSettings.Value.profileName;
        string currentFamilyName = this.printerview.CurrentFamilyName;
        string newValue = string.Format(string.Format(Locale.GlobalLocale.T("T_ProfileMismatchDialog_Message"), (object) profileName, (object) currentFamilyName), (object) profileName, (object) currentFamilyName);
        this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), profileMismatchDialog.Replace("{MESSAGE}", newValue), new PopupMessageBox.XMLButtonCallback(this.TypeMismatchButtonCallback), (object) data));
      }
      else
        this.LoadPrinterView(data);
    }

    private void LoadPrinterView(ModelLoadingManager.PrinterViewLoadData data)
    {
      if (data.printSettings.HasValue)
        this.printerview.LoadM3DPrintSettings(data.printSettings);
      foreach (PrintDetails.ObjectDetails other in data.printerview_settings.objectList)
      {
        PrintDetails.ObjectDetails objectDetails = new PrintDetails.ObjectDetails(other);
        objectDetails.zipFileName = data.zipfile;
        objectDetails.printerSettingsXMLFile = (string) null;
        string str = other.filename;
        if (!string.IsNullOrEmpty(data.directory))
          str = Path.Combine(data.directory, str);
        this.LoadModelIntoPrinter(str, new ModelLoadingManager.OnModelLoadedDel(this.OnModelLoadedCallback), new ModelLoadingManager.LoadFailedCallback(this.OnPrinterViewModelLoadFailed), objectDetails);
      }
    }

    private void OnPrinterViewModelLoadFailed(string name)
    {
      this.messagebox.AddMessageToQueue(string.Format("Unable to load model, {0}, from collection.", (object) name));
    }

    private void OnFileLoadFailure(string name)
    {
      this.messagebox.AddMessageToQueue(string.Format("Unable to load model, {0}.", (object) name));
    }

    private void TypeMismatchButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.tag == "Yes")
      {
        ModelLoadingManager.PrinterViewLoadData data1 = data as ModelLoadingManager.PrinterViewLoadData;
        if (data1 != null)
          this.LoadPrinterView(data1);
        parentFrame.CloseCurrent();
      }
      else if (button.tag == "No")
      {
        parentFrame.CloseCurrent();
      }
      else
      {
        if (!(button.tag == "dontshowcheckbox"))
          return;
        this.settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = !button.Checked;
      }
    }

    private string FindXMLSettingsFromList(List<string> xmlFiles, string modelName, string xmlFileString)
    {
      if (!string.IsNullOrEmpty(modelName))
      {
        foreach (string xmlFile in xmlFiles)
        {
          if (xmlFile.Contains(modelName) && xmlFile.EndsWith(xmlFileString))
            return xmlFile;
        }
      }
      foreach (string xmlFile in xmlFiles)
      {
        if (xmlFile.EndsWith(xmlFileString))
          return xmlFile;
      }
      return string.Empty;
    }

    private bool ImportModel(string filename, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback, PrintDetails.ObjectDetails objectDetails)
    {
      this.IncFilesLoading();
      try
      {
        long num = 0;
        using (StreamReader streamReader = new StreamReader(filename))
          num = streamReader.BaseStream.Length;
        if (num > 10485760L)
          this.informationbox.AddMessageToQueue("Fairly complex and may slow down this program");
        M3D.Graphics.Ext3D.ModelRendering.Model model = this.printerview.GetModel(filename);
        if (model == null)
        {
          ModelLoadingManager.AsyncModelLoadData state = new ModelLoadingManager.AsyncModelLoadData(filename, loadedCallback, onFailCallback, objectDetails);
          if (this.settings.CurrentAppearanceSettings.ShowRemoveModelWarning && this.printerview.ModelLoaded && (objectDetails == null || objectDetails.printerSettingsXMLFile == null || objectDetails.printerViewXMLFile == null))
          {
            this.DecFilesLoading();
            this.messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), Resources.removeModelDialog, new PopupMessageBox.XMLButtonCallback(this.RemoveModelButtonCallback), (object) state));
            return true;
          }
          if (this.printerview.ModelLoaded && !this.settings.CurrentAppearanceSettings.UseMultipleModels)
            this.printerview.RemovePrintableModels();
          this.StartLoadModelThread(state);
        }
        else
          this.modelLoadedQueue.Enqueue(new ModelLoadingManager.ModelLoadDetails(model, objectDetails));
      }
      catch (Exception ex)
      {
        this.DecFilesLoading();
        this.ShowFileLoadingExeption(ex, filename, onFailCallback);
        return false;
      }
      return true;
    }

    public bool LoadModelIntoPrinter(LibraryRecord record, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      Form1.debugLogger.Add("LoadModelIntoPrinter()", "loading model into printer.", DebugLogger.LogType.Secondary);
      int num = this.LoadModelIntoPrinter(record.cachefilename, onFailCallback) ? 1 : 0;
      if (num == 0)
        return num != 0;
      Form1.debugLogger.Add("LoadModelIntoPrinter()", "Model loaded into printer.", DebugLogger.LogType.Secondary);
      Form1.debugLogger.Add("LoadModelIntoPrinter()", "OpenGL GetError(): " + GL.GetError().ToString() + ".", DebugLogger.LogType.Secondary);
      return num != 0;
    }

    private void StartLoadModelThread(ModelLoadingManager.AsyncModelLoadData state)
    {
      this.printerview.ResetPrinterView();
      ThreadPool.QueueUserWorkItem(new WaitCallback(this.LoadModelThread), (object) state);
    }

    private void LoadModelThread(object state)
    {
      ModelLoadingManager.AsyncModelLoadData asyncModelLoadData = state as ModelLoadingManager.AsyncModelLoadData;
      M3D.Graphics.Ext3D.ModelRendering.Model model = (M3D.Graphics.Ext3D.ModelRendering.Model) null;
      ModelLoadingManager.Result result = ModelLoadingManager.Result.Success;
      try
      {
        model = ModelLoadingManager.LoadModelFromFile(asyncModelLoadData.filename);
        model.fileName = asyncModelLoadData.filename;
      }
      catch (Exception ex)
      {
        result = ModelLoadingManager.Result.Failed;
      }
      if (model == null || result == ModelLoadingManager.Result.Failed)
      {
        this.ShowFileLoadingExeption(new Exception("load failure"), asyncModelLoadData.filename, asyncModelLoadData.onFailCallback);
        this.DecFilesLoading();
      }
      else
      {
        if (asyncModelLoadData.loadedCallback == null)
          return;
        asyncModelLoadData.loadedCallback(result, model, asyncModelLoadData.objectDetails);
      }
    }

    private void RemoveModelButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.tag == "dontshowcheckbox")
      {
        this.settings.CurrentAppearanceSettings.ShowRemoveModelWarning = !button.Checked;
      }
      else
      {
        if (button.tag == "Remove")
        {
          this.printerview.RemovePrintableModels();
          this.settings.CurrentAppearanceSettings.UseMultipleModels = false;
        }
        else if (button.tag == "Continue")
          this.settings.CurrentAppearanceSettings.UseMultipleModels = true;
        ModelLoadingManager.AsyncModelLoadData state = data as ModelLoadingManager.AsyncModelLoadData;
        if (state != null)
        {
          this.IncFilesLoading();
          this.StartLoadModelThread(state);
        }
        parentFrame.CloseCurrent();
      }
    }

    private void ShowFileLoadingExeption(Exception e, string filename, ModelLoadingManager.LoadFailedCallback onLoadFailed)
    {
      if (onLoadFailed != null)
        onLoadFailed(filename);
      if (e is FileNotFoundException)
        this.messagebox.AddMessageToQueue("Sorry, but the file was not found.");
      else
        this.messagebox.AddMessageToQueue("There was an error loading the file. Please check to make sure the file is valid.");
    }

    public static M3D.Graphics.Ext3D.ModelRendering.Model LoadModelFromFile(string fileName)
    {
      return new M3D.Graphics.Ext3D.ModelRendering.Model(ModelLoadingManager.GetModelLoader(fileName).Load(fileName), fileName);
    }

    private static ModelImporterInterface GetModelLoader(string fileName)
    {
      string lowerInvariant = new SplitFileName(fileName).ext.ToLowerInvariant();
      ModelImporterInterface importerInterface = (ModelImporterInterface) null;
      if (lowerInvariant == "stl")
        importerInterface = (ModelImporterInterface) new ModelSTLImporter();
      else if (lowerInvariant == "obj")
        importerInterface = (ModelImporterInterface) new ModelOBJImporter();
      else if (lowerInvariant == "m3d")
        importerInterface = (ModelImporterInterface) new ModelM3DImporter();
      else if (lowerInvariant == "smf")
        importerInterface = (ModelImporterInterface) new ModelSMFImporter();
      return importerInterface;
    }

    private void OnModelLoadedCallback(ModelLoadingManager.Result result, M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails details)
    {
      if (result == ModelLoadingManager.Result.Success)
      {
        this.IncFilesOptimizing();
        this.modelLoadedQueue.Enqueue(new ModelLoadingManager.ModelLoadDetails(model, details));
      }
      else
      {
        this.DecFilesLoading();
        this.informationbox.AddMessageToQueue("Unable to load model.");
      }
    }

    private void IncFilesLoading()
    {
      lock (this.filesLoadingSync)
        ++this.filesLoading;
    }

    private void DecFilesLoading()
    {
      lock (this.filesLoadingSync)
        --this.filesLoading;
    }

    public bool LoadingNewModel
    {
      get
      {
        lock (this.filesLoadingSync)
          return this.filesLoading > 0;
      }
    }

    private void IncFilesOptimizing()
    {
      lock (this.filesOptimizingSync)
        ++this.filesOptimizing;
    }

    private void DecFilesOptimizing()
    {
      lock (this.filesOptimizingSync)
        --this.filesOptimizing;
    }

    public bool OptimizingModel
    {
      get
      {
        lock (this.filesOptimizingSync)
          return this.filesOptimizing > 0;
      }
    }

    public enum Result
    {
      Success,
      Failed,
    }

    private class AsyncModelLoadData
    {
      public string filename;
      public ModelLoadingManager.OnModelLoadedDel loadedCallback;
      public PrintDetails.ObjectDetails objectDetails;
      public ModelLoadingManager.LoadFailedCallback onFailCallback;

      public AsyncModelLoadData(string filename, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback, PrintDetails.ObjectDetails objectDetails)
      {
        this.filename = filename;
        this.loadedCallback = loadedCallback;
        this.objectDetails = objectDetails;
        this.onFailCallback = onFailCallback;
      }
    }

    private class AsyncZipLoadData
    {
      public string zipFileName;
      public List<string> modelFiles;
      public List<string> xmlFiles;
      public ZipFile zf;
      public string iconFile;
      public string extractTo;
      public ModelLoadingManager.OnModelLoadedDel loadedCallback;

      public AsyncZipLoadData(string zipFileName, ModelLoadingManager.OnModelLoadedDel loadedCallback)
      {
        this.zipFileName = zipFileName;
        this.loadedCallback = loadedCallback;
        this.modelFiles = new List<string>();
        this.xmlFiles = new List<string>();
      }
    }

    private struct ModelLoadDetails
    {
      public M3D.Graphics.Ext3D.ModelRendering.Model model;
      public PrintDetails.ObjectDetails details;

      public ModelLoadDetails(M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails details)
      {
        this.model = model;
        this.details = details;
      }
    }

    private class PrinterViewLoadData
    {
      public PrintDetails.PrintJobObjectViewDetails printerview_settings;
      public PrintDetails.M3DSettings? printSettings;
      public string directory;
      public string zipfile;

      public PrinterViewLoadData(PrintDetails.PrintJobObjectViewDetails printerview_settings, PrintDetails.M3DSettings? printSettings, string directory, string zipfile)
      {
        this.printerview_settings = printerview_settings;
        this.printSettings = printSettings;
        this.directory = directory;
        this.zipfile = zipfile;
      }
    }

    public delegate void OnModelLoadedDel(ModelLoadingManager.Result result, M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails details);

    public delegate void LoadFailedCallback(string filename);
  }
}
