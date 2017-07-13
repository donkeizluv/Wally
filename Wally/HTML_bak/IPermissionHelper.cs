namespace Wally.HTML
{
    /// <summary>
    /// An interface for getting permissions of the running application
    /// </summary>
    internal interface IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsRegistryAvailable();

        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        bool GetIsDnsAvailable();
    }
}