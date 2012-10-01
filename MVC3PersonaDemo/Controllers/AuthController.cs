using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Collections.Specialized;
using System.Net;
using System.Text;

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
                var data = new NameValueCollection();
                data["assertion"] = assertion;
                data["audience"] = "http://localhost:46758/"; // Use your website's URL here.

                var response = web.UploadValues("https://verifier.login.persona.org/verify", "POST", data);
                var buffer = Encoding.Convert(Encoding.GetEncoding("iso-8859-1"), Encoding.UTF8, response);
                var tempString = Encoding.UTF8.GetString(buffer, 0, response.Length);

                if (true)
                {
                    
                }
            }
            return View();


            
            /*# The request has to have an assertion for us to verify
    if 'assertion' not in request.form:
        abort(400)
 
    # Send the assertion to Mozilla's verifier service.
    data = {'assertion': request.form['assertion'], 'audience': 'https://example.com:443'}
    resp = requests.post('https://verifier.login.persona.org/verify', data=data, verify=True)
 
    # Did the verifier respond?
    if resp.ok:
        # Parse the response
        verification_data = json.loads(resp.content)
 
        # Check if the assertion was valid
        if verification_data['status'] == 'okay':
            # Log the user in by setting a secure session cookie
            session.update({'email': verification_data['email']})
            return resp.content
 
    # Oops, something failed. Abort.
    abort(500)*/
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
