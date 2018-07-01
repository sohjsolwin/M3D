// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.PrinterView
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
        return this.SelectedModelTransformPair?.transformNode;
      }
    }

    public ModelTransformPair SelectedModelTransformPair
    {
      get
      {
        if (this.selected_model_index >= 0 && this.sceneGraph.ModelCount > 0 && this.selected_model_index < this.sceneGraph.ModelCount)
          return this.sceneGraph.GetModel(this.selected_model_index);
        return (ModelTransformPair) null;
      }
    }

    public bool ModelLoaded
    {
      get
      {
        return this.sceneGraph.ModelCount > 0;
      }
    }

    public string LoadedModelFileName
    {
      get
      {
        return this.SelectedModelTransformPair?.modelNode.fileName;
      }
    }

    public PrinterViewHistory History
    {
      get
      {
        return this.history;
      }
    }

    public ModelLoadingManager ModelLoadingInterface
    {
      get
      {
        return this.model_loading_manager;
      }
    }

    public ViewState ViewState
    {
      get
      {
        return this.viewstate;
      }
    }

    public M3D.Model.Utils.Vector3 MinScale
    {
      get
      {
        return this.min_scale;
      }
    }

    public M3D.Model.Utils.Vector3 MaxScale
    {
      get
      {
        return this.max_scale;
      }
    }

    public OpenGLConnection OpenGLConnection
    {
      get
      {
        return this.openGLConnection;
      }
    }

    public Frame GetEditFrame()
    {
      return this.EditFrame;
    }

    public bool AutoPrint
    {
      get
      {
        return this._autoPrint;
      }
    }

    public bool RightMouseButtonControlEnabled
    {
      get
      {
        return this.m_bAllowRightMouseButtonControls;
      }
      set
      {
        this.m_bAllowRightMouseButtonControls = value;
      }
    }

    public string CurrentFamilyName { get; private set; }

    public PrinterSizeProfile CurrentSizeProfile { get; private set; }

    public PrinterView(Form1 mainForm, GUIHost host, OpenGLConnection openGLConnection, SpoolerConnection spooler_connection, SlicerConnectionBase slicer_connection, ModelLoadingManager model_loading_manager, PopupMessageBox messagebox, MessagePopUp infobox, SettingsManager settings_manager, LibraryView libraryview)
      : base(123456)
    {
      this.bUpdateWhenNotVisible = true;
      this.openGLConnection = openGLConnection;
      this.history = new PrinterViewHistory(this);
      this.history.RecordHistory = false;
      this.RelativeX = 0.0f;
      this.RelativeY = 0.0f;
      this.RelativeWidth = PrinterView.librarySize.X;
      this.RelativeHeight = PrinterView.librarySize.Y;
      this.targetWidth = PrinterView.librarySize.X;
      this.targetHeight = PrinterView.librarySize.Y;
      this.mainForm = mainForm;
      this.mCameraVelocity = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      this.m_gui_host = host;
      this.slicer_connection = slicer_connection;
      this.spooler_connection = spooler_connection;
      this.model_loading_manager = model_loading_manager;
      this.settings_manager = settings_manager;
      this.messagebox = messagebox;
      this.infobox = infobox;
      this.libraryview = libraryview;
      this.CreatePrinterViewObjects();
      this.CreateEditViewObjects();
      this.viewstate = ViewState.Hidden;
      spooler_connection.OnSelectedPrinterChanged += new SpoolerConnection.SelectedPrinterChangedCallback(this.SelectedPrinterChanged);
      this.history.RecordHistory = true;
    }

    public override ElementType GetElementType()
    {
      return ElementType.Element;
    }

    private void CreatePrinterViewObjects()
    {
      this.CurrentGridUnits = this.settings_manager.CurrentAppearanceSettings.Units;
      this.sceneGraph = new PrinterViewSceneGraph(this.m_gui_host, this.CurrentGridUnits, this.settings_manager.CurrentAppearanceSettings.CaseType);
      this.AddChildElement((Element2D) this.sceneGraph);
      this.xAxisVector = new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f);
      this.yAxisVector = new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f);
      this.SetPrinterColor(this.settings_manager.CurrentAppearanceSettings.PrinterColor);
      this.SetModelColor(this.settings_manager.CurrentAppearanceSettings.ModelColor);
      this.UpdatePrinterFamily();
    }

    private void CreateEditViewObjects()
    {
      Sprite.texture_height_pixels = 512;
      Sprite.texture_width_pixels = 1024;
      Sprite.pixel_perfect = true;
      this.m_gui_host.SetFontProperty(FontSize.VeryLarge, 20f);
      this.m_gui_host.SetFontProperty(FontSize.Large, 14f);
      this.m_gui_host.SetFontProperty(FontSize.Medium, 11f);
      this.m_gui_host.SetFontProperty(FontSize.Small, 8f);
      this.WrapOnNegative = false;
      this.EditFrame = new Frame(0, (Element2D) this);
      this.EditFrame.SetPosition(0, 0);
      this.EditFrame.RelativeWidth = 2f;
      this.EditFrame.RelativeHeight = 1f;
      this.AddChildElement((Element2D) this.EditFrame);
      this.AdjustmentsDialog = new ModelAdjustmentsDialog(8001, this);
      this.AdjustmentsDialog.Init(this.m_gui_host);
      this.AdjustmentsDialog.Width = 339;
      this.AdjustmentsDialog.Height = 182;
      this.AdjustmentsDialog.X = 96;
      this.AdjustmentsDialog.Visible = false;
      this.AddChildElement((Element2D) this.AdjustmentsDialog);
      this.BottomControlBar = new EditViewControlBar(9001, this);
      this.BottomControlBar.Init(this.m_gui_host);
      this.AddChildElement((Element2D) this.BottomControlBar);
      this.toolbar = new ToolBar(10001, this, this.libraryview, this.AdjustmentsDialog, this.slicer_connection);
      this.toolbar.Init(this.m_gui_host);
      this.toolbar.SetPosition(0, 200);
      this.toolbar.CenterVerticallyInParent = true;
      this.AddChildElement((Element2D) this.toolbar);
      this.printerdialog = new PrintDialogMainWindow(11001, this.m_gui_host, this, this.spooler_connection, this.messagebox, this.model_loading_manager, this.slicer_connection, this.libraryview.RecentPrints, this.settings_manager);
      this.printerdialog.Visible = false;
      this.m_gui_host.Refresh();
      this.viewstate = ViewState.Hidden;
      this.OnSetPosition();
      this.ShowView(false);
    }

    private void DoModelListChanged()
    {
      if (this.OnModelListChanged == null)
        return;
      this.OnModelListChanged(this.sceneGraph.GetModelDataList());
    }

    private string GetSimplifiedName(string filepath)
    {
      string name = Path.GetFileNameWithoutExtension(filepath);
      if (name.Length > 24)
        name = name.Substring(0, 24);
      return this.CheckDuplicateNames(name);
    }

    private string CheckDuplicateNames(string name)
    {
      int num = -1;
      string str1 = "_";
      foreach (ModelTransformPair.Data modelData in this.sceneGraph.GetModelDataList())
      {
        if (modelData.name.StartsWith(name))
        {
          if (modelData.name == name)
          {
            num = 1;
          }
          else
          {
            string str2 = modelData.name.Substring(name.Length);
            if (str2.StartsWith(str1))
            {
              string s = str2.Substring(1);
              int result = -1;
              if (int.TryParse(s, out result))
                num = result + 1;
            }
          }
        }
      }
      if (num > 0)
        name = name + str1 + num.ToString("D2");
      return name;
    }

    private uint GetNextModelIndex()
    {
      return ++this.lastmodelID;
    }

    public void SelectModel(Ray pickRay)
    {
      int index1 = -1;
      M3D.Model.Utils.Vector3 vector3_1 = (M3D.Model.Utils.Vector3) null;
      for (int index2 = 0; index2 < this.sceneGraph.ModelCount; ++index2)
      {
        ModelTransformPair model = this.sceneGraph.GetModel(index2);
        M3D.Graphics.Utils.BoundingBox boundingBox = new M3D.Graphics.Utils.BoundingBox(model.modelSize.Min, model.modelSize.Max);
        M3D.Model.Utils.Vector3 vector3_2 = new M3D.Model.Utils.Vector3(pickRay.Position);
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
            M3D.Model.Utils.Vector3 vector3_5 = new M3D.Model.Utils.Vector3(local.x, local.y, local.z);
            if ((double) vector3_5.Distance(pickRay.Position) < (double) vector3_1.Distance(pickRay.Position))
            {
              index1 = index2;
              vector3_1 = vector3_5;
            }
          }
        }
      }
      if (index1 < 0)
        return;
      this.SelectModel(index1);
    }

    public void SelectModel(int index)
    {
      uint previousObject = uint.MaxValue;
      uint objectID = uint.MaxValue;
      ModelTransformPair modelTransformPair1 = this.SelectedModelTransformPair;
      if (modelTransformPair1 != null)
      {
        previousObject = modelTransformPair1.data.ID;
        this.UnselectModel();
      }
      if (index >= 0 && index < this.sceneGraph.ModelCount)
      {
        this.selected_model_index = index;
        ModelTransformPair modelTransformPair2 = this.SelectedModelTransformPair;
        objectID = modelTransformPair2.data.ID;
        this.SetModelRanges(modelTransformPair2, this.CurrentSizeProfile);
        if (this.sceneGraph.ModelCount > 1)
          modelTransformPair2.modelNode.Highlight = true;
      }
      this.History.PushSelectObject(objectID, previousObject);
    }

    public void PushUpdatedInfomation(ModelTransformPair model)
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null || (int) model.data.ID != (int) modelTransformPair.data.ID)
        return;
      this.AdjustmentsDialog.PushUpdatedInfomation(model);
    }

    public ModelTransformPair GetModelByID(uint id)
    {
      for (int index = 0; index < this.sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = this.sceneGraph.GetModel(index);
        if ((int) model.data.ID == (int) id)
          return model;
      }
      return (ModelTransformPair) null;
    }

    public bool SelectModelbyID(uint id)
    {
      for (int index = 0; index < this.sceneGraph.ModelCount; ++index)
      {
        if ((int) this.sceneGraph.GetModel(index).data.ID == (int) id)
        {
          this.SelectModel(index);
          return true;
        }
      }
      return false;
    }

    public void UnselectModel()
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair != null)
      {
        this.AdjustmentsDialog.Refresh();
        modelTransformPair.modelNode.Highlight = false;
      }
      this.selected_model_index = -1;
    }

    public Matrix4x4 GetObjectSlicerTransformation()
    {
      Matrix4x4 matrix4x4 = new Matrix4x4();
      matrix4x4.Identity();
      M3D.Model.Utils.Vector3 vector3_1 = new M3D.Model.Utils.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      M3D.Model.Utils.Vector3 vector3_2 = new M3D.Model.Utils.Vector3(float.MinValue, float.MinValue, float.MinValue);
      for (int index = 0; index < this.sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = this.sceneGraph.GetModel(index);
        model.CalculateExtents();
        if ((double) model.modelSize.Min.x < (double) vector3_1.x)
          vector3_1.x = model.modelSize.Min.x;
        if ((double) model.modelSize.Min.y < (double) vector3_1.y)
          vector3_1.y = model.modelSize.Min.y;
        if ((double) model.modelSize.Max.x > (double) vector3_2.x)
          vector3_2.x = model.modelSize.Max.x;
        if ((double) model.modelSize.Max.y > (double) vector3_2.y)
          vector3_2.y = model.modelSize.Max.y;
      }
      matrix4x4.m[3, 0] = (float) (((double) vector3_1.x + (double) vector3_2.x) / 2.0);
      matrix4x4.m[3, 1] = (float) (((double) vector3_1.y + (double) vector3_2.y) / 2.0);
      return matrix4x4;
    }

    public bool IsModelLoaded()
    {
      return this.sceneGraph.ModelCount > 0;
    }

    public void RemovePrintableModels()
    {
      this.History.RecordHistory = false;
      this.toolbar.DeactivateModelAdjustDialog();
      this.UnselectModel();
      while (this.sceneGraph.ModelCount > 0)
        this.RemoveModel(0);
      this.selected_model_index = -1;
      this.SetControlStateMaster(false);
      this.sceneGraph.ResetExceedBounds();
      this.History.Clear();
      this.History.RecordHistory = true;
    }

    public void ResetObjectMatrices()
    {
      this.CenterPrinterObject();
    }

    public int AddModel(M3D.Graphics.Ext3D.ModelRendering.Model model, PrintDetails.ObjectDetails objectDetails)
    {
      bool recordHistory = this.history.RecordHistory;
      this.history.RecordHistory = false;
      if (this.settings_manager.CurrentAppearanceSettings.AutoDetectModelUnits)
      {
        PrinterView.Units sourceUnits = this.DetectUnits(new ModelSize(model.modelData.Min, model.modelData.Max));
        if (sourceUnits != PrinterView.Units.MMs)
        {
          this.RescaleModel(model, sourceUnits);
          if (sourceUnits == PrinterView.Units.Inches)
            this.infobox.AddMessageToQueue("T_InchesDetected");
          else if (sourceUnits == PrinterView.Units.Microns)
            this.infobox.AddMessageToQueue("T_MicronsDetected");
        }
      }
      ModelSize modelSize = new ModelSize(model.modelData.Min, model.modelData.Max);
      M3D.Model.Utils.Vector3 freePosition = this.sceneGraph.FindFreePosition(new OpenTK.Vector2(modelSize.Ext.x, modelSize.Ext.y));
      ModelTransformPair modtrans_pair;
      int num = this.sceneGraph.AddModel(model, out modtrans_pair);
      modtrans_pair.data.name = this.GetSimplifiedName(model.fileName);
      modtrans_pair.data.ID = objectDetails == null || objectDetails.UID == uint.MaxValue ? this.GetNextModelIndex() : objectDetails.UID;
      this.SelectModel(num);
      this.ResetTransformationValues();
      this.ResetObjectMatrices();
      this.AutoScaleModel();
      this.PlaceModel(freePosition);
      if (objectDetails != null)
        this.SetModelAdjustmentsFromObjectDetails(modtrans_pair, objectDetails);
      this.sceneGraph.PlaceObjectOnFloorAndCheckBounds(num);
      this.AdjustmentsDialog.CheckLinkedRescalingCheckbox(true);
      if (objectDetails == null)
        this.libraryview.RecentModels.GenerateIconForLibrary(modtrans_pair);
      this.DoModelListChanged();
      this.history.RecordHistory = objectDetails == null || objectDetails.UID == uint.MaxValue;
      this.history.PushAddModelFile(modtrans_pair.data.ID, modtrans_pair.modelNode.fileName, modtrans_pair.modelNode.zipFileName, modtrans_pair.transformNode.TransformData);
      this.history.RecordHistory = recordHistory;
      this.mainForm.SetToEditView();
      return num;
    }

    private PrinterView.Units DetectUnits(ModelSize modelSize)
    {
      if ((double) modelSize.Ext.x < 10.0 && (double) modelSize.Ext.y < 10.0 && (double) modelSize.Ext.z < 10.0)
        return PrinterView.Units.Inches;
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
          return;
        num = 25.4f;
      }
      model.geometryData.Scale(num, num, num);
      model.modelData.Scale(new M3D.Model.Utils.Vector3(num, num, num));
    }

    private void AutoScaleModel()
    {
      float num1 = Math.Abs(this.sceneGraph.PrinterBedWidth * this.sceneGraph.PrinterBedLength);
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      M3D.Model.Utils.Vector3 scaleValues1 = this.GetScaleValues();
      M3D.Model.Utils.Vector3 scaleValues2 = this.GetScaleValues();
      float num2 = 1f;
      modelTransformPair.modelNode.CalculateMinMax();
      M3D.Model.Utils.Vector3 ext = modelTransformPair.modelNode.Ext;
      float num3 = Math.Abs((float) ((double) ext.x * (double) scaleValues1.x * ((double) ext.y * (double) scaleValues1.y)));
      if ((double) num3 < 0.01 * (double) num1)
      {
        if ((double) num3 > 1.0)
          num2 = 0.01f * num1 / num3;
        else if ((double) num3 < 1.0)
          num2 = num1 * 0.1f * num3;
      }
      if ((double) num2 > (double) this.MaxScale.x)
        num2 = this.MaxScale.x;
      if ((double) num2 > (double) this.MaxScale.y)
        num2 = this.MaxScale.y;
      if ((double) num2 > (double) this.MaxScale.z)
        num2 = this.MaxScale.z;
      if ((double) num2 < (double) this.MinScale.x)
        num2 = this.MinScale.x;
      if ((double) num2 < (double) this.MinScale.y)
        num2 = this.MinScale.y;
      if ((double) num2 < (double) this.MinScale.z)
        num2 = this.MinScale.z;
      scaleValues2.x *= num2;
      scaleValues2.y *= num2;
      scaleValues2.z *= num2;
      if ((double) scaleValues2.x < (double) scaleValues1.x / 2.0 || (double) scaleValues2.x > (double) scaleValues1.x * 2.0)
        this.messagebox.AddMessageToQueue(new SpoolerMessage(MessageType.UserDefined, Locale.GlobalLocale.T("T_WARNING_LargeScaleUsedMaybeCorrupt")), PopupMessageBox.MessageBoxButtons.OK);
      this.ScalePrinterObject(num2, num2, num2);
    }

    public M3D.Graphics.Ext3D.ModelRendering.Model GetModel(string filename)
    {
      for (int index = 0; index < this.sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = this.sceneGraph.GetModel(index);
        if (model.modelNode.fileName == filename)
          return model.modelNode.GetModelInfo();
      }
      return (M3D.Graphics.Ext3D.ModelRendering.Model) null;
    }

    private void SetModelAdjustmentsFromObjectDetails(ModelTransformPair modtrans_pair, PrintDetails.ObjectDetails objectDetails)
    {
      Model3DNode modelNode = modtrans_pair.modelNode;
      modelNode.zipFileName = objectDetails.zipFileName;
      PrintDetails.PrintJobObjectViewDetails printerview_settings;
      if (!string.IsNullOrEmpty(objectDetails.printerViewXMLFile) && SettingsManager.LoadPrinterViewFile(objectDetails.printerViewXMLFile, out printerview_settings))
      {
        string lower = Path.GetFileNameWithoutExtension(modelNode.fileName).ToLower();
        PrintDetails.ObjectDetails objectDetails1 = (PrintDetails.ObjectDetails) null;
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
            objectDetails1 = printerview_settings.objectList[0];
          objectDetails.transform = objectDetails1.transform;
          objectDetails.hidecontrols = objectDetails1.hidecontrols;
        }
      }
      if (!string.IsNullOrEmpty(objectDetails.printerSettingsXMLFile))
        this.LoadM3DPrintSettings(SettingsManager.LoadPrintJobInfoFile(objectDetails.printerSettingsXMLFile));
      if (objectDetails.transform != null)
      {
        modtrans_pair.transformNode.Translation = objectDetails.transform.translation.GUIVector();
        modtrans_pair.transformNode.Rotation = objectDetails.transform.rotation.GUIVector();
        modtrans_pair.transformNode.Scale = objectDetails.transform.scale.GUIVector();
      }
      this.SetControlStateMaster(objectDetails.hidecontrols);
    }

    public void LoadM3DPrintSettings(PrintDetails.M3DSettings? printsettings)
    {
      if (!printsettings.HasValue)
        return;
      List<Slicer.General.KeyValuePair<string, string>> curaSettings = printsettings.Value.printSettings.CuraSettings;
      if (curaSettings != null && this.slicer_connection.SlicerSettings != null)
        this.slicer_connection.SlicerSettings.LoadFromUserKeyValuePairList(curaSettings);
      else
        this.infobox.AddMessageToQueue("Unable to load print settings because a printer has not been connected.");
      this.jobOptionsFromXML = printsettings.Value.printSettings.options;
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter == null || selectedPrinter.MyPrinterProfile == null)
        return;
      SettingsManager.PrintSettings printSettingsSafe = this.settings_manager.Settings.GetPrintSettingsSafe(selectedPrinter.MyPrinterProfile.ProfileName);
      if (printsettings.Value.printSettings.options == null)
        return;
      printSettingsSafe.WaveBonding = printsettings.Value.printSettings.options.use_wave_bonding;
    }

    public void RemoveModel(int index)
    {
      if (index < 0 || index >= this.sceneGraph.ModelCount)
        return;
      this.sceneGraph.RemoveModel(index);
      this.DoModelListChanged();
    }

    public bool ObjectTransformed
    {
      set
      {
        this.objectTransformed = value;
      }
      get
      {
        return this.objectTransformed;
      }
    }

    public void ResetPrinterView()
    {
      this.sceneGraph.ResetPrinterTransformation();
      this.xAxisVector.x = 1f;
      this.xAxisVector.y = 0.0f;
      this.xAxisVector.z = 0.0f;
      this.yAxisVector.x = 0.0f;
      this.yAxisVector.y = -1f;
      this.yAxisVector.z = 0.0f;
    }

    public void CenterPrinterObject()
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      TransformationNode.Transform transformData1 = modelTransformPair.transformNode.TransformData;
      this.PlaceModel(new M3D.Model.Utils.Vector3(this.sceneGraph.PrinterCenter));
      TransformationNode.Transform transformData2 = modelTransformPair.transformNode.TransformData;
      this.history.PushTransformObject(modelTransformPair.data.ID, transformData1, transformData2);
      this.sceneGraph.PlaceObjectOnFloorAndCheckBounds(this.selected_model_index);
      this.PushUpdatedInfomation(modelTransformPair);
    }

    public void PlaceModel(M3D.Model.Utils.Vector3 position)
    {
      TransformationNode modelTransformation = this.ModelTransformation;
      if (modelTransformation == null)
        return;
      modelTransformation.Translation.x = position.x;
      modelTransformation.Translation.y = position.y;
      this.AdjustmentsDialog.SetTranslationValues(position.x, position.y);
    }

    public void AdjustPrinterObjectTranslation(float x, float y, float z)
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      this.AdjustmentsDialog.SetTranslationValues(new M3D.Model.Utils.Vector3(modelTransformPair.transformNode.Translation.x + x, modelTransformPair.transformNode.Translation.y + y, modelTransformPair.transformNode.Translation.z + z));
    }

    public void ScalePrinterObject(float x, float y, float z)
    {
      if (this.SelectedModelTransformPair == null)
        return;
      this.AdjustmentsDialog.SetScaleValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    public void RotatePrinterObject(float x, float y, float z)
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      this.AdjustmentsDialog.SetRotationValues(new M3D.Model.Utils.Vector3(modelTransformPair.transformNode.Rotation.x + x, modelTransformPair.transformNode.Rotation.y + y, modelTransformPair.transformNode.Rotation.z + z));
    }

    public void SetPrinterObjectTranslation(M3D.Model.Utils.Vector3 vector)
    {
      this.SetPrinterObjectTranslation(vector.x, vector.y);
    }

    public void SetPrinterObjectTranslation(float x, float y)
    {
      if (this.SelectedModelTransformPair == null)
        return;
      this.AdjustmentsDialog.SetTranslationValues(new M3D.Model.Utils.Vector3(x, y, 0.0f));
    }

    public void SetPrinterObjectRotatation(M3D.Model.Utils.Vector3 vector)
    {
      this.SetPrinterObjectRotatation(vector.x, vector.y, vector.z);
    }

    public void SetPrinterObjectRotatation(float x, float y, float z)
    {
      if (this.SelectedModelTransformPair == null)
        return;
      this.AdjustmentsDialog.SetRotationValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    public void SetPrinterObjectScale(M3D.Model.Utils.Vector3 vector)
    {
      this.SetPrinterObjectScale(vector.x, vector.y, vector.z);
    }

    public void SetPrinterObjectScale(float x, float y, float z)
    {
      if (this.selected_model_index < 0 || this.selected_model_index >= this.sceneGraph.ModelCount)
        return;
      this.AdjustmentsDialog.SetScaleValues(new M3D.Model.Utils.Vector3(x, y, z));
    }

    private void CheckUpdatedColors()
    {
      if (this.printerColorString != this.settings_manager.CurrentAppearanceSettings.PrinterColor)
        this.SetPrinterColor(this.settings_manager.CurrentAppearanceSettings.PrinterColor);
      if (this.modelColorString != this.settings_manager.CurrentAppearanceSettings.ModelColor || this.modelColorString == "Automatic")
        this.SetModelColor(this.settings_manager.CurrentAppearanceSettings.ModelColor);
      if (!(ImageCapture.IconColor != this.settings_manager.CurrentAppearanceSettings.IconColor))
        return;
      ImageCapture.IconColor = this.settings_manager.CurrentAppearanceSettings.IconColor;
    }

    private void CheckUpdatedCaseType()
    {
      if (!(this.SerialNumber == PrinterSerialNumber.Undefined) || this.sceneGraph.PrinterModelCaseType == this.settings_manager.CurrentAppearanceSettings.CaseType)
        return;
      this.sceneGraph.PrinterModelCaseType = this.settings_manager.CurrentAppearanceSettings.CaseType;
      this.UpdatePrinterFamily();
    }

    private void CheckUpdatedMeasumentUnits()
    {
      if (this.CurrentGridUnits == this.settings_manager.CurrentAppearanceSettings.Units)
        return;
      this.CurrentGridUnits = this.settings_manager.CurrentAppearanceSettings.Units;
      this.sceneGraph.GridCurrentUnits = this.CurrentGridUnits;
    }

    private void SetPrinterColor(string color)
    {
      this.printerColorString = color;
      if (color == "Automatic")
      {
        this.sceneGraph.PrinterColor = Colors.GetColorFromSN(this.SerialNumber.ToString());
      }
      else
      {
        try
        {
          this.sceneGraph.PrinterColor = Colors.GetColor(color);
        }
        catch (ArgumentException ex)
        {
          if (ex.Message == "Not a valid color")
            this.sceneGraph.PrinterColor = Colors.GetColor("Black");
        }
      }
      if (!this.mainForm.zoomed)
        return;
      this.sceneGraph.PrinterColorAlpha = this.printerZoomedAlpha;
    }

    private void SetModelColor(string color)
    {
      this.modelColorString = color;
      if (color.Equals("Automatic"))
      {
        FilamentSpool filamentSpool = (FilamentSpool) null;
        IPrinter selectedPrinter = (IPrinter) this.spooler_connection.SelectedPrinter;
        if (selectedPrinter != null)
          filamentSpool = selectedPrinter.GetCurrentFilament();
        if (filamentSpool != (FilamentSpool) null)
        {
          int num = (int) Enum.ToObject(typeof (FilamentConstants.ColorsEnum), filamentSpool.filament_color_code);
          color = FilamentConstants.ColorsToString((FilamentConstants.ColorsEnum) num);
          Color4 color4;
          FilamentConstants.HexToRGB(FilamentConstants.generateHEXFromColor((FilamentConstants.ColorsEnum) num), out color4.R, out color4.G, out color4.B, out color4.A);
          if (!(color4 != this.sceneGraph.ModelColor))
            return;
          this.sceneGraph.ModelColor = color4;
        }
        else
          this.sceneGraph.ModelColor = Colors.GetColor("LightBlue");
      }
      else
      {
        try
        {
          this.sceneGraph.ModelColor = Colors.GetColor(color);
        }
        catch (ArgumentException ex)
        {
          if (ex.Message == "Not a valid color")
            this.sceneGraph.ModelColor = Colors.GetColor("Black");
        }
      }
      this.modelColorString = color;
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      if (base.OnKeyboardEvent(keyboardevent))
        return true;
      if (keyboardevent.type == KeyboardEventType.InputKey)
      {
        InputKeyEvent inputKeyEvent = (InputKeyEvent) keyboardevent;
        if (inputKeyEvent.Ctrl)
        {
          if (inputKeyEvent.Ch == '\x001A')
            this.history.Undo();
          else if (inputKeyEvent.Ch == '\x0019')
            this.history.Redo();
        }
      }
      else if (keyboardevent.type == KeyboardEventType.CommandKey && ((CommandKeyEvent) keyboardevent).Key == KeyboardCommandKey.Delete)
        this.RemoveSelectedModel();
      return false;
    }

    public void RemoveSelectedModel()
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      int selectedModelIndex = this.selected_model_index;
      this.History.PushRemoveModelFile(modelTransformPair.data.ID, modelTransformPair.modelNode.fileName, modelTransformPair.modelNode.zipFileName, modelTransformPair.transformNode.TransformData);
      bool recordHistory = this.History.RecordHistory;
      this.History.RecordHistory = false;
      this.UnselectModel();
      this.RemoveModel(selectedModelIndex);
      this.sceneGraph.ResetExceedBounds();
      int index = selectedModelIndex - 1;
      if (index < 0)
        index = this.sceneGraph.ModelCount - 1;
      this.SelectModel(index);
      this.History.RecordHistory = recordHistory;
    }

    public void DuplicateSelection()
    {
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair == null)
        return;
      this.model_loading_manager.LoadModelIntoPrinter(modelTransformPair.modelNode.fileName);
    }

    public override void OnMouseLeave()
    {
      this.dragging = false;
      base.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      if (!this.Enabled)
        return false;
      OpenTK.Vector2 vector2;
      vector2.X = (float) mouseevent.pos.x;
      vector2.Y = (float) mouseevent.pos.y;
      MouseButton button = mouseevent.button;
      if (mouseevent.type == MouseEventType.Down)
      {
        if (button == MouseButton.Left)
        {
          if (!this.animating && this.viewstate == ViewState.Hidden)
          {
            if (this.mainForm != null)
              this.mainForm.SetToEditView();
          }
          else
          {
            this.SelectModel(this.sceneGraph.UnProjectMouseCoordinates(vector2.X, vector2.Y, this.sceneGraph.CreateViewMatrix()));
            this.m_gui_host.SetFocus(this.ID);
          }
        }
        this.dragging = true;
        vector2.X = (float) (int) vector2.X;
        vector2.Y = (float) (int) vector2.Y;
        this.prevX = (int) vector2.X;
        this.prevY = (int) vector2.Y;
      }
      else if (mouseevent.type == MouseEventType.Up)
        this.dragging = false;
      else if (mouseevent.type == MouseEventType.Move)
      {
        if (this.dragging)
        {
          switch (button)
          {
            case MouseButton.Left:
              this.rotVelocity = (float) (((double) vector2.X - (double) this.prevX) / 3.0);
              float tilt = (float) (((double) vector2.Y - (double) this.prevY) / 3.0);
              if ((double) tilt < 1.0 && (double) tilt > -1.0)
                tilt = 0.0f;
              this.sceneGraph.TiltPrinter(tilt);
              break;
            case MouseButton.Right:
              if (!this.animating && this.selected_model_index >= 0 && ((double) this.prevX != (double) vector2.X || (double) this.prevY != (double) vector2.Y))
              {
                if (this.AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Rotation)
                  this.AdjustmentsDialog.AdjustRotationValues(0.0f, 0.0f, (float) ((int) vector2.X - this.prevX));
                if (this.AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Scale)
                {
                  this.AdjustmentsDialog.CheckLinkedRescalingCheckbox(true);
                  float num = (float) (((double) vector2.Y - (double) this.prevY) / 100.0);
                  this.AdjustmentsDialog.AdjustScaleValues(num, num, num);
                }
                if (this.AdjustmentsDialog.Mode == ModelAdjustmentsDialog.EditMode.Translation)
                {
                  float num = 0.5f;
                  M3D.Model.Utils.Vector3 vector3 = this.xAxisVector * ((vector2.X - (float) this.prevX) * num) + this.yAxisVector * ((vector2.Y - (float) this.prevY) * num);
                  this.AdjustmentsDialog.AdjustTranslationValues(vector3.x, vector3.y, 0.0f);
                  break;
                }
                break;
              }
              break;
          }
          this.prevX = (int) vector2.X;
          this.prevY = (int) vector2.Y;
        }
      }
      else if (mouseevent.type == MouseEventType.MouseWheel && mouseevent.delta != 0)
      {
        if (mouseevent.delta < 0)
        {
          if (!this.animating && this.mainForm != null)
            this.mainForm.SetToEditView();
        }
        else if (!this.animating && this.mainForm != null)
          this.mainForm.SetToLibraryView();
      }
      return true;
    }

    private void UpdatePrinterFamily()
    {
      if (this.sceneGraph.PrinterModelCaseType == PrinterSizeProfile.CaseType.Micro1Case)
      {
        this.CurrentSizeProfile = (PrinterSizeProfile) new Micro1PrinterSizeProfile();
        this.CurrentFamilyName = PrinterObject.GetFamilyFromProfile("Micro");
      }
      else if (this.sceneGraph.PrinterModelCaseType == PrinterSizeProfile.CaseType.ProCase)
      {
        this.CurrentSizeProfile = (PrinterSizeProfile) new ProPrinterSizeProfile();
        this.CurrentFamilyName = PrinterObject.GetFamilyFromProfile("Pro");
      }
      if (this.SelectedModelTransformPair != null)
        this.SetModelRanges(this.SelectedModelTransformPair, this.CurrentSizeProfile);
      this.sceneGraph.SizeFromPrinterProfile(this.CurrentSizeProfile);
    }

    public void SetControlDialogVisibility(bool visible)
    {
      if (this.toolbar != null)
        this.toolbar.Visible = visible && this.viewstate == ViewState.Active;
      this.controlsHidden = !visible;
    }

    public void SetControlStateMaster(bool bShouldDisable)
    {
      this.SetControlDialogVisibility(!bShouldDisable);
      if (this.BottomControlBar != null)
        this.BottomControlBar.SetControlStateMaster(bShouldDisable);
      this.RightMouseButtonControlEnabled = !bShouldDisable;
      this._autoPrint = bShouldDisable;
    }

    public void ResetControlState()
    {
      this.SetControlStateMaster(false);
    }

    public override void OnUpdate()
    {
      this.Process();
      base.OnUpdate();
    }

    public void Process()
    {
      this.ProcessEditView();
      this.CheckUpdatedColors();
      this.CheckUpdatedMeasumentUnits();
      this.CheckUpdatedCaseType();
      if (this.sceneGraph.ViewPointPos != this.targetViewPointPos && this.targetViewPointPos != (M3D.Model.Utils.Vector3) null || (double) this.RelativeWidth != (double) this.targetWidth)
      {
        if (this.viewstate == ViewState.ToHidden)
        {
          this.ShowView(false);
          this.viewstate = ViewState.Hidden;
        }
        this.animating = true;
        this.elapsed = DateTime.Now.Ticks / 10000L - this.startTime;
        if (this.elapsed > this.animationTime)
          this.elapsed = this.animationTime;
        ++this.frameCount;
        this.fps = 1000f / (float) (this.elapsed / (long) this.frameCount);
        float num1 = this.movementMultiplier * 60f / this.fps;
        if (this.sceneGraph.ViewPointPos != this.targetViewPointPos)
        {
          this.vectorToTargetViewPointPos = this.targetViewPointPos - this.originalViewPointPos;
          float num2 = (this.sceneGraph.ViewPointPos - this.originalViewPointPos).Length() / (this.targetViewPointPos - this.originalViewPointPos).Length();
          if ((double) num2 <= 0.509999990463257)
          {
            this.mCameraVelocity += this.vectorToTargetViewPointPos * num1;
          }
          else
          {
            this.mCameraVelocity -= this.vectorToTargetViewPointPos * num1;
            if ((double) this.mCameraVelocity.Dot(this.vectorToTargetViewPointPos) < 0.0)
              num2 = 1f;
          }
          if ((double) num2 >= 1.0)
          {
            this.sceneGraph.ViewPointPos = this.targetViewPointPos;
            this.mCameraVelocity = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
            this.mainForm.zoomed = !this.mainForm.zoomed;
            if (this.mainForm.zoomed)
            {
              this.sceneGraph.PrinterColorAlpha = this.printerZoomedAlpha;
              this.sceneGraph.GridVisible = true;
            }
            else
            {
              this.sceneGraph.PrinterColorAlpha = 1f;
              this.sceneGraph.GridVisible = false;
            }
            this.startTime = DateTime.Now.Ticks / 10000L;
          }
          else
            this.sceneGraph.ViewPointPos = this.sceneGraph.ViewPointPos + this.mCameraVelocity;
        }
        if ((double) this.RelativeWidth == (double) this.targetWidth)
          return;
        float num3 = this.targetWidth - this.originalWidth;
        float num4 = Math.Abs(this.RelativeWidth - this.originalWidth) / Math.Abs(num3);
        if ((double) num4 <= 0.509999990463257)
        {
          this.mWidthVelocity += num3 * num1;
        }
        else
        {
          this.mWidthVelocity -= num3 * num1;
          if (Math.Sign(this.mWidthVelocity) != Math.Sign(num3))
            num4 = 1f;
        }
        if ((double) num4 >= 1.0)
        {
          this.RelativeWidth = this.targetWidth;
          this.mWidthVelocity = 0.0f;
        }
        else
          this.RelativeWidth += this.mWidthVelocity;
      }
      else if (this.viewstate == ViewState.ToActive)
      {
        this.ShowView(true);
        this.elapsed = DateTime.Now.Ticks / 10000L - this.startTime;
        this.realanimtiontime = 400;
        if (this.elapsed >= (long) this.realanimtiontime)
        {
          this.elapsed = (long) this.realanimtiontime;
          this.viewstate = ViewState.Active;
          this.EditFrame.RelativeWidth = 1f;
        }
        float num1 = (float) this.elapsed / (float) this.realanimtiontime;
        int num2 = (int) ((double) this.mainForm.glControl1.Height * 0.174999997019768);
        int num3 = this.Height + 10;
        int num4 = this.Height - 121;
        this.AdjustmentsDialog.Y = (int) ((double) (num2 - -291) * (double) num1) - 291;
        this.BottomControlBar.Y = (int) ((double) (num4 - num3) * (double) num1) + num3;
        this.EditFrame.RelativeWidth = (float) (1.5 - (double) num1 * 0.5);
      }
      else
      {
        this.animating = false;
        if ((double) this.rotVelocity == 0.0)
          return;
        this.sceneGraph.RotatePrinter(this.rotVelocity);
        float num = (float) Math.PI / 180f;
        this.xAxisVector.RotateVector(-this.rotVelocity * num, false, false, true);
        this.yAxisVector.RotateVector(-this.rotVelocity * num, false, false, true);
        this.rotateVelocityFriction = Math.Abs(this.rotVelocity) / 5f;
        if ((double) this.rotateVelocityFriction < (double) this.minRotateVelocityFriction)
          this.rotateVelocityFriction = this.minRotateVelocityFriction;
        if ((double) this.rotVelocity > 0.0)
        {
          this.rotVelocity -= this.rotateVelocityFriction;
          if ((double) this.rotVelocity >= 0.0)
            return;
          this.rotVelocity = 0.0f;
        }
        else
        {
          if ((double) this.rotVelocity >= 0.0)
            return;
          this.rotVelocity += this.rotateVelocityFriction;
          if ((double) this.rotVelocity <= 0.0)
            return;
          this.rotVelocity = 0.0f;
        }
      }
    }

    private void ProcessEditView()
    {
      if (this.model_loading_manager != null)
        this.BottomControlBar.EnableButtons(true, this.ModelLoaded);
      if (!this.ObjectTransformed)
        return;
      ModelTransformPair modelTransformPair = this.SelectedModelTransformPair;
      if (modelTransformPair != null)
      {
        TransformationNode.Transform transformData1 = modelTransformPair.transformNode.TransformData;
        this.AdjustmentsDialog.SaveCurrentSliderInfo();
        TransformationNode.Transform transformData2 = modelTransformPair.transformNode.TransformData;
        this.history.PushTransformObject(modelTransformPair.data.ID, transformData1, transformData2);
        this.sceneGraph.PlaceObjectOnFloorAndCheckBounds(this.selected_model_index);
      }
      this.ObjectTransformed = false;
    }

    public void SetPosition(float x, float y)
    {
      this.RelativeX = x;
      this.RelativeY = y;
      this.OnSetPosition();
    }

    public void SetSize(float width, float height)
    {
      this.RelativeWidth = width;
      this.RelativeHeight = height;
    }

    public void SetViewPointPos(float x, float y, float z)
    {
      this.sceneGraph.ViewPointPos = new M3D.Model.Utils.Vector3(x, y, z);
    }

    public void SetTargetViewPointPos(M3D.Model.Utils.Vector3 vector)
    {
      this.SetTargetViewPointPos(vector.x, vector.y, vector.z);
    }

    public void SetTargetViewPointPos(float x, float y, float z)
    {
      this.originalViewPointPos = this.sceneGraph.ViewPointPos;
      this.targetViewPointPos = new M3D.Model.Utils.Vector3(x, y, z);
      this.startTime = DateTime.Now.Ticks / 10000L;
      this.movementMultiplier = 0.01f;
      this.frameCount = 0;
    }

    public void SetTargetSize(float width, float height)
    {
      this.originalWidth = this.RelativeWidth;
      this.originalHeight = this.RelativeHeight;
      this.targetWidth = width;
      this.targetHeight = height;
      this.startTime = DateTime.Now.Ticks / 10000L;
      this.movementMultiplier = 0.01f;
      this.frameCount = 0;
    }

    public override void OnParentResize()
    {
      this.OnSetPosition();
      base.OnParentResize();
    }

    public void OnSetPosition()
    {
      if (this.viewstate != ViewState.Active)
        return;
      if (this.AdjustmentsDialog != null)
        this.AdjustmentsDialog.Y = (int) ((double) this.mainForm.glControl1.Height * 0.174999997019768);
      if (this.BottomControlBar == null)
        return;
      this.BottomControlBar.Y = this.Height - 121;
    }

    public bool TransitionViewState(ViewState new_state)
    {
      if (new_state == this.viewstate || new_state == ViewState.ToActive || new_state == ViewState.ToHidden)
        return false;
      float num1 = 0.0f;
      float num2 = -0.5f;
      if (new_state == ViewState.Active)
      {
        this.viewstate = ViewState.ToActive;
        this.target_y = num1;
        this.realanimtiontime = 1500;
        this.SetToEditView();
      }
      else
      {
        this.viewstate = ViewState.ToHidden;
        this.target_y = num2;
        this.realanimtiontime = 300;
        this.SetToLibraryView();
      }
      this.startTime = DateTime.Now.Ticks / 10000L;
      return true;
    }

    public void GotoLibraryView()
    {
      if (this.mainForm == null)
        return;
      this.mainForm.SetToLibraryView();
    }

    private bool SetToEditView()
    {
      if (!(this.targetViewPointPos == (M3D.Model.Utils.Vector3) null) && !(this.targetViewPointPos != PrinterView.editViewPoint))
        return false;
      this.SetTargetSize(PrinterView.editSize.X, PrinterView.editSize.Y);
      this.SetTargetViewPointPos(PrinterView.editViewPoint);
      return true;
    }

    private bool SetToLibraryView()
    {
      if (!(this.targetViewPointPos == (M3D.Model.Utils.Vector3) null) && !(this.targetViewPointPos != PrinterView.libraryViewPoint))
        return false;
      this.SetTargetSize(PrinterView.librarySize.X, PrinterView.librarySize.Y);
      this.SetTargetViewPointPos(PrinterView.libraryViewPoint);
      return true;
    }

    public override void Refresh()
    {
      base.Refresh();
      if (this.AdjustmentsDialog == null)
        return;
      this.AdjustmentsDialog.Refresh();
    }

    private void ShowView(bool show)
    {
      if (this.EditFrame.Visible == show)
        return;
      this.BottomControlBar.Visible = show;
      this.toolbar.Visible = show && !this.controlsHidden;
      this.EditFrame.Visible = show;
      if (!show)
        return;
      this.Refresh();
    }

    public void OnWindowResize(int new_width, int new_height)
    {
      this.OnSetPosition();
    }

    public void Print()
    {
      PrinterObject selectedPrinter = this.spooler_connection.SelectedPrinter;
      if (selectedPrinter == null)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoPrinter"));
      else if (selectedPrinter.Info.current_job != null)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_AlreadyPrinting"));
      else if (selectedPrinter.PrinterState == PrinterObject.State.IsCalibrating)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_Calibrating"));
      else if (!this.ModelLoaded)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NoModel"));
      else if (!selectedPrinter.Info.extruder.Z_Valid || !selectedPrinter.Info.calibration.Calibration_Valid)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_NotCalibrated"));
      else if (selectedPrinter.GetCurrentFilament() == (FilamentSpool) null)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_No3DInk"));
      else if (this.sceneGraph.ObjectExceedsBounds)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_PrinterViewError_OutofBounds"));
      else
        this.PrepareForPrinting(selectedPrinter);
    }

    internal PrintJobDetails CreatePrintJobDetails(out bool modelZTooSmall)
    {
      PrintJobDetails printJobDetails = new PrintJobDetails();
      modelZTooSmall = false;
      M3D.Model.Utils.Vector3 total1 = new M3D.Model.Utils.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      M3D.Model.Utils.Vector3 total2 = new M3D.Model.Utils.Vector3(float.MinValue, float.MinValue, float.MinValue);
      for (int index = 0; index < this.sceneGraph.ModelCount; ++index)
      {
        ModelTransformPair model = this.sceneGraph.GetModel(index);
        Model3DNode modelNode = model.modelNode;
        TransformationNode transformNode = model.transformNode;
        modelZTooSmall |= (double) model.modelSize.Ext.z < 0.400000005960464;
        this.UpdateMin(ref total1, model.modelSize.Min);
        this.UpdateMax(ref total2, model.modelSize.Max);
        printJobDetails.slicer_objects.Add(new ModelTransform(modelNode.ModelData, transformNode.GetTransformationMatrix()));
        printJobDetails.objectDetailsList.Add(new PrintDetails.ObjectDetails(modelNode.fileName, new PrintDetails.Transform(transformNode.Translation, transformNode.Scale, transformNode.Rotation)));
        this.libraryview.RecentModels.GenerateIconForLibrary(model);
      }
      string icon_file = Path.Combine(Paths.WorkingFolder, "previewimage.jpg");
      M3D.Model.Utils.Vector3 center = new M3D.Model.Utils.Vector3((float) (((double) total1.x + (double) total2.x) / 2.0), (float) (((double) total1.z + (double) total2.z) / 2.0), (float) (((double) total1.y + (double) total2.y) / 2.0));
      ImageCapture.GenerateMultiModelPreview(this.sceneGraph.GetAllModels(), icon_file, new OpenTK.Vector2(400f, 400f), new Color4(0.8431373f, 0.8901961f, 0.9921569f, 1f), this.OpenGLConnection.GLControl1, center);
      printJobDetails.preview_image = icon_file;
      return printJobDetails;
    }

    private void PrepareForPrinting(PrinterObject printer)
    {
      bool modelZTooSmall;
      PrintJobDetails printJobDetails = this.CreatePrintJobDetails(out modelZTooSmall);
      printJobDetails.printer = printer;
      if (modelZTooSmall)
        this.messagebox.AddMessageToQueue(Locale.GlobalLocale.T("T_WARNING_ModelToSmall"), PopupMessageBox.MessageBoxButtons.YESNO, new PopupMessageBox.OnUserSelectionDel(this.DoPrintCallback), (object) new PrinterView.PrintDialogDetails(printJobDetails, printer));
      else
        this.ShowPrintDialog(printJobDetails, printer);
    }

    private void UpdateMin(ref M3D.Model.Utils.Vector3 total, M3D.Model.Utils.Vector3 min)
    {
      if ((double) min.x < (double) total.x)
        total.x = min.x;
      if ((double) min.y < (double) total.y)
        total.y = min.y;
      if ((double) min.z >= (double) total.z)
        return;
      total.z = min.z;
    }

    private void UpdateMax(ref M3D.Model.Utils.Vector3 total, M3D.Model.Utils.Vector3 max)
    {
      if ((double) max.x > (double) total.x)
        total.x = max.x;
      if ((double) max.y > (double) total.y)
        total.y = max.y;
      if ((double) max.z <= (double) total.z)
        return;
      total.z = max.z;
    }

    private void ShowPrintDialog(PrintJobDetails details, PrinterObject printer)
    {
      if (this._autoPrint)
      {
        details.printer = printer;
        details.jobOptions = this.jobOptionsFromXML;
        details.GenerateSlicerSettings(printer, this);
        details.settings.transformation = this.GetObjectSlicerTransformation();
        details.autoPrint = true;
        this.printerdialog.Show(PrintDialogWidgetFrames.PreSlicingFrame, details);
      }
      else
        this.printerdialog.Show(PrintDialogWidgetFrames.PrintDialogFrame, details);
    }

    private void DoPrintCallback(PopupMessageBox.PopupResult result, MessageType type, PrinterSerialNumber sn, object user_data)
    {
      if (result != PopupMessageBox.PopupResult.Button1_YesOK)
        return;
      PrinterView.PrintDialogDetails printDialogDetails = (PrinterView.PrintDialogDetails) user_data;
      this.ShowPrintDialog(printDialogDetails.details, printDialogDetails.printer);
    }

    public void ResetTransformationValues()
    {
      this.AdjustmentsDialog.SetTranslationValues(0.0f, 0.0f);
      this.AdjustmentsDialog.SetRotationValues(0.0f, 0.0f, 0.0f);
      float num = (double) this.MaxScale.x < (double) this.MaxScale.y ? ((double) this.MaxScale.z < (double) this.MaxScale.x ? this.MaxScale.z : this.MaxScale.x) : this.MaxScale.y;
      if ((double) num < 1.0)
        this.AdjustmentsDialog.SetScaleValues(num, num, num);
      else
        this.AdjustmentsDialog.SetScaleValues(1f, 1f, 1f);
    }

    public M3D.Model.Utils.Vector3 GetTranslationValues()
    {
      this.AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = this.ModelTransformation;
      if (modelTransformation != null)
        return modelTransformation.Translation;
      return new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public M3D.Model.Utils.Vector3 GetScaleValues()
    {
      this.AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = this.ModelTransformation;
      if (modelTransformation != null)
        return modelTransformation.Scale;
      return new M3D.Model.Utils.Vector3(1f, 1f, 1f);
    }

    public M3D.Model.Utils.Vector3 GetRotationValues()
    {
      this.AdjustmentsDialog.SaveCurrentSliderInfo();
      TransformationNode modelTransformation = this.ModelTransformation;
      if (modelTransformation != null)
        return modelTransformation.Rotation;
      return new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    private void SelectedPrinterChanged(PrinterSerialNumber serial_number)
    {
      this.SerialNumber = serial_number;
      string serial_number1 = this.SerialNumber.ToString();
      if (this.printerColorString == "Automatic")
        this.SetPrinterColor("Automatic");
      if (serial_number == PrinterSerialNumber.Undefined)
      {
        this.sceneGraph.PrinterModelCaseType = this.settings_manager.CurrentAppearanceSettings.CaseType;
        this.UpdatePrinterFamily();
      }
      else
      {
        PrinterObject printerBySerialNumber = this.spooler_connection.GetPrinterBySerialNumber(serial_number1);
        if (printerBySerialNumber != null)
        {
          this.CurrentSizeProfile = printerBySerialNumber.MyPrinterProfile.PrinterSizeConstants;
          this.sceneGraph.SizeFromPrinterProfile(this.CurrentSizeProfile);
          if (this.SelectedModelTransformPair != null)
            this.SetModelRanges(this.SelectedModelTransformPair, this.CurrentSizeProfile);
          this.CurrentFamilyName = PrinterObject.GetFamilyFromProfile(printerBySerialNumber.MyPrinterProfile.ProfileName);
          this.slicer_connection.SlicerSettingStack.SetCurrentSettingsFromPrinterProfile((IPrinter) printerBySerialNumber);
          if (!((FilamentSpool) null == printerBySerialNumber.GetCurrentFilament()) || !this.printerdialog.Visible)
            return;
          this.printerdialog.CloseWindow();
          this.infobox.AddMessageToQueue("The print dialog was closed because the printer doesn't have 3D Ink.");
        }
        else
        {
          if (!this.printerdialog.Visible)
            return;
          this.printerdialog.CloseWindow();
          this.infobox.AddMessageToQueue("The print dialog was closed because the printer disconnected.");
        }
      }
    }

    public void SetModelRanges(ModelTransformPair selectedmodel, PrinterSizeProfile sizeProfile)
    {
      M3D.Model.Utils.Vector3 max = new M3D.Model.Utils.Vector3(sizeProfile.shell_size.x / selectedmodel.OriginalModelSize.Ext.x, sizeProfile.shell_size.y / selectedmodel.OriginalModelSize.Ext.y, sizeProfile.shell_size.z / selectedmodel.OriginalModelSize.Ext.z);
      this.SetScaleRange(new M3D.Model.Utils.Vector3(0.1f / selectedmodel.OriginalModelSize.Ext.x, 0.1f / selectedmodel.OriginalModelSize.Ext.y, 0.1f / selectedmodel.OriginalModelSize.Ext.z), max);
      this.AdjustmentsDialog.RefreshSliders();
      this.ObjectTransformed = true;
    }

    public void SetScaleRange(M3D.Model.Utils.Vector3 min, M3D.Model.Utils.Vector3 max)
    {
      this.min_scale = min;
      this.max_scale = max;
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
