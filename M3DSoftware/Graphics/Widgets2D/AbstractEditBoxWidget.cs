// Decompiled with JetBrains decompiler
// Type: M3D.Graphics.Widgets2D.AbstractEditBoxWidget
// Assembly: M3DGUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 1F16290A-C81C-448C-AD40-1D1E8ABC54ED
// Assembly location: C:\Program Files (x86)\M3D - Software\2017.12.18.1.8.3.0\M3DSoftware.exe

using System.Xml.Serialization;

namespace M3D.Graphics.Widgets2D
{
  public abstract class AbstractEditBoxWidget : Element2D, IDataAccessable
  {
    public AbstractEditBoxWidget(int ID, Element2D parent)
      : base(ID, parent)
    {
    }

    public override ElementType GetElementType()
    {
      return ElementType.EditBoxWidget;
    }

    [XmlText]
    public abstract string Text { get; set; }

    [XmlIgnore]
    public object Value
    {
      get
      {
        return (object)Text;
      }
      set
      {
        Text = value.ToString();
      }
    }
  }
}
