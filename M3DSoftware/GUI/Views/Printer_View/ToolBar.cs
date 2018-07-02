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
      colorframe = new Frame();
      colorframe.SetPosition(1, 4);
      colorframe.BGColor = new Color4(0.5f, 0.5f, 0.5f, 0.56f);
      AddChildElement(colorframe);
      buttonframe = new Frame();
      buttonframe.SetPosition(1, 4);
      AddChildElement(buttonframe);
      frameborder = new ImageWidget();
      frameborder.Init(host, "extendedcontrols2", 8f, 939f, 58f, 994f);
      frameborder.SetGrowableHeight(8, 8, 32);
      frameborder.SetGrowableWidth(1, 8, 32);
      frameborder.SetPosition(0, 0);
      frameborder.RelativeWidth = 1f;
      frameborder.RelativeHeight = 1f;
      frameborder.Color = new Color4(1f, 1f, 1f, 0.5f);
      AddChildElement(frameborder);
      CreateChildToolWindows(host);
      AddDefaultButtons(host);
    }

    private void AddDefaultButtons(GUIHost host)
    {
      translation_button = new ButtonWidget(2);
      translation_button.Init(host, "guicontrols", 704f, 128f, 760f, 183f, 768f, 128f, 824f, 183f, 832f, 128f, 888f, 183f, 449f, 705f, 505f, 760f);
      translation_button.Text = "";
      translation_button.SetCallback(new ButtonCallback(MyButtonCallback));
      translation_button.DontMove = true;
      translation_button.ClickType = ButtonType.Checkable;
      translation_button.CanClickOff = true;
      translation_button.GroupID = 2002;
      translation_button.tag = "ToolBar::Default::TranslateButton";
      translation_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_TRANSLATE");
      AddButton(translation_button);
      scaling_button = new ButtonWidget(1);
      scaling_button.Init(host, "guicontrols", 704f, 192f, 760f, 247f, 768f, 192f, 824f, 247f, 832f, 192f, 888f, 247f, 513f, 705f, 569f, 760f);
      scaling_button.Text = "";
      scaling_button.SetCallback(new ButtonCallback(MyButtonCallback));
      scaling_button.DontMove = true;
      scaling_button.ClickType = ButtonType.Checkable;
      scaling_button.CanClickOff = true;
      scaling_button.GroupID = 2002;
      scaling_button.tag = "ToolBar::Default::ScaleButton";
      scaling_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_SCALE");
      AddButton(scaling_button);
      rotation_button = new ButtonWidget(0);
      rotation_button.Init(host, "guicontrols", 704f, 256f, 760f, 311f, 768f, 256f, 824f, 311f, 832f, 256f, 888f, 311f, 577f, 705f, 633f, 760f);
      rotation_button.Text = "";
      rotation_button.SetCallback(new ButtonCallback(MyButtonCallback));
      rotation_button.DontMove = true;
      rotation_button.ClickType = ButtonType.Checkable;
      rotation_button.CanClickOff = true;
      rotation_button.GroupID = 2002;
      rotation_button.tag = "ToolBar::Default::RotateButton";
      rotation_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_ROTATE");
      AddButton(rotation_button);
      AddSpace(6);
      modellist_button = new ButtonWidget(3);
      modellist_button.Init(host, "extendedcontrols2", 67f, 975f, 116f, 1022f, 119f, 975f, 168f, 1022f, 171f, 975f, 220f, 1022f, 223f, 975f, 272f, 1022f);
      modellist_button.Text = "";
      modellist_button.SetCallback(new ButtonCallback(MyButtonCallback));
      modellist_button.DontMove = true;
      modellist_button.ClickType = ButtonType.Checkable;
      modellist_button.CanClickOff = true;
      modellist_button.GroupID = 2003;
      modellist_button.tag = "ToolBar::Default::ModelListButton";
      modellist_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_MODELLIST");
      AddButton(modellist_button);
      AddSpace(6);
      duplicate_button = new ButtonWidget(4);
      duplicate_button.Init(host, "extendedcontrols2", 275f, 875f, 324f, 922f, 327f, 875f, 376f, 922f, 379f, 875f, 428f, 922f, 431f, 875f, 480f, 922f);
      duplicate_button.Text = "";
      duplicate_button.SetCallback(new ButtonCallback(MyButtonCallback));
      duplicate_button.DontMove = false;
      duplicate_button.ClickType = ButtonType.Clickable;
      duplicate_button.CanClickOff = true;
      duplicate_button.tag = "ToolBar::Default::DuplicateButton";
      duplicate_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_DUPLICATE");
      duplicate_button.GroupID = 2004;
      AddButton(duplicate_button);
      remove_button = new ButtonWidget(5);
      remove_button.Init(host, "extendedcontrols2", 275f, 925f, 324f, 972f, 327f, 925f, 376f, 972f, 379f, 925f, 428f, 972f, 431f, 925f, 480f, 972f);
      remove_button.Text = "";
      remove_button.SetCallback(new ButtonCallback(MyButtonCallback));
      remove_button.DontMove = false;
      remove_button.ClickType = ButtonType.Clickable;
      remove_button.tag = "ToolBar::Default::RemoveButton";
      remove_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_REMOVE");
      remove_button.GroupID = 2004;
      AddButton(remove_button);
      undo_button = new ButtonWidget(6);
      undo_button.Init(host, "extendedcontrols2", 67f, 925f, 116f, 972f, 119f, 925f, 168f, 972f, 171f, 925f, 220f, 972f, 223f, 925f, 272f, 972f);
      undo_button.Text = "";
      undo_button.SetCallback(new ButtonCallback(MyButtonCallback));
      undo_button.DontMove = false;
      undo_button.ClickType = ButtonType.Clickable;
      undo_button.tag = "ToolBar::Default::UndoButton";
      undo_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_UNDO");
      undo_button.GroupID = 2004;
      AddButton(undo_button);
      redo_button = new ButtonWidget(7);
      redo_button.Init(host, "extendedcontrols2", 67f, 875f, 116f, 922f, 119f, 875f, 168f, 922f, 171f, 875f, 220f, 922f, 223f, 875f, 272f, 922f);
      redo_button.Text = "";
      redo_button.Width = 50;
      redo_button.Height = 48;
      redo_button.SetPosition(0, 0);
      redo_button.SetCallback(new ButtonCallback(MyButtonCallback));
      redo_button.DontMove = false;
      redo_button.ClickType = ButtonType.Clickable;
      redo_button.tag = "ToolBar::Default::RedoButton";
      redo_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_REDO");
      redo_button.GroupID = 2004;
      AddButton(redo_button);
      AddSpace(6);
      save_button = new ButtonWidget(8);
      save_button.Init(host, "extendedcontrols2", 275f, 975f, 324f, 1022f, 327f, 975f, 376f, 1022f, 379f, 975f, 428f, 1022f, 431f, 975f, 480f, 1022f);
      save_button.Text = "";
      save_button.SetCallback(new ButtonCallback(MyButtonCallback));
      save_button.DontMove = true;
      save_button.ClickType = ButtonType.Clickable;
      save_button.tag = "ToolBar::Default::SaveButton";
      save_button.ToolTipMessage = host.Locale.T("T_TOOLTIP_SAVE");
      save_button.GroupID = 2005;
      AddButton(save_button);
    }

    private void CreateChildToolWindows(GUIHost host)
    {
      modelListToolbox = new ModelListToolbox(3, printerview);
      modelListToolbox.Init(host);
      modelListToolbox.SetSize(300, 170);
      modelListToolbox.SetPosition(96, 200);
      modelListToolbox.RelativeY = 0.175f;
      printerview.AddChildElement(modelListToolbox);
    }

    private void AddButton(ButtonWidget button)
    {
      button.SetPosition(0, next_button_y);
      button.SetSize(50, 48);
      buttonframe.AddChildElement(button);
      next_button_y += 48;
      SetSize(55, 8 + next_button_y);
    }

    private void AddSpace(int amount)
    {
      next_button_y += amount;
      SetSize(55, 8 + next_button_y);
    }

    public override void OnUpdate()
    {
      base.OnUpdate();
      if (printerview.ModelLoaded)
      {
        rotation_button.Enabled = true;
        translation_button.Enabled = true;
        scaling_button.Enabled = true;
        duplicate_button.Enabled = true;
        remove_button.Enabled = true;
        modellist_button.Enabled = true;
        save_button.Enabled = true;
      }
      else
      {
        DisableControlButton(rotation_button);
        DisableControlButton(translation_button);
        DisableControlButton(scaling_button);
        DisableControlButton(modellist_button);
        duplicate_button.Enabled = false;
        remove_button.Enabled = false;
        save_button.Enabled = false;
      }
      if (printerview.History.CanUndo)
      {
        undo_button.Enabled = true;
      }
      else
      {
        undo_button.Enabled = false;
      }

      if (printerview.History.CanRedo)
      {
        redo_button.Enabled = true;
      }
      else
      {
        redo_button.Enabled = false;
      }
    }

    public void DeactivateModelAdjustDialog()
    {
      ModelAdjustmentButtonClicked(false);
      adjustmentsDialog.Deactivate();
    }

    private void DisableControlButton(ButtonWidget button)
    {
      if (button.Checked)
      {
        button.Checked = false;
      }

      button.Enabled = false;
    }

    private void MyButtonCallback(ButtonWidget button)
    {
      switch (button.ID)
      {
        case 0:
          RotateButtonClicked(button.Checked);
          break;
        case 1:
          ScaleButtonClicked(button.Checked);
          break;
        case 2:
          TranslateButtonClicked(button.Checked);
          break;
        case 3:
          ModelListButtonClicked(button.Checked);
          break;
        case 4:
          DuplicateButtonClicked();
          break;
        case 5:
          RemoveButtonClicked();
          break;
        case 6:
          UndoButtonClicked();
          break;
        case 7:
          RedoButtonClicked();
          break;
        case 8:
          SavePrintSettingsClicked();
          break;
      }
    }

    public override void SetSize(int width, int height)
    {
      buttonframe.SetSize(width - 5, height - 8);
      colorframe.SetSize(width - 5, height - 8);
      base.SetSize(width, height);
    }

    private void RotateButtonClicked(bool clickedOn)
    {
      ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
      {
        return;
      }

      adjustmentsDialog.UseRotationSliders();
    }

    private void ScaleButtonClicked(bool clickedOn)
    {
      ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
      {
        return;
      }

      adjustmentsDialog.UseScaleSliders();
    }

    private void TranslateButtonClicked(bool clickedOn)
    {
      ModelAdjustmentButtonClicked(clickedOn);
      if (!clickedOn)
      {
        return;
      }

      adjustmentsDialog.UseTranslationSliders();
    }

    public override void SetVisible(bool bVisible)
    {
      if (!bVisible)
      {
        adjustmentsDialog.Visible = false;
        modelListToolbox.Visible = false;
      }
      else
      {
        adjustmentsDialog.Visible = translation_button.Checked || rotation_button.Checked || scaling_button.Checked;
        modelListToolbox.Visible = modellist_button.Checked;
      }
      base.SetVisible(bVisible);
    }

    private void ModelListButtonClicked(bool clickedOn)
    {
      modelListToolbox.Visible = clickedOn;
      CheckDialogPositions();
    }

    private void SavePrintSettingsClicked()
    {
      var str = "Untitled.zip";
      var saveFileDialog = new SaveFileDialog
      {
        FileName = str,
        DefaultExt = ".zip",
        AddExtension = true,
        Filter = "Zip (*.zip)|*.zip"
      };
      if (saveFileDialog.ShowDialog() != DialogResult.OK)
      {
        return;
      }

      RecentPrintsHistory.PrintHistory printHistory = GatherData();
      if (RecentPrintsHistory.SavePrintHistoryToZip(saveFileDialog.FileName, printHistory))
      {
        libraryview.RecentModels.CopyAndAssignIconForLibrary(saveFileDialog.FileName, printHistory.iconfilename);
      }

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
      PrintJobDetails printJobDetails = printerview.CreatePrintJobDetails(out var modelZTooSmall);
      printJobDetails.GenerateSlicerSettings(null, printerview);
      var splitFileName = new SplitFileName(printJobDetails.objectDetailsList[0].filename);
      var printerJob = new JobParams("", splitFileName.name + "." + splitFileName.ext, printJobDetails.preview_image, FilamentSpool.TypeEnum.NoFilament, 0.0f, 0.0f)
      {
        options = printJobDetails.jobOptions,
        preprocessor = null,
        filament_temperature = 0,
        autoprint = printJobDetails.autoPrint
      };
      List<Slicer.General.KeyValuePair<string, string>> keyValuePairList = slicer_connection.SlicerSettings.GenerateUserKeyValuePairList();
      foreach (PrintDetails.ObjectDetails objectDetails in printJobDetails.objectDetailsList)
      {
        if (printJobDetails.autoPrint)
        {
          objectDetails.hidecontrols = true;
        }
      }
      RecentPrintsHistory.CreatePrintHistoryFolder(printerJob, null, slicer_connection.SlicerSettings.ProfileName, keyValuePairList, printJobDetails.objectDetailsList, out RecentPrintsHistory.PrintHistory cph);
      return cph;
    }

    private void ModelAdjustmentButtonClicked(bool clickedOn)
    {
      adjustmentsDialog.SaveCurrentSliderInfo();
      adjustmentsDialog.Visible = clickedOn;
      CheckDialogPositions();
    }

    private void CheckDialogPositions()
    {
      if (adjustmentsDialog.Visible && modelListToolbox.Visible)
      {
        modelListToolbox.RelativeYAdj = adjustmentsDialog.Height + 10;
      }
      else
      {
        if (!modelListToolbox.Visible)
        {
          return;
        }

        modelListToolbox.RelativeYAdj = 0;
      }
    }

    private void DuplicateButtonClicked()
    {
      printerview.DuplicateSelection();
    }

    private void RemoveButtonClicked()
    {
      printerview.RemoveSelectedModel();
    }

    private void UndoButtonClicked()
    {
      printerview.History.Undo();
    }

    private void RedoButtonClicked()
    {
      printerview.History.Redo();
    }

    public override void Refresh()
    {
      if (translation_button.Checked)
      {
        adjustmentsDialog.UseTranslationSliders();
      }

      if (scaling_button.Checked)
      {
        adjustmentsDialog.UseScaleSliders();
      }

      if (rotation_button.Checked)
      {
        adjustmentsDialog.UseRotationSliders();
      }

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
