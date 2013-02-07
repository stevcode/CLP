using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.IO;

namespace CLP.Models
{
    public static class StrokeCollectionExtension
    {
        public static string SaveStrokeCollection(this StrokeCollection strokes)
        {
            

            MemoryStream stream = new MemoryStream();
            strokes.Save(stream, true);
            return "";
        }
    }
}
