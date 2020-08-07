// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using PhotoLabeler.Croscutting.Interfaces;
using Serilog;

namespace PhotoLabeler.Croscutting.Implementations
{
	/// <summary>
	/// Class with services related to cryptographic operations
	/// </summary>
	public class CryptoService : ICryptoService
	{

		private readonly ILogger _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="CryptoService"/> class.
		/// </summary>
		/// <param name="logger">The logger.</param>
		public CryptoService(ILogger logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Gets the MD5 signature from file asynchronous.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		public async Task<string> GetMd5FromFileAsync(string file, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				_logger.Debug("Canceling GetMd5FromFileAsync. Token has cancellation requested.");
				throw new TaskCanceledException("The task was canceled");
			}
			_logger.Debug($"Getting MD5 for {file}...");
			using var md5 = MD5.Create();
			using var stream = File.OpenRead(file);
			var md5String = await Task.Run(() => Convert.ToBase64String(md5.ComputeHash(stream)), cancellationToken);
			_logger.Debug($"Md5 for {file} computed successfully: {md5String}.");
			return md5String;
		}
	}
}
