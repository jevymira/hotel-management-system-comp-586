namespace Abstractions;
public interface IDBConnectionConfigFactory<T>
{
    T GetConfig();
}
