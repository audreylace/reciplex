using System.Threading.Channels;

namespace Reciplex.Server.Database.UsersDomain;

class UsersQueuedForDeletionNotificationService() : IUsersQueuedForDeletionNotificationService
{
    private readonly Channel<long> _user = Channel.CreateBounded<long>(
        new BoundedChannelOptions(1)
        {
            SingleReader = true,
            FullMode = BoundedChannelFullMode.DropWrite,
        }
    );

    public void TriggerWake()
    {
        _user.Writer.TryWrite(0);
    }

    public Task WaitForWake(TimeSpan timeout, CancellationToken ct)
    {
        return _user.WaitOnChannel(timeout, ct);
    }
}
