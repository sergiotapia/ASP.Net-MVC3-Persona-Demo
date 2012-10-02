using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using JsonFx.Json;

namespace MVC3PersonaDemo.Controllers
{
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
}
