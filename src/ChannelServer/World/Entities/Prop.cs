﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Mabi.Structs;
using Aura.Shared.Mabi.Const;
using System.Threading;
using Aura.Data;
using Aura.Channel.Network;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
	/// <remarks>
	/// Not all options are used in all props. Things like ExtraData, State,
	/// etc. are all very prop specific.
	/// </remarks>
	public class Prop : Entity
	{
		private static long _propId = MabiId.ServerProps;

		public override DataType DataType { get { return DataType.Prop; } }

		public PropInfo Info;

		/// <summary>
		/// True if this prop was spawned by the server.
		/// </summary>
		/// <remarks>
		/// *sigh* Yes, we're checking the id, happy now, devCAT? .__.
		/// </remarks>
		public bool ServerSide { get { return (this.EntityId >= MabiId.ServerProps); } }

		/// <summary>
		/// Called when a player interacts with the prop (touch, attack).
		/// </summary>
		public PropFunc Behavior { get; set; }

		public string Name { get; set; }
		public string Title { get; set; }

		/// <summary>
		/// Known states: single, closed, open, state1-3
		/// </summary>
		public string State { get; set; }

		/// <summary>
		/// XML with additional options.
		/// </summary>
		public string XML { get; set; }

		public override EntityType EntityType { get { return Entities.EntityType.Prop; } }

		public override int RegionId
		{
			get { return this.Info.Region; }
			set { this.Info.Region = value; }
		}

		public Prop(int id, int region, int x, int y, float direction, float scale = 1f, float altitude = 0)
			: this("", "", "", id, region, x, y, direction, scale, altitude)
		{
		}

		public Prop(string name, string title, string extra, int id, int region, int x, int y, float direction, float scale = 1, float altitude = 0)
			: this(0, name, title, extra, id, region, x, y, direction, scale, altitude)
		{
			this.EntityId = Interlocked.Increment(ref _propId);
			this.EntityId += (long)region << 32;
			this.EntityId += AuraData.RegionInfoDb.GetAreaId(region, x, y) << 16;
		}

		public Prop(long entityId, string name, string title, string extra, int id, int region, int x, int y, float direction, float scale = 1, float altitude = 0)
		{
			this.EntityId = entityId;

			this.Name = name;
			this.Title = title;
			this.XML = extra;

			this.Info.Id = id;
			this.Info.Region = region;
			this.Info.X = x;
			this.Info.Y = y;
			this.Info.Direction = direction;
			this.Info.Scale = scale;

			this.Info.Color1 =
			this.Info.Color2 =
			this.Info.Color3 =
			this.Info.Color4 =
			this.Info.Color5 =
			this.Info.Color6 =
			this.Info.Color7 =
			this.Info.Color8 =
			this.Info.Color9 = 0xFF808080;
		}

		public override Position GetPosition()
		{
			return new Position((int)this.Info.X, (int)this.Info.Y);
		}

		public override string ToString()
		{
			return string.Format("Prop: 0x{0}, Region: {1}, X: {2}, Y: {3}", this.EntityIdHex, this.Info.Region, this.Info.X, this.Info.Y);
		}

		/// <summary>
		/// Returns prop behavior for dropping.
		/// </summary>
		/// <param name="dropType"></param>
		/// <returns></returns>
		public static PropFunc GetDropBehavior(int dropType)
		{
			return (creature, prop) =>
			{
				if (RandomProvider.Get().NextDouble() > ChannelServer.Instance.Conf.World.PropDropChance)
					return;

				var dropInfo = AuraData.PropDropDb.Find(dropType);
				if (dropInfo == null)
				{
					Log.Warning("GetDropBehavior: Unknown prop drop type '{0}'.", dropType);
					return;
				}

				var rnd = RandomProvider.Get();

				// Get random item from potential drops
				var dropItemInfo = dropInfo.GetRndItem(rnd);
				var rndAmount = (dropItemInfo.Amount > 1 ? (ushort)rnd.Next(1, dropItemInfo.Amount) : (ushort)1);

				var item = new Item(dropItemInfo.ItemClass);
				item.Info.Amount = rndAmount;
				item.Drop(prop.Region, creature.GetPosition());
			};
		}

		/// <summary>
		/// Returns prop behavior for warping.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static PropFunc GetWarpBehavior(int region, int x, int y)
		{
			return (creature, prop) =>
			{
				creature.Warp(region, x, y);
			};
		}
	}

	public delegate void PropFunc(Creature creature, Prop prop);
}
