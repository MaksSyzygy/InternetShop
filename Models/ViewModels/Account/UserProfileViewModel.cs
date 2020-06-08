using Shop.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

//Класс для ПРОСМОТРА личного кабинета
namespace Shop.Models.ViewModels.Account
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }

        [Required]//Обязательное поле, при проверке на валидность если поле не заполнено - вернется сообщение про ошибку
        [DisplayName("Имя")]
        public string FirstName { get; set; }

        [Required]
        [DisplayName("Фамилия")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]//DataType - Конкретный тип для типов данных
        [DisplayName("Email адрес")]
        public string Email { get; set; }

        [Required]
        [DisplayName("Логин")]
        public string UserName { get; set; }

        [DisplayName("Пароль")]
        public string Password { get; set; }

        [DisplayName("Подтверждение пароля")]
        public string ConfirmPassword { get; set; }//для подтверждения пароля

        public UserProfileViewModel(UserDTO row)//для связи с моделью в параметре указываем DTO представления
        {
            Id = row.Id;
            FirstName = row.FirstName;
            LastName = row.LastName;
            Email = row.Email;
            UserName = row.UserName;
            Password = row.Password;
        }

        public UserProfileViewModel() { }
    }
}