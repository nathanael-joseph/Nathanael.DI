
internal class FileLogger : IMessageLogger, IDisposable
{
    private const string FilePath = "logfile.log";

    private bool _disposed;
    private readonly StreamWriter _fileStreamWriter;

    public FileLogger()
    {
        _fileStreamWriter = new StreamWriter(new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.Read));
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