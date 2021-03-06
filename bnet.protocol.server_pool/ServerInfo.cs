using bnet.protocol.attribute;
using System;
using System.Collections.Generic;
using System.IO;

namespace bnet.protocol.server_pool
{
	public class ServerInfo : IProtoBuf
	{
		public bool HasReplace;

		private bool _Replace;

		public bool HasState;

		private ServerState _State;

		private List<Attribute> _Attribute = new List<Attribute>();

		public bool HasProgramId;

		private uint _ProgramId;

		public ProcessId Host
		{
			get;
			set;
		}

		public bool Replace
		{
			get
			{
				return this._Replace;
			}
			set
			{
				this._Replace = value;
				this.HasReplace = true;
			}
		}

		public ServerState State
		{
			get
			{
				return this._State;
			}
			set
			{
				this._State = value;
				this.HasState = (value != null);
			}
		}

		public List<Attribute> Attribute
		{
			get
			{
				return this._Attribute;
			}
			set
			{
				this._Attribute = value;
			}
		}

		public List<Attribute> AttributeList
		{
			get
			{
				return this._Attribute;
			}
		}

		public int AttributeCount
		{
			get
			{
				return this._Attribute.get_Count();
			}
		}

		public uint ProgramId
		{
			get
			{
				return this._ProgramId;
			}
			set
			{
				this._ProgramId = value;
				this.HasProgramId = true;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return true;
			}
		}

		public void Deserialize(Stream stream)
		{
			ServerInfo.Deserialize(stream, this);
		}

		public static ServerInfo Deserialize(Stream stream, ServerInfo instance)
		{
			return ServerInfo.Deserialize(stream, instance, -1L);
		}

		public static ServerInfo DeserializeLengthDelimited(Stream stream)
		{
			ServerInfo serverInfo = new ServerInfo();
			ServerInfo.DeserializeLengthDelimited(stream, serverInfo);
			return serverInfo;
		}

		public static ServerInfo DeserializeLengthDelimited(Stream stream, ServerInfo instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return ServerInfo.Deserialize(stream, instance, num);
		}

		public static ServerInfo Deserialize(Stream stream, ServerInfo instance, long limit)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			instance.Replace = false;
			if (instance.Attribute == null)
			{
				instance.Attribute = new List<Attribute>();
			}
			while (limit < 0L || stream.get_Position() < limit)
			{
				int num = stream.ReadByte();
				if (num == -1)
				{
					if (limit >= 0L)
					{
						throw new EndOfStreamException();
					}
					return instance;
				}
				else
				{
					int num2 = num;
					if (num2 != 10)
					{
						if (num2 != 16)
						{
							if (num2 != 26)
							{
								if (num2 != 34)
								{
									if (num2 != 45)
									{
										Key key = ProtocolParser.ReadKey((byte)num, stream);
										uint field = key.Field;
										if (field == 0u)
										{
											throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
										}
										ProtocolParser.SkipKey(stream, key);
									}
									else
									{
										instance.ProgramId = binaryReader.ReadUInt32();
									}
								}
								else
								{
									instance.Attribute.Add(bnet.protocol.attribute.Attribute.DeserializeLengthDelimited(stream));
								}
							}
							else if (instance.State == null)
							{
								instance.State = ServerState.DeserializeLengthDelimited(stream);
							}
							else
							{
								ServerState.DeserializeLengthDelimited(stream, instance.State);
							}
						}
						else
						{
							instance.Replace = ProtocolParser.ReadBool(stream);
						}
					}
					else if (instance.Host == null)
					{
						instance.Host = ProcessId.DeserializeLengthDelimited(stream);
					}
					else
					{
						ProcessId.DeserializeLengthDelimited(stream, instance.Host);
					}
				}
			}
			if (stream.get_Position() == limit)
			{
				return instance;
			}
			throw new ProtocolBufferException("Read past max limit");
		}

