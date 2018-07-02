using M3D.Graphics.Ext3D;

namespace M3D.GUI.Views.Printer_View.History.Nodes
{
  internal class AddModelFileHistoryNode : BaseModelFileHistoryNode
  {
    public AddModelFileHistoryNode(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
      : base(objectID, filename, zipfilename, transform)
    {
    }

    public override void Undo(PrinterView printerView)
    {
      RemoveModel(printerView);
    }

    public override void Redo(PrinterView printerView)
    {
      AddModel(printerView);
    }
  }
}
