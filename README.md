[看不懂英文嗎 來這裡!](https://github.com/appleneko2001/tak2flac/blob/master/README.zh-Hant.md)

# tak2flac
Simple tak splitter with FFMpeg and FFProbe

### Requirements
* Latest FFMpeg and FFProbe binaries
* Environment variables that contains path of FFMpeg and FFProbe binaries
* (For Windows).Net framework 4 (Windows 7 or newer are installed with OS)
* (For Linux) ~~Mono runtime that supports .Net framework 4~~ USE DOTNET CORE 3.1 INSTEAD!!! [Official instructions of install .Net Core](https://docs.microsoft.com/dotnet/core/install/linux-package-managers)
* (For Linux) Be sure your linux OS architecture are x64, if you want use dotNet Core. Otherwize use mono recommended.

### How to use
```sh
tak2flac -i "input.tak" [-o "output_path"] [-cue "additional_cue_path"]
```

### Install
#### Install it with hard way
* Windows way:
  * Download [FFMpeg](https://ffmpeg.org/download.html) if you don't have it.
  * Extract FFMpeg archive file to somewhere, for example, to C:\Program files\FFMpeg
  * Define two environment variable named FFMPEG_BINARY and FFPROBE_BINARY, they should be pointed to path of ffmpeg binary and ffprobe binary. 
  > It can be changed by right-click "My Computer" (or "This PC") -> "Properties" -> "System" on right side of window -> "Advanced" tab -> "Environment Variables..." -> New system variables -> Create variable named FFMPEG_BINARY and FFPROBE_BINARY, value should write path of those binary.
  * Download tak2flac release zip file and unpack it to folder that PATH contained (or add FFMpeg binary folder and unpack to there)
  * run command prompt and type tak2flac.
  * It should be no any messages. Otherwize you should reconfigure it.


* Linux way (sudo will be required for elevated privileges): 
  * Use package manager (apt/yum/snap or etc..) install FFMpeg. ~~For install mono you can visit this page~~ We have dotNet Core Binaries so just install dotNet Core instead!
  ```sh
  sudo apt update
  sudo apt install ffmpeg
  ``` 
  * Write those things on last line of your ~/.profile (or similar like that)
  ```sh
    FFMPEG_BINARY="Path of ffmpeg binary"
    FFPROBE_BINARY="Path of ffprobe binary"
  ```
  > !Tips! After set up environment variables they could not applied on current session. Reboot computer and changes will applied.
  * Download tak2flac core release zip file and unpack it to folder that PATH contained. For example, /usr/bin (or ~/.local/bin directory instead if .profile contains auto-detect and set PATH variables, no sudo requirement but only this user can use tak2flac)
  * Give file mode 0755 to tak2flac (chmod 0755 tak2flac).
  * Run terminal, type tak2flac and it should returned empty message.
  
#### Install it with easy way 
Still not ready for windows. But we have install script for linux OS.
```sh
curl -sS https://raw.githubusercontent.com/appleneko2001/tak2flac/master/install-tak2flac.sh > install-tak2flac.sh
chmod +x ./install-tak2flac.sh
./install-tak2flac.sh
```

### How it works?
For works correctly should have .tak file with cue or both file, no cuesheet will not work.

First, this program will use FFProbe to get input media information to gather cuesheet, duration and tags.
After that program will parses cuesheet data and generate process parameters for FFMpeg spliting, and writing tags.
When our parameters has ready, and finally FFMpeg will started working with those parameters and we will have splited media files from .tak

### Want to improve this program
Any suggestions are accepted and report bugs! Create issue and just tell me your suggestion or problem when using this program.
