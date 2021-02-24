[看不懂英文嗎 來這裡!](https://github.com/appleneko2001/tak2flac/blob/master/README.zh-Hant.md)

# tak2flac
Simple TAK (Tom's Lossless Audio Kompressor) audio splitter with FFMpeg and FFProbe. For more about TAK format: [Hydrogenaudio Wiki](https://wiki.hydrogenaud.io/index.php?title=TAK)

### Requirements
* Latest FFMpeg and FFProbe binaries
* Environment variables that contains path of FFMpeg and FFProbe binaries
* (For Windows).Net framework 4 (Windows 7 or newer are installed with OS so you don't need to install them)
* (For Linux) ~~Mono runtime that supports .Net framework 4~~ USE DOTNET CORE 3.1 INSTEAD!!! [Official instructions of install .Net Core](https://docs.microsoft.com/dotnet/core/install/linux-package-managers)
* (For Linux) Be sure your linux OS architecture are x64, if you want use .NET Core binaries. Otherwize you should use mono insteaded.

### How to use
```sh
tak2flac [-i] "<input_file>" [-o "output_path"] [-cue "additional_cue_path"] [-s] [-v] [-b]
    "<input_file>" Input a media file that used for split. It should be .tak format, or other format that 
                   ffmpeg supported and integrated cuesheet data. "-i" are optional.
    -cue "file.cue" Input a external cuesheet data file. When your media file are not integrated it.
    -s  Use newer method to create timelines (but timelines will get shorter for most cases)
    -v  Verbose output
    -b  Disable mixed ffmpeg parameter mode (Disable batch mode) //Idk how to explain it right way but in general cases will launch FFMPEG with mixed parameter
                    // after defines this argument will run FFMPEG in multiple times (according to counts of split).
Example:
    tak2flac "media.tak"  // Simple way
    tak2flac -i "media.tak" -s  // Input tak file and use newer method to create timeline
    tak2flac "media.flac" -cue "media.cue" -s -b  // Input flac file, cuesheet file, disable batch mode and make timelines shorter
    tak2flac -i "audio_media.flac" -cue "audio_media.cue" -o "./output" -s -b -v  // Input flac file, cuesheet file, define output
                //directory, enable almost full verbose, disable batch mode and make timelines shorter
```

### Install
#### Install it with hard way
* For Windows OS:
  * Download [FFMpeg](https://ffmpeg.org/download.html) if you don't have it.
  * Extract FFMpeg archive file to somewhere, for example, to C:\Program files\FFMpeg
  * Define two environment variable named FFMPEG_BINARY and FFPROBE_BINARY, they should be pointed to path of ffmpeg binary and ffprobe binary. 
  > It can be changed by right-click "My Computer" (or "This PC") -> "Properties" -> "System" on right side of window -> "Advanced" tab -> "Environment Variables..." -> New system variables -> Create variable named FFMPEG_BINARY and FFPROBE_BINARY, value should write path of those binary.
  * Download tak2flac release zip file and unpack it to folder that PATH contained (or add FFMpeg binary folder and unpack to there)
  * run command prompt and type tak2flac.
  * It should be no any messages. Otherwize you should reconfigure it.


* For Linux OS (elevated privileges will be required for all users): 
  * Use package manager (apt/yum/snap or etc..) install FFMpeg.
  ```sh
  sudo apt update
  sudo apt install ffmpeg
  ``` 
  * Install .NET Core 3 Runtime if your Linux OS doesn't have it. [Follow official instructions of install .Net Core](https://docs.microsoft.com/dotnet/core/install/linux-package-managers)
  * Write those things on last line of your ~/.profile (or similar like that)
  ```sh
    FFMPEG_BINARY="Path of ffmpeg binary"
    FFPROBE_BINARY="Path of ffprobe binary"
  ```
  > !Tips! After set up environment variables may not applied on current session. Reboot computer or logout and changes will applied.
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

This program will use FFProbe to get media information from cuesheet data first.
After that program will parses cuesheet data and generate parameters for start the progress of split file with FFMpeg, and writing tags.
When our parameters ready for use, the FFMpeg will be launched to split .tak file.

### Want to improve this program
Any suggestions are accepted and report bugs! Create issue and just tell me your suggestion or problem when using this program.
