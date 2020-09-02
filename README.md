# tak2flac
Simple tak spliter with FFMpeg and FFProbe

### Requirements
* Latest FFMpeg and FFProbe binaries
* Environment variables that contains path of FFMpeg and FFProbe binaries
* (For Windows).Net framework 4 (Windows 7 or newer are installed with OS)
* (For Linux) Mono runtime that supports .Net framework 4

### How to use
```sh
tak2flac -i "input.tak" [-o "output_path"] [-cue "additional_cue_path"]
```

### Install
#### Install it with hard way
* Download [FFMpeg](https://ffmpeg.org/download.html) if you don't have it.
* Extract FFMpeg archive file to somewhere, for example, to C:\Program files\FFMpeg
* Define two environment variable named FFMPEG_BINARY and FFPROBE_BINARY, they should be pointed to path of ffmpeg binary and ffprobe binary.
  * Windows way:
  It can be changed by right-click "My Computer" (or "This PC") -> "Properties" -> "System" on right side of window -> "Advanced" tab -> "Environment Variables..." -> New system variables -> Create variable named FFMPEG_BINARY and FFPROBE_BINARY, value should write path of those binary.
  * Linux way: 
  Write those things on last line of your ~/.profile (or similar like that)
  ```sh
    FFMPEG_BINARY="Path of ffmpeg binary"
    FFPROBE_BINARY="Path of ffprobe binary"
  ```
* Download tak2flac release zip file and unpack it to folder that PATH contained (or add FFMpeg binary folder and unpack to there)
* Close all command prompt (terminal) instance for apply changes of environment variable.
* run command prompt and type tak2flac (or terminal if you use mono, type mono tak2flac).
* It should be no any messages. Otherwize you should reconfigure it.
#### Install it with easy way 
Still not ready.

### How it works?
For works correctly should have .tak file with cue or both file, no cuesheet will not work.

First, this program will use FFProbe to get input media information to gather cuesheet, duration and tags.
After that program will parses cuesheet data and generate process parameters for FFMpeg spliting, and writing tags.
When our parameters has ready, and finally FFMpeg will started working with those parameters and we will have splited media files from .tak

### Want to improve this program
Any suggestions are accepted and report bugs! Create issue and just tell me your suggestion or problem when using this program.
