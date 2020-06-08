using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Shop.Models.Data
{
    //Контекст БД
    public class DB : DbContext
    {
        public DbSet<PagesDTO> Pages { get; set; }//кеш данных(контекст данных), связь между моделью и БД
        public DbSet<SidebarDTO> Sidebars { get; set; }
        public DbSet<CategoryDTO> Categories { get; set; }
        public DbSet<ProductDTO> Products { get; set; }
        public DbSet<UserDTO> Users { get; set; }
        public DbSet<RoleDTO> Roles { get; set; }
        public DbSet<UserRoleDTO> UserRoles { get; set; }
        public DbSet<OrderDTO> Orders { get; set; }
        public DbSet<OrderDetailsDTO> OrderDetails { get; set; }
    }
}