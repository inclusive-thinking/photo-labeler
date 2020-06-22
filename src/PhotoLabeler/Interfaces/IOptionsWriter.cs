using Newtonsoft.Json.Linq;

using System;

namespace PhotoLabeler.Interfaces
{
	/// <summary>
	/// Exposes methods to update configurations on runtime.
	/// </summary>
	public interface IOptionsWriter
	{
		/// <summary>
		/// Updates the options.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="reload">if set to <c>true</c> [reload].</param>
		void UpdateOptions(Action<JObject> callback, bool reload = true);
	}
}