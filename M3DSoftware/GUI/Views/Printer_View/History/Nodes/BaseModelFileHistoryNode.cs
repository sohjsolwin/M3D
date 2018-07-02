// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.History.Nodes.BaseModelFileHistoryNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.GUI.Controller.Settings;

namespace M3D.GUI.Views.Printer_View.History.Nodes
{
  internal abstract class BaseModelFileHistoryNode : HistoryNode
  {
    private string filename;
    private string zipfilename;
    private TransformationNode.Transform transform;

    public BaseModelFileHistoryNode(uint objectID, string filename, string zipfilename, TransformationNode.Transform transform)
      : base(objectID)
    {
      this.filename = filename;
      this.zipfilename = zipfilename;
      this.transform = transform;
    }

    protected void AddModel(PrinterView printerView)
    {
      printerView.ModelLoadingInterface.LoadModelIntoPrinter(new PrintDetails.ObjectDetails(filename, new PrintDetails.Transform(transform.translation, transform.scaling, transform.rotation))
      {
        UID = ObjectID,
        zipFileName = zipfilename
      });
    }

    protected void RemoveModel(PrinterView printerView)
    {
      if (!printerView.SelectModelbyID(ObjectID))
      {
        return;
      }

      printerView.RemoveSelectedModel();
    }
  }
}
