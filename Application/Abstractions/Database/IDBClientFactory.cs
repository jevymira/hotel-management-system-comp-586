namespace Application.Abstractions.Database;

public interface IDBClientFactory<T>
{

    T GetClient();
}
