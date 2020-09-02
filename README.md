# tak2flac
Simple tak spliter with FFMpeg and FFProbe

### Requirements
* Latest FFMpeg and FFProbe binaries
* Environment variables that contains path of FFMpeg and FFProbe binaries
* .Net framework 4 (Windows 7 or newer are installed with OS)

### How to use
```sh
tak2flac -i "input.tak" [-o "output_path"] [-cue "additional_cue_path"]
```

### Install
#### Install it with hard way
1. Download [FFMpeg](https://ffmpeg.org/download.html) if you don't have it.
2. Extract FFMpeg archive file to somewhere, for example, to C:\Program files\FFMpeg
3. Define two environment variable named FFMPEG_BINARY and FFPROBE_BINARY, they should be pointed to path of ffmpeg binary and ffprobe binary.
4. Close all cmd instance, run cmd and type tak2flac.
5. It should be no any messages. Otherwize you should reconfigure it.
#### Install it with easy way 
Still not ready, available lately.

### How it works?
For works correctly should have .tak file with cue or both file, no cuesheet will not work.

First, this program will use FFProbe to get input media information to gather cuesheet, duration and tags.
After that program will parses cuesheet data and generate process parameters for FFMpeg spliting, and writing tags.
When our parameters has ready, and finally FFMpeg will started working with those parameters and we will have splited media files from .tak
