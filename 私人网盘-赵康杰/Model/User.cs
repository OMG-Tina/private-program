//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.Folders = new HashSet<Folder>();
            this.Store_data = new HashSet<Store_data>();
        }
    
        public int User_ID { get; set; }
        public string User_Name { get; set; }
        public string User_Password { get; set; }
    
        public virtual ICollection<Folder> Folders { get; set; }
        public virtual ICollection<Store_data> Store_data { get; set; }
    }
}
