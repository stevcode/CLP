using System;
using Catel;

namespace Classroom_Learning_Partner
{
    public static class ObjectExtentions
    {
        public static double? ToDouble(this object potentialNumber)
        {
            Argument.IsNotNull("potentialNumber", potentialNumber);

            double number;

            try
            {
                number = Convert.ToDouble(potentialNumber);
            }
            catch (Exception)
            {
                return null;
            }

            return number;
        }

        public static int? ToInt(this object potentialNumber)
        {
            Argument.IsNotNull("potentialNumber", potentialNumber);

            int number;

            try
            {
                number = Convert.ToInt32(potentialNumber);
            }
            catch (Exception)
            {
                return null;
            }

            return number;
        }
    }
}