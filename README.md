# Cambridge.Raven
ASP.NET MVC Authentication Filter for Raven.

## Setup

Put the [https://raven.cam.ac.uk/project/keys/](public keys used by Raven) in your `App_Data` folder. You can also put them elsewhere if you modify `Web.config` to add the following key to the `appSettings` section:

```xml
<appSettings>
    <add key="RavenCertificatePath" value="C:\MyCertificateStore" />
</appSettings>
```

The value should be set to the path where the public keys are stored.

## Usage

Add `Cambridge.Raven` as a reference to your ASP.NET MVC project. Once imported, there are several different ways in which you can use Raven authentication.

### Controller

If all of a controller's actions should require Raven authentication, then the easiest way to accomplish this is to inherit from the `RavenController` class.

```C#
public class MyController : RavenController
{
    public ActionResult Index()
    {
        return View();
    }
}
```

### Attribute

If you only some actions require a Raven session, then the ``RavenAuthorizeAttribute`` may be used to indicate that.

```C#
public class MyController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
    
    [RavenAuthorize]
    public ActionResult Foo()
    {
        return View();
    }
}
```

This attribute may also be used on a controller if inheriting from `RavenController` is not desirable:

```C#
[RavenAuthorize]
public class MyController : Controller
{
    public ActionResult Index()
    {
        return View();
    }
}
```

You can also use the standard security attributes in ASP.NET such as `AllowAnonymous` to e.g. allow individual actions to be accessed without a Raven session.

### Custom 

The Raven authentication flow is implemented by the `RavenAuth` class and can be used without a controller or attribute. For example, you could write the following in a custom controller to implement your own version of `RavenController`:

```C#
private RavenAuth auth;

public MyController()
{
    this.auth = new RavenAuth();
}

protected override void OnAuthorization(AuthorizationContext filterContext)
{
    this.auth.Authorize(filterContext);
}
```

`Authorize` will throw an exception if something goes wrong.

### Customisation 

You can modify the base URL used for the Raven authentication flow by editing your `Web.config` and adding a `RavenBaseURL` key to the `appSettings` section. For example, to use the test service:

```xml
<appSettings>
    <add key="RavenBaseURL" value="https://demo.raven.cam.ac.uk/auth/" />
</appSettings>
```
