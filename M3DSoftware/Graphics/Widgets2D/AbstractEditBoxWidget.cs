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
