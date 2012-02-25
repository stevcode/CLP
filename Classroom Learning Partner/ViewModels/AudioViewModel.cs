using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Classroom_Learning_Partner.Model;

namespace Classroom_Learning_Partner.ViewModels
{
    public class AudioViewModel
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        bool recording = false;
        String path;
        public AudioViewModel(String filepath)
        {
            //path = "C:\\Users\\AudioFiles\\"+ filepath + "audio.wav";
            path = "C:\\Audio_Files\\" + filepath + ".wav";
            if (!Directory.Exists("C:\\Audio_Files"))
            {
                DirectoryInfo worked = Directory.CreateDirectory("C:\\Audio_Files");
            }

          /*  AppMessages.Audio.Register(this, (tup) =>
            {
                string item = tup.Item1;
                //this.path = tup.Item2;
                if (item == "start")
                {
                    if (recording == false)
                    {
                        Logger.Instance.WriteToLog("Recording Audio and saving to " + path);
                        recordAudio();
                    }
                    else
                    {
                        stopAudio();
                    }
                    recording = !recording;
                }
                else if(!recording && item == "play")
                {
                    playAudio();
                }
            });
           */
        }
        public void AudioButtonPressed(string item, string path)
        {
            if (item == "start")
            {
                if (recording == false)
                {
                    Logger.Instance.WriteToLog("Recording Audio and saving to " + path);
                    recordAudio();
                }
                else
                {
                    stopAudio();
                }
                recording = !recording;
            }
            else if (!recording && item == "play")
            {
                playAudio();
            }
        }
        private void recordAudio()
        {
            mciSendString("open new Type waveaudio Alias recsound", "", 0, 0);
            mciSendString("record recsound", "", 0, 0);
            //Console.WriteLine("recording, press Enter to stop and save ...");
            //System.Threading.Thread.Sleep(5000);

           // Console.WriteLine(mciSendString("save recsound C:\\Users\\Claire\\Documents\\result.wav", "", 0, 0));
           
        }
        private void stopAudio()
        {
            mciSendString("save recsound "+ path, "", 0, 0);
            mciSendString("close recsound ", "", 0, 0);
        }
        private void playAudio()
        {
            
            Console.WriteLine("Playing audio");
            string s;
           
            // access media file
            //s = "open \"C:\\Users\\Claire\\"+path+" type waveaudio alias mysound";
            Console.WriteLine(mciSendString("open "+path+" type waveaudio alias mysound", null, 0, 0));

            // play from start
            s = "play mysound from 0 wait";	// append "wait" if you want blocking
            mciSendString(s, null, 0, 0);
            //System.Threading.Thread.Sleep(2000);
            // stop playback
            s = "stop mysound";		// if playing asynchronously
            mciSendString(s, null, 0, 0);

            // deallocate resources
            s = "close mysound";
            mciSendString(s, null, 0, 0);
        }
       /* String lengthBuf = new String();
        public static int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, 0);
            mciSendString("status wave length", lengthBuf, 200, 0);
            mciSendString("close wave", null, 0, 0);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }*/
    }
}
