using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace Cambridge.Raven
{
    public class RavenAuth
    {
        #region Constants
        /// <summary>
        /// The URL of the default Raven authentication page.
        /// </summary>
        public const String RAVEN_BASE_URL = "https://raven.cam.ac.uk/auth/";

        public const String RAVEN_AUTHENTICATE = "authenticate.html";
        public const String RAVEN_LOGOUT = "logout.html";
        /// <summary>
        /// The name of the parameter used by Raven to send a response.
        /// </summary>
        public const String WLS_RESPONSE_PARAM = "WLS-Response";
        #endregion

        #region Instance members
        /// <summary>
        /// The URL of the Raven authentication page we will use.
        /// </summary>
        private String baseURL = RAVEN_BASE_URL;
        /// <summary>
        /// The certificate store.
        /// </summary>
        private X509Store store;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the URL to which users will be redirected for authentication.
        /// </summary>
        public String BaseURL
        {
            get { return this.baseURL; }
            set { this.baseURL = value; }
        }

        public String LogoutURL
        {
            get
            {
                return String.Format("{0}{1}", this.baseURL, RAVEN_LOGOUT);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public RavenAuth()
        {
            String customSetting = WebConfigurationManager.AppSettings["RavenBaseURL"];

            if (!String.IsNullOrWhiteSpace(customSetting))
                this.baseURL = customSetting;

            this.LoadCertificate();
            //this.store = new X509Store("");
        }
        #endregion

        private void LoadCertificate()
        {

        }

        private void CreateTicket(HttpResponseBase response, RavenResponse ravenResponse)
        {
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, 
                    ravenResponse.Parameters["principal"], 
                    DateTime.Now, 
                    ravenResponse.Expires, 
                    false, 
                    ravenResponse.Principal);

            String encryptedTicket = FormsAuthentication.Encrypt(ticket);

            HttpCookie formsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);



            response.Cookies.Add(formsCookie);
        }

        public Boolean LoadIdentity(HttpContextBase context)
        {
            HttpCookie authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                context.User = new RavenPrincipal(new RavenIdentity(ticket.UserData));

                return true;
            }

            return false;
        }

        #region Authorize
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public void Authorize(AuthorizationContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;
            HttpResponseBase response = filterContext.HttpContext.Response;

            if (this.LoadIdentity(filterContext.HttpContext))
                return;

            // test if there is 
            if (!request.HttpMethod.Equals("POST"))
            {
                String wlsResponse = request.Params[WLS_RESPONSE_PARAM];

                if (!String.IsNullOrWhiteSpace(wlsResponse))
                {
                    RavenResponse ravenResponse = new RavenResponse(wlsResponse);

                    this.CreateTicket(response, ravenResponse);

                    response.Redirect(ravenResponse.URL);
                    return;
                }
            }
           
            // if we end up here, then we don't have
            RavenRequest ravenRequest = new RavenRequest();
            ravenRequest.Parameters.Add("url", request.Url.AbsoluteUri);

            response.Redirect(String.Format("{0}{1}{2}",
                this.baseURL, RAVEN_AUTHENTICATE, ravenRequest.ToString()));
        }
        #endregion
    }
}
