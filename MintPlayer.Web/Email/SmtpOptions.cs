namespace MintPlayer.Web.Email;

/// <summary>
/// SMTP settings bound from the <c>Smtp</c> configuration section. When <see cref="Host"/> is
/// empty the app runs in "dev" mode: <see cref="MintPlayerEmailSender"/> logs messages instead of
/// sending them, so register/confirm/reset flows work locally without a mail server.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;

    /// <summary>Use STARTTLS (typical for port 587). When false, connection security is auto-negotiated.</summary>
    public bool UseStartTls { get; set; } = true;

    public string? User { get; set; }
    public string? Password { get; set; }

    public string From { get; set; } = "no-reply@mintplayer.com";
    public string FromName { get; set; } = "MintPlayer";

    /// <summary>True once a host is set; gates real sending vs. dev logging.</summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
