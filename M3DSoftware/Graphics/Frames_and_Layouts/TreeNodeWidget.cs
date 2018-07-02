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
      : this(ID, null)
    {
    }

    public TreeNodeWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
      layoutMode = Layout.LayoutMode.ResizeLayoutToFitChildren;
    }

    public void Init(GUIHost host, string label, bool open)
    {
      BorderColor = new Color4(0.5f, 0.5f, 0.5f, 1f);
      BGColor = new Color4(1f, 1f, 1f, 1f);
      border_width = indent;
      CreateLabel(host);
      Label = label;
      labelButton.Checked = open;
      TopLevel = false;
    }

    public override void InitChildren(Element2D parent, GUIHost host, ButtonCallback MyButtonCallback)
    {
      base.InitChildren(parent, host, MyButtonCallback);
      Init(host, _label, false);
    }

    private void OnButtonClicked(ButtonWidget button)
    {
      button.Text = LabelButtonText;
      if (button.Checked)
      {
        labelButton.TextDownColor = new Color4(1f, 1f, 1f, 1f);
        BGColor = new Color4(97, 97, 97, byte.MaxValue);
        foreach (Element2D child in ChildList)
        {
          if (child != labelButton)
          {
            child.Visible = true;
            child.X = 20;
          }
        }
        if (Parent != null)
        {
          Parent.TurnOffGroup(GroupID, this);
        }
      }
      else
      {
        if (TopLevel)
        {
          labelButton.TextColor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
          labelButton.TextDownColor = new Color4(0.25f, 0.25f, 0.25f, 1f);
          BGColor = new Color4(1f, 1f, 1f, 1f);
        }
        foreach (Element2D child in ChildList)
        {
          if (child != labelButton)
          {
            child.Visible = false;
          }
        }
      }
      Refresh();
    }

    public override void Refresh()
    {
      base.Refresh();
      Parent.Refresh();
    }

    public override void SetOff()
    {
      labelButton.Checked = false;
    }

    private void CreateLabel(GUIHost host)
    {
      labelButton = new ButtonWidget(2001, this);
      labelButton.Init(host, ButtonTemplate.TextOnly);
      labelButton.SetCallback(new ButtonCallback(OnButtonClicked));
      labelButton.Height = label_height;
      labelButton.RelativeWidth = 1f;
      labelButton.Alignment = QFontAlignment.Left;
      labelButton.AlwaysHighlightOnMouseOver = true;
      labelButton.DontMove = true;
      labelButton.ClickType = ButtonType.Checkable;
      labelButton.CanClickOff = true;
      labelButton.Text = LabelButtonText;
      AddFirstChild(labelButton);
      Refresh();
    }

    [XmlAttribute("label")]
    public string Label
    {
      get
      {
        return _label;
      }
      set
      {
        _label = value;
        if (labelButton == null)
        {
          return;
        }

        labelButton.Text = LabelButtonText;
      }
    }

    [XmlAttribute("label-height")]
    public int LabelHeight
    {
      get
      {
        return label_height;
      }
      set
      {
        label_height = value;
        if (labelButton == null)
        {
          return;
        }

        labelButton.Height = label_height;
      }
    }

    private string LabelButtonText
    {
      get
      {
        if (labelButton != null && labelButton.Checked)
        {
          return OpenText;
        }

        return ClosedText;
      }
    }

    private string OpenText
    {
      get
      {
        return "-   " + _label;
      }
    }

    private string ClosedText
    {
      get
      {
        return "+   " + _label;
      }
    }

    [XmlIgnore]
    public bool TopLevel
    {
      get
      {
        return _topLevel;
      }
      set
      {
        _topLevel = value;
        if (_topLevel && !labelButton.Checked)
        {
          labelButton.TextColor = new Color4(0.1843137f, 0.3294118f, 0.345098f, 1f);
          labelButton.TextDownColor = new Color4(0.25f, 0.25f, 0.25f, 1f);
          BGColor = new Color4(1f, 1f, 1f, 1f);
        }
        else
        {
          labelButton.TextColor = new Color4(1f, 1f, 1f, 1f);
          labelButton.TextDownColor = new Color4(1f, 1f, 1f, 1f);
          BGColor = new Color4(97, 97, 97, byte.MaxValue);
        }
      }
    }

    private enum ControlIDs
    {
      TreeNodeWidgetButton = 2001, // 0x000007D1
    }
  }
}
