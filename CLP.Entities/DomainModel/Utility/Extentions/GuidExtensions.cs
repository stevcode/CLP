using System;
using Catel;

namespace CLP.Entities.Ann
{
    public static class GuidExtensions
    {
        public static string ToCompactID(this Guid guid)
        {
            Argument.IsNotNull("guid", guid);

            var compactedID = Convert.ToBase64String(guid.ToByteArray());
            compactedID = compactedID.Replace("/", "_").Replace("+", "-").Replace("=","");
  
            return compactedID;
        }
    }
}
