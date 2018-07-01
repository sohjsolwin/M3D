// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.IElement
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

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
      this._ID = ID;
      this.IParent = parent;
      this._bEnabled = true;
      this._bVisible = true;
      this.group = -1;
    }

    protected void SetBaseParent(IElement parent)
    {
      this.IParent = parent;
    }

    public virtual void Refresh()
    {
    }

    public virtual void OnRender()
    {
      if (this.DoOnRender == null)
        return;
      this.DoOnRender();
    }

    public virtual void OnUpdate()
    {
      if (this.DoOnUpdate == null)
        return;
      this.DoOnUpdate();
    }

    protected virtual void OnHide()
    {
      if (this.DoOnHide == null)
        return;
      this.DoOnHide();
    }

    protected virtual void OnUnhide()
    {
      if (this.DoOnUnhide == null)
        return;
      this.DoOnUnhide();
    }

    public virtual void SetEnabled(bool bEnabled)
    {
      if (this._bEnabled == bEnabled)
        return;
      this._bEnabled = bEnabled;
    }

    public virtual void SetVisible(bool bVisible)
    {
      if (this._bVisible == bVisible)
        return;
      this._bVisible = bVisible;
      if (this._bVisible)
        this.OnUnhide();
      else
        this.OnHide();
    }

    [XmlAttribute("id")]
    public int ID
    {
      get
      {
        return this._ID;
      }
      set
      {
        this._ID = value;
      }
    }

    [XmlAttribute("enabled")]
    public virtual bool Enabled
    {
      get
      {
        if (this.IParent == null)
        {
          if (IElement.HasEnabledOnlyItem || !this._bEnabled)
            return IElement.InEnabledOnlyList(this);
          return true;
        }
        if (!this._bEnabled || !this.IParent.Enabled)
          return IElement.InEnabledOnlyList(this);
        return true;
      }
      set
      {
        this.SetEnabled(value);
      }
    }

    [XmlAttribute("visible")]
    public bool Visible
    {
      get
      {
        if (!this._bVisible)
          return false;
        if (this.IParent == null)
          return true;
        return this.IParent.Visible;
      }
      set
      {
        this.SetVisible(value);
      }
    }

    [XmlIgnore]
    public bool SelfIsVisible
    {
      get
      {
        return this._bVisible;
      }
    }

    [XmlAttribute("group")]
    public int GroupID
    {
      get
      {
        return this.group;
      }
      set
      {
        this.group = value;
      }
    }

    [XmlAttribute("tooltip")]
    public string ToolTipMessage
    {
      get
      {
        return this.toolTipMessage;
      }
      set
      {
        if (value.StartsWith("T_"))
          this.toolTipMessage = Locale.GlobalLocale.T(value);
        else
          this.toolTipMessage = value;
      }
    }

    public static Color4 GenerateColorFromHtml(string hexColor)
    {
      Color4 color4 = new Color4();
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
        return IElement.EnabledOnlyList.Contains(element);
    }

    public static bool HasEnabledOnlyItem
    {
      get
      {
        lock (IElement.EnabledOnlyList)
          return IElement.EnabledOnlyList.Count > 0;
      }
    }

    public static void AddEnabledOnlyItem(IElement element)
    {
      lock (IElement.EnabledOnlyList)
      {
        if (IElement.EnabledOnlyList.Contains(element))
          return;
        IElement.EnabledOnlyList.Add(element);
      }
    }

    public static void RemoveEnabledOnlyItem(IElement element)
    {
      lock (IElement.EnabledOnlyList)
      {
        if (!IElement.EnabledOnlyList.Contains(element))
          return;
        IElement.EnabledOnlyList.Remove(element);
      }
    }

    public static void ClearEnabledOnlyList()
    {
      lock (IElement.EnabledOnlyList)
        IElement.EnabledOnlyList.Clear();
    }
  }
}
