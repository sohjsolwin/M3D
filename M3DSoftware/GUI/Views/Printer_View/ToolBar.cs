// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.ToolBar
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Tools;
using M3D.GUI.Views.Library_View;
using M3D.GUI.Views.Printer_View.Print_Dialog_Widget;
using M3D.Slicer.General;
using M3D.Spooling.Common;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace M3D.GUI.Views.Printer_View
{
  internal class ToolBar : Frame
  {
    private ButtonWidget translation_button;
    private ButtonWidget scaling_button;
    private ButtonWidget rotation_button;
    private ButtonWidget modellist_button;
    private ButtonWidget duplicate_button;
    private ButtonWidget remove_button;
    private ButtonWidget undo_button;
    private ButtonWidget redo_button;
    private ButtonWidget save_button;
    private ModelListToolbox modelListToolbox;
    private ModelAdjustmentsDialog adjustmentsDialog;
    private PrinterView printerview;
    private LibraryView libraryview;
    private SlicerConnectionBase slicer_connection;
    private ImageWidget frameborder;
    private Frame buttonframe;
    private Frame colorframe;
    private const int ControlBarButtonsWidth = 50;
    private const int ControlBarButtonsHeight = 48;
    private const int leftbordersize = 1;
    private const int rightbordersize = 4;
    private const int topbordersize = 4;
    private const int botbordersize = 4;
    private int next_button_y;

    public ToolBar(int ID, PrinterView printerview, LibraryView libraryview, ModelAdjustmentsDialog adjustmentsDialog, SlicerConnectionBase slicer_connection)
    {
      this.adjustmentsDialog = adjustmentsDialog;
      this.printerview = printerview;
      this.libraryview = libraryview;
      this.slicer_connection = slicer_connection;
    }

    public void Init(GUIHost host)
    {
      Sprite.texture_height_pixels = 1024;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      this.colorframe = new Frame();
      this.colorframe.SetPosition(1, 4);
      this.colorframe.BGColor = new Color4(0.5f, 0.5f, 0.5f, 0.56f);
      this.AddChildElement((Element2D) this.colorframe);
      this.buttonframe = new Frame();
      this.buttonframe.SetPosition(1, 4);
      this.AddChildElement((Element2D) this.buttonframe);
      this.frameborder = new ImageWidget();
      this.frameborder.Init(host, "extendedcontrols2", 8f, 939f, 58f, 994f);
      this.frameborder.SetGrowableHeight(8, 8, 32);
      this.frameborder.SetGrowableWidth(1, 8, 32);
      this.frameborder.SetPosition(0, 0);
      this.frameborder.RelativeWidth = 1f;
      this.frameborder.RelativeHeight = 1f;
      this.frameborder.Color = new Color4(1f, 1f, 1f, 0.5f);
      this.AddChildElement((Element2D) this.frameborder);
      this.CreateChildToolWindows(host);
      this.AddDefaultButtons(host);
    }

    private void AddDefaultButtons(GUIHost host)
    {
      this.translation_button = new ButtonWidget(2);
      this.translation_button.Init(host, "guicontrols", 704f, 128f, 760f, 183f, 768f, 128f, 824f, 183f, 832f, 128f, 888f, 183f, 449f, 705f, 505f, 760f);
      this.translation_button.Text = "";
      this.translation_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.translation_button.DontMove = true;
      this.translation_button.ClickType = ButtonType.Checkable;
      this.translation_button.CanClickOff = true;
      this.translation_button.GroupID = 2002;
      this.translation_button.tag = "ToolBar::Default::TranslateButton";
      this.translation_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_TRANSLATE");
      this.AddButton(this.translation_button);
      this.scaling_button = new ButtonWidget(1);
      this.scaling_button.Init(host, "guicontrols", 704f, 192f, 760f, 247f, 768f, 192f, 824f, 247f, 832f, 192f, 888f, 247f, 513f, 705f, 569f, 760f);
      this.scaling_button.Text = "";
      this.scaling_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.scaling_button.DontMove = true;
      this.scaling_button.ClickType = ButtonType.Checkable;
      this.scaling_button.CanClickOff = true;
      this.scaling_button.GroupID = 2002;
      this.scaling_button.tag = "ToolBar::Default::ScaleButton";
      this.scaling_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_SCALE");
      this.AddButton(this.scaling_button);
      this.rotation_button = new ButtonWidget(0);
      this.rotation_button.Init(host, "guicontrols", 704f, 256f, 760f, 311f, 768f, 256f, 824f, 311f, 832f, 256f, 888f, 311f, 577f, 705f, 633f, 760f);
      this.rotation_button.Text = "";
      this.rotation_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.rotation_button.DontMove = true;
      this.rotation_button.ClickType = ButtonType.Checkable;
      this.rotation_button.CanClickOff = true;
      this.rotation_button.GroupID = 2002;
      this.rotation_button.tag = "ToolBar::Default::RotateButton";
      this.rotation_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_ROTATE");
      this.AddButton(this.rotation_button);
      this.AddSpace(6);
      this.modellist_button = new ButtonWidget(3);
      this.modellist_button.Init(host, "extendedcontrols2", 67f, 975f, 116f, 1022f, 119f, 975f, 168f, 1022f, 171f, 975f, 220f, 1022f, 223f, 975f, 272f, 1022f);
      this.modellist_button.Text = "";
      this.modellist_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.modellist_button.DontMove = true;
      this.modellist_button.ClickType = ButtonType.Checkable;
      this.modellist_button.CanClickOff = true;
      this.modellist_button.GroupID = 2003;
      this.modellist_button.tag = "ToolBar::Default::ModelListButton";
      this.modellist_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_MODELLIST");
      this.AddButton(this.modellist_button);
      this.AddSpace(6);
      this.duplicate_button = new ButtonWidget(4);
      this.duplicate_button.Init(host, "extendedcontrols2", 275f, 875f, 324f, 922f, 327f, 875f, 376f, 922f, 379f, 875f, 428f, 922f, 431f, 875f, 480f, 922f);
      this.duplicate_button.Text = "";
      this.duplicate_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.duplicate_button.DontMove = false;
      this.duplicate_button.ClickType = ButtonType.Clickable;
      this.duplicate_button.CanClickOff = true;
      this.duplicate_button.tag = "ToolBar::Default::DuplicateButton";
      this.duplicate_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_DUPLICATE");
      this.duplicate_button.GroupID = 2004;
      this.AddButton(this.duplicate_button);
      this.remove_button = new ButtonWidget(5);
      this.remove_button.Init(host, "extendedcontrols2", 275f, 925f, 324f, 972f, 327f, 925f, 376f, 972f, 379f, 925f, 428f, 972f, 431f, 925f, 480f, 972f);
      this.remove_button.Text = "";
      this.remove_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.remove_button.DontMove = false;
      this.remove_button.ClickType = ButtonType.Clickable;
      this.remove_button.tag = "ToolBar::Default::RemoveButton";
      this.remove_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_REMOVE");
      this.remove_button.GroupID = 2004;
      this.AddButton(this.remove_button);
      this.undo_button = new ButtonWidget(6);
      this.undo_button.Init(host, "extendedcontrols2", 67f, 925f, 116f, 972f, 119f, 925f, 168f, 972f, 171f, 925f, 220f, 972f, 223f, 925f, 272f, 972f);
      this.undo_button.Text = "";
      this.undo_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.undo_button.DontMove = false;
      this.undo_button.ClickType = ButtonType.Clickable;
      this.undo_button.tag = "ToolBar::Default::UndoButton";
      this.undo_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_UNDO");
      this.undo_button.GroupID = 2004;
      this.AddButton(this.undo_button);
      this.redo_button = new ButtonWidget(7);
      this.redo_button.Init(host, "extendedcontrols2", 67f, 875f, 116f, 922f, 119f, 875f, 168f, 922f, 171f, 875f, 220f, 922f, 223f, 875f, 272f, 922f);
      this.redo_button.Text = "";
      this.redo_button.Width = 50;
      this.redo_button.Height = 48;
      this.redo_button.SetPosition(0, 0);
      this.redo_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.redo_button.DontMove = false;
      this.redo_button.ClickType = ButtonType.Clickable;
      this.redo_button.tag = "ToolBar::Default::RedoButton";
      this.redo_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_REDO");
      this.redo_button.GroupID = 2004;
      this.AddButton(this.redo_button);
      this.AddSpace(6);
      this.save_button = new ButtonWidget(8);
      this.save_button.Init(host, "extendedcontrols2", 275f, 975f, 324f, 1022f, 327f, 975f, 376f, 1022f, 379f, 975f, 428f, 1022f, 431f, 975f, 480f, 1022f);
      this.save_button.Text = "";
      this.save_button.SetCallback(new ButtonCallback(this.MyButtonCallback));
      this.save_button.DontMove = true;
      this.save_button.ClickType = ButtonType.Clickable;
      this.save_button.tag = "ToolBar::Default::SaveButton";
      this.save_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_SAVE");
      this.save_button.GroupID = 2005;
      this.AddButton(this.save_button);
    }

    private void CreateChildToolWindows(GUIHost host)
    {
      this.modelListToolbox = new ModelListToolbox(3, this.printerview);
      this.modelListToolbox.Init(host);
      this.modelListToolbox.SetSize(300, 170);
      this.modelListToolbox.SetPosition(96, 200);
      this.modelListToolbox.RelativeY = 0.175f;
      this.printerview.AddChildElement((Element2D) this.modelListToolbox);
    }

    private void AddButton(ButtonWidget button)
    {
      button.SetPosition(0, this.next_button_y);
      button.SetSize(50, 48);
      this.buttonframe.AddChildElement((Element2D) button);
      this.next_button_y += 48;
      this.SetSize(55, 8 + this.next_button_y);
    }

    private void AddSpace(int amount)
    {
      this.next_button_y += amount;
      this.SetSize(55, 8 + this.next_button_y);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (this.printerview.ModelLoaded)
      {
        this.rotation_button.Enabled = true;
        this.translation_button.Enabled = true;
        this.scaling_button.Enabled = true;
        this.duplicate_button.Enabled = true;
        this.remove_button.Enabled = true;
        this.modellist_button.Enabled = true;
        this.save_button.Enabled = true;
      }
      else
      {
        this.DisableControlButton(this.rotation_button);
        this.DisableControlButton(this.translation_button);
        this.DisableControlButton(this.scaling_button);
        this.DisableControlButton(this.modellist_button);
        this.duplicate_button.Enabled = false;
        this.remove_button.Enabled = false;
        this.save_button.Enabled = false;
      }
      if (this.printerview.History.CanUndo)
        this.undo_button.Enabled = true;
      else
        this.undo_button.Enabled = false;
      if (this.printerview.History.CanRedo)
        this.redo_button.Enabled = true;
      else
        this.redo_button.Enabled = false;
    }

    public void DeactivateModelAdjustDialog()
    {
      this.ModelAdjustmentButtonClicked(false);
      this.adjustmentsDialog.Deactivate();
    }

    private void DisableControlButton(ButtonWidget button)
    {
      if (button.Checked)
        button.Checked = false;
      button.Enabled = false;
    }

    private void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          this.RotateButtonClicked(button.Checked);
          break;
        case 1:
          this.ScaleButtonClicked(button.Checked);
          break;
        case 2:
          this.TranslateButtonClicked(button.Checked);
          break;
        case 3:
          this.ModelListButtonClicked(button.Checked);
          break;
        case 4:
          this.DuplicateButtonClicked();
          break;
        case 5:
          this.RemoveButtonClicked();
          break;
        case 6:
          this.UndoButtonClicked();
          break;
        case 7:
          this.RedoButtonClicked();
          break;
        case 8:
          this.SavePrintSettingsClicked();
          break;
      }
    }

    public override void SetSize(int width, int height)
    {
      this.buttonframe.SetSize(width - 5, height - 8);
      this.colorframe.SetSize(width - 5, height - 8);
      base.SetSize(width, height);
    }

    private void RotateButtonClicked(bool clickedOn)
    {
      this.ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
        return;
      this.adjustmentsDialog.UseRotationSliders();
    }

    private void ScaleButtonClicked(bool clickedOn)
    {
      this.ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
        return;
      this.adjustmentsDialog.UseScaleSliders();
    }

    private void TranslateButtonClicked(bool clickedOn)
    {
      this.ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
        return;
      this.adjustmentsDialog.UseTranslationSliders();
    }

    public override void SetVisible(bool bVisible)
    {
      if (!bVisible)
      {
        this.adjustmentsDialog.Visible = false;
        this.modelListToolbox.Visible = false;
      }
      else
      {
        this.adjustmentsDialog.Visible = this.translation_button.Checked || this.rotation_button.Checked || this.scaling_button.Checked;
        this.modelListToolbox.Visible = this.modellist_button.Checked;
      }
      base.SetVisible(bVisible);
    }

    private void ModelListButtonClicked(bool clickedOn)
    {
      this.modelListToolbox.Visible = clickedOn;
      this.CheckDialogPositions();
    }

    private void SavePrintSettingsClicked()
    {
      string str = "Untitled.zip";
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      saveFileDialog.FileName = str;
      saveFileDialog.DefaultExt = ".zip";
      saveFileDialog.AddExtension = true;
      saveFileDialog.Filter = "Zip (*.zip)|*.zip";
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
        return;
      RecentPrintsHistory.PrintHistory printHistory = this.GatherData();
      if (RecentPrintsHistory.SavePrintHistoryToZip(saveFileDialog.FileName, printHistory))
        this.libraryview.RecentModels.CopyAndAssignIconForLibrary(saveFileDialog.FileName, printHistory.iconfilename);
      try
      {
        Directory.Delete(printHistory.folder, true);
      }
      catch (IOException ex)
      {
      }
    }

    private RecentPrintsHistory.PrintHistory GatherData()
    {
      bool modelZTooSmall;
      PrintJobDetails printJobDetails = this.printerview.CreatePrintJobDetails(out modelZTooSmall);
      printJobDetails.GenerateSlicerSettings((PrinterObject) null, this.printerview);
      SplitFileName splitFileName = new SplitFileName(printJobDetails.objectDetailsList[0].filename);
      JobParams printerJob = new JobParams("", splitFileName.name + "." + splitFileName.ext, printJobDetails.preview_image, FilamentSpool.TypeEnum.NoFilament, 0.0f, 0.0f);
      printerJob.options = printJobDetails.jobOptions;
      printerJob.preprocessor = (FilamentPreprocessorData) null;
      printerJob.filament_temperature = 0;
      printerJob.autoprint = printJobDetails.autoPrint;
      List<Slicer.General.KeyValuePair<string, string>> keyValuePairList = this.slicer_connection.SlicerSettings.GenerateUserKeyValuePairList();
      foreach (PrintDetails.ObjectDetails objectDetails in printJobDetails.objectDetailsList)
      {
        if (printJobDetails.autoPrint)
          objectDetails.hidecontrols = true;
      }
      RecentPrintsHistory.PrintHistory cph;
      RecentPrintsHistory.CreatePrintHistoryFolder(printerJob, (PrinterObject) null, this.slicer_connection.SlicerSettings.ProfileName, keyValuePairList, printJobDetails.objectDetailsList, out cph);
      return cph;
    }

    private void ModelAdjustmentButtonClicked(bool clickedOn)
    {
      this.adjustmentsDialog.SaveCurrentSliderInfo();
      this.adjustmentsDialog.Visible = clickedOn;
      this.CheckDialogPositions();
    }

    private void CheckDialogPositions()
    {
      if (this.adjustmentsDialog.Visible && this.modelListToolbox.Visible)
      {
        this.modelListToolbox.RelativeYAdj = this.adjustmentsDialog.Height + 10;
      }
      else
      {
        if (!this.modelListToolbox.Visible)
          return;
        this.modelListToolbox.RelativeYAdj = 0;
      }
    }

    private void DuplicateButtonClicked()
    {
      this.printerview.DuplicateSelection();
    }

    private void RemoveButtonClicked()
    {
      this.printerview.RemoveSelectedModel();
    }

    private void UndoButtonClicked()
    {
      this.printerview.History.Undo();
    }

    private void RedoButtonClicked()
    {
      this.printerview.History.Redo();
    }

    public override void Refresh()
    {
      if (this.translation_button.Checked)
        this.adjustmentsDialog.UseTranslationSliders();
      if (this.scaling_button.Checked)
        this.adjustmentsDialog.UseScaleSliders();
      if (this.rotation_button.Checked)
        this.adjustmentsDialog.UseRotationSliders();
      base.Refresh();
    }

    private enum ControlIDs
    {
      RotateButton,
      ScaleButton,
      TranslateButton,
      ModelListButton,
      DuplicateButton,
      RemoveButton,
      UndoButton,
      RedoButton,
      SaveButton,
    }
  }
}
