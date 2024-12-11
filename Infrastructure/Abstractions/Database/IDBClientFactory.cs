namespace Infrastructure.Abstractions.Database;

public interface IDBClientFactory<T>
{
    /// <summary>
    /// Produces a database client.
    /// </summary>
    /// <returns>Database client.</returns>
    T GetClient();
}
