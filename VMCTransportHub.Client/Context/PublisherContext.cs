using System;
using System.Collections.Generic;
using System.Collections.Generic.Extension;
using VMCTransportBridge;
using VMCTransportBridge.Serialization;

namespace VMCTransportHub.Client
{
    public sealed class PublisherContext : IDisposable
    {
        public bool TransportIsConnected => (_transport != null && _transport.IsConnected);
        public bool MessageReceiverIsRunning => _messageReceiver.IsRunning;
        public ushort VMCSourcePort => _messageReceiver.Port;

        public uint MessageCount => _messageCount;
        public uint PublishedMessageCount => _publishedMessageCount;
        public bool MessageLoggingIsEnabled => _messageLoggingIsEnabled;
        public FixedSizeQueue<MessageLog> MessageLogs => _messageLogs;

        private readonly IMessageSerializer _messageSerializer;
        private readonly IMessageReceiver _messageReceiver;

        private readonly FixedSizeQueue<MessageLog> _messageLogs = new FixedSizeQueue<MessageLog>(4096);

        private ITransport _transport;
        private Publisher _publisher;

        private uint _messageCount;
        private uint _publishedMessageCount;
        private bool _messageLoggingIsEnabled;

        public PublisherContext(IMessageSerializer messageSerializer, IMessageReceiver messageReceiver)
        {
            _messageSerializer = messageSerializer;
            _messageReceiver = messageReceiver;

            _messageReceiver.OnReceivePerformerAppStatus += OnReceivePerformerAppStatus;
            _messageReceiver.OnReceiveLocalVrm += OnReceiveLocalVrm;
            _messageReceiver.OnReceiveRemoteVrm += OnReceiveRemoteVrm;
            _messageReceiver.OnReceiveTime += OnReceiveTime;
            _messageReceiver.OnReceiveRootTransform += OnReceiveRootTransform;
            _messageReceiver.OnReceiveBoneTransform += OnReceiveBoneTransform;
            _messageReceiver.OnReceiveBlendShapeProxyValue += OnReceiveBlendShapeProxyValue;
            _messageReceiver.OnReceiveBlendShapeProxyApply += OnReceiveBlendShapeProxyApply;
            _messageReceiver.OnReceiveCamera += OnReceiveCamera;
            _messageReceiver.OnReceiveLight += OnReceiveLight;
            _messageReceiver.OnReceiveControllerInput += OnReceiveControllerInput;
            _messageReceiver.OnReceiveKeyInput += OnReceiveKeyInput;
            _messageReceiver.OnReceiveDeviceTransform += OnReceiveDeviceTransform;
            _messageReceiver.OnReceiveDeviceLocalTransform += OnReceiveDeviceLocalTransform;
        }

        public void Dispose()
        {           
            _publisher?.Dispose();
            _publisher = null;
            
            _messageReceiver.OnReceivePerformerAppStatus -= OnReceivePerformerAppStatus;
            _messageReceiver.OnReceiveLocalVrm -= OnReceiveLocalVrm;
            _messageReceiver.OnReceiveRemoteVrm -= OnReceiveRemoteVrm;
            _messageReceiver.OnReceiveTime -= OnReceiveTime;
            _messageReceiver.OnReceiveRootTransform -= OnReceiveRootTransform;
            _messageReceiver.OnReceiveBoneTransform -= OnReceiveBoneTransform;
            _messageReceiver.OnReceiveBlendShapeProxyValue -= OnReceiveBlendShapeProxyValue;
            _messageReceiver.OnReceiveBlendShapeProxyApply -= OnReceiveBlendShapeProxyApply;
            _messageReceiver.OnReceiveCamera -= OnReceiveCamera;
            _messageReceiver.OnReceiveLight -= OnReceiveLight;
            _messageReceiver.OnReceiveControllerInput -= OnReceiveControllerInput;
            _messageReceiver.OnReceiveKeyInput -= OnReceiveKeyInput;
            _messageReceiver.OnReceiveDeviceTransform -= OnReceiveDeviceTransform;
            _messageReceiver.OnReceiveDeviceLocalTransform -= OnReceiveDeviceLocalTransform;
        }

        public void AddTransport(ITransport transport)
        {
            _transport = transport;
            _publisher = new Publisher(_transport, _messageSerializer, _messageReceiver);
            _publisher.OnSendMessage += OnPublish;
        }

        public void RemoveTransport()
        {
            _publisher.OnSendMessage -= OnPublish;
            _publisher?.Dispose();
            _publisher = null;
            _transport = null;
        }

        public void StartMessageReceiver(ushort port)
        {
            _messageCount = 0;
            _messageReceiver.Start(port);
        }

        public void StopMessageReceiver()
        {
            _messageReceiver.Stop();
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

        private void OnPublish(int messageId, ArraySegment<byte> serializedMessage)
        {
            _publishedMessageCount++;
        }

        private void OnReceivePerformerAppStatus(PerformerAppStatus value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.PerformerAppStatus));
                }
            }
        }

        private void OnReceiveLocalVrm(LocalVrm value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.LocalVrm));
                }
            }
        }

        private void OnReceiveRemoteVrm(RemoteVrm value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.RemoteVrm));
                }
            }
        }

        private void OnReceiveTime(Time value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.Time));
                }
            }
        }

        private void OnReceiveRootTransform(RootTransform value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.RootTransform));
                }
            }
        }

        private void OnReceiveBoneTransform(BoneTransform value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    var messageDataString = $"{value.Name}, {value.PositionX}, {value.PositionY}, {value.PositionZ}";
                    _messageLogs.Enqueue(new MessageLog(OscAddress.BoneTransform, messageDataString));
                }
            }
        }

        private void OnReceiveBlendShapeProxyValue(BlendShapeProxyValue value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.BlendShapeProxyValue));
                }
            }
        }

        private void OnReceiveBlendShapeProxyApply(BlendShapeProxyApply value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.BlendShapeProxyApply));
                }
            }
        }

        private void OnReceiveCamera(Camera value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.Camera));
                }
            }
        }

        private void OnReceiveLight(Light value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.Light));
                }
            }
        }

        private void OnReceiveControllerInput(ControllerInput value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.ControllerInput));
                }
            }
        }

        private void OnReceiveKeyInput(KeyInput value)
        {
            if (TransportIsConnected)
            {
                _messageCount++;
                if (_messageLoggingIsEnabled)
                {
                    _messageLogs.Enqueue(new MessageLog(OscAddress.KeyInput));
                }
            }
        }

        private void OnReceiveDeviceTransform(DeviceTransform value)
        {
            if (TransportIsConnected)
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

                    var messageDataString = $"{value.DeviceType}, {value.Serial}, {value.PositionX}, {value.PositionY}, {value.PositionZ}";
                    _messageLogs.Enqueue(new MessageLog(messageType, messageDataString));
                }
            }
        }

        private void OnReceiveDeviceLocalTransform(DeviceLocalTransform value)
        {
            if (TransportIsConnected)
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
                    _messageLogs.Enqueue(new MessageLog(messageType));
                }
            }
        }
    }

    public class MessageLog
    {
        public DateTime ProcessedDateTime;
        public string MessageType = "";
        public string MessageDataString = "";

        public MessageLog(string messageType)
        {
            ProcessedDateTime = DateTime.Now;
            MessageType = messageType;
        }

        public MessageLog(string messageType, string messageDataString)
        {
            ProcessedDateTime = DateTime.Now;
            MessageType = messageType;
            MessageDataString = messageDataString;
        }
    }
}
