namespace Infrastructure.Abstractions.Database;
public interface IDBConnectionConfigFactory<T>
{
    T GetConfig();
}
