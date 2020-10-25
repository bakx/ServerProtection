using System;
using System.Text;
using Microsoft.Diagnostics.Tracing;

namespace Plugins
{
	public class IISLogTraceData : TraceEvent
	{
		public int EnabledFieldsFlags => GetInt32At(GetOffsetForField(0));

		/// <summary>
		/// The date on which the activity occurred.
		/// </summary>
		public string Date => GetUnicodeStringAt(GetOffsetForField(1));

		/// <summary>
		/// The time, in coordinated universal time (UTC), at which the activity occurred.
		/// </summary>
		public string Time => GetUnicodeStringAt(GetOffsetForField(2));

		/// <summary>
		/// The IP address of the client that made the request.
		/// </summary>
		public string ClientIp => GetUnicodeStringAt(GetOffsetForField(3));

		/// <summary>
		/// The name of the authenticated user that accessed the server. Anonymous users are indicated by a hyphen.
		/// </summary>
		public string UserName => GetUnicodeStringAt(GetOffsetForField(4));

		/// <summary>
		/// The Internet service name and instance number that was running on the client.
		/// </summary>
		public string ServiceName => GetUnicodeStringAt(GetOffsetForField(5));

		/// <summary>
		/// The name of the server on which the log file entry was generated.
		/// </summary>
		public string ServerName => GetUnicodeStringAt(GetOffsetForField(6));

		/// <summary>
		/// The IP address of the server on which the log file entry was generated.
		/// </summary>
		public string ServerIp => GetUnicodeStringAt(GetOffsetForField(7));

		/// <summary>
		/// The requested verb, for example, a GET method.
		/// </summary>
		public string Method => GetUTF8StringAt(GetOffsetForField(8));

		/// <summary>
		/// The target of the verb, for example, Default.htm.
		/// </summary>
		public string UriStem => GetUnicodeStringAt(GetOffsetForField(9));

		/// <summary>
		/// The query, if any, that the client was trying to perform. A Universal Resource Identifier (URI) query is necessary only for dynamic pages.
		/// </summary>
		public string UriQuery => GetUTF8StringAt(GetOffsetForField(10));

		/// <summary>
		/// The HTTP status code.
		/// </summary>
		public int ProtocolStatus => GetInt16At(GetOffsetForField(11));

		/// <summary>
		/// The Windows status code.
		/// </summary>
		public int Win32Status => GetInt32At(GetOffsetForField(12));

		/// <summary>
		/// The number of bytes sent by the server.
		/// </summary>
		public long ScBytes => GetInt64At(GetOffsetForField(13));

		/// <summary>
		/// The number of bytes received and processed by the server.
		/// </summary>
		public long CsBytes => GetInt64At(GetOffsetForField(14));

		/// <summary>
		/// The length of time that the action took, in milliseconds.
		/// </summary>
		public long TimeTaken => GetInt64At(GetOffsetForField(15));

		/// <summary>
		/// The server port number that is configured for the service.
		/// </summary>
		public int ServerPort => GetInt16At(GetOffsetForField(16));

		/// <summary>
		/// The browser type that the client used.
		/// </summary>
		public string UserAgent => GetUTF8StringAt(GetOffsetForField(17));

		/// <summary>
		/// The content of the cookie sent or received, if any.
		/// </summary>
		public string Cookie => GetUTF8StringAt(GetOffsetForField(18));

		/// <summary>
		/// The site that the user last visited. This site provided a link to the current site.
		/// </summary>
		public string Referrer => GetUTF8StringAt(GetOffsetForField(19));

		/// <summary>
		/// The HTTP protocol version that the client used.
		/// </summary>
		public string CsVersion => GetUnicodeStringAt(GetOffsetForField(20));

		public string CsHost => GetUTF8StringAt(GetOffsetForField(21));

		public int ScSubstatus => GetInt16At(GetOffsetForField(22));

		public string CustomFields => GetUnicodeStringAt(GetOffsetForField(23));

		private static readonly string[] PayloadNamesCache;
		public override string[] PayloadNames => PayloadNamesCache;

		private Action<IISLogTraceData> target;

		protected sealed override Delegate Target
		{
			get => target;
			set => target = (Action<IISLogTraceData>) value;
		}

		static IISLogTraceData()
		{
			const string names = "EnabledFieldsFlags,Date,Time,C_ip,Cs_username,"
								 + "S_sitename,S_computername,S_ip,Cs_method,Cs_uri_stem,"
			                     + "Cs_uri_query,Sc_status,Sc_win32_status,Sc_bytes,"
			                     + "Cs_bytes,Time_taken,S_port,CsUser_agent,Cookie,Referrer,"
			                     + "Cs_version,Cs_host,Sc_substatus,CustomFields";
			PayloadNamesCache = names.Split(',');
		}

