using System;
using System.Net;
using System.Security;
using System.Security.Permissions;

namespace Wally.HTML
{
    /// <summary>
    ///     Wraps getting AppDomain permissions
    /// </summary>
    internal class PermissionHelper : IPermissionHelper
    {
        /// <summary>
        ///     Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsDnsAvailable()
        {
            var permissionSets = new PermissionSet(PermissionState.None);
            permissionSets.AddPermission(new DnsPermission(PermissionState.Unrestricted));
            return permissionSets.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        /// <summary>
        ///     Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsRegistryAvailable()
        {
            var permissionSets = new PermissionSet(PermissionState.None);
            permissionSets.AddPermission(new RegistryPermission(PermissionState.Unrestricted));
            return permissionSets.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }
    }
}