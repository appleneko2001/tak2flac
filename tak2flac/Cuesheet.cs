using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tak2flac
{
    public class CuesheetProbeInfo
    {
        public string Title => GetTags(nameof(Title));
        public string Performer => GetTags(nameof(Performer));
        public TimeSpan TimePointer;
        internal void AddTags(string key, string value)
        {
            Tags.Add(key.ToLower(), value);
        }
        public string GetTags(string key)
        {
            if (Tags.TryGetValue(key.ToLower(), out var result))
                return result;
            else
                return null;
        }
        private Dictionary<string, string> Tags = new Dictionary<string, string>();
        public Dictionary<string, string> GetAllTags => Tags;
    }
    public class CuesheetInfo
    {
        public string Genre => GetTags(nameof(Genre));
        public string Date => GetTags(nameof(Date));
        public string DiscId => GetTags(nameof(DiscId));
        public string Comment => GetTags(nameof(Comment));
        public string Performer => GetTags(nameof(Performer));
        public string Title => GetTags(nameof(Title));
        internal void AddTags(string key, string value)
        {
            Tags.Add(key.ToLower(), value);
        }
        public string GetTags(string key)
        {
            if (Tags.TryGetValue(key.ToLower(), out var result))
                return result;
            else
                return null;
        }
        private Dictionary<string, string> Tags = new Dictionary<string, string>();
        public Dictionary<string, string> GetAllTags => Tags;
    }
    public class Cuesheet
    {

        public Cuesheet(string cueSheetData)
        {
            var root = new CuesheetInfo();
            List<CuesheetProbeInfo> Probes = new List<CuesheetProbeInfo>();
            string[] csDataLines = cueSheetData.Replace("\r\n", "\n").Split('\n');
            bool trackProbes = false;
            int index = 0;

            CuesheetProbeInfo probe = null;

            foreach (var data in csDataLines)
            {
                var s = data.RemoveUnnecessarySpaces().SplitKeepQuotes();
                if (s[0].ToLower() == "rem")
                {
                    //if (s[1].ToLower() == "genre")
                    //    root.Genre = s[2];
                    //else if (s[1].ToLower() == "date")
                    //    root.Date = s[2];
                    //else if (s[1].ToLower() == "discid")
                    //    root.DiscId = s[2];
                    //else if (s[1].ToLower() == "comment")
                    //    root.Comment = s[2];
                    root.AddTags(s[1], s[2]);
                }
                else if (s[0].ToLower() == "performer")
                {
                    if (!trackProbes)
                        //root.Performer = s[1].RemoveQuotes();
                        root.AddTags("performer", s[1].RemoveQuotes());
                    else
                        //probe.Performer = s[1].RemoveQuotes();
                        probe.AddTags("performer", s[1].RemoveQuotes());
                }
                else if (s[0].ToLower() == "title")
                {
                    if (!trackProbes)
                        //root.Title = s[1].RemoveQuotes();
                        root.AddTags("title", s[1].RemoveQuotes());
                    else
                        //probe.Title = s[1].RemoveQuotes();
                        probe.AddTags("title", s[1].RemoveQuotes());
                }
                else if (s[0].ToLower() == "index")
                {
                    if (trackProbes)
                        probe.TimePointer = s[2].ParseTimeSpan();
                }
                else if (s[0].ToLower() == "track")
                {
                    trackProbes = true;
                    index = int.Parse(s[1]);
                    if (probe != null)
                        Probes.Add(probe);
                    probe = new CuesheetProbeInfo();
                }
            }

            if (probe != null)
                Probes.Add(probe);
            Root = root;
            AudioProbes = Probes.ToArray();
        }

        public CuesheetInfo Root;
        public CuesheetProbeInfo[] AudioProbes;
    }
}
