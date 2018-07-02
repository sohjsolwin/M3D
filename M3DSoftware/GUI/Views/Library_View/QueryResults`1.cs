using System.Collections.Generic;

namespace M3D.GUI.Views.Library_View
{
  public class QueryResults<T> where T : LibraryRecord
  {
    public List<T> records;

    public QueryResults()
    {
      records = new List<T>();
    }
  }
}
