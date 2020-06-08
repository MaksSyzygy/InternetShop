using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shop.Models.ViewModels.Shop
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sorting { get; set; }

        public CategoryViewModel(CategoryDTO row)//из кэша данных CategoryDTO выбираем элемент и присваиваем для отображения
        {
            Id = row.Id;
            Name = row.Name;
            Description = row.Description;
            Sorting = row.Sorting;
        }

        public CategoryViewModel() { }
    }
}