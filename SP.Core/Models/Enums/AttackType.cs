namespace SP.Models.Enums
{
	public enum AttackType
	{
		/// <summary>
		///  Comment/forum spam, HTTP referer spam, or other CMS spam. 
		/// </summary>
		WebSpam = 1,
		/// <summary>
		///  Scanning for open ports and vulnerable services. 
		/// </summary>
		PortScan = 2,
		/// <summary>
		/// Attempts at SQL injection
		/// </summary>
		SqlInjection = 3,
		/// <summary>
		///  Credential brute-force attacks on webpage logins and services like SSH, FTP, SIP, SMTP, RDP, etc.
		/// </summary>
		BruteForce = 4,
		/// <summary>
		/// Attempts to probe for or exploit installed web applications.
		/// </summary>
		WebExploit = 5
	}
}