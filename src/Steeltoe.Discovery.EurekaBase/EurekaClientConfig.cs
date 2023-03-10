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

using System;

namespace Steeltoe.Discovery.Eureka
{
    public class EurekaClientConfig : IEurekaClientConfig
    {
        public const int Default_RegistryFetchIntervalSeconds = 30;
        public const int Default_InstanceInfoReplicationIntervalSeconds = 40;
        public const int Default_EurekaServerConnectTimeoutSeconds = 5;
        public const int Default_EurekaServerRetryCount = 3;
        public const string Default_ServerServiceUrl = "http://localhost:8761/eureka/";

        public EurekaClientConfig()
        {
            RegistryFetchIntervalSeconds = Default_RegistryFetchIntervalSeconds;
            ShouldGZipContent = true;
            EurekaServerConnectTimeoutSeconds = Default_EurekaServerConnectTimeoutSeconds;
            ShouldRegisterWithEureka = true;
            ShouldDisableDelta = false;
            ShouldFilterOnlyUpInstances = true;
            ShouldFetchRegistry = true;
            ShouldOnDemandUpdateStatusChange = true;
            EurekaServerServiceUrls = Default_ServerServiceUrl;
            ValidateCertificates = true;
            EurekaServerRetryCount = Default_EurekaServerRetryCount;
            HealthCheckEnabled = true;
            HealthContribEnabled = true;
        }

        // Configuration property: eureka:client:registryFetchIntervalSeconds
        public int RegistryFetchIntervalSeconds { get; set; }

        // Configuration property: eureka:client:instanceInfoReplicationIntervalSeconds
        [Obsolete("Eureka client does not use this value, will be removed in next release")]
        public int InstanceInfoReplicationIntervalSeconds { get; set; } = Default_InstanceInfoReplicationIntervalSeconds;

        // Configuration property: eureka:client:shouldRegisterWithEureka
        public bool ShouldRegisterWithEureka { get; set; }

        // Configuration property: eureka:client:allowRedirects
        [Obsolete("Eureka client does not support this feature, will be removed in next release")]
        public bool AllowRedirects { get; set; } = false;

        // Configuration property: eureka:client:shouldDisableDelta
        public bool ShouldDisableDelta { get; set; }

        // Configuration property: eureka:client:shouldFilterOnlyUpInstances
        public bool ShouldFilterOnlyUpInstances { get; set; }

        // Configuration property: eureka:client:shouldFetchRegistry
        public bool ShouldFetchRegistry { get; set; }

        // Configuration property: eureka:client:registryRefreshSingleVipAddress
        public string RegistryRefreshSingleVipAddress { get; set; }

        // Configuration property: eureka:client:shouldOnDemandUpdateStatusChange
        public bool ShouldOnDemandUpdateStatusChange { get; set; }

        // Configuration property: eureka:client:enabled
        public bool Enabled { get; set; } = true;

        // Configuration property: eureka:client:healthCheckEnabled
        public bool HealthCheckEnabled { get; set; }

        public string EurekaServerServiceUrls { get; set; }

        public int EurekaServerConnectTimeoutSeconds { get; set; }

        public int EurekaServerRetryCount { get; set; }

        public string ProxyHost { get; set; }

        public int ProxyPort { get; set; }

        public string ProxyUserName { get; set; }

        public string ProxyPassword { get; set; }

        public bool ShouldGZipContent { get; set; }

        public bool ValidateCertificates { get; set; }

        public bool HealthContribEnabled { get; set; }

        public string HealthMonitoredApps { get; set; }
    }
}
