// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace PhotoLabeler.Croscutting.Interfaces
{
	/// <summary>
	/// Exposes methods to help with debugging.
	/// </summary>
	public interface IDebugService
	{
		/// <summary>
		/// Measures the execution of the <paramref name="functionToExecute"/>.
		/// </summary>
		/// <typeparam name="T">The type of returned result</typeparam>
		/// <param name="functionToExecute">The function to execute.</param>
		/// <param name="functionName">Name of the function.</param>
		/// <returns>the original result of executed function.</returns>
		T MeasureExecution<T>(Func<T> functionToExecute, string functionName);

		/// <summary>
		/// Measures the execution of the <paramref name="functionToExecute"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="functionName">Name of the function.</param>
		/// <param name="functionToExecute">The function to execute.</param>
		/// <returns></returns>
		Task<T> MeasureExecutionAsync<T>(string functionName, Func<Task<T>> functionToExecute);
	}
}
