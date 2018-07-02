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
      lock (threadSync)
      {
        undo_history.Clear();
        redo_history.Clear();
      }
    }

    public void PushAddModelFile(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
    {
      lock (threadSync)
      {
        if (!_recordHistory)
        {
          return;
        }

        PushToHistory(new AddModelFileHistoryNode(objectID, filename, zipfilename, transform));
      }
    }

    public void PushRemoveModelFile(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
    {
      lock (threadSync)
      {
        if (!_recordHistory)
        {
          return;
        }

        PushToHistory(new RemoveModelFileHistoryNode(objectID, filename, zipfilename, transform));
      }
    }

    public void PushSelectObject(uint objectID, uint previousObject)
    {
      lock (threadSync)
      {
        if (!_recordHistory)
        {
          return;
        }

        PushToHistory(new SelectObjectHistoryNode(objectID, previousObject));
      }
    }

    public void PushTransformObject(uint objectID, TransformationNode.Transform previousTransform, TransformationNode.Transform newTransform)
    {
      if (previousTransform.Equals(ref newTransform))
      {
        return;
      }

      lock (threadSync)
      {
        if (!_recordHistory)
        {
          return;
        }

        PushToHistory(new TransformObjectHistoryNode(objectID, previousTransform, newTransform));
      }
    }

    private void PushToHistory(HistoryNode item)
    {
      undo_history.Push(item);
      redo_history.Clear();
    }

    public bool Undo()
    {
      return UndoRedoHelper(undo_history, PrinterViewHistory.Action.Undo);
    }

    public bool Redo()
    {
      return UndoRedoHelper(redo_history, PrinterViewHistory.Action.Redo);
    }

    public bool CanUndo
    {
      get
      {
        if (undo_history.Count > 0)
        {
          return !printerView.ModelLoadingInterface.LoadingNewModel;
        }

        return false;
      }
    }

    public bool CanRedo
    {
      get
      {
        if (redo_history.Count > 0)
        {
          return !printerView.ModelLoadingInterface.LoadingNewModel;
        }

        return false;
      }
    }

    public bool RecordHistory
    {
      get
      {
        lock (threadSync)
        {
          return _recordHistory;
        }
      }
      set
      {
        lock (threadSync)
        {
          _recordHistory = value;
        }
      }
    }

    private bool UndoRedoHelper(Stack<HistoryNode> history, PrinterViewHistory.Action action)
    {
      lock (threadSync)
      {
        if (history.Count == 0)
        {
          return false;
        }

        var recordHistory = _recordHistory;
        _recordHistory = false;
        HistoryNode historyNode = history.Pop();
        switch (action)
        {
          case PrinterViewHistory.Action.Undo:
            historyNode.Undo(printerView);
            redo_history.Push(historyNode);
            break;
          case PrinterViewHistory.Action.Redo:
            historyNode.Redo(printerView);
            undo_history.Push(historyNode);
            break;
        }
        _recordHistory = recordHistory;
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
