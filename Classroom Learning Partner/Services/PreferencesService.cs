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

        private string path = "/users/dirk/desktop/prefs.xml"; //TODO: use name composite path
        private XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<string>));

        //call this to save what preferences we currently have
        public void savePreferencesToDisk()
        {
            FileStream stream = new FileStream(path, FileMode.OpenOrCreate);

            //var prefService = ServiceLocator.Default.ResolveType<IPreferencesService>();
            //prefService.visibleButtons.Add("TestingStringToVisibleButtons");

            Console.WriteLine("Serializing xml for preferences to " + path);
            //serializer.Serialize(stream, prefService);
            serializer.Serialize(stream, this.visibleButtons);
            stream.Close();
        }

        public void loadPreferencesFromDisk()
        {
            //testing deserialization
            StreamReader reader = new StreamReader(path);
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

        public static PreferencesNameComposite ParseFilePath(string prefFilePath)
        {
            var fileInfo = new FileInfo(prefFilePath);
            var pageFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            var pageFileParts = pageFileName.Split(';');
            if (pageFileParts.Length != 3)
            {
                return null;
            }

            var nameComposite = new PreferencesNameComposite
            {
                ID = pageFileParts[1],
                CLASS_NAME = pageFileParts[2],
            };

            return nameComposite;
        }
    }
}
