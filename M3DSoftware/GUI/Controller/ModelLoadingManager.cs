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
      libraryview = null;
      printerview = null;
      messagebox = null;
      informationbox = null;
      modelLoadedQueue = new ConcurrentQueue<ModelLoadingManager.ModelLoadDetails>();
    }

    public void Init(SettingsManager settings, LibraryView libraryview, PrinterView printerview, PopupMessageBox messagebox, MessagePopUp informationbox)
    {
      this.libraryview = libraryview;
      this.printerview = printerview;
      this.settings = settings;
      this.messagebox = messagebox;
      this.informationbox = informationbox;
      update_timer = new System.Windows.Forms.Timer
      {
        Interval = 200
      };
      update_timer.Tick += new EventHandler(on_UpdateTimerTick);
      update_timer.Start();
    }

    public void OnShutdown()
    {
      shutdown.Value = true;
      update_timer.Stop();
    }

    private void on_UpdateTimerTick(object sender, EventArgs e)
    {
      while (modelLoadedQueue.TryDequeue(out ModelLoadDetails result) && !shutdown.Value)
      {
        M3D.Graphics.Ext3D.ModelRendering.Model model = result.model;
        if (model != null)
        {
          printerview.AddModel(model, result.details);
        }

        DecFilesOptimizing();
        DecFilesLoading();
      }
    }

    public bool LoadModelIntoPrinter(string filename)
    {
      return LoadModelIntoPrinter(filename, new ModelLoadingManager.LoadFailedCallback(OnFileLoadFailure));
    }

    public bool LoadModelIntoPrinter(string filename, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      try
      {
        if (!OptimizingModel && !LoadingNewModel)
        {
          return LoadModelIntoPrinter(filename, new ModelLoadingManager.OnModelLoadedDel(OnModelLoadedCallback), onFailCallback, null);
        }

        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool LoadModelIntoPrinter(PrintDetails.ObjectDetails objectDetails)
    {
      return LoadModelIntoPrinter(objectDetails.filename, new ModelLoadingManager.OnModelLoadedDel(OnModelLoadedCallback), new ModelLoadingManager.LoadFailedCallback(OnFileLoadFailure), objectDetails);
    }

    private bool LoadModelIntoPrinter(string filename, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback, PrintDetails.ObjectDetails objectDetails)
    {
      if (new SplitFileName(filename).ext.ToLowerInvariant() == "zip")
      {
        return LoadZip(filename, loadedCallback, onFailCallback);
      }

      if (xmlPrinterSettingsZipFileLoaded)
      {
        printerview.ResetControlState();
        xmlPrinterSettingsZipFileLoaded = false;
      }
      return ImportModel(filename, loadedCallback, onFailCallback, objectDetails);
    }

    private bool LoadZip(string zipFileName, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      IncFilesLoading();
      var asyncZipLoadData = new ModelLoadingManager.AsyncZipLoadData(zipFileName, loadedCallback);
      FileStream file;
      try
      {
        file = File.OpenRead(zipFileName);
      }
      catch (IOException ex)
      {
        DecFilesLoading();
        ShowFileLoadingExeption(ex, zipFileName, onFailCallback);
        return false;
      }
      try
      {
        asyncZipLoadData.zf = new ZipFile(file);
        asyncZipLoadData.iconFile = null;
        asyncZipLoadData.extractTo = Path.Combine(Paths.PublicDataFolder, "ExtractedZipFiles", Path.GetFileNameWithoutExtension(zipFileName));
        foreach (ZipEntry zipEntry in asyncZipLoadData.zf)
        {
          if (zipEntry.IsFile)
          {
            var str = Path.Combine(asyncZipLoadData.extractTo, Path.GetFileName(zipEntry.Name));
            if (ModelLoadingManager.GetModelLoader(zipEntry.Name) != null)
            {
              asyncZipLoadData.modelFiles.Add(str);
            }
            else if (zipEntry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
              asyncZipLoadData.xmlFiles.Add(str);
            }
            else if (zipEntry.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || zipEntry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || zipEntry.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
            {
              asyncZipLoadData.iconFile = str;
            }
          }
        }
        if (asyncZipLoadData.modelFiles.Count > 0)
        {
          if (asyncZipLoadData.xmlFiles.Count > 0)
          {
            printerview.RemovePrintableModels();
            xmlPrinterSettingsZipFileLoaded = true;
          }
          if (!Directory.Exists(asyncZipLoadData.extractTo))
          {
            Directory.CreateDirectory(asyncZipLoadData.extractTo);
          }
        }
      }
      catch (Exception ex)
      {
        file.Close();
        DecFilesLoading();
        return false;
      }
      if (asyncZipLoadData.modelFiles.Count > 0)
      {
        libraryview.RecentModels.CopyAndAssignIconForLibrary(zipFileName, asyncZipLoadData.iconFile);
        ThreadPool.QueueUserWorkItem(new WaitCallback(LoadZipWorkerThread), asyncZipLoadData);
        return true;
      }
      file.Close();
      DecFilesLoading();
      return false;
    }

    private void LoadZipWorkerThread(object state)
    {
      var asyncZipLoadData = state as ModelLoadingManager.AsyncZipLoadData;
      var zipFileName = asyncZipLoadData.zipFileName;
      ModelLoadingManager.OnModelLoadedDel loadedCallback = asyncZipLoadData.loadedCallback;
      try
      {
        foreach (ZipEntry entry in asyncZipLoadData.zf)
        {
          if (entry.IsFile)
          {
            byte[] buffer = new byte[4096];
            using (FileStream fileStream = File.Create(Path.Combine(asyncZipLoadData.extractTo, Path.GetFileName(entry.Name))))
            {
              StreamUtils.Copy(asyncZipLoadData.zf.GetInputStream(entry), fileStream, buffer);
            }
          }
        }
        var settingsFromList1 = FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, null, "printerview.xml");
        var settingsFromList2 = FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, null, "printersettings.xml");
        if (asyncZipLoadData.xmlFiles.Count == 2 && !string.IsNullOrEmpty(settingsFromList1) && !string.IsNullOrEmpty(settingsFromList2))
        {
          LoadPrinterView(settingsFromList1, settingsFromList2, asyncZipLoadData.extractTo, zipFileName);
        }
        else
        {
          foreach (var modelFile in asyncZipLoadData.modelFiles)
          {
            var withoutExtension = Path.GetFileNameWithoutExtension(modelFile);
            var settingsFromList3 = FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, withoutExtension, "printerview.xml");
            var settingsFromList4 = FindXMLSettingsFromList(asyncZipLoadData.xmlFiles, withoutExtension, "printersettings.xml");
            ImportModel(modelFile, loadedCallback, new ModelLoadingManager.LoadFailedCallback(OnPrinterViewModelLoadFailed), new PrintDetails.ObjectDetails(modelFile, settingsFromList3, settingsFromList4, zipFileName));
          }
        }
      }
      catch (Exception ex)
      {
      }
      asyncZipLoadData.zf.Close();
      DecFilesLoading();
    }

    public void LoadPrinterView(string printerViewFile, string printerSettingsFile, string directory, string zipfile)
    {
      printerview.RemovePrintableModels();
      if (!SettingsManager.LoadPrinterViewFile(printerViewFile, out PrintDetails.PrintJobObjectViewDetails printerview_settings))
      {
        return;
      }

      LoadPrinterView(printerview_settings, printerSettingsFile, directory, zipfile);
    }

    public void LoadPrinterView(PrintDetails.PrintJobObjectViewDetails printerview_settings)
    {
      LoadPrinterView(printerview_settings, "", "", "");
    }

    private void LoadPrinterView(PrintDetails.PrintJobObjectViewDetails printerview_settings, string printerSettingsXMLFile, string directory, string zipfile)
    {
      var printSettings = new PrintDetails.M3DSettings?();
      if (!string.IsNullOrEmpty(printerSettingsXMLFile))
      {
        printSettings = SettingsManager.LoadPrintJobInfoFile(printerSettingsXMLFile);
      }

      var data = new ModelLoadingManager.PrinterViewLoadData(printerview_settings, printSettings, directory, zipfile);
      if (printSettings.HasValue && settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning && !PrinterObject.IsProfileMemberOfFamily(printSettings.Value.profileName, printerview.CurrentFamilyName))
      {
        var profileMismatchDialog = Resources.profileMismatchDialog;
        var profileName = printSettings.Value.profileName;
        var currentFamilyName = printerview.CurrentFamilyName;
        var newValue = string.Format(string.Format(Locale.GlobalLocale.T("T_ProfileMismatchDialog_Message"), profileName, currentFamilyName), profileName, currentFamilyName);
        messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), profileMismatchDialog.Replace("{MESSAGE}", newValue), new PopupMessageBox.XMLButtonCallback(TypeMismatchButtonCallback), data));
      }
      else
      {
        LoadPrinterView(data);
      }
    }

    private void LoadPrinterView(ModelLoadingManager.PrinterViewLoadData data)
    {
      if (data.printSettings.HasValue)
      {
        printerview.LoadM3DPrintSettings(data.printSettings);
      }

      foreach (PrintDetails.ObjectDetails other in data.printerview_settings.objectList)
      {
        var objectDetails = new PrintDetails.ObjectDetails(other)
        {
          zipFileName = data.zipfile,
          printerSettingsXMLFile = null
        };
        var str = other.filename;
        if (!string.IsNullOrEmpty(data.directory))
        {
          str = Path.Combine(data.directory, str);
        }

        LoadModelIntoPrinter(str, new ModelLoadingManager.OnModelLoadedDel(OnModelLoadedCallback), new ModelLoadingManager.LoadFailedCallback(OnPrinterViewModelLoadFailed), objectDetails);
      }
    }

    private void OnPrinterViewModelLoadFailed(string name)
    {
      messagebox.AddMessageToQueue(string.Format("Unable to load model, {0}, from collection.", name));
    }

    private void OnFileLoadFailure(string name)
    {
      messagebox.AddMessageToQueue(string.Format("Unable to load model, {0}.", name));
    }

    private void TypeMismatchButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.tag == "Yes")
      {
        if (data is ModelLoadingManager.PrinterViewLoadData data1)
        {
          LoadPrinterView(data1);
        }

        parentFrame.CloseCurrent();
      }
      else if (button.tag == "No")
      {
        parentFrame.CloseCurrent();
      }
      else
      {
        if (!(button.tag == "dontshowcheckbox"))
        {
          return;
        }

        settings.CurrentAppearanceSettings.ShowPrinterMismatchWarning = !button.Checked;
      }
    }

    private string FindXMLSettingsFromList(List<string> xmlFiles, string modelName, string xmlFileString)
    {
      if (!string.IsNullOrEmpty(modelName))
      {
        foreach (var xmlFile in xmlFiles)
        {
          if (xmlFile.Contains(modelName) && xmlFile.EndsWith(xmlFileString))
          {
            return xmlFile;
          }
        }
      }
      foreach (var xmlFile in xmlFiles)
      {
        if (xmlFile.EndsWith(xmlFileString))
        {
          return xmlFile;
        }
      }
      return string.Empty;
    }

    private bool ImportModel(string filename, ModelLoadingManager.OnModelLoadedDel loadedCallback, ModelLoadingManager.LoadFailedCallback onFailCallback, PrintDetails.ObjectDetails objectDetails)
    {
      IncFilesLoading();
      try
      {
        long num = 0;
        using (var streamReader = new StreamReader(filename))
        {
          num = streamReader.BaseStream.Length;
        }

        if (num > 10485760L)
        {
          informationbox.AddMessageToQueue("Fairly complex and may slow down this program");
        }

        M3D.Graphics.Ext3D.ModelRendering.Model model = printerview.GetModel(filename);
        if (model == null)
        {
          var state = new ModelLoadingManager.AsyncModelLoadData(filename, loadedCallback, onFailCallback, objectDetails);
          if (settings.CurrentAppearanceSettings.ShowRemoveModelWarning && printerview.ModelLoaded && (objectDetails == null || objectDetails.printerSettingsXMLFile == null || objectDetails.printerViewXMLFile == null))
          {
            DecFilesLoading();
            messagebox.AddXMLMessageToQueue(new PopupMessageBox.MessageDataXML(new SpoolerMessage(), Resources.removeModelDialog, new PopupMessageBox.XMLButtonCallback(RemoveModelButtonCallback), state));
            return true;
          }
          if (printerview.ModelLoaded && !settings.CurrentAppearanceSettings.UseMultipleModels)
          {
            printerview.RemovePrintableModels();
          }

          StartLoadModelThread(state);
        }
        else
        {
          modelLoadedQueue.Enqueue(new ModelLoadingManager.ModelLoadDetails(model, objectDetails));
        }
      }
      catch (Exception ex)
      {
        DecFilesLoading();
        ShowFileLoadingExeption(ex, filename, onFailCallback);
        return false;
      }
      return true;
    }

    public bool LoadModelIntoPrinter(LibraryRecord record, ModelLoadingManager.LoadFailedCallback onFailCallback)
    {
      Form1.debugLogger.Add("LoadModelIntoPrinter()", "loading model into printer.", DebugLogger.LogType.Secondary);
      var num = LoadModelIntoPrinter(record.cachefilename, onFailCallback) ? 1 : 0;
      if (num == 0)
      {
        return num != 0;
      }

      Form1.debugLogger.Add("LoadModelIntoPrinter()", "Model loaded into printer.", DebugLogger.LogType.Secondary);
      Form1.debugLogger.Add("LoadModelIntoPrinter()", "OpenGL GetError(): " + GL.GetError().ToString() + ".", DebugLogger.LogType.Secondary);
      return num != 0;
    }

    private void StartLoadModelThread(ModelLoadingManager.AsyncModelLoadData state)
    {
      printerview.ResetPrinterView();
      ThreadPool.QueueUserWorkItem(new WaitCallback(LoadModelThread), state);
    }

    private void LoadModelThread(object state)
    {
      var asyncModelLoadData = state as ModelLoadingManager.AsyncModelLoadData;
      var model = (M3D.Graphics.Ext3D.ModelRendering.Model) null;
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
        ShowFileLoadingExeption(new Exception("load failure"), asyncModelLoadData.filename, asyncModelLoadData.onFailCallback);
        DecFilesLoading();
      }
      else
      {
        if (asyncModelLoadData.loadedCallback == null)
        {
          return;
        }

        asyncModelLoadData.loadedCallback(result, model, asyncModelLoadData.objectDetails);
      }
    }

    private void RemoveModelButtonCallback(ButtonWidget button, SpoolerMessage message, PopupMessageBox parentFrame, XMLFrame childFrame, object data)
    {
      if (button.tag == "dontshowcheckbox")
      {
        settings.CurrentAppearanceSettings.ShowRemoveModelWarning = !button.Checked;
      }
      else
      {
        if (button.tag == "Remove")
        {
          printerview.RemovePrintableModels();
          settings.CurrentAppearanceSettings.UseMultipleModels = false;
        }
        else if (button.tag == "Continue")
        {
          settings.CurrentAppearanceSettings.UseMultipleModels = true;
        }

        if (data is ModelLoadingManager.AsyncModelLoadData state)
        {
          IncFilesLoading();
          StartLoadModelThread(state);
        }
        parentFrame.CloseCurrent();
      }
    }

    private void ShowFileLoadingExeption(Exception e, string filename, ModelLoadingManager.LoadFailedCallback onLoadFailed)
    {
      onLoadFailed?.Invoke(filename);

      if (e is FileNotFoundException)
      {
        messagebox.AddMessageToQueue("Sorry, but the file was not found.");
      }
      else
      {
        messagebox.AddMessageToQueue("There was an error loading the file. Please check to make sure the file is valid.");
      }
    }

    public static M3D.Graphics.Ext3D.ModelRendering.Model LoadModelFromFile(string fileName)
    {
      return new M3D.Graphics.Ext3D.ModelRendering.Model(ModelLoadingManager.GetModelLoader(fileName).Load(fileName), fileName);
    }

    private static ModelImporterInterface GetModelLoader(string fileName)
    {
      var lowerInvariant = new SplitFileName(fileName).ext.ToLowerInvariant();
      var importerInterface = (ModelImporterInterface) null;
      if (lowerInvariant == "stl")
      {
        importerInterface = new ModelSTLImporter();
      }
      else if (lowerInvariant == "obj")
      {
        importerInterface = new ModelOBJImporter();
      }
      else if (lowerInvariant == "m3d")
      {
        importerInterface = new ModelM3DImporter();
      }
      else if (lowerInvariant == "smf")
      {
        importerInterface = new ModelSMFImporter();
      }

      return importerInterface;
    }

    private void OnModelLoadedCallback(ModelLoadingManager.Result result, M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails details)
    {
      if (result == ModelLoadingManager.Result.Success)
      {
        IncFilesOptimizing();
        modelLoadedQueue.Enqueue(new ModelLoadingManager.ModelLoadDetails(model, details));
      }
      else
      {
        DecFilesLoading();
        informationbox.AddMessageToQueue("Unable to load model.");
      }
    }

    private void IncFilesLoading()
    {
      lock (filesLoadingSync)
      {
        ++filesLoading;
      }
    }

    private void DecFilesLoading()
    {
      lock (filesLoadingSync)
      {
        --filesLoading;
      }
    }

    public bool LoadingNewModel
    {
      get
      {
        lock (filesLoadingSync)
        {
          return filesLoading > 0;
        }
      }
    }

    private void IncFilesOptimizing()
    {
      lock (filesOptimizingSync)
      {
        ++filesOptimizing;
      }
    }

    private void DecFilesOptimizing()
    {
      lock (filesOptimizingSync)
      {
        --filesOptimizing;
      }
    }

    public bool OptimizingModel
    {
      get
      {
        lock (filesOptimizingSync)
        {
          return filesOptimizing > 0;
        }
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
        modelFiles = new List<string>();
        xmlFiles = new List<string>();
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
