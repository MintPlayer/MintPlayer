using System.Net.Mail;

namespace MintPlayer.Data.Abstractions.Services;

public interface IMailService
{
	Task<SmtpClient> CreateSmtpClient();
}
