using Demo;
using FluentAssertions;
using Renci.SshNet;

namespace tests;

[Collection(nameof(CollectionIntegrationTests))]
public sealed class DemoClientTests : IDisposable
{
    private readonly IntegrationTestsFactory _factory;
    private readonly DemoSFTPService _service;
    private readonly SftpClient _sftpClient;


    public DemoClientTests(IntegrationTestsFactory factory)
    {
        _factory = factory;
        _sftpClient = _factory.CreateSftpClient();
        _service = new(_sftpClient);
    }

    public void Dispose() => _sftpClient.Dispose();



    [Fact]
    public async Task File_sent_by_ftp_should_be_the_same_in_file_system()
    {
        // Arrange
        var fileContent = $"Hello World! -> {Guid.NewGuid()}";
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync(fileContent);
        await writer.FlushAsync();
        stream.Position = 0;

        var remotePath = _factory.GetSftpRemotePath("fake.txt");

        using var remoteFile = new MemoryStream();


        // Act
        _service.SendFile(stream, remotePath);

        _sftpClient.Connect();
        _sftpClient.DownloadFile(remotePath, remoteFile);
        _sftpClient.Disconnect();
        remoteFile.Position = 0;
        using var reader = new StreamReader(remoteFile);
        var act = await reader.ReadToEndAsync();


        // Assert
        act.Should().Be(fileContent);
    }

    [Fact]
    public async Task Throw_invalid_operation_exception_when_client_cant_transfer_file()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        await writer.WriteAsync("Fake invalid file");
        await writer.FlushAsync();

        var remotePath = _factory.GetSftpRemotePath("fake_invalid.txt");


        // Act
        var act = () => _service.SendFile(stream, remotePath);


        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
