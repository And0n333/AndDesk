using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace AndDesk.Infrastructure;

public sealed class SingleInstance : IDisposable
{
    private const string MutexName = @"Global\AndDesk_SingleInstance_Mutex";
    private const string PipeName = "AndDesk_Wakeup_Pipe";

    private Mutex? _mutex;
    private bool _hasHandle;
    private CancellationTokenSource? _cts;

    public event Action? OnWakeupRequested;

    public bool Start()
    {
        _mutex = new Mutex(true, MutexName, out _hasHandle);
        if (_hasHandle)
        {
            StartPipeServer();
            return true;
        }

        // Another instance is already running, try to notify it
        NotifyExistingInstance();
        return false;
    }

    private void StartPipeServer()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous
                    );

                    await server.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(server);
                    string? msg = await reader.ReadLineAsync(token);

                    if (msg == "WAKEUP")
                    {
                        OnWakeupRequested?.Invoke();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Pipe server error: {ex.Message}");
                    await Task.Delay(1000, token);
                }
            }
        }, token);
    }

    private static void NotifyExistingInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1500); // Wait up to 1.5 seconds
            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine("WAKEUP");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to notify existing instance: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        if (_mutex != null)
        {
            if (_hasHandle)
            {
                _mutex.ReleaseMutex();
            }
            _mutex.Dispose();
        }
    }
}
