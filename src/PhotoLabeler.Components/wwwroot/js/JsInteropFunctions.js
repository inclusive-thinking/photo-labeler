window.jsInteropFunctions = {
	showPrompt: function (message) {
		return prompt(message, 'Type anything here');
	},
	focusSelectedItemInsideContainer: function (containerId) {
		var focusedItem = $("#" + containerId + " [tabindex='0']:first");
		focusedItem.focus();
	},

	scrollIntoView: function (element) {
		element.scrollIntoView();
	}
};
