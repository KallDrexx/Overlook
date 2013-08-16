using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace Overlook.Server.WebModules
{
    public class TestModule : NancyModule
    {
        public TestModule()
        {
            Get["/"] = parameters => "Hello World";
        }
    }
}
