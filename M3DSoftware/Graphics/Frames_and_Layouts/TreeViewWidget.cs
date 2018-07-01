// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.TreeViewWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using System.Collections.Generic;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class TreeViewWidget : ScrollableVerticalLayout
  {
    public TreeViewWidget()
      : this(0, (Element2D) null)
    {
    }

    public TreeViewWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public TreeViewWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      foreach (Element2D child in (IEnumerable<Element2D>) this.ScollableChildframe.ChildList)
      {
        if (child is TreeNodeWidget)
          ((TreeNodeWidget) child).TopLevel = true;
      }
    }
  }
}
