// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Specialized_Nodes.Micro1CaseNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this.PrinterModel = new Model3DNode(3000);
      this.PrinterModel.SetModel(this.LoadModel(Path.Combine(Paths.GUIObjectsFolder, "printer.m3d")));
      this.PrinterColor = new Color4(0.1686275f, 0.7529412f, 0.9529412f, 1f);
      this.PrinterModel.Shininess = 30f;
      this.PrinterModel.Ambient = new Color4(this.PrinterColor.R / 4f, this.PrinterColor.G / 4f, this.PrinterColor.B / 4f, 1f);
      this.PrinterModel.Diffuse = this.PrinterColor;
      this.PrinterModel.Specular = new Color4(0.4f, 0.4f, 0.4f, 1f);
      this.PrinterBed = new Model3DNode(3001);
      this.PrinterBed.SetModel(this.LoadModel(Path.Combine(Paths.GUIObjectsFolder, "bed.m3d")));
      this.PrinterBed.Shininess = 30f;
      this.PrinterBed.TranslateModelData(0.0f, 0.0f, -47.6f);
      this.PrinterBed.Ambient = new Color4(this.PrinterColor.R / 4f, this.PrinterColor.G / 4f, this.PrinterColor.B / 4f, 1f);
      this.PrinterBed.Diffuse = this.PrinterColor;
      this.PrinterBed.Specular = new Color4(0.4f, 0.4f, 0.4f, 1f);
      this.PrinterLogo = new PrinterLogoObjectNode(Path.Combine(Paths.GUIImagesFolder, "m3d.png"), 3002, new Vector3(0.0f, -75f, -60f), new Vector3(40f, 0.0f, 11f));
      this.AddChildElement((Element3D) this.PrinterLogo);
      this.AddChildElement((Element3D) this.PrinterModel);
      this.AddChildElement((Element3D) this.PrinterBed);
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
        return this.PrinterModel;
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
