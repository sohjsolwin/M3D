using M3D.Graphics.Ext3D;
using M3D.Model.Utils;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;
using System.IO;

namespace M3D.GUI.Views.Printer_View.Specialized_Nodes
{
  public class Micro1CaseNode : PrinterCaseNode
  {
    public Micro1CaseNode()
    {
      PrinterModel = new Model3DNode(3000);
      PrinterModel.SetModel(LoadModel(Path.Combine(Paths.GUIObjectsFolder, "printer.m3d")));
      PrinterColor = new Color4(0.1686275f, 0.7529412f, 0.9529412f, 1f);
      PrinterModel.Shininess = 30f;
      PrinterModel.Ambient = new Color4(PrinterColor.R / 4f, PrinterColor.G / 4f, PrinterColor.B / 4f, 1f);
      PrinterModel.Diffuse = PrinterColor;
      PrinterModel.Specular = new Color4(0.4f, 0.4f, 0.4f, 1f);
      PrinterBed = new Model3DNode(3001);
      PrinterBed.SetModel(LoadModel(Path.Combine(Paths.GUIObjectsFolder, "bed.m3d")));
      PrinterBed.Shininess = 30f;
      PrinterBed.TranslateModelData(0.0f, 0.0f, -47.6f);
      PrinterBed.Ambient = new Color4(PrinterColor.R / 4f, PrinterColor.G / 4f, PrinterColor.B / 4f, 1f);
      PrinterBed.Diffuse = PrinterColor;
      PrinterBed.Specular = new Color4(0.4f, 0.4f, 0.4f, 1f);
      PrinterLogo = new PrinterLogoObjectNode(Path.Combine(Paths.GUIImagesFolder, "m3d.png"), 3002, new Vector3(0.0f, -75f, -60f), new Vector3(40f, 0.0f, 11f));
      AddChildElement((Element3D)PrinterLogo);
      AddChildElement((Element3D)PrinterModel);
      AddChildElement((Element3D)PrinterBed);
    }

    public override PrinterSizeProfile.CaseType CaseType
    {
      get
      {
        return PrinterSizeProfile.CaseType.Micro1Case;
      }
    }

    public override Model3DNode ShellModel
    {
      get
      {
        return PrinterModel;
      }
    }

    public override float ZOffset
    {
      get
      {
        return 0.0f;
      }
    }

    public override float GUICaseSize
    {
      get
      {
        return 140f;
      }
    }
  }
}
