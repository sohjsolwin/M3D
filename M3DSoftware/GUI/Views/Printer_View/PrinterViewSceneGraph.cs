// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.PrinterViewSceneGraph
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics;
using M3D.Graphics.Ext3D;
using M3D.Graphics.Ext3D.ModelRendering;
using M3D.GUI.Controller.Settings;
using M3D.GUI.Views.Printer_View.Specialized_Nodes;
using M3D.Properties;
using M3D.Spooling.Common;
using M3D.Spooling.Printer_Profiles;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Drawing;

namespace M3D.GUI.Views.Printer_View
{
  internal class PrinterViewSceneGraph : Frame3DView
  {
    private object exceedsBoundsSync = new object();
    private List<ModelTransformPair> ModelList;
    private TransformationNode OpenGLCoordinateSystem;
    private TransformationNode PrinterTypeAdjustments;
    private TransformationNode PrinterTransformation;
    private TransformationNode PrinterTiltTransform;
    private TransformationNode ObjectToPrinterSpace;
    private TransformationNode GridSizeAdjustments;
    private PrinterModelNode PrinterModel;
    private GridObjectNode Grid;
    private PrinterSizeProfile PrinterBoundsConst;
    private CustomShape[,] ExceedsBoundsGeometry;
    private Color4 modelColor;
    private M3D.Model.Utils.Vector3 printableRegionExts;
    private Color4 printerColor;
    private float printerColorAlpha;

    public bool ObjectExceedsBounds { get; private set; }

    public float PrinterBedWidth { get; private set; } = 109f;

    public float PrinterBedLength { get; private set; } = 105f;

    public M3D.Model.Utils.Vector3 PrinterCenter { get; private set; }

    public bool GridVisible
    {
      get
      {
        return this.Grid.Visible;
      }
      set
      {
        this.Grid.Visible = value;
      }
    }

    public SettingsManager.GridUnit GridCurrentUnits
    {
      get
      {
        return this.Grid.CurrentUnits;
      }
      set
      {
        this.Grid.SetUnits(value);
      }
    }

    public Color4 ModelColor
    {
      get
      {
        return this.modelColor;
      }
      set
      {
        this.modelColor = value;
        if (this.ModelList.Count <= 0)
          return;
        foreach (ModelTransformPair model in this.ModelList)
        {
          model.modelNode.Ambient = new Color4(this.modelColor.R / 4f, this.modelColor.G / 4f, this.modelColor.B / 4f, 1f);
          model.modelNode.Diffuse = this.modelColor;
        }
      }
    }

    public Color4 PrinterColor
    {
      get
      {
        return this.printerColor;
      }
      set
      {
        this.printerColor = value;
        this.PrinterModel.PrinterColor = value;
      }
    }

    public float PrinterColorAlpha
    {
      get
      {
        return this.printerColorAlpha;
      }
      set
      {
        this.printerColorAlpha = value;
        if (this.PrinterModel == null)
          return;
        Color4 printerColor = this.printerColor;
        printerColor.A *= value;
        this.PrinterModel.PrinterColor = printerColor;
      }
    }

    public int ModelCount
    {
      get
      {
        return this.ModelList.Count;
      }
    }

    public PrinterSizeProfile.CaseType PrinterModelCaseType
    {
      get
      {
        return this.PrinterModel.CaseType;
      }
      set
      {
        this.PrinterModel.SetCase(value);
        this.Grid.SetCaseType(value);
      }
    }