		public void Serialize(Stream stream)
		{
			ServerInfo.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, ServerInfo instance)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			if (instance.Host == null)
			{
				throw new ArgumentNullException("Host", "Required by proto specification.");
			}
			stream.WriteByte(10);
			ProtocolParser.WriteUInt32(stream, instance.Host.GetSerializedSize());
			ProcessId.Serialize(stream, instance.Host);
			if (instance.HasReplace)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteBool(stream, instance.Replace);
			}
			if (instance.HasState)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteUInt32(stream, instance.State.GetSerializedSize());
				ServerState.Serialize(stream, instance.State);
			}
			if (instance.Attribute.get_Count() > 0)
			{
				using (List<Attribute>.Enumerator enumerator = instance.Attribute.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Attribute current = enumerator.get_Current();
						stream.WriteByte(34);
						ProtocolParser.WriteUInt32(stream, current.GetSerializedSize());
						bnet.protocol.attribute.Attribute.Serialize(stream, current);
					}
				}
			}
			if (instance.HasProgramId)
			{
				stream.WriteByte(45);
				binaryWriter.Write(instance.ProgramId);
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			uint serializedSize = this.Host.GetSerializedSize();
			num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
			if (this.HasReplace)
			{
				num += 1u;
				num += 1u;
			}
			if (this.HasState)
			{
				num += 1u;
				uint serializedSize2 = this.State.GetSerializedSize();
				num += serializedSize2 + ProtocolParser.SizeOfUInt32(serializedSize2);
			}
			if (this.Attribute.get_Count() > 0)
			{
				using (List<Attribute>.Enumerator enumerator = this.Attribute.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Attribute current = enumerator.get_Current();
						num += 1u;
						uint serializedSize3 = current.GetSerializedSize();
						num += serializedSize3 + ProtocolParser.SizeOfUInt32(serializedSize3);
					}
				}
			}
			if (this.HasProgramId)
			{
				num += 1u;
				num += 4u;
			}
			num += 1u;
			return num;
		}

		public void SetHost(ProcessId val)
		{
			this.Host = val;
		}

		public void SetReplace(bool val)
		{
			this.Replace = val;
		}

		public void SetState(ServerState val)
		{
			this.State = val;
		}

		public void AddAttribute(Attribute val)
		{
			this._Attribute.Add(val);
		}

		public void ClearAttribute()
		{
			this._Attribute.Clear();
		}

		public void SetAttribute(List<Attribute> val)
		{
			this.Attribute = val;
		}

		public void SetProgramId(uint val)
		{
			this.ProgramId = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			num ^= this.Host.GetHashCode();
			if (this.HasReplace)
			{
				num ^= this.Replace.GetHashCode();
			}
			if (this.HasState)
			{
				num ^= this.State.GetHashCode();
			}
			using (List<Attribute>.Enumerator enumerator = this.Attribute.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Attribute current = enumerator.get_Current();
					num ^= current.GetHashCode();
				}
			}
			if (this.HasProgramId)
			{
				num ^= this.ProgramId.GetHashCode();
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			ServerInfo serverInfo = obj as ServerInfo;
			if (serverInfo == null)
			{
				return false;
			}
			if (!this.Host.Equals(serverInfo.Host))
			{
				return false;
			}
			if (this.HasReplace != serverInfo.HasReplace || (this.HasReplace && !this.Replace.Equals(serverInfo.Replace)))
			{
				return false;
			}
			if (this.HasState != serverInfo.HasState || (this.HasState && !this.State.Equals(serverInfo.State)))
			{
				return false;
			}
			if (this.Attribute.get_Count() != serverInfo.Attribute.get_Count())
			{
				return false;
			}
			for (int i = 0; i < this.Attribute.get_Count(); i++)
			{
				if (!this.Attribute.get_Item(i).Equals(serverInfo.Attribute.get_Item(i)))
				{
					return false;
				}
			}
			return this.HasProgramId == serverInfo.HasProgramId && (!this.HasProgramId || this.ProgramId.Equals(serverInfo.ProgramId));
		}

		public static ServerInfo ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<ServerInfo>(bs, 0, -1);
		}
	}
}
