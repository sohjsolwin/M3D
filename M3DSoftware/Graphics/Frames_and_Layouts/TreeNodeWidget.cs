// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Frames_and_Layouts.TreeNodeWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using M3D.Graphics.Widgets2D;
using OpenTK.Graphics;
using QuickFont;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace M3D.Graphics.Frames_and_Layouts
{
  public class TreeNodeWidget : VerticalLayout
  {
    [XmlAttribute("indent")]
    public int indent = 10;
    private int label_height;
    private string _label;
    private bool _topLevel;
    private ButtonWidget labelButton;

    public TreeNodeWidget()
      : this(0)
    {
    }

    public TreeNodeWidget(int ID)
      : this(ID, (Element2D) null)
    {
    }

    public TreeNodeWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      this.layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
    }

    public void Init(GUIHost host, string label, bool open)
    {
      this.BorderColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      this.BGColor = new Color4(1f, 1f, 1f, 1f);
      this.border_width = this.indent;
      this.CreateLabel(host);
      this.Label = label;
      this.labelButton.Checked = open;
      this.TopLevel = false;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      this.Init(host, this._label, false);
    }

    private void OnButtonClicked(ButtonWidget button)
    {
      button.Text = this.LabelButtonText;
      if (button.Checked)
      {
        this.labelButton.TextDownColor = new Color4(1f, 1f, 1f, 1f);
        this.BGColor = new Color4((byte) 97, (byte) 97, (byte) 97, byte.MaxValue);
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        {
          if (child != this.labelButton)
          {
            child.Visible = true;
            child.X = 20;
          }
        }
        if (this.Parent != null)
          this.Parent.TurnOffGroup(this.GroupID, (Element2D) this);
      }
      else
      {
        if (this.TopLevel)
        {
          this.labelButton.TextColor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
          this.labelButton.TextDownColor = new Color4(0.25f, 0.25f, 0.25f, 1f);
          this.BGColor = new Color4(1f, 1f, 1f, 1f);
        }
        foreach (Element2D child in (IEnumerable<Element2D>) this.ChildList)
        {
          if (child != this.labelButton)
            child.Visible = false;
        }
      }
      this.Refresh();
    }

    public override void Refresh()
    {
      base.Refresh();
      this.Parent.Refresh();
    }

    public override void SetOff()
    {
      this.labelButton.Checked = false;
    }

    private void CreateLabel(GUIHost host)
    {
      this.labelButton = new ButtonWidget(2001, (Element2D) this);
      this.labelButton.Init(host, ButtonTemplate.TextOnly);
      this.labelButton.SetCallback(new ButtonCallback(this.OnButtonClicked));
      this.labelButton.Height = this.label_height;
      this.labelButton.RelativeWidth = 1f;
      this.labelButton.Alignment = QFontAlignment.Left;
      this.labelButton.AlwaysHighlightOnMouseOver = true;
      this.labelButton.DontMove = true;
      this.labelButton.ClickType = ButtonType.Checkable;
      this.labelButton.CanClickOff = true;
      this.labelButton.Text = this.LabelButtonText;
      this.AddFirstChild((Element2D) this.labelButton);
      this.Refresh();
    }

    [XmlAttribute("label")]
    public string Label
    {
      get
      {
        return this._label;
      }
      set
      {
        this._label = value;
        if (this.labelButton == null)
          return;
        this.labelButton.Text = this.LabelButtonText;
      }
    }

    [XmlAttribute("label-height")]
    public int LabelHeight
    {
      get
      {
        return this.label_height;
      }
      set
      {
        this.label_height = value;
        if (this.labelButton == null)
          return;
        this.labelButton.Height = this.label_height;
      }
    }

    private string LabelButtonText
    {
      get
      {
        if (this.labelButton != null && this.labelButton.Checked)
          return this.OpenText;
        return this.ClosedText;
      }
    }

    private string OpenText
    {
      get
      {
        return "-   " + this._label;
      }
    }

    private string ClosedText
    {
      get
      {
        return "+   " + this._label;
      }
    }

    [XmlIgnore]
    public bool TopLevel
    {
      get
      {
        return this._topLevel;
      }
      set
      {
        this._topLevel = value;
        if (this._topLevel && !this.labelButton.Checked)
        {
          this.labelButton.TextColor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
          this.labelButton.TextDownColor = new Color4(0.25f, 0.25f, 0.25f, 1f);
          this.BGColor = new Color4(1f, 1f, 1f, 1f);
        }
        else
        {
          this.labelButton.TextColor = new Color4(1f, 1f, 1f, 1f);
          this.labelButton.TextDownColor = new Color4(1f, 1f, 1f, 1f);
          this.BGColor = new Color4((byte) 97, (byte) 97, (byte) 97, byte.MaxValue);
        }
      }
    }

    private enum ControlIDs
    {
      TreeNodeWidgetButton = 2001, // 0x000007D1
    }
  }
}
