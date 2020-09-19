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
        private static bool RequireShortDuration = false;
        private static bool Verbose = false;
        private static bool DisableBatchMode = false;
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
                // Input file parameter
                if (item.StartsWith("-i", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    input = args[i];
                }
                // Output directory parameter
                else if (item.StartsWith("-o", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    output = args[i];
                }
                // Input cuesheet file parameter
                else if (item.StartsWith("-cue", StringComparison.InvariantCultureIgnoreCase))
                {
                    i++;
                    additionalCue = args[i];
                }
                // Require shorter duration parameter
                else if (item.StartsWith("-s", StringComparison.InvariantCultureIgnoreCase))
                    RequireShortDuration = true;
                // Verbose output parameter
                else if (item.StartsWith("-v", StringComparison.InvariantCultureIgnoreCase))
                    Verbose = true;
                // Disable batch mode parameter
                else if (item.StartsWith("-b", StringComparison.InvariantCultureIgnoreCase))
                    DisableBatchMode = true;
                // Input file parameter (shorter way)
                else if (File.Exists(item))
                    input = item;
            }
            if (Verbose)
            {
                Console.WriteLine("Starting tak2flac with verbose mode.");
                if(RequireShortDuration)
                    Console.WriteLine("Timeline short mode enabled. Will use more shorter and minimal duration timeline from cuesheet.");
                if (DisableBatchMode)
                    Console.WriteLine("Batch ffmpeg mode disabled. ");
            }
            if (input is null)
                InvalidArguments();
            else
            {
                if (output is null)
                    output = "./"; 
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
                    if (Verbose)
                        Console.WriteLine("Media not integrated cuesheet data. External cue file required.");
                    if (additionalCue == null || !File.Exists(additionalCue))
                    {
                        Console.WriteLine("Cannot get tags. Please provide a cue file for timeline definitions by parameter -cue.");
                        return;
                    }
                    cueData = File.ReadAllText(additionalCue);
                    if (Verbose)
                        Console.WriteLine("External cuesheet data: " + cueData);
                }
                if (cueData == null)
                    cueData = metadata.format.tags.Cuesheet;
                var Duration = TimeSpan.FromSeconds(metadata.format.duration); // Im sorry I used wrong method to get full duration, use FromSeconds instead now.
                if (Verbose)
                    Console.WriteLine("Media full duration: " + Duration);
                if (Verbose)
                    Console.WriteLine($"Spawning parameters for ffmpeg...");
                if (!DisableBatchMode)
                {
                    var param = CreateFFMpegParameters(output, input, cueData, Duration);
                    if (Verbose)
                    {
                        Console.WriteLine($"Parameter: {param}");
                        Console.WriteLine($"Starting ffmpeg...");
                        CreateFFMpegProcessWithOutput(param, false);
                    }
                    else
                    {
                        CreateFFMpegProcess(param);
                    }
                }
                else
                {
                    var parameters = CreateFFMpegParametersLines(output, cueData, Duration);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        // Always confirms replace files, and remove all stranges video streams (cuz tak and flac are audio media file, video streams on here is nonsense)
                        // but Idk it will affects cover art or not, in my case are fine and doesnt affects cover art. Any issue plz tell me.
                        // I try my best to fix them :V
                        var param = $"-y -i {input} -map 0 -map -0:v " + parameters[i];
                        if (Verbose)
                        {
                            Console.WriteLine($"Starting ffmpeg ({i + 1} / {parameters.Length})...");
                            CreateFFMpegProcessWithOutput(param, false);
                        }
                        else
                        {
                            CreateFFMpegProcess(param);
                        }
                    }
                }

            }
            return;

        } 

        /// <summary>
        /// Get media metadata, information by FFPROBE component.
        /// </summary>
        /// <param name="file">Definited media file.</param>
        /// <returns>Returns metadata class that filled some metadatas.</returns>
        public static FFProbeMetadata GetMediaMetadata(string file)
        { 
            file = file.PatchFilePath(Environment.CurrentDirectory); 
            var probeData = CreateFFMpegProcessWithOutput($"-print_format json -show_format -show_streams {file}", true, FFMpegComponentEnum.FFPROBE);
            if (Verbose)
                Console.WriteLine("Result of FFPROBE output: " + probeData);
            return JsonConvert.DeserializeObject<FFProbeMetadata>(probeData);
        }

        /// <summary>
        /// Generate FFMPEG parameters line
        /// </summary>
        /// <param name="outputPath">Output directory</param>
        /// <param name="cueData">Cuesheet data</param>
        /// <param name="fullDuration">Media full duration</param>
        /// <returns></returns>
        public static string[] CreateFFMpegParametersLines(string outputPath, string cueData, TimeSpan fullDuration)
        {
            var cueSheet = new Cuesheet(cueData);
            var Duration = fullDuration;
            List<string> parameters = new List<string>();
            var pointerCounts = cueSheet.AudioProbes.Length;
            List<Timeline> Timelines = new List<Timeline>();
            if (Verbose)
                Console.WriteLine("Preparing timelines...");
            for (int i = 0; i < pointerCounts; i++)
            {
                if (Verbose)
                    Console.WriteLine($"Creating timeline for track {cueSheet.AudioProbes[i].Title} (index: {i + 1})");
                TimeSpan start = cueSheet.AudioProbes[i].GetRecommentedStartIndex();
                TimeSpan end = (!RequireShortDuration) ?
                    ((i < pointerCounts - 1) ? cueSheet.AudioProbes[i + 1].GetLastEndPointer() : Duration) : // legacy mode
                    ((i < pointerCounts - 1) ? cueSheet.AudioProbes[i + 1].GetEarlyIndex() : Duration); // with parameter -s
                Timeline timeline = new Timeline(start, end);
                Timelines.Add(timeline);
                if (Verbose)
                    Console.WriteLine($"Timeline created: start: {start}, end: {end}, duration: {timeline.Duration}");
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

                var result = $"-ss {timeline.Start.ToString(@"hh\:mm\:ss\.ff")} -t {timeline.Duration.ToString(@"hh\:mm\:ss\.ff")} {metadataParams} \"{outputPath + cueProbe.Title}.flac\" ";
                if (Verbose)
                    Console.WriteLine($"Parameter {indexTl + 1} has ready: {result}");
                parameters.Add(result);
                indexTl++;
            }
            return parameters.ToArray();
        }
        /// <summary>
        /// Generate FFMPEG parameters with batch mode.
        /// </summary>
        /// <param name="outputPath">Output directory</param>
        /// <param name="file">Input file</param>
        /// <param name="cueData">Cuesheet data</param>
        /// <param name="fullDuration">Media full duration</param>
        /// <returns>Returns full parameters, use it with ProcessStartInfo.</returns>
        public static string CreateFFMpegParameters(string outputPath, string file, string cueData, TimeSpan fullDuration)
        {
            var parameters = CreateFFMpegParametersLines(outputPath, cueData, fullDuration);
            var finalParam = $"-y -i {file} -map 0 -map -0:v ";
            foreach (var p in parameters)
            {
                finalParam += p;
            }

            return finalParam;
        }

        /// <summary>
        /// Create FFMPEG process, passes some start arguments
        /// </summary>
        /// <param name="args">Parameters that generated from CreateFFMpegParameters() or CreateFFMpegParametersLines() (require mixes input file argument at least)</param>
        /// <param name="quietVerbose">Set it true if you want to disable output.</param>
        /// <param name="component">Default case are FFMPEG. But you can use FFPROBE component.</param>
        static void CreateFFMpegProcess(string args, bool quietVerbose = true, FFMpegComponentEnum component = FFMpegComponentEnum.FFMPEG)
        {
            var programPath = component == FFMpegComponentEnum.FFMPEG ? FFMPEG_BINARY_PATH : FFPROBE_BINARY_PATH;
            ProcessStartInfo arg = new ProcessStartInfo(programPath, (quietVerbose ? "-v panic " : "") + args);
            arg.StandardOutputEncoding = Encoding.UTF8;
            arg.UseShellExecute = false;
            arg.CreateNoWindow = true; 
            arg.RedirectStandardError = true;
            arg.RedirectStandardOutput = true;
            Process p = new Process();
            p.StartInfo = arg;
            p.ErrorDataReceived += (proc, errorLine) => Console.Error.WriteLine(errorLine.Data);
            p.OutputDataReceived += (proc, textLine) => { if (!quietVerbose) Console.Out.WriteLine(textLine.Data); };
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
        }
        /// <summary>
        /// Create FFMPEG process with return outputs.
        /// </summary>
        /// <param name="args">Parameters that generated from CreateFFMpegParameters() or CreateFFMpegParametersLines() (require mixes input file argument at least)</param>
        /// <param name="quietVerbose">Set it true if you want to disable output.</param>
        /// <param name="component">Default case are FFMPEG. But you can use FFPROBE component.</param>
        /// <returns></returns>
        static string CreateFFMpegProcessWithOutput(string args, bool quietVerbose = true, FFMpegComponentEnum component = FFMpegComponentEnum.FFMPEG)
        {
            using (StringWriter writer = new StringWriter())
            {
                var programPath = component == FFMpegComponentEnum.FFMPEG ? FFMPEG_BINARY_PATH : FFPROBE_BINARY_PATH;
                ProcessStartInfo arg = new ProcessStartInfo(programPath, (quietVerbose ? "-v panic " : "") + args);
                arg.StandardOutputEncoding = Encoding.UTF8;
                arg.StandardErrorEncoding = Encoding.UTF8;
                arg.UseShellExecute = false;
                arg.CreateNoWindow = true; 
                arg.RedirectStandardError = true;
                arg.RedirectStandardOutput = true;
                Process p = new Process();
                p.EnableRaisingEvents = true;
                p.StartInfo = arg;
                p.ErrorDataReceived += (proc, errorLine) => 
                    Console.Out.WriteLine(errorLine.Data);
                p.OutputDataReceived += (proc, textLine) => 
                { 
                    writer.WriteLine(textLine.Data); 
                    if(!quietVerbose)
                        Console.Out.WriteLine(textLine.Data); 
                };
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine(); 
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