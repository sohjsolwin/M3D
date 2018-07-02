// Decompiled with JetBrains decompiler
// Type: M3D.GUI.Views.Printer_View.Specialized_Nodes.GridObjectNode
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Ext3D;
using M3D.Graphics.Ext3D.ModelRendering;
using M3D.GUI.Controller.Settings;
using M3D.Properties;
using M3D.Spooling.Printer_Profiles;
using System.Collections.Generic;
using System.Drawing;

namespace M3D.GUI.Views.Printer_View.Specialized_Nodes
{
  public class GridObjectNode : CustomShape
  {
    private int[,] texture_handles;

    public GridObjectNode(int ID, float bedwidth, float bedheight)
      : base(ID, (Element3D) null)
    {
      texture_handles = new int[2, 2];
      texture_handles[0, 0] = CreateTexture(Resources.gridinchestexture_micro1);
      texture_handles[0, 1] = CreateTexture(Resources.gridmmtexture_micro1);
      texture_handles[1, 0] = CreateTexture(Resources.gridinchestexture_pro);
      texture_handles[1, 1] = CreateTexture(Resources.gridmmtexture_pro);
      var vertex_list = new List<VertexTNV>();
      var vector3_1 = new M3D.Model.Utils.Vector3(-6.6667f, -6.6667f, 0.0f);
      var vector3_2 = new M3D.Model.Utils.Vector3(100f, 100f, 0.0f);
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(0.0f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_2.y, vector3_2.z)));
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(0.0f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(1f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(1f, 0.0f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_2.y, vector3_2.z)));
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(0.0f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_1.x, vector3_1.y, vector3_2.z)));
      vertex_list.Add(new VertexTNV(new OpenTK.Vector2(1f, 1f), new M3D.Model.Utils.Vector3(0.0f, 0.0f, 1f), new M3D.Model.Utils.Vector3(vector3_2.x, vector3_1.y, vector3_2.z)));
      Create(vertex_list, texture_handles[0, 1]);
      CurrentCaseType = PrinterSizeProfile.CaseType.Micro1Case;
      CurrentUnits = SettingsManager.GridUnit.MM;
    }

    public void SetCaseType(PrinterSizeProfile.CaseType casetype)
    {
      if (CurrentCaseType == casetype)
      {
        return;
      }

      CurrentCaseType = casetype;
      UpdateTexture();
    }

    public void SetUnits(SettingsManager.GridUnit units)
    {
      if (CurrentUnits == units)
      {
        return;
      }

      CurrentUnits = units;
      UpdateTexture();
    }

    private int CreateTexture(Bitmap bitmap)
    {
      var texture = 0;
      Element3D.CreateTexture(ref texture, bitmap);
      bitmap.Dispose();
      return texture;
    }

    private void UpdateTexture()
    {
      TextureHandle = texture_handles[(int)CurrentCaseType, (int)CurrentUnits];
    }

    public PrinterSizeProfile.CaseType CurrentCaseType { get; private set; }

    public SettingsManager.GridUnit CurrentUnits { get; private set; }
  }
}
