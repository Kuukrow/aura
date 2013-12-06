﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Channel.Database;
using Aura.Channel.Scripting;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;

namespace Aura.Channel.Network
{
	public class ChannelClient : Client
	{
		public Account Account { get; set; }

		//public PlayerCreature Character { get; set; }
		public Dictionary<long, Creature> Creatures { get; protected set; }

		public NpcSession NpcSession { get; set; }

		public ChannelClient()
		{
			this.Creatures = new Dictionary<long, Creature>();
			this.NpcSession = new NpcSession();
		}

		public Creature GetCreature(long id)
		{
			Creature creature;
			this.Creatures.TryGetValue(id, out creature);
			return creature;
		}

		public PlayerCreature GetPlayerCreature(long id)
		{
			return this.GetCreature(id) as PlayerCreature;
		}
	}

	/// <summary>
	/// Dummy client for creatures, so we don't have to care about who is
	/// actually able to receive data.
	/// </summary>
	public class DummyClient : ChannelClient
	{
		public override void Send(byte[] buffer)
		{ }

		public override void Send(Packet packet)
		{ }

		public override void Kill()
		{ }
	}
}