    public PrinterViewSceneGraph(GUIHost host, SettingsManager.GridUnit initialGridUnits, PrinterSizeProfile.CaseType initialCaseType)
      : base(123456)
    {
      this.RelativeX = 0.0f;
      this.RelativeY = 0.0f;
      this.RelativeWidth = 1f;
      this.RelativeHeight = 1f;
      this.ViewPointPos = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 10f);
      this.ModelList = new List<ModelTransformPair>();
      this.CameraLookAtPos = new M3D.Model.Utils.Vector3(0.0f, -10f, 0.0f);
      this.AddChildElement3D((Element3D) new LightNode(0, 0, new Vector4(0.0f, 400f, 1000f, 1f), new Color4(0.2f, 0.2f, 0.2f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(1f, 1f, 1f, 1f)));
      this.AddChildElement3D((Element3D) new LightNode(1, 1, new Vector4(0.0f, 20f, -400f, 1f), new Color4(0.0f, 0.0f, 0.0f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(0.3f, 0.3f, 0.3f, 1f)));
      this.OpenGLCoordinateSystem = new TransformationNode(0, this.origin);
      this.OpenGLCoordinateSystem.Rotation = new M3D.Model.Utils.Vector3(-90f, 0.0f, 0.0f);
      this.AddChildElement3D((Element3D) this.OpenGLCoordinateSystem);
      this.PrinterTiltTransform = new TransformationNode(0, (Element3D) this.OpenGLCoordinateSystem);
      this.OpenGLCoordinateSystem.AddChildElement((Element3D) this.PrinterTiltTransform);
      this.PrinterTransformation = new TransformationNode(0, (Element3D) this.PrinterTiltTransform);
      this.PrinterTypeAdjustments = new TransformationNode(3011, (Element3D) this.PrinterTransformation);
      float num = 1.1f;
      this.PrinterTypeAdjustments.Scale = new M3D.Model.Utils.Vector3(num, num, 1f);
      this.PrinterTransformation.AddChildElement((Element3D) this.PrinterTypeAdjustments);
      this.PrinterModel = new PrinterModelNode(initialCaseType);
      this.PrinterTypeAdjustments.AddChildElement((Element3D) this.PrinterModel);
      this.ObjectToPrinterSpace = new TransformationNode(3005, (Element3D) this.PrinterTransformation);
      this.ObjectToPrinterSpace.Translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      this.PrinterTransformation.AddChildElement((Element3D) this.ObjectToPrinterSpace);
      this.GridSizeAdjustments = new TransformationNode(3012, (Element3D) this.ObjectToPrinterSpace);
      this.ObjectToPrinterSpace.AddChildElement((Element3D) this.GridSizeAdjustments);
      this.Grid = new GridObjectNode(3003, 100f, 100f);
      this.Grid.SetUnits(initialGridUnits);
      this.Grid.Visible = false;
      this.Grid.Emission = new Color4(1f, 1f, 1f, 1f);
      this.GridSizeAdjustments.AddChildElement((Element3D) this.Grid);
      TexturedFloorNode texturedFloorNode = new TexturedFloorNode(3004);
      Bitmap bitmap = new Bitmap((Image) Resources.shadowtexture);
      int texture = 0;
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      texturedFloorNode.Create(new M3D.Model.Utils.Vector3(0.0f, 0.0f, -76f), 450f, 240f, texture);
      this.OpenGLCoordinateSystem.AddChildElement((Element3D) texturedFloorNode);
      this.PrinterTiltTransform.AddChildElement((Element3D) this.PrinterTransformation);
    }

    private void CreateBoundsGeometry(Element3D parent)
    {
      lock (this.exceedsBoundsSync)
      {
        if (this.PrinterBoundsConst.PrintableRegion == (StackedBoundingBox) null || this.PrinterBoundsConst.PrintableRegion.bounds_list.Count < 1)
          return;
        StackedBoundingBox printableRegion = this.PrinterBoundsConst.PrintableRegion;
        if (this.ExceedsBoundsGeometry != null)
        {
          for (int index1 = 0; index1 < this.ExceedsBoundsGeometry.GetLength(0); ++index1)
          {
            for (int index2 = 0; index2 < this.ExceedsBoundsGeometry.GetLength(1); ++index2)
              parent.RemoveChildElement((Element3D) this.ExceedsBoundsGeometry[index1, index2]);
          }
        }
        List<BoundingBox> boundsList = printableRegion.bounds_list;
        this.ExceedsBoundsGeometry = new CustomShape[boundsList.Count, 6];
        List<VertexTNV> vertex_list = new List<VertexTNV>();
        for (int index = 0; index < boundsList.Count; ++index)
        {
          M3D.Model.Utils.Vector3 vector3_1 = new M3D.Model.Utils.Vector3(boundsList[index].min.x, boundsList[index].min.y, boundsList[index].min.z);
          M3D.Model.Utils.Vector3 vector3_2 = new M3D.Model.Utils.Vector3(boundsList[index].max.x, boundsList[index].max.y, boundsList[index].max.z);
          this.ExceedsBoundsGeometry[index, 0] = new CustomShape(3006);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)));
          this.ExceedsBoundsGeometry[index, 0].Create(vertex_list, 0);
          this.BoundingBoxColoringHelper(ref this.ExceedsBoundsGeometry[index, 0]);
          parent.AddChildElement((Element3D) this.ExceedsBoundsGeometry[index, 0]);
          this.ExceedsBoundsGeometry[index, 1] = new CustomShape(3007);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_1.z)));
          this.ExceedsBoundsGeometry[index, 1].Create(vertex_list, 0);
          this.BoundingBoxColoringHelper(ref this.ExceedsBoundsGeometry[index, 1]);
          parent.AddChildElement((Element3D) this.ExceedsBoundsGeometry[index, 1]);
          this.ExceedsBoundsGeometry[index, 2] = new CustomShape(3008);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)));
          this.ExceedsBoundsGeometry[index, 2].Create(vertex_list, 0);
          this.BoundingBoxColoringHelper(ref this.ExceedsBoundsGeometry[index, 2]);
          parent.AddChildElement((Element3D) this.ExceedsBoundsGeometry[index, 2]);
          this.ExceedsBoundsGeometry[index, 3] = new CustomShape(3009);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_1.z)));
          this.ExceedsBoundsGeometry[index, 3].Create(vertex_list, 0);
          this.BoundingBoxColoringHelper(ref this.ExceedsBoundsGeometry[index, 3]);
          parent.AddChildElement((Element3D) this.ExceedsBoundsGeometry[index, 3]);
          if (index + 1 == boundsList.Count)
          {
            this.ExceedsBoundsGeometry[index, 4] = new CustomShape(3010);
            vertex_list.Clear();
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
            this.ExceedsBoundsGeometry[index, 4].Create(vertex_list, 0);
            this.BoundingBoxColoringHelper(ref this.ExceedsBoundsGeometry[index, 4]);
            parent.AddChildElement((Element3D) this.ExceedsBoundsGeometry[index, 4]);
          }
        }
      }
    }

    private void BoundingBoxColoringHelper(ref CustomShape shape)
    {
      shape.Visible = false;
      shape.Ambient = new Color4(1f, 0.0f, 0.0f, 0.5f);
      shape.Diffuse = new Color4(1f, 0.0f, 0.0f, 0.5f);
      shape.Emission = new Color4(1f, 0.0f, 0.0f, 0.5f);
      shape.Specular = new Color4(0.0f, 0.0f, 0.0f, 1f);
      shape.CullBackFaces = false;
    }

    public int AddModel(M3D.Graphics.Ext3D.ModelRendering.Model model, out ModelTransformPair modtrans_pair)
    {
      modtrans_pair = new ModelTransformPair();
      modtrans_pair.modelNode = new Model3DNode(3200);
      modtrans_pair.modelNode.SetModel(model);
      modtrans_pair.modelNode.Ambient = new Color4(this.modelColor.R / 4f, this.modelColor.G / 4f, this.modelColor.B / 4f, 1f);
      modtrans_pair.modelNode.Diffuse = this.modelColor;
      modtrans_pair.modelNode.Specular = new Color4(0.5f, 0.5f, 0.5f, 1f);
      modtrans_pair.modelNode.Shininess = 100f;
      modtrans_pair.transformNode = new TransformationNode(3100, (Element3D) this.ObjectToPrinterSpace);
      modtrans_pair.transformNode.AddChildElement((Element3D) modtrans_pair.modelNode);
      modtrans_pair.CalculateExtents();
      this.ObjectToPrinterSpace.AddChildElement((Element3D) modtrans_pair.transformNode);
      this.ModelList.Add(modtrans_pair);
      return this.ModelList.Count - 1;
    }

    private void SetPrintableExts(float x, float y, float z)
    {
      this.printableRegionExts = new M3D.Model.Utils.Vector3(x, y, z);
      this.PrinterCenter = new M3D.Model.Utils.Vector3(x / 2f, y / 2f, 0.0f);
    }

    public M3D.Model.Utils.Vector3 FindFreePosition(OpenTK.Vector2 modelsize)
    {
      M3D.Model.Utils.Vector3 position = new M3D.Model.Utils.Vector3(this.PrinterCenter);
      if (this.ModelCount < 1)
        return position;
      float num1 = modelsize.X * 0.75f;
      float num2 = modelsize.Y * 0.75f;
      for (int index = 0; index < 6; ++index)
      {
        float num3 = (float) (1.0 + 0.25 * (double) index);
        float num4 = (float) (((double) this.PrinterBedWidth * (double) num3 - (double) modelsize.X) / 2.0);
        float num5 = (float) (((double) this.PrinterBedLength * (double) num3 - (double) modelsize.Y) / 2.0);
        float num6 = (float) ((double) this.PrinterCenter.x + (double) num4 - 2.0);
        float num7 = (float) ((double) this.PrinterCenter.y + (double) num5 - 2.0);
        float num8 = (float) ((double) this.PrinterCenter.x - (double) num4 + 2.0);
        float num9 = (float) ((double) this.PrinterCenter.y - (double) num5 + 2.0);
        int num10 = 0;
label_24:
        if (num10 < 8)
        {
          int num11;
          int num12;
          if (num10 == 0)
          {
            num11 = 1;
            num12 = 0;
          }
          else if (num10 == 1)
          {
            num11 = 0;
            num12 = 1;
          }
          else if (num10 == 2)
          {
            num11 = -1;
            num12 = 0;
          }
          else if (num10 == 3)
          {
            num11 = 0;
            num12 = -1;
          }
          else if (num10 == 4)
          {
            num11 = 2;
            num12 = 2;
          }
          else if (num10 == 5)
          {
            num11 = -2;
            num12 = 2;
          }
          else if (num10 == 6)
          {
            num11 = -2;
            num12 = -2;
          }
          else
          {
            num11 = 2;
            num12 = -2;
          }
          position.x = this.PrinterCenter.x;
          position.y = this.PrinterCenter.y;
          while (!this.CheckIsPositionFree(position, modelsize))
          {
            position.x += (float) num11 * num1;
            position.y += (float) num12 * num2;
            if ((double) position.x >= (double) num6 || (double) position.y >= (double) num7 || ((double) position.x <= (double) num8 || (double) position.y <= (double) num9))
            {
              ++num10;
              goto label_24;
            }
          }
          return position;
        }
      }
      return position;
    }

    private bool CheckIsPositionFree(M3D.Model.Utils.Vector3 position, OpenTK.Vector2 modelsize)
    {
      BoundingBox boundingBox = new BoundingBox(new Vector3D(position.x - modelsize.X / 2f, position.y - modelsize.Y / 2f, 0.0f), new Vector3D(position.x + modelsize.X / 2f, position.y + modelsize.Y / 2f, 0.0f));
      bool flag = true;
      foreach (ModelTransformPair model in this.ModelList)
      {
        float x = model.transformNode.Translation.x;
        float y = model.transformNode.Translation.y;
        float num1 = model.modelSize.Ext.x / 2f;
        float num2 = model.modelSize.Ext.y / 2f;
        BoundingBox other = new BoundingBox(new Vector3D(x - num1 / 2f, y - num2 / 2f, 0.0f), new Vector3D(x + num1 / 2f, y + num2 / 2f, 0.0f));
        if (boundingBox.OverLap(other))
        {
          flag = false;
          break;
        }
      }
      return flag;
    }

    public void RemovePrintableModels()
    {
      while (this.ModelList.Count > 0)
        this.RemoveModel(0);
    }

    public void RemoveModel(int index)
    {
      if (index < 0 || index >= this.ModelList.Count)
        return;
      this.ObjectToPrinterSpace.RemoveChildElement((Element3D) this.ModelList[index].transformNode);
      this.ModelList.RemoveAt(index);
    }

    public void PlaceObjectOnFloorAndCheckBounds(int selected_model_index)
    {
      if (this.ModelList.Count <= 0 || selected_model_index < 0)
        return;
      this.ModelList[selected_model_index].CalculateExtents();
      this.PlaceObjectOnFloor(selected_model_index);
      this.ObjectWithinBounds(selected_model_index);
    }

    public void PlaceObjectOnFloor(int selected_model_index)
    {
      if (this.ModelList.Count <= 0 || selected_model_index < 0)
        return;
      ModelTransformPair model = this.ModelList[selected_model_index];
      M3D.Model.Utils.Vector3 vector3 = new M3D.Model.Utils.Vector3(0.0f, 0.0f, (float) (-(double) model.modelSize.Min.z + 1.0));
      model.transformNode.Translation.z += vector3.z;
      model.CalculateExtents();
    }

    public void ResetPrinterTransformation()
    {
      this.PrinterTransformation.Translation.x = 0.0f;
      this.PrinterTransformation.Translation.y = 0.0f;
      this.PrinterTransformation.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      this.PrinterTiltTransform.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public void TiltPrinter(float tilt)
    {
      this.PrinterTiltTransform.Rotation.x += tilt;
      if ((double) this.PrinterTiltTransform.Rotation.x > 90.0)
      {
        this.PrinterTiltTransform.Rotation.x = 90f;
      }
      else
      {
        if ((double) this.PrinterTiltTransform.Rotation.x >= 0.0)
          return;
        this.PrinterTiltTransform.Rotation.x = 0.0f;
      }
    }

    public void RotatePrinter(float rotVelocity)
    {
      this.PrinterTransformation.Rotation.z += rotVelocity;
    }

    private void ObjectWithinBounds(int selected_model_index)
    {
      this.ResetExceedBounds();
      lock (this.exceedsBoundsSync)
      {
        if (this.ModelList.Count <= 0 || selected_model_index < 0)
          return;
        ModelTransformPair model = this.ModelList[selected_model_index];
        List<M3D.Model.Utils.Vector3> usingTransformMatric = model.modelNode.CalculateBoundingUsingTransformMatric(model.transformNode.GetTransformationMatrix());
        StackedBoundingBox printableRegion = this.PrinterBoundsConst.PrintableRegion;
        foreach (M3D.Model.Utils.Vector3 vector3 in usingTransformMatric)
        {
          int count = printableRegion.bounds_list.Count;
          for (int index = 0; index < count; ++index)
          {
            if ((double) vector3.z >= (double) printableRegion.bounds_list[index].min.z && (index >= count - 1 || (double) vector3.z <= (double) printableRegion.bounds_list[index].max.z))
            {
              int num = printableRegion.bounds_list[index].outOfBoundsCheck(vector3.x, vector3.y, vector3.z);
              if ((num & 1) != 0)
                this.ExceedsBoundsGeometry[index, 1].Visible = true;
              if ((num & 2) != 0)
                this.ExceedsBoundsGeometry[index, 0].Visible = true;
              if ((num & 4) != 0)
                this.ExceedsBoundsGeometry[index, 2].Visible = true;
              if ((num & 8) != 0)
                this.ExceedsBoundsGeometry[index, 3].Visible = true;
              if ((num & 32) != 0 && index + 1 == printableRegion.bounds_list.Count)
                this.ExceedsBoundsGeometry[index, 4].Visible = true;
            }
          }
        }
        this.ObjectExceedsBounds = false;
        for (int index1 = 0; index1 < this.ExceedsBoundsGeometry.GetLength(0) && !this.ObjectExceedsBounds; ++index1)
        {
          for (int index2 = 0; index2 < this.ExceedsBoundsGeometry.GetLength(1) && !this.ObjectExceedsBounds; ++index2)
          {
            if (this.ExceedsBoundsGeometry[index1, index2] != null)
              this.ObjectExceedsBounds = this.ExceedsBoundsGeometry[index1, index2].Visible;
          }
        }
      }
    }

    public void ResetExceedBounds()
    {
      lock (this.exceedsBoundsSync)
      {
        int length = this.ExceedsBoundsGeometry.GetLength(0);
        for (int index = 0; index < length; ++index)
        {
          this.ExceedsBoundsGeometry[index, 2].Visible = false;
          this.ExceedsBoundsGeometry[index, 3].Visible = false;
          this.ExceedsBoundsGeometry[index, 1].Visible = false;
          this.ExceedsBoundsGeometry[index, 0].Visible = false;
          if (index + 1 == length)
            this.ExceedsBoundsGeometry[index, 4].Visible = false;
        }
        this.ObjectExceedsBounds = false;
      }
    }

    public void SizeFromPrinterProfile(PrinterSizeProfile sizeProfile)
    {
      this.PrinterModelCaseType = sizeProfile.case_type;
      M3D.Model.Utils.Vector3 ext = this.PrinterModel.ShellModel.Ext;
      this.PrinterTypeAdjustments.Scale.x = sizeProfile.shell_size.x / ext.x;
      this.PrinterTypeAdjustments.Scale.y = sizeProfile.shell_size.y / ext.y;
      this.PrinterTypeAdjustments.Scale.z = sizeProfile.shell_size.z / ext.x;
      this.PrinterTransformation.Translation.z = this.PrinterModel.ZOffset;
      this.ObjectToPrinterSpace.Translation.x = sizeProfile.printBedSize.x / -2f;
      this.ObjectToPrinterSpace.Translation.y = sizeProfile.printBedSize.y / -2f;
      this.ObjectToPrinterSpace.Translation.z = (float) ((double) sizeProfile.shell_size.z / -2.0 + (double) sizeProfile.fluff_height * (double) this.PrinterTypeAdjustments.Scale.z);
      this.GridSizeAdjustments.Scale.x = sizeProfile.printBedSize.x / 100f;
      this.GridSizeAdjustments.Scale.y = sizeProfile.printBedSize.y / 100f;
      this.PrinterBedWidth = sizeProfile.printBedSize.x;
      this.PrinterBedLength = sizeProfile.printBedSize.y;
      this.SetPrintableExts(sizeProfile.printBedSize.x, sizeProfile.printBedSize.y, 110f);
      this.PrinterCenter = new M3D.Model.Utils.Vector3(sizeProfile.HomeLocation.x, sizeProfile.HomeLocation.y, 0.0f);
      float num = this.PrinterModel.GUICaseSize / sizeProfile.shell_size.z;
      this.PrinterTransformation.Scale = new M3D.Model.Utils.Vector3(num, num, num);
      this.PrinterBoundsConst = sizeProfile;
      this.CreateBoundsGeometry((Element3D) this.ObjectToPrinterSpace);
    }

    public List<ModelTransformPair.Data> GetModelDataList()
    {
      List<ModelTransformPair.Data> dataList = new List<ModelTransformPair.Data>();
      foreach (ModelTransformPair model in this.ModelList)
        dataList.Add(model.data);
      return dataList;
    }

    public ModelTransformPair GetModel(int index)
    {
      return this.ModelList[index];
    }

    public List<ModelTransformPair> GetAllModels()
    {
      return new List<ModelTransformPair>((IEnumerable<ModelTransformPair>) this.ModelList);
    }

    public Matrix4 CreateViewMatrix()
    {
      return this.ObjectToPrinterSpace.GetTransformationMatrix() * this.PrinterTransformation.GetTransformationMatrix() * this.PrinterTiltTransform.GetTransformationMatrix() * this.OpenGLCoordinateSystem.GetTransformationMatrix();
    }

    public enum ElementIDs
    {
      Static = 0,
      PrinterModel = 3000, // 0x00000BB8
      PrinterBed = 3001, // 0x00000BB9
      PrinterLogo = 3002, // 0x00000BBA
      PrinterGrid = 3003, // 0x00000BBB
      PrinterShadow = 3004, // 0x00000BBC
      ObjectToPrinterSpace = 3005, // 0x00000BBD
      ExceedsBoundBack = 3006, // 0x00000BBE
      ExceedsBoundFront = 3007, // 0x00000BBF
      ExceedsBoundLeft = 3008, // 0x00000BC0
      ExceedsBoundRight = 3009, // 0x00000BC1
      ExceedsBoundTop = 3010, // 0x00000BC2
      PrinterTypeAdjustments = 3011, // 0x00000BC3
      GridSizeAdjustments = 3012, // 0x00000BC4
      ObjectTransStart = 3100, // 0x00000C1C
      ObjectModel = 3200, // 0x00000C80
    }

    private enum ExceedsGeometryIndex
    {
      Back,
      Front,
      Left,
      Right,
      Top,
    }
  }
}
