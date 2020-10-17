using System.Collections.Generic;
using System.Diagnostics;

namespace Plugins.Models
{
	public class EventLogEntry
	{
		/// <summary>
		/// </summary>
		/// <param name="source"></param>
		public EventLogEntry(IReadOnlyList<string> source)
		{
			Debug.Assert(source.Count > 20);

			SubjectSecurityID = source[0];
			SubjectAccountName = source[1];
			SubjectAccountDomain = source[2];
			SubjectLogonID = source[3];
			AccountSecurityID = source[4];
			AccountAccountName = source[5];
			AccountAccountDomain = source[6];
			Status = source[7];
			FailureReason = source[8];
			SubStatus = source[9];
			LogonType = source[10];
			LogonProcess = source[11];
			AuthenticationPackage = source[12];
			SourceWorkstationName = source[13];
			TransitedServices = source[14];
			PackageName = source[15];
			KeyLength = source[16];
			CallerProcessID = source[17];
			CallerProcessName = source[18];
			SourceNetworkAddress = source[19];
			SourcePort = source[20];
		}

		public string SubjectSecurityID { get; set; }
		public string SubjectAccountName { get; set; }
		public string SubjectAccountDomain { get; set; }
		public string SubjectLogonID { get; set; }
		public string AccountSecurityID { get; set; }
		public string AccountAccountName { get; set; }
		public string AccountAccountDomain { get; set; }
		public string Status { get; set; }
		public string FailureReason { get; set; }
		public string SubStatus { get; set; }
		public string LogonType { get; set; }
		public string LogonProcess { get; set; }
		public string AuthenticationPackage { get; set; }
		public string SourceWorkstationName { get; set; }
		public string TransitedServices { get; set; }
		public string PackageName { get; set; }
		public string KeyLength { get; set; }
		public string CallerProcessID { get; set; }
		public string CallerProcessName { get; set; }
		public string SourceNetworkAddress { get; set; }
		public string SourcePort { get; set; }
	}
}