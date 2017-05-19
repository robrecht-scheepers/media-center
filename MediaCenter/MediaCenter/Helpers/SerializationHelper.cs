using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MediaCenter.Helpers
{
    public class SerializationHelper
    {
        public static T Deserialize<T>(string jsonString)
        {
            T obj = default(T);
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof (T));
                obj = (T) ser.ReadObject(stream);
            }
            return obj;
        }

        public static string Serialize<T>(T t)
        {
            string jsonString;
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
                DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
                ds.WriteObject(stream, t);
                jsonString = Encoding.UTF8.GetString(stream.ToArray());
            }

            return jsonString;
        }
    }
}
