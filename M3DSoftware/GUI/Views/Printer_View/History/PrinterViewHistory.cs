// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.History.PrinterViewHistory
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.GUI.Views.Printer_View.History.Nodes;
using System.Collections.Generic;

namespace M3D.GUI.Views.Printer_View.History
{
  public class PrinterViewHistory
  {
    private Stack<HistoryNode> undo_history = new Stack<HistoryNode>();
    private Stack<HistoryNode> redo_history = new Stack<HistoryNode>();
    private object threadSync = new object();
    private bool _recordHistory = true;
    private PrinterView printerView;

    public PrinterViewHistory(PrinterView printerView)
    {
      this.printerView = printerView;
    }

    public void Clear()
    {
      lock (this.threadSync)
      {
        this.undo_history.Clear();
        this.redo_history.Clear();
      }
    }

    public void PushAddModelFile(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
    {
      lock (this.threadSync)
      {
        if (!this._recordHistory)
          return;
        this.PushToHistory((HistoryNode) new AddModelFileHistoryNode(objectID, filename, zipfilename, transform));
      }
    }

    public void PushRemoveModelFile(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
    {
      lock (this.threadSync)
      {
        if (!this._recordHistory)
          return;
        this.PushToHistory((HistoryNode) new RemoveModelFileHistoryNode(objectID, filename, zipfilename, transform));
      }
    }

    public void PushSelectObject(uint objectID, uint previousObject)
    {
      lock (this.threadSync)
      {
        if (!this._recordHistory)
          return;
        this.PushToHistory((HistoryNode) new SelectObjectHistoryNode(objectID, previousObject));
      }
    }

    public void PushTransformObject(uint objectID, TransformationNode.Transform previousTransform, TransformationNode.Transform newTransform)
    {
      if (previousTransform.Equals(ref newTransform))
        return;
      lock (this.threadSync)
      {
        if (!this._recordHistory)
          return;
        this.PushToHistory((HistoryNode) new TransformObjectHistoryNode(objectID, previousTransform, newTransform));
      }
    }

    private void PushToHistory(HistoryNode item)
    {
      this.undo_history.Push(item);
      this.redo_history.Clear();
    }

    public bool Undo()
    {
      return this.UndoRedoHelper(this.undo_history, PrinterViewHistory.Action.Undo);
    }

    public bool Redo()
    {
      return this.UndoRedoHelper(this.redo_history, PrinterViewHistory.Action.Redo);
    }

    public bool CanUndo
    {
      get
      {
        if (this.undo_history.Count > 0)
          return !this.printerView.ModelLoadingInterface.LoadingNewModel;
        return false;
      }
    }

    public bool CanRedo
    {
      get
      {
        if (this.redo_history.Count > 0)
          return !this.printerView.ModelLoadingInterface.LoadingNewModel;
        return false;
      }
    }

    public bool RecordHistory
    {
      get
      {
        lock (this.threadSync)
          return this._recordHistory;
      }
      set
      {
        lock (this.threadSync)
          this._recordHistory = value;
      }
    }

    private bool UndoRedoHelper(Stack<HistoryNode> history, PrinterViewHistory.Action action)
    {
      lock (this.threadSync)
      {
        if (history.Count == 0)
          return false;
        bool recordHistory = this._recordHistory;
        this._recordHistory = false;
        HistoryNode historyNode = history.Pop();
        switch (action)
        {
          case PrinterViewHistory.Action.Undo:
            historyNode.Undo(this.printerView);
            this.redo_history.Push(historyNode);
            break;
          case PrinterViewHistory.Action.Redo:
            historyNode.Redo(this.printerView);
            this.undo_history.Push(historyNode);
            break;
        }
        this._recordHistory = recordHistory;
        return true;
      }
    }

    private enum Action
    {
      Undo,
      Redo,
    }
  }
}
