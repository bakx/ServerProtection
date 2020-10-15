using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SP.Overview.Pages
{
	public class StatisticsModel : PageModel
	{
		private readonly ILogger<StatisticsModel> log;

		public StatisticsModel(ILogger<StatisticsModel> log)
		{
			this.log = log;
		}

		public void OnGet()
		{
		}
	}
}