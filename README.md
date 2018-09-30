# Simple Video On Demand
Service does real time video conversion to web video formats mp4/ogg/webm using ffmpeg. Watch videos in Android browser or download them to your device.

[Byte range](https://blogs.msdn.microsoft.com/webdev/2012/11/23/asp-net-web-api-and-http-byte-range-support/) support is currently not implemented, which makes impossible to watch videos on iPhone\iPad.

## Configuration
All configuration is done in  appsettings.json

1. Download FFmpeg binaries from [https://ffmpeg.org](https://ffmpeg.org/download.html)
2. Specify full path to ffmpeg.exe file in configuration.
3. Specify video file root folder, subfolders will be included.
4. Execute ``dotnet run``

Website will be started on ``44333`` port by default.
