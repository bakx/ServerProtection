using System;
using Microsoft.Diagnostics.Tracing;

namespace Plugins
{
	public class IISLogTraceEventParser : TraceEventParser
	{
		public static readonly Guid ProviderGuid = new Guid("7E8AD27F-B271-4EA2-A783-A47BDE29143B");
		public const string ProviderName = "Microsoft-Windows-IIS-Logging";
		public const int IisLogEventId = 6200;

		public IISLogTraceEventParser(TraceEventSource source)
		  : base(source)
		{
		}

		public event Action<IISLogTraceData> IISLog
		{
			add => source.RegisterEventTemplate(IisLogTemplate(value));
			remove => source.UnregisterEventTemplate(value, IisLogEventId, ProviderGuid);
		}

		protected override string GetProviderName()
		{
			return ProviderName;
		}

		private static IISLogTraceData IisLogTemplate(Action<IISLogTraceData> action)
		{
			return new IISLogTraceData(action, IisLogEventId, 0, "Logs", Guid.Empty, 0, "", ProviderGuid, ProviderName);
		}
		private static TraceEvent[] templates;
		protected override void EnumerateTemplates(
			Func<string, string, EventFilterResponse> eventsToObserve,
			Action<TraceEvent> callback)
		{

			templates ??= new TraceEvent[]
			{
				IisLogTemplate(null)
			};

			foreach (TraceEvent e in templates)
			{
				callback(e);
			}
		}
	}
}
