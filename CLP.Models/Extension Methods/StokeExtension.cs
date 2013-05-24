using System;
using System.Windows.Ink;

namespace CLP.Models
{
    public static class StokeExtension
    {
        public static string GetStrokeUniqueID(this Stroke s)
        {
            try
            {
                if(s == null) //|| !s.ContainsPropertyData(CLPPage.StrokeIDKey)
                {
                    Logger.Instance.WriteToLog("Null Stroke or Missing StrokeID");
                    return null;
                }

                return s.GetPropertyData(CLPPage.StrokeIDKey) as string;

            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("GetStrokeUniqueID Fail: " + ex.Message);
                return null;
            }
        }

        public static void SetStrokeUniqueID(this Stroke s, string uniqueID)
        {
            try
            {
                s.AddPropertyData(CLPPage.StrokeIDKey, uniqueID);
            }
            catch(Exception ex)
            {
                Logger.Instance.WriteToLog("SetStrokeUniqueID Fail (Top level): " + ex.Message);
            }
        }
    }
}
