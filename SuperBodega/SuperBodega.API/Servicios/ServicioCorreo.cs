using MailKit.Net.Smtp;
using MimeKit;
using SuperBodega.Domain.Entidades;

namespace SuperBodega.API.Servicios;

public class ServicioCorreo
{
    public async Task EnviarCorreo(
        string destino,
        Venta venta)
    {
        var email = new MimeMessage();

        email.From.Add(
            MailboxAddress.Parse(
                "superbodegaproyecto@gmail.com")
        );

        email.To.Add(
            MailboxAddress.Parse(destino)
        );

        email.Subject =
            "Confirmación de compra - SuperBodega";

        string productosHtml = "";

        foreach (var detalle in venta.Detalles!)
        {
            productosHtml += $@"
            <tr>
                <td>{detalle.Producto?.Nombre}</td>
                <td>{detalle.Cantidad}</td>
                <td>₡ {detalle.PrecioUnitario}</td>
            </tr>";
        }

        email.Body = new TextPart("html")
        {
            Text = $@"
            <h1>
                Gracias por comprar en SuperBodega
            </h1>

            <p>
                Su compra fue registrada exitosamente.
            </p>

            <hr/>

            <h2>
                Detalle de la compra
            </h2>

            <table border='1'
                   cellpadding='8'
                   cellspacing='0'
                   style='border-collapse: collapse;'>

                <tr style='background-color:#f2f2f2'>
                    <th>Producto</th>
                    <th>Cantidad</th>
                    <th>Precio</th>
                </tr>

                {productosHtml}

            </table>

            <br/>

            <h2>
                Total: Q {venta.Total}
            </h2>

            <p>
                <strong>Estado:</strong>
                {venta.Estado}
            </p>

            <p>
                <strong>Fecha:</strong>
                {venta.Fecha}
            </p>

            <hr/>

            <h3>
                SuperBodega
            </h3>

            <p>
                Gracias por su preferencia.
            </p>
            "
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