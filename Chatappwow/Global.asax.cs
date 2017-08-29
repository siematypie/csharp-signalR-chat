using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;

namespace Chatappwow
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
//            DbAdder.init();
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(6);
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
