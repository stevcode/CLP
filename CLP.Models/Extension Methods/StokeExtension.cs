using System;
using System.Windows.Ink;

namespace CLP.Models
{
    public static class StokeExtension
    {
        public static Guid StrokeIDKey = new Guid("00000000-0000-0000-0000-000000000001");

        public static string GetStrokeUniqueID(this Stroke s)
        {
            try
            {
                if (!s.ContainsPropertyData(StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    s.AddPropertyData(StrokeIDKey, newUniqueID);
                }
            }
            catch(Exception)
            {
                
            }

            try
            {
                return s.GetPropertyData(StrokeIDKey) as string;
            }
            catch(Exception)
            {
                return "";
            }
        }

        public static void SetStrokeUniqueID(this Stroke s, string uniqueID)
        {
            try
            {
                if(s.ContainsPropertyData(StrokeIDKey))
                {
                    s.RemovePropertyData(StrokeIDKey);
                }
            }
            catch(Exception)
            {
            }

            try
            {
                s.AddPropertyData(StrokeIDKey, uniqueID);
            }
            catch(Exception)
            {
            }
        }
    }
}
