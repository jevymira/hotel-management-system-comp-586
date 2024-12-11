namespace Infrastructure.Abstractions.Database;
public interface IDBConnectionConfigFactory<T>
{
    /// <summary>
    /// Produces a configuration for a database client.
    /// </summary>
    /// <returns>Database client configuration.</returns>
    T GetConfig();
}
