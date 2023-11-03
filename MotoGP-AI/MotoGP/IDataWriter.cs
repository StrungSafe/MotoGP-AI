namespace MotoGP;

public interface IDataWriter
{
    Task Write<T>(string filePath, T data);
}