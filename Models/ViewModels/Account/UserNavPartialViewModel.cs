using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Account
{
    public class UserNavPartialViewModel
    {
        [DisplayName("Имя")]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        public string LastName { get; set; }
    }
}