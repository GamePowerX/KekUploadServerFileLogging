# KekUploadServerFileLogging
This is a simple file logging plugin for [KekUploadServer](https://github.com/GamePowerX/KekUploadServer).

## Installation
1. Build the project
2. Copy the `KekUploadServerFileLogging.dll` file from the `bin/Debug/net7.0` folder to the `plugins` folder of your KekUploadServer installation
3. Start the server
4. Check the `logs` folder for the log file
5. Configure the plugin in the `plugins/FileLogging/config.json` file
6. Restart the server

## Configuration
The `config.json` file contains the following properties:
- `LogPath`: The path to the log files (default: `logs`)
- `LogLevel`: The log level (0 = Trace, 1 = Debug, 2 = Information, 3 = Warning, 4 = Error, 5 = Critical) (default: 2)
- `IncludeDateTime`: Whether to include the date and time in the log messages (default: true)
- `IncludeLevel`: Whether to include the log level in the log messages (default: true)
- `IncludeEventId`: Whether to include the event id in the log messages (default: false)
- `IncludeState`: Whether to include the state in the log messages (default: false)
- `IncludeException`: Whether to include the exception in the log messages (default: false)
- `IncludeScope`: Whether to include the scope in the log messages (default: false)
- `FormatScopeAsJson`: Whether to format the scope as json or ToString() (default: false)
- `IncludeCategory`: Whether to include the category in the log messages (default: false)
- `DateTimeFormat`: The format of the date and time in the log messages (default: `yyyy-MM-dd HH:mm:ss`)
- `FileFormat`: The format of the log messages (default: `{DateTime} [{Level}] {Category}: {Message}{NewLine}{Exception}`)
- `FileExtension`: The file extension of the log files (default: `log`)
- `FilePrefix`: The prefix of the log files (default: `KekUploadServerLog`)
- `FileSuffix`: The suffix of the log files (default: ``)
- `UseUtc`: Whether to use UTC or local time (default: false)

## Contribute
You can contribute to this project by creating a pull request or by creating an issue.

## License
This project is licensed under the MIT license. See the [LICENSE](https://github.com/GamePowerX/KekUploadServerFileLogging/blob/master/LICENSE) file for more information.