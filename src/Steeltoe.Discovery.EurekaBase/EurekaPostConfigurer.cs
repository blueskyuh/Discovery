// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Configuration;
using Steeltoe.CloudFoundry.Connector.Services;
using System;

namespace Steeltoe.Discovery.Eureka
{
    public class EurekaPostConfigurer
    {
        public const string SPRING_APPLICATION_NAME_KEY = "spring:application:name";
        public const string SPRING_APPLICATION_INSTANCEID_KEY = "spring:application:instance_id";
        public const string SPRING_CLOUD_DISCOVERY_REGISTRATIONMETHOD_KEY = "spring:cloud:discovery:registrationMethod";

        internal const string EUREKA_URI_SUFFIX = "/eureka/";

        internal const string ROUTE_REGISTRATIONMETHOD = "route";
        internal const string DIRECT_REGISTRATIONMETHOD = "direct";
        internal const string HOST_REGISTRATIONMETHOD = "hostname";

        internal const string CF_APP_GUID = "cfAppGuid";
        internal const string CF_INSTANCE_INDEX = "cfInstanceIndex";
        internal const string SURGICAL_ROUTING_HEADER = "X-CF-APP-INSTANCE";
        internal const string INSTANCE_ID = "instanceId";
        internal const string ZONE = "zone";
        internal const string UNKNOWN_ZONE = "unknown";
        internal const int DEFAULT_NONSECUREPORT = 80;
        internal const int DEFAULT_SECUREPORT = 443;

        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaClientOptions clientOptions)
        {
            if (clientOptions == null || si == null)
            {
                return;
            }

            var uri = si.Uri;

            if (!uri.EndsWith(EUREKA_URI_SUFFIX))
            {
                uri = uri + EUREKA_URI_SUFFIX;
            }

            clientOptions.EurekaServerServiceUrls = uri;
            clientOptions.AccessTokenUri = si.TokenUri;
            clientOptions.ClientId = si.ClientId;
            clientOptions.ClientSecret = si.ClientSecret;
        }

        public static void UpdateConfiguration(IConfiguration config, EurekaInstanceOptions options)
        {
            var defaultId = options.GetHostName(false) + ":" + EurekaInstanceOptions.Default_Appname + ":" + EurekaInstanceOptions.Default_NonSecurePort;

            if (EurekaInstanceOptions.Default_Appname.Equals(options.AppName))
            {
                string springAppName = config.GetValue<string>(SPRING_APPLICATION_NAME_KEY);
                if (!string.IsNullOrEmpty(springAppName))
                {
                    options.AppName = springAppName;
                }
            }

            if (string.IsNullOrEmpty(options.VirtualHostName))
            {
                options.VirtualHostName = options.AppName;
            }

            if (string.IsNullOrEmpty(options.SecureVirtualHostName))
            {
                options.SecureVirtualHostName = options.AppName;
            }

            if (defaultId.Equals(options.InstanceId))
            {
                string springInstanceId = config.GetValue<string>(SPRING_APPLICATION_INSTANCEID_KEY);
                if (!string.IsNullOrEmpty(springInstanceId))
                {
                    options.InstanceId = springInstanceId;
                }
                else
                {
                    options.InstanceId = options.GetHostName(false) + ":" + options.AppName + ":" + options.NonSecurePort;
                }
            }

            if (string.IsNullOrEmpty(options.RegistrationMethod))
            {
                string springRegMethod = config.GetValue<string>(SPRING_CLOUD_DISCOVERY_REGISTRATIONMETHOD_KEY);
                if (!string.IsNullOrEmpty(springRegMethod))
                {
                    options.RegistrationMethod = springRegMethod;
                }
            }
        }

        public static void UpdateConfiguration(IConfiguration config, EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            if (instOptions == null)
            {
                return;
            }

            UpdateConfiguration(config, instOptions);

            if (si == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(instOptions.RegistrationMethod) ||
                ROUTE_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForRoute(si, instOptions);
                return;
            }

            if (DIRECT_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForDirect(si, instOptions);
                return;
            }

            if (HOST_REGISTRATIONMETHOD.Equals(instOptions.RegistrationMethod, StringComparison.OrdinalIgnoreCase))
            {
                UpdateWithDefaultsForHost(si, instOptions, instOptions.HostName);
                return;
            }
        }

        private static void UpdateWithDefaultsForHost(EurekaServiceInfo si, EurekaInstanceOptions instOptions, string hostName)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.HostName = hostName;
            instOptions.InstanceId = hostName + ":" + si.ApplicationInfo.InstanceId;
        }

        private static void UpdateWithDefaultsForDirect(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.PreferIpAddress = true;
            instOptions.NonSecurePort = si.ApplicationInfo.Port;
            instOptions.SecurePort = si.ApplicationInfo.Port;
            instOptions.InstanceId = si.ApplicationInfo.InternalIP + ":" + si.ApplicationInfo.InstanceId;
        }

        private static void UpdateWithDefaultsForRoute(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            UpdateWithDefaults(si, instOptions);
            instOptions.NonSecurePort = DEFAULT_NONSECUREPORT;
            instOptions.SecurePort = DEFAULT_SECUREPORT;

            if (si.ApplicationInfo.ApplicationUris?.Length > 0)
            {
                instOptions.InstanceId = si.ApplicationInfo.ApplicationUris[0] + ":" + si.ApplicationInfo.InstanceId;
            }
        }

        private static void UpdateWithDefaults(EurekaServiceInfo si, EurekaInstanceOptions instOptions)
        {
            if (si.ApplicationInfo.ApplicationUris != null && si.ApplicationInfo.ApplicationUris.Length > 0)
            {
                instOptions.HostName = si.ApplicationInfo.ApplicationUris[0];
            }

            instOptions.IpAddress = si.ApplicationInfo.InternalIP;

            var map = instOptions.MetadataMap;
            map[CF_APP_GUID] = si.ApplicationInfo.ApplicationId;
            map[CF_INSTANCE_INDEX] = si.ApplicationInfo.InstanceIndex.ToString();
            map[INSTANCE_ID] = si.ApplicationInfo.InstanceId;
            map[ZONE] = UNKNOWN_ZONE;
        }
    }
}
