using Microsoft.Extensions.Options;
using MintPlayer.Data.Abstractions.Services;
using MintPlayer.Data.Extensions;
using MintPlayer.Data.Options;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MintPlayer.Data.Services
{
    internal class MailService : IMailService
    {
        private readonly IOptions<SmtpOptions> smtpOptions;
        public MailService(IOptions<SmtpOptions> smtpOptions)
        {
            this.smtpOptions = smtpOptions;
        }

        public Task<SmtpClient> CreateSmtpClient()
        {
            var client = new SmtpClient();
            client.Connect(smtpOptions.Value.Host, smtpOptions.Value.Port, smtpOptions.Value.UseTLS);
            client.Credentials = new System.Net.NetworkCredential(smtpOptions.Value.User, smtpOptions.Value.Password);

            return Task.FromResult(client);
        }
    }
}
