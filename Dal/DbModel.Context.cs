﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace com.caijunxiong.dal
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ManageEntities : DbContext
    {
        public ManageEntities()
            : base("name=ManageEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<MenuPrivilege> MenuPrivileges { get; set; }
        public virtual DbSet<Privilege> Privileges { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<SystemPrivilege> SystemPrivileges { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserInfo> UserInfoes { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserVerify> UserVerifies { get; set; }
        public virtual DbSet<Config> Configs { get; set; }
    }
}
