using System.Net.Mail;

namespace MintPlayer.Data.Extensions;

internal static class SmtpClientExtensions
{
	internal static void Connect(this SmtpClient smtpClient, string host, int port, bool useTLS)
	{
		smtpClient.Host = host;
		smtpClient.Port = port;
		smtpClient.EnableSsl = useTLS;
		smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
	}
}
