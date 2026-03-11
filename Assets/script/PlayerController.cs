using Unity.Netcode.Components;
using UnityEngine;
using Unity.Netcode;

#if UNITY_EDITOR
using Unity.Netcode.Editor;
using UnityEditor;

/// <summary>
/// The custom editor for the <see cref="PlayerController"/> component.
/// </summary>
[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : NetworkTransformEditor
{
    public Vector3 cameraPositionOffset = new Vector3(0, 1.6f, 0);
    public Quaternion cameraOrientationOffset = new Quaternion();
    protected Transform cameraTransform;
    protected Camera theCamera;
    
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;
    private SerializedProperty m_ObjectToCreate;

    public override void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(PlayerController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(PlayerController.ApplyVerticalInputToZAxis));
        base.OnEnable();
        m_ObjectToCreate = serializedObject.FindProperty(nameof(PlayerController.ObjectToCreate));
    }

    private void DisplayPlayerControllerProperties()
    {
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);
        EditorGUILayout.PropertyField(m_ObjectToCreate);
    }

    public override void OnInspectorGUI()
    {
        var PlayerController = target as PlayerController;
        void SetExpanded(bool expanded) { PlayerController.PlayerControllerPropertiesVisible = expanded; };
        DrawFoldOutGroup<PlayerController>(PlayerController.GetType(), DisplayPlayerControllerProperties, PlayerController.PlayerControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif


public class PlayerController : NetworkTransform
{
#if UNITY_EDITOR
    // These bool properties ensure that any expanded or collapsed property views
    // within the inspector view will be saved and restored the next time the
    // asset/prefab is viewed.
    public bool PlayerControllerPropertiesVisible;
#endif

    public Vector3 cameraPositionOffset = new Vector3(0, 1.6f, 0);
    public Quaternion cameraOrientationOffset = new Quaternion();
    protected Transform cameraTransform;
    protected Camera theCamera;

    public float Speed = 10;
    public bool ApplyVerticalInputToZAxis;
    [Header("Objet à faire apparaître")]
    public GameObject ObjectToCreate;
    private Vector3 m_Motion;

    public void Start()
    {
        CatchCamera();
    }

    public void CatchCamera()
    {
        if (IsSpawned && HasAuthority)
        {
            // attach the camera to the navigation rig
            theCamera = (Camera)GameObject.FindFirstObjectByType(typeof(Camera));
            theCamera.enabled = true;
            cameraTransform = theCamera.transform;
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = cameraPositionOffset;
            cameraTransform.localRotation = cameraOrientationOffset;
        }
    }
    
    private void Update()
    {
        if (!IsSpawned || !HasAuthority)
        {
            return; 
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CatchCamera();
        }

        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);

        if (Input.GetKeyDown (KeyCode.P)) 
        {
            Debug.Log ("Spawn requested");
            var myNewCube = Instantiate (ObjectToCreate);
            var myNetworkedNewCube = myNewCube.GetComponent<NetworkObject>();
            Vector3 newPosition = new Vector3 (transform.position.x+1, transform.position.y + 1.0f, transform.position.z);
            myNewCube.transform.position = newPosition;
            myNetworkedNewCube.Spawn();
        }
    }
}