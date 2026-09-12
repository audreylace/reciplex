namespace Reciplex.Server.Database.SearchExporter.Repositories;

enum IndexMutationOperationOutcome
{
    /// <summary>
    /// Other error occurred
    /// </summary>
    Error,

    /// <summary>
    /// The remote rejected the documents submitted
    /// </summary>
    /// <remarks>
    /// Caller should split up the documents and try
    /// them one by one to figure out which one is the problem.
    /// </remarks>
    BatchFailed,

    /// <summary>
    /// Documents inserted into the index
    /// </summary>
    Success,
}
