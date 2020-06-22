using Microsoft.JSInterop;
using System.Threading.Tasks;

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
