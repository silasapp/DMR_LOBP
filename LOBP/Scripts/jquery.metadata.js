/*******************************************************************************
 jquery.mb.components
 Copyright (c) 2001-2010. Matteo Bicocchi (Pupunzi); Open lab srl, Firenze - Italy
 email: mbicocchi@open-lab.com
 site: http://pupunzi.com

 Licences: MIT, GPL
 http://www.opensource.org/licenses/mit-license.php
 http://www.gnu.org/licenses/gpl.html
 ******************************************************************************/

(function($){$.extend({metadata:{defaults:{type:'class',name:'metadata',cre:/({.*})/,single:'metadata'},setType:function(a,b){this.defaults.type=a;this.defaults.name=b},get:function(a,b){var c=$.extend({},this.defaults,b);if(!c.single.length)c.single='metadata';var d=$.data(a,c.single);if(d)return d;d="{}";if(c.type=="class"){var m=c.cre.exec(a.className);if(m)d=m[1]}else if(c.type=="elem"){if(!a.getElementsByTagName)return undefined;var e=a.getElementsByTagName(c.name);if(e.length)d=$.trim(e[0].innerHTML)}else if(a.getAttribute!=undefined){var f=a.getAttribute(c.name);if(f)d=f}if(d.indexOf('{')<0)d="{"+d+"}";d=eval("("+d+")");$.data(a,c.single,d);return d}}});$.fn.metadata=function(a){return $.metadata.get(this[0],a)}})(jQuery);