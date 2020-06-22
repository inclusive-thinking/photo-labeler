using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoLabeler.Interfaces
{
	/// <summary>
	/// Exposes methods to read options and update values in configuration file
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="Microsoft.Extensions.Options.IOptions{T}" />
	public interface IWritableOptions<out T> : IOptions<T> where T : class, new()
	{
		/// <summary>
		/// Updates the config associated to <typeparamref name="T"/> and apply the specified <paramref name="applyChanges"/> function.
		/// </summary>
		/// <param name="applyChanges">The apply changes.</param>
		void Update(Action<T> applyChanges);
	}
}