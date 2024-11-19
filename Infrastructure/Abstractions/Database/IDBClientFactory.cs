namespace Infrastructure.Abstractions.Database;

public interface IDBClientFactory<T>
{
    T GetClient();
}
