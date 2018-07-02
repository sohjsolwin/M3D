using M3D.Graphics.TextLocalization;
using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace M3D.Graphics
{
  public abstract class IElement
  {
    private static List<IElement> EnabledOnlyList = new List<IElement>();
    [XmlIgnore]
    protected string toolTipMessage = "";
    [XmlAttribute("tag")]
    public string tag;
    [XmlIgnore]
    public ElementStandardDelegate DoOnRender;
    [XmlIgnore]
    public ElementStandardDelegate DoOnUpdate;
    [XmlIgnore]
    public ElementStandardDelegate DoOnHide;
    [XmlIgnore]
    public ElementStandardDelegate DoOnUnhide;
    [XmlIgnore]
    private int _ID;
    [XmlIgnore]
    private bool _bEnabled;
    [XmlIgnore]
    private bool _bVisible;
    [XmlIgnore]
    private int group;
    [XmlIgnore]
    protected IElement IParent;

    public IElement(int ID, IElement parent)
    {
      _ID = ID;
      IParent = parent;
      _bEnabled = true;
      _bVisible = true;
      group = -1;
    }

    protected void SetBaseParent(IElement parent)
    {
      IParent = parent;
    }

    public virtual void Refresh()
    {
    }

    public virtual void OnRender()
    {
      if (DoOnRender == null)
      {
        return;
      }

      DoOnRender();
    }

    public virtual void OnUpdate()
    {
      if (DoOnUpdate == null)
      {
        return;
      }

      DoOnUpdate();
    }

    protected virtual void OnHide()
    {
      if (DoOnHide == null)
      {
        return;
      }

      DoOnHide();
    }

    protected virtual void OnUnhide()
    {
      if (DoOnUnhide == null)
      {
        return;
      }

      DoOnUnhide();
    }

    public virtual void SetEnabled(bool bEnabled)
    {
      if (_bEnabled == bEnabled)
      {
        return;
      }

      _bEnabled = bEnabled;
    }

    public virtual void SetVisible(bool bVisible)
    {
      if (_bVisible == bVisible)
      {
        return;
      }

      _bVisible = bVisible;
      if (_bVisible)
      {
        OnUnhide();
      }
      else
      {
        OnHide();
      }
    }

    [XmlAttribute("id")]
    public int ID
    {
      get
      {
        return _ID;
      }
      set
      {
        _ID = value;
      }
    }

    [XmlAttribute("enabled")]
    public virtual bool Enabled
    {
      get
      {
        if (IParent == null)
        {
          if (IElement.HasEnabledOnlyItem || !_bEnabled)
          {
            return IElement.InEnabledOnlyList(this);
          }

          return true;
        }
        if (!_bEnabled || !IParent.Enabled)
        {
          return IElement.InEnabledOnlyList(this);
        }

        return true;
      }
      set
      {
        SetEnabled(value);
      }
    }

    [XmlAttribute("visible")]
    public bool Visible
    {
      get
      {
        if (!_bVisible)
        {
          return false;
        }

        if (IParent == null)
        {
          return true;
        }

        return IParent.Visible;
      }
      set
      {
        SetVisible(value);
      }
    }

    [XmlIgnore]
    public bool SelfIsVisible
    {
      get
      {
        return _bVisible;
      }
    }

    [XmlAttribute("group")]
    public int GroupID
    {
      get
      {
        return group;
      }
      set
      {
        group = value;
      }
    }

    [XmlAttribute("tooltip")]
    public string ToolTipMessage
    {
      get
      {
        return toolTipMessage;
      }
      set
      {
        if (value.StartsWith("T_"))
        {
          toolTipMessage = Locale.GlobalLocale.T(value);
        }
        else
        {
          toolTipMessage = value;
        }
      }
    }

    public static Color4 GenerateColorFromHtml(string hexColor)
    {
      var color4 = new Color4();
      try
      {
        Color color = ColorTranslator.FromHtml(hexColor);
        return new Color4(color.R, color.G, color.B, color.A);
      }
      catch (Exception ex)
      {
        throw new Exception("GenerateColorFromHtml encountered a problem", ex);
      }
    }

    private static bool InEnabledOnlyList(IElement element)
    {
      lock (IElement.EnabledOnlyList)
      {
        return IElement.EnabledOnlyList.Contains(element);
      }
    }

    public static bool HasEnabledOnlyItem
    {
      get
      {
        lock (IElement.EnabledOnlyList)
        {
          return IElement.EnabledOnlyList.Count > 0;
        }
      }
    }

    public static void AddEnabledOnlyItem(IElement element)
    {
      lock (IElement.EnabledOnlyList)
      {
        if (IElement.EnabledOnlyList.Contains(element))
        {
          return;
        }

        IElement.EnabledOnlyList.Add(element);
      }
    }

    public static void RemoveEnabledOnlyItem(IElement element)
    {
      lock (IElement.EnabledOnlyList)
      {
        if (!IElement.EnabledOnlyList.Contains(element))
        {
          return;
        }

        IElement.EnabledOnlyList.Remove(element);
      }
    }

    public static void ClearEnabledOnlyList()
    {
      lock (IElement.EnabledOnlyList)
      {
        IElement.EnabledOnlyList.Clear();
      }
    }
  }
}
