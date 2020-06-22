window.interopFunctions = {
	focus: function (element) {
		if (element === null || element === undefined) {
			return false;
		}

		if (element.tagName !== "INPUT" && element.tagName !== "BUTTON" && element.tagName !== "A") {
			var attributeValue = element.getAttribute("tabindex");
			if (attributeValue === null || attributeValue === "" || attributeValue === "0") {
				element.setAttribute("tabindex", "-1");
			}
		}
		return element.focus();
	},
	alert: function (text) {
		alert(text);
	},
	focusByQuerySelector: function (selector) {
		var element = document.querySelector(selector);
		if (element !== null) {

			if (element.tagName !== "INPUT" && element.tagName !== "BUTTON" && element.tagName !== "A") {
				var attributeValue = element.getAttribute("tabindex");
				if (attributeValue === null || attributeValue === "") {
					element.setAttribute("tabindex", "-1");
				}
			}

			return element.focus();
		}
		return false;
	},
	focusInHeader: function (level) {
		var element = document.querySelector("h" + level);
		if (element !== null) {
			var attributeValue = element.getAttribute("tabindex");
			if (attributeValue === null || attributeValue === "" || attributeValue === "0") {
				element.setAttribute("tabindex", "-1");
			}
			element.focus();
		}
	},
	click: function (element) {
		element.click();
	},
	setTitle: function (title) {
		document.title = title;
	},
	getTitle: function () {
		return document.title;
	},
	confirm: function (message) {
		return confirm(message);
	}
};