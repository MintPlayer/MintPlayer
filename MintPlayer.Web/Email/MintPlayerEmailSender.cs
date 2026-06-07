using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MintPlayer.Web.Email;

/// <summary>
/// Identity's transactional email sender (account confirmation + password reset), implemented
/// with MailKit over the <c>Smtp</c> config section. Registered as <see cref="IEmailSender{TUser}"/>
/// so it overrides the framework's no-op default — the ASP.NET Identity API endpoints
/// (<c>/spark/auth/register</c>, <c>/forgotPassword</c>, …) call into this.
///
/// When SMTP isn't configured (no host — e.g. local dev) the message is logged instead of sent,
/// so the confirm/reset links are visible in the console without a mail server.
/// </summary>
public sealed class MintPlayerEmailSender : IEmailSender<MintPlayerUser>
{
    private readonly SmtpOptions options;
    private readonly ILogger<MintPlayerEmailSender> logger;

    public MintPlayerEmailSender(IOptions<SmtpOptions> options, ILogger<MintPlayerEmailSender> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    public Task SendConfirmationLinkAsync(MintPlayerUser user, string email, string confirmationLink) =>
        SendAsync(email, "Confirm your MintPlayer account",
            Layout("Confirm your account",
                "Welcome to MintPlayer! Confirm your email address to activate your account.",
                "Confirm email", confirmationLink));

    public Task SendPasswordResetLinkAsync(MintPlayerUser user, string email, string resetLink) =>
        SendAsync(email, "Reset your MintPlayer password",
            Layout("Reset your password",
                "We received a request to reset your password. Click the button below to choose a new one. If you didn't request this, you can ignore this email.",
                "Reset password", resetLink));

    public Task SendPasswordResetCodeAsync(MintPlayerUser user, string email, string resetCode) =>
        SendAsync(email, "Reset your MintPlayer password",
            Layout("Reset your password",
                $"Use this code to reset your password: <strong>{resetCode}</strong>",
                null, null));

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (!options.IsConfigured)
        {
            // Dev mode: no SMTP host configured — log the message so links are usable locally.
            logger.LogInformation("[DEV email — SMTP not configured] To: {To} | Subject: {Subject}\n{Body}",
                to, subject, htmlBody);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(options.FromName, options.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var security = options.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        await client.ConnectAsync(options.Host, options.Port, security);
        if (!string.IsNullOrEmpty(options.User))
        {
            await client.AuthenticateAsync(options.User, options.Password);
        }
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
    }

    /// <summary>Minimal branded HTML wrapper with an optional call-to-action button.</summary>
    private static string Layout(string heading, string intro, string? buttonText, string? buttonUrl)
    {
        var button = buttonText is not null && buttonUrl is not null
            ? $"""<p style="margin:24px 0;"><a href="{buttonUrl}" style="background:#212529;color:#fff;padding:10px 18px;border-radius:6px;text-decoration:none;display:inline-block;">{buttonText}</a></p><p style="font-size:12px;color:#6c757d;">If the button doesn't work, copy this link:<br>{buttonUrl}</p>"""
            : string.Empty;

        return $"""
            <div style="font-family:system-ui,Segoe UI,Roboto,sans-serif;max-width:480px;margin:0 auto;color:#212529;">
              <h2 style="color:#212529;">{heading}</h2>
              <p>{intro}</p>
              {button}
              <hr style="border:none;border-top:1px solid #dee2e6;margin:24px 0;">
              <p style="font-size:12px;color:#6c757d;">MintPlayer</p>
            </div>
            """;
    }
}
