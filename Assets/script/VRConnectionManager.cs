using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class VRConnectionManager : MonoBehaviour
{
   private string _profileName;
   private string _sessionName;
   private int _maxPlayers = 10;
   private ConnectionState _state = ConnectionState.Disconnected;
   private ISession _session;
   private NetworkManager m_NetworkManager;
   private bool _isConnecting = false; 

   private enum ConnectionState
   {
       Disconnected,
       Connecting,
       Connected,
   }

    private void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        _profileName = $"JoueurVR_{Guid.NewGuid().ToString("N").Substring(0, 20)}"; 
        InitializeUnityServices();
    }

    private async void InitializeUnityServices()
    {
        try {
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }

    public async void JoinSharedWorld()
    {
        if (_isConnecting) 
        {
            Debug.Log("Une tentative de connexion est déjà en cours.");
            return;
        }

        _isConnecting = true; 
        _sessionName = "ZZZ";

        Debug.Log($"Tentative de connexion : Profil={_profileName}, Session={_sessionName}");

        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SwitchProfile(_profileName);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            var options = new SessionOptions()
            {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

            _state = ConnectionState.Connected;
            Debug.Log("VR Connection Successful!");
        }
        catch (Exception e)
        {
            Debug.LogError($"VR Connection Failed: {e.Message}");
        }
        finally
        {
            _isConnecting = false; 
        }
    }

    private void OnDestroy()
    {
        _session?.LeaveAsync();
    }
}