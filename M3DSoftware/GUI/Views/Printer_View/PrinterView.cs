using M3D.Graphics;
using M3D.Graphics.Ext3D;
using M3D.Graphics.Ext3D.ModelRendering;
using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.TextLocalization;
using M3D.Graphics.Utils;
using M3D.Graphics.Widgets2D;
using M3D.GUI.Controller;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Dialogs;
using M3D.GUI.Forms;
using M3D.GUI.OpenGL;
using M3D.GUI.Views.Library_View;
using M3D.GUI.Views.Printer_View.History;
using M3D.GUI.Views.Printer_View.Print_Dialog_Widget;
using M3D.Model;
using M3D.Slicer.General;
using M3D.Spooling.Client;
using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace M3D.GUI.Views.Printer_View
{
  public class PrinterView : Frame
  {
    public static readonly OpenTK.Vector2 librarySize = new OpenTK.Vector2(0.5f, 1f);
    public static readonly M3D.Model.Utils.Vector3 libraryViewPoint = new M3D.Model.Utils.Vector3(0.0f, 90f, 400f);
    private static readonly OpenTK.Vector2 editSize = new OpenTK.Vector2(1f, 1f);
    private static readonly M3D.Model.Utils.Vector3 editViewPoint = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 180f);
    private PrinterSerialNumber SerialNumber = PrinterSerialNumber.Undefined;
    private string printerColorString = "Automatic";
    private string modelColorString = "Automatic";
    private SettingsManager.GridUnit CurrentGridUnits = SettingsManager.GridUnit.MM;
    private M3D.Model.Utils.Vector3 min_scale = new M3D.Model.Utils.Vector3(0.1f, 0.1f, 0.1f);
    private M3D.Model.Utils.Vector3 max_scale = new M3D.Model.Utils.Vector3(4f, 4f, 4f);
    protected long animationTime = 1500;
    protected float minRotateVelocityFriction = 0.5f;
    private float printerZoomedAlpha = 0.075f;
    private bool m_bAllowRightMouseButtonControls = true;
    private int selected_model_index = -1;
    public PrinterView.ModelListChanged OnModelListChanged;
    private M3D.Model.Utils.Vector3 xAxisVector;
    private M3D.Model.Utils.Vector3 yAxisVector;
    private bool objectTransformed;
    private GUIHost m_gui_host;
    private ToolBar toolbar;
    private PrintDialogMainWindow printerdialog;
    private ModelAdjustmentsDialog AdjustmentsDialog;
    private EditViewControlBar BottomControlBar;
    private Frame EditFrame;
    private ViewState viewstate;
    private int realanimtiontime;
    private float target_y;
    private ModelLoadingManager model_loading_manager;
    private SettingsManager settings_manager;
    private SpoolerConnection spooler_connection;
    private SlicerConnectionBase slicer_connection;
    private PopupMessageBox messagebox;
    private LibraryView libraryview;
    private MessagePopUp infobox;
    private OpenGLConnection openGLConnection;
    private PrinterViewHistory history;
    protected float originalWidth;
    protected float originalHeight;
    protected float targetWidth;
    protected float targetHeight;
    private bool dragging;
    private bool animating;
    private bool controlsHidden;
    protected M3D.Model.Utils.Vector3 originalViewPointPos;
    protected M3D.Model.Utils.Vector3 targetViewPointPos;
    protected M3D.Model.Utils.Vector3 vectorToTargetViewPointPos;
    protected M3D.Model.Utils.Vector3 mCameraVelocity;
    protected float mWidthVelocity;
    protected long startTime;
    protected long elapsed;
    protected float fps;
    protected int frameCount;
    protected float movementMultiplier;
    private Form1 mainForm;
    protected int prevY;
    protected int prevX;
    private float rotVelocity;
    protected float rotVelocityInc;
    protected float rotateVelocityFriction;
    private JobOptions jobOptionsFromXML;
    private bool _autoPrint;
    private uint lastmodelID;
    private PrinterViewSceneGraph sceneGraph;

    public TransformationNode ModelTransformation
    {
      get
      {
        return SelectedModelTransformPair?.transformNode;
      }
    }

    public ModelTransformPair SelectedModelTransformPair
    {
      get
      {
        if (selected_model_index >= 0 && sceneGraph.ModelCount > 0 && selected_model_index < sceneGraph.ModelCount)
        {
          return sceneGraph.GetModel(selected_model_index);
        }

        return (ModelTransformPair) null;
      }
    }

    public bool ModelLoaded
    {
      get
      {
        return sceneGraph.ModelCount > 0;
      }
    }

    public string LoadedModelFileName
    {
      get
      {
        return SelectedModelTransformPair?.modelNode.fileName;
      }
    }

    public PrinterViewHistory History
    {
      get
      {
        return history;
      }
    }

    public ModelLoadingManager ModelLoadingInterface
    {
      get
      {
        return model_loading_manager;
      }
    }

    public ViewState ViewState
    {
      get
      {
        return viewstate;
      }
    }

    public M3D.Model.Utils.Vector3 MinScale
    {
      get
      {
        return min_scale;
      }
    }

    public M3D.Model.Utils.Vector3 MaxScale
    {
      get
      {
        return max_scale;
      }
    }

    public OpenGLConnection OpenGLConnection
    {
      get
      {
        return openGLConnection;
      }
    }

    public Frame GetEditFrame()
    {
      return EditFrame;
    }

    public bool AutoPrint
    {
      get
      {
        return _autoPrint;
      }
    }

    public bool RightMouseButtonControlEnabled
    {
      get
      {
        return m_bAllowRightMouseButtonControls;
      }
      set
      {
        m_bAllowRightMouseButtonControls = value;
      }
    }

    public string CurrentFamilyName { get; private set; }

    public PrinterSizeProfile CurrentSizeProfile { get; private set; }

    public PrinterView(Form1 mainForm, GUIHost host, OpenGLConnection openGLConnection, SpoolerConnection spooler_connection, SlicerConnectionBase slicer_connection, ModelLoadingManager model_loading_manager, PopupMessageBox messagebox, MessagePopUp infobox, SettingsManager settings_manager, LibraryView libraryview)
      : base(123456)
    {
      bUpdateWhenNotVisible = true;
      this.openGLConnection = openGLConnection;
      history = new PrinterViewHistory(this)
      {
        RecordHistory = false
      };
      RelativeX = 0.0f;
      RelativeY = 0.0f;
      RelativeWidth = PrinterView.librarySize.X;
      RelativeHeight = PrinterView.librarySize.Y;
      targetWidth = PrinterView.librarySize.X;
      targetHeight = PrinterView.librarySize.Y;
      this.mainForm = mainForm;
      mCameraVelocity = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      m_gui_host = host;
      this.slicer_connection = slicer_connection;
      this.spooler_connection = spooler_connection;
      this.model_loading_manager = model_loading_manager;
      this.settings_manager = settings_manager;
      this.messagebox = messagebox;
      this.infobox = infobox;
      this.libraryview = libraryview;
      CreatePrinterViewObjects();
      CreateEditViewObjects();
      viewstate = ViewState.Hidden;
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(SelectedPrinterChanged);
      history.RecordHistory = true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.Element;
    }

    private void CreatePrinterViewObjects()
    {
      CurrentGridUnits = settings_manager.CurrentAppearanceSettings.Units;
      sceneGraph = new PrinterViewSceneGraph(m_gui_host, CurrentGridUnits, settings_manager.CurrentAppearanceSettings.CaseType);
      AddChildElement((Element2D)sceneGraph);
      xAxisVector = new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f);
      yAxisVector = new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f);
      SetPrinterColor(settings_manager.CurrentAppearanceSettings.PrinterColor);
      SetModelColor(settings_manager.CurrentAppearanceSettings.ModelColor);
      UpdatePrinterFamily();
    }

    private void CreateEditViewObjects()
    {
      Sprite.texture_height_pixels = 512;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      m_gui_host.SetFontProperty(FontSize.VeryLarge, 20f);
      m_gui_host.SetFontProperty(FontSize.Large, 14f);
      m_gui_host.SetFontProperty(FontSize.Medium, 11f);
      m_gui_host.SetFontProperty(FontSize.Small, 8f);
      WrapOnNegative = false;
      EditFrame = new Frame(0, (Element2D) this);
      EditFrame.SetPosition(0, 0);
      EditFrame.RelativeWidth = 2f;
      EditFrame.RelativeHeight = 1f;
      AddChildElement((Element2D)EditFrame);
      AdjustmentsDialog = new ModelAdjustmentsDialog(8001, this);
      AdjustmentsDialog.Init(m_gui_host);
      AdjustmentsDialog.Width = 339;
      AdjustmentsDialog.Height = 182;
      AdjustmentsDialog.X = 96;
      AdjustmentsDialog.Visible = false;
      AddChildElement((Element2D)AdjustmentsDialog);
      BottomControlBar = new EditViewControlBar(9001, this);
      BottomControlBar.Init(m_gui_host);
      AddChildElement((Element2D)BottomControlBar);
      toolbar = new ToolBar(10001, this, libraryview, AdjustmentsDialog, slicer_connection);
      toolbar.Init(m_gui_host);
      toolbar.SetPosition(0, 200);
      toolbar.CenterVerticallyInParent = true;
      AddChildElement((Element2D)toolbar);
      printerdialog = new PrintDialogMainWindow(11001, m_gui_host, this, spooler_connection, messagebox, model_loading_manager, slicer_connection, libraryview.RecentPrints, settings_manager)
      {
        Visible = false
      };
      m_gui_host.Refresh();
      viewstate = ViewState.Hidden;
      OnSetPosition();
      ShowView(false);
    }

    private void DoModelListChanged()
    {
      if (OnModelListChanged == null)
      {
        return;
      }

      OnModelListChanged(sceneGraph.GetModelDataList());
    }

    private string GetSimplifiedName(string filepath)
    {
      var name = Path.GetFileNameWithoutExtension(filepath);
      if (name.Length > 24)
      {
        name = name.Substring(0, 24);
      }

      return CheckDuplicateNames(name);
    }

    private string CheckDuplicateNames(string name)
    {
      var num = -1;
      var str1 = "_";
      foreach (ModelTransformPair.Data modelData in sceneGraph.GetModelDataList())
      {
        if (modelData.name.StartsWith(name))
        {
          if (modelData.name == name)
          {
            num = 1;
          }
          else
          {
            var str2 = modelData.name.Substring(name.Length);
            if (str2.StartsWith(str1))
            {
              var s = str2.Substring(1);
              var result = -1;
              if (int.TryParse(s, out result))
              {
                num = result + 1;
              }
            }
          }
        }
      }
      if (num > 0)
      {
        name = name + str1 + num.ToString("D2");
      }

      return name;
    }

    private uint GetNextModelIndex()
    {
      return ++lastmodelID;
    }

    public void SelectModel(Ray pickRay)
    {
      var index1 = -1;
      var vector3_1 = (M3D.Model.Utils.Vector3) null;
      for (var index2 = 0; index2 < sceneGraph.ModelCount; ++index2)
      {
        ModelTransformPair model = sceneGraph.GetModel(index2);
        var boundingBox = new M3D.Graphics.Utils.BoundingBox(model.modelSize.Min, model.modelSize.Max);
        var vector3_2 = new M3D.Model.Utils.Vector3(pickRay.Position);
        M3D.Model.Utils.Vector3 vector3_3 = vector3_2 + pickRay.Direction;
        M3D.Model.Utils.Vector3 p1 = vector3_2;
        M3D.Model.Utils.Vector3 p2 = vector3_3;
        if (boundingBox.LineIntercepts(out var local, p1, p2))
        {
          if (index1 < 0)
          {
            index1 = index2;
            vector3_1 = new M3D.Model.Utils.Vector3(local.x, local.y, local.z);
          }
          else
          {
            var vector3_5 = new M3D.Model.Utils.Vector3(local.x, local.y, local.z);
            if ((double) vector3_5.Distance(pickRay.Position) < (double) vector3_1.Distance(pickRay.Position))
            {
              index1 = index2;
              vector3_1 = vector3_5;
            }
          }
        }
      }
      if (index1 < 0)
      {
        return;
      }

      SelectModel(index1);
    }

    public void SelectModel(int index)
    {
      var previousObject = uint.MaxValue;
      var objectID = uint.MaxValue;
      ModelTransformPair modelTransformPair1 = SelectedModelTransformPair;
      if (modelTransformPair1 != null)
      {
        previousObject = modelTransformPair1.data.ID;
        UnselectModel();
      }
      if (index >= 0 && index < sceneGraph.ModelCount)
      {
        selected_model_index = index;
        ModelTransformPair modelTransformPair2 = SelectedModelTransformPair;
        objectID = modelTransformPair2.data.ID;
        SetModelRanges(modelTransformPair2, CurrentSizeProfile);
        if (sceneGraph.ModelCount > 1)
        {
          modelTransformPair2.modelNode.Highlight = true;
        }
      }
      History.PushSelectObject(objectID, previousObject);
    }

    public void PushUpdatedInfomation(ModelTransformPair model)
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null || (int) model.data.ID != (int) modelTransformPair.data.ID)
      {
        return;
      }

      AdjustmentsDialog.PushUpdatedInfomation(model);
    }

    public ModelTransformPair GetModelByID(uint id)
    {
      for (var index = 0; index < sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = sceneGraph.GetModel(index);
        if ((int) model.data.ID == (int) id)
        {
          return model;
        }
      }
      return (ModelTransformPair) null;
    }

    public bool SelectModelbyID(uint id)
    {
      for (var index = 0; index < sceneGraph.ModelCount; ++index)
      {
        if ((int)sceneGraph.GetModel(index).data.ID == (int) id)
        {
          SelectModel(index);
          return true;
        }
      }
      return false;
    }

    public void UnselectModel()
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair != null)
      {
        AdjustmentsDialog.Refresh();
        modelTransformPair.modelNode.Highlight = false;
      }
      selected_model_index = -1;
    }

    public Matrix4x4 GetObjectSlicerTransformation()
    {
      var matrix4x4 = new Matrix4x4();
      matrix4x4.Identity();
      var vector3_1 = new M3D.Model.Utils.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      var vector3_2 = new M3D.Model.Utils.Vector3(float.MinValue, float.MinValue, float.MinValue);
      for (var index = 0; index < sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = sceneGraph.GetModel(index);
        model.CalculateExtents();
        if ((double) model.modelSize.Min.x < (double) vector3_1.x)
        {
          vector3_1.x = model.modelSize.Min.x;
        }

        if ((double) model.modelSize.Min.y < (double) vector3_1.y)
        {
          vector3_1.y = model.modelSize.Min.y;
        }

        if ((double) model.modelSize.Max.x > (double) vector3_2.x)
        {
          vector3_2.x = model.modelSize.Max.x;
        }

        if ((double) model.modelSize.Max.y > (double) vector3_2.y)
        {
          vector3_2.y = model.modelSize.Max.y;
        }
      }
      matrix4x4.m[3, 0] = (float) (((double) vector3_1.x + (double) vector3_2.x) / 2.0);
      matrix4x4.m[3, 1] = (float) (((double) vector3_1.y + (double) vector3_2.y) / 2.0);
      return matrix4x4;
    }

    public bool IsModelLoaded()
    {
      return sceneGraph.ModelCount > 0;
    }

    public void RemovePrintableModels()
    {
      History.RecordHistory = false;
      toolbar.DeactivateModelAdjustDialog();
      UnselectModel();
      while (sceneGraph.ModelCount > 0)
      {
        RemoveModel(0);
      }

      selected_model_index = -1;
      SetControlStateMaster(false);
      sceneGraph.ResetExceedBounds();
      History.Clear();
      History.RecordHistory = true;
    }

    public void ResetObjectMatrices()
    {
      CenterPrinterObject();
    }

    public int AddModel(M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails objectDetails)
    {
      var recordHistory = history.RecordHistory;
      history.RecordHistory = false;
      if (settings_manager.CurrentAppearanceSettings.AutoDetectModelUnits)
      {
        PrinterView.Units sourceUnits = DetectUnits(new ModelSize(model.modelData.Min, model.modelData.Max));
        if (sourceUnits != PrinterView.Units.MMs)
        {
          RescaleModel(model, sourceUnits);
          if (sourceUnits == PrinterView.Units.Inches)
          {
            infobox.AddMessageToQueue("T_InchesDetected");
          }
          else if (sourceUnits == PrinterView.Units.Microns)
          {
            infobox.AddMessageToQueue("T_MicronsDetected");
          }
        }
      }
      var modelSize = new ModelSize(model.modelData.Min, model.modelData.Max);
      M3D.Model.Utils.Vector3 freePosition = sceneGraph.FindFreePosition(new OpenTK.Vector2(modelSize.Ext.x, modelSize.Ext.y));
      var num = sceneGraph.AddModel(model, out ModelTransformPair modtrans_pair);
      modtrans_pair.data.name = GetSimplifiedName(model.fileName);
      modtrans_pair.data.ID = objectDetails == null || objectDetails.UID == uint.MaxValue ? GetNextModelIndex() : objectDetails.UID;
      SelectModel(num);
      ResetTransformationValues();
      ResetObjectMatrices();
      AutoScaleModel();
      PlaceModel(freePosition);
      if (objectDetails != null)
      {
        SetModelAdjustmentsFromObjectDetails(modtrans_pair, objectDetails);
      }

      sceneGraph.PlaceObjectOnFloorAndCheckBounds(num);
      AdjustmentsDialog.CheckLinkedRescalingCheckbox(true);
      if (objectDetails == null)
      {
        libraryview.RecentModels.GenerateIconForLibrary(modtrans_pair);
      }

      DoModelListChanged();
      history.RecordHistory = objectDetails == null || objectDetails.UID == uint.MaxValue;
      history.PushAddModelFile(modtrans_pair.data.ID, modtrans_pair.modelNode.fileName, modtrans_pair.modelNode.zipFileName, modtrans_pair.transformNode.TransformData);
      history.RecordHistory = recordHistory;
      mainForm.SetToEditView();
      return num;
    }

    private PrinterView.Units DetectUnits(ModelSize modelSize)
    {
      if ((double) modelSize.Ext.x < 10.0 && (double) modelSize.Ext.y < 10.0 && (double) modelSize.Ext.z < 10.0)
      {
        return PrinterView.Units.Inches;
      }

      return (double) modelSize.Ext.x > 10000.0 || (double) modelSize.Ext.y > 10000.0 || (double) modelSize.Ext.z > 10000.0 ? PrinterView.Units.Microns : PrinterView.Units.MMs;
    }

    private void RescaleModel(M3D.Graphics.Ext3D.ModelRendering.Model model, PrinterView.Units sourceUnits)
    {
      float num;
      if (sourceUnits == PrinterView.Units.Microns)
      {
        num = 1f / 1000f;
      }
      else
      {
        if (PrinterView.Units.Inches != sourceUnits)
        {
          return;
        }

        num = 25.4f;
      }
      model.geometryData.Scale(num, num, num);
      model.modelData.Scale(new M3D.Model.Utils.Vector3(num, num, num));
    }

    private void AutoScaleModel()
    {
      var num1 = Math.Abs(sceneGraph.PrinterBedWidth * sceneGraph.PrinterBedLength);
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      M3D.Model.Utils.Vector3 scaleValues1 = GetScaleValues();
      M3D.Model.Utils.Vector3 scaleValues2 = GetScaleValues();
      var num2 = 1f;
      modelTransformPair.modelNode.CalculateMinMax();
      M3D.Model.Utils.Vector3 ext = modelTransformPair.modelNode.Ext;
      var num3 = Math.Abs((float) ((double) ext.x * (double) scaleValues1.x * ((double) ext.y * (double) scaleValues1.y)));
      if ((double) num3 < 0.01 * (double) num1)
      {
        if ((double) num3 > 1.0)
        {
          num2 = 0.01f * num1 / num3;
        }
        else if ((double) num3 < 1.0)
        {
          num2 = num1 * 0.1f * num3;
        }
      }
      if ((double) num2 > (double)MaxScale.x)
      {
        num2 = MaxScale.x;
      }

      if ((double) num2 > (double)MaxScale.y)
      {
        num2 = MaxScale.y;
      }

      if ((double) num2 > (double)MaxScale.z)
      {
        num2 = MaxScale.z;
      }

      if ((double) num2 < (double)MinScale.x)
      {
        num2 = MinScale.x;
      }

      if ((double) num2 < (double)MinScale.y)
      {
        num2 = MinScale.y;
      }

      if ((double) num2 < (double)MinScale.z)
      {
        num2 = MinScale.z;
      }

      scaleValues2.x *= num2;
      scaleValues2.y *= num2;
      scaleValues2.z *= num2;
      if ((double) scaleValues2.x < (double) scaleValues1.x / 2.0 || (double) scaleValues2.x > (double) scaleValues1.x * 2.0)
      {
        messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, Locale.GlobalLocale.T("T_WARNING_LargeScaleUsedMaybeCorrupt")), PopupMessageBox.MessageBoxButtons.OK);
      }

      ScalePrinterObject(num2, num2, num2);
    }

    public M3D.Graphics.Ext3D.ModelRendering.Model GetModel(string filename)
    {
      for (var index = 0; index < sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = sceneGraph.GetModel(index);
        if (model.modelNode.fileName == filename)
        {
          return model.modelNode.GetModelInfo();
        }
      }
      return (M3D.Graphics.Ext3D.ModelRendering.Model) null;
    }

    private void SetModelAdjustmentsFromObjectDetails(ModelTransformPair modtrans_pair, PrintDetails.ObjectDetails objectDetails)
    {
      Model3DNode modelNode = modtrans_pair.modelNode;
      modelNode.zipFileName = objectDetails.zipFileName;
      if (!string.IsNullOrEmpty(objectDetails.printerViewXMLFile) && SettingsManager.LoadPrinterViewFile(objectDetails.printerViewXMLFile, out PrintDetails.PrintJobObjectViewDetails printerview_settings))
      {
        var lower = Path.GetFileNameWithoutExtension(modelNode.fileName).ToLower();
        var objectDetails1 = (PrintDetails.ObjectDetails)null;
        foreach (PrintDetails.ObjectDetails objectDetails2 in printerview_settings.objectList)
        {
          if (Path.GetFileNameWithoutExtension(objectDetails2.filename).ToLower() == lower)
          {
            objectDetails1 = objectDetails2;
            break;
          }
        }
        if (objectDetails1 != null || printerview_settings.objectList.Count > 0)
        {
          if (objectDetails1 == null)
          {
            objectDetails1 = printerview_settings.objectList[0];
          }

          objectDetails.transform = objectDetails1.transform;
          objectDetails.hidecontrols = objectDetails1.hidecontrols;
        }
      }
      if (!string.IsNullOrEmpty(objectDetails.printerSettingsXMLFile))
      {
        LoadM3DPrintSettings(SettingsManager.LoadPrintJobInfoFile(objectDetails.printerSettingsXMLFile));
      }

      if (objectDetails.transform != null)
      {
        modtrans_pair.transformNode.Translation = objectDetails.transform.translation.GUIVector();
        modtrans_pair.transformNode.Rotation = objectDetails.transform.rotation.GUIVector();
        modtrans_pair.transformNode.Scale = objectDetails.transform.scale.GUIVector();
      }
      SetControlStateMaster(objectDetails.hidecontrols);
    }

    public void LoadM3DPrintSettings(PrintDetails.M3DSettings? printsettings)
    {
      if (!printsettings.HasValue)
      {
        return;
      }

      List<Slicer.General.KeyValuePair<string, string>> curaSettings = printsettings.Value.printSettings.CuraSettings;
      if (curaSettings != null && slicer_connection.SlicerSettings != null)
      {
        slicer_connection.SlicerSettings.LoadFromUserKeyValuePairList(curaSettings);
      }
      else
      {
        infobox.AddMessageToQueue("Unable to load print settings because a printer has not been connected.");
      }

      jobOptionsFromXML = printsettings.Value.printSettings.options;
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter.MyPrinterProfile == null)
      {
        return;
      }

      SettingsManager.PrintSettings printSettingsSafe = settings_manager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
      if (printsettings.Value.printSettings.options == null)
      {
        return;
      }

      printSettingsSafe.WaveBonding = printsettings.Value.printSettings.options.use_wave_bonding;
    }

    public void RemoveModel(int index)
    {
      if (index < 0 || index >= sceneGraph.ModelCount)
      {
        return;
      }

      sceneGraph.RemoveModel(index);
      DoModelListChanged();
    }

    public bool ObjectTransformed
    {
      set
      {
        objectTransformed = value;
      }
      get
      {
        return objectTransformed;
      }
    }

    public void ResetPrinterView()
    {
      sceneGraph.ResetPrinterTransformation();
      xAxisVector.x = 1f;
      xAxisVector.y = 0.0f;
      xAxisVector.z = 0.0f;
      yAxisVector.x = 0.0f;
      yAxisVector.y = -1f;
      yAxisVector.z = 0.0f;
    }

    public void CenterPrinterObject()
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      TransformationNode.Transform transformData1 = modelTransformPair.transformNode.TransformData;
      PlaceModel(new M3D.Model.Utils.Vector3(sceneGraph.PrinterCenter));
      TransformationNode.Transform transformData2 = modelTransformPair.transformNode.TransformData;
      history.PushTransformObject(modelTransformPair.data.ID, transformData1, transformData2);
      sceneGraph.PlaceObjectOnFloorAndCheckBounds(selected_model_index);
      PushUpdatedInfomation(modelTransformPair);
    }

    public void PlaceModel(M3D.Model.Utils.Vector3 position)
    {
      TransformationNode modelTransformation = ModelTransformation;
      if (modelTransformation == null)
      {
        return;
      }

      modelTransformation.Translation.x = position.x;
      modelTransformation.Translation.y = position.y;
      AdjustmentsDialog.SetTranslationValues(position.x, position.y);
    }

    public void AdjustPrinterObjectTranslation(float x, float y, float z)
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      AdjustmentsDialog.SetTranslationValues(new M3D.Model.Utils.Vector3(modelTransformPair.transformNode.Translation.x + x, modelTransformPair.transformNode.Translation.y + y, modelTransformPair.transformNode.Translation.z + z));
    }

    public void ScalePrinterObject(float x, float y, float z)
    {
      if (SelectedModelTransformPair == null)
      {
        return;
      }

      AdjustmentsDialog.SetScaleValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    public void RotatePrinterObject(float x, float y, float z)
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      AdjustmentsDialog.SetRotationValues(new M3D.Model.Utils.Vector3(modelTransformPair.transformNode.Rotation.x + x, modelTransformPair.transformNode.Rotation.y + y, modelTransformPair.transformNode.Rotation.z + z));
    }

    public void SetPrinterObjectTranslation(M3D.Model.Utils.Vector3 vector)
    {
      SetPrinterObjectTranslation(vector.x, vector.y);
    }

    public void SetPrinterObjectTranslation(float x, float y)
    {
      if (SelectedModelTransformPair == null)
      {
        return;
      }

      AdjustmentsDialog.SetTranslationValues(new M3D.Model.Utils.Vector3(x, y, 0.0f));
    }

    public void SetPrinterObjectRotatation(M3D.Model.Utils.Vector3 vector)
    {
      SetPrinterObjectRotatation(vector.x, vector.y, vector.z);
    }

    public void SetPrinterObjectRotatation(float x, float y, float z)
    {
      if (SelectedModelTransformPair == null)
      {
        return;
      }

      AdjustmentsDialog.SetRotationValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    public void SetPrinterObjectScale(M3D.Model.Utils.Vector3 vector)
    {
      SetPrinterObjectScale(vector.x, vector.y, vector.z);
    }

    public void SetPrinterObjectScale(float x, float y, float z)
    {
      if (selected_model_index < 0 || selected_model_index >= sceneGraph.ModelCount)
      {
        return;
      }

      AdjustmentsDialog.SetScaleValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    private void CheckUpdatedColors()
    {
      if (printerColorString != settings_manager.CurrentAppearanceSettings.PrinterColor)
      {
        SetPrinterColor(settings_manager.CurrentAppearanceSettings.PrinterColor);
      }

      if (modelColorString != settings_manager.CurrentAppearanceSettings.ModelColor || modelColorString == "Automatic")
      {
        SetModelColor(settings_manager.CurrentAppearanceSettings.ModelColor);
      }

      if (!(ImageCapture.IconColor != settings_manager.CurrentAppearanceSettings.IconColor))
      {
        return;
      }

      ImageCapture.IconColor = settings_manager.CurrentAppearanceSettings.IconColor;
    }

    private void CheckUpdatedCaseType()
    {
      if (!(SerialNumber == PrinterSerialNumber.Undefined) || sceneGraph.PrinterModelCaseType == settings_manager.CurrentAppearanceSettings.CaseType)
      {
        return;
      }

      sceneGraph.PrinterModelCaseType = settings_manager.CurrentAppearanceSettings.CaseType;
      UpdatePrinterFamily();
    }

    private void CheckUpdatedMeasumentUnits()
    {
      if (CurrentGridUnits == settings_manager.CurrentAppearanceSettings.Units)
      {
        return;
      }

      CurrentGridUnits = settings_manager.CurrentAppearanceSettings.Units;
      sceneGraph.GridCurrentUnits = CurrentGridUnits;
    }

    private void SetPrinterColor(string color)
    {
      printerColorString = color;
      if (color == "Automatic")
      {
        sceneGraph.PrinterColor = Colors.GetColorFromSN(SerialNumber.ToString());
      }
      else
      {
        try
        {
          sceneGraph.PrinterColor = Colors.GetColor(color);
        }
        catch (ArgumentException ex)
        {
          if (ex.Message == "Not a valid color")
          {
            sceneGraph.PrinterColor = Colors.GetColor("Black");
          }
        }
      }
      if (!mainForm.zoomed)
      {
        return;
      }

      sceneGraph.PrinterColorAlpha = printerZoomedAlpha;
    }

    private void SetModelColor(string color)
    {
      modelColorString = color;
      if (color.Equals("Automatic"))
      {
        var filamentSpool = (FilamentSpool) null;
        var selectedPrinter = (IPrinter)spooler_connection.SelectedPrinter;
        if (selectedPrinter != null)
        {
          filamentSpool = selectedPrinter.GetCurrentFilament();
        }

        if (filamentSpool != (FilamentSpool) null)
        {
          var num = (int) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), filamentSpool.filament_color_code);
          color = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) num);
          Color4 color4;
          FilamentConstants.HexToRGB(FilamentConstants.generateHEXFromColor((FilamentConstants.ColorsEnum) num), out color4.R, out color4.G, out color4.B, out color4.A);
          if (!(color4 != sceneGraph.ModelColor))
          {
            return;
          }

          sceneGraph.ModelColor = color4;
        }
        else
        {
          sceneGraph.ModelColor = Colors.GetColor("LightBlue");
        }
      }
      else
      {
        try
        {
          sceneGraph.ModelColor = Colors.GetColor(color);
        }
        catch (ArgumentException ex)
        {
          if (ex.Message == "Not a valid color")
          {
            sceneGraph.ModelColor = Colors.GetColor("Black");
          }
        }
      }
      modelColorString = color;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (base.OnKeyboardEvent(keyboardevent))
      {
        return true;
      }

      if (keyboardevent.type == KeyboardEventType.InputKey)
      {
        var inputKeyEvent = (InputKeyEvent) keyboardevent;
        if (inputKeyEvent.Ctrl)
        {
          if (inputKeyEvent.Ch == '\x001A')
          {
            history.Undo();
          }
          else if (inputKeyEvent.Ch == '\x0019')
          {
            history.Redo();
          }
        }
      }
      else if (keyboardevent.type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Delete)
      {
        RemoveSelectedModel();
      }

      return false;
    }

    public void RemoveSelectedModel()
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      var selectedModelIndex = selected_model_index;
      History.PushRemoveModelFile(modelTransformPair.data.ID, modelTransformPair.modelNode.fileName, modelTransformPair.modelNode.zipFileName, modelTransformPair.transformNode.TransformData);
      var recordHistory = History.RecordHistory;
      History.RecordHistory = false;
      UnselectModel();
      RemoveModel(selectedModelIndex);
      sceneGraph.ResetExceedBounds();
      var index = selectedModelIndex - 1;
      if (index < 0)
      {
        index = sceneGraph.ModelCount - 1;
      }

      SelectModel(index);
      History.RecordHistory = recordHistory;
    }

    public void DuplicateSelection()
    {
      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair == null)
      {
        return;
      }

      model_loading_manager.LoadModelIntoPrinter(modelTransformPair.modelNode.fileName);
    }

    public override void OnMouseLeave()
    {
      dragging = false;
      base.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (!Enabled)
      {
        return false;
      }

      OpenTK.Vector2 vector2;
      vector2.X = (float) mouseevent.pos.x;
      vector2.Y = (float) mouseevent.pos.y;
      MouseButton button = mouseevent.button;
      if (mouseevent.type == MouseEventType.Down)
      {
        if (button == MouseButton.Left)
        {
          if (!animating && viewstate == ViewState.Hidden)
          {
            if (mainForm != null)
            {
              mainForm.SetToEditView();
            }
          }
          else
          {
            SelectModel(sceneGraph.UnProjectMouseCoordinates(vector2.X, vector2.Y, sceneGraph.CreateViewMatrix()));
            m_gui_host.SetFocus(ID);
          }
        }
        dragging = true;
        vector2.X = (float) (int) vector2.X;
        vector2.Y = (float) (int) vector2.Y;
        prevX = (int) vector2.X;
        prevY = (int) vector2.Y;
      }
      else if (mouseevent.type == MouseEventType.Up)
      {
        dragging = false;
      }
      else if (mouseevent.type == MouseEventType.Move)
      {
        if (dragging)
        {
          switch (button)
          {
            case MouseButton.Left:
              rotVelocity = (float) (((double) vector2.X - (double)prevX) / 3.0);
              var tilt = (float) (((double) vector2.Y - (double)prevY) / 3.0);
              if ((double) tilt < 1.0 && (double) tilt > -1.0)
              {
                tilt = 0.0f;
              }

              sceneGraph.TiltPrinter(tilt);
              break;
            case MouseButton.Right:
              if (!animating && selected_model_index >= 0 && ((double)prevX != (double) vector2.X || (double)prevY != (double) vector2.Y))
              {
                if (AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Rotation)
                {
                  AdjustmentsDialog.AdjustRotationValues(0.0f, 0.0f, (float) ((int) vector2.X - prevX));
                }

                if (AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Scale)
                {
                  AdjustmentsDialog.CheckLinkedRescalingCheckbox(true);
                  var num = (float) (((double) vector2.Y - (double)prevY) / 100.0);
                  AdjustmentsDialog.AdjustScaleValues(num, num, num);
                }
                if (AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Translation)
                {
                  var num = 0.5f;
                  M3D.Model.Utils.Vector3 vector3 = xAxisVector * ((vector2.X - (float)prevX) * num) + yAxisVector * ((vector2.Y - (float)prevY) * num);
                  AdjustmentsDialog.AdjustTranslationValues(vector3.x, vector3.y, 0.0f);
                  break;
                }
                break;
              }
              break;
          }
          prevX = (int) vector2.X;
          prevY = (int) vector2.Y;
        }
      }
      else if (mouseevent.type == MouseEventType.MouseWheel && mouseevent.delta != 0)
      {
        if (mouseevent.delta < 0)
        {
          if (!animating && mainForm != null)
          {
            mainForm.SetToEditView();
          }
        }
        else if (!animating && mainForm != null)
        {
          mainForm.SetToLibraryView();
        }
      }
      return true;
    }

    private void UpdatePrinterFamily()
    {
      if (sceneGraph.PrinterModelCaseType == PrinterSizeProfile.CaseType.Micro1Case)
      {
        CurrentSizeProfile = (PrinterSizeProfile) new Micro1PrinterSizeProfile();
        CurrentFamilyName = PrinterObject.GetFamilyFromProfile("Micro");
      }
      else if (sceneGraph.PrinterModelCaseType == PrinterSizeProfile.CaseType.ProCase)
      {
        CurrentSizeProfile = (PrinterSizeProfile) new ProPrinterSizeProfile();
        CurrentFamilyName = PrinterObject.GetFamilyFromProfile("Pro");
      }
      if (SelectedModelTransformPair != null)
      {
        SetModelRanges(SelectedModelTransformPair, CurrentSizeProfile);
      }

      sceneGraph.SizeFromPrinterProfile(CurrentSizeProfile);
    }

    public void SetControlDialogVisibility(bool visible)
    {
      if (toolbar != null)
      {
        toolbar.Visible = visible && viewstate == ViewState.Active;
      }

      controlsHidden = !visible;
    }

    public void SetControlStateMaster(bool bShouldDisable)
    {
      SetControlDialogVisibility(!bShouldDisable);
      if (BottomControlBar != null)
      {
        BottomControlBar.SetControlStateMaster(bShouldDisable);
      }

      RightMouseButtonControlEnabled = !bShouldDisable;
      _autoPrint = bShouldDisable;
    }

    public void ResetControlState()
    {
      SetControlStateMaster(false);
    }

    public override void OnUpdate()
    {
      Process();
      base.OnUpdate();
    }

    public void Process()
    {
      ProcessEditView();
      CheckUpdatedColors();
      CheckUpdatedMeasumentUnits();
      CheckUpdatedCaseType();
      if (sceneGraph.ViewPointPos != targetViewPointPos && targetViewPointPos != (M3D.Model.Utils.Vector3) null || (double)RelativeWidth != (double)targetWidth)
      {
        if (viewstate == ViewState.ToHidden)
        {
          ShowView(false);
          viewstate = ViewState.Hidden;
        }
        animating = true;
        elapsed = DateTime.Now.Ticks / 10000L - startTime;
        if (elapsed > animationTime)
        {
          elapsed = animationTime;
        }

        ++frameCount;
        fps = 1000f / (float) (elapsed / (long)frameCount);
        var num1 = movementMultiplier * 60f / fps;
        if (sceneGraph.ViewPointPos != targetViewPointPos)
        {
          vectorToTargetViewPointPos = targetViewPointPos - originalViewPointPos;
          var num2 = (sceneGraph.ViewPointPos - originalViewPointPos).Length() / (targetViewPointPos - originalViewPointPos).Length();
          if ((double) num2 <= 0.509999990463257)
          {
            mCameraVelocity += vectorToTargetViewPointPos * num1;
          }
          else
          {
            mCameraVelocity -= vectorToTargetViewPointPos * num1;
            if ((double)mCameraVelocity.Dot(vectorToTargetViewPointPos) < 0.0)
            {
              num2 = 1f;
            }
          }
          if ((double) num2 >= 1.0)
          {
            sceneGraph.ViewPointPos = targetViewPointPos;
            mCameraVelocity = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
            mainForm.zoomed = !mainForm.zoomed;
            if (mainForm.zoomed)
            {
              sceneGraph.PrinterColorAlpha = printerZoomedAlpha;
              sceneGraph.GridVisible = true;
            }
            else
            {
              sceneGraph.PrinterColorAlpha = 1f;
              sceneGraph.GridVisible = false;
            }
            startTime = DateTime.Now.Ticks / 10000L;
          }
          else
          {
            sceneGraph.ViewPointPos = sceneGraph.ViewPointPos + mCameraVelocity;
          }
        }
        if ((double)RelativeWidth == (double)targetWidth)
        {
          return;
        }

        var num3 = targetWidth - originalWidth;
        var num4 = Math.Abs(RelativeWidth - originalWidth) / Math.Abs(num3);
        if ((double) num4 <= 0.509999990463257)
        {
          mWidthVelocity += num3 * num1;
        }
        else
        {
          mWidthVelocity -= num3 * num1;
          if (Math.Sign(mWidthVelocity) != Math.Sign(num3))
          {
            num4 = 1f;
          }
        }
        if ((double) num4 >= 1.0)
        {
          RelativeWidth = targetWidth;
          mWidthVelocity = 0.0f;
        }
        else
        {
          RelativeWidth += mWidthVelocity;
        }
      }
      else if (viewstate == ViewState.ToActive)
      {
        ShowView(true);
        elapsed = DateTime.Now.Ticks / 10000L - startTime;
        realanimtiontime = 400;
        if (elapsed >= (long)realanimtiontime)
        {
          elapsed = (long)realanimtiontime;
          viewstate = ViewState.Active;
          EditFrame.RelativeWidth = 1f;
        }
        var num1 = (float)elapsed / (float)realanimtiontime;
        var num2 = (int) ((double)mainForm.glControl1.Height * 0.174999997019768);
        var num3 = Height + 10;
        var num4 = Height - 121;
        AdjustmentsDialog.Y = (int) ((double) (num2 - -291) * (double) num1) - 291;
        BottomControlBar.Y = (int) ((double) (num4 - num3) * (double) num1) + num3;
        EditFrame.RelativeWidth = (float) (1.5 - (double) num1 * 0.5);
      }
      else
      {
        animating = false;
        if ((double)rotVelocity == 0.0)
        {
          return;
        }

        sceneGraph.RotatePrinter(rotVelocity);
        var num = (float) Math.PI / 180f;
        xAxisVector.RotateVector(-rotVelocity * num, false, false, true);
        yAxisVector.RotateVector(-rotVelocity * num, false, false, true);
        rotateVelocityFriction = Math.Abs(rotVelocity) / 5f;
        if ((double)rotateVelocityFriction < (double)minRotateVelocityFriction)
        {
          rotateVelocityFriction = minRotateVelocityFriction;
        }

        if ((double)rotVelocity > 0.0)
        {
          rotVelocity -= rotateVelocityFriction;
          if ((double)rotVelocity >= 0.0)
          {
            return;
          }

          rotVelocity = 0.0f;
        }
        else
        {
          if ((double)rotVelocity >= 0.0)
          {
            return;
          }

          rotVelocity += rotateVelocityFriction;
          if ((double)rotVelocity <= 0.0)
          {
            return;
          }

          rotVelocity = 0.0f;
        }
      }
    }

    private void ProcessEditView()
    {
      if (model_loading_manager != null)
      {
        BottomControlBar.EnableButtons(true, ModelLoaded);
      }

      if (!ObjectTransformed)
      {
        return;
      }

      ModelTransformPair modelTransformPair = SelectedModelTransformPair;
      if (modelTransformPair != null)
      {
        TransformationNode.Transform transformData1 = modelTransformPair.transformNode.TransformData;
        AdjustmentsDialog.SaveCurrentSliderInfo();
        TransformationNode.Transform transformData2 = modelTransformPair.transformNode.TransformData;
        history.PushTransformObject(modelTransformPair.data.ID, transformData1, transformData2);
        sceneGraph.PlaceObjectOnFloorAndCheckBounds(selected_model_index);
      }
      ObjectTransformed = false;
    }

    public void SetPosition(float x, float y)
    {
      RelativeX = x;
      RelativeY = y;
      OnSetPosition();
    }

    public void SetSize(float width, float height)
    {
      RelativeWidth = width;
      RelativeHeight = height;
    }

    public void SetViewPointPos(float x, float y, float z)
    {
      sceneGraph.ViewPointPos = new M3D.Model.Utils.Vector3(x, y, z);
    }

    public void SetTargetViewPointPos(M3D.Model.Utils.Vector3 vector)
    {
      SetTargetViewPointPos(vector.x, vector.y, vector.z);
    }

    public void SetTargetViewPointPos(float x, float y, float z)
    {
      originalViewPointPos = sceneGraph.ViewPointPos;
      targetViewPointPos = new M3D.Model.Utils.Vector3(x, y, z);
      startTime = DateTime.Now.Ticks / 10000L;
      movementMultiplier = 0.01f;
      frameCount = 0;
    }

    public void SetTargetSize(float width, float height)
    {
      originalWidth = RelativeWidth;
      originalHeight = RelativeHeight;
      targetWidth = width;
      targetHeight = height;
      startTime = DateTime.Now.Ticks / 10000L;
      movementMultiplier = 0.01f;
      frameCount = 0;
    }

    public override void OnParentResize()
    {
      OnSetPosition();
      base.OnParentResize();
    }

    public void OnSetPosition()
    {
      if (viewstate != ViewState.Active)
      {
        return;
      }

      if (AdjustmentsDialog != null)
      {
        AdjustmentsDialog.Y = (int) ((double)mainForm.glControl1.Height * 0.174999997019768);
      }

      if (BottomControlBar == null)
      {
        return;
      }

      BottomControlBar.Y = Height - 121;
    }

    public bool TransitionViewState(ViewState new_state)
    {
      if (new_state == viewstate || new_state == ViewState.ToActive || new_state == ViewState.ToHidden)
      {
        return false;
      }

      var num1 = 0.0f;
      var num2 = -0.5f;
      if (new_state == ViewState.Active)
      {
        viewstate = ViewState.ToActive;
        target_y = num1;
        realanimtiontime = 1500;
        SetToEditView();
      }
      else
      {
        viewstate = ViewState.ToHidden;
        target_y = num2;
        realanimtiontime = 300;
        SetToLibraryView();
      }
      startTime = DateTime.Now.Ticks / 10000L;
      return true;
    }

    public void GotoLibraryView()
    {
      if (mainForm == null)
      {
        return;
      }

      mainForm.SetToLibraryView();
    }

    private bool SetToEditView()
    {
      if (!(targetViewPointPos == (M3D.Model.Utils.Vector3) null) && !(targetViewPointPos != PrinterView.editViewPoint))
      {
        return false;
      }

      SetTargetSize(PrinterView.editSize.X, PrinterView.editSize.Y);
      SetTargetViewPointPos(PrinterView.editViewPoint);
      return true;
    }

    private bool SetToLibraryView()
    {
      if (!(targetViewPointPos == (M3D.Model.Utils.Vector3) null) && !(targetViewPointPos != PrinterView.libraryViewPoint))
      {
        return false;
      }

      SetTargetSize(PrinterView.librarySize.X, PrinterView.librarySize.Y);
      SetTargetViewPointPos(PrinterView.libraryViewPoint);
      return true;
    }

    public override void Refresh()
    {
      base.Refresh();
      if (AdjustmentsDialog == null)
      {
        return;
      }

      AdjustmentsDialog.Refresh();
    }

    private void ShowView(bool show)
    {
      if (EditFrame.Visible == show)
      {
        return;
      }

      BottomControlBar.Visible = show;
      toolbar.Visible = show && !controlsHidden;
      EditFrame.Visible = show;
      if (!show)
      {
        return;
      }

      Refresh();
    }

    public void OnWindowResize(int new_width, int new_height)
    {
      OnSetPosition();
    }

    public void Print()
    {
      PrinterObject selectedPrinter = spooler_connection.SelectedPrinter;
      if (selectedPrinter == null)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoPrinter"));
      }
      else if (selectedPrinter.Info.current_job != null)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_AlreadyPrinting"));
      }
      else if (selectedPrinter.PrinterState == PrinterObject.State.IsCalibrating)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_Calibrating"));
      }
      else if (!ModelLoaded)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoModel"));
      }
      else if (!selectedPrinter.Info.extruder.Z_Valid || !selectedPrinter.Info.calibration.Calibration_Valid)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NotCalibrated"));
      }
      else if (selectedPrinter.GetCurrentFilament() == (FilamentSpool) null)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_No3DInk"));
      }
      else if (sceneGraph.ObjectExceedsBounds)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_OutofBounds"));
      }
      else
      {
        PrepareForPrinting(selectedPrinter);
      }
    }

    internal PrintJobDetails CreatePrintJobDetails(out bool modelZTooSmall)
    {
      var printJobDetails = new PrintJobDetails();
      modelZTooSmall = false;
      var total1 = new M3D.Model.Utils.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      var total2 = new M3D.Model.Utils.Vector3(float.MinValue, float.MinValue, float.MinValue);
      for (var index = 0; index < sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = sceneGraph.GetModel(index);
        Model3DNode modelNode = model.modelNode;
        TransformationNode transformNode = model.transformNode;
        modelZTooSmall |= (double) model.modelSize.Ext.z < 0.400000005960464;
        UpdateMin(ref total1, model.modelSize.Min);
        UpdateMax(ref total2, model.modelSize.Max);
        printJobDetails.slicer_objects.Add(new ModelTransform(modelNode.ModelData, transformNode.GetTransformationMatrix()));
        printJobDetails.objectDetailsList.Add(new PrintDetails.ObjectDetails(modelNode.fileName, new PrintDetails.Transform(transformNode.Translation, transformNode.Scale, transformNode.Rotation)));
        libraryview.RecentModels.GenerateIconForLibrary(model);
      }
      var icon_file = Path.Combine(Paths.WorkingFolder, "previewimage.jpg");
      var center = new M3D.Model.Utils.Vector3((float) (((double) total1.x + (double) total2.x) / 2.0), (float) (((double) total1.z + (double) total2.z) / 2.0), (float) (((double) total1.y + (double) total2.y) / 2.0));
      ImageCapture.GenerateMultiModelPreview(sceneGraph.GetAllModels(), icon_file, new OpenTK.Vector2(400f, 400f), new Color4(0.8431373f, 0.8901961f, 0.9921569f, 1f), OpenGLConnection.GLControl1, center);
      printJobDetails.preview_image = icon_file;
      return printJobDetails;
    }

    private void PrepareForPrinting(PrinterObject printer)
    {
      PrintJobDetails printJobDetails = CreatePrintJobDetails(out var modelZTooSmall);
      printJobDetails.printer = printer;
      if (modelZTooSmall)
      {
        messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_WARNING_ModelToSmall"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(DoPrintCallback), (object) new PrinterView.PrintDialogDetails(printJobDetails, printer));
      }
      else
      {
        ShowPrintDialog(printJobDetails, printer);
      }
    }

    private void UpdateMin(ref M3D.Model.Utils.Vector3 total, M3D.Model.Utils.Vector3 min)
    {
      if ((double) min.x < (double) total.x)
      {
        total.x = min.x;
      }

      if ((double) min.y < (double) total.y)
      {
        total.y = min.y;
      }

      if ((double) min.z >= (double) total.z)
      {
        return;
      }

      total.z = min.z;
    }

    private void UpdateMax(ref M3D.Model.Utils.Vector3 total, M3D.Model.Utils.Vector3 max)
    {
      if ((double) max.x > (double) total.x)
      {
        total.x = max.x;
      }

      if ((double) max.y > (double) total.y)
      {
        total.y = max.y;
      }

      if ((double) max.z <= (double) total.z)
      {
        return;
      }

      total.z = max.z;
    }

    private void ShowPrintDialog(PrintJobDetails details, PrinterObject printer)
    {
      if (_autoPrint)
      {
        details.printer = printer;
        details.jobOptions = jobOptionsFromXML;
        details.GenerateSlicerSettings(printer, this);
        details.settings.transformation = GetObjectSlicerTransformation();
        details.autoPrint = true;
        printerdialog.Show(PrintDialogWidgetFrames.PreSlicingFrame, details);
      }
      else
      {
        printerdialog.Show(PrintDialogWidgetFrames.PrintDialogFrame, details);
      }
    }

    private void DoPrintCallback(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
      {
        return;
      }

      var printDialogDetails = (PrinterView.PrintDialogDetails) user_data;
      ShowPrintDialog(printDialogDetails.details, printDialogDetails.printer);
    }

    public void ResetTransformationValues()
    {
      AdjustmentsDialog.SetTranslationValues(0.0f, 0.0f);
      AdjustmentsDialog.SetRotationValues(0.0f, 0.0f, 0.0f);
      var num = (double)MaxScale.x < (double)MaxScale.y ? ((double)MaxScale.z < (double)MaxScale.x ? MaxScale.z : MaxScale.x) : MaxScale.y;
      if ((double) num < 1.0)
      {
        AdjustmentsDialog.SetScaleValues(num, num, num);
      }
      else
      {
        AdjustmentsDialog.SetScaleValues(1f, 1f, 1f);
      }
    }

    public M3D.Model.Utils.Vector3 GetTranslationValues()
    {
      AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = ModelTransformation;
      if (modelTransformation != null)
      {
        return modelTransformation.Translation;
      }

      return new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public M3D.Model.Utils.Vector3 GetScaleValues()
    {
      AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = ModelTransformation;
      if (modelTransformation != null)
      {
        return modelTransformation.Scale;
      }

      return new M3D.Model.Utils.Vector3(1f, 1f, 1f);
    }

    public M3D.Model.Utils.Vector3 GetRotationValues()
    {
      AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = ModelTransformation;
      if (modelTransformation != null)
      {
        return modelTransformation.Rotation;
      }

      return new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    private void SelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      SerialNumber = serial_number;
      var serial_number1 = SerialNumber.ToString();
      if (printerColorString == "Automatic")
      {
        SetPrinterColor("Automatic");
      }

      if (serial_number == PrinterSerialNumber.Undefined)
      {
        sceneGraph.PrinterModelCaseType = settings_manager.CurrentAppearanceSettings.CaseType;
        UpdatePrinterFamily();
      }
      else
      {
        PrinterObject printerBySerialNumber = spooler_connection.GetPrinterBySerialNumber(serial_number1);
        if (printerBySerialNumber != null)
        {
          CurrentSizeProfile = printerBySerialNumber.MyPrinterProfile.PrinterSizeConstants;
          sceneGraph.SizeFromPrinterProfile(CurrentSizeProfile);
          if (SelectedModelTransformPair != null)
          {
            SetModelRanges(SelectedModelTransformPair, CurrentSizeProfile);
          }

          CurrentFamilyName = PrinterObject.GetFamilyFromProfile(printerBySerialNumber.MyPrinterProfile.ProfileName);
          slicer_connection.SlicerSettingStack.SetCurrentSettingsFromPrinterProfile((IPrinter) printerBySerialNumber);
          if (!((FilamentSpool) null == printerBySerialNumber.GetCurrentFilament()) || !printerdialog.Visible)
          {
            return;
          }

          printerdialog.CloseWindow();
          infobox.AddMessageToQueue("The print dialog was closed because the printer doesn't have 3D Ink.");
        }
        else
        {
          if (!printerdialog.Visible)
          {
            return;
          }

          printerdialog.CloseWindow();
          infobox.AddMessageToQueue("The print dialog was closed because the printer disconnected.");
        }
      }
    }

    public void SetModelRanges(ModelTransformPair selectedmodel, PrinterSizeProfile sizeProfile)
    {
      var max = new M3D.Model.Utils.Vector3(sizeProfile.shell_size.x / selectedmodel.OriginalModelSize.Ext.x, sizeProfile.shell_size.y / selectedmodel.OriginalModelSize.Ext.y, sizeProfile.shell_size.z / selectedmodel.OriginalModelSize.Ext.z);
      SetScaleRange(new M3D.Model.Utils.Vector3(0.1f / selectedmodel.OriginalModelSize.Ext.x, 0.1f / selectedmodel.OriginalModelSize.Ext.y, 0.1f / selectedmodel.OriginalModelSize.Ext.z), max);
      AdjustmentsDialog.RefreshSliders();
      ObjectTransformed = true;
    }

    public void SetScaleRange(M3D.Model.Utils.Vector3 min, M3D.Model.Utils.Vector3 max)
    {
      min_scale = min;
      max_scale = max;
    }

    public enum ElementIDs
    {
      ControlDialogFrame = 8001, // 0x00001F41
      EditViewControlBar = 9001, // 0x00002329
      ToolBar = 10001, // 0x00002711
      PrintDialog = 11001, // 0x00002AF9
    }

    public enum Units
    {
      Microns,
      Inches,
      MMs,
    }

    public delegate void ModelListChanged(List<ModelTransformPair.Data> modelNames);

    private struct PrintDialogDetails
    {
      public PrintJobDetails details;
      public PrinterObject printer;

      public PrintDialogDetails(PrintJobDetails details, PrinterObject printer)
      {
        this.details = details;
        this.printer = printer;
      }
    }
  }
}
