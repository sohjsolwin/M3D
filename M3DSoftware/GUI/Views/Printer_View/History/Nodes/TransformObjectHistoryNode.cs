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
