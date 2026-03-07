using ExitGames.Client.Photon;
using GorillaLibrary.GameModes.Models;
using GorillaLibrary.GameModes.Utilities;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaLibrary.GameModes.Behaviours
{
    internal class NetworkController : MonoBehaviourPunCallbacks
    {
        public static NetworkController Instance { get; private set; }

        private InternalRoom lastRoom;

        public override void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            base.OnEnable(); // Tell Photon to register this object as a callback target, this will be important shortly

            if (NetworkSystem.Instance is NetworkSystem netSys && netSys is NetworkSystemPUN && PhotonNetwork.NetworkingClient is LoadBalancingClient client)
            {
                // The following code inserts our callbacks right before the network system does
                // This ensures any relative members a part of Utilla are properly defined before anything else gets their values

                AccessTools.Method(client.GetType(), "UpdateCallbackTargets").Invoke(client, null);
                MatchMakingCallbacksContainer callbackContainer = client.MatchMakingCallbackTargets;

                for (int i = 0; i < callbackContainer.Count; i++)
                {
                    IMatchmakingCallbacks individualCallback = callbackContainer[i];
                    if ((object)individualCallback is MonoBehaviour behaviour && behaviour.gameObject == netSys.gameObject)
                    {
                        if (callbackContainer.Contains(this)) callbackContainer.Remove(this);
                        callbackContainer.Insert(i, this);
                        break;
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (Instance == this) Instance = null;
        }

        public override void OnJoinedRoom()
        {
            if (ApplicationQuittingState.IsQuitting) return;

            // trigger events

            NetworkSystem netSys = NetworkSystem.Instance;
            bool isPrivate = netSys.SessionIsPrivate;
            string gameMode = netSys.GameModeString;

            GameModeUtility.CurrentGamemode = GameModeUtility.FindGamemodeInString(gameMode);

            InternalRoom args = new()
            {
                IsPrivate = isPrivate,
                Gamemode = gameMode
            };

            GameModeManager.Instance.OnRoomJoin(args);

            lastRoom = args;

            //RoomUtils.ResetQueue();
        }

        public override void OnLeftRoom()
        {
            if (ApplicationQuittingState.IsQuitting) return;

            GameModeUtility.CurrentGamemode = null;

            if (lastRoom != null)
            {
                GameModeManager.Instance.OnRoomLeft(lastRoom);
                lastRoom = null;
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (ApplicationQuittingState.IsQuitting || !NetworkSystem.Instance.InRoom || NetworkSystem.Instance.GameModeString is not string gameMode || gameMode == null) return;

            GameModeUtility.CurrentGamemode = GameModeUtility.FindGamemodeInString(gameMode);

            if (lastRoom.Gamemode != gameMode || lastRoom.IsPrivate != NetworkSystem.Instance.SessionIsPrivate)
            {
                GameModeManager.Instance.OnRoomLeft(lastRoom);

                lastRoom.Gamemode = gameMode;
                lastRoom.IsPrivate = NetworkSystem.Instance.SessionIsPrivate;

                GameModeManager.Instance.OnRoomJoin(lastRoom);
            }
        }
    }
}
