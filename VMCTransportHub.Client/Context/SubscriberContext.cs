using System;
using System.Collections.Generic;
using System.Collections.Generic.Extension;
using VMCTransportBridge;
using VMCTransportBridge.Serialization;

namespace VMCTransportHub.Client
{
    public sealed class SubscriberContext : IDisposable
    {
        public bool TransportIsConnected => (_transport != null && _transport.IsConnected);
        public bool MessageSenderIsRunning => _messageSender.IsRunning;
        public string VMCDestinationAddress => _messageSender.DestinationAddress;
        public ushort VMCDestinationPort => _messageSender.DestinationPort;

        public bool MessageFilterIsEnabled => (_subscriber != null && _subscriber.MessageFilterIsEnabled);
        public int MessageFilterClientId => (_subscriber != null) ? _subscriber.MessageFilterClientId : -1;

        public uint MessageCount => _messageCount;
        public uint TransportedMessageCount => _transportedMessageCount;
        public bool MessageLoggingIsEnabled => _messageLoggingIsEnabled;
        public FixedSizeQueue<TransportedMessageLog> MessageLogs => _messageLogs;

        private readonly IMessageSerializer _messageSerializer;
        private readonly IMessageSender _messageSender;

        private readonly FixedSizeQueue<TransportedMessageLog> _messageLogs = new FixedSizeQueue<TransportedMessageLog>(4096);

        private ITransport _transport;
        private Subscriber _subscriber;

        private uint _messageCount;
        private uint _transportedMessageCount;
        private bool _messageLoggingIsEnabled;

        public SubscriberContext(IMessageSerializer messageSerializer, IMessageSender messageSender)
        {
            _messageSerializer = messageSerializer;
            _messageSender = messageSender;
            
            _messageSender.OnSendPerformerAppStatus += OnSendPerformerAppStatus;
            _messageSender.OnSendLocalVrm += OnSendLocalVrm;
            _messageSender.OnSendRemoteVrm += OnSendRemoteVrm;
            _messageSender.OnSendTime += OnSendTime;
            _messageSender.OnSendRootTransform += OnSendRootTransform;
            _messageSender.OnSendBoneTransform += OnSendBoneTransform;
            _messageSender.OnSendBlendShapeProxyValue += OnSendBlendShapeProxyValue;
            _messageSender.OnSendBlendShapeProxyApply += OnSendBlendShapeProxyApply;
            _messageSender.OnSendCamera += OnSendCamera;
            _messageSender.OnSendLight += OnSendLight;
            _messageSender.OnSendControllerInput += OnSendControllerInput;
            _messageSender.OnSendKeyInput += OnSendKeyInput;
            _messageSender.OnSendDeviceTransform += OnSendDeviceTransform;
            _messageSender.OnSendDeviceLocalTransform += OnSendDeviceLocalTransform;
            
            _messageSender.OnSendTransportedPerformerAppStatus += OnSendTransportedPerformerAppStatus;
            _messageSender.OnSendTransportedLocalVrm += OnSendTransportedLocalVrm;
            _messageSender.OnSendTransportedRemoteVrm += OnSendTransportedRemoteVrm;
            _messageSender.OnSendTransportedTime += OnSendTransportedTime;
            _messageSender.OnSendTransportedRootTransform += OnSendTransportedRootTransform;
            _messageSender.OnSendTransportedBoneTransform += OnSendTransportedBoneTransform;
            _messageSender.OnSendTransportedBlendShapeProxyValue += OnSendTransportedBlendShapeProxyValue;
            _messageSender.OnSendTransportedBlendShapeProxyApply += OnSendTransportedBlendShapeProxyApply;
            _messageSender.OnSendTransportedCamera += OnSendTransportedCamera;
            _messageSender.OnSendTransportedLight += OnSendTransportedLight;
            _messageSender.OnSendTransportedControllerInput += OnSendTransportedControllerInput;
            _messageSender.OnSendTransportedKeyInput += OnSendTransportedKeyInput;
            _messageSender.OnSendTransportedDeviceTransform += OnSendTransportedDeviceTransform;
            _messageSender.OnSendTransportedDeviceLocalTransform += OnSendTransportedDeviceLocalTransform;
        }

        public void Dispose()
        {
            _subscriber.Dispose();
            _subscriber = null;

            _messageSender.OnSendPerformerAppStatus -= OnSendPerformerAppStatus;
            _messageSender.OnSendLocalVrm -= OnSendLocalVrm;
            _messageSender.OnSendRemoteVrm -= OnSendRemoteVrm;
            _messageSender.OnSendTime -= OnSendTime;
            _messageSender.OnSendRootTransform -= OnSendRootTransform;
            _messageSender.OnSendBoneTransform -= OnSendBoneTransform;
            _messageSender.OnSendBlendShapeProxyValue -= OnSendBlendShapeProxyValue;
            _messageSender.OnSendBlendShapeProxyApply -= OnSendBlendShapeProxyApply;
            _messageSender.OnSendCamera -= OnSendCamera;
            _messageSender.OnSendLight -= OnSendLight;
            _messageSender.OnSendControllerInput -= OnSendControllerInput;
            _messageSender.OnSendKeyInput -= OnSendKeyInput;
            _messageSender.OnSendDeviceTransform -= OnSendDeviceTransform;
            _messageSender.OnSendDeviceLocalTransform -= OnSendDeviceLocalTransform;
            
            _messageSender.OnSendTransportedPerformerAppStatus -= OnSendTransportedPerformerAppStatus;
            _messageSender.OnSendTransportedLocalVrm -= OnSendTransportedLocalVrm;
            _messageSender.OnSendTransportedRemoteVrm -= OnSendTransportedRemoteVrm;
            _messageSender.OnSendTransportedTime -= OnSendTransportedTime;
            _messageSender.OnSendTransportedRootTransform -= OnSendTransportedRootTransform;
            _messageSender.OnSendTransportedBoneTransform -= OnSendTransportedBoneTransform;
            _messageSender.OnSendTransportedBlendShapeProxyValue -= OnSendTransportedBlendShapeProxyValue;
            _messageSender.OnSendTransportedBlendShapeProxyApply -= OnSendTransportedBlendShapeProxyApply;
            _messageSender.OnSendTransportedCamera -= OnSendTransportedCamera;
            _messageSender.OnSendTransportedLight -= OnSendTransportedLight;
            _messageSender.OnSendTransportedControllerInput -= OnSendTransportedControllerInput;
            _messageSender.OnSendTransportedKeyInput -= OnSendTransportedKeyInput;
            _messageSender.OnSendTransportedDeviceTransform -= OnSendTransportedDeviceTransform;
            _messageSender.OnSendTransportedDeviceLocalTransform -= OnSendTransportedDeviceLocalTransform;
        }

        public void AddTransport(ITransport transport)
        {
            _transport = transport;
            _subscriber = new Subscriber(_transport, _messageSerializer, _messageSender);
            _subscriber.OnReceiveTransportedMessage += OnReceiveTransportedMessage;
        }

        public void RemoveTransport()
        {
            _subscriber.OnReceiveTransportedMessage -= OnReceiveTransportedMessage;
            _subscriber.Dispose();
            _subscriber = null;
            _transport = null;
        }

        public void StartMessageSender(string destinationAddress, ushort destinationPort)
        {
            _messageCount = 0;
            _messageSender.Start(destinationAddress, destinationPort);
        }

        public void StopMessageSender()
        {
            _messageSender.Stop();
        }

        public void EnableMessageFilter(int networkClientId)
        {
            _subscriber?.EnableMessageFilter(networkClientId);
        }

        public void DisableMessageFilter()
        {
            _subscriber?.DisableMessageFilter();
        }

        public void EnableMessageLogging()
        {
            ClearMessageLogs();
            _messageLoggingIsEnabled = true;
        }

        public void DisableMessageLogging()
        {
            _messageLoggingIsEnabled = false;
        }

        public void ClearMessageLogs()
        {
            _messageLogs.Clear();
        }

        private void OnReceiveTransportedMessage(int messageId, int networkCliendId, ArraySegment<byte> serializedMessage)
        {
            _transportedMessageCount++;
        }
        
