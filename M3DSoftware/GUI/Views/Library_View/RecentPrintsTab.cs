// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Library_View.RecentPrintsTab
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.Slicer.General;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace M3D.GUI.Views.Library_View
{
  public class RecentPrintsTab : LibraryViewTab
  {
    public const string RecentModelGroupID = "313a13a6-9edf-44c7-a81d-50b914e423cc-6bb0e036-df11-4d66-82b3-8b7f0de03d2c";
    private ModelLoadingManager model_loading_manager;
    private LibraryView libraryView;
    private RecentPrintsHistory recentPrintsHistory;

    public RecentPrintsTab(LibraryView libraryView, ModelLoadingManager model_loading_manager)
    {
      this.model_loading_manager = model_loading_manager;
      this.libraryView = libraryView;
      this.recentPrintsHistory = new RecentPrintsHistory();
    }

    public bool CanSaveRecords
    {
      get
      {
        return true;
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
      if (!(record is RecentPrintsHistory.PrintHistory))
        return;
      RecentPrintsHistory.PrintHistory printHistory = (RecentPrintsHistory.PrintHistory) record;
      string str = "Untitled.zip";
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.FileName = str;
      saveFileDialog.DefaultExt = ".zip";
      saveFileDialog.AddExtension = true;
      saveFileDialog.Filter = "Zip (*.zip)|*.zip";
      if (saveFileDialog.ShowDialog() != DialogResult.OK || !RecentPrintsHistory.SavePrintHistoryToZip(saveFileDialog.FileName, printHistory))
        return;
      this.libraryView.RecentModels.CopyAndAssignIconForLibrary(saveFileDialog.FileName, printHistory.iconfilename);
    }

    public void Show(GUIHost host, GridLayout LibraryGrid, string filter)
    {
      LibraryGrid.Clear();
      Sprite.pixel_perfect = false;
      QueryResults<RecentPrintsHistory.PrintHistory> queryResults = this.recentPrintsHistory.QuereyRecords(filter);
      if (queryResults == null)
        return;
      int num = 0;
      foreach (RecentPrintsHistory.PrintHistory record in queryResults.records)
      {
        ButtonWidget buttonWidget = new ButtonWidget(1064 + num, (Element2D) null);
        if (string.IsNullOrEmpty(record.iconfilename) || !File.Exists(record.iconfilename))
          buttonWidget.Init(host, "null.png");
        else
          buttonWidget.Init(host, record.iconfilename);
        buttonWidget.DontMove = true;
        string str1 = record.ToString();
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
      host.Refresh();
    }

    public void MyButtonCallback(ButtonWidget button)
    {
      if (button.ID < 1064 || !(button.Data is RecentPrintsHistory.PrintHistory))
        return;
      this.LoadRecord((LibraryRecord) button.Data);
    }

    public void LoadRecord(LibraryRecord record)
    {
      bool flag = false;
      if (record is RecentPrintsHistory.PrintHistory)
      {
        RecentPrintsHistory.PrintHistory printHistory = (RecentPrintsHistory.PrintHistory) record;
        if (Directory.Exists(printHistory.folder))
        {
          this.model_loading_manager.LoadPrinterView(Path.Combine(printHistory.folder, "printerview.xml"), Path.Combine(printHistory.folder, "printersettings.xml"), printHistory.folder, "313a13a6-9edf-44c7-a81d-50b914e423cc-6bb0e036-df11-4d66-82b3-8b7f0de03d2c");
          flag = true;
        }
      }
      if (flag)
        return;
      this.RemoveRecord(record);
    }

    public void RemoveRecord(LibraryRecord record)
    {
      if (!(record is RecentPrintsHistory.PrintHistory))
        return;
      this.recentPrintsHistory.RemoveRecord((RecentPrintsHistory.PrintHistory) record);
    }

    public void AddRecentPrintHistory(JobParams printerJob, PrinterObject printer, string slicerProfileName, List<Slicer.General.KeyValuePair<string, string>> complete_slicer_settings, List<PrintDetails.ObjectDetails> original_objectList)
    {
      this.recentPrintsHistory.AddRecentPrintHistory(printerJob, printer, slicerProfileName, complete_slicer_settings, original_objectList);
    }
  }
}
