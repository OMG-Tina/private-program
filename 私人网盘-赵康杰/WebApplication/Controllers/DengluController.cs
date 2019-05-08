using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class DengluController : Controller
    {
        Services.Denglu denglu = new Services.Denglu();
        // GET: Denglu
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Model.User User)
        {

            Model.User user = denglu.Login(User);
            if(user!=null){
                Session["User"] = user;
                HttpCookie cookie = new HttpCookie("MyCook");
                cookie.Values.Add("UserID", user.User_ID.ToString());
                cookie.Values.Add("UserName", user.User_Name);
                cookie.Values.Add("UserPwd", user.User_Password);
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [HttpGet]
        public ActionResult zhuce()
        {
            return View();
        }
        [HttpPost]
        public ActionResult zhuce(Model.User User)
        {
            int jg = denglu.InsertUser(User);
            if(jg>0){

                Session["User"] = User;
                Model.User newUser = denglu.Login(User);
                string target = Server.MapPath("~/Upload/");
                Directory.CreateDirectory(target + newUser.User_ID);
                return RedirectToAction("Index","Home");
            }
            return Content("<script>alert('注册失败！')</script>");
        }


     
    }
}