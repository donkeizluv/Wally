using System;
using System.Net;
using System.Security;
using System.Security.Permissions;

namespace Wally.HTML
{
    /// <summary>
    /// Wraps getting AppDomain permissions
    /// </summary>
    internal class PermissionHelper : IPermissionHelper
    {
        /// <summary>
        /// Checks to see if Registry access is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsRegistryAvailable()
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            RegistryPermission writePermission = new RegistryPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);
            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }

        /// <summary>
        /// Checks to see if DNS information is available to the caller
        /// </summary>
        /// <returns></returns>
        public bool GetIsDnsAvailable()
        {
            PermissionSet permissionSet = new PermissionSet(PermissionState.None);
            DnsPermission writePermission = new DnsPermission(PermissionState.Unrestricted);
            permissionSet.AddPermission(writePermission);
            return permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }
    }
}