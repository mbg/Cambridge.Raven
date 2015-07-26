using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenResponse
    {
        #region Constants
        public const Char RESPONSE_SEP = '!';
        /// <summary>
        /// 
        /// </summary>
        
        #endregion

        #region Instance members
        private String[] RESPONSE_FIELDS = { 
            "ver", "status", "msg", "issue", "id", "url", 
            "principal", "ptags", "auth", "sso", "life",
            "params", "kid", "sig"
        };
        private String data;
        private Dictionary<String, String> parameters;
        private RavenVersion version = RavenVersion.WLS3;
        private RavenStatus status = RavenStatus.OK;
        private DateTime issued;
        private DateTime expires;
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
        /// Gets the protocol version indicated by the response. This is always WLS3.
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

        public String Message
        {
            get { return this.parameters["msg"]; }
        }

        public DateTime Issued
        {
            get { return this.issued; }
        }

        public DateTime Expires
        {
            get { return this.expires; }
        }

        public String ID
        {
            get { return this.parameters["id"]; }
        }

        public String URL
        {
            get { return this.parameters["url"]; }
        }

        public String Principal
        {
            get { return this.parameters["principal"]; }
        }

        public String Tags
        {
            get { return this.parameters["ptags"]; }
        }

        public String Auth
        {
            get { return this.parameters["auth"]; }
        }

        public String KeyID
        {
            get { return this.parameters["kid"]; }
        }

        public String Signature
        {
            get { return this.parameters["sig"]; }
        }
        #endregion

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

            if (ver != 3)
                throw new RavenException("Unsupported protcol version.");

            // try to parse the status code 
            Int32 statusCode = 0;

            if (!Int32.TryParse(parts[1], out statusCode))
                throw new RavenResponseException("could not identify status code");

            this.status = (RavenStatus)statusCode;

            // add all of the fields to the dictionary
            for (Int32 i = 0; i < parts.Length; i++)
            {
                this.parameters.Add(RESPONSE_FIELDS[i], parts[i]);
            }

            

            if (!this.parameters["issue"].FromRavenTime(out this.issued))
                throw new RavenException("Unable to parse the time when the response was issued.");

            if (this.status == RavenStatus.OK)
            {
                Int32 lifetime = 0;

                if (!Int32.TryParse(this.parameters["life"], out lifetime))
                    throw new RavenResponseException("seesion lifetime is not specified");

                this.expires = this.issued.AddSeconds(lifetime);
            }
        }
    }
}
