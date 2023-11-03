namespace MotoGP;

public interface IDataReader
{
    Task<T> Read<T>(string filePath);
}