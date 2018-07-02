// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.History.Nodes.TransformObjectHistoryNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;

namespace M3D.GUI.Views.Printer_View.History.Nodes
{
  internal class TransformObjectHistoryNode : HistoryNode
  {
    private TransformationNode.Transform previousTransform;
    private TransformationNode.Transform newTransform;

    public TransformObjectHistoryNode(uint objectID, TransformationNode.Transform previousTransform, TransformationNode.Transform newTransform)
      : base(objectID)
    {
      this.previousTransform = previousTransform;
      this.newTransform = newTransform;
    }

    public override void Undo(PrinterView printerView)
    {
      ModelTransformPair modelById = printerView.GetModelByID(ObjectID);
      if (modelById == null)
      {
        return;
      }

      modelById.transformNode.TransformData = previousTransform;
      printerView.PushUpdatedInfomation(modelById);
    }

    public override void Redo(PrinterView printerView)
    {
      ModelTransformPair modelById = printerView.GetModelByID(ObjectID);
      if (modelById == null)
      {
        return;
      }

      modelById.transformNode.TransformData = newTransform;
      printerView.PushUpdatedInfomation(modelById);
    }
  }
}
