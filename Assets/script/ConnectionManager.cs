using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [Header("Prefabs des Joueurs")]
    public GameObject pcPlayerPrefab;
    public GameObject vrPlayerPrefab;
   private string _profileName;
   private string _sessionName;
   private int _maxPlayers = 10;
   private ConnectionState _state = ConnectionState.Disconnected;
   private ISession _session;
   private NetworkManager m_NetworkManager;

   private enum ConnectionState
   {
       Disconnected,
       Connecting,
       Connected,
   }

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        await UnityServices.InitializeAsync();
    }

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
        }
    }

private void OnClientConnectedCallback(ulong clientId)
    {
        // Seul le Serveur (Hôte) a le droit de faire apparaître des objets sur le réseau
        if (m_NetworkManager.IsServer)
        {
            GameObject prefabToSpawn;

            // Si le client qui vient de se connecter est le PC lui-même
            if (clientId == m_NetworkManager.LocalClientId)
            {
                prefabToSpawn = pcPlayerPrefab;
                Debug.Log($"Le Serveur (PC) se connecte. Spawn du corps PC.");
            }
            else // Sinon, c'est un client externe (le casque VR)
            {
                prefabToSpawn = vrPlayerPrefab;
                Debug.Log($"Un Client (VR) se connecte. Spawn de l'avatar VR.");
            }

            // On instancie le bon prefab et on l'assigne à ce joueur
            GameObject playerInstance = Instantiate(prefabToSpawn);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

   private void OnGUI()
   {
       if (_state == ConnectionState.Connected)
           return;

       GUI.enabled = _state != ConnectionState.Connecting;

       using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
       {
           GUILayout.Label("Profile Name", GUILayout.Width(100));
           _profileName = GUILayout.TextField(_profileName);
       }

       using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
       {
           GUILayout.Label("Session Name", GUILayout.Width(100));
           _sessionName = GUILayout.TextField(_sessionName);
       }

       GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(_profileName) && !string.IsNullOrEmpty(_sessionName);

       if (GUILayout.Button("Create or Join Session"))
       {
            _ = CreateOrJoinSessionAsync();
       }
   }

   private void OnDestroy()
   {
       _session?.LeaveAsync();
   }

   private async Task CreateOrJoinSessionAsync()
   {
       _state = ConnectionState.Connecting;

       try
       {
            if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }
            AuthenticationService.Instance.SwitchProfile(_profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions() {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

           _state = ConnectionState.Connected;
       }
       catch (Exception e)
       {
           _state = ConnectionState.Disconnected;
           Debug.LogException(e);
       }
   }
}
