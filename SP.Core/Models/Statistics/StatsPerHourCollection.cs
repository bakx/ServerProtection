using System.Collections.Generic;

namespace SP.Models.Statistics
{
	public class StatsPerHourCollection
	{
		public string Key { get; set; }
		public List<StatsPerHour> Data { get; set; }
	}
}