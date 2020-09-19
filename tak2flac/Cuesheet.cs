using System;
using System.Collections.Generic;
using System.Linq;

namespace tak2flac
{
    public class CuesheetProbeInfo
    {
        public Dictionary<string, string> GetAllTags { get; } = new Dictionary<string, string>();
        public Dictionary<int, TimeSpan> Indexes;

        public string Title => GetTags(nameof(Title));

        public string Performer => GetTags(nameof(Performer));

        [Obsolete("This property will return last index pointer. Use Indexes property to get more detailed list of index pointers.")]
        public TimeSpan TimePointer => GetLastEndPointer(); 
        internal void AddIndex(string index, string ts)
        {
            if (Indexes == null)
                Indexes = new Dictionary<int, TimeSpan>();
            var i = int.Parse(index);
            if (Indexes.ContainsKey(i))
                Indexes[i] = ts.ParseTimeSpan();
            else
                Indexes.Add(i, ts.ParseTimeSpan());
        }
        internal void AddTags(string key, string value)
        {
            GetAllTags.Add(key.ToLower(), value);
        }
        public string GetTags(string key)
        {
            if (GetAllTags.TryGetValue(key.ToLower(), out var result))
                return result;
            else
                return null;
        }
        public TimeSpan GetLastEndPointer()
        {
            CheckAvabilityIndexes();
            long maxTicks = 0;
            foreach (var item in Indexes)
            {
                maxTicks = Math.Max(maxTicks, item.Value.Ticks);
            }
            return TimeSpan.FromTicks(maxTicks);
        }
        public TimeSpan GetRecommentedStartIndex()
        {
            CheckAvabilityIndexes();
            if (Indexes.ContainsKey(1))
                return Indexes[1];
            else if (Indexes.ContainsKey(0))
                return Indexes[0];
            else
                return GetLastEndPointer();
        }
        public TimeSpan GetEarlyIndex()
        {
            CheckAvabilityIndexes();
            long minTicks = Indexes.First().Value.Ticks;
            foreach (var item in Indexes)
            {
                minTicks = Math.Min(minTicks, item.Value.Ticks);
            }
            return TimeSpan.FromTicks(minTicks);
        }
        private void CheckAvabilityIndexes()
        {
            if (Indexes is null)
                throw new NullReferenceException("Indexes dictionary is not initialized.");
            if (Indexes.Count == 0)
                throw new ArgumentOutOfRangeException("Indexes dictionary is empty.");
        }

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
            GetAllTags.Add(key.ToLower(), value);
        }
        public string GetTags(string key)
        {
            if (GetAllTags.TryGetValue(key.ToLower(), out var result))
                return result;
            else
                return null;
        }

        public Dictionary<string, string> GetAllTags { get; } = new Dictionary<string, string>();
    }
    public class Cuesheet
    {
        public Cuesheet(string cueSheetData)
        {
            var root = new CuesheetInfo();
            List<CuesheetProbeInfo> Probes = new List<CuesheetProbeInfo>();
            string[] csDataLines = cueSheetData.Replace("\r\n", "\n").Split('\n');
            bool trackProbes = false;
            CuesheetProbeInfo probe = null;

            foreach (var data in csDataLines)
            {
                var s = data.RemoveUnnecessarySpaces().SplitKeepQuotes();
                if (s[0].ToLower() == "rem")
                { 
                    root.AddTags(s[1], s[2]);
                }
                else if (s[0].ToLower() == "performer")
                {
                    if (!trackProbes) 
                        root.AddTags("performer", s[1].RemoveQuotes());
                    else 
                        probe.AddTags("performer", s[1].RemoveQuotes());
                }
                else if (s[0].ToLower() == "title")
                {
                    if (!trackProbes) 
                        root.AddTags("title", s[1].RemoveQuotes());
                    else 
                        probe.AddTags("title", s[1].RemoveQuotes());
                }
                else if (s[0].ToLower() == "index")
                {
                    if (trackProbes)
                        probe.AddIndex(s[1], s[2]);
                    //probe.TimePointer = s[2].ParseTimeSpan();
                }
                else if (s[0].ToLower() == "track")
                {
                    trackProbes = true;
                    //int index = int.Parse(s[1]);
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
