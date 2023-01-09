using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Markdig;
using VMCTransportBridge;
using VMCTransportBridge.Transports.Grpc.Client;
using VMCTransportBridge.Transports.PhotonRealtime;

namespace VMCTransportHub.Client
{
    public sealed class ApplicationContext : IDisposable
    {
        public static readonly string ConfigurationFileName = "appsettings.json";
        public static readonly string LicenseFileName = "License.md";
        public static readonly string ThirdPartyNoticesFileName = "ThirdPartyNotices.md";

        public int SelectedTransportIndex { get; set; } = -1;

        public List<ITransport> Transports => _transports;
        public List<string> TransportNames => _transportNames;

        private readonly List<ITransport> _transports = new List<ITransport>();
        private readonly List<string> _transportNames = new List<string>();

        private string _licenceHtml;
        private string _thirdPartyNoticesHtml;

        public ApplicationContext
        (
            GrpcTransport grpcTransport,
            PhotonRealtimeTransport photonRealtimeTransport
        )
        {
            _transports.Add(grpcTransport);
            _transportNames.Add("gRPC");

            _transports.Add(photonRealtimeTransport);
            _transportNames.Add("Photon Realtime");
        }

        public void Dispose()
        {
        }

        public void OpenConfigurationFile()
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(location);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"{directory}\\{ConfigurationFileName}");
            }
        }

        public async Task<string> GetLicenseHtmlAsync()
        {
            if (string.IsNullOrEmpty(_licenceHtml))
            {
                var markdownPipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
                var text = await System.IO.File.ReadAllTextAsync(LicenseFileName);
                _licenceHtml = Markdown.ToHtml(text, markdownPipeline);
            }
            return _licenceHtml;
        }

        public async Task<string> GetThirdPartyNoticesHtmlAsync()
        {
            if (string.IsNullOrEmpty(_thirdPartyNoticesHtml))
            {
                var markdownPipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
                var text = await System.IO.File.ReadAllTextAsync(ThirdPartyNoticesFileName);
                _thirdPartyNoticesHtml = Markdown.ToHtml(text, markdownPipeline);
            }
            return _thirdPartyNoticesHtml;
        }
    }
}