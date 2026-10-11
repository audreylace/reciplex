using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reciplex.Server.Database.RecipeBooksDomain;
using Reciplex.Server.Database.Results;
using Reciplex.Server.Database.SearchExporter;

namespace Reciplex.Server.Database.UsersDomain;

class UsersQueuedForDeletionHostedService(
    IUsersService usersService,
    IUsersQueuedForDeletionNotificationService notificationService,
    IRecipeBooksService recipeBooksService,
    ILogger<UsersQueuedForDeletionHostedService> logger
) : BackgroundService
{
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
                    await notificationService.WaitForWake(TimeSpan.FromMinutes(1), stoppingToken);
                }
                else if (nextEntry.Result is SuccessResult<UserDao> successResult)
                {
                    await PurgeOneUserAsync(successResult.Value, stoppingToken);
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

    private async Task PurgeOneUserAsync(UserDao userDao, CancellationToken ct)
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
                (
                    bookQueryResult.Result is SuccessResult<bool> doesUserOwnAnyBooks
                    && doesUserOwnAnyBooks.Value
                )
                || bookQueryResult.Result is UserNotFoundResult
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

                if (userResult.Result is not EmptySuccessResult)
                {
                    DatabaseResultVariant<
                        EmptySuccessResult,
                        ConflictResult,
                        ValidationFailureResult
                    > purgeResult = await usersService.PurgeUserFromDatabaseAsync(
                        userDao.Id,
                        userDao.ConcurrencyTag,
                        ct
                    );

                    if (purgeResult.Result is EmptySuccessResult)
                    {
                        return;
                    }
                }
            }
        }

        await usersService.RequeueUserForDeletionAsync(userDao.Id, userDao.ConcurrencyTag, ct);
    }
}
