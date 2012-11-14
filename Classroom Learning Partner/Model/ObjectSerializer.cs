using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Classroom_Learning_Partner.Model
{
    public static class ObjectSerializer
    {
        public static string ToString(object obj)
        {
            BinaryFormatter binFormat = new BinaryFormatter();

            using (MemoryStream mStream = new MemoryStream())
            {
                binFormat.Serialize(mStream, obj);
                string s = Convert.ToBase64String(mStream.ToArray());
                return s;
            }
        }

        public static object ToObject(string s)
        {
            BinaryFormatter binFormat = new BinaryFormatter();
            byte[] byteArray = Convert.FromBase64String(s);
            
            using (MemoryStream mStream = new MemoryStream(byteArray))
            {
                object obj = binFormat.Deserialize(mStream);
                return obj;
            }       
        }
    }
}
