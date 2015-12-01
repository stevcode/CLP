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
    /* not being used currently - should it be?
    //serialize this
    [Serializable()]
    public class Preferences
    {
        [System.Xml.Serialization.XmlElement("visibleButtons")]
        public ObservableCollection<string> visibleButtons { get; set; }

        [System.Xml.Serialization.XmlElement("color")]
        public string color { get; set; }
    }
    */

    public class PreferencesService : IPreferencesService
    {
        private ObservableCollection<string> buttonsToShow = new ObservableCollection<string>();
        private string savedColorVal;

        public ObservableCollection<string> visibleButtons
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


        public String savedColor
        {
            get
            { return savedColorVal; }
            set
            { savedColorVal = value; }
        }

        //private string path = "/users/dirk/desktop/prefs.xml"; //TODO: use name composite path
        private XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));
        //private XmlSerializer serializer = new XmlSerializer(typeof(PreferencesService));

        //call this to save what preferences we currently have
        public void savePreferencesToDisk(string folderPath)
        {

            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath);
            Console.WriteLine(nameComposite.ToFileName());
            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate);

            //var prefService = ServiceLocator.Default.ResolveType<IPreferencesService>();
            //prefService.visibleButtons.Add("TestingStringToVisibleButtons");

            Console.WriteLine("Serializing xml for preferences to " + filePath);
            //serializer.Serialize(stream, prefService);
            serializer.Serialize(stream, this.visibleButtons);
            stream.Close();
        }

        public void loadPreferencesFromDisk(string folderPath)
        {
            var nameComposite = PreferencesNameComposite.ParseFilePath(folderPath);
            var fileName = nameComposite.ToFileName();

            var filePath = Path.Combine(folderPath, nameComposite.ToFileName() + ".xml");

            StreamReader reader = new StreamReader(filePath);
            this.visibleButtons = (ObservableCollection<string>)serializer.Deserialize(reader);
            reader.Close();

            
            Console.WriteLine("visibleButtons after loading:");
            foreach (string s in this.visibleButtons)
            {
                Console.WriteLine(s);
            }
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
