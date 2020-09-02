using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace tak2flac
{
    public class Program
    {
        private static string FFMPEG_BINARY_PATH => GetEnv("FFMPEG_BINARY");
        private static string FFPROBE_BINARY_PATH => GetEnv("FFPROBE_BINARY");
        /// <summary>
        /// Main entry of program.
        /// Usage: -i "input_file" [-o "output_path"] [-cue "cue_descriptor_file"]
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            bool requestReturn = false; 
            if (FFMPEG_BINARY_PATH is null || FFPROBE_BINARY_PATH is null)
            {
                requestReturn = true;
            } 
            if (requestReturn)
            {
                Console.WriteLine("Press any key to abort.");
                Console.ReadKey();
                return;
            } 
            string input = null, output = null, additionalCue = null;
            for(int i = 0; i < args.Length; i++)
            {
                var item = args[i];
                if (item.StartsWith("-i", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    input = args[i];
                }
                else if (item.StartsWith("-o", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    output = args[i];
                }
                else if (item.StartsWith("-cue", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    additionalCue = args[i];
                }
                else if (File.Exists(item))
                {
                    input = item;
                }
            }
            if (input is null)
                InvalidArguments();
            else
            {
                if (output is null)
                    output = "." + Path.DirectorySeparatorChar; 
                var metadata = GetMediaMetadata(input);
                string cueData = null;
                if(metadata.streams.Length == 0)
                {
                    Console.WriteLine("No found streams.");
                    return;
                }    
                if(metadata.format == null)
                {
                    Console.WriteLine("Unknown format.");
                    return;
                }
                if (metadata.format.tags == null)
                {
                    if (additionalCue == null || !File.Exists(additionalCue))
                    {
                        Console.WriteLine("Cannot get tags. Please provide a cue file for spliting with parameter -cue.");
                        return;
                    }
                    cueData = File.ReadAllText(additionalCue);
                }
                if (cueData == null)
                    cueData = metadata.format.tags.Cuesheet;
                var Duration = TimeSpan.FromMilliseconds(metadata.format.duration);
                var param = CreateFFMpegParameters(output, input, cueData, Duration);
                CreateFFMpegProcess(param);
            }
            return;

        } 

        public static FFProbeMetadata GetMediaMetadata(string file)
        {
            Console.WriteLine(file);
            file = file.PatchFilePath();
            var probeData = CreateFFMpegProcessWithOutput($"-print_format json -show_format -show_streams {file}", true, FFMpegComponentEnum.FFPROBE);
            return JsonConvert.DeserializeObject<FFProbeMetadata>(probeData);
        }

        public static string CreateFFMpegParameters(string outputPath, string file, string cueData, TimeSpan fullDuration)
        {
            var cueSheet = new Cuesheet(cueData);
            var Duration = fullDuration;
            List<string> parameters = new List<string>();
            var pointerCounts = cueSheet.AudioProbes.Length;
            List<Timeline> Timelines = new List<Timeline>();
            for (int i = 0; i < pointerCounts; i++)
            {
                TimeSpan start = cueSheet.AudioProbes[i].TimePointer;
                Timeline timeline;
                if (i < pointerCounts - 1)
                    timeline = new Timeline(start, cueSheet.AudioProbes[i + 1].TimePointer);
                else
                    timeline = new Timeline(start, Duration);
                Timelines.Add(timeline);
            }

            int indexTl = 0;
            foreach (var cueProbe in cueSheet.AudioProbes)
            {
                Timeline timeline = Timelines[indexTl];
                string metadataParams = "";
                Dictionary<string, string> tagDict = new Dictionary<string, string>();
                foreach (var tags in cueSheet.Root.GetAllTags)
                {
                    var key = tags.Key;
                    if (key == "title")
                        key = "album";
                    else if (key == "performer")
                        key = "artist";
                    tagDict.AddOrModify(key, tags.Value);
                }
                foreach (var tags in cueProbe.GetAllTags)
                {
                    tagDict.AddOrModify(tags.Key, tags.Value);
                }
                foreach (var t in tagDict)
                {
                    metadataParams += $"-metadata \"{t.Key}={t.Value}\" ";
                }

                parameters.Add($"-ss {timeline.Start.ToString(@"hh\:mm\:ss\.ff")} -t {timeline.Duration.ToString(@"hh\:mm\:ss\.ff")} {metadataParams} \"{outputPath + cueProbe.Title}.flac\" ");

                indexTl++;
            }
            var finalParam = $"-y -i {file} ";
            foreach (var p in parameters)
            {
                finalParam += p;
            }
            return finalParam;
        }

        static void CreateFFMpegProcess(string args, bool quietVerbose = true, FFMpegComponentEnum component = FFMpegComponentEnum.FFMPEG)
        {
            var programPath = component == FFMpegComponentEnum.FFMPEG ? FFMPEG_BINARY_PATH : FFPROBE_BINARY_PATH;
            ProcessStartInfo arg = new ProcessStartInfo(programPath, (quietVerbose ? "-v quiet " : "") + args);
            arg.StandardOutputEncoding = Encoding.UTF8;
            arg.UseShellExecute = false;
            arg.CreateNoWindow = true;
            arg.RedirectStandardInput = true;
            arg.RedirectStandardError = true;
            arg.RedirectStandardOutput = true;
            Process p = new Process();
            p.StartInfo = arg;
            p.ErrorDataReceived += (proc, errorLine) => Console.Error.WriteLine(errorLine.Data);
            p.OutputDataReceived += (proc, textLine) => Console.Out.WriteLine(textLine.Data);
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
        }
        static string CreateFFMpegProcessWithOutput(string args, bool quietVerbose = true, FFMpegComponentEnum component = FFMpegComponentEnum.FFMPEG)
        {
            using (StringWriter writer = new StringWriter())
            {
                var programPath = component == FFMpegComponentEnum.FFMPEG ? FFMPEG_BINARY_PATH : FFPROBE_BINARY_PATH;
                ProcessStartInfo arg = new ProcessStartInfo(programPath, (quietVerbose ? "-v quiet " : "") + args);
                arg.StandardOutputEncoding = Encoding.UTF8;
                arg.UseShellExecute = false;
                arg.CreateNoWindow = true;
                arg.RedirectStandardInput = true;
                arg.RedirectStandardError = true;
                arg.RedirectStandardOutput = true;
                Process p = new Process();
                
                p.StartInfo = arg;
                p.ErrorDataReceived += (proc, errorLine) => 
                    Console.Error.WriteLine(errorLine.Data);
                p.OutputDataReceived += (proc, textLine) => 
                { 
                    writer.WriteLine(textLine.Data); 
                    Console.Out.WriteLine(textLine.Data); 
                };
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                return writer.ToString();
            }
        }
        static void InvalidArguments()
        {


        }
        static string GetEnv(string envName)
        {
            var env = Environment.GetEnvironmentVariable(envName);
            if (env is null)
            {
                Console.WriteLine($"Please define environment variable {envName} to dependency binary file path."); 
            }  
            else if (!File.Exists(env))
            {
                Console.WriteLine($"Executable file are not found on path: {env}");
                Console.WriteLine($"Please redefine again environment variable {envName}");
            }
            return env;
        }
    }
}