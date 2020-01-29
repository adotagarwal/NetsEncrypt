
using Newtonsoft.Json;
using System;

namespace LetsEncryptClient
{
    static class Helpers
    {
        /// <summary>
        /// Output shim to allow interop with Linqpad
        /// </summary>
        /// <param name="o"></param>
        public static void Dump(this object o)
        {
            Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}
