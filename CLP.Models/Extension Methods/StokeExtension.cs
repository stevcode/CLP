using System;
using System.Linq;
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
                if (!s.ContainsPropertyData(CLPPage.StrokeIDKey))
                {
                    string newUniqueID = Guid.NewGuid().ToString();
                    s.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("GetStrokeUniqueID Fail");
            }

            try
            {
                return s.GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            catch(Exception)
            {
                Console.WriteLine("GetStrokeUniqueID Fail");
                return "";
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
            }
            catch(Exception)
            {
                Console.WriteLine("SetStrokeUniqueID Fail");
            }

            try
            {
                s.AddPropertyData(CLPPage.StrokeIDKey, uniqueID);
            }
            catch(Exception)
            {
                Console.WriteLine("SetStrokeUniqueID Fail");
            }
        }
    }
}
