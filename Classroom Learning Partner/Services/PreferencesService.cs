using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CLP.CustomControls;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Catel.IoC;

namespace Classroom_Learning_Partner.Services
{
    public class PreferencesService : IPreferencesService
    {
        private ObservableCollection<string> buttonsToShow = new ObservableCollection<string>();
        private string savedColorVal;

        public enum prefType
        {
            TEACHER,STUDENT,PROJECTOR
        }

        public ObservableCollection<string> visibleButtonsTeacher
        {
            get
            {
                return buttonsToShow;
            }
            set
            {
                buttonsToShow = value;
            }
        }

        public ObservableCollection<string> visibleButtonsStudent //TODO update this to separate from teacher
        {
            get
            {
                return buttonsToShow;
            }
            set
            {
                buttonsToShow = value;
            }
        }

        public ObservableCollection<string> visibleButtonsProjector
        {
            get
            {
                return buttonsToShow;
            }
            set
            {
                buttonsToShow = value;
            }
        }



        public void addPreference(string ID, prefType type)
        {
            if (type == PreferencesService.prefType.TEACHER)
            {
                visibleButtonsTeacher.Add(ID);
            }
            //TODO: implement the other types
        }

        public void removePreference(string ID, prefType type)
        {
            if (type == PreferencesService.prefType.TEACHER)
            {
                visibleButtonsTeacher.Remove(ID);
            }
            //TODO: implement the other types
        }


        public String savedColor
        {
            get
            { return savedColorVal; }
            set
            { savedColorVal = value; }
        }

        private string folderPath;

        public string FolderPath
        {
            get { return folderPath; }
            set { folderPath = value; }
        }

        private XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
        //private XmlSerializer serializer = new XmlSerializer(typeof(PreferencesService));

        //call this to save what preferences we currently have
        public void savePreferencesToDisk()
        {
            Console.WriteLine(this.folderPath);
            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath);
            Console.WriteLine(nameComposite.ToFileName());
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            FileInfo file = new FileInfo(filePath);
            file.Delete();

            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);

            //clear file
            //stream.SetLength(0);
            //stream.Close();

            //var prefService = ServiceLocator.Default.ResolveType<IPreferencesService>();
            //prefService.visibleButtonsTeacher.Add("TestingStringToVisibleButtons");

            Console.WriteLine("Serializing xml for preferences to " + filePath);
            //serializer.Serialize(stream, prefService);
            serializer.Serialize(stream, this.visibleButtonsTeacher);
            stream.Close();
        }

        public void loadPreferencesFromDisk()
        {
            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath);
            var fileName = nameComposite.ToFileName();

            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            StreamReader reader = new StreamReader(filePath);
            try
            {
                this.visibleButtonsTeacher = (ObservableCollection<string>)serializer.Deserialize(reader);
            }
            catch
            {
                //this.visibleButtonsTeacher = 
            }
            
            reader.Close();

        }
    }


    public class PreferencesNameComposite
    {
        public const string QUALIFIER_TEXT = "preferences";
        public string ID
        { get; set; }
        public string CLASS_NAME
        { get; set; }

        public string ToFileName()
        { return string.Format("{0};{1};{2};", QUALIFIER_TEXT, CLASS_NAME, ID); }

        /*
        public static PreferencesNameComposite ParsePreferences(PreferencesService prefService, string folderPath)
        {
            var nameComposite = new PreferencesNameComposite
            {
                ID = prefService;
                CLASS_NAME = "className" //TODO: update this
            };

            return nameComposite; 
        }*/
        
        public static PreferencesNameComposite ParseFilePath(string folderFilePath)
        {
            if (folderFilePath == null)
            {
                Console.WriteLine("null folderpath");
                return null;
            }
            var fileInfo = new FileInfo(folderFilePath);
            var pageFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var pageFileParts = pageFileName.Split(';');
            Console.WriteLine("folderFilePath: " + folderFilePath);
            //needs to be an author notebook and have the proper filename
            if (pageFileParts.Length != 5 || pageFileParts[4] != "A") 
            {
                return null;
            }

            var nameComposite = new PreferencesNameComposite
            {
                ID = pageFileParts[1],
                CLASS_NAME = pageFileParts[3],
            };

            return nameComposite;
        }
    }
}
