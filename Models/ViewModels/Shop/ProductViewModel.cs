using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Models.ViewModels.Shop
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]//Обязательное поле
        [DisplayName("Имя продукта")]
        public string Name { get; set; }
        public string Title { get; set; }

        [Required]
        [DisplayName("Описание")]
        public string Description { get; set; }
        [DisplayName("Цена")]
        public decimal Price { get; set; }
        public string CategoryName { get; set; }

        [Required]
        [DisplayName("Категория продукта")]
        public int CategoryId { get; set; }

        [DisplayName("Изображение")]
        public string ImageName { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }//SelectListItem - коллекция элементов модели
        public IEnumerable<string> GalleryImages { get; set; }//коллекция картинок

        public ProductViewModel(ProductDTO row)//инициализируем для связи и дальнейшего отображения данных в представлении
        {
            Id = row.Id;
            Name = row.Name;
            Title = row.Title;
            Description = row.Description;
            Price = row.Price;
            CategoryName = row.CategoryName;
            CategoryId = row.CategoryId;
            ImageName = row.ImageName;
        }

        public ProductViewModel() { }
    }
}