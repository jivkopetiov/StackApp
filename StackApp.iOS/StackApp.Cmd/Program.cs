using StackApp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StackApp.Cmd
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceProxy.InitSite("stackoverflow");
        }
    }
}
