// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using PhotoLabeler.Croscutting.Interfaces;
using Serilog;

namespace PhotoLabeler.Croscutting.Implementations
{
	public class DebugService : IDebugService
	{

		private readonly ILogger _logger;

		public DebugService(ILogger logger)
		{
			_logger = logger;
		}

		public T MeasureExecution<T>(Func<T> functionToExecute, string functionName)
		{
			if (functionToExecute is null)
			{
				throw new ArgumentNullException(nameof(functionToExecute));
			}

			if (functionName is null)
			{
				throw new ArgumentNullException(nameof(functionName));
			}
			_logger.Debug($"Measuring {functionName} execution time...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var result = functionToExecute();
			stopwatch.Stop();
			_logger.Debug($"The function {functionName} took {stopwatch.Elapsed.TotalMilliseconds} milliseconds to execute.");
			return result;
		}

		public async Task<T> MeasureExecutionAsync<T>(string functionName, Func<Task<T>> functionToExecute)
		{
			if (functionToExecute is null)
			{
				throw new ArgumentNullException(nameof(functionToExecute));
			}

			if (functionName is null)
			{
				throw new ArgumentNullException(nameof(functionName));
			}
			_logger.Debug($"Measuring {functionName} execution time...");
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			var result = await functionToExecute();
			stopwatch.Stop();
			_logger.Debug($"The function {functionName} took {stopwatch.Elapsed.TotalMilliseconds} milliseconds to execute.");
			return result;
		}

	}
}
