using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anticaptcha_example
{
    internal class PostDataGambi
    {
        public string task { get; set; }
        public int softId { get; set; }
        public string clientKey { get; set; }
    }
}
