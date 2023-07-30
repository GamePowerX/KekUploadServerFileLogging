using System.Text.Json;
using KekUploadServerApi;
using Microsoft.Extensions.Logging;

namespace KekUploadServerFileLogging;

public class FileLogging : IPlugin
{
    private IKekUploadServer _server = null!;
    private ILogger<FileLogging> _logger = null!;
    
    public Task Load(IKekUploadServer server)
    {
        _server = server;
        var logger = _server.GetPluginLogger<FileLogging>();
        var dataPath = _server.GetPluginDataPath(this);
        var configPath = Path.Combine(dataPath, "config.json");
        if (!File.Exists(configPath))
        {
            logger.LogInformation("Config file not found, creating...");
            var newConfig = new Config();
            var json = JsonSerializer.Serialize(newConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
            logger.LogInformation("Config file created!");
        }
        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
        if(config == null)
        {
            logger.LogError("Config file is invalid!");
            return Task.CompletedTask;
        }
        Directory.CreateDirectory(config.LogPath);
        _server.RegisterLoggerProvider(this, new FileLoggerProvider(config));
        logger.LogInformation("FileLogging loaded!");
        _logger = logger;
        return Task.CompletedTask;
    }

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Unload()
    {
        return Task.CompletedTask;
    }

    public PluginInfo Info => new()
    {
        Name = "FileLogging",
        Author = "GamePowerX",
        Version = "1.0.0-alpha1",
        Description = "A plugin that logs to a file"
    };
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly Config _config;
    public FileLoggerProvider(Config config)
    {
        this._config = config;
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(categoryName, _config);
    }
}

public class Config
{
    public string Readme { get; } = "This is the config for the FileLogging plugin for KekUploadServer, for help visit https://github.com/GamePowerX/KekUploadServerFileLogging";
    public string LogPath { get; set; } = "logs";
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool IncludeDateTime { get; set; } = true;
    public bool IncludeLevel { get; set; } = true;
    public bool IncludeEventId { get; set; } = false;
    public bool IncludeState { get; set; } = false;
    public bool IncludeException { get; set; } = false;
    public bool IncludeScope { get; set; } = false;
    public bool FormatScopeAsJson { get; set; } = false;
    public bool IncludeCategory { get; set; } = false;
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
    public string FileFormat { get; set; } = "{DateTime} [{Level}] {Category}: {Message}{NewLine}{Exception}";
    public string FileExtension { get; set; } = "log";
    public string FilePrefix { get; set; } = "KekUploadServerLog";
    public string FileSuffix { get; set; } = "";
    public bool UseUtc { get; set; } = false;
}

public class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Config _config;
    public FileLogger(string categoryName, Config config)
    {
        _categoryName = categoryName;
        _config = config;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // check if logLevel is enabled from config
        if (!IsEnabled(logLevel))
        {
            return;
        }
        // check config for what to include in log
        var includeDateTime = _config.IncludeDateTime;
        var includeLevel = _config.IncludeLevel;
        var includeEventId = _config.IncludeEventId;
        var includeState = _config.IncludeState;
        var includeException = _config.IncludeException;
        var includeScope = _config.IncludeScope;
        var formatScopeAsJson = _config.FormatScopeAsJson;
        var includeCategory = _config.IncludeCategory;
        var dateTimeFormat = _config.DateTimeFormat;
        var fileFormat = _config.FileFormat;
        var fileExtension = _config.FileExtension;
        var filePrefix = _config.FilePrefix;
        var fileSuffix = _config.FileSuffix;
        var useUtc = _config.UseUtc;
        // get current time
        var dateTime = useUtc ? DateTime.UtcNow : DateTime.Now;
        // format the message
        var message = formatter(state, exception);
        // format the file name
        var fileName = $"{filePrefix}{dateTime:yyyyMMdd}{fileSuffix}.{fileExtension}";
        // format the log
        var log = fileFormat
            .Replace("{DateTime}", includeDateTime ? dateTime.ToString(dateTimeFormat) : "")
            .Replace("{Level}", includeLevel ? logLevel.ToString() : "")
            .Replace("{EventId}", includeEventId ? eventId.ToString() : "")
            .Replace("{State}", includeState ? state?.ToString() : "")
            .Replace("{Exception}", includeException ? exception?.ToString() : "")
            .Replace("{Scope}", "")
            .Replace("{Category}", includeCategory ? _categoryName : "")
            .Replace("{Message}", message)
            .Replace("{NewLine}", Environment.NewLine);
        
        var logPath = Path.Combine(_config.LogPath, fileName);
        // write the log to file
        File.AppendAllText(logPath, log);
        
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        // check if logLevel is enabled from config
        return logLevel >= _config.LogLevel;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        var includeScope = _config.IncludeScope;
        var formatScopeAsJson = _config.FormatScopeAsJson;
        var scope = new Scope<TState>(state);
        if (!includeScope) return null;
        scope.DisposedEvent += (sender, args) =>
        {
            if(formatScopeAsJson)
            {
                var json = JsonSerializer.Serialize(args.State, new JsonSerializerOptions { WriteIndented = true });
                Log(LogLevel.Information, new EventId(), args.State, null, (s, e) => "Scope Ended (Disposed): " + json);
            }
            else
            {
                Log(LogLevel.Information, new EventId(), args.State, null, (s, e) => "Scope Ended (Disposed): " + args.State);
            }
        };
        if (formatScopeAsJson)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            Log(LogLevel.Information, new EventId(), state, null, (s, e) => "New Scope: " + json);
            return scope;
        }
        else
        {
            Log(LogLevel.Information, new EventId(), state, null, (s, e) => "New Scope: " + state);
            return scope;
        }
    }
}

public class Scope<T> : IDisposable where T : notnull
{
    private readonly T _state;
    public event EventHandler<ScopeDisposedEventArgs<T>>? DisposedEvent;
    public Scope(T state)
    {
        _state = state;
    }

    public void Dispose()
    {
        DisposedEvent?.Invoke(this, new ScopeDisposedEventArgs<T>(_state));
        GC.SuppressFinalize(this);
    }
}

public class ScopeDisposedEventArgs<T> : EventArgs where T : notnull
{
    public ScopeDisposedEventArgs(T state)
    {
        State = state;
    }
    
    public T State { get; }
}