using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkedXRGrabInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    private NetworkObject m_NetworkObject;
    private Renderer m_Renderer;

    private Color catchableColor = Color.cyan;
    private Color caughtColor = Color.yellow;
    private Color initialColor;

    protected override void Awake()
    {
        base.Awake();
        m_NetworkObject = GetComponent<NetworkObject>();
        m_Renderer = GetComponentInChildren<Renderer>();

        if (m_Renderer != null)
        {
            initialColor = m_Renderer.material.color;
        }
    }


    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        LocalShowCatchable();
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        LocalHideCatchable();
    }

    public void LocalShowCatchable()
    {
        ShowCatchableRpc();
    }

    public void LocalHideCatchable()
    {
        HideCatchableRpc();
    }


    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (m_NetworkObject != null && m_NetworkObject.IsSpawned)
        {
            if (!m_NetworkObject.IsOwner)
            {
                RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        base.OnSelectEntered(args);

        LocalCatch(); 
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        LocalRelease();
    }

    public void LocalCatch()
    {
        Debug.Log("Réclamation de l'ownership et mise à jour couleur");

        if (m_NetworkObject != null && !m_NetworkObject.IsOwner)
        {
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        ShowCaughtRpc();
    }

    public void LocalRelease()
    {
        ShowReleasedRpc();
    }

    //  RPC POUR LA SYNCHRONISATION DES COULEURS 

    [Rpc(SendTo.Everyone)]
    public void ShowCaughtRpc()
    {
        if (m_Renderer != null) m_Renderer.material.color = caughtColor;
    }

    [Rpc(SendTo.Everyone)]
    public void ShowReleasedRpc()
    {
        if (m_Renderer != null) m_Renderer.material.color = catchableColor;
    }

    [Rpc(SendTo.Everyone)]
    public void ShowCatchableRpc()
    {
        if (m_Renderer != null && !isSelected) 
            m_Renderer.material.color = catchableColor;
    }

    [Rpc(SendTo.Everyone)]
    public void HideCatchableRpc()
    {
        if (m_Renderer != null && !isSelected) 
            m_Renderer.material.color = initialColor;
    }

    // RPC POUR L'OWNERSHIP

    private void RequestOwnershipServerRpc(ulong clientId)
    {
        if (m_NetworkObject != null)
        {
            m_NetworkObject.ChangeOwnership(clientId);
        }
    }
}