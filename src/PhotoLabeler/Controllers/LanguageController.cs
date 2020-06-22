// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PhotoLabeler.Constants;
using PhotoLabeler.Data.Repositories.Interfaces;

namespace PhotoLabeler.Controllers
{
	/// <summary>
	/// The language controller class
	/// </summary>
	/// <seealso cref="ControllerBase" />
	[Route("~/[controller]")]
	public class LanguageController : ControllerBase
	{

		private readonly RequestLocalizationOptions _localizationOptions;

		private readonly IAppConfigRepository _appConfigRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageController" /> class.
		/// </summary>
		/// <param name="localizationOptions">The localization options.</param>
		/// <param name="appConfigRepository">The application configuration repository.</param>
		public LanguageController(RequestLocalizationOptions localizationOptions, IAppConfigRepository appConfigRepository)
		{
			_localizationOptions = localizationOptions;
			_appConfigRepository = appConfigRepository;
		}

		/// <summary>
		/// Indexes the specified new language.
		/// </summary>
		/// <param name="cultureName">The new language.</param>
		/// <param name="redirectUri">The redirect URI.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">cultureName
		/// or
		/// redirectUri</exception>
		public IActionResult Index(string cultureName, string redirectUri)
		{
			if (cultureName is null)
			{
				throw new ArgumentNullException(nameof(cultureName));
			}

			if (redirectUri is null)
			{
				throw new ArgumentNullException(nameof(redirectUri));
			}

			var newCulture = _localizationOptions.SupportedCultures.SingleOrDefault(c => c.Name == cultureName);
			if (newCulture is null)
			{
				newCulture = CultureInfo.CreateSpecificCulture(cultureName);
			}
			if (newCulture is null)
			{
				newCulture = _localizationOptions.DefaultRequestCulture.Culture;
			}
			HttpContext.Response.Cookies.Append(
				CookieRequestCultureProvider.DefaultCookieName,
				CookieRequestCultureProvider.MakeCookieValue(
					new RequestCulture(newCulture)));

			return LocalRedirect(redirectUri + "?refresh=" + DateTime.Now.Ticks.ToString());
		}

		/// <summary>
		/// Sets the by configuration.
		/// </summary>
		/// <returns></returns>
		[HttpGet("SetCultureByConfig")]
		public async Task<IActionResult> SetCultureByConfig()
		{
			var config = await _appConfigRepository.GetAppConfigByKeyAsync(ConfigConstants.LanguageConfigKey);
			if (config == null)
			{
				await _appConfigRepository.SetEntryAsync(ConfigConstants.LanguageConfigKey, _localizationOptions.DefaultRequestCulture.Culture.Name);
			}

			return Index(config?.Value ?? _localizationOptions.DefaultRequestCulture.Culture.Name, "/");
		}
	}
}
