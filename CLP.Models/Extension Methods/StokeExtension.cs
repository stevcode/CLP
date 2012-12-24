using System;
using System.Windows.Ink;

namespace CLP.Models
{
    public static class StokeExtension
    {
        public static string GetStrokeUniqueID(this Stroke s)
        {
            if (!s.ContainsPropertyData(CLPPage.StrokeIDKey))
            {
                string newUniqueID = Guid.NewGuid().ToString();
                s.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
            }

            return s.GetPropertyData(CLPPage.StrokeIDKey) as string;
        }
    }
}
