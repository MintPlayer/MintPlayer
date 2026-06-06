using System;

namespace MintPlayer.Data.Options
{
	public class JwtIssuerOptions
	{
		/// <summary>
		/// 4.1.1.  "iss" (Issuer) Claim - The "iss" (issuer) claim identifies the principal that issued the JWT.
		/// </summary>
		public string Issuer { get; set; }

		/// <summary>
		/// 4.1.2.  "sub" (Subject) Claim - The "sub" (subject) claim identifies the principal that is the subject of the JWT.
		/// </summary>
		public string Subject { get; set; }

		/// <summary>
		/// 4.1.3.  "aud" (Audience) Claim - The "aud" (audience) claim identifies the recipients that the JWT is intended for.
		/// </summary>
		public string Audience { get; set; }

		/// <summary>
		/// Set the timespan the token will be valid for (default is 120 min)
		/// </summary>
		public TimeSpan ValidFor { get; set; }

		/// <summary>
		/// Key for singing the JWT
		/// </summary>
		public string Key { get; set; }
	}
}
