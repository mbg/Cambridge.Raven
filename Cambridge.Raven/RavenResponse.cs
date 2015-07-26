using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Cambridge.Raven
{
    public class RavenResponse
    {
        #region Constants
        /// <summary>
        /// The character used by Raven to separate fields in the response data.
        /// </summary>
        public const Char RESPONSE_SEP = '!';        
        #endregion

        #region Instance members
        /// <summary>
        /// The names of the fields in the response (as suggested by the specification).
        /// </summary>
        private String[] RESPONSE_FIELDS = { 
            "ver", "status", "msg", "issue", "id", "url", 
            "principal", "ptags", "auth", "sso", "life",
            "params", "kid", "sig"
        };
        /// <summary>
        /// Stores the unprocessed response data.
        /// </summary>
        private String data;
        /// <summary>
        /// Stores the unprocessed field values, indexed by field names.
        /// </summary>
        private Dictionary<String, String> parameters;
        /// <summary>
        /// The protocol version used by the server.
        /// </summary>
        private RavenVersion version = RavenVersion.WLS3;
        /// <summary>
        /// The status code returned by the server.
        /// </summary>
        private RavenStatus status = RavenStatus.OK;
        /// <summary>
        /// The System.DateTime object indicating when the response was generated.
        /// </summary>
        private DateTime issued;
        /// <summary>
        /// The System.DateTime object indicating when the session will expire.
        /// </summary>
        private DateTime expires;
        /// <summary>
        /// The decoded signature of the response.
        /// </summary>
        private Byte[] signature;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a dictionary containing all of the response fields.
        /// </summary>
        public Dictionary<String, String> Parameters
        {
            get
            {
                return this.parameters;
            }
        }
        /// <summary>
        /// Gets the protocol version indicated by the response. 
        /// </summary>
        public RavenVersion Version
        {
            get
            {
                return this.version;
            }
        }
        /// <summary>
        /// Gets the status code returned by Raven.
        /// </summary>
        public RavenStatus Status
        {
            get
            {
                return this.status;
            }
        }
        /// <summary>
        /// Gets a message which may have been included in the response.
        /// </summary>
        public String Message
        {
            get { return this.parameters["msg"]; }
        }
        /// <summary>
        /// Gets the DateTime object indicating when the response was created on the server.
        /// </summary>
        public DateTime Issued
        {
            get { return this.issued; }
        }
        /// <summary>
        /// If authentication was successful, this gets the DateTime object which indicates 
        /// the time when the session expires. Note that the session may expire before this time.
        /// </summary>
        public DateTime Expires
        {
            get { return this.expires; }
        }
        /// <summary>
        /// Gets an identifier for this response which, combined with the time when this response
        /// was issued, may serve as a unique identifier for this response.
        /// </summary>
        public String ID
        {
            get { return this.parameters["id"]; }
        }
        /// <summary>
        /// Gets the URL to which the user should be redirected.
        /// </summary>
        public String URL
        {
            get { return this.parameters["url"]; }
        }
        /// <summary>
        /// Gets the CRSid of the user if authentication was successful.
        /// </summary>
        public String Principal
        {
            get { return this.parameters["principal"]; }
        }
        /// <summary>
        /// Gets a comma-separated list of tags. Raven only uses this to
        /// indicate whether the user is a current member of the university
        /// or not.
        /// </summary>
        public String Tags
        {
            get { return this.parameters["ptags"]; }
        }
        /// <summary>
        /// Gets a System.String indicating which authentication method was used.
        /// </summary>
        public String Auth
        {
            get { return this.parameters["auth"]; }
        }
        /// <summary>
        /// Gets the numeric ID of the public key that was used by the server.
        /// </summary>
        public String KeyID
        {
            get { return this.parameters["kid"]; }
        }
        /// <summary>
        /// Gets the decoded signature of this response.
        /// </summary>
        public Byte[] Signature
        {
            get
            {
                return this.signature;
            }
        }
        /// <summary>
        /// Gets the unprocessed response data.
        /// </summary>
        public String Data
        {
            get
            {
                return this.data;
            }
        }
        /// <summary>
        /// Gets the response data which was used by the server to generate the signature.
        /// </summary>
        public String SignatureData
        {
            get
            {
                Int32 last = this.data.LastIndexOf(RESPONSE_SEP);
                Int32 secondToLast = this.data.LastIndexOf(RESPONSE_SEP, last - 1);

                return this.data.Substring(0, secondToLast);
            }
        }
        /// <summary>
        /// Gets the ID of the public key used to generate the signature.
        /// </summary>
        public String CertificateID
        {
            get
            {
                return this.parameters["kid"];
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs a RavenResponse object from data contained in a System.String.
        /// </summary>
        /// <param name="data">The System.String which contains the response data.</param>
        public RavenResponse(String data)
        {
            // store the response and initialise a new dictionary for
            // the data we have received
            this.data = data;
            this.parameters = new Dictionary<String, String>();

            // split the response into its components
            String[] parts = data.Split(RESPONSE_SEP);

            if (parts.Length < 2)
                throw new RavenResponseException("too few fields");

            // try to parse the protocol version and check that it is supported
            Int32 ver = 0;

            if (!Int32.TryParse(parts[0], out ver))
                throw new RavenResponseException("could not identify protocol version");

            if (ver > 3)
                throw new RavenException("Unsupported protcol version.");

            this.version = (RavenVersion)ver;

            // try to parse the status code 
            Int32 statusCode = 0;

            if (!Int32.TryParse(parts[1], out statusCode))
                throw new RavenResponseException("could not identify status code");

            this.status = (RavenStatus)statusCode;

            // add all of the fields to the dictionary
            for (Int32 i = 0; i < parts.Length; i++)
            {
                this.parameters.Add(RESPONSE_FIELDS[i], HttpUtility.UrlDecode(parts[i]));
            }

            // try to parse the time when the session was started
            if (!this.parameters["issue"].FromRavenTime(out this.issued))
                throw new RavenException("Unable to parse the time when the Raven response was issued.");

            // if the status is OK, then there is some additional data we need to process
            if (this.status == RavenStatus.OK)
            {
                // calculate the session lifetime:
                // the 'life' field contains the lifetime in seconds,
                // so we can calculate the time when the session expires by
                // adding those seconds to the time when the session was started
                Int32 lifetime = 0;

                if (!Int32.TryParse(this.parameters["life"], out lifetime))
                    throw new RavenResponseException("seesion lifetime is not specified");

                this.expires = this.issued.AddSeconds(lifetime);

                if (this.parameters["kid"].StartsWith("0"))
                    throw new RavenResponseException("invalid key ID");

                // decode the signature
                this.signature = this.DecodeSignature(this.parameters["sig"]);
            }
        }
        #endregion

        #region DecodeSignature
        /// <summary>
        /// Decodes the response signature which is in base64 with some
        /// characters replaced.
        /// </summary>
        /// <param name="signature">The signature to decode.</param>
        /// <returns>Returns the decoded date contained in the signature.</returns>
        private Byte[] DecodeSignature(String signature)
        {
            StringBuilder sb = new StringBuilder(signature.Length);

            for (Int32 i = 0; i < signature.Length; i++)
            {
                // '-' should be '+'
                // '.' should be '/'
                // '_' should be '='
                if (signature[i] == '-')
                    sb.Append('+');
                else if (signature[i] == '.')
                    sb.Append('/');
                else if (signature[i] == '_')
                    sb.Append('=');
                else
                    sb.Append(signature[i]);
            }

            return Convert.FromBase64String(sb.ToString());
        }
        #endregion
    }
}