		public IISLogTraceData(Action<IISLogTraceData> action, int eventID, int task, string taskName, Guid taskGuid,
			int opcode, string opcodeName, Guid providerGuid, string providerName)
			: base(eventID, task, taskName, taskGuid, opcode, opcodeName, providerGuid, providerName)
		{
			Target = action;
		}

		protected override void Dispatch()
		{
			Action<IISLogTraceData> action = target;
			action?.Invoke(this);
		}

		public override object PayloadValue(int index)
		{
			return index switch
			{
				0 => EnabledFieldsFlags,
				1 => Date,
				2 => Time,
				3 => ClientIp,
				4 => UserName,
				5 => ServiceName,
				6 => ServerName,
				7 => ServerIp,
				8 => Method,
				9 => UriStem,
				10 => UriQuery,
				11 => ProtocolStatus,
				12 => Win32Status,
				13 => ScBytes,
				14 => CsBytes,
				15 => TimeTaken,
				16 => ServerPort,
				17 => UserAgent,
				18 => Cookie,
				19 => Referrer,
				20 => CsVersion,
				21 => CsHost,
				22 => ScSubstatus,
				23 => CustomFields,
				_ => null
			};
		}

		public override StringBuilder ToXml(StringBuilder sb)
		{
			Prefix(sb);
			XmlAttrib(sb, "EnabledFieldsFlags", EnabledFieldsFlags.ToString("x"));
			XmlAttrib(sb, "Date", Date);
			XmlAttrib(sb, "Time", Time);
			XmlAttrib(sb, "C_ip", ClientIp);
			XmlAttrib(sb, "Cs_username", UserName);
			XmlAttrib(sb, "S_sitename", ServiceName);
			XmlAttrib(sb, "S_computername", ServerName);
			XmlAttrib(sb, "S_ip", ServerIp);
			XmlAttrib(sb, "Cs_method", Method);
			XmlAttrib(sb, "Cs_uri_stem", UriStem);
			XmlAttrib(sb, "Cs_uri_query", UriQuery);
			XmlAttrib(sb, "Sc_status", ProtocolStatus);
			XmlAttrib(sb, "Sc_win32_status", Win32Status);
			XmlAttrib(sb, "Sc_bytes", ScBytes);
			XmlAttrib(sb, "Cs_bytes", CsBytes);
			XmlAttrib(sb, "Time_taken", TimeTaken);
			XmlAttrib(sb, "S_port", ServerPort);
			XmlAttrib(sb, "CsUser_agent", UserAgent);
			XmlAttrib(sb, "Cookie", Cookie);
			XmlAttrib(sb, "Referrer", Referrer);
			XmlAttrib(sb, "Cs_version", CsVersion);
			XmlAttrib(sb, "Cs_host", CsHost);
			XmlAttrib(sb, "Sc_substatus", ScSubstatus);
			XmlAttrib(sb, "CustomFields", CustomFields);
			sb.Append("/>");
			return sb;
		}


		private enum Types
		{
			UInt16,
			UInt32,
			UInt64,
			UnicodeString,
			AnsiString
		}

		private static readonly Types[] FieldTypes =
		{
			Types.UInt32,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.UnicodeString,
			Types.AnsiString,
			Types.UnicodeString,
			Types.AnsiString,
			Types.UInt16,
			Types.UInt32,
			Types.UInt64,
			Types.UInt64,
			Types.UInt64,
			Types.UInt16,
			Types.AnsiString,
			Types.AnsiString,
			Types.AnsiString,
			Types.UnicodeString,
			Types.AnsiString,
			Types.UInt16,
			Types.UnicodeString
		};

		private int GetOffsetForField(int index)
		{
			int offset = 0;
			for (int i = 1; i <= index; i++)
			{
				offset = AddSizeOf(FieldTypes[i - 1], offset);
			}

			return offset;
		}

		private int AddSizeOf(Types type, int offset)
		{
			return type switch
			{
				Types.UInt16 => offset + sizeof(short),
				Types.UInt32 => offset + sizeof(int),
				Types.UInt64 => offset + sizeof(long),
				Types.UnicodeString => SkipUnicodeString(offset),
				Types.AnsiString => SkipUTF8String(offset),
				_ => throw new NotSupportedException("Data type not supported")
			};
		}
	}
}