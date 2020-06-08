$(function () {
    /* Подтверждение удаления страницы */

    $("a.delete").click(function () {
        if (!confirm("Подтверждение удаления страницы")) return false;
    });

    /*Скрипт сортировки - перетаскивания строк на странице (drag and drop)*/
    $("table#pages tbody").sortable({//берем таблицу по классу pages и тело tbody, вызываем sortable
        items: "tr:not(.home)",//любую строку кроме class = home
        placeholder: "ui-state-highlight",//событие когда цепляем строку(tr) мышкой
        update: function () {//обновляем
            var ids = $("table#pages tbody").sortable("serialize");
            var url = "/Admin/Page/ReorderPages";//хранение url-адреса метода, пересортировка страниц

            //post-метод отправки
            $.post(url, ids, function (data) {

            });
        }
    });
});