﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shop.Models.Data
{
    //Класс контекст данных
    [Table("tblCategories")]
    public class CategoryDTO
    {
        [Key]//Первичный ключ
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sorting { get; set; }
    }
}