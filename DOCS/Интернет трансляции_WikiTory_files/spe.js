var oldContent;

document.observe('dom:loaded', function() {
  oldContent = $('bodyContent').innerHTML;
});

function spe_ajax(articleTitle) {
  new Ajax.Request('http://wiki.auditory.ru/extensions/antiplag/spe_against_plagiat.php?cap=' + articleTitle, {
    method: 'get',
    onSuccess: function(transport){
      var response = transport.responseText || '<div class="spediv">Нет ответа от сервера</div>';
      $('bodyContent').update('<div class="spediv"><strong>' + response + '</strong><br /><a style="text-decoration:none;border-bottom:1px dashed;" href="javascript:void(0);" onClick="returnOriginalArticle();">Вернуться к статье</a>.</div>');
    },
    onFailure: function(){ $('auditory').scrollTo(); $('bodyContent').update('<div class="spediv">Что-то пошло не так (или не туда).</div>'); },
    onLoading: function(){ $('auditory').scrollTo(); $('bodyContent').update('<div class="spediv" align="center"><img src="/skins/wikitory/spe_loading.gif" /><br /><br />Подождите, проверяем...</div>'); }
  });
}

function returnOriginalArticle () {
  $('bodyContent').update(oldContent);
  return false;
}
