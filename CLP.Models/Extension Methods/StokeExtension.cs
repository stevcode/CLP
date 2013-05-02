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
                    Logger.Instance.WriteToLog("Null Stroke");
                    return null;
                }

                //try
                //{
                    //if(!s.ContainsPropertyData(CLPPage.StrokeIDKey))
                    //{
                    //    try
                    //    {
                    //        var newUniqueID = Guid.NewGuid().ToString();
                    //        s.AddPropertyData(CLPPage.StrokeIDKey, newUniqueID);
                    //    }
                    //    catch(System.Exception ex)
                    //    {
                    //        Logger.Instance.WriteToLog("GetStrokeUniqueID Fail as AddPropertyData (Line 31): " + ex.Message);
                    //        return null;
                    //    }
                        
                    //}

                    try
                    {
                        return s.GetPropertyData(CLPPage.StrokeIDKey) as string;
                    }
                    catch(System.Exception ex)
                    {
                        Logger.Instance.WriteToLog("GetStrokeUniqueID Fail at GetPropertyData (Line 43): " + ex.Message);
                        return null;
                    }
                    
                //}
                //catch(System.Exception ex)
                //{
                //    Logger.Instance.WriteToLog("GetStrokeUniqueID Fail at ContainsPropertyData (Line 22): " + ex.Message);
                //    return null;
                //}
                
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
