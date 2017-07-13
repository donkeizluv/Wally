using System;
using System.Diagnostics;

namespace Wally.Day_Dream
{
    internal static class ExManager
    {
        public static void Ex(Exception ex)
        {
            Debug.WriteLine($"Ex: {ex.Message}");
            Debug.WriteLine($"Inner Ex: {ex.InnerException?.Message?? "null"}");
#if DEBUG
            throw ex;
#endif
        }
    }
}