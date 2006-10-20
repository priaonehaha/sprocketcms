/*
	// Sprocket AJAX Engine //
	-----------------------------------
	The code in this file is a very heavily modified version of the code found in
	an early version of Ajax.Net, by Michael Schwartz, which can be found at
	http://ajax.schwarz-interactive.de. The server-side code has been completely
	rewritten to suit Sprocket's requirements and the client side (this file) is maybe
	20% intact from the original version. The modifications build in Sprocket's
	authentication requirements and pass request handling directly to the Sprocket
	modules rather than an ASP.Net page class.
	-----------------------------------
	Nathan Ridley / snowdevil@gmail.com
*/

// Ensures existance of a universal request object that works across different browsers
if(typeof XMLHttpRequest == 'undefined')
	XMLHttpRequest = function() {
		try {
			return new ActiveXObject("Microsoft.XMLHTTP");
		} catch(ex) {
			return null;
		}
	}

/*
	Ajax methods are called via objects created dynamically by the server. The objects are
	named the same as the module that defines the server-side methods, and contain a method
	that has the same name as the server-side method. For example, the server could generate:
	Ajax.SomeModule = {
		FirstMethod : function() { Ajax.Request('SomeModule.FirstMethod', arguments); },
		SecondMethod : function() { Ajax.Request('SomeModule.SecondMethod', arguments); }
	}
	Let's assume First method is defined on the server like so:
	public int FirstMethod(string msg, int num, DateTime requestDate)
	
	In this case, to make an ajax request to SomeModule and call FirstMethod, you'd use
	something like the following javascript:
	
	Ajax.SomeModule.FirstMethod('my message', 7, new Date(), myCallbackFunction, myContextObject1, ..., myContextObjectN);
	
	You'd make sure you define a function somewhere called myCallbackFunction of course.
	The format of myCallbackFunction is:
	
	functionName(response, ajaxCallNumber, myContextObject1,...n).
	
	The arguments immediately following the callback functions are context objects.
	Context objects are simply arguments that are passed to your callback function, so ensure
	that your callback function defines the appropriate matching arguments in the function
	declaration.
*/
SprocketAjax = {
	RequestPool : [],
	AuthKey : '$AUTHKEY$',
	ApplicationRoot : '$APPLICATIONROOT$',
	
	CreateRequest : function() {
		for(var i=0; i<this.RequestPool.length; i++) {
			var x = this.RequestPool[i];
			if(x.readyState == 4) {
				x.abort();
				return x;
			}
		}
		var x = new XMLHttpRequest();
		this.RequestPool[this.RequestPool.length] = x;
		return x;
	},
	
	// This method is to be called from server-generated methods. Those methods are called in the
	// format: ModuleName.MethodName(arg1, ..., argN[, callbackFunction[, context1[..., contextN]]])
	// The "context" argument is passed back to the callback function.
	// The callback is called in the format: callbackFunction(responseObject, context1, ..., contextN)
	Request : function(moduleName, args) {
		var req = this.CreateRequest();
		var serverArgsCount = 0;
		for(var i=0; i<args.length; i++) {
			if(typeof args[i] == 'function') {
				var callback = args[i];
				var context = null;
				if(i+1 < args.length) {
					context = [];
					for(var k=i+1; k<args.length; k++)
						context[k-(i+1)] = args[k];
				}
				this.ExecuteCall(moduleName, args, callback, serverArgsCount, context);
				return;
			} else { // check the value of each argument
				if(args[i] != null)
					if(args[i].getFullYear) // is date
						args[i] = args[i].toUTCString();
				serverArgsCount++;
			}
		}
		this.ExecuteCall(moduleName, args, this.DefaultCallback, serverArgsCount, null);
	},
	
	callNumber : 0,
	
	// This method is used internally by the Request() method.
	ExecuteCall : function(moduleName, serverArgs, callback, serverArgsCount, context) {
		var cnum = this.callNumber++; // keep track of how many ajax calls have been made since the page was loaded
		this.WriteConsole('-&gt;[' + cnum + ']: ' + moduleName); // optionally log the call to a console area on the page
		var req = this.CreateRequest();
		req.onreadystatechange = function() { // define the callback function
			if(req.readyState == 4) {
				var contextArgs = context;
				var callnum = cnum;
				SprocketAjax.WriteConsole('&lt;-[' + cnum + ']: ' + moduleName +
					'<span onclick="this.nextSibling.style.display=\'\';">[Response]</span><div style="display:none">' + req.responseText + '</div>');
				try {
					var response = JSON.parse(req.responseText);
				} catch(ex) {
					if(!req.responseText)
						return;
					alert('There was an error parsing the JSON response. The error was:\n\n' + ex.message + '\n\nThe response text was:\n\n' + req.responseText);
				}
				if(response.__error)
					SprocketAjax.DefaultCallback(response);
				else {
					if(!contextArgs)
						callback(response, callnum);
					else {
						var call = 'callback(response, callnum';
						for(var i=0; i<contextArgs.length; i++)
							call += ', contextArgs[' + i + ']';
						call += ');';
						eval(call);
					}
				}
			}
		}
		
		// prepare the request for transmission
		req.open('POST', this.ApplicationRoot + 'AjaxRequest.ajax', true);
		
		// prepare the data object for transmission
		var preparedArgs = []
		for(var i=0; i<serverArgsCount; i++) {
			if(typeof serverArgs[i] == 'function')
				break;
			preparedArgs[i] = serverArgs[i];
		}
		var data = {
			ModuleName : moduleName,
			AuthKey : this.AuthKey,
			MethodArgs : preparedArgs
		}
		
		// send the data asyncronously to the server and return control to calling object
		req.send(JSON.stringify(data));
	},
	
	// This is the default callback method used when none is specified.
	DefaultCallback : function(response) {
		if(response.__error)
			alert(response.__error.replace(/\\r\\n/g,'\n'));
	},
	
	WriteConsole : function(message) {
		var c = document.getElementById('_AjaxConsole');
		if(c) c.innerHTML += '<div>' + message + '</div>';
	},
	
	LoadingMessage : '<h2>Loading</h2>' //'<div class="content-loading">Loading...</div>'
}
