using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Catel.Data;
using Catel.Runtime.Serialization;

namespace CLP.Entities
{
    [Serializable]
    public class GridDisplay : ADisplayBase
    {
        #region Constructors

        /// <summary>
        /// Initializes <see cref="GridDisplay" /> from scratch.
        /// </summary>
        public GridDisplay() { }

        /// <summary>
        /// Initializes <see cref="GridDisplay" /> from parent <see cref="Notebook" />.
        /// </summary>
        public GridDisplay(Notebook notebook)
            : base(notebook) { }

        /// <summary>
        /// Initializes <see cref="GridDisplay" /> based on <see cref="SerializationInfo" />.
        /// </summary>
        /// <param name="info"><see cref="SerializationInfo" /> that contains the information.</param>
        /// <param name="context"><see cref="StreamingContext" />.</param>
        public GridDisplay(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion //Constructors

        #region Methods

        public override void AddPageToDisplay(CLPPage page)
        {
            Pages.Add(page);
        }

        public override void RemovePageFromDisplay(CLPPage page)
        {
            Pages.Remove(page);
        }

        #endregion //Methods

        #region Cache

        public override void ToXML(string filePath)
        {           
            var fileInfo = new FileInfo(filePath);
            if(!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }

            using(Stream stream = new FileStream(filePath, FileMode.Create))
            {
                var xmlSerializer = SerializationFactory.GetXmlSerializer();
                xmlSerializer.Serialize(this, stream);
                ClearIsDirtyOnAllChilds();
            }
        }

        public override void Save(string folderPath)
        {
            var fileName = "grid" + ";" + DisplayNumber + ";" + ID + ".xml";
            CompositePageIDs.Clear();
            foreach(var compositeID in Pages.Select(page => page.ID + ";" + page.OwnerID + ";" + page.DifferentiationLevel + ";" + page.VersionIndex)) 
            {
                CompositePageIDs.Add(compositeID);
            }
            var filePath = Path.Combine(folderPath, fileName);
            ToXML(filePath);
        }

        public static IDisplay Load(string filePath, Notebook notebook)
        {
            var gridDisplay = Load<GridDisplay>(filePath, SerializationMode.Xml);
            if(gridDisplay == null)
            {
                return null;
            }

            foreach(var compositePageID in gridDisplay.CompositePageIDs)
            {
                var compositeSections = compositePageID.Split(';');
                var id = compositeSections[0];
                var ownerID = compositeSections[1];
                var differentiationlevel = compositeSections[2];
                var versionindex = Convert.ToInt32(compositeSections[3]);

                var page = notebook.GetPageByCompositeKeys(id, ownerID, versionindex);
                if(page == null)
                {
                    continue;
                }

                gridDisplay.Pages.Add(page);
            }

            return gridDisplay;
        }

        #endregion //Cache
    }
}