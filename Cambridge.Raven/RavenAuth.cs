using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace Cambridge.Raven
{
    /// <summary>
    /// Implements the Raven authentication flow.
    /// </summary>
    public class RavenAuth
    {
        #region Constants
        /// <summary>
        /// The URL of the default Raven authentication page.
        /// </summary>
        public const String RAVEN_BASE_URL = "https://raven.cam.ac.uk/auth/";
        /// <summary>
        /// The filename of the authentication page.
        /// </summary>
        public const String RAVEN_AUTHENTICATE = "authenticate.html";
        /// <summary>
        /// The filename of the logout page.
        /// </summary>
        public const String RAVEN_LOGOUT = "logout.html";
        /// <summary>
        /// The name of the parameter used by Raven to send a response.
        /// </summary>
        public const String WLS_RESPONSE_PARAM = "WLS-Response";
        /// <summary>
        /// The default path to where the certificates are stored.
        /// </summary>
        public const String CERTIFICATE_PATH = "~/App_Data/";
        #endregion

        #region Instance members
        /// <summary>
        /// The URL of the Raven authentication page we will use.
        /// </summary>
        private String baseURL = RAVEN_BASE_URL;
        /// <summary>
        /// The path to where the certificates are stored.
        /// </summary>
        private String certificatePath = CERTIFICATE_PATH;
        /// <summary>
        /// The path to which a user should be redirected if something has gone wrong.
        /// </summary>
        private String errorURL = null;
        /// <summary>
        /// The certificate store (this used to be a X509Store).
        /// </summary>
        private Dictionary<String, X509Certificate2> certificates;
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
        /// <summary>
        /// Gets or sets the path to where the certificates are stored.
        /// </summary>
        public String CertificatePath
        {
            get { return this.certificatePath; }
            set { this.certificatePath = value; }
        }
        /// <summary>
        /// Gets the URL which users may use to log out of Raven.
        /// </summary>
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
        /// Constructs a new instance of this class.
        /// </summary>
        public RavenAuth()
        {
            // try to load a base URL from the Web.config
            String baseURL = WebConfigurationManager.AppSettings["RavenBaseURL"];

            if (!String.IsNullOrWhiteSpace(baseURL))
                this.baseURL = baseURL;

            // try to load a path to the Raven certificates from Web.config
            String certificatePath = WebConfigurationManager.AppSettings["RavenCertificatePath"];

            if (!String.IsNullOrWhiteSpace(certificatePath))
                this.certificatePath = certificatePath;

            // try to load the error URL from the Web.config
            String errorURL = WebConfigurationManager.AppSettings["RavenError"];

            if (!String.IsNullOrWhiteSpace(errorURL))
                this.errorURL = errorURL;

            // load certificates
            this.LoadCertificate();
        }
        #endregion

        #region LoadCertificate
        /// <summary>
        /// Loads the Raven public key(s).
        /// </summary>
        private void LoadCertificate()
        {
            // initialise a dictionary for certificates
            this.certificates = new Dictionary<String, X509Certificate2>();

            // find the absolute path to the certificates
            String path = HttpContext.Current.Server.MapPath(this.certificatePath);

            // load all certificates from that folder
            foreach(String certFilename in Directory.EnumerateFiles(path, "pubkey*.crt"))
            {
                X509Certificate2 cert = new X509Certificate2(certFilename);
                
                this.certificates.Add(Path.GetFileNameWithoutExtension(certFilename), cert);
            }
        }
        #endregion

        #region CreateTicket
        /// <summary>
        /// Generate a Forms authentication ticket for the current session.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="ravenResponse"></param>
        private void CreateTicket(HttpResponseBase response, RavenResponse ravenResponse)
        {
            // generate a ticket for the Raven session
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    1, 
                    ravenResponse.Parameters["principal"], 
                    DateTime.Now, 
                    ravenResponse.Expires, 
                    false, 
                    ravenResponse.Principal);

            // encrypt it so that the client can't mess with its contents
            String encryptedTicket = FormsAuthentication.Encrypt(ticket);

            // put it in a cookie
            HttpCookie formsCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

            response.Cookies.Add(formsCookie);
        }
        #endregion

        #region LoadIdentity
        /// <summary>
        /// Attempts to load the user's identity from a cookie.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Returns true if the user is authenticated or false if not.</returns>
        public Boolean LoadIdentity(HttpContextBase context)
        {
            // try to find the authentication cookie
            HttpCookie authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                // decrypt the contents
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                context.User = new RavenPrincipal(new RavenIdentity(ticket.UserData));

                // there is an active session
                return true;
            }

            // there is no active session
            return false;
        }
        #endregion

        #region Validate
        /// <summary>
        /// Validates the signature of a response.
        /// </summary>
        /// <param name="response"></param>
        private Boolean Validate(RavenResponse response)
        {
            // find the public key corresponding to the key used by the
            // server to generate the response signature
            String key = String.Format("pubkey{0}", response.CertificateID);

            if (!this.certificates.ContainsKey(key))
                throw new RavenException("Couldn't find the public key needed to validate the response.");

            X509Certificate2 cert = this.certificates[key];

            // calculate a hash for the signature data:
            // 1. convert the signature data into bytes using the ASCII encoding
            // 2. calculate a SHA1 hash for the bytes
            SHA1Managed sha1 = new SHA1Managed();
            ASCIIEncoding ascii = new ASCIIEncoding();
            Byte[] asciiHash = sha1.ComputeHash(ascii.GetBytes(response.SignatureData));

            // 3. validate this hash against the signature obtained from the response
            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)cert.PublicKey.Key;
            RSAPKCS1SignatureDeformatter def = new RSAPKCS1SignatureDeformatter(csp);
            def.SetHashAlgorithm("SHA1");

            return def.VerifySignature(asciiHash, response.Signature);
        }
        #endregion

        #region Authorize
        /// <summary>
        /// Performs the Raven authentication flow.
        /// </summary>
        /// <param name="filterContext"></param>
        public void Authorize(AuthorizationContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;
            HttpResponseBase response = filterContext.HttpContext.Response;

            if (this.LoadIdentity(filterContext.HttpContext))
                return;

            // if this is not a POST request, then we can look for a response
            if (!request.HttpMethod.Equals("POST"))
            {
                // try to get the response
                String wlsResponse = request.Params[WLS_RESPONSE_PARAM];

                if (!String.IsNullOrWhiteSpace(wlsResponse))
                {
                    // parse the response data
                    RavenResponse ravenResponse = new RavenResponse(wlsResponse);

                    // if the server has indicated that authentication was successful,
                    // validate the response signature and set an authentication cookie
                    if (ravenResponse.Status == RavenStatus.OK)
                    {
                        if (!this.Validate(ravenResponse))
                            throw new RavenException("Failed to validate response signature.");

                        // create a Forms authentication ticket and cookie
                        this.CreateTicket(response, ravenResponse);

                        // redirect the user back to where they started
                        response.Redirect(ravenResponse.URL);
                    }
                    else
                    {
                        // check to see if there is a URL we should redirect the user to
                        // if not: throw an exception
                        if (String.IsNullOrWhiteSpace(this.errorURL))
                        {
                            throw new RavenResponseException(
                                "Authentication failed: " + ravenResponse.Status.ToString(), 
                                ravenResponse.Status);
                        }

                        response.Redirect(this.errorURL + (Int32)ravenResponse.Status);
                    }

                    return;
                }
            }
           
            // if we end up here, then we don't have a Raven session
            RavenRequest ravenRequest = new RavenRequest();
            ravenRequest.Parameters.Add("url", request.Url.AbsoluteUri);

            // redirect the user so they can set one up
            response.Redirect(String.Format("{0}{1}{2}",
                this.baseURL, RAVEN_AUTHENTICATE, ravenRequest.ToString()));
        }
        #endregion
    }
}
