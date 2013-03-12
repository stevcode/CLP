using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.IO;
using System.Windows;

namespace CLP.Models
{
    public static class StrokeCollectionExtension
    {
        public static DataObject SaveStrokeCollection(this StrokeCollection strokes)
        {
            DataObject serializedStrokes;
            using(MemoryStream strokesInMemory = new MemoryStream())
            {
                strokes.Save(strokesInMemory, true);
                serializedStrokes =  new DataObject(StrokeCollection.InkSerializedFormat, strokesInMemory);
            }

            return serializedStrokes;
        }

        public static void LoadStrokeCollection(this StrokeCollection strokes, DataObject serializedStrokes)
        {
            using(MemoryStream strokesInMemory = new MemoryStream())
            {
                strokes = new StrokeCollection(strokesInMemory);
            }
        }
    }
}
