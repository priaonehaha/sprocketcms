// macro for getElementById; returns e if 
function $(e) { return (typeof(e) == 'object') ? e : document.getElementById(e); }

// is value equal to any of the following arguments (similar to SQL's IN operator)
function $in(value, arg1, arg2, arg99) {
	for(var i=1; i<arguments.length; i++)
		if(arguments[i] == value) return true;
	return false;
}

// finds all descendants of a specified element that have the named attribute which
// is optionally set to a specified value
function $childrenWithAttribute(parentElement, attributeName, attributeValue) {
	var found = (arguments.length <= 3) ? [] : arguments[3];
	var pe = $(parentElement);
	for(var i=0; i<pe.childNodes.length; i++) {
		if(!pe.childNodes[i].getAttribute) continue;
		var val = pe.childNodes[i].getAttribute(attributeName);
		if(val != '' && val != null && (val == attributeValue || !attributeValue))
			found[found.length] = pe.childNodes[i];
		$childrenWithAttribute(pe.childNodes[i], attributeName, attributeValue, found);
	}
	return found;
}

// finds the parent element that has the specified attribute, optionally with a specific
// value. If it doesn't exist, null is returned.
function $parentWithAttribute(element, attributeName, attributeValue) {
	if(!element.getAttribute)
		return null;
	var val = element.getAttribute(attributeName);
	if(val != '' && (val == attributeValue || !attributeValue))
		return element;
	if(!element.parentNode)
		return null;
	return $parentWithAttribute(element.parentNode, attributeName, attributeValue);
}

function $nullIfBlank(str) {
	if(str == '')
		return null;
	else
		return str;
}

function htmlEncode(str) {
	if(str == null) return '';
	return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}

function trim(str) {
	return str.replace(/^\s*((\S|(\S\s+\S))*)\s*$/, '$1');
}

function queryString(arg) {
	var qn = location.href.indexOf('?');
	if(qn == -1 || qn == location.href.length - 1) return null;
	var qs = location.href.substring(qn+1);
	var arr = qs.split(/&/g);
	for(var i=0; i<arr.length; i++) {
		var nvp = arr[i].split(/=/);
		if(nvp.length == 2 && nvp[0] == arg)
			return unescape(nvp[1]);
	}
	return null;
}

function parseDate(dateString) {
	return new Date(Date.parse(dateString));
}

function isValidEmail(email) {
	return (/^[A-Z0-9._%-]+@[A-Z0-9.-]+\.[A-Z]{2,4}$/i).test(email);
}

function getGMTOffset() {
	var dt = new Date();
	var sub = dt.getUTCDate() < dt.getDate();
	var hours;
	if(sub)
		hours = ((24 - dt.getUTCHours()) + dt.getHours());
	else
		hours = 0 - (dt.getUTCHours() - dt.getHours());
	return hours;
}

Guid = {
	Empty : '00000000-0000-0000-0000-000000000000'
};

// event handler class
function Event() {
	this.handlers = [];
}

Event.prototype.AddHandler = function(funcRef, context) {
	this.handlers[this.handlers.length] = {
		funcRef : funcRef,
		context : context
	};
}

Event.prototype.Fire = function() {
	for(var i=0; i<this.handlers.length; i++) {
		if(arguments.length > 0) {
			var str = 'arguments[0]';
			for(var j=1; j<arguments.length; j++)
				str += ', arguments[' + j + ']';
			var handler = this.handlers[i];
			if(this.handlers[i].context)
				eval('handler.funcRef.call(handler.context, ' + str + ');');
			else
				eval('handler.funcRef(' + str + ');');
		} else {
			if(this.handlers[i].context)
				this.handlers[i].funcRef.call(this.handlers[i].context);
			else
				this.handlers[i].funcRef(arguments);
		}
	}
}

// timer class
function Timer(interval) {
	this.interval = interval*1;
	this.Elapsed = new Event();
}

Timer.prototype.Start = function(interval) {
	this.Stop();
	if(interval) this.interval = interval*1;
	var refthis = this;
	this.intervalID = setInterval(function() { refthis.OnTimerElapsed(); }, this.interval);
}

Timer.prototype.RunOnce = function(interval) {
	this.Stop();
	if(interval) this.interval = interval*1;
	var refthis = this;
	this.intervalID = setTimeout(function() { refthis.OnTimerElapsed(); }, this.interval);
}

Timer.prototype.Stop = function() {
	if(!this.intervalID) return;
	clearTimeout(this.intervalID);
	this.intervalID = null;
}

Timer.prototype.OnTimerElapsed = function() {
	this.Elapsed.Fire();
}

function debug() {
	var str = 'DEBUG:';
	for(var i=0; i<arguments.length; i++)
		str += '\n' + arguments[i];
	alert(str);
}

/* The following stylesheet code is courtesy of http://www.hunlock.com/blogs/Totally_Pwn_CSS_with_Javascript */

function getCSSRule(ruleName, deleteFlag) {               // Return requested style obejct
   ruleName=ruleName.toLowerCase();                       // Convert test string to lower case.
   if (document.styleSheets) {                            // If browser can play with stylesheets
      for (var i=0; i<document.styleSheets.length; i++) { // For each stylesheet
         var styleSheet=document.styleSheets[i];          // Get the current Stylesheet
         var ii=0;                                        // Initialize subCounter.
         var cssRule=false;                               // Initialize cssRule. 
         do {                                             // For each rule in stylesheet
            if (styleSheet.cssRules) {                    // Browser uses cssRules?
               cssRule = styleSheet.cssRules[ii];         // Yes --Mozilla Style
            } else {                                      // Browser usses rules?
               cssRule = styleSheet.rules[ii];            // Yes IE style. 
            }                                             // End IE check.
            if (cssRule)  {                               // If we found a rule...
               if (cssRule.selectorText.toLowerCase()==ruleName) { //  match ruleName?
                  if (deleteFlag=='delete') {             // Yes.  Are we deleteing?
                     if (styleSheet.cssRules) {           // Yes, deleting...
                        styleSheet.deleteRule(ii);        // Delete rule, Moz Style
                     } else {                             // Still deleting.
                        styleSheet.removeRule(ii);        // Delete rule IE style.
                     }                                    // End IE check.
                     return true;                         // return true, class deleted.
                  } else {                                // found and not deleting.
                     return cssRule;                      // return the style object.
                  }                                       // End delete Check
               }                                          // End found rule name
            }                                             // end found cssRule
            ii++;                                         // Increment sub-counter
         } while (cssRule)                                // end While loop
      }                                                   // end For loop
   }                                                      // end styleSheet ability check
   return false;                                          // we found NOTHING!
}                                                         // end getCSSRule 

function killCSSRule(ruleName) {                          // Delete a CSS rule   
   return getCSSRule(ruleName,'delete');                  // just call getCSSRule w/delete flag.
}                                                         // end killCSSRule

function addCSSRule(ruleName) {                           // Create a new css rule
   if (document.styleSheets) {                            // Can browser do styleSheets?
      if (!getCSSRule(ruleName)) {                        // if rule doesn't exist...
         if (document.styleSheets[0].addRule) {           // Browser is IE?
            document.styleSheets[0].addRule(ruleName, null,0);      // Yes, add IE style
         } else {                                         // Browser is IE?
            document.styleSheets[0].insertRule(ruleName+' { }', 0); // Yes, add Moz style.
         }                                                // End browser check
      }                                                   // End already exist check.
   }                                                      // End browser ability check.
   return getCSSRule(ruleName);                           // return rule we just created.
} 