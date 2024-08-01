using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Renci.SshNet;

namespace tests;

public sealed class IntegrationTestsFactory : IAsyncLifetime
{
    public const string SFTP_KEY_PASSWORD = "Ab123456";

    private const string SFTP_USER = "demouser";
    private const string SFTP_REMOTE_PATH = "data";
    private const int SFTP_CONTAINER_PORT = 22;

    private readonly IContainer _sftpContainer;

    public IntegrationTestsFactory()
        => _sftpContainer = new ContainerBuilder()
            .WithImage("atmoz/sftp:alpine")
            .WithEnvironment("SFTP_USERS", $"{SFTP_USER}:{SFTP_KEY_PASSWORD}:::{SFTP_REMOTE_PATH}")
            .WithPortBinding(SFTP_CONTAINER_PORT, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Server listening on :: port 22.", o => o.WithTimeout(TimeSpan.FromSeconds(10))))
            .Build();

    public SftpClient CreateSftpClient()
        => new(
            _sftpContainer.Hostname,
            _sftpContainer.GetMappedPublicPort(SFTP_CONTAINER_PORT),
            SFTP_USER,
            SFTP_KEY_PASSWORD);

    public string GetSftpRemotePath(string path)
        => $"{SFTP_REMOTE_PATH}/{path}";


    public Task InitializeAsync()
        => _sftpContainer.StartAsync();

    public Task DisposeAsync()
        => _sftpContainer.StopAsync();
}

[CollectionDefinition(nameof(CollectionIntegrationTests))]
public sealed class CollectionIntegrationTests : ICollectionFixture<IntegrationTestsFactory> { }
