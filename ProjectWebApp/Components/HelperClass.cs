using BYSResults;

namespace ProjectWebApp.Components
{
    public class HelperClass
    {
        //	Gets the Exception instance that caused the current exception.
        //	An object that describes the error that caused the current exception.
        public static Exception GetInnerException(System.Exception ex)
        {
            while (ex.InnerException != null)
                ex = ex.InnerException;
            return ex;
        }

        // Converts a list of error objects into their string representations.
        public static List<string> GetErrorMessages(List<Error> errorMessage)
        {
            // Initialize a new list to hold the extracted error messages
            List<string> errorList = new();

            // Iterate over each Error object in the incoming list
            foreach (var error in errorMessage)
            {
                // Convert the current Error to its string form and add it to errorList
                errorList.Add(error.ToString());
            }

            // Return the populated list of error message strings
            return errorList;
        }
    }
}
