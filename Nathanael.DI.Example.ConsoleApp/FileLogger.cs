namespace Nathanael.DI.Example.ConsoleApp;

internal class FileLogger : IMessageLogger, IDisposable
{
    private const string FilePath = "logfile.log";
    private bool _disposed;
    private readonly StreamWriter _fileStreamWriter;

    public FileLogger()
    {
        var fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        _fileStreamWriter = new StreamWriter(fs);
    }
    
    public void Log(string message)
    {
        _fileStreamWriter.WriteLine($"[{DateTime.Now}] - {message}");
        _fileStreamWriter.Flush();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _fileStreamWriter.Close();
                _fileStreamWriter.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}