using System.Threading.Channels;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Implements <see cref="IUserDeletionSignal"/>
/// </summary>
sealed class UserDeletionSignal() : IUserDeletionSignal
{
    /// <summary>
    /// Bounded channel used like a conditional variable
    /// </summary>
    private readonly Channel<long> _user = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    /// <inheritdoc />
    public void SignalDeletion()
    {
        _user.Writer.TryWrite(0);
    }

    /// <inheritdoc />
    public Task WaitForSignal(TimeSpan timeout, CancellationToken ct)
    {
        return _user.WaitOnChannel(timeout, ct);
    }
}
