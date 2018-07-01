// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.RecentModelTab
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Ext3D;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Utils;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Tools;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.IO;

namespace M3D.GUI.Views.Library_View
{
  public class RecentModelTab : LibraryViewTab
  {
    private RecentModelHistory libraryData;
    private ModelLoadingManager model_loading_manager;
    private LibraryView libraryView;
    private GLControl glControl;
    private MessagePopUp infobox;

    public RecentModelTab(LibraryView libraryView, ModelLoadingManager model_loading_manager, MessagePopUp infobox, GLControl glControl)
    {
      this.libraryData = new RecentModelHistory();
      this.model_loading_manager = model_loading_manager;
      this.libraryView = libraryView;
      this.glControl = glControl;
      this.infobox = infobox;
    }

    public bool CanSaveRecords
    {
      get
      {
        return false;
      }
    }

    public bool CanRemoveRecords
    {
      get
      {
        return true;
      }
    }

    public void SaveRecord(LibraryRecord record)
    {
    }

    public void Show(GUIHost host, GridLayout LibraryGrid, string filter)
    {
      LibraryGrid.Clear();
      Sprite.pixel_perfect = false;
      QueryResults<RecentModelHistory.RecentRecord> queryResults = this.libraryData.QuereyRecords(filter);
      if (queryResults == null)
        return;
      int num = 0;
      foreach (RecentModelHistory.RecentRecord record in queryResults.records)
      {
        ButtonWidget buttonWidget = new ButtonWidget(1064 + num, (Element2D) null);
        if (string.IsNullOrEmpty(record.iconfilename) || !File.Exists(record.iconfilename))
          buttonWidget.Init(host, "null.png");
        else
          buttonWidget.Init(host, record.iconfilename);
        buttonWidget.DontMove = true;
        string str1 = record._3dmodelfilename;
        if (str1.Length > 30)
        {
          string str2 = str1.Substring(0, 15);
          string str3 = str1.Substring(str1.Length - 15, 15);
          string str4 = " ... ";
          string str5 = str3;
          str1 = str2 + str4 + str5;
        }
        buttonWidget.Text = str1;
        buttonWidget.Color = new Color4(0.0f, 0.5f, 1f, 1f);
        buttonWidget.Size = FontSize.Small;
        buttonWidget.VAlignment = TextVerticalAlignment.Bottom;
        buttonWidget.Data = (object) record;
        buttonWidget.SetCallback(new ButtonCallback(this.MyButtonCallback));
        buttonWidget.SetFullyDraggable();
        LibraryGrid.AddChildElement((Element2D) buttonWidget);
        ++num;
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID < 1064 || !(button.Data is RecentModelHistory.RecentRecord))
        return;
      this.LoadRecord((LibraryRecord) button.Data);
    }

    public void LoadRecord(LibraryRecord record)
    {
      if (!string.IsNullOrEmpty(record.cachefilename))
      {
        this.model_loading_manager.LoadModelIntoPrinter(record, new ModelLoadingManager.LoadFailedCallback(this.OnLoadFailure));
      }
      else
      {
        this.libraryData.RemoveFileFromRecent(record.ID);
        this.libraryView.ScheduleRefresh();
        this.infobox.AddMessageToQueue("Unable to load model from previous software.");
      }
    }

    private void OnLoadFailure(string name)
    {
      this.libraryData.RemoveFileFromRecent(name);
      this.libraryView.ScheduleRefresh();
    }

    public void RemoveRecord(LibraryRecord record)
    {
      this.libraryData.RemoveFileFromRecent(record.ID);
    }

    public void GenerateIconForLibrary(ModelTransformPair model_transform)
    {
      string zipFileName = model_transform.modelNode.zipFileName;
      if (!string.IsNullOrEmpty(zipFileName))
      {
        if (zipFileName == "313a13a6-9edf-44c7-a81d-50b914e423cc-6bb0e036-df11-4d66-82b3-8b7f0de03d2c")
          return;
        RecentModelHistory.RecentRecord record = this.libraryData.GetRecord(zipFileName);
        if (record != null && !string.IsNullOrEmpty(record.iconfilename))
        {
          this.ImageGenerated(zipFileName, record.iconfilename);
          return;
        }
      }
      string iconFileName = this.GenerateIconFileName(model_transform.modelNode.fileName);
      ImageCapture.GenerateAndSaveIcon(model_transform, iconFileName, new Vector2(400f, 400f), new Color4(0.8431373f, 0.8901961f, 0.9921569f, 1f), this.glControl, new ImageCapture.PreviewImageCallback(this.ImageGenerated));
    }

    public void CopyAndAssignIconForLibrary(string fileName, string sourceIconFile)
    {
      string str = (string) null;
      if (!string.IsNullOrEmpty(sourceIconFile))
      {
        str = this.GenerateIconFileName(fileName);
        try
        {
          File.Copy(sourceIconFile, str);
        }
        catch (IOException ex)
        {
          str = (string) null;
        }
      }
      this.ImageGenerated(fileName, str);
    }

    private string GenerateIconFileName(string modelname)
    {
      SplitFileName splitFileName = new SplitFileName(modelname);
      string str = Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Icons");
      if (!Directory.Exists(str))
        Directory.CreateDirectory(str);
      long num = DateTime.Now.Ticks / 10000L;
      return Path.Combine(str, splitFileName.name + num.ToString() + ".png");
    }

    private void ImageGenerated(string fileName, string iconFile)
    {
      this.libraryData.AddModelToRecent(fileName, iconFile);
      this.libraryView.ScheduleRefresh();
    }
  }
}
