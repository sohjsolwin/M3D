namespace M3D.GUI.Views.Printer_View.History.Nodes
{
  internal class SelectObjectHistoryNode : HistoryNode
  {
    private uint previousObject;

    public SelectObjectHistoryNode(uint objectID, uint previousObject)
      : base(objectID)
    {
      this.previousObject = previousObject;
    }

    public override void Undo(PrinterView printerView)
    {
      printerView.SelectModelbyID(previousObject);
    }

    public override void Redo(PrinterView printerView)
    {
      printerView.SelectModelbyID(ObjectID);
    }
  }
}
