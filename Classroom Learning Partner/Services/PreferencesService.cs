using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CLP.CustomControls;
using System.Collections.ObjectModel;

namespace Classroom_Learning_Partner.Services
{
    //serialize this
    [Serializable()]
    public class Preferences
    {
        [System.Xml.Serialization.XmlElement("visibleButtons")]
        public ObservableCollection<string> visibleButtons { get; set; }

        [System.Xml.Serialization.XmlElement("color")]
        public string color { get; set; }
    }


    public class PreferencesService : IPreferencesService
    {
        private ObservableCollection<string> buttonsToShow;
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
