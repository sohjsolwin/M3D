// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Specialized_Nodes.PrinterModelNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;

namespace M3D.GUI.Views.Printer_View.Specialized_Nodes
{
  internal class PrinterModelNode : PrinterCaseNode
  {
    private PrinterCaseNode current;
    private Micro1CaseNode Micro1Case;
    private ProCaseNode ProCase;

    public PrinterModelNode(PrinterSizeProfile.CaseType casetype)
    {
      this.Micro1Case = new Micro1CaseNode();
      this.ProCase = new ProCaseNode();
      this.Micro1Case.Visible = false;
      this.ProCase.Visible = false;
      this.AddChildElement((Element3D) this.Micro1Case);
      this.AddChildElement((Element3D) this.ProCase);
      this.SetCase(casetype);
    }

    public void SetCase(PrinterSizeProfile.CaseType casetype)
    {
      if (this.current != null && casetype == this.current.CaseType)
        return;
      if (this.current != null)
        this.current.Visible = false;
      switch (casetype)
      {
        case PrinterSizeProfile.CaseType.Micro1Case:
          this.current = (PrinterCaseNode) this.Micro1Case;
          break;
        case PrinterSizeProfile.CaseType.ProCase:
          this.current = (PrinterCaseNode) this.ProCase;
          break;
      }
      this.current.Visible = true;
    }

    public override Color4 PrinterColor
    {
      get
      {
        return this.current.PrinterColor;
      }
      set
      {
        this.Micro1Case.PrinterColor = value;
        this.ProCase.PrinterColor = value;
      }
    }

    public override Model3DNode ShellModel
    {
      get
      {
        return this.current.ShellModel;
      }
    }

    public override PrinterSizeProfile.CaseType CaseType
    {
      get
      {
        return this.current.CaseType;
      }
    }

    public override float ZOffset
    {
      get
      {
        return this.current.ZOffset;
      }
    }

    public override float GUICaseSize
    {
      get
      {
        return this.current.GUICaseSize;
      }
    }
  }
}
