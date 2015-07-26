using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    public class RavenResponseException : RavenException
    {
        #region Instance members
        /// <summary>
        /// The status returned by the server (if available).
        /// </summary>
        private RavenStatus status = RavenStatus.Unknown;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the status returned by the server, if available.
        /// </summary>
        public RavenStatus Status
        {
            get { return this.status; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs an exception with the specified message.
        /// </summary>
        /// <param name="message"></param>
        public RavenResponseException(String message)
            : base("Invalid response received: " + message)
        {
        }
        /// <summary>
        /// Constructs an exception with the specified message and status.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="status"></param>
        public RavenResponseException(String message, RavenStatus status)
            : this(message)
        {
            this.status = status;
        }
        #endregion
    }
}
