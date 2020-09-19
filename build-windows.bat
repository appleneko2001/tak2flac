rd /S /Q build
dotnet publish -v m -c Release -r win7-x64  --self-contained false ./tak2flac/tak2flac.Core.csproj
dotnet publish -v m -c Release -r win7-x86 --self-contained false ./tak2flac/tak2flac.Core.csproj
dotnet publish -v m -c Release -r win-x64 --self-contained false ./tak2flac/tak2flac.Core.csproj
dotnet publish -v m -c Release -r win-x86 --self-contained false ./tak2flac/tak2flac.Core.csproj
move ./tak2flac/bin/Release/netcoreapp3.1 ./build
rem FK I HATE CMD WHY NO mv command here