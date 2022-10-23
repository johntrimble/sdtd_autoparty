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

        private static bool ServerHasParty() {
            return GameManager.Instance.World.GetPlayers().Find( (EntityPlayer player) => player.IsInParty() ) != null;
        }

        private static List<EntityPlayer> GetOtherPlayers(EntityPlayer player) {
            List<EntityPlayer> players = GameManager.Instance.World.GetPlayers();
            if (player != null) {
                players = players.FindAll( (EntityPlayer other) => other.entityId != player.entityId );
            }
            return players;
        }

        private static List<EntityPlayer> GetFriendsOfPlayer(EntityPlayer player) {
            return GetOtherPlayers(player).FindAll( (EntityPlayer other) => other.IsFriendsWith(player) );
        }

        private static EntityPlayer GetFirstPlayerThatIsPartyLead(List<EntityPlayer> players) {
            return players.Find( (EntityPlayer other) => other.IsInParty() && other.IsPartyLead() && other.Party != null );
        }

        private static void AddToFriendsParty(EntityPlayer player) {
            if (player.IsInParty()) {
                return;
            }

            Log.Out(string.Format("[AUTOPARTY] Player {0} is not in party, looking for party to join", player.EntityName));
            List<EntityPlayer> friends = GetFriendsOfPlayer(player);
            EntityPlayer partyLead = GetFirstPlayerThatIsPartyLead(friends);
            if (partyLead != null) {
                Log.Out(string.Format("[AUTOPARTY] Player {0} is freinds with {1}, joining their party", player.EntityName, partyLead.EntityName));
                Party.ServerHandleAcceptInvite(partyLead, player);
                return;
            } else if (ServerHasParty()) {
                Log.Out(string.Format("[AUTOPARTY] Could not find allied party for {0}, not creating new party as parties already exist", player.EntityName));
                return;
            } else {
                partyLead = friends.Find((EntityPlayer other) => !other.IsInParty());
                if (partyLead != null) {
                    Log.Out(string.Format("[AUTOPARTY] Player {0} is freinds with {1}, creating new party", player.EntityName, partyLead.EntityName));
                    Party.ServerHandleAcceptInvite(partyLead, player);
                    return;
                }
            }

            Log.Out(string.Format("[AUTOPARTY] Could not find allied party for {0}", player.EntityName));
        }

        private static void PlayerSpawned(ClientInfo info, RespawnType t, Vector3i pos) {
            Log.Out(string.Format("[AUTOPARTY] Handler fired, respawn type {0}", t));
            
            if (info != null && t == RespawnType.JoinMultiplayer) {
                Log.Out(string.Format("[AUTOPARTY] Player spawned in {0}", info.playerName));

                EntityPlayer player = GetEntityPlayer(info.entityId);
                AddToFriendsParty(player);
            }
        }
    }
}