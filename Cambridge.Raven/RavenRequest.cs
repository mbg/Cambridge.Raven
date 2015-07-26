using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenRequest
    {
        #region Constants
        public const String PROTOCOL_VERSION = "3";
        #endregion

        #region Instance members
        private Dictionary<String, String> parameters;
        #endregion

        #region Properties
        public Dictionary<String, String> Parameters
        {
            get { return this.parameters; }
        }

        public String Description
        {
            get { return this.parameters["desc"];  }
            set { this.parameters["desc"] = value; }
        }

        public String AAuth
        {
            get { return this.parameters["aauth"]; }
            set { this.parameters["aauth"] = value; }
        }

        
        #endregion

        public RavenRequest()
        {
            this.parameters = new Dictionary<String, String>();
            this.parameters.Add("ver", PROTOCOL_VERSION);
            this.parameters.Add("date", DateTime.Now.ToRavenTime());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (Int32 i = 0; i < this.parameters.Count; i++)
            {
                var param = this.parameters.ElementAt(i);

                if (i > 0)
                {
                    sb.Append("&");
                }
                else
                {
                    sb.Append("?");
                }

                sb.AppendFormat("{0}={1}", param.Key, param.Value);
            }

            return sb.ToString();
        }
    }
}
