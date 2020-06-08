using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Models.ViewModels.Pages
{
    public class PageViewModel
    {
        public int Id { get; set; }

        [Required]//поле обязательное для заполнения
        [StringLength(50, MinimumLength = 3)]//длина строки
        public string Title { get; set; }
        public string Description { get; set; }

        [Required]
        [StringLength(int.MaxValue, MinimumLength = 0)]

        [AllowHtml]
        public string Body { get; set; }
        public int Sorting { get; set; }

        [Display(Name = "Sidebar")]//задает имя для отображения на странице
        public bool HasSidebar { get; set; }

        public PageViewModel(PagesDTO row)
        {
            Id = row.Id;
            Title = row.Title;
            Description = row.Description;
            Body = row.Body;
            Sorting = row.Sorting;
            HasSidebar = row.HasSidebar;
        }

        public PageViewModel() { }
    }
}