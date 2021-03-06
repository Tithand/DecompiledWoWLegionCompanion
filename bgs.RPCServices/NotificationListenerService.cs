using bnet.protocol.notification;
using System;

namespace bgs.RPCServices
{
	public class NotificationListenerService : ServiceDescriptor
	{
		public const uint ON_NOTIFICATION_REC_ID = 1u;

		public NotificationListenerService() : base("bnet.protocol.notification.NotificationListener")
		{
			this.Methods = new MethodDescriptor[2];
			this.Methods[(int)((UIntPtr)1)] = new MethodDescriptor("bnet.protocol.notification.NotificationListener.OnNotificationReceived", 1u, new MethodDescriptor.ParseMethod(ProtobufUtil.ParseFromGeneric<Notification>));
		}
	}
}
