﻿//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Steeltoe.Common.Discovery;


namespace Steeltoe.Discovery.Eureka
{

    public class EurekaClientOptions : EurekaClientConfig, IDiscoveryClientOptions
    {
        public const string EUREKA_CLIENT_CONFIGURATION_PREFIX = "eureka:client";

        public new const int Default_InstanceInfoReplicationIntervalSeconds = 30;

        public EurekaClientOptions()
        {
            InstanceInfoReplicationIntervalSeconds = Default_InstanceInfoReplicationIntervalSeconds;
            EurekaServer = new EurekaServerConfig(this);
        }

        ///  Configuration property: eureka:client:accessTokenUri
        public string AccessTokenUri { get; set; }
        ///  Configuration property: eureka:client:clientSecret
        public string ClientSecret { get; set; }
        ///  Configuration property: eureka:client:clientId
        public string ClientId { get; set; }
        ///  Configuration property: eureka:client:serviceUrl
        public string ServiceUrl
        {
            get
            {
                return this.EurekaServerServiceUrls;
            }
            set
            {
                this.EurekaServerServiceUrls = value;
            }
        }
        /// Configuration property: eureka:client:validate_certificates
        public bool Validate_Certificates
        {
            get
            {
                return this.ValidateCertificates;
            }
            set
            {
                this.ValidateCertificates = value;
            }
        }
        /// Configuration property: eureka:client:eurekaServer
        public EurekaServerConfig EurekaServer { get; set; }
        public class EurekaServerConfig
        {
            EurekaClientOptions _options;
            public EurekaServerConfig(EurekaClientOptions options)
            {
                _options = options;
            }

            /// <summary>
            /// Configuration property: eureka:client:eurekaServer:proxyHost
            /// </summary>
            public string ProxyHost
            {
                get
                {
                    return _options.ProxyHost;
                }
                set
                {
                    _options.ProxyHost = value;
                }
            }
            /// <summary>
            /// Configuration property: eureka:client:eurekaServer:proxyPort
            /// </summary>
            public int ProxyPort
            {
                get
                {
                    return _options.ProxyPort;
                }
                set
                {
                    _options.ProxyPort = value;
                }
            }
            /// <summary>
            ///  Configuration property: eureka:client:eurekaServer:proxyUserName
            /// </summary>
            public string ProxyUserName
            {
                get
                {
                    return _options.ProxyUserName;
                }
                set
                {
                    _options.ProxyUserName = value;
                }
            }
            /// <summary>
            ///  Configuration property: eureka:client:eurekaServer:proxyPassword
            /// </summary>
            public string ProxyPassword
            {
                get
                {
                    return _options.ProxyPassword;
                }
                set
                {
                    _options.ProxyPassword = value;
                }
            }
            /// <summary>
            ///  Configuration property: eureka:client:eurekaServer:shouldGZipContent
            /// </summary>
            public bool ShouldGZipContent
            {
                get
                {
                    return _options.ShouldGZipContent;
                }
                set
                {
                    _options.ShouldGZipContent = value;
                }
            }
            /// <summary>
            ///  Configuration property: eureka:client:eurekaServer:connectTimeoutSeconds
            /// </summary>
            public int ConnectTimeoutSeconds
            {
                get
                {
                    return _options.EurekaServerConnectTimeoutSeconds;
                }
                set
                {
                    _options.EurekaServerConnectTimeoutSeconds = value;
                }
            }


        }

    }
}
