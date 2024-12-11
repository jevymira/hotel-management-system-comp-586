namespace Application.Abstractions.Database;
public interface IDBConnectionConfigFactory<T>
{
    T GetConfig();
}
