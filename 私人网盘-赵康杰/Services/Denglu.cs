using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class Denglu
    {

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public int InsertUser(Model.User User) { 
            using(var db = new  Model.ZKJSkyDriveEntities()){
                db.Entry<Model.User>(User).State = System.Data.Entity.EntityState.Added;
                return db.SaveChanges();
            }
        }




        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public Model.User Login(Model.User User)
        {
            using(var db = new Model.ZKJSkyDriveEntities()){
                return db.Set<Model.User>()
                    .FirstOrDefault(x => x.User_Name == User.User_Name && x.User_Password == User.User_Password);
            }

        }

        /// <summary>
        /// 根据用户id查询用户信息
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public Model.User GetUserByID(int UserID)
        {
            using (Model.ZKJSkyDriveEntities db = new Model.ZKJSkyDriveEntities())
            {
                return db.Set<Model.User>().Find(UserID);
            }
        }
    }
}
