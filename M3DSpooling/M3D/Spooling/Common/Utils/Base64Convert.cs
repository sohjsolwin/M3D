using System;
using System.Text;

namespace M3D.Spooling.Common.Utils
{
  public class Base64Convert
  {
    public static string Base64Decode(string base64EncodedData)
    {
      return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
    }

    public static string Base64Encode(string plainText)
    {
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }
  }
}
