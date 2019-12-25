using System.IO;
using Day25.Speech.Callbacks;
using Microsoft.CognitiveServices.Speech.Audio;

namespace Day25.Speech.Extensions
{
    public static class AudioConfigExtension
    {
        public static AudioConfig OpenWavFile(this string filename)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filename));
            return reader.OpenWavFile();
        }

        public static AudioConfig OpenWavFile(this BinaryReader reader)
        {
            AudioStreamFormat format = reader.ReadWaveHeader();
            return AudioConfig.FromStreamInput(new BinaryAudioStreamReader(reader), format);
        }
    }
}