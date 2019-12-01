using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ManagerApp
{
    class Router_entry
    {
        public String Router_id;
        public EndPoint Router_address;

        public Router_entry(String a, EndPoint b) {
            Router_id = a;
            Router_address = b;
        }
    }


}
