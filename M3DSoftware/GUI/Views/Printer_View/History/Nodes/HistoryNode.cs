namespace M3D.GUI.Views.Printer_View.History.Nodes
{
  internal abstract class HistoryNode
  {
    private uint objectID;

    public HistoryNode(uint objectID)
    {
      this.objectID = objectID;
    }

    public abstract void Undo(PrinterView printerView);

    public abstract void Redo(PrinterView printerView);

    public uint ObjectID
    {
      get
      {
        return objectID;
      }
    }
  }
}
