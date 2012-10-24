using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Twitterizer;

namespace Website.Infrustructure.Utils
{
    public class TwitterHelper
    {
        private string _accessToken;
        private string _accessTokenSecret;
        private string _consumerKey;
        private string _consumerSecret;
        private OAuthTokens twToken;

        public TwitterHelper(string accessToken, string accessTokenSecret)
        {
            _accessToken = accessToken;
            _accessTokenSecret = accessTokenSecret;
            _consumerKey = ConfigurationManager.AppSettings["oauth.twitter.ConsumerKey"].ToString();
            _consumerSecret = ConfigurationManager.AppSettings["oauth.twitter.ConsumerSecret"].ToString();

            twToken = new OAuthTokens();
            twToken.AccessToken = _accessToken;
            twToken.AccessTokenSecret = _accessTokenSecret;
            twToken.ConsumerKey = _consumerKey;
            twToken.ConsumerSecret = _consumerSecret;
        }

        public TwitterUser GetUser(string username)
        {
            TwitterResponse<TwitterUser> showUserResponse = TwitterUser.Show(twToken, username);
            if (showUserResponse.Result == Twitterizer.RequestResult.Success)
            {
                return showUserResponse.ResponseObject;
            }
            else
            {
                return null;
            }
        }
    }
}