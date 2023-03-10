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

using Consul;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.CloudFoundry.Connector;
using Steeltoe.Common.Discovery;
using Steeltoe.Common.HealthChecks;
using Steeltoe.Discovery.Consul.Discovery;
using Steeltoe.Discovery.Consul.Registry;
using Steeltoe.Discovery.Eureka;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.IO;
using System.Net.Http;
using Xunit;

namespace Steeltoe.Discovery.Client.Test
{
    public class DiscoveryServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddDiscoveryClient_ThrowsIfServiceCollectionNull()
        {
            // Arrange
            IServiceCollection services = null;
            IConfigurationRoot config = null;
            DiscoveryOptions options = null;
            Action<DiscoveryOptions> action = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, config));
            Assert.Contains(nameof(services), ex.Message);
            ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, options));
            Assert.Contains(nameof(services), ex.Message);
            ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, action));
            Assert.Contains(nameof(services), ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_ThrowsIfConfigurtionNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, config));
            Assert.Contains(nameof(config), ex.Message);
        }

        [Fact]
        public void AddDiscoverClient_ThrowsIfDiscoveryOptionsNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            DiscoveryOptions discoveryOptions = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, discoveryOptions));
            Assert.Contains(nameof(discoveryOptions), ex.Message);
        }

        [Fact]
        public void AddDiscoverClient_ThrowsIfDiscoveryOptionsClientType_Unknown()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            DiscoveryOptions discoveryOptions = new DiscoveryOptions();

            // Act and Assert
            var ex = Assert.Throws<ArgumentException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, discoveryOptions));
            Assert.Contains("UNKNOWN", ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_ThrowsIfSetupOptionsNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            Action<DiscoveryOptions> setupOptions = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, setupOptions));
            Assert.Contains(nameof(setupOptions), ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_WithEurekaConfig_AddsDiscoveryClient()
        {
            // Arrange
            var appsettings = @"
{
    'spring': {
        'application': {
            'name': 'myName'
        },
    },
    'eureka': {
        'client': {
            'shouldFetchRegistry': false,
            'shouldRegisterWithEureka': false,
            'serviceUrl': 'http://localhost:8761/eureka/'
        }
    }
}";

            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient(config);

            var service = services.BuildServiceProvider().GetService<IDiscoveryClient>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddDiscoveryClient_WithNoEurekaConfig_AddsDiscoveryClient_UnknownClientType()
        {
            // Arrange
            var appsettings = @"
{
    'spring': {
        'application': {
            'name': 'myName'
        },
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            var services = new ServiceCollection();
            Assert.Throws<ArgumentException>(() => services.AddDiscoveryClient(config));
        }

        [Fact]
        public void AddDiscoveryClient_WithDiscoveryOptions_AddsDiscoveryClient()
        {
            // Arrange
            DiscoveryOptions options = new DiscoveryOptions()
            {
                ClientType = DiscoveryClientType.EUREKA,
                ClientOptions = new EurekaClientOptions()
                {
                    ShouldFetchRegistry = false,
                    ShouldRegisterWithEureka = false
                }
            };

            var services = new ServiceCollection();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient(options);

            var service = services.BuildServiceProvider().GetService<IDiscoveryClient>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddDiscoveryClient_AddsEurekaServerHealthContributor()
        {
            // Arrange
            DiscoveryOptions options = new DiscoveryOptions()
            {
                ClientType = DiscoveryClientType.EUREKA,
                ClientOptions = new EurekaClientOptions()
                {
                    ShouldFetchRegistry = false,
                    ShouldRegisterWithEureka = false
                }
            };

            var services = new ServiceCollection();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient(options);

            var built = services.BuildServiceProvider();
            var service = built.GetService<IDiscoveryClient>();
            Assert.NotNull(service);
            var healthContrib = built.GetService<IHealthContributor>();
            Assert.IsType<EurekaServerHealthContributor>(healthContrib);
        }

        [Fact]
        public void AddDiscoveryClient_WiresUp_HealthCheckerHandler()
        {
            // Arrange
            DiscoveryOptions options = new DiscoveryOptions()
            {
                ClientType = DiscoveryClientType.EUREKA,
                ClientOptions = new EurekaClientOptions()
                {
                    ShouldFetchRegistry = false,
                    ShouldRegisterWithEureka = false
                }
            };

            var services = new ServiceCollection();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient(options);
            services.AddSingleton<IHealthCheckHandler, ScopedEurekaHealthCheckHandler>();

            var built = services.BuildServiceProvider();
            var service = built.GetService<IDiscoveryClient>();
            Assert.NotNull(service);
            var eurekaService = service as EurekaDiscoveryClient;
            Assert.IsType<ScopedEurekaHealthCheckHandler>(eurekaService.HealthCheckHandler);
            ScopedEurekaHealthCheckHandler handler = eurekaService.HealthCheckHandler as ScopedEurekaHealthCheckHandler;
            Assert.NotNull(handler._scopeFactory);
        }

        [Fact]
        public void AddDiscoveryClient_WithDiscoveryOptions_MissingOptions_Throws()
        {
            // Arrange
            DiscoveryOptions options = new DiscoveryOptions()
            {
                ClientType = DiscoveryClientType.EUREKA,
                ClientOptions = null,
                RegistrationOptions = null
            };

            var services = new ServiceCollection();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            Assert.Throws<ArgumentException>(() => services.AddDiscoveryClient(options));
        }

        [Fact]
        public void AddDiscoveryClient_WithSetupAction_AddsDiscoveryClient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient((options) =>
           {
               options.ClientType = DiscoveryClientType.EUREKA;
               options.ClientOptions = new EurekaClientOptions()
               {
                   ShouldFetchRegistry = false,
                   ShouldRegisterWithEureka = false
               };
               options.RegistrationOptions = new EurekaInstanceOptions();
           });

            var service = services.BuildServiceProvider().GetService<IDiscoveryClient>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddDiscoveryClient_WithDiscoveryOptionsAndHttpClient_AddsDiscoveryClient()
        {
            // Arrange
            DiscoveryOptions options = new DiscoveryOptions()
            {
                ClientType = DiscoveryClientType.EUREKA,
                ClientOptions = new EurekaClientOptions()
                {
                    ShouldFetchRegistry = true,
                    ShouldRegisterWithEureka = false
                }
            };

            var services = new ServiceCollection();

            // Add provider to be injected into HttpClient
            var hprovider = new TestClientHandlerProvider();
            services.AddSingleton<IEurekaDiscoveryClientHandlerProvider>(hprovider);

            services.AddSingleton<IApplicationLifetime>(new TestApplicationLifetime());
            services.AddDiscoveryClient(options);

            var service = services.BuildServiceProvider().GetService<IDiscoveryClient>();
            Assert.NotNull(service);

            // Provider should have been called
            Assert.True(hprovider.Called);
        }

        [Fact]
        public void AddDiscoveryClient_ThrowsIfServiceNameNull()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = null;
            string serviceName = null;

            // Act and Assert
            var ex = Assert.Throws<ArgumentNullException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, config, serviceName));
            Assert.Contains(nameof(serviceName), ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_WithServiceName_NoVCAPs_ThrowsConnectorException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IConfigurationRoot config = new ConfigurationBuilder().Build();

            // Act and Assert
            var ex = Assert.Throws<ConnectorException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, config, "foobar"));
            Assert.Contains("foobar", ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_MultipleRegistryServices_ThrowsConnectorException()
        {
            // Arrange
            var env1 = @"
{
      'limits': {
        'fds': 16384,
        'mem': 1024,
        'disk': 1024
      },
      'application_name': 'spring-cloud-broker',
      'application_uris': [
        'spring-cloud-broker.apps.testcloud.com'
      ],
      'name': 'spring-cloud-broker',
      'space_name': 'p-spring-cloud-services',
      'space_id': '65b73473-94cc-4640-b462-7ad52838b4ae',
      'uris': [
        'spring-cloud-broker.apps.testcloud.com'
      ],
      'users': null,
      'version': '07e112f7-2f71-4f5a-8a34-db51dbed30a3',
      'application_version': '07e112f7-2f71-4f5a-8a34-db51dbed30a3',
      'application_id': '798c2495-fe75-49b1-88da-b81197f2bf06'
    }
}";
            var env2 = @"
{
      'p-service-registry': [
{
    'credentials': {
        'uri': 'https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com',
        'client_id': 'p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe',
        'client_secret': 'dCsdoiuklicS',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
        },
    'syslog_drain_url': null,
    'label': 'p-service-registry',
    'provider': null,
    'plan': 'standard',
    'name': 'myDiscoveryService',
    'tags': [
    'eureka',
    'discovery',
    'registry',
    'spring-cloud'
    ]
},
{
    'credentials': {
        'uri': 'https://eureka-6a1b81f5-79e2-4d14-a86b-ddf584635a60.apps.testcloud.com',
        'client_id': 'p-service-registry-06e28efd-24be-4ce3-9784-854ed8d2acbe',
        'client_secret': 'dCsdoiuklicS',
        'access_token_uri': 'https://p-spring-cloud-services.uaa.system.testcloud.com/oauth/token'
        },
    'syslog_drain_url': null,
    'label': 'p-service-registry',
    'provider': null,
    'plan': 'standard',
    'name': 'myDiscoveryService2',
    'tags': [
    'eureka',
    'discovery',
    'registry',
    'spring-cloud'
    ]
}
]
}
";

            // Arrange
            IServiceCollection services = new ServiceCollection();

            Environment.SetEnvironmentVariable("VCAP_APPLICATION", env1);
            Environment.SetEnvironmentVariable("VCAP_SERVICES", env2);

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddCloudFoundry();
            var config = builder.Build();

            // Act and Assert
            var ex = Assert.Throws<ConnectorException>(() => DiscoveryServiceCollectionExtensions.AddDiscoveryClient(services, config));
            Assert.Contains("Multiple", ex.Message);
        }

        [Fact]
        public void AddDiscoveryClient_WithConsulConfiguration_AddsDiscoveryClient()
        {
            // Arrange
            var appsettings = @"
{
    'spring': {
        'application': {
            'name': 'myName'
        },
    },
    'consul': {
        'host': 'foo.bar',
        'discovery': {
            'register': false,
            'deregister': false,
            'instanceid': 'instanceid',
            'port': 1234
        }
    }
}";

            var path = TestHelpers.CreateTempFile(appsettings);
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);

            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddDiscoveryClient(config);
            var provider = services.BuildServiceProvider();

            var service = provider.GetService<IDiscoveryClient>();
            Assert.NotNull(service);
            var service1 = provider.GetService<IConsulClient>();
            Assert.NotNull(service1);
            var service2 = provider.GetService<IScheduler>();
            Assert.NotNull(service2);
            var service3 = provider.GetService<IConsulServiceRegistry>();
            Assert.NotNull(service3);
            var service4 = provider.GetService<IConsulRegistration>();
            Assert.NotNull(service4);
            var service5 = provider.GetService<IConsulServiceRegistrar>();
            Assert.NotNull(service5);
            var service6 = provider.GetService<IHealthContributor>();
            Assert.NotNull(service6);
        }

        public class TestClientHandlerProvider : IEurekaDiscoveryClientHandlerProvider
        {
            public bool Called { get; set; } = false;

            public HttpClientHandler GetHttpClientHandler()
            {
                Called = true;
                return new System.Net.Http.HttpClientHandler();
            }
        }
    }
}
