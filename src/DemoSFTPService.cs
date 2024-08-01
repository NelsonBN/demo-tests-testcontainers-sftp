using Renci.SshNet;
using System;
using System.IO;

namespace Demo;

public sealed class DemoSFTPService(SftpClient client)
{
    private readonly SftpClient _client = client;

    public void SendFile(Stream file, string filePath)
    {
        _client.Connect();

        ulong sentBits = 0;
        _client.UploadFile(
            file,
            filePath,
            (bits) => sentBits = bits);

        _client.Disconnect();

        if (sentBits != (ulong)file.Length)
        {
            throw new InvalidOperationException($"The file '{filePath}' was not sent correctly");
        }
    }
}
