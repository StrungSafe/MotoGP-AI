namespace MotoGP.Extensions;

public interface IHostExceptionHandler
{
    Task RunAsync(Task task);
}