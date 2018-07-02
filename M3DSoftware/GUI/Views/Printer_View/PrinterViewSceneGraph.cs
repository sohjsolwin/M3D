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
      AddChildElement3D(new LightNode(0, 0, new Vector4(0.0f, 400f, 1000f, 1f), new Color4(0.2f, 0.2f, 0.2f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(1f, 1f, 1f, 1f)));
      AddChildElement3D(new LightNode(1, 1, new Vector4(0.0f, 20f, -400f, 1f), new Color4(0.0f, 0.0f, 0.0f, 1f), new Color4(1f, 1f, 1f, 1f), new Color4(0.3f, 0.3f, 0.3f, 1f)));
      OpenGLCoordinateSystem = new TransformationNode(0, origin)
      {
        Rotation = new M3D.Model.Utils.Vector3(-90f, 0.0f, 0.0f)
      };
      AddChildElement3D(OpenGLCoordinateSystem);
      PrinterTiltTransform = new TransformationNode(0, OpenGLCoordinateSystem);
      OpenGLCoordinateSystem.AddChildElement(PrinterTiltTransform);
      PrinterTransformation = new TransformationNode(0, PrinterTiltTransform);
      PrinterTypeAdjustments = new TransformationNode(3011, PrinterTransformation);
      var num = 1.1f;
      PrinterTypeAdjustments.Scale = new M3D.Model.Utils.Vector3(num, num, 1f);
      PrinterTransformation.AddChildElement(PrinterTypeAdjustments);
      PrinterModel = new PrinterModelNode(initialCaseType);
      PrinterTypeAdjustments.AddChildElement(PrinterModel);
      ObjectToPrinterSpace = new TransformationNode(3005, PrinterTransformation)
      {
        Translation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f)
      };
      PrinterTransformation.AddChildElement(ObjectToPrinterSpace);
      GridSizeAdjustments = new TransformationNode(3012, ObjectToPrinterSpace);
      ObjectToPrinterSpace.AddChildElement(GridSizeAdjustments);
      Grid = new GridObjectNode(3003, 100f, 100f);
      Grid.SetUnits(initialGridUnits);
      Grid.Visible = false;
      Grid.Emission = new Color4(1f, 1f, 1f, 1f);
      GridSizeAdjustments.AddChildElement(Grid);
      var texturedFloorNode = new TexturedFloorNode(3004);
      var bitmap = new Bitmap(Resources.shadowtexture);
      var texture = 0;
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      texturedFloorNode.Create(new M3D.Model.Utils.Vector3(0.0f, 0.0f, -76f), 450f, 240f, texture);
      OpenGLCoordinateSystem.AddChildElement(texturedFloorNode);
      PrinterTiltTransform.AddChildElement(PrinterTransformation);
    }

    private void CreateBoundsGeometry(Element3D parent)
    {
      lock (exceedsBoundsSync)
      {
        if (PrinterBoundsConst.PrintableRegion == null || PrinterBoundsConst.PrintableRegion.bounds_list.Count < 1)
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
              parent.RemoveChildElement(ExceedsBoundsGeometry[index1, index2]);
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
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_1.Z)));
          ExceedsBoundsGeometry[index, 0].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 0]);
          parent.AddChildElement(ExceedsBoundsGeometry[index, 0]);
          ExceedsBoundsGeometry[index, 1] = new CustomShape(3007);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, -1f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_1.Z)));
          ExceedsBoundsGeometry[index, 1].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 1]);
          parent.AddChildElement(ExceedsBoundsGeometry[index, 1]);
          ExceedsBoundsGeometry[index, 2] = new CustomShape(3008);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_1.Z)));
          ExceedsBoundsGeometry[index, 2].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 2]);
          parent.AddChildElement(ExceedsBoundsGeometry[index, 2]);
          ExceedsBoundsGeometry[index, 3] = new CustomShape(3009);
          vertex_list.Clear();
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_2.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_1.Z)));
          vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(-1f, 0.0f, 0.0f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_1.Z)));
          ExceedsBoundsGeometry[index, 3].Create(vertex_list, 0);
          BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 3]);
          parent.AddChildElement(ExceedsBoundsGeometry[index, 3]);
          if (index + 1 == boundsList.Count)
          {
            ExceedsBoundsGeometry[index, 4] = new CustomShape(3010);
            vertex_list.Clear();
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_2.Z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_1.Y, vector3_2.Z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_2.Z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.X, vector3_2.Y, vector3_2.Z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_2.Y, vector3_2.Z)));
            vertex_list.Add(new VertexTNV(new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.X, vector3_1.Y, vector3_2.Z)));
            ExceedsBoundsGeometry[index, 4].Create(vertex_list, 0);
            BoundingBoxColoringHelper(ref ExceedsBoundsGeometry[index, 4]);
            parent.AddChildElement(ExceedsBoundsGeometry[index, 4]);
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
      modtrans_pair.transformNode = new TransformationNode(3100, ObjectToPrinterSpace);
      modtrans_pair.transformNode.AddChildElement(modtrans_pair.modelNode);
      modtrans_pair.CalculateExtents();
      ObjectToPrinterSpace.AddChildElement(modtrans_pair.transformNode);
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
        var num3 = (float) (1.0 + 0.25 * index);
        var num4 = (float)((PrinterBedWidth * (double)num3 - modelsize.X) / 2.0);
        var num5 = (float)((PrinterBedLength * (double)num3 - modelsize.Y) / 2.0);
        var num6 = (float)(PrinterCenter.X + (double)num4 - 2.0);
        var num7 = (float)(PrinterCenter.Y + (double)num5 - 2.0);
        var num8 = (float)(PrinterCenter.X - (double)num4 + 2.0);
        var num9 = (float)(PrinterCenter.Y - (double)num5 + 2.0);
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
          position.X = PrinterCenter.X;
          position.Y = PrinterCenter.Y;
          while (!CheckIsPositionFree(position, modelsize))
          {
            position.X += num11 * num1;
            position.Y += num12 * num2;
            if (position.X >= (double)num6 || position.Y >= (double)num7 || (position.X <= (double)num8 || position.Y <= (double)num9))
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
      var boundingBox = new BoundingBox(new Vector3D(position.X - modelsize.X / 2f, position.Y - modelsize.Y / 2f, 0.0f), new Vector3D(position.X + modelsize.X / 2f, position.Y + modelsize.Y / 2f, 0.0f));
      var flag = true;
      foreach (ModelTransformPair model in ModelList)
      {
        var x = model.transformNode.Translation.X;
        var y = model.transformNode.Translation.Y;
        var num1 = model.modelSize.Ext.X / 2f;
        var num2 = model.modelSize.Ext.Y / 2f;
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

      ObjectToPrinterSpace.RemoveChildElement(ModelList[index].transformNode);
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
      var vector3 = new M3D.Model.Utils.Vector3(0.0f, 0.0f, (float) (-model.modelSize.Min.Z + 1.0));
      model.transformNode.Translation.Z += vector3.Z;
      model.CalculateExtents();
    }

    public void ResetPrinterTransformation()
    {
      PrinterTransformation.Translation.X = 0.0f;
      PrinterTransformation.Translation.Y = 0.0f;
      PrinterTransformation.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
      PrinterTiltTransform.Rotation = new M3D.Model.Utils.Vector3(0.0f, 0.0f, 0.0f);
    }

    public void TiltPrinter(float tilt)
    {
      PrinterTiltTransform.Rotation.X += tilt;
      if (PrinterTiltTransform.Rotation.X > 90.0)
      {
        PrinterTiltTransform.Rotation.X = 90f;
      }
      else
      {
        if (PrinterTiltTransform.Rotation.X >= 0.0)
        {
          return;
        }

        PrinterTiltTransform.Rotation.X = 0.0f;
      }
    }

    public void RotatePrinter(float rotVelocity)
    {
      PrinterTransformation.Rotation.Z += rotVelocity;
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
            if (vector3.Z >= (double)printableRegion.bounds_list[index].min.z && (index >= count - 1 || vector3.Z <= (double)printableRegion.bounds_list[index].max.z))
            {
              var num = printableRegion.bounds_list[index].OutOfBoundsCheck(vector3.X, vector3.Y, vector3.Z);
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
      PrinterTypeAdjustments.Scale.X = sizeProfile.shell_size.x / ext.X;
      PrinterTypeAdjustments.Scale.Y = sizeProfile.shell_size.y / ext.Y;
      PrinterTypeAdjustments.Scale.Z = sizeProfile.shell_size.z / ext.X;
      PrinterTransformation.Translation.Z = PrinterModel.ZOffset;
      ObjectToPrinterSpace.Translation.X = sizeProfile.printBedSize.x / -2f;
      ObjectToPrinterSpace.Translation.Y = sizeProfile.printBedSize.y / -2f;
      ObjectToPrinterSpace.Translation.Z = (float)(sizeProfile.shell_size.z / -2.0 + sizeProfile.fluff_height * (double)PrinterTypeAdjustments.Scale.Z);
      GridSizeAdjustments.Scale.X = sizeProfile.printBedSize.x / 100f;
      GridSizeAdjustments.Scale.Y = sizeProfile.printBedSize.y / 100f;
      PrinterBedWidth = sizeProfile.printBedSize.x;
      PrinterBedLength = sizeProfile.printBedSize.y;
      SetPrintableExts(sizeProfile.printBedSize.x, sizeProfile.printBedSize.y, 110f);
      PrinterCenter = new M3D.Model.Utils.Vector3(sizeProfile.HomeLocation.x, sizeProfile.HomeLocation.y, 0.0f);
      var num = PrinterModel.GUICaseSize / sizeProfile.shell_size.z;
      PrinterTransformation.Scale = new M3D.Model.Utils.Vector3(num, num, num);
      PrinterBoundsConst = sizeProfile;
      CreateBoundsGeometry(ObjectToPrinterSpace);
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
      return new List<ModelTransformPair>(ModelList);
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
