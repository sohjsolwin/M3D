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
        return Grid.Visible;
      }
      set
      {
        Grid.Visible = value;
      }
    }

    public SettingsManager.GridUnit GridCurrentUnits
    {
      get
      {
        return Grid.CurrentUnits;
      }
      set
      {
        Grid.SetUnits(value);
      }
    }

    public Color4 ModelColor
    {
      get
      {
        return modelColor;
      }
      set
      {
        modelColor = value;
        if (ModelList.Count <= 0)
        {
          return;
        }

        foreach (ModelTransformPair model in ModelList)
        {
          model.modelNode.Ambient = new Color4(modelColor.R / 4f, modelColor.G / 4f, modelColor.B / 4f, 1f);
          model.modelNode.Diffuse = modelColor;
        }
      }
    }

    public Color4 PrinterColor
    {
      get
      {
        return printerColor;
      }
      set
      {
        printerColor = value;
        PrinterModel.PrinterColor = value;
      }
    }

    public float PrinterColorAlpha
    {
      get
      {
        return printerColorAlpha;
      }
      set
      {
        printerColorAlpha = value;
        if (PrinterModel == null)
        {
          return;
        }

        Color4 printerColor = this.printerColor;
        printerColor.A *= value;
        PrinterModel.PrinterColor = printerColor;
      }
    }

    public int ModelCount
    {
      get
      {
        return ModelList.Count;
      }
    }

    public PrinterSizeProfile.CaseType PrinterModelCaseType
    {
      get
      {
        return PrinterModel.CaseType;
      }
      set
      {
        PrinterModel.SetCase(value);
        Grid.SetCaseType(value);
      }
    }

    public PrinterViewSceneGraph(GUIHost host, SettingsManager.GridUnit initialGridUnits, PrinterSizeProfile.CaseType initialCaseType)
      : base(123456)
    {
      RelativeX = 0.0f;
      RelativeY = 0.0f;
      RelativeWidth = 1f;
      RelativeHeight = 1f;
      ViewPointPos = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 10f);
      ModelList = new List<ModelTransformPair>();
      CameraLookAtPos = new M3D.Model.Utils.Vector3(0.0f, -10f, 0.0f);
      AddChildElement3D((Element3D) new LightNode(0, 0, new Vector4(0.0f, 400f, 1000f, 1f), new Color4(0.2f, 0.2f, 0.2f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(1f, 1f, 1f, 1f)));
      AddChildElement3D((Element3D) new LightNode(1, 1, new Vector4(0.0f, 20f, -400f, 1f), new Color4(0.0f, 0.0f, 0.0f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(0.3f, 0.3f, 0.3f, 1f)));
      OpenGLCoordinateSystem = new TransformationNode(0, origin)
      {
        Rotation = new M3D.Model.Utils.Vector3(-90f, 0.0f, 0.0f)
      };
      AddChildElement3D((Element3D)OpenGLCoordinateSystem);
      PrinterTiltTransform = new TransformationNode(0, (Element3D)OpenGLCoordinateSystem);
      OpenGLCoordinateSystem.AddChildElement((Element3D)PrinterTiltTransform);
      PrinterTransformation = new TransformationNode(0, (Element3D)PrinterTiltTransform);
      PrinterTypeAdjustments = new TransformationNode(3011, (Element3D)PrinterTransformation);
      var num = 1.1f;
      PrinterTypeAdjustments.Scale = new M3D.Model.Utils.Vector3(num, num, 1f);
      PrinterTransformation.AddChildElement((Element3D)PrinterTypeAdjustments);
      PrinterModel = new PrinterModelNode(initialCaseType);
      PrinterTypeAdjustments.AddChildElement((Element3D)PrinterModel);
      ObjectToPrinterSpace = new TransformationNode(3005, (Element3D)PrinterTransformation)
      {
        Translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f)
      };
      PrinterTransformation.AddChildElement((Element3D)ObjectToPrinterSpace);
      GridSizeAdjustments = new TransformationNode(3012, (Element3D)ObjectToPrinterSpace);
      ObjectToPrinterSpace.AddChildElement((Element3D)GridSizeAdjustments);
      Grid = new GridObjectNode(3003, 100f, 100f);
      Grid.SetUnits(initialGridUnits);
      Grid.Visible = false;
      Grid.Emission = new Color4(1f, 1f, 1f, 1f);
      GridSizeAdjustments.AddChildElement((Element3D)Grid);
      var texturedFloorNode = new TexturedFloorNode(3004);
      var bitmap = new Bitmap((Image) Resources.shadowtexture);
      var texture = 0;
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      texturedFloorNode.Create(new M3D.Model.Utils.Vector3(0.0f, 0.0f, -76f), 450f, 240f, texture);
      OpenGLCoordinateSystem.AddChildElement((Element3D) texturedFloorNode);
      PrinterTiltTransform.AddChildElement((Element3D)PrinterTransformation);
    }

    private void CreateBoundsGeometry(Element3D parent)
    {
      lock (exceedsBoundsSync)
      {
        if (PrinterBoundsConst.PrintableRegion == (StackedBoundingBox) null || PrinterBoundsConst.PrintableRegion.bounds_list.Count < 1)
        {
          return;
        }

        StackedBoundingBox printableRegion = PrinterBoundsConst.PrintableRegion;
        if (ExceedsBoundsGeometry != null)
        {
          for (var index1 = 0; index1 < ExceedsBoundsGeometry.GetLength(0); ++index1)
          {
            for (var index2 = 0; index2 < ExceedsBoundsGeometry.GetLength(1); ++index2)
            {
              parent.RemoveChildElement((Element3D)ExceedsBoundsGeometry[index1, index2]);
            }
          }
        }
        List<BoundingBox> boundsList = printableRegion.bounds_list;
        ExceedsBoundsGeometry = new CustomShape[boundsList.Count, 6];
        var vertex_list = new List<VertexTNV>();
        for (var index = 0; index < boundsList.Count; ++index)
        {
          var vector3_1 = new Model.Utils.Vector3(boundsList[index].min.x, boundsList[index].min.y, boundsList[index].min.z);
          var vector3_2 = new M3D.Model.Utils.Vector3(boundsList[index].max.x, boundsList[index].max.y, boundsList[index].max.z);
          ExceedsBoundsGeometry[index, 0] = new CustomShape(3006);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)));
          ExceedsBoundsGeometry[index, 0].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 0]);
          parent.AddChildElement((Element3D)ExceedsBoundsGeometry[index, 0]);
          ExceedsBoundsGeometry[index, 1] = new CustomShape(3007);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_1.z)));
          ExceedsBoundsGeometry[index, 1].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 1]);
          parent.AddChildElement((Element3D)ExceedsBoundsGeometry[index, 1]);
          ExceedsBoundsGeometry[index, 2] = new CustomShape(3008);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_1.z)));
          ExceedsBoundsGeometry[index, 2].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 2]);
          parent.AddChildElement((Element3D)ExceedsBoundsGeometry[index, 2]);
          ExceedsBoundsGeometry[index, 3] = new CustomShape(3009);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_1.z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_1.z)));
          ExceedsBoundsGeometry[index, 3].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 3]);
          parent.AddChildElement((Element3D)ExceedsBoundsGeometry[index, 3]);
          if (index + 1 == boundsList.Count)
          {
            ExceedsBoundsGeometry[index, 4] = new CustomShape(3010);
            vertex_list.Clear();
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
            ExceedsBoundsGeometry[index, 4].Create(vertex_list, 0);
            BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 4]);
            parent.AddChildElement((Element3D)ExceedsBoundsGeometry[index, 4]);
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

    public int AddModel(Graphics.Ext3D.ModelRendering.Model model, out ModelTransformPair modtrans_pair)
    {
      modtrans_pair = new ModelTransformPair
      {
        modelNode = new Model3DNode(3200)
      };
      modtrans_pair.modelNode.SetModel(model);
      modtrans_pair.modelNode.Ambient = new Color4(modelColor.R / 4f, modelColor.G / 4f, modelColor.B / 4f, 1f);
      modtrans_pair.modelNode.Diffuse = modelColor;
      modtrans_pair.modelNode.Specular = new Color4(0.5f, 0.5f, 0.5f, 1f);
      modtrans_pair.modelNode.Shininess = 100f;
      modtrans_pair.transformNode = new TransformationNode(3100, (Element3D)ObjectToPrinterSpace);
      modtrans_pair.transformNode.AddChildElement((Element3D) modtrans_pair.modelNode);
      modtrans_pair.CalculateExtents();
      ObjectToPrinterSpace.AddChildElement((Element3D) modtrans_pair.transformNode);
      ModelList.Add(modtrans_pair);
      return ModelList.Count - 1;
    }

    private void SetPrintableExts(float x, float y, float z)
    {
      printableRegionExts = new M3D.Model.Utils.Vector3(x, y, z);
      PrinterCenter = new M3D.Model.Utils.Vector3(x / 2f, y / 2f, 0.0f);
    }

    public M3D.Model.Utils.Vector3 FindFreePosition(OpenTK.Vector2 modelsize)
    {
      var position = new M3D.Model.Utils.Vector3(PrinterCenter);
      if (ModelCount < 1)
      {
        return position;
      }

      var num1 = modelsize.X * 0.75f;
      var num2 = modelsize.Y * 0.75f;
      for (var index = 0; index < 6; ++index)
      {
        var num3 = (float) (1.0 + 0.25 * (double) index);
        var num4 = (float)((PrinterBedWidth * (double)num3 - modelsize.X) / 2.0);
        var num5 = (float) (((double)PrinterBedLength * (double) num3 - (double) modelsize.Y) / 2.0);
        var num6 = (float) ((double)PrinterCenter.x + (double) num4 - 2.0);
        var num7 = (float) ((double)PrinterCenter.y + (double) num5 - 2.0);
        var num8 = (float) ((double)PrinterCenter.x - (double) num4 + 2.0);
        var num9 = (float) ((double)PrinterCenter.y - (double) num5 + 2.0);
        var num10 = 0;
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
          position.x = PrinterCenter.x;
          position.y = PrinterCenter.y;
          while (!CheckIsPositionFree(position, modelsize))
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
      var boundingBox = new BoundingBox(new Vector3D(position.x - modelsize.X / 2f, position.y - modelsize.Y / 2f, 0.0f), new Vector3D(position.x + modelsize.X / 2f, position.y + modelsize.Y / 2f, 0.0f));
      var flag = true;
      foreach (ModelTransformPair model in ModelList)
      {
        var x = model.transformNode.Translation.x;
        var y = model.transformNode.Translation.y;
        var num1 = model.modelSize.Ext.x / 2f;
        var num2 = model.modelSize.Ext.y / 2f;
        var other = new BoundingBox(new Vector3D(x - num1 / 2f, y - num2 / 2f, 0.0f), new Vector3D(x + num1 / 2f, y + num2 / 2f, 0.0f));
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
      while (ModelList.Count > 0)
      {
        RemoveModel(0);
      }
    }

    public void RemoveModel(int index)
    {
      if (index < 0 || index >= ModelList.Count)
      {
        return;
      }

      ObjectToPrinterSpace.RemoveChildElement((Element3D)ModelList[index].transformNode);
      ModelList.RemoveAt(index);
    }

    public void PlaceObjectOnFloorAndCheckBounds(int selected_model_index)
    {
      if (ModelList.Count <= 0 || selected_model_index < 0)
      {
        return;
      }

      ModelList[selected_model_index].CalculateExtents();
      PlaceObjectOnFloor(selected_model_index);
      ObjectWithinBounds(selected_model_index);
    }

    public void PlaceObjectOnFloor(int selected_model_index)
    {
      if (ModelList.Count <= 0 || selected_model_index < 0)
      {
        return;
      }

      ModelTransformPair model = ModelList[selected_model_index];
      var vector3 = new M3D.Model.Utils.Vector3(0.0f, 0.0f, (float) (-(double) model.modelSize.Min.z + 1.0));
      model.transformNode.Translation.z += vector3.z;
      model.CalculateExtents();
    }

    public void ResetPrinterTransformation()
    {
      PrinterTransformation.Translation.x = 0.0f;
      PrinterTransformation.Translation.y = 0.0f;
      PrinterTransformation.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      PrinterTiltTransform.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public void TiltPrinter(float tilt)
    {
      PrinterTiltTransform.Rotation.x += tilt;
      if ((double)PrinterTiltTransform.Rotation.x > 90.0)
      {
        PrinterTiltTransform.Rotation.x = 90f;
      }
      else
      {
        if ((double)PrinterTiltTransform.Rotation.x >= 0.0)
        {
          return;
        }

        PrinterTiltTransform.Rotation.x = 0.0f;
      }
    }

    public void RotatePrinter(float rotVelocity)
    {
      PrinterTransformation.Rotation.z += rotVelocity;
    }

    private void ObjectWithinBounds(int selected_model_index)
    {
      ResetExceedBounds();
      lock (exceedsBoundsSync)
      {
        if (ModelList.Count <= 0 || selected_model_index < 0)
        {
          return;
        }

        ModelTransformPair model = ModelList[selected_model_index];
        List<M3D.Model.Utils.Vector3> usingTransformMatric = model.modelNode.CalculateBoundingUsingTransformMatric(model.transformNode.GetTransformationMatrix());
        StackedBoundingBox printableRegion = PrinterBoundsConst.PrintableRegion;
        foreach (M3D.Model.Utils.Vector3 vector3 in usingTransformMatric)
        {
          var count = printableRegion.bounds_list.Count;
          for (var index = 0; index < count; ++index)
          {
            if ((double) vector3.z >= (double) printableRegion.bounds_list[index].min.z && (index >= count - 1 || (double) vector3.z <= (double) printableRegion.bounds_list[index].max.z))
            {
              var num = printableRegion.bounds_list[index].outOfBoundsCheck(vector3.x, vector3.y, vector3.z);
              if ((num & 1) != 0)
              {
                ExceedsBoundsGeometry[index, 1].Visible = true;
              }

              if ((num & 2) != 0)
              {
                ExceedsBoundsGeometry[index, 0].Visible = true;
              }

              if ((num & 4) != 0)
              {
                ExceedsBoundsGeometry[index, 2].Visible = true;
              }

              if ((num & 8) != 0)
              {
                ExceedsBoundsGeometry[index, 3].Visible = true;
              }

              if ((num & 32) != 0 && index + 1 == printableRegion.bounds_list.Count)
              {
                ExceedsBoundsGeometry[index, 4].Visible = true;
              }
            }
          }
        }
        ObjectExceedsBounds = false;
        for (var index1 = 0; index1 < ExceedsBoundsGeometry.GetLength(0) && !ObjectExceedsBounds; ++index1)
        {
          for (var index2 = 0; index2 < ExceedsBoundsGeometry.GetLength(1) && !ObjectExceedsBounds; ++index2)
          {
            if (ExceedsBoundsGeometry[index1, index2] != null)
            {
              ObjectExceedsBounds = ExceedsBoundsGeometry[index1, index2].Visible;
            }
          }
        }
      }
    }

    public void ResetExceedBounds()
    {
      lock (exceedsBoundsSync)
      {
        var length = ExceedsBoundsGeometry.GetLength(0);
        for (var index = 0; index < length; ++index)
        {
          ExceedsBoundsGeometry[index, 2].Visible = false;
          ExceedsBoundsGeometry[index, 3].Visible = false;
          ExceedsBoundsGeometry[index, 1].Visible = false;
          ExceedsBoundsGeometry[index, 0].Visible = false;
          if (index + 1 == length)
          {
            ExceedsBoundsGeometry[index, 4].Visible = false;
          }
        }
        ObjectExceedsBounds = false;
      }
    }

    public void SizeFromPrinterProfile(PrinterSizeProfile sizeProfile)
    {
      PrinterModelCaseType = sizeProfile.case_type;
      M3D.Model.Utils.Vector3 ext = PrinterModel.ShellModel.Ext;
      PrinterTypeAdjustments.Scale.x = sizeProfile.shell_size.x / ext.x;
      PrinterTypeAdjustments.Scale.y = sizeProfile.shell_size.y / ext.y;
      PrinterTypeAdjustments.Scale.z = sizeProfile.shell_size.z / ext.x;
      PrinterTransformation.Translation.z = PrinterModel.ZOffset;
      ObjectToPrinterSpace.Translation.x = sizeProfile.printBedSize.x / -2f;
      ObjectToPrinterSpace.Translation.y = sizeProfile.printBedSize.y / -2f;
      ObjectToPrinterSpace.Translation.z = (float) ((double) sizeProfile.shell_size.z / -2.0 + (double) sizeProfile.fluff_height * (double)PrinterTypeAdjustments.Scale.z);
      GridSizeAdjustments.Scale.x = sizeProfile.printBedSize.x / 100f;
      GridSizeAdjustments.Scale.y = sizeProfile.printBedSize.y / 100f;
      PrinterBedWidth = sizeProfile.printBedSize.x;
      PrinterBedLength = sizeProfile.printBedSize.y;
      SetPrintableExts(sizeProfile.printBedSize.x, sizeProfile.printBedSize.y, 110f);
      PrinterCenter = new M3D.Model.Utils.Vector3(sizeProfile.HomeLocation.x, sizeProfile.HomeLocation.y, 0.0f);
      var num = PrinterModel.GUICaseSize / sizeProfile.shell_size.z;
      PrinterTransformation.Scale = new M3D.Model.Utils.Vector3(num, num, num);
      PrinterBoundsConst = sizeProfile;
      CreateBoundsGeometry((Element3D)ObjectToPrinterSpace);
    }

    public List<ModelTransformPair.Data> GetModelDataList()
    {
      var dataList = new List<ModelTransformPair.Data>();
      foreach (ModelTransformPair model in ModelList)
      {
        dataList.Add(model.data);
      }

      return dataList;
    }

    public ModelTransformPair GetModel(int index)
    {
      return ModelList[index];
    }

    public List<ModelTransformPair> GetAllModels()
    {
      return new List<ModelTransformPair>((IEnumerable<ModelTransformPair>)ModelList);
    }

    public Matrix4 CreateViewMatrix()
    {
      return ObjectToPrinterSpace.GetTransformationMatrix() * PrinterTransformation.GetTransformationMatrix() * PrinterTiltTransform.GetTransformationMatrix() * OpenGLCoordinateSystem.GetTransformationMatrix();
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
