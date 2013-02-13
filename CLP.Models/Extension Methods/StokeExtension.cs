using System;
using System.Windows.Ink;

namespace CLP.Models
{
    public static class StokeExtension
    {
        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");

        public static string GetStrokeUniqueID(this Stroke s)
        {
            if (!s.ContainsPropertyData(StrokeIDKey))
            {
                string newUniqueID = Guid.NewGuid().ToString();
                s.AddPropertyData(StrokeIDKey, newUniqueID);
            }

            return s.GetPropertyData(StrokeIDKey) as string;
        }

        public static void SetStrokeUniqueID(this Stroke s, string uniqueID)
        {
            if(s.ContainsPropertyData(StrokeIDKey))
            {
                s.RemovePropertyData(StrokeIDKey);
            }

            s.AddPropertyData(StrokeIDKey, uniqueID);
        }
    }
}
