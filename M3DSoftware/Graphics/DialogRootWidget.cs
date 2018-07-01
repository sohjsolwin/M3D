// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.DialogRootWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;

namespace M3D.Graphics
{
  internal class DialogRootWidget : Element2D
  {
    public override void OnRender(GUIHost host)
    {
      Element2D element2D = this.ChildList.Last();
      if (element2D == null)
        return;
      base.OnRender(host);
      element2D.OnRender(host);
    }

    public override void AddChildElement(Element2D child)
    {
      base.AddChildElement(child);
      Frame frame = child as Frame;
      if (frame != null)
        frame.DarkenBackground = true;
      this.Refresh();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      Element2D element2D = this.ChildList.Last();
      if (element2D != null)
        return element2D.OnKeyboardEvent(keyboardevent);
      return false;
    }

    public override void OnMouseLeave()
    {
      this.ChildList.Last()?.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      Element2D element2D = this.ChildList.Last();
      if (element2D != null)
        return element2D.OnMouseCommand(mouseevent);
      return false;
    }

    public override void OnMouseMove(int x, int y)
    {
      this.ChildList.Last()?.OnMouseMove(x, y);
    }

    public override Element2D GetSelfOrDependantAtPoint(int x, int y)
    {
      Element2D element2D1 = this.ChildList.Last();
      Element2D element2D2 = (Element2D) null;
      if (element2D1 != null)
        element2D2 = element2D1.GetSelfOrDependantAtPoint(x, y);
      if (element2D2 == null)
        element2D2 = (Element2D) this;
      return element2D2;
    }

    public override bool Enabled
    {
      get
      {
        return this.ChildList.Count > 0;
      }
      set
      {
      }
    }
  }
}
