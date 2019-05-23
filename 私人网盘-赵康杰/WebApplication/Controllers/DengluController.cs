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
                HttpCookie cookie = new HttpCookie("MyCook");
                cookie.Values.Add("UserID", user.User_ID.ToString());
                cookie.Values.Add("UserName", user.User_Name);
                cookie.Values.Add("UserPwd", user.User_Password);
                cookie.Expires = DateTime.Now.AddDays(365);
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index","Home");
            }
            return Content("<script>alert('账号或密码错误！')</script>");
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

               
                Model.User newUser = denglu.Login(User);
                HttpCookie cookie = new HttpCookie("MyCook");
                cookie.Values.Add("UserID", newUser.User_ID.ToString());
                cookie.Values.Add("UserName", newUser.User_Name);
                cookie.Values.Add("UserPwd", newUser.User_Password);
                cookie.Expires = DateTime.Now.AddDays(365);
                Response.Cookies.Add(cookie);
                string target = Server.MapPath("~/Upload/");
                Directory.CreateDirectory(target + newUser.User_ID);
                return RedirectToAction("Index","Home");
            }
            return Content("<script>alert('注册失败！')</script>");
        }


     
    }
}