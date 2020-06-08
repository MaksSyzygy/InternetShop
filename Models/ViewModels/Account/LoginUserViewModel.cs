using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Account
{
    public class LoginUserViewModel
    {
        [Required]
        [DisplayName("Логин")]
        public string UserName { get; set; }

        [Required]
        [DisplayName("Пароль")]
        public string Password { get; set; }

        [DisplayName("Запомнить в системе")]
        public bool RememberMe { get; set; }//галочка запомнить в системе
    }
}