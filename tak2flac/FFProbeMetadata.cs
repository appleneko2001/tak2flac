using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tak2flac
{
    public class FFProbeMetadataStreams
    {
        public int index = 0;
        public string codec_name;
        public string codec_long_name;
        public string codec_type;
        public ulong duration_ts;
        public double duration;
        public int start_pts;
        public double start_time;
    }
    public class FFProbeMetadataFormat
    {
        public string filename;
        public string format_name;
        public double start_time;
        public double duration;
        public ulong size;
        public long bit_rate;
        public FFProbeMetadataTags tags;
    }
    public class FFProbeMetadataTags
    {
        public string Album;
        public string Artist;
        public string comment;
        public string Year;
        public string Discid;
        public string Genre;
        public string Cuesheet;
    }
    public class FFProbeMetadata
    {
        public FFProbeMetadataStreams[] streams;
        public FFProbeMetadataFormat format;
    }
}
