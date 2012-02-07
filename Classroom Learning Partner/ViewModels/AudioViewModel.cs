using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Classroom_Learning_Partner.ViewModels
{
    class AudioViewModel
    {
        [DllImport("winmm.dll", EntryPoint = "mciSendStringA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        bool recording = false;
        String path;
        public AudioViewModel(String filepath)
        {
            path = "C:\\Users\\Claire\\"+ filepath + "audio.wav";
            AppMessages.Audio.Register(this, (item) =>
            {
                if (item == "start")
                {
                    if (recording == false)
                    {
                        System.Console.WriteLine("Recording Audio and saving to " + path);
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
            s = "play mysound from 0";	// append "wait" if you want blocking
            mciSendString(s, null, 0, 0);
            System.Threading.Thread.Sleep(2000);
            // stop playback
            s = "stop mysound";		// if playing asynchronously
            mciSendString(s, null, 0, 0);

            // deallocate resources
            s = "close mysound";
            mciSendString(s, null, 0, 0);
        }
    }
}
