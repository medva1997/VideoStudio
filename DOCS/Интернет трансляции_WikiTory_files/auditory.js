function toggleAuditoryMenu(){
	if(document.getElementById('auditory_more').style.display == 'none'){
		document.getElementById('auditory_more').style.display = 'block';
	} else {
		document.getElementById('auditory_more').style.display = 'none';
	}
	return false;
}

var req;
if (window.XMLHttpRequest) {
	req = new XMLHttpRequest();
} else if (window.ActiveXObject) {
	try {
		req = new ActiveXObject("Msxml2.XMLHTTP");
	} catch(e) {
		try {
			req = new ActiveXObject("Microsoft.XMLHTTP");
		} catch(e) {
			throw(e);
		}
	}
}
req.open('GET', '/auditory_menu/json/menu.json', false);
req.send(null);
var menu = eval('(' + req.responseText + ')');

var html = '';
if(typeof menu.visible != 'undefined'){
	for(var i=0; i < menu.visible.length; i++){
		html += '<li><a href="'+ menu.visible[i].url + '">' + menu.visible[i].name + '</a></li>';
	}
}
html += '<li><a href="#" onclick="toggleAuditoryMenu(); return false;">Ещё<img src="http://auditory.ru/auditory_menu/menu_down.png" alt="more" /></a>';
html += '<ul id="auditory_more" style="display:none;">';
if(typeof menu.hidden != 'undefined'){
	for(var i=0; i < menu.hidden.length; i++){
		html += '<li><a href="'+ menu.hidden[i].url + '">' + menu.hidden[i].name + '</a></li>';
	}
}
html += '</ul></li>';
var fileref=document.createElement("link");
fileref.setAttribute("rel", "stylesheet");
fileref.setAttribute("type", "text/css");
fileref.setAttribute("media", "screen");
fileref.setAttribute("href", "http://auditory.ru/auditory_menu/auditory.css");
document.getElementsByTagName("head")[0].appendChild(fileref);
var Menualert = false;
if (typeof menu.alert != 'undefined'){
	Menualert = '';
	for(var i=0; i < menu.alert.length; i++){
		Menualert += menu.alert[i];
	}
}

function auditoryMenu () {
	var container=document.getElementById('auditoryContainer');
	var ul=document.getElementById('auditory');
	if (ul === null) return false;
	ul.innerHTML = html;
	ul.setAttribute("style","display:block;");
	var login=document.getElementById('auditory_login');
	if (login) login.setAttribute("style","display:block;");
	if(Menualert){
		var div = document.createElement("div");
		div.setAttribute("id", "auditory_alert");
		div.innerHTML = Menualert;
		container.appendChild(div);
	}
};
auditoryMenu();
