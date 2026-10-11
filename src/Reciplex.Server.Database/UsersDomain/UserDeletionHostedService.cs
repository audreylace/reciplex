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
    IUserDeletionSignal deletionSignal,
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
                    await deletionSignal.WaitForSignal(TimeSpan.FromMinutes(1), stoppingToken);
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
        DatabaseResultVariant<EmptySuccessResult, UserNotFoundResult> accessEntryDeletionResult =
            await recipeBooksService.DeleteRecipesBookAccessEntriesOwnedByUserAsync(userDao.Id, ct);

        // queue recipe book's owned by the user for deletion
        DatabaseResultVariant<EmptySuccessResult, UserNotFoundResult> booksDeletionQueueResult =
            await recipeBooksService.QueueRecipesBooksOwnedByUserForDeletionAsync(userDao.Id, ct);

        if (
            accessEntryDeletionResult.Result is EmptySuccessResult
            && booksDeletionQueueResult.Result is EmptySuccessResult
        )
        {
            // check if the user own's any books
            DatabaseResultVariant<SuccessResult<bool>, UserNotFoundResult> bookQueryResult =
                await recipeBooksService.HasAnyBooksAsync(userDao.Id, ct);

            if (
                bookQueryResult.Result switch
                {
                    SuccessResult<bool> hasBooks => !hasBooks.Value, // Purge if user owns NO remaining books
                    UserNotFoundResult => true, // user does not exist in book system so they should not own any books
                    _ => false,
                }
            )
            {
                DatabaseResultVariant<
                    EmptySuccessResult,
                    ConflictResult,
                    ValidationFailureResult
                > userResult = await usersService.PurgeUserFromDatabaseAsync(
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
