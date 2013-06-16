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
                if(s == null)
                {
                    Logger.Instance.WriteToLog("Null Stroke or Missing StrokeID");
                    return null;
                }

                return s.GetPropertyData(CLPPage.StrokeIDKey) as string;
            }
            catch(Exception ex)
            {
                var nullTest = s == null ? "TRUE" : "FALSE";
                Logger.Instance.WriteToLog("Stroke is null: " + nullTest);
                Logger.Instance.WriteToLog("GetStrokeUniqueID Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " +
                                           (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
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
                var nullTest = s == null ? "TRUE" : "FALSE";
                Logger.Instance.WriteToLog("SetStrokeUniqueID Fail. uniqueID is: " + uniqueID);
                Logger.Instance.WriteToLog("Stroke is null: " + nullTest);
                Logger.Instance.WriteToLog("SetStrokeUniqueID Exception: " + ex.Message);
                Logger.Instance.WriteToLog("[UNHANDLED ERROR] - " + ex.Message + " " +
                                           (ex.InnerException != null ? "\n" + ex.InnerException.Message : null));
                Logger.Instance.WriteToLog("[HResult]: " + ex.HResult);
                Logger.Instance.WriteToLog("[Source]: " + ex.Source);
                Logger.Instance.WriteToLog("[Method]: " + ex.TargetSite);
                Logger.Instance.WriteToLog("[StackTrace]: " + ex.StackTrace);
            }
        }
    }
}
