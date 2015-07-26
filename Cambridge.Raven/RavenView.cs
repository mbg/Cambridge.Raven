using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Cambridge.Raven
{
    /// <summary>
    /// Provides a base class for untyped views which make use of Raven identities.
    /// </summary>
    public abstract class RavenView : WebViewPage
    {

    }

    /// <summary>
    /// Provides a base class for typed views which make use of Raven identities.
    /// </summary>
    /// <typeparam name="TModel">The type of the view's model.</typeparam>
    public abstract class RavenView<TModel> : WebViewPage<TModel>
    {

    }
}