        private void OnSendPerformerAppStatus(PerformerAppStatus value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.PerformerAppStatus, _transport.ClientId));
            }
        }

        private void OnSendLocalVrm(LocalVrm value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.LocalVrm, _transport.ClientId));
            }
        }

        private void OnSendRemoteVrm(RemoteVrm value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.RemoteVrm, _transport.ClientId));
            }
        }

        private void OnSendTime(Time value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.Time, _transport.ClientId));
            }
        }

        private void OnSendRootTransform(RootTransform value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.RootTransform, _transport.ClientId));
            }
        }

        private void OnSendBoneTransform(BoneTransform value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.BoneTransform, _transport.ClientId));
            }
        }

        private void OnSendBlendShapeProxyValue(BlendShapeProxyValue value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.BlendShapeProxyValue, _transport.ClientId));
            }
        }

        private void OnSendBlendShapeProxyApply(BlendShapeProxyApply value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.BlendShapeProxyApply, _transport.ClientId));
            }
        }

        private void OnSendCamera(Camera value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.Camera, _transport.ClientId));
            }
        }

        private void OnSendLight(Light value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.Light, _transport.ClientId));
            }
        }

        private void OnSendControllerInput(ControllerInput value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.ControllerInput, _transport.ClientId));
            }
        }

        private void OnSendKeyInput(KeyInput value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.KeyInput, _transport.ClientId));
            }
        }

        private void OnSendDeviceTransform(DeviceTransform value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                var messageType = "DeviceTransform.Unknown";
                if (value.DeviceType is DeviceType.HeadMountedDisplay)
                {
                    messageType = OscAddress.HmdDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Controller)
                {
                    messageType = OscAddress.ControllerDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Tracker)
                {
                    messageType = OscAddress.TrackerDeviceTransform;
                }
                _messageLogs.Enqueue(new TransportedMessageLog(messageType, _transport.ClientId));
            }
        }

        private void OnSendDeviceLocalTransform(DeviceLocalTransform value)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                var messageType = "DeviceLocalTransform.Unknown";
                if (value.DeviceType is DeviceType.HeadMountedDisplay)
                {
                    messageType = OscAddress.HmdDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Controller)
                {
                    messageType = OscAddress.ControllerDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Tracker)
                {
                    messageType = OscAddress.TrackerDeviceTransform;
                }
                _messageLogs.Enqueue(new TransportedMessageLog(messageType, _transport.ClientId));
            }
        }
        
        
        private void OnSendTransportedPerformerAppStatus(PerformerAppStatus value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedPerformerAppStatus, _transport.ClientId));
            }
        }

        private void OnSendTransportedLocalVrm(LocalVrm value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedLocalVrm, _transport.ClientId));
            }
        }

        private void OnSendTransportedRemoteVrm(RemoteVrm value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedRemoteVrm, _transport.ClientId));
            }
        }

        private void OnSendTransportedTime(Time value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedTime, _transport.ClientId));
            }
        }

        private void OnSendTransportedRootTransform(RootTransform value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedRootTransform, _transport.ClientId));
            }
        }

        private void OnSendTransportedBoneTransform(BoneTransform value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                var messageDataString = $"{value.Name}, {value.PositionX}, {value.PositionY}, {value.PositionZ}";
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedBoneTransform, _transport.ClientId, messageDataString));
            }
        }

        private void OnSendTransportedBlendShapeProxyValue(BlendShapeProxyValue value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedBlendShapeProxyValue, _transport.ClientId));
            }
        }

        private void OnSendTransportedBlendShapeProxyApply(BlendShapeProxyApply value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedBlendShapeProxyApply, _transport.ClientId));
            }
        }

        private void OnSendTransportedCamera(Camera value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedCamera, _transport.ClientId));
            }
        }

        private void OnSendTransportedLight(Light value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedLight, _transport.ClientId));
            }
        }

        private void OnSendTransportedControllerInput(ControllerInput value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedControllerInput, _transport.ClientId));
            }
        }

        private void OnSendTransportedKeyInput(KeyInput value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                _messageLogs.Enqueue(new TransportedMessageLog(OscAddress.TransportedKeyInput, _transport.ClientId));
            }
        }

        private void OnSendTransportedDeviceTransform(DeviceTransform value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                var messageType = "Unknown";
                if (value.DeviceType is DeviceType.HeadMountedDisplay)
                {
                    messageType = OscAddress.TransportedHmdDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Controller)
                {
                    messageType = OscAddress.TransportedControllerDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Tracker)
                {
                    messageType = OscAddress.TransportedTrackerDeviceTransform;
                }

                var messageDataString = $"{value.DeviceType}, {value.Serial}, {value.PositionX}, {value.PositionY}, {value.PositionZ}";
                _messageLogs.Enqueue(new TransportedMessageLog(messageType, _transport.ClientId, messageDataString));
            }
        }

        private void OnSendTransportedDeviceLocalTransform(DeviceLocalTransform value, int networkClientId)
        {
            _messageCount++;
            if (_messageLoggingIsEnabled)
            {
                var messageType = "Unknown";
                if (value.DeviceType is DeviceType.HeadMountedDisplay)
                {
                    messageType = OscAddress.TransportedHmdDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Controller)
                {
                    messageType = OscAddress.TransportedControllerDeviceTransform;
                }
                else if (value.DeviceType is DeviceType.Tracker)
                {
                    messageType = OscAddress.TransportedTrackerDeviceTransform;
                }
                _messageLogs.Enqueue(new TransportedMessageLog(messageType, _transport.ClientId));
            }
        }
    }

    public class TransportedMessageLog
    {
        public DateTime ProcessedDateTime;
        public string MessageType = "";
        public string MessageDataString = "";
        public int TransportClientId = -1;

        public TransportedMessageLog(string messageType, int transportClientId)
        {
            ProcessedDateTime = DateTime.Now;
            MessageType = messageType;
            TransportClientId = transportClientId;
        }

        public TransportedMessageLog(string messageType, int transportClientId, string messageDataString)
        {
            ProcessedDateTime = DateTime.Now;
            MessageType = messageType;
            TransportClientId = transportClientId;
            MessageDataString = messageDataString;
        }
    }
}