// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.History.Nodes.SelectObjectHistoryNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
