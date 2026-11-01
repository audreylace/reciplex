using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Abstractions;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;

namespace Reciplex.Server.Database.UsersDomain;

/// <summary>
/// Finishes user deletion in the background
/// </summary>
/// <param name="usersService">user repository</param>
/// <param name="deletionSignal">service for signaling that there is work to do</param>
/// <param name="recipeBooksService">book repository</param>
/// <param name="logger">logger for this instance</param>
sealed class UserDeletionHostedService(
    IUsersService usersService,
    IRecipeBooksService recipeBooksService,
    ILogger<UserDeletionHostedService> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Info_LoopStarting();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                DatabaseResultVariant<EmptySuccessResult, SuccessResult<UserDao>>? nextEntry =
                    await usersService.GetNextUserQueuedForDeletionAsync(stoppingToken);
                if (nextEntry.Result is EmptySuccessResult)
                {
                    await usersService.WaitForSignal(TimeSpan.FromMinutes(1), stoppingToken);
                }
                else if (nextEntry.Result is SuccessResult<UserDao> successResult)
                {
                    await TryPurgeOneUserAsync(successResult.Value, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Error_BackgroundLoopFailure(ex);
                await SafeDelay.DelayAsync(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        logger.Info_LoopStopping();
    }

    /// <summary>
    /// Attempts to purge one user from the database
    /// </summary>
    /// <param name="userDao">the user to purge</param>
    /// <param name="ct">cancels the operation</param>
    private async Task TryPurgeOneUserAsync(UserDao userDao, CancellationToken ct)
    {
        // clear user's access entries
        await recipeBooksService.DeleteAllRecipesBookAccessEntriesOwnedByUserAsync(userDao.Id, ct);

        // queue recipe book's owned by the user for deletion
        var count = await recipeBooksService.QueueAllRecipesBooksOwnedByUserForDeletionAsync(
            userDao.Id,
            ct
        );

        if (count < 1)
        {
            // check if the user own's any books
            bool hasBooks = await recipeBooksService.HasAnyBooksAsync(userDao.Id, ct);
            if (!hasBooks)
            {
                DatabaseResultVariant<EmptySuccessResult, ConflictResult> userResult =
                    await usersService.PurgeUserFromDatabaseAsync(
                        userDao.Id,
                        userDao.ConcurrencyTag,
                        ct
                    );

                if (userResult.Result is EmptySuccessResult)
                {
                    return;
                }
            }
        }

        // if we are here that means we need to try again at some future time so re-queue
        await usersService.RequeueUserForDeletionAsync(userDao.Id, userDao.ConcurrencyTag, ct);
    }
}
