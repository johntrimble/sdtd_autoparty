using System.Collections.Generic;

namespace AutoParty {
    public class API : IModApi {
        public void InitMod(Mod _modInstanc) {
            Log.Out("[AUTOPARTY] Started");
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawned);
        }

        public static EntityPlayer GetEntityPlayer(int entityId) {
            EntityPlayer entityPlayer;
            GameManager.Instance.World.Players.dict.TryGetValue(entityId, out entityPlayer);
            return entityPlayer;
        }

        private static void PlayerSpawned(ClientInfo info, RespawnType t, Vector3i pos) {
            Log.Out(string.Format("[AUTOPARTY] Handler fired, respawn type {0}", t));
            
            if (info != null && t == RespawnType.JoinMultiplayer) {
                Log.Out(string.Format("[AUTOPARTY] Player spawned in {0}", info.playerName));

                EntityPlayer player = GetEntityPlayer(info.entityId);
                if (player.IsInParty()) {
                    return;
                }

                Log.Out(string.Format("[AUTOPARTY] Player {0} is not in party, looking for party to join", player.EntityName));
                foreach (KeyValuePair<int, EntityPlayer> entry in GameManager.Instance.World.Players.dict ) {
                    EntityPlayer other = entry.Value;
                    if (other.entityId == player.entityId) {
                        continue;
                    }

                    if (player.partyInvites != null && player.partyInvites.Contains(other)) {
                        continue;
                    }

                    if ( (other.IsFriendsWith(player) && other.IsInParty() && other.IsPartyLead() && other.Party != null) || !other.IsInParty() ) {
                        Log.Out(string.Format("[AUTOPARTY] Player {0} is freinds with {1}, joining their party", player.EntityName, other.EntityName));
                        Party.ServerHandleAcceptInvite(other, player);
                        return;
                    }
                }

                Log.Out(string.Format("[AUTOPARTY] Could not find allied party for {0}", player.EntityName));
            }
        }
    }
}