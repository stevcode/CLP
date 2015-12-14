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
        private ObservableCollection<string> buttonsToShowTeacher = new ObservableCollection<string>();
        private ObservableCollection<string> buttonsToShowStudent = new ObservableCollection<string>();
        private ObservableCollection<string> buttonsToShowProjector = new ObservableCollection<string>();

        public enum prefType
        {
            TEACHER,STUDENT,PROJECTOR
        }

        public ObservableCollection<string> visibleButtonsTeacher
        {
            get
            {
                return buttonsToShowTeacher;
            }
            set
            {
                buttonsToShowTeacher = value;
            }
        }

        public ObservableCollection<string> visibleButtonsStudent
        {
            get
            {
                return buttonsToShowStudent;
            }
            set
            {
                buttonsToShowStudent = value;
            }
        }

        public ObservableCollection<string> visibleButtonsProjector
        {
            get
            {
                return buttonsToShowProjector;
            }
            set
            {
                buttonsToShowProjector = value;
            }
        }



        public void addPreference(string ID, prefType type)
        {
            if (type == PreferencesService.prefType.TEACHER)
            {
                if (!visibleButtonsTeacher.Contains(ID))
                {
                    visibleButtonsTeacher.Add(ID);
                }
            }
            if (type == PreferencesService.prefType.STUDENT)
            {
                if (!visibleButtonsStudent.Contains(ID))
                {
                    visibleButtonsStudent.Add(ID);
                }
            }
            if (type == PreferencesService.prefType.PROJECTOR)
            {
                if (!visibleButtonsProjector.Contains(ID))
                {
                    visibleButtonsProjector.Add(ID);
                }
            }
        }

        public void removePreference(string ID, prefType type)
        {
            if (type == PreferencesService.prefType.TEACHER)
            {
                visibleButtonsTeacher.Remove(ID);
            }
            if (type == PreferencesService.prefType.STUDENT)
            {
                visibleButtonsStudent.Remove(ID);
            }
            if (type == PreferencesService.prefType.PROJECTOR)
            {
                visibleButtonsProjector.Remove(ID);
            }
        }

        private string folderPath = null;

        public string FolderPath
        {
            get { return folderPath; }
            set { folderPath = value; }
        }

        private XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
        //private XmlSerializer serializer = new XmlSerializer(typeof(PreferencesService));


        private void savePreferencesToDiskHelper(string folderPath, PreferencesService.prefType type)
        {
            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath, type);
            Console.WriteLine(nameComposite.ToFileName());
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            //delete file to clear it (so buttons can be removed, and to stop duplicates in xml)
            FileInfo file = new FileInfo(filePath);
            file.Delete();

            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);

            if (type == PreferencesService.prefType.STUDENT)
            {
                serializer.Serialize(stream, this.visibleButtonsStudent);
            }
            else if (type == PreferencesService.prefType.TEACHER)
            {
                serializer.Serialize(stream, this.visibleButtonsTeacher);
            }
            else if (type == PreferencesService.prefType.PROJECTOR)
            {
                serializer.Serialize(stream, this.visibleButtonsProjector);
            }
            
            stream.Close();
        }

        //call this to save what preferences we currently have
        public void savePreferencesToDisk()
        {
            if (this.folderPath != null)
            {
                //Do we ever need to load any of the three preferences seperately?
                savePreferencesToDiskHelper(folderPath, PreferencesService.prefType.TEACHER);
                savePreferencesToDiskHelper(folderPath, PreferencesService.prefType.STUDENT);
                savePreferencesToDiskHelper(folderPath, PreferencesService.prefType.PROJECTOR);
            }
        }

        private void loadPreferencesFromDiskHelper(PreferencesService.prefType type)
        {
            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath, type);
            var fileName = nameComposite.ToFileName();

            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            try
            {
                StreamReader reader = new StreamReader(filePath);
                this.visibleButtonsTeacher = (ObservableCollection<string>)serializer.Deserialize(reader);
                reader.Close();
            }
            catch
            {
                
            }
        }

        public void loadPreferencesFromDisk()
        {
            loadPreferencesFromDiskHelper(PreferencesService.prefType.TEACHER);
            loadPreferencesFromDiskHelper(PreferencesService.prefType.STUDENT);
            loadPreferencesFromDiskHelper(PreferencesService.prefType.PROJECTOR);
        }
    }


    public class PreferencesNameComposite
    {
        public const string QUALIFIER_TEXT = "preferences";
        public string ID
        { get; set; }
        public string CLASS_NAME
        { get; set; }
        public string TYPE
        { get; set; }

        public string ToFileName()
        { return string.Format("{0};{1};{2};{3}", QUALIFIER_TEXT, CLASS_NAME, ID, TYPE); }

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
        
        public static PreferencesNameComposite ParseFilePath(string folderFilePath, PreferencesService.prefType type)
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
                TYPE = type.ToString()
            };

            return nameComposite;
        }
    }
}
