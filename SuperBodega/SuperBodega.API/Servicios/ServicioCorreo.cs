using MailKit.Net.Smtp;
using MimeKit;

namespace SuperBodega.API.Servicios;

public class ServicioCorreo
{
	public async Task EnviarCorreo(
		string destino,
		string asunto,
		string mensaje)
	{
		var email = new MimeMessage();

		email.From.Add(
			MailboxAddress.Parse("superbodegaproyecto@gmail.com")
		);

		email.To.Add(
			MailboxAddress.Parse(destino)
		);

		email.Subject = asunto;

		email.Body = new TextPart("plain")
		{
			Text = mensaje
		};

		using var smtp = new SmtpClient();

		await smtp.ConnectAsync(
			"smtp.gmail.com",
			587,
			MailKit.Security.SecureSocketOptions.StartTls
		);

		await smtp.AuthenticateAsync(
			"superbodegaproyecto@gmail.com",
			"lkcwrffahktyiohs"
		);

		await smtp.SendAsync(email);

		await smtp.DisconnectAsync(true);
	}
}