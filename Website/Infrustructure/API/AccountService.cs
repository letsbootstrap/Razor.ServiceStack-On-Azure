using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Website.Infrustructure.DataSource;
using Website.Infrustructure.ViewModels;

namespace Website.Infrustructure.API
{
    [Route("/account/status")]
    public class AccountStatus
    {
    }

    public class AccountStatusResponse
    {
        public IAuthSession session { get; set; }
    }

    public class AccountService : Service
    {
        public IRepository Repository { get; set; }

        public object Get(AccountStatus request)
        {
            return this.Request.GetSession();
        }
    }
}