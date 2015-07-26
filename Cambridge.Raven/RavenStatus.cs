using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cambridge.Raven
{
    /// <summary>
    /// Enumerates WLS status codes.
    /// </summary>
    public enum RavenStatus
    {
        /// <summary>
        /// Default value in case no status is available.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// The user successfully identified him/her self and their
        /// identity is present in the response. All responses with
        /// status code 200 are signed.
        /// </summary>
        OK = 200,
        /// <summary>
        /// The user actively abandoned the authentication process by
        /// selecting a cancel button or similar process. Note that
        /// users can equally abandoned the authentication process
        /// by directing their browser elsewhere after an
        /// authentication request. In this case no response will
        /// be forthcoming.
        /// </summary>
        Cancelled = 410,
        /// <summary>
        /// The WLS does not support any of the authentication types
        /// specified in the 'aauth' parameter of the authentication
        /// request.
        /// </summary>
        AuthTypeUnsupported = 510,
        /// <summary>
        /// The WLS does not support the version of the protocol
        /// used in the authentication response. This status code
        /// will only ever be sent in a response with the 'ver'
        /// field' set to 1.
        /// </summary>
        UnsupportedProtocolVersion = 520,
        /// <summary>
        /// There was a problem decoding the request parameters that
        /// is not covered by a more specific status - perhaps an
        /// unrecognised parameter.
        /// </summary>
        RequestParameterError = 530,
        /// <summary>
        /// The request specified 'iact' as 'no' but either the user
        /// is not currently identified to the WLS or  the user has
        /// asked to be notified before responses that identify
        /// him/her are issued. 
        /// </summary>
        InteractionRequired = 540,
        /// <summary>
        /// The WAA is not authorised to use this WLS.
        /// </summary>
        WAANotAuthorised = 560,
        /// <summary>
        /// The WLS declines to provide authentication services on
        /// this occasion.
        /// </summary>
        AuthenticationDeclined = 570
    }
}
