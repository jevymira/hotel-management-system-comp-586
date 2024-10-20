namespace Abstractions;

public interface IDBClientFactory<T>
{
    T GetClient();
}
