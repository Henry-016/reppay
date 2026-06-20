using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace RepPay.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarEmailRecuperacao(string emailDestino, string codigo)
        {
            var emailRemetente = _config["EmailSettings:Remetente"];
            var senhaApp = _config["EmailSettings:SenhaApp"];

            if (string.IsNullOrEmpty(emailRemetente) || string.IsNullOrEmpty(senhaApp))
            {
                throw new Exception("As credenciais de e-mail não foram configuradas no servidor.");
            }

            var clienteSmtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(emailRemetente, senhaApp),
                EnableSsl = true,
            };

            var mensagem = new MailMessage
            {
                From = new MailAddress(emailRemetente, "Equipe RepPay"),
                Subject = "Código de Recuperação de Senha",
                Body = $@"
                    <div style='font-family: Arial; padding: 20px; color: #333;'>
                        <h2>Olá!</h2>
                        <p>Recebemos um pedido de recuperação de senha para a sua conta.</p>
                        <p>Seu código de segurança é: <h1 style='color: #00bcd4;'>{codigo}</h1></p>
                        <p>Se não foi você, apenas ignore este e-mail.</p>
                    </div>",
                IsBodyHtml = true
            };

            mensagem.To.Add(emailDestino);

            clienteSmtp.Send(mensagem);
        }
    }
}