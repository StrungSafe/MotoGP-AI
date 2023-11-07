using Microsoft.Extensions.Logging;

namespace MotoGP.Extensions;

public class HostExceptionHandler : IHostExceptionHandler
{
    private readonly ILogger<HostExceptionHandler> logger;

    public HostExceptionHandler(ILogger<HostExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async Task RunAsync(Task task)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Unhandled exception thrown...shutting down");
        }
    }
}