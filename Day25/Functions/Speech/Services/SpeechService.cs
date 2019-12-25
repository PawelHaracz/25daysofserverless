using System;
using System.IO;
using System.Threading.Tasks;
using Day25.Speech.Extensions;
using Microsoft.CognitiveServices.Speech;

namespace Day25.Speech.Services
{
    public class SpeechService
    {
        private readonly SpeechConfig _config;
        
        public SpeechService(SpeechConfig config)
        {
            _config = config;
        }

        public async Task<string> ToText(Stream speech)
        {
            using var binaryReader = new BinaryReader(speech);
            using var audio = binaryReader.OpenWavFile();
            using var recognizer = new SpeechRecognizer(_config, audio);
            var result = await recognizer.RecognizeOnceAsync();
            
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                return result.Text;
            }
            
            throw new Exception(result.Reason.ToString());
        }
        
      
    }
}