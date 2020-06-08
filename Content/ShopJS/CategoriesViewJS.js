$(function () {

    /* Добавление новой категории */

    /*Объявляем и инициализируем переменные для функций*/

    var newCatA = $("a#newcata");/*Класс ссылки для добавления*/
    var newCatTextInput = $("#newcatname");/*Класс текстового поля ввода*/
    var ajaxText = $("span.ajax-text");/*Класс картинки для загрузки*/
    var table = $("table#pages tbody");/*Класс таблицы вывода*/

    /*Функция нажатия по Enter*/

    newCatTextInput.keyup(function (e) {
        if (e.keyCode == 13) {
            newCatA.click();
        }
    });

    /*Функция CLick*/

    newCatA.click(function (e) {
        e.preventDefault();

        var catName = newCatTextInput.val();

        if (catName.length < 3) {
            alert("Имя категории должно быть больше 3-х символов");

            return false;
        }

        ajaxText.show();

        var url = "/admin/shop/AddNewCategory";

        $.post(url, { catName: catName }, function (data) {
            var response = data.trim();

            if (response == "titletaken") {//если метод AddNewCategory контроллера вернет titletaken
                ajaxText.html("<span class='alert alert-danger'>Этот заголовок уже используется!</span>");
                setTimeout(function () {
                    ajaxText.fadeOut("fast", function () {
                        ajaxText.html("<img src='/Content/img/loading.gif' height='30 />'");
                    });
                }, 2000);
                return false;
            }
            else {
                if (!$("table#pages").length) {
                    location.reload();
                }
                else {
                    ajaxText.html("<span class='alert alert-success'>Категория успешно добавлена!</span>");
                    setTimeout(function () {
                        ajaxText.fadeOut("fast", function () {
                            ajaxText.html("img src='/Content/img/loading.gif' height='30'");
                        });
                    }, 2000);

                    newCatTextInput.val("");

                    var toAppend = $("table#pages tbody tr:last").clone();
                    toAppend.attr("id", "id_" + data);
                    toAppend.find("#item_Name").val(catName);
                    toAppend.find("a.delete").attr("href", "/admin/shop/DeleteCategory/" + data);
                    table.append(toAppend);
                    table.sortable("refresh");
                }
            }
        });
    });

    /* Подтверждение удаления категории */

    $("body").on("click", "a.delete", function () {
        if (!confirm("Подтверждение удаления категории")) return false;
    });

    /*Переименование категорий*/

    var originalTextBoxValue;

    $("table#pages input.text-box").dblclick(function () {
        originalTextBoxValue = $(this).val();
        $(this).attr("readonly", false);
    });

    $("table#pages input.text-box").keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).blur();
        }
    });

    $("table#pages input.text-box").blur(function () {
        var $this = $(this);
        var ajaxdiv = $this.parent().parent().parent().find(".ajaxdivtd");
        var newCatName = $this.val();
        var id = $this.parent().parent().parent().parent().parent().attr("id").substring(3);
        var url = "/admin/shop/RenameCategory";

        if (newCatName.length < 3) {
            alert("Имя категории должно быть больше 3-х символов");
            $this.attr("readonly", true);
            return false;
        }

        $.post(url, { newCatName: newCatName, id: id }, function (data) {
            var response = data.trim();

            if (response == "titletaken") {
                $this.val(originalTextBoxValue);
                ajaxdiv.html("<div class='alert alert-danger'>Этот заголовок занят</div>").show();
            }
            else {
                ajaxdiv.html("<div class='alert alert-success'>Имя категории изменено</div>").show();
            }

            setTimeout(function () {
                ajaxdiv.fadeOut("fast", function () {
                    ajaxdiv.html("");
                });
            }, 3000);
        }).done(function () {
            $this.attr("readonly", true);
        });
    });

    /*Скрипт сортировки - перетаскивания строк на странице (drag and drop)*/
    $("table#pages tbody").sortable({//берем таблицу по классу pages и тело tbody, вызываем sortable
        items: "tr:not(.header)",//любую строку кроме class = header
        placeholder: "ui-state-highlight",//событие когда цепляем строку(tr) мышкой
        update: function () {//обновляем
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Shop/ReorderCategories";//хранение url-адреса метода, пересортировка страниц

            //post-метод отправки
            $.post(url, ids, function (data) {

            });
        }
    });
});