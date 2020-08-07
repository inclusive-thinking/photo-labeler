// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace PhotoLabeler.Croscutting.Interfaces
{
	/// <summary>
	/// Abstraction of CryptoService, with methods related to cryptograhpic operations
	/// </summary>
	public interface ICryptoService
	{
		/// <summary>
		/// Gets the MD5 signature from file asynchronous.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		Task<string> GetMd5FromFileAsync(string file, CancellationToken cancellationToken);
	}
}
