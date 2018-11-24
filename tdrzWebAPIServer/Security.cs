using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tdrzWebAPIServer
{
    public class Security
    {
        public static string Encode(string data) => qiyubrother.extend.Base64.EncodeString(data, Encoding.UTF8);
        public static string Decode(string data) => qiyubrother.extend.Base64.DecodeString(data, Encoding.UTF8);
    }
}
