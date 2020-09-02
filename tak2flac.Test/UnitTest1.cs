using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace tak2flac.Test
{
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// Test ffprobe metadata parsing and cuesheets parsing.
        /// choosed one track file randomly.
        /// </summary>
        [TestMethod]
        public void TakParse01()
        {
            // Next media file will not provided by any reasons.
            string takFile = System.IO.Path.Combine(Environment.CurrentDirectory,"..", "..", "..", "data","マイペースでいきましょう.tak");
            var result = Program.GetMediaMetadata(takFile);
            if(result is null || result.format is null || result.streams is null)
            {
                throw new NullReferenceException("Result is null.");
            }
            else
            {
                Assert.AreEqual(result.streams[0].codec_name, "tak");
                Assert.AreEqual(result.format.filename.EndsWith("マイペースでいきましょう.tak"), true);
            }
        }

        [TestMethod]
        public void CueSheetParse01()
        {
            // Comment data are not covered with quotes so we'll not check it are equal or not.
            string cueSheetData = "REM GENRE Anime\r\nREM DATE 2011\r\nREM DISCID 2103F104\r\nREM COMMENT ExactAudioCopy v0.99pb5\r\nPERFORMER \"七森中☆ごらく部\"\r\nTITLE \"マイペースでいきましょう\"\r\nFILE \"七森中☆ごらく部 - マイペースでいきましょう.wav\" WAVE\r\n  TRACK 01 AUDIO\r\n    TITLE \"マイペースでいきましょう\"\r\n    ISRC JPPC01101399\r\n    INDEX 01 00:00:00\r\n  TRACK 02 AUDIO\r\n    TITLE \"Precious Friends\"\r\n    ISRC JPPC01101400\r\n    INDEX 00 03:39:68\r\n    INDEX 01 03:42:38\r\n  TRACK 03 AUDIO\r\n    TITLE \"マイペースでいきましょう [からおけ]\"\r\n    ISRC JPPC01101401\r\n    INDEX 00 08:22:52\r\n    INDEX 01 08:26:22\r\n  TRACK 04 AUDIO\r\n    TITLE \"Precious Friends [からおけ]\"\r\n    ISRC JPPC01101402\r\n    INDEX 00 12:06:00\r\n    INDEX 01 12:08:45\r\n";
            var result = new Cuesheet(cueSheetData);
            if (result is null)
            {
                throw new NullReferenceException("Result is null.");
            }
            else
            {
                // Detect main metadata are equal or not
                Assert.AreEqual(result.Root.Title, "マイペースでいきましょう");
                Assert.AreEqual(result.Root.Performer, "七森中☆ごらく部");
                Assert.AreEqual(result.Root.Genre, "Anime");
                Assert.AreEqual(result.Root.Date, "2011");
                Assert.AreEqual(result.Root.DiscId, "2103F104");
                // Detect probes metadata are equal or not
                Assert.AreEqual(result.AudioProbes[0].Title, "マイペースでいきましょう");
                Assert.AreEqual(result.AudioProbes[0].TimePointer, new TimeSpan());

                Assert.AreEqual(result.AudioProbes[1].Title, "Precious Friends");
                var target = new TimeSpan(0, 0, 3, 42, 380);
                Assert.AreEqual(result.AudioProbes[1].TimePointer, target);

                Assert.AreEqual(result.AudioProbes[2].Title, "マイペースでいきましょう [からおけ]");
                target = new TimeSpan(0, 0, 8, 26, 220);
                Assert.AreEqual(result.AudioProbes[2].TimePointer, target);

                Assert.AreEqual(result.AudioProbes[3].Title, "Precious Friends [からおけ]");
                target = new TimeSpan(0, 0, 12, 08, 450);
                Assert.AreEqual(result.AudioProbes[3].TimePointer, target);
            }
        }

        [TestMethod]
        public void CheckValidationFFmpegParameters()
        {
            string cueSheetData = "REM GENRE Anime\r\nREM DATE 2011\r\nREM DISCID 2103F104\r\nREM COMMENT ExactAudioCopy v0.99pb5\r\nPERFORMER \"七森中☆ごらく部\"\r\nTITLE \"マイペースでいきましょう\"\r\nFILE \"七森中☆ごらく部 - マイペースでいきましょう.wav\" WAVE\r\n  TRACK 01 AUDIO\r\n    TITLE \"マイペースでいきましょう\"\r\n    ISRC JPPC01101399\r\n    INDEX 01 00:00:00\r\n  TRACK 02 AUDIO\r\n    TITLE \"Precious Friends\"\r\n    ISRC JPPC01101400\r\n    INDEX 00 03:39:68\r\n    INDEX 01 03:42:38\r\n  TRACK 03 AUDIO\r\n    TITLE \"マイペースでいきましょう [からおけ]\"\r\n    ISRC JPPC01101401\r\n    INDEX 00 08:22:52\r\n    INDEX 01 08:26:22\r\n  TRACK 04 AUDIO\r\n    TITLE \"Precious Friends [からおけ]\"\r\n    ISRC JPPC01101402\r\n    INDEX 00 12:06:00\r\n    INDEX 01 12:08:45\r\n";
            var result = Program.CreateFFMpegParameters(".\\", "test", cueSheetData, new TimeSpan(0, 0, 16, 49));
            Assert.AreEqual(result, "-y -i test -ss 00:00:00.00 -t 00:03:42.38 -metadata \"genre=Anime\" -metadata \"date=2011\" -metadata \"discid=2103F104\" -metadata \"comment=ExactAudioCopy\" -metadata \"artist=七森中☆ごらく部\" -metadata \"album=マイペースでいきましょう\" -metadata \"title=マイペースでいきましょう\"  \".\\マイペースでいきましょう.flac\" -ss 00:03:42.38 -t 00:04:43.84 -metadata \"genre=Anime\" -metadata \"date=2011\" -metadata \"discid=2103F104\" -metadata \"comment=ExactAudioCopy\" -metadata \"artist=七森中☆ごらく部\" -metadata \"album=マイペースでいきましょう\" -metadata \"title=Precious Friends\"  \".\\Precious Friends.flac\" -ss 00:08:26.22 -t 00:03:42.23 -metadata \"genre=Anime\" -metadata \"date=2011\" -metadata \"discid=2103F104\" -metadata \"comment=ExactAudioCopy\" -metadata \"artist=七森中☆ごらく部\" -metadata \"album=マイペースでいきましょう\" -metadata \"title=マイペースでいきましょう [からおけ]\"  \".\\マイペースでいきましょう [からおけ].flac\" -ss 00:12:08.45 -t 00:04:40.55 -metadata \"genre=Anime\" -metadata \"date=2011\" -metadata \"discid=2103F104\" -metadata \"comment=ExactAudioCopy\" -metadata \"artist=七森中☆ごらく部\" -metadata \"album=マイペースでいきましょう\" -metadata \"title=Precious Friends [からおけ]\"  \".\\Precious Friends [からおけ].flac\" ");
        }

        [TestMethod]
        public void RunSplit()
        {
            // Next media file will not provided by any reasons.
            string takFile = System.IO.Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "data", "マイペースでいきましょう.tak");
            takFile = System.IO.Path.GetRelativePath(Environment.CurrentDirectory, takFile);
            Program.Main(new string[] { "-i", takFile });
        }
    }
}
