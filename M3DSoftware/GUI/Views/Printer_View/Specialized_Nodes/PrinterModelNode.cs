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
      Micro1Case = new Micro1CaseNode();
      ProCase = new ProCaseNode();
      Micro1Case.Visible = false;
      ProCase.Visible = false;
      AddChildElement(Micro1Case);
      AddChildElement(ProCase);
      SetCase(casetype);
    }

    public void SetCase(PrinterSizeProfile.CaseType casetype)
    {
      if (current != null && casetype == current.CaseType)
      {
        return;
      }

      if (current != null)
      {
        current.Visible = false;
      }

      switch (casetype)
      {
        case PrinterSizeProfile.CaseType.Micro1Case:
          current = Micro1Case;
          break;
        case PrinterSizeProfile.CaseType.ProCase:
          current = ProCase;
          break;
      }
      current.Visible = true;
    }

    public override Color4 PrinterColor
    {
      get
      {
        return current.PrinterColor;
      }
      set
      {
        Micro1Case.PrinterColor = value;
        ProCase.PrinterColor = value;
      }
    }

    public override Model3DNode ShellModel
    {
      get
      {
        return current.ShellModel;
      }
    }

    public override PrinterSizeProfile.CaseType CaseType
    {
      get
      {
        return current.CaseType;
      }
    }

    public override float ZOffset
    {
      get
      {
        return current.ZOffset;
      }
    }

    public override float GUICaseSize
    {
      get
      {
        return current.GUICaseSize;
      }
    }
  }
}
