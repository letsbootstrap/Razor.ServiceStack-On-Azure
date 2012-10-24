using SquishIt.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Website
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            new AppHost().Init();

            Application_Bundle();
        }

        protected void Application_Bundle()
        {
            Bundle.Css()
                    .Add("/Content/css/bootstrap.css")
                    .Add("/Content/css/bootstrap-responsive.css")
                    .Add("/Content/css/bootstrap-lightbox.css")
                    .Add("/Content/css/themes/original.css")
                    .Add("/Content/css/themes/libnotify.css")
                    .Add("/Content/css/themes/jackedup.css")
                    .Add("/Content/site.css")
                    .ForceRelease()
                    .WithMinifier(new SquishIt.Framework.Minifiers.CSS.NullMinifier())     // Ouput un-minified but in one file
                    .AsCached("stylebase", "~/files/css/codebase/" + ConfigurationManager.AppSettings["BuildVersion"].ToString());

            Bundle.JavaScript()
                    .Add("/Content/scripts/jquery-1.7.1.js")
                    .Add("/Content/scripts/bootstrap.js")
                    .Add("/Content/scripts/bootstrap-lightbox.js")
                    .Add("/Content/scripts/humane.js")
                    .Add("/Content/scripts/jquery.cookie.js")
                    .Add("/Content/site.js")
                    .ForceRelease()
                    .WithMinifier(new SquishIt.Framework.Minifiers.JavaScript.NullMinifier())     // Ouput un-minified but in one file
                    .AsCached("codebase", "~/files/js/codebase/" + ConfigurationManager.AppSettings["BuildVersion"].ToString());
        }
    }
}