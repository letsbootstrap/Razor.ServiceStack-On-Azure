using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web;
using Website.Infrustructure.DataSource;
using Website.Infrustructure.Models;
using Website.Infrustructure.ViewModels;

namespace Website.Infrustructure.API
{
    [Route("/assets")]
    [Route("/assets/tag/{Tag}")]
    [Route("/assets/{Id}")]
    public class Assets
    {
        public int? Id { get; set; }
        public string Tag { get; set; }
        public Asset Photo { get; set; }
    }

    public class AssetsResponse
    {
        public IAuthSession session { get; set; }
        public List<Asset> items { get; set; }
        public string Tag { get; set; }
    }

    [ClientCanSwapTemplates]
    public class AdminService : Service
    {
        public IRepository Repository { get; set; }

        [DefaultView("Assets")]
        public object Get(Assets request)
        {
            IAuthSession session = this.Request.GetSession();
            List<Asset> items = new List<Asset>();
            if (session.IsAuthenticated)
            {
                if (!string.IsNullOrEmpty(request.Tag))
                {
                    request.Tag = request.Tag.Replace("-", " ").ToLower();
                    items = Repository.Find<Asset>(t => t.UserAuthId == int.Parse(session.UserAuthId) && t.Tags.Contains(request.Tag))
                                  .OrderBy(t => t.Title)
                                  .ToList();
                }
                else if (request.Id.HasValue)
                {
                    items = Repository.Find<Asset>(t => t.UserAuthId == int.Parse(session.UserAuthId) && t.Id == request.Id.Value)
                                  .OrderBy(t => t.Title)
                                  .ToList();
                }
                else
                {
                    items = Repository.Find<Asset>(t => t.UserAuthId == int.Parse(session.UserAuthId))
                                  .OrderBy(t => t.Title)
                                  .ToList();
                }

                if (this.RequestContext.ResponseContentType == "text/html")
                {
                    // Razor:
                    return new AssetsResponse { items = items, session = session, Tag = request.Tag };
                }
                else
                {
                    // API:
                    return items;
                }
            }
            else
            {
                if (this.RequestContext.ResponseContentType == "text/html")
                {
                    // Razor:
                    ViewMessage message = new ViewMessage
                    {
                        Title = "Please sign in",
                        Introduction = "",
                        Message = @"<p class='lead'>You need to sign in first. <a href='/SignIn?return=" + this.Request.RawUrl + "'>Click here</a> to sign in.</p>"
                    };
                    return new HttpResult(message) { View = "Message" };
                }
                else
                {
                    // API:
                    return new HttpResult("Please Sign in first.", HttpStatusCode.Unauthorized);
                }
            }
        }

        public object Post(Assets request)
        {
            IAuthSession session = this.Request.GetSession();
            if (session.IsAuthenticated)
            {
                Asset asset = request.Photo;
                asset.UserAuthId = int.Parse(session.UserAuthId);
                if (!string.IsNullOrEmpty(asset.Tags))
                    asset.Tags = asset.Tags.ToLower();

                List<ValidationResult> results = Utils.General.ValidateObject(asset);

                if (!results.Any())
                {
                    asset = Repository.Add<Asset>(asset);
                    return new HttpResult(asset, HttpStatusCode.Created);
                }
                else
                {
                    return new HttpResult(results, HttpStatusCode.BadRequest);
                }
            }
            else
            {
                return new HttpResult("Please Sign in first.", HttpStatusCode.Unauthorized);
            }
        }

        public object Delete(Assets request)
        {
            IAuthSession session = this.Request.GetSession();
            if (session.IsAuthenticated)
            {
                var assets = Repository.Find<Asset>(t => t.UserAuthId == int.Parse(session.UserAuthId) && t.Id == request.Id);
                if (assets.Count() > 0)
                {
                    Repository.Delete<Asset>(assets[0].Id);
                }
                return null;
            }
            else
            {
                return new HttpResult("Please Sign in first.", HttpStatusCode.Unauthorized);
            }
        }
    }
}