using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class Home
    {

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="NewPwd"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int UpdatePwd(string NewPwd,Model.User user) {
            using (var db = new Model.ZKJSkyDriveEntities()) {
                Model.User odduser = db.Set<Model.User>().FirstOrDefault(x=>x.User_Name==user.User_Name && x.User_Password==user.User_Password);
                odduser.User_Password =  NewPwd;
                db.Entry<Model.User>(odduser).State = System.Data.Entity.EntityState.Modified;
                return db.SaveChanges();
            } 
        }


        /// <summary>
        /// 查询用户所有文件夹
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public List<Model.Folder> GetFolderList(int UserId)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>()
                    .Where(x => x.FounderID == UserId && x.FatherFolderID == 0 && x.DeleteState == 0)
                    .OrderByDescending(x => x.EstablishTime)
                    .ToList();
            }
        }

        /// <summary>
        /// 查询用户所有文件
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public List<Model.Store_data> GetAll(int UserID)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>()
                    .Where(x => x.User_ID == UserID && x.Folder_ID == 0 && x.DeleteState == 0)
                    .OrderByDescending(x => x.EstablishTime)
                    .ToList();
            }
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int Insert(Model.Store_data data)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                db.Entry<Model.Store_data>(data).State = System.Data.Entity.EntityState.Added;
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 根据文件id查询文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Model.Store_data GetFile(int id)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>().Find(id);
            }
        }

        /// <summary>
        /// 根据文件名查询文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Model.Store_data GetFile(string FileName, int id)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>().FirstOrDefault(x => x.Real_FileName == FileName && x.User_ID == id);
            }
        }



        /// <summary>
        /// 新增文件夹
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public int InsertFolder(Model.Folder folder)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                db.Entry<Model.Folder>(folder).State = System.Data.Entity.EntityState.Added;
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 根据文件夹名查询文件夹
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public Model.Folder GetFolder(string FolderName)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>().FirstOrDefault(x => x.Folder_Name == FolderName);
            }
        }

        /// <summary>
        /// 根据文件名查询文件夹
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public Model.Folder GetFolderByName(string FolderName, int UserId)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>().FirstOrDefault(x => x.Folder_Name == FolderName && x.FounderID == UserId);
            }
        }

        /// <summary>
        /// 验证文件夹是否重复
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public Model.Folder YanZhenFolderByName(string FolderName, int FatherFolderID, int UserId)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>().FirstOrDefault(x => x.Folder_Name == FolderName && x.FatherFolderID == FatherFolderID && x.FounderID == UserId);
            }
        }

        /// <summary>
        /// 清除重复的旧文件
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public bool DeleteOldFile(int FileID) {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                db.Database.ExecuteSqlCommand("delete from Store_data  where Store_data_ID  =  '" + FileID + "'");
                return db.SaveChanges()>0;

            }
        }

        /// <summary>
        /// 根据文件id查询文件夹
        /// </summary>
        /// <param name="FolderName"></param>
        /// <returns></returns>
        public Model.Folder GetFolderByID(int FolderID, int UserId) 
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>().FirstOrDefault(x => x.Folder_ID == FolderID && x.FounderID == UserId && x.DeleteState == 0);
            }
        }

        /// <summary>
        /// 根据父ID查询文件夹
        /// </summary>
        /// <param name="Folder"></param>
        /// <returns></returns>
        public List<Model.Folder> GetFolderByID(int FolderID)
        {
            var db = new Model.ZKJSkyDriveEntities();

            return db.Set<Model.Folder>()
                .Where(x => x.FatherFolderID == FolderID && x.DeleteState == 0)
                .OrderByDescending(x => x.EstablishTime)
                .ToList();
        }

        public int GetNewFolderId(int UserId)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Folder>().Where(x => x.FounderID == UserId).Max(x => x.Folder_ID);
            }
        }


        /// <summary>
        /// 根据文件夹id查询文件
        /// </summary>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public List<Model.Store_data> GetShareDataByID(int FolderID)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>()
                    .Where(x => x.Folder_ID == FolderID && x.DeleteState == 0)
                    .OrderByDescending(x => x.EstablishTime)
                    .ToList();
            }
        }

        /// <summary>
        /// 计算文件大小
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public string GetFileSize(int size)
        {
            string FileSize = "";
            if (size != 0)
            {
                if (size >= 1073741824)
                {
                    FileSize = System.Math.Round(Convert.ToDouble((double)size / (double)1073741824), 2).ToString() + "GB";  //GB
                }
                else if (size >= 1048576)
                {
                    FileSize = System.Math.Round(Convert.ToDouble((double)size / (double)1048576), 2).ToString() + "MB";
                }
                else if (size >= 1024)
                {

                    FileSize = System.Math.Round(Convert.ToDouble((double)size / (double)1024), 2).ToString() + "KB";
                    int a = size / 1024 * 100;
                    int b = size / 1024;
                }
                else
                {
                    FileSize = size.ToString() + "B";                   
                }
            }
            else
            {
                FileSize = size.ToString() + "B";
            }
            return FileSize;
        }


        /// <summary>
        /// 根据后缀查询文件
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="SuffixName"></param>
        /// <returns></returns>
        public List<Model.Store_data> GetStore_dataBySuffixName(int Uid,string SuffixName)
        { 
            using(var db = new  Model.ZKJSkyDriveEntities()){
                return db.Set<Model.Store_data>().Where(x => x.SuffixName == SuffixName && x.User_ID == Uid && x.DeleteState == 0).ToList();
            }
        }

        /// <summary>
        /// 回收站文件
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="SuffixName"></param>
        /// <returns></returns>
        public List<Model.Store_data> recovery(int Uid)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>().Where(x => x.DeleteState == 1 && x.User_ID == Uid).ToList();
            }
        }

        /// <summary>
        /// 查询其他文件
        /// </summary>
        /// <param name="Uid"></param>
        /// <param name="SuffixName"></param>
        /// <returns></returns>
        public List<Model.Store_data> GetStore_dataBySuffixName(int Uid)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.Store_data>().Where(x =>x.SuffixName=="weizhi.png" && x.User_ID == Uid).ToList();
            }
        }


        /// <summary>
        /// 修改文件状态为回收站
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public int UpdateFile(int FileID) {
            using (var db = new Model.ZKJSkyDriveEntities()) {
                Model.Store_data store = db.Set<Model.Store_data>().Find(FileID);
                store.DeleteState = 1;
                store.DeleteTime = DateTime.Now;
                db.Entry<Model.Store_data>(store).State = System.Data.Entity.EntityState.Modified;
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 还原文件
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public int UpdateFile2(int FileID)
        {
            using (var db = new Model.ZKJSkyDriveEntities())
            {
                Model.Store_data store = db.Set<Model.Store_data>().Find(FileID);
                store.DeleteState = 0;
                store.EstablishTime = DateTime.Now;
                db.Entry<Model.Store_data>(store).State = System.Data.Entity.EntityState.Modified;
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 彻底删除
        /// </summary>
        /// <param name="FileID"></param>
        /// <returns></returns>
        public int DeleteFile(int FileID)
        {
            using (var db = new Model.ZKJSkyDriveEntities()) {
                Model.Store_data store = db.Set<Model.Store_data>().Find(FileID);
                db.Entry<Model.Store_data>(store).State = System.Data.Entity.EntityState.Deleted;
                return db.SaveChanges();
            }
        }


        public Model.Store_data GetfileByid(int id) {
            using (var db = new Model.ZKJSkyDriveEntities()) {
                return db.Set<Model.Store_data>().Find(id);
            }
        }
    }
}
