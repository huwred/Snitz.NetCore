using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MVCForum.Extensions
{
public interface IShutdownService
{
    Task TriggerShutdownAsync();
}

public class ShutdownService : IShutdownService
{
    public async Task TriggerShutdownAsync()
    {
            Thread.Sleep(1000); // Let the response flush
            var exePath = Process.GetCurrentProcess().MainModule.FileName;
            Process.Start(exePath); // Relaunch
            Console.WriteLine("Application restarted.");
            Environment.Exit(0);    // Terminate current process
    }
}
}
