
using Newtonsoft.Json;
using System;

namespace LetsEncryptClient
{
    static class Helpers
    {
        public static void Dump(this object o)
        {
            Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}
