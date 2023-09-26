using System;
namespace PEAS.Services.Email
{
    public interface IEmailService
    {

    }

    public class EmailService : IEmailService
	{
		public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
		{
		}
	}
}