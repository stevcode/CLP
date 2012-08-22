using System;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.Serialization;
using Catel.Data;
using System.Diagnostics;

namespace CLP.Models
{
    [Serializable]
    [AllowNonSerializableMembers]
    public class CLPAudio : CLPPageObjectBase
    {

        #region Constructor & destructor
        /// <summary>
        /// Initializes a new object from scratch.
        /// </summary>
        public CLPAudio(string ID)
            : base()
        {
            //string fullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\" + path + ".mp3";
            //if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files"))
            //{
            //    DirectoryInfo worked = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\");
            //}
            //int i = 0;
            //while (File.Exists(fullPath))
            //{
            //    fullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Audio_Files\" + path + i.ToString() + ".mp3";
            //    i++;
            //}
            this.File = new Byte[0];
            this.ID = ID;
            XPosition = 10;
            YPosition = 10;
            Height = 70;
            Width = 200;
            CanAcceptStrokes = false;
        }

        //Parameterless constructor for Protobuf
        protected CLPAudio()
        {
            this.File = new Byte[0];
            this.ID = ID;
            XPosition = 10;
            YPosition = 10;
            Height = 50;
            Width = 50;
        }

        /// <summary>
        /// Initializes a new object based on <see cref="SerializationInfo"/>.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo"/> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext"/>.</param>
        protected CLPAudio(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion

        #region Properties
        
       public string FilePath
        {
            get { return GetValue<string>(FilePathProperty); }
            private set { SetValue(FilePathProperty, value); }
        }
        public static readonly PropertyData FilePathProperty = RegisterProperty("FilePath", typeof(string), null);
        
        public Byte[] File
        {
            get { return GetValue<Byte[]>(FileProperty); }
            set { SetValue(FileProperty, value); }
        }
        public static readonly PropertyData FileProperty = RegisterProperty("File", typeof(Byte[]), null);

        public string ID
        {
            get { return GetValue<string>(IDProperty); }
            set { SetValue(IDProperty, value); }
        }
        public static readonly PropertyData IDProperty = RegisterProperty("ID", typeof(string), null);


        #endregion

        #region methods
        public override string PageObjectType
        {
            get { return "CLPAudio"; }
        }

        public override ICLPPageObject Duplicate()
        {
            return this.Duplicate();
        }

        public static void ConvertWavMP3(string wavFile, string outmp3File)
        {
            if(System.IO.File.Exists(outmp3File))
            {
                System.IO.File.Delete(outmp3File);
            }
	        ProcessStartInfo psi = new ProcessStartInfo();
	        psi.FileName = @"External Libraries\lame.exe";
	        psi.Arguments = "-b 20 " + wavFile + " " + outmp3File;
	        psi.WindowStyle = ProcessWindowStyle.Hidden;
	        Process p = Process.Start(psi);
	        p.WaitForExit();
        }

        public static byte[] FileToByteArray(string _FileName)
        {
            byte[] _Buffer = null;

            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                // attach filestream to binary reader
                System.IO.BinaryReader _BinaryReader = new System.IO.BinaryReader(_FileStream);

                // get total byte length of the file
                long _TotalBytes = new System.IO.FileInfo(_FileName).Length;

                // read entire file into buffer
                _Buffer = _BinaryReader.ReadBytes((Int32)_TotalBytes);

                // close file reader
                _FileStream.Close();
                _FileStream.Dispose();
                _BinaryReader.Close();
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}", _Exception.ToString());
            }

            return _Buffer;
        }
        #endregion

    }
}
