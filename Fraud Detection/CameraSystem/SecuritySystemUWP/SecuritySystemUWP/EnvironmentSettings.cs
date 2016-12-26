using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace SecuritySystemUWP
{
    /// <summary>
    /// Helper class to retrieve device environment settings
    /// </summary>
    public static class EnvironmentSettings
    {
        /// <summary>
        /// Returns the name of current device as a string
        /// </summary>
        public static string GetDeviceName()
        {
            // iterate hostnames to find device name
            var hostname = NetworkInformation.GetHostNames()
                .FirstOrDefault(x => x.Type == HostNameType.DomainName);
            if (hostname != null)
            {
                return hostname.CanonicalName;
            }
            // if not found
            return "Unknown";
        }

        public static string GetIPAddress()
        {
            //get the connection profiles
            var icp = NetworkInformation.GetInternetConnectionProfile();

            //verify that we have content
            if (icp != null
                  && icp.NetworkAdapter != null
                  && icp.NetworkAdapter.NetworkAdapterId != null)
            {
                //get the profile name
                var name = icp.ProfileName;

                //get the system's host name
                var hostnames = NetworkInformation.GetHostNames();

                //verify each hostname information
                foreach (var hn in hostnames)
                {
                    if (hn.IPInformation != null
                        && hn.IPInformation.NetworkAdapter != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                                                   != null
                        && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                                    == icp.NetworkAdapter.NetworkAdapterId
                        && hn.Type == HostNameType.Ipv4)
                    {
                        //return current ip
                        return hn.CanonicalName;
                    }
                }
            }

            //if we do not have any IP, return 0.0.0.0
            return "0.0.0.0";
        }

        /// <summary>
        /// Retrieves current operating system version
        /// </summary>
        /// <returns>OS version</returns>
        public static string GetOSVersion()
        {
            ulong version = 0;
            // grab and parse OS version
            if (ulong.TryParse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion, out version))
            {
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}",
                    (version & 0xFFFF000000000000) >> 48,
                    (version & 0x0000FFFF00000000) >> 32,
                    (version & 0x00000000FFFF0000) >> 16,
                    version & 0x000000000000FFFF);
            }
            // if not found
            return "0.0.0.0";
        }

        /// <summary>
        /// Retrieves current application version
        /// </summary>
        /// <returns>App version</returns>
        public static string GetAppVersion()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
        }

    }
}
