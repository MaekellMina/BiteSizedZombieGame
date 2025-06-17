using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using NaughtyAttributes;

namespace cc.IntrusiveThots
{


    /// <summary>
    ///Sample Rate	 Meaning             	Quality             	        File Size
    ///22050 Hz      22.05k samples/sec	    Lower quality,smaller size  	Small
    ///44100 Hz	     44.1k samples/sec	    CD quality (default)	        Medium
    ///48000 Hz	     48k samples/sec	    Used in video production	    Medium
    ///96000+ Hz     96k+ samples/sec	    High-resolution audio	        Large
    /// </summary>
    public class BeepGenerator : MonoBehaviour
    {

        public enum Note
        {
            C3, D3, E3, F3, G3, A3, B3,
            C4, D4, E4, F4, G4, A4, B4,
            C5, D5, E5, F5, G5, A5, B5,
            C6
        }
        public enum Channels
        {
            Mono,
            Stereo

        }


        [SerializeField]
        string Filename = "Beep";

        [SerializeField]
        int sampleRate = 44100;

        [SerializeField]
        float frequency = 440;

        [SerializeField]
        float duration = 1.0f; // 1 second
        [SerializeField]
        public Note selectedNote = Note.A4;
        [SerializeField]
        public Channels channel = Channels.Mono;

        [Button]
        void GenerateBeep()
        {
            string directory = Path.Combine(Application.dataPath, "Beeps");
            Directory.CreateDirectory(directory); // Ensure the folder exists

            string path = Path.Combine(directory, $"{Filename}_{selectedNote}.wav");

            short[] data = GenerateSineWave(sampleRate, frequency, duration);

            SaveWav(path, data, sampleRate);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif


        }
        short[] GenerateSineWave(int sampleRate, float freq, float duration)
        {
            int samples = (int)(sampleRate * duration);
            short[] buffer = new short[samples];
            double amplitude = 32760; // Max value for 16-bit audio?
            double angleIncrement = 2.0 * Math.PI * freq / sampleRate;

            for (int i = 0; i < samples; i++)
            {
                buffer[i] = (short)(amplitude * Math.Sin(angleIncrement * i));
            }

            return buffer;
        }

        void SaveWav(string filePath, short[] samples, int sampleRate)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                int byteRate = sampleRate * 2; // mono, 16-bit
                int dataSize = samples.Length * 2;

                // RIFF header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt subchunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // PCM
                writer.Write((short)1); // format: PCM

                if (channel == Channels.Mono)
                    writer.Write((short)1);
                else
                    writer.Write((short)2);

                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)2); // block align
                writer.Write((short)16); // bits per sample

                // data subchunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                // Write the audio samples
                foreach (short sample in samples)
                    writer.Write(sample);
            }
        }



        private void OnValidate()
        {
            frequency = NoteToFrequency(selectedNote);
        }
        public static float NoteToFrequency(Note note)
        {
            // Frequencies based on A4 = 440Hz
            switch (note)
            {
                case Note.C3: return 130.81f;
                case Note.D3: return 146.83f;
                case Note.E3: return 164.81f;
                case Note.F3: return 174.61f;
                case Note.G3: return 196.00f;
                case Note.A3: return 220.00f;
                case Note.B3: return 246.94f;
                case Note.C4: return 261.63f;
                case Note.D4: return 293.66f;
                case Note.E4: return 329.63f;
                case Note.F4: return 349.23f;
                case Note.G4: return 392.00f;
                case Note.A4: return 440.00f;
                case Note.B4: return 493.88f;
                case Note.C5: return 523.25f;
                case Note.D5: return 587.33f;
                case Note.E5: return 659.25f;
                case Note.F5: return 698.46f;
                case Note.G5: return 783.99f;
                case Note.A5: return 880.00f;
                case Note.B5: return 987.77f;
                case Note.C6: return 1046.50f;
                default: return 440.00f;
            }
        }
    }


}