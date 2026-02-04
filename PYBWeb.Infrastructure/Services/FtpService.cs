using FluentFTP;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class FtpService : IFTPService

{

    public async Task<List<FtpFile>> ListDirectoryAsync(string host, string user, string pass, string dir)
    {
        using var client = new FtpClient(host) { Credentials = new NetworkCredential(user, pass) };
        client.DataConnectionType = FtpDataConnectionType.PASV; // Passivo como FileZilla

            try
            {
                await client.ConnectAsync();
                var items = await client.GetListingAsync(dir, FtpListOption.AllFiles);

                foreach (var x in items)
                {
                    Console.WriteLine($"Nome: {x.Name} | Tipo: {x.Type} | Raw: {x.RawPermissions} | Size: {x.Size} | Modified: {x.Modified}");
                }

                await client.DisconnectAsync();

                return items.Select(x => new FtpFile
                {
                    Name = x.Name,
                    Type = x.Type.ToString(),
                    RawPermissions = x.RawPermissions,
                    Size = x.Size,
                    Modified = x.Modified.ToString("dd/MM/yyyy HH:mm"),
                    FullName = x.FullName
                }).ToList();
            }
            catch (Exception ex)
            {
                // LOG DETALHADO
                Console.WriteLine("Erro FTP: " + ex.ToString());
                throw;
            }
    }
}