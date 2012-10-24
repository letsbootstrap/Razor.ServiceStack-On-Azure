using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Common;
using Website.Infrustructure.Utils;
using Website.Infrustructure.Models;
using Funq;
using ServiceStack.ServiceHost;
using Website.Infrustructure.DataSource;
using ServiceStack.Common.Web;

namespace Website.Infrustructure.Authentication
{
    public class CustomUserSession : AuthUserSession
    {
        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            //Fill the IAuthSession with data which you want to retrieve in the app eg:
            //Resolve the DbFactory from the IOC and persist the user info
            var DbFactory = authService.TryResolve<IDbConnectionFactory>();
            var user = session.TranslateTo<User>();
            TimeSpan sessionExpiry = TimeSpan.FromDays(7 * 2); //2 weeks

            user.UserAuthId = int.Parse(session.UserAuthId);
            if (string.IsNullOrEmpty(user.UserName))
                user.UserName = user.Id.ToString();

            foreach (var authToken in session.ProviderOAuthAccess)
            {
                if (authToken.Provider == FacebookAuthProvider.Name)
                {
                    user.UserName = "fb-" + authToken.UserName.Replace("_", "-");
                    user.DisplayName = authToken.DisplayName;
                    user.FacebookName = authToken.DisplayName;
                    user.FacebookFirstName = authToken.FirstName;
                    user.FacebookLastName = authToken.LastName;
                    user.FacebookEmail = authToken.Email;
                    user.ProfileUrl = "http://www.facebook.com/" + authToken.UserName;
                    user.AvatarUrl = "http://graph.facebook.com/" + authToken.UserName + "/picture";
                }
                else if (authToken.Provider == TwitterAuthProvider.Name)
                {
                    user.UserName = "tw-" + authToken.UserName.Replace("_", "-");
                    user.TwitterName = authToken.DisplayName;
                    user.ProfileUrl = "https://twitter.com/" + authToken.UserName;

                    TwitterHelper twHelper = new TwitterHelper(authToken.AccessToken, authToken.AccessTokenSecret);
                    Twitterizer.TwitterUser twUser = twHelper.GetUser(authToken.UserName);
                    if (twUser != null)
                    {
                        user.AvatarUrl = twUser.ProfileImageLocation;
                        user.DisplayName = twUser.Name;
                    }
                }
            }

            var DbUser = DbFactory.Run(repo => repo.Select<User>(u => u.UserName == user.UserName));
            if (DbUser.Count == 0)
            {
                user.ApiKey = Guid.NewGuid().ToString().Replace("-", "");

                if (string.IsNullOrEmpty(user.UserName))
                    user.UserName = session.UserAuthName;
                if (string.IsNullOrEmpty(user.DisplayName))
                    user.DisplayName = session.DisplayName;
                if (string.IsNullOrEmpty(user.AvatarUrl))
                    user.AvatarUrl = "/Content/Images/avatar.jpg";
                if (string.IsNullOrEmpty(user.ProfileUrl))
                    user.ProfileUrl = "/Users/View/" + user.UserAuthId.ToString();

                DbFactory.Run(repo => repo.Insert<User>(user));
            }
            else
            {
                var u = DbUser[0];
                if (!string.IsNullOrEmpty(user.DisplayName))
                    u.DisplayName = user.DisplayName;
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                    u.AvatarUrl = user.AvatarUrl;
                if (!string.IsNullOrEmpty(user.ProfileUrl))
                    u.ProfileUrl = user.ProfileUrl;

                DbFactory.Run(repo => repo.Update<User>(u));
            }

            var userAuth = DbFactory.Run(repo => repo.Select<UserAuth>(u => u.Id == user.UserAuthId && u.UserName == null));
            if (userAuth.Count > 0)
            {
                // A username is required in the UserAuth table so that a 
                // user can log in using the /auth/credentials endpoint:
                userAuth[0].UserName = user.UserName;
                DbFactory.Run(dbCmd => dbCmd.Save(userAuth[0]));
            }

            //Important: You need to save the session!
            session.UserName = user.UserName;
            authService.SaveSession(session, sessionExpiry);
        }

        public static void AutoSignIn(ref Container container, ref IHttpRequest req, ref IHttpResponse res)
        {
            string authSession = string.Empty;
            if (req.Cookies.ContainsKey("auth-session"))
            {
                if (string.IsNullOrEmpty(req.Cookies["auth-session"].Value))
                {
                    authSession = "0";
                    res.Cookies.DeleteCookie("auth-session");
                }
                else
                {
                    authSession = req.Cookies["auth-session"].Value;
                }
            }
            if (!req.GetSession().IsAuthenticated && !string.IsNullOrEmpty(authSession))
            {
                // Auto sign-in the user:
                var repository = container.Resolve<IRepository>();
                var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>();
                int userAuthId = int.Parse(authSession.Split('|')[0]);
                string apiKey = authSession.Split('|')[1];
                var users = repository.Find<User>(u => u.UserAuthId == userAuthId && u.ApiKey == apiKey);
                if (users.Count() > 0)
                {
                    UserAuth userAuth = authRepo.GetUserAuthByUserName(users[0].UserName);
                    CustomUserSession user = userAuth.TranslateTo<CustomUserSession>();
                    user.IsAuthenticated = true;
                    user.UserAuthId = users[0].UserAuthId.ToString();
                    req.SaveSession(user);
                }
            }
            else if (req.GetSession().IsAuthenticated && string.IsNullOrEmpty(authSession))
            {
                // Create the auto sign-in coookie:
                var repository = container.Resolve<IRepository>();
                int userAuthId = int.Parse(req.GetSession().UserAuthId);
                User user = repository.Find<User>(u => u.UserAuthId == userAuthId).FirstOrDefault();
                res.Cookies.AddPermanentCookie("auth-session", req.GetSession().UserAuthId + "|" + user.ApiKey);
            }
        }

        public static void ApiSignIn(ref Container container, ref IHttpRequest req, ref IHttpResponse res)
        {
            if (req.QueryString["apikey"] != null)
            {
                var repository = container.Resolve<IRepository>();
                var authRepo = (OrmLiteAuthRepository)container.Resolve<IUserAuthRepository>();
                int userAuthId = int.Parse(req.QueryString["apikey"].Split('|')[0]);
                string apiKey = req.QueryString["apikey"].Split('|')[1];
                var users = repository.Find<User>(u => u.UserAuthId == userAuthId && u.ApiKey == apiKey);
                if (users.Count() > 0)
                {
                    UserAuth userAuth = authRepo.GetUserAuthByUserName(users[0].UserName);
                    CustomUserSession user = userAuth.TranslateTo<CustomUserSession>();
                    user.IsAuthenticated = true;
                    user.UserAuthId = users[0].UserAuthId.ToString();
                    req.SaveSession(user);
                }
            }
        }
    }
}