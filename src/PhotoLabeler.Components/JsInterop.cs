using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace PhotoLabeler.Components
{
	public class JsInterop
	{
		public static ValueTask<string> Prompt(IJSRuntime jsRuntime, string message)
		{
			return jsRuntime.InvokeAsync<string>(
				"jsInteropFunctions.showPrompt",
				message);
		}

		public static async Task ConvertListToTree(IJSRuntime jsRuntime)
		{
			try
			{
				await jsRuntime.InvokeAsync<string>("jsInteropFunctions.convertListToTree");
			}
			catch
			{
			}

		}
	}
}
