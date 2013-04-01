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
                if(s == null)
                {
                    Console.WriteLine("Null Stroke");
                    return null;
                }

                if (!s.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    var newUniqueID = Guid.NewGuid().ToString();
                    s.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                }
                return s.GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            catch(Exception)
            {
                Console.WriteLine("GetStrokeUniqueID Fail");
                return null;
            }
        }

        public static void SetStrokeUniqueID(this Stroke s, string uniqueID)
        {
            try
            {
                if(s.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    s.RemovePropertyData(CLPPage.StrokeIDKey);
                }

                s.AddPropertyData(CLPPage.StrokeIDKey, uniqueID);
            }
            catch(Exception)
            {
                Console.WriteLine("SetStrokeUniqueID Fail");
            }
        }
    }
}
