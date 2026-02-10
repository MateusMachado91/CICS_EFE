using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFTPService
{
    Task<List<FtpFile>> ListDirectoryAsync(string host, string user, string pass, string dir);
}

public class FtpFile
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string RawPermissions { get; set; }
    public long Size { get; set; }
    public string Modified { get; set; }
    public string FullName { get; set; }
}