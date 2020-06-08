using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shop.Models.Data
{
    [Table("tblUserRoles")]
    public class UserRoleDTO
    {
        [Key, Column(Order = 0)]//Column - задаем порядок выполнения для первичных ключей
        public int UserId { get; set; }

        [Key, Column(Order = 1)]
        public int RoleId { get; set; }

        [ForeignKey("UserId")]//устанавливаем вторичный ключ для связывания данных из другого DTO
        public virtual UserDTO User { get; set; }

        [ForeignKey("RoleId")]
        public virtual RoleDTO Role { get; set; }
    }
}