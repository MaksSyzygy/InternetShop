Fileman - утилита файл-менеджер, позволяющая вставлять картинки на страницы сайта
Подключается в ckeditor/config.js
В editorconfig вставляется следующий код:
var roxyFileman = '/fileman/index.html?integration=ckeditor';
    config.filebrowserBrowseUrl = roxyFileman;
    config.filebrowserImageBrowseUrl = roxyFileman + '&type=image';
    config.removeDialogTabs = 'link:upload;image:upload';