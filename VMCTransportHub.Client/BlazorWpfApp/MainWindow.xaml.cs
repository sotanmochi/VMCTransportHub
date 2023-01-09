using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VMCTransportBridge;
using VMCTransportBridge.Serialization;
using VMCTransportBridge.Transports.Grpc.Client;
using VMCTransportBridge.Transports.PhotonRealtime;
using Grpc.Net.Client;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace VMCTransportHub.Client.BlazorWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configuration
            var configuration = new ConfigurationBuilder().AddJsonFile(ApplicationContext.ConfigurationFileName).Build();

            // Create Service Provider
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();

            // Context
            serviceCollection.AddSingleton<ApplicationContext>();
            serviceCollection.AddSingleton<PublisherContext>();
            serviceCollection.AddSingleton<SubscriberContext>();

            InitializeInfrastructure(serviceCollection, configuration);

            // Build Service Provider
            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }

        private void InitializeInfrastructure(ServiceCollection serviceCollection, IConfigurationRoot configuration)
        {
            // Message Receiver/Sender
            serviceCollection.AddSingleton<IMessageReceiver, OscMessageReceiver>(_ => new OscMessageReceiver(defaultPort: 39539));
            serviceCollection.AddSingleton<IMessageSender, OscMessageSender>(_ => new OscMessageSender(defaultPort: 39542));

            // Message Serializer
            var messageSerializer = new MessagePackMessageSerializer();
            serviceCollection.AddSingleton<IMessageSerializer>(messageSerializer);

            // Grpc Transport
            var grpcServerAddress = configuration["Transport:Grpc:ServerAddress"];
            var grpcTransport = new GrpcTransport(messageSerializer, GrpcChannel.ForAddress(grpcServerAddress));
            serviceCollection.AddSingleton<GrpcTransport>(grpcTransport);

            // Photon Realtime Transport
            var punVersion = configuration["Transport:PhotonRealtime:PunVersion"];
            var appVersion = configuration["Transport:PhotonRealtime:AppVersion"];
            var region = configuration["Transport:PhotonRealtime:Region"];
            var appId = configuration["Transport:PhotonRealtime:AppId"];
            
            var appSettings = new AppSettings()
            {
                AppVersion = appVersion,
                AppIdRealtime = appId,
                FixedRegion = region,
            };
            
            var photonRealtimeTransport = new PhotonRealtimeTransport
            (
                punVersion,
                new PhotonRealtimeConnectParameters(){ AppSettings = appSettings },
                new PhotonRealtimeJoinParameters(){ RoomName = "DefaultRoom" },
                ConnectionProtocol.Udp,
                ReceiverGroup.All
            );
            
            serviceCollection.AddSingleton<PhotonRealtimeTransport>(photonRealtimeTransport);
        }
    }
}
