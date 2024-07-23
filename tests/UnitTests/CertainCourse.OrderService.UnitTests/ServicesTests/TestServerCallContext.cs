using Grpc.Core;

namespace CertainCourse.OrderService.Tests.ServicesTests;

public class TestServerCallContext : ServerCallContext
{
    private readonly CancellationToken _cancellationToken;

    private TestServerCallContext(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override string MethodCore => throw new NotImplementedException();
    protected override string HostCore => throw new NotImplementedException();
    protected override string PeerCore => throw new NotImplementedException();
    protected override DateTime DeadlineCore => throw new NotImplementedException();
    protected override Metadata RequestHeadersCore => throw new NotImplementedException();
    protected override Metadata ResponseTrailersCore => throw new NotImplementedException();
    protected override Status StatusCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    protected override WriteOptions WriteOptionsCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    protected override AuthContext AuthContextCore { get; }

    public static ServerCallContext Create(CancellationToken cancellationToken = default)
    {
        return new TestServerCallContext(cancellationToken);
    }

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        throw new NotImplementedException();
    }

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
    {
        throw new NotImplementedException();
    }
}