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
      libraryData = new RecentModelHistory();
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
      QueryResults<RecentModelHistory.RecentRecord> queryResults = libraryData.QuereyRecords(filter);
      if (queryResults == null)
      {
        return;
      }

      var num = 0;
      foreach (RecentModelHistory.RecentRecord record in queryResults.records)
      {
        var buttonWidget = new ButtonWidget(1064 + num, (Element2D) null);
        if (string.IsNullOrEmpty(record.iconfilename) || !File.Exists(record.iconfilename))
        {
          buttonWidget.Init(host, "null.png");
        }
        else
        {
          buttonWidget.Init(host, record.iconfilename);
        }

        buttonWidget.DontMove = true;
        var str1 = record._3dmodelfilename;
        if (str1.Length > 30)
        {
          var str2 = str1.Substring(0, 15);
          var str3 = str1.Substring(str1.Length - 15, 15);
          var str4 = " ... ";
          var str5 = str3;
          str1 = str2 + str4 + str5;
        }
        buttonWidget.Text = str1;
        buttonWidget.Color = new Color4(0.0f, 0.5f, 1f, 1f);
        buttonWidget.Size = FontSize.Small;
        buttonWidget.VAlignment = TextVerticalAlignment.Bottom;
        buttonWidget.Data = (object) record;
        buttonWidget.SetCallback(new ButtonCallback(MyButtonCallback));
        buttonWidget.SetFullyDraggable();
        LibraryGrid.AddChildElement((Element2D) buttonWidget);
        ++num;
      }
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID < 1064 || !(button.Data is RecentModelHistory.RecentRecord))
      {
        return;
      }

      LoadRecord((LibraryRecord) button.Data);
    }

    public void LoadRecord(LibraryRecord record)
    {
      if (!string.IsNullOrEmpty(record.cachefilename))
      {
        model_loading_manager.LoadModelIntoPrinter(record, new ModelLoadingManager.LoadFailedCallback(OnLoadFailure));
      }
      else
      {
        libraryData.RemoveFileFromRecent(record.ID);
        libraryView.ScheduleRefresh();
        infobox.AddMessageToQueue("Unable to load model from previous software.");
      }
    }

    private void OnLoadFailure(string name)
    {
      libraryData.RemoveFileFromRecent(name);
      libraryView.ScheduleRefresh();
    }

    public void RemoveRecord(LibraryRecord record)
    {
      libraryData.RemoveFileFromRecent(record.ID);
    }

    public void GenerateIconForLibrary(ModelTransformPair model_transform)
    {
      var zipFileName = model_transform.modelNode.zipFileName;
      if (!string.IsNullOrEmpty(zipFileName))
      {
        if (zipFileName == "313a13a6-9edf-44c7-a81d-50b914e423cc-6bb0e036-df11-4d66-82b3-8b7f0de03d2c")
        {
          return;
        }

        RecentModelHistory.RecentRecord record = libraryData.GetRecord(zipFileName);
        if (record != null && !string.IsNullOrEmpty(record.iconfilename))
        {
          ImageGenerated(zipFileName, record.iconfilename);
          return;
        }
      }
      var iconFileName = GenerateIconFileName(model_transform.modelNode.fileName);
      ImageCapture.GenerateAndSaveIcon(model_transform, iconFileName, new Vector2(400f, 400f), new Color4(0.8431373f, 0.8901961f, 0.9921569f, 1f), glControl, new ImageCapture.PreviewImageCallback(ImageGenerated));
    }

    public void CopyAndAssignIconForLibrary(string fileName, string sourceIconFile)
    {
      var str = (string) null;
      if (!string.IsNullOrEmpty(sourceIconFile))
      {
        str = GenerateIconFileName(fileName);
        try
        {
          File.Copy(sourceIconFile, str);
        }
        catch (IOException ex)
        {
          str = (string) null;
        }
      }
      ImageGenerated(fileName, str);
    }

    private string GenerateIconFileName(string modelname)
    {
      var splitFileName = new SplitFileName(modelname);
      var str = Path.Combine(Paths.PublicDataFolder, "MyLibrary", "Icons");
      if (!Directory.Exists(str))
      {
        Directory.CreateDirectory(str);
      }

      var num = DateTime.Now.Ticks / 10000L;
      return Path.Combine(str, splitFileName.name + num.ToString() + ".png");
    }

    private void ImageGenerated(string fileName, string iconFile)
    {
      libraryData.AddModelToRecent(fileName, iconFile);
      libraryView.ScheduleRefresh();
    }
  }
}
