using System;
using System.Xml.Serialization;

namespace M3D.SlicerConnectionCura.SlicerSettingsItems
{
  [Serializable]
  public struct Range<T> where T : IComparable<T>
  {
    [XmlAttribute]
    public T min;
    [XmlAttribute]
    public T max;

    public Range(T min, T max)
    {
      this.min = min;
      this.max = max;
    }
  }
}
