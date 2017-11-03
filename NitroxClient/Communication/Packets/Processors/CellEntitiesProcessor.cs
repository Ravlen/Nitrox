﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class CellEntitiesProcessor : ClientPacketProcessor<CellEntities>
    {
        private PacketSender packetSender;
        private HashSet<string> alreadySpawnedGuids = new HashSet<string>();
        
        public CellEntitiesProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CellEntities packet)
        {
            foreach(Entity entity in packet.Entities)
            {
                if(!alreadySpawnedGuids.Contains(entity.Guid))
                {
                    if (entity.TechType != TechType.None)
                    {
                        GameObject gameObject = CraftData.InstantiateFromPrefab(entity.TechType);
                        gameObject.transform.position = entity.Position;
                        GuidHelper.SetNewGuid(gameObject, entity.Guid);
                        gameObject.SetActive(true);

                        Log.Debug("Received cell entity: " + entity.Guid + " at " + entity.Position + " of type " + entity.TechType);

                        if (entity.SimulatingPlayerId.IsPresent() && entity.SimulatingPlayerId.Get() == packetSender.PlayerId)
                        {
                            Log.Debug("Simulating positioning of: " + entity.Guid);
                            EntityPositionBroadcaster.WatchEntity(entity.Guid, gameObject);
                        }
                    }

                    alreadySpawnedGuids.Add(entity.Guid);
                }

                Multiplayer.Logic.SimulationOwnership.AddOwnedGuid(entity.Guid, entity.SimulatingPlayerId.Get());
            }
        }
    }
}