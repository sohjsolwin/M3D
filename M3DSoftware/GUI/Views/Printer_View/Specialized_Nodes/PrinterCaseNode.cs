// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Specialized_Nodes.PrinterCaseNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.Graphics.Ext3D.ModelRendering;
using M3D.GUI.Controller;
using M3D.Spooling.Printer_Profiles;
using OpenTK.Graphics;
using System.IO;

namespace M3D.GUI.Views.Printer_View.Specialized_Nodes
{
  public abstract class PrinterCaseNode : Element3D
  {
    protected Color4 printerColor;
    protected Model3DNode PrinterModel;
    protected Model3DNode PrinterBed;
    protected PrinterLogoObjectNode PrinterLogo;

    public virtual Color4 PrinterColor
    {
      get
      {
        return printerColor;
      }
      set
      {
        printerColor = value;
        var color4 = new Color4(printerColor.R / 4f, printerColor.G / 4f, printerColor.B / 4f, 1f);
        if (PrinterModel != null)
        {
          PrinterModel.Ambient = color4;
          PrinterModel.Diffuse = printerColor;
        }
        if (PrinterBed != null)
        {
          PrinterBed.Ambient = color4;
          PrinterBed.Diffuse = printerColor;
        }
        if (PrinterLogo == null)
        {
          return;
        }

        PrinterLogo.Ambient = color4;
        PrinterLogo.Diffuse = printerColor;
      }
    }

    public abstract Model3DNode ShellModel { get; }

    public abstract PrinterSizeProfile.CaseType CaseType { get; }

    public abstract float ZOffset { get; }

    public abstract float GUICaseSize { get; }

    public M3D.Graphics.Ext3D.ModelRendering.Model LoadModel(string filename)
    {
      M3D.Graphics.Ext3D.ModelRendering.Model model = ModelLoadingManager.LoadModelFromFile(filename);
      if (model != null)
      {
        return model;
      }

      throw new FileLoadException();
    }

    public enum ElementIDs
    {
      PrinterModel = 3000, // 0x00000BB8
      PrinterBed = 3001, // 0x00000BB9
      PrinterLogo = 3002, // 0x00000BBA
    }
  }
}
