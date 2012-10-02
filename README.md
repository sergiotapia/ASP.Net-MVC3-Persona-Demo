ASP.Net MVC3 - Mozilla Persona Demo
=================================================================================================

![Mozilla | Persona](http://www.mozilla.org/media/img/persona/title.png "Mozilla Persona")

This is a small demo web application that shows how you can easily implement 
Mozilla Persona authentication to a ASP.Net MVC3 web application.

Let's push this standard forward as it's so easy for the users to log in and easy for us
developers to implement.


Getting Started?
------------------

Read the official Documentation first and foremost. At least a cursory glance to see what it's
about and why you should be excited!


General Rundown
------------------

1. You need to add the Persona javascript shiv. For the time being only Firefox offers native BrowserID support, but using this javascript shiv, Persona will work on all major browsers!

    ```html
    <script src="https://login.persona.org/include.js"></script>
    ```


2. Add the client side scripts needed to work with Persona. It comes down to creating two simple links or button, and invoking some API calls on Persona's `navigation.id` object.

    ```html
    <script type="text/javascript" language="javascript">
        $(document).ready(function() {
            var signinLink = document.getElementById('signin');
            if (signinLink) {
                signinLink.onclick = function () { navigator.id.request(); };
            };

            var signoutLink = document.getElementById('signout');
            if (signoutLink) {
                signoutLink.onclick = function () { navigator.id.logout(); };
            };

            var currentUser = null;
            @if (User.Identity.IsAuthenticated) {
                <text>
                currentUser = '@User.Identity.Name';
                </text>
            }

            navigator.id.watch({
                loggedInUser: currentUser,
                onlogin: function (assertion) {
                    // A user has logged in! Here you need to:
                    // 1. Send the assertion to your backend for verification and to create a session.
                    // 2. Update your UI.
                    $.ajax({ /* <-- This example uses jQuery, but you can use whatever you'd like */
                        type: 'POST',
                        url: '/auth/login', // This is a URL on your website.
                        data: { assertion: assertion },
                        success: function (res, status, xhr) { window.location.reload(); },
                        error: function (res, status, xhr) { alert("login failure" + res); }
                    });
                },
                onlogout: function () {
                    // A user has logged out! Here you need to:
                    // Tear down the user's session by redirecting the user or making a call to your backend.
                    // Also, make that loggedInUser will get set to null on the next page load.
                    // (That's a literal JavaScript null. Not false, 0, or undefined. null.)
                    $.ajax({
                        type: 'POST',
                        url: '/auth/logout', // This is a URL on your website.
                        success: function (res, status, xhr) { window.location.reload(); },
                        error: function (res, status, xhr) { alert("logout failure" + res); }
                    });
                }
            });
        });
    </script>
    ```


3. On the server side, create a controller to receive the invoked POST requests:

    ```csharp
    public class AuthController : Controller
    {
        [HttpPost]
        public ActionResult Login(string assertion)
        {
            if (assertion == null)
            {
                // The 'assertion' key of the API wasn't POSTED. Redirect, 
                // or whatever you'd like, to try again.
                return RedirectToAction("Index", "Home");
            }

            using (var web = new WebClient())
            {
                // Build the data we're going to POST.
                var data = new NameValueCollection();
                data["assertion"] = assertion;
                data["audience"] = "http://localhost:46758/"; // Use your website's URL here.

                // POST the data to the Persona provider (in this case Mozilla)
                var response = web.UploadValues("https://verifier.login.persona.org/verify", "POST", data);
                var buffer = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, response);

                // Convert the response to JSON.
                var tempString = Encoding.UTF8.GetString(buffer, 0, response.Length);
                var reader = new JsonReader();
                dynamic output = reader.Read(tempString);
                if (output.status == "okay")
                {
                    string email = output.email; // Since this is dynamic, convert it to string.
                    FormsAuthentication.SetAuthCookie(email, true);
                    return RedirectToAction("Index", "Home");    
                }

                return RedirectToAction("Index", "Home");

                // Example JSON response.
                /*{"status":"okay","email":"johndoe@smith.com","audience":"http://localhost:46758/","expires":1349141963794,"issuer":"login.persona.org"}*/
            }
        }

        [HttpPost]
        public ActionResult Logout()
        {
            // Just sign the user out of the Forms Authentication framework.
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

    }
    ```

