using ProductManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using log4net;
using log4net.Config;

namespace ProductManagement.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HomeController));
        //GET: Home/Login
        //For Login
        public ActionResult Login()
        {
            return View();
        }
        //POST: Home/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(tblLogin objUser)
        {
            //Exceptional Handling
            try
            {
                //Validate the model state
                if (ModelState.IsValid)
                {
                    using (var db = new ProdcutsEntities())
                    {
                        //Compare the user data for the login
                        var obj = db.tblLogins.Where(a => a.emailid.Equals(objUser.emailid) && a.password.Equals(objUser.password)).FirstOrDefault();
                        if (obj != null)
                        {
                            Log.Info("Login Successfully");
                            ViewBag.Message = obj.username.ToString();
                            //Set the coockie for the authentication for the product details
                            FormsAuthentication.SetAuthCookie(obj.username.ToString(), false);
                            //Redirect to Index page
                            return this.RedirectToAction("Index", "Products");
                        }
                        else
                        {
                            //Set error for Invalid credential
                            ModelState.AddModelError("", "Invalid Email Id or Password");
                            return View("Login");
                        }
                    }
                }
            }
            //Catch the exception
            catch (Exception ex)
            {
                //Set error for the model state
                ModelState.AddModelError("", "Model Error");
                Log.Error(ex.ToString());
            }
            //Return view with user object
            return View(objUser);
        }
        //GET: Home/Logout
        //For user logout
        public ActionResult Logout()
        {
            //Logout Logic
            FormsAuthentication.SignOut();
            //ViewBag.Message("Login Successfully");
            //Return to the Login Page
            return RedirectToAction("Login", "Home");
        }
    }
}