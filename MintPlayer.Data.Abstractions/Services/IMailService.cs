using System.Net.Mail;
using System.Threading.Tasks;

namespace MintPlayer.Data.Abstractions.Services
{
	public interface IMailService
	{
		Task<SmtpClient> CreateSmtpClient();
	}
}
