using M3D.Graphics.Frames_and_Layouts;
using M3D.Graphics.Widgets2D;

namespace M3D.Graphics
{
  internal class DialogRootWidget : Element2D
  {
    public override void OnRender(GUIHost host)
    {
      Element2D element2D = ChildList.Last();
      if (element2D == null)
      {
        return;
      }

      base.OnRender(host);
      element2D.OnRender(host);
    }

    public override void AddChildElement(Element2D child)
    {
      base.AddChildElement(child);
      if (child is Frame frame)
      {
        frame.DarkenBackground = true;
      }

      Refresh();
    }

    public override bool OnKeyboardEvent(KeyboardEvent keyboardevent)
    {
      Element2D element2D = ChildList.Last();
      if (element2D != null)
      {
        return element2D.OnKeyboardEvent(keyboardevent);
      }

      return false;
    }

    public override void OnMouseLeave()
    {
      ChildList.Last()?.OnMouseLeave();
    }

    public override bool OnMouseCommand(MouseEvent mouseevent)
    {
      Element2D element2D = ChildList.Last();
      if (element2D != null)
      {
        return element2D.OnMouseCommand(mouseevent);
      }

      return false;
    }

    public override void OnMouseMove(int x, int y)
    {
      ChildList.Last()?.OnMouseMove(x, y);
    }

    public override Element2D GetSelfOrDependantAtPoint(int x, int y)
    {
      Element2D element2D1 = ChildList.Last();
      var element2D2 = (Element2D) null;
      if (element2D1 != null)
      {
        element2D2 = element2D1.GetSelfOrDependantAtPoint(x, y);
      }

      if (element2D2 == null)
      {
        element2D2 = (this);
      }

      return element2D2;
    }

    public override bool Enabled
    {
      get
      {
        return ChildList.Count > 0;
      }
      set
      {
      }
    }
  }
}
