using TeaShop.Models;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace TeaShop.Providers
{
    public class AuthProvider : OAuthAuthorizationServerProvider
    {
        TeaShopDBContext db;
        public AuthProvider()
        {
            db = new TeaShopDBContext();
        }
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext poContext)
        {
            int nStatus = 0;
            string sError = "";

            //
            // Any Logic goes here
            //

            // returning Auth type and claims
            if (nStatus == 0)
            {
                poContext.Validated();
            }
            else if (nStatus == 1)
            {
                poContext.SetError("invalid_grant", sError);
                return;
            }
            else
            {
               

                poContext.SetError("Server Error", sError);
                return;
            }
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext poContext)
        {
            poContext.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            bool bOk = true;
            int nStatus = 0;
            string sError = "";

            Int64 nId;
            string sUsername;
            string sPasswrd;

            nId = -1;
            sUsername = "";
            sPasswrd = "";


            ClaimsIdentity ClaimsInToken;

        

            if (bOk)
            {
                try
                {
                    sUsername = poContext.UserName;
                    sPasswrd = poContext.Password;
                }
                catch (Exception ex)
                {
                    bOk = false;
                    sError = "Incomplete authentication or authorization data.| Message: " + ex.Message;
                }
            }

            // Finding user
            if (nStatus == 0)
            {
               
              //  nStatus = oBalMUsers.udfChkLoginDetails(out sError, out nId, sPhoneNum, sPasswrd);

                chkLoginDets(out sError, out bOk, out nId, sUsername, sPasswrd);
            }

            // Returning Auth type and claims
            if (bOk)
            {
                ClaimsInToken = new ClaimsIdentity(poContext.Options.AuthenticationType);
                ClaimsInToken.AddClaim(new Claim("sUserId", nId.ToString()));

                poContext.Validated(ClaimsInToken);
            }
            else if (bOk)
            {
                poContext.SetError("invalid_grant", sError);
                return;
            }
            else
            {
               

                poContext.SetError("Server Error", sError);
                return;
            }
        }

        /// <summary>
        /// Need to override if we want to return additional properties
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }

        public void chkLoginDets(out string psError, out bool bOk, out Int64 nId, string sUsername, string sPassword )
        {
            psError = "";
            bOk = false;
            nId = 0;

            List<LoginDet> lsLoginDet = new List<LoginDet>();

            lsLoginDet = (from LGNDT in db.LoginDets
                          where LGNDT.UserName == sUsername
                          && LGNDT.Password == sPassword
                          select LGNDT).ToList();

            if (lsLoginDet.Count > 1)
            {
                bOk = false;
                psError = "More than one entry found with same EmailId(" + sUsername + ").";
            }

            else if(lsLoginDet.Count == 0)
            {
                psError = "Invalid login credentials.";
                bOk = false;
            }
            else
            {
                bOk = true;
            }
               

        }
    }
}