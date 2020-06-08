using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Models.ViewModels.Pages
{
    public class SidebarViewModel
    {
        public int Id { get; set; }

        [AllowHtml]//разрешает теги html внутри тела
        public string Body { get; set; }

        public SidebarViewModel(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }

        public SidebarViewModel() { }
    }
}