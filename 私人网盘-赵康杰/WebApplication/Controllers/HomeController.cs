using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Excel;


namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        Services.Home Home = new Services.Home();
        Services.Denglu Denglu = new Services.Denglu();
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                if (Request.Cookies["MyCook"]["UserID"] != "")
                {
                    //获取用户id
                    int UserID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]);
                    //根据用户id查询用户信息
                    Model.User User = Denglu.GetUserByID(UserID);

                    ViewBag.User = User;
                    //用户所有文件夹
                    ViewBag.Folder = Home.GetFolderList(UserID);
                    //所有加载文件数量
                    int Count = Home.GetFolderList(UserID).Count + Home.GetAll(UserID).Count;
                    ViewBag.Count = Count;

                    //所有文件
                    return View(Home.GetAll(UserID));
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Denglu");
            }

            return RedirectToAction("Index", "Denglu");
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            List<string> paths = new List<string>();
            if (Request.Files.Count == 0)
            {
                //Directory.CreateDirectory("");
                //Request.Files.Count 文件数为0上传不成功
                return View();
            }

            //文件大小不为0
            HttpFileCollectionBase files = Request.Files;
            //取得目标文件夹的路径
            string target = Server.MapPath("~/Upload/" + Request.Cookies["MyCook"]["UserID"] + "/");
            int cg = 0;
            int sb = -1;
            int x = 0;
            for (int i = 0; i < files.Count; i++)
            {

                 if (files[i].ContentLength == 0)
                {

                    //文件大小（以字节为单位）为0时，做一些操作
                    //失败文件数，失败+1
                    sb = sb + 1;
                    //return Content("<script>alert('文件大小为0，上传失败')</script>");
                }
                else
                {
                    string filename = files[i].FileName;//取得文件名字
                    //判断上传文件是否包含文件夹，含/就是包含文件夹路径的
                    if (filename.Contains("/"))
                    {
                        //根据/分割，以便将文件名与文件夹路径分开
                        string[] b2 = filename.Split('/');
                        //获得文件名
                        string wenjian = b2[b2.Length - 1];
                        //将数组转为泛型集合操作
                        List<string> newb2 = new List<string>(b2);
                        //排除集合最后一个元素，也就是文件名，得到文件夹路径
                        newb2.Remove(newb2.Last<string>());
                        //定义一个文件夹路径变量
                        string lujin = null;
                        //遍历去除文件名的文件路径集合

                        foreach (var item in newb2)
                        {
                            //拼接文件路径
                            lujin += item + "\\";
                            if (x > 0)
                            {

                                Model.Folder folder = new Model.Folder
                                {
                                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                    Folder_Name = item,
                                    FatherFolderID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                    DeleteState = 0,
                                    EstablishTime = DateTime.Now
                                };
                                if (Home.GetFolderByName(item, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])) == null)
                                {
                                    Home.InsertFolder(folder);
                                    x = x + 1;
                                }

                            }
                            else
                            {
                                Model.Folder folder = new Model.Folder
                                {
                                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                    Folder_Name = item,
                                    FatherFolderID = 0,
                                    DeleteState = 0,
                                    EstablishTime = DateTime.Now
                                };
                                if (Home.GetFolderByName(item, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])) == null)
                                {
                                    Home.InsertFolder(folder);
                                    x = x + 1;
                                }


                            }
                            //E:\C#作业\Baidu-pan\Baidu.Web\Upload\文件夹上传测试文档\测试2\5c70e0a0-90a0-4d95-b7c9-c95284f41ee2.txt


                        }


                        //创建文件夹路径
                        Directory.CreateDirectory(target + lujin);

                        string newfilename = filename;

                        //使用GUID 全球唯一标识符给文件重名称：GUID + 后缀名
                        newfilename = Guid.NewGuid() + filename.Substring(filename.LastIndexOf("."));

                        //获取存储的目标地址，也就是文件夹位置
                        string path = target + lujin + newfilename;
                        //保存文件
                        files[i].SaveAs(path);
                        //成功上传文件数，成功+1
                        cg = cg + 1;
                        paths.Add(path);

                        
                        //将文件信息保存到数据库

                        //覆盖重复文件
                        Model.Store_data oldStore_data = Home.GetFile(wenjian, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
                        if (oldStore_data!=null)
                        {
                            System.IO.File.Delete(oldStore_data.DataRoute);
                            Home.DeleteOldFile(oldStore_data.Store_data_ID);
                        }

                        string[] icon = wenjian.Split('.');
                        string icons = Server.MapPath("~/Icon/" + icon[icon.Length - 1].ToUpper() + ".png");
                        Model.Store_data data = null;
                        if (System.IO.File.Exists(icons))
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = wenjian,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/" + icon[icon.Length - 1].ToUpper() + ".png"
                            };
                        }
                        else
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = wenjian,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/weizhi.png"
                            };
                        }
                        Home.Insert(data);

                    }
                    else
                    {
                        string newfilename = filename;

                        //使用GUID 全球唯一标识符给文件重名称：GUID + 后缀名
                        newfilename = Guid.NewGuid() + filename.Substring(filename.LastIndexOf("."));

                        //获取存储的目标地址，也就是文件夹位置
                        string path = target + newfilename;
                        files[i].SaveAs(path);
                        cg = cg + 1;
                        paths.Add(path);


                        
                        //将文件信息保存到数据库

                        //覆盖重复文件
                        Model.Store_data oldStore_data = Home.GetFile(filename, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
                        if (oldStore_data != null)
                        {
                            System.IO.File.Delete(oldStore_data.DataRoute);
                            Home.DeleteOldFile(oldStore_data.Store_data_ID);
                        }
                        string[] icon = filename.Split('.');
                        Model.Store_data data = null;
                        if(System.IO.File.Exists(Server.MapPath("~/Icon/" + icon[icon.Length - 1].ToUpper() + ".png")))
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = filename,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = 0,
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/" + icon[icon.Length - 1].ToUpper() + ".png"
                            };
                        }
                        else
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = filename,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = 0,
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/weizhi.png"
                            };
                        }

                        Home.Insert(data);

                    }
                }

            }
            return RedirectToAction("Index");
            //return Content(@"<script> alert('成功" + cg + "个，失败" + sb + "个'); window.location.href = '/Home/Index'; </script>");
        }


        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public ActionResult SignOut()
        {
            HttpCookie myCookie = Request.Cookies["MyCook"];
            myCookie.Expires = DateTime.Now.AddDays(-1);
            System.Web.HttpContext.Current.Response.Cookies.Set(myCookie);
            return RedirectToAction("Index", "Denglu");

        }

        [HttpGet]
        public ActionResult UpdatePwd()
        {
            return View();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePwds(string NewPwd) {
            Model.User user = new Model.User();
            user.User_Name = Request.Cookies["MyCook"]["UserName"];
            user.User_Password = Request.Cookies["MyCook"]["UserPwd"];
            int jg2 = Home.UpdatePwd(NewPwd, user);
            return Json(jg2.ToString(), JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 验证旧密码
        /// </summary>
        /// <param name="UserPwd"></param>
        /// <returns></returns>
        public ActionResult VerifyPassword(string UserPwd) {
            string user = Request.Cookies["MyCook"]["UserPwd"].ToString();
            if(user.Equals(UserPwd)){
                return Json("1", JsonRequestBehavior.AllowGet);
            }
            return Json("0", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult OpenForder(string ID)
        {
            ViewBag.User = Request.Cookies["MyCook"]["UserName"];
            ViewBag.Folder = Home.GetFolderByID(Convert.ToInt32(ID));
            return View(Home.GetShareDataByID(Convert.ToInt32(ID)));

        }

        /// <summary>
        /// 查询子文件夹
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public ActionResult OpenFolder2(string FolderName)
        {
            Model.Folder Folder = Home.GetFolderByID(Convert.ToInt32(FolderName), Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
            var list = Home.GetFolderByID(Folder.Folder_ID);

            var list1 = list.Select(x => new { x.Folder_ID, x.Folder_Name, EstablishTime = x.EstablishTime.ToString("yyyy-MM-dd") });
            return Json(list1, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 查询子文件
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public ActionResult OpenFolder3(string FolderName)
        {
            Model.Folder Folder = Home.GetFolderByID(Convert.ToInt32(FolderName), Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
            var list = Home.GetShareDataByID(Folder.Folder_ID);

            var list1 = list.Select(x => new { x.Store_data_ID, x.Real_FileName, x.File_Size, EstablishTime = x.EstablishTime.ToString("yyyy-MM-dd"), x.icon });
            return Json(list1, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 新建文件夹
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public ActionResult AddFolder(string FolderName, string FatherFolderID)
        {
            Model.Folder folder = new Model.Folder();
            if (FatherFolderID != null)
            {
                folder = new Model.Folder
                {
                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                    Folder_Name = FolderName,
                    FatherFolderID = Convert.ToInt32(FatherFolderID),
                    DeleteState = 0,
                    EstablishTime = DateTime.Now
                };
            }
            else {
                folder = new Model.Folder
                {
                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                    Folder_Name = FolderName,
                    FatherFolderID = 0,
                    DeleteState = 0,
                    EstablishTime = DateTime.Now
                };
            }
            int uid = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]);
            int jg = 0;
            if (Home.YanZhenFolderByName(FolderName, Convert.ToInt32(FatherFolderID), uid) == null)
            {
                jg =Home.InsertFolder(folder);
               
            }
            return Json(jg.ToString(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Picture() {
            return View();
        }

        [HttpGet]
        public ActionResult Classification()
        {
            ViewBag.User = Request.Cookies["MyCook"]["UserName"];
            int uid = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]);
            string Keyword = Request.QueryString["Keyword"];
            if ("文档".Equals(Keyword))
            {
                ViewBag.Leibie = "文档";
                List<Model.Store_data> list1 = Home.GetStore_dataBySuffixName(uid, ".txt");
                List<Model.Store_data> list2 = Home.GetStore_dataBySuffixName(uid, ".docx");
                foreach (var item in list2)
                {
                    Model.Store_data data = new Model.Store_data 
                    {
                        Store_data_ID = item.Store_data_ID,
                        User_ID = item.User_ID,
                        DataRoute =  item.DataRoute,
                        Real_FileName =  item.Real_FileName,
                        File_Size =  item.File_Size,
                        Folder_ID =  item.Folder_ID,
                        EstablishTime =  item.EstablishTime,
                        DeleteState = item.DeleteState,
                        SuffixName =  item.SuffixName,
                        icon = item.icon
                    };
                    list1.Add(data);
                }
                return View(list1);
            }
            else if ("图片".Equals(Keyword))
            {
                ViewBag.Leibie = "图片";
                return View(Home.GetStore_dataBySuffixName(uid, ".png"));

            }
            else if ("视频".Equals(Keyword))
            {
                ViewBag.Leibie = "视频";
                return View(Home.GetStore_dataBySuffixName(uid, ".mp4"));

            }
            else if ("音乐".Equals(Keyword))
            {
                ViewBag.Leibie = "音乐";
                return View(Home.GetStore_dataBySuffixName(uid, ".mp3"));

            }
            else if ("其他".Equals(Keyword))
            {
                ViewBag.Leibie = "其他";
                return View(Home.GetStore_dataBySuffixName(uid));
            }
            return View();
        }

        

        [HttpPost]
        public ActionResult Classification(FormCollection form, string fenlei)
        {
            List<string> paths = new List<string>();
            if (Request.Files.Count == 0)
            {
                //Directory.CreateDirectory("");
                //Request.Files.Count 文件数为0上传不成功
                return View();
            }
           
           

            //文件大小不为0
            HttpFileCollectionBase files = Request.Files;
            //取得目标文件夹的路径
            string target = Server.MapPath("~/Upload/" + Request.Cookies["MyCook"]["UserID"] + "/");
            int cg = 0;
            int sb = -1;
            int x = 0;
            for (int i = 0; i < files.Count; i++)
            {

                if (files[i].ContentLength == 0)
                {

                    //文件大小（以字节为单位）为0时，做一些操作
                    //失败文件数，失败+1
                    sb = sb + 1;
                    //return Content("<script>alert('文件大小为0，上传失败')</script>");
                }
                else
                {
                    string filename = files[i].FileName;//取得文件名字
                    //判断上传文件是否包含文件夹，含/就是包含文件夹路径的
                    if (filename.Contains("/"))
                    {
                        //根据/分割，以便将文件名与文件夹路径分开
                        string[] b2 = filename.Split('/');
                        //获得文件名
                        string wenjian = b2[b2.Length - 1];
                        //将数组转为泛型集合操作
                        List<string> newb2 = new List<string>(b2);
                        //排除集合最后一个元素，也就是文件名，得到文件夹路径
                        newb2.Remove(newb2.Last<string>());
                        //定义一个文件夹路径变量
                        string lujin = null;
                        //遍历去除文件名的文件路径集合

                        foreach (var item in newb2)
                        {
                            //拼接文件路径
                            lujin += item + "\\";
                            if (x > 0)
                            {

                                Model.Folder folder = new Model.Folder
                                {
                                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                    Folder_Name = item,
                                    FatherFolderID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                    DeleteState = 0,
                                    EstablishTime = DateTime.Now
                                };
                                if (Home.GetFolderByName(item, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])) == null)
                                {
                                    Home.InsertFolder(folder);
                                    x = x + 1;
                                }

                            }
                            else
                            {
                                Model.Folder folder = new Model.Folder
                                {
                                    FounderID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                    Folder_Name = item,
                                    FatherFolderID = 0,
                                    DeleteState = 0,
                                    EstablishTime = DateTime.Now
                                };
                                if (Home.GetFolderByName(item, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])) == null)
                                {
                                    Home.InsertFolder(folder);
                                    x = x + 1;
                                }


                            }
                            //E:\C#作业\Baidu-pan\Baidu.Web\Upload\文件夹上传测试文档\测试2\5c70e0a0-90a0-4d95-b7c9-c95284f41ee2.txt


                        }
                        //创建文件夹路径
                        Directory.CreateDirectory(target + lujin);

                        string newfilename = filename;

                        //使用GUID 全球唯一标识符给文件重名称：GUID + 后缀名
                        newfilename = Guid.NewGuid() + filename.Substring(filename.LastIndexOf("."));

                        //获取存储的目标地址，也就是文件夹位置
                        string path = target + lujin + newfilename;
                        //保存文件
                        files[i].SaveAs(path);
                        //成功上传文件数，成功+1
                        cg = cg + 1;
                        paths.Add(path);

                        string[] icon = wenjian.Split('.');
                        //将文件信息保存到数据库

                        //覆盖重复文件
                        Model.Store_data oldStore_data = Home.GetFile(wenjian, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
                        if (oldStore_data != null)
                        {
                            System.IO.File.Delete(oldStore_data.DataRoute);
                            Home.DeleteOldFile(oldStore_data.Store_data_ID);
                        }

                        string icons = Server.MapPath("~/Icon/" + icon[icon.Length - 1].ToUpper() + ".png");
                        Model.Store_data data = null;
                        if (System.IO.File.Exists(icons))
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = wenjian,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/" + icon[icon.Length - 1].ToUpper() + ".png"
                            };
                        }
                        else {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = wenjian,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = Home.GetNewFolderId(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])),
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/weizhi.png"
                            };
                        }

                       
                        Home.Insert(data);

                    }
                    else
                    {
                        string newfilename = filename;

                        //使用GUID 全球唯一标识符给文件重名称：GUID + 后缀名
                        newfilename = Guid.NewGuid() + filename.Substring(filename.LastIndexOf("."));

                        //获取存储的目标地址，也就是文件夹位置
                        string path = target + newfilename;
                        files[i].SaveAs(path);
                        cg = cg + 1;
                        paths.Add(path);


                        string[] icon = filename.Split('.');
                        //将文件信息保存到数据库

                        //覆盖重复文件
                        Model.Store_data oldStore_data = Home.GetFile(filename, Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]));
                        if (oldStore_data != null)
                        {
                            System.IO.File.Delete(oldStore_data.DataRoute);
                            Home.DeleteOldFile(oldStore_data.Store_data_ID);
                        }

                        string icons = Server.MapPath("~/Icon/" + icon[icon.Length - 1].ToUpper() + ".png");
                        Model.Store_data data = null;
                        if (System.IO.File.Exists(icons))
                        {
                                data = new Model.Store_data
                                {
                                    User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                    DataRoute = path,
                                    Real_FileName = filename,
                                    File_Size = Home.GetFileSize(files[i].ContentLength),
                                    Folder_ID = 0,
                                    EstablishTime = DateTime.Now,
                                    DeleteState = 0,
                                    SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                    icon = "/Icon/" + icon[icon.Length - 1].ToUpper() + ".png"
                                };
                        }
                        else
                        {
                            data = new Model.Store_data
                            {
                                User_ID = Convert.ToInt32(Request.Cookies["MyCook"]["UserID"]),
                                DataRoute = path,
                                Real_FileName = filename,
                                File_Size = Home.GetFileSize(files[i].ContentLength),
                                Folder_ID = 0,
                                EstablishTime = DateTime.Now,
                                DeleteState = 0,
                                SuffixName = filename.Substring(filename.LastIndexOf(".")),
                                icon = "/Icon/weizhi.png"
                            };
                        }
                        

                        Home.Insert(data);

                    }
                }

            }
            return Redirect("/Home/Classification?Keyword="+fenlei);
        }

        public ActionResult recycle() {
            ViewBag.User = Request.Cookies["MyCook"]["UserName"];
            return View(Home.recovery(Convert.ToInt32(Request.Cookies["MyCook"]["UserID"])));
        }


        /// <summary>
        /// 下载
        /// </summary>
        /// <returns></returns>
        public ActionResult Download()
        {
            string fileID = Request.QueryString["id"];//取得文件id
            Model.Store_data data = Home.GetFile(Convert.ToInt32(fileID));
            return File(new FileStream(data.DataRoute, FileMode.Open), "text/plain",
            data.Real_FileName);
        }


        /// <summary>
        /// 将文件放到回收站
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public ActionResult UpdateFile(string[] FileID)
        { 
            //string[] id = FileID.Split(',');
            int jg = 0;
            for (int i = 0; i < FileID.Length; i++)
            {
                jg = Home.UpdateFile(Convert.ToInt32(FileID[i]));
            }

            return Json(jg.ToString(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 将文件从回收站还原
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public ActionResult UpdateFile2(string[] FileID)
        {
            //string[] id = FileID.Split(',');
            int jg = 0;
            for (int i = 0; i < FileID.Length; i++)
            {
                jg = Home.UpdateFile2(Convert.ToInt32(FileID[i]));
            }

            return Json(jg.ToString(), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 彻底删除
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public ActionResult DeleteFile(string[] FileID)
        {
            int jg = 0;
            for (int i = 0; i < FileID.Length; i++)
            {
                Model.Store_data sta = Home.GetFile(Convert.ToInt32(FileID[i]));
                if (sta!=null)
                {
                    jg = Home.DeleteFile(Convert.ToInt32(FileID[i]));
                    System.IO.File.Delete(sta.DataRoute);
                }
                
            }

            return Json(jg.ToString(), JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult QueryTXT(string url)
        {


            string physicalPath = Server.MapPath(Server.UrlDecode(url));
            string extension = Path.GetExtension(physicalPath);
 
            string htmlUrl = "";
            switch(extension.ToLower())
            {
               case ".xls":
               case ".xlsx":
                   htmlUrl = PreviewExcel(physicalPath, url);
                   break;
               case ".doc":
               case ".docx":
                   htmlUrl = PreviewWord(physicalPath, url);
                   break;
               case ".txt":
                   htmlUrl = PreviewTxt(physicalPath, url);
                   return Json(htmlUrl, JsonRequestBehavior.AllowGet);
                   
               case ".pdf":
                   htmlUrl = PreviewPdf(physicalPath, url);
                   break;
               case ".jpg":
               case ".jpeg":
               case ".bmp":
               case ".gif":
               case ".png":
                   htmlUrl = PreviewImg(physicalPath, url);
                   break;
               default:
                   htmlUrl = PreviewOther(physicalPath, url);
                   break;
            }
            string urls = Url.Content(htmlUrl);
            return Redirect(urls);
        }
         /// <summary>
        /// 预览Excel
        /// </summary>
        public string PreviewExcel(string physicalPath, string url)
        {
           Microsoft.Office.Interop.Excel.Application application = null;
           Microsoft.Office.Interop.Excel.Workbook workbook = null;
            application= new Microsoft.Office.Interop.Excel.Application();
            object missing = Type.Missing;
            object trueObject = true;
           application.Visible = false;
           application.DisplayAlerts = false;
            workbook =application.Workbooks.Open(physicalPath, missing, trueObject, missing, missing,missing,
               missing, missing, missing, missing, missing, missing, missing, missing,missing);
            //Save Excelto Html
            object format = Microsoft.Office.Interop.Excel.XlFileFormat.xlHtml;
            string htmlName = Path.GetFileNameWithoutExtension(physicalPath) + ".html";
            String outputFile = Path.GetDirectoryName(physicalPath) + "\\" + htmlName;
           workbook.SaveAs(outputFile, format, missing, missing, missing,
                             missing, XlSaveAsAccessMode.xlNoChange, missing,
                             missing, missing, missing, missing);
           workbook.Close();
           application.Quit();
            return Path.GetDirectoryName(Server.UrlDecode(url)) + "\\" + htmlName;
        }

         /// <summary>
        /// 预览Word
        /// </summary>
        public string PreviewWord(string physicalPath, string url)
        {
           Microsoft.Office.Interop.Word._Application application = null;
           Microsoft.Office.Interop.Word._Document doc = null;
            application= new Microsoft.Office.Interop.Word.Application();
            object missing = Type.Missing;
            object trueObject = true;
            application.Visible= false;
           application.DisplayAlerts = WdAlertLevel.wdAlertsNone;
            doc =application.Documents.Open(physicalPath, missing, trueObject, missing, missing,missing,
               missing, missing, missing, missing, missing, missing, missing, missing,missing, missing);
            //Save Excelto Html
            object format = Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatHTML;
            string htmlName = Path.GetFileNameWithoutExtension(physicalPath) + ".html";
            String outputFile = Path.GetDirectoryName(physicalPath) + "\\" + htmlName;
           //doc.SaveAs2()
           doc.SaveAs(outputFile, format, missing, missing, missing,
                             missing, XlSaveAsAccessMode.xlNoChange, Type.Missing,
                             missing, missing, missing, missing);
            doc.Close();
           application.Quit();
            return Path.GetDirectoryName(Server.UrlDecode(url)) + "\\" + htmlName;
        }
        //Microsoft.Office.Core.MsoEncoding
        /// <summary>
        /// 预览Txt
        /// </summary>
        public string PreviewTxt(string physicalPath, string url)
        {
           
             //存在则读取

            StreamReader sr = new StreamReader(physicalPath, System.Text.Encoding.Default);
             string outData = sr.ReadToEnd();
             //关闭流
             sr.Close();
             return outData;
            //return Server.UrlDecode(url);
        }


        /// <summary>
        /// 预览Pdf
        /// </summary>
        public string PreviewPdf(string physicalPath, string url)
        {
            return Server.UrlDecode(url);
        }

        /// <summary>
        /// 预览图片
        /// </summary>
        public string PreviewImg(string physicalPath, string url)
        {
            return Server.UrlDecode(url);
        }

         /// <summary>
        /// 预览其他文件
        /// </summary>
        public string PreviewOther(string physicalPath, string url)
        {
            return Server.UrlDecode(url);
        }


        
    }
}