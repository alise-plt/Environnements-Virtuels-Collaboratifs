using UnityEngine;

using Unity.Netcode;

using Unity.Netcode.Components;


public class CursorDriver : NetworkBehaviour {


   private bool active ;

   private Camera theCamera ;


   // Start is called once before the first execution of Update after the MonoBehaviour is created

   void Start () {

       if (HasAuthority && IsSpawned) {

           theCamera = (Camera)GameObject.FindFirstObjectByType (typeof(Camera)) ;    

           active = false ;

       }

   }


   // Update is called once per frame

   void Update () {

       if (HasAuthority && IsSpawned) {

           if (Input.GetKeyDown (KeyCode.LeftAlt)) {

               active = true ;

           }

           if (Input.GetKeyUp (KeyCode.LeftAlt)) {

               active = false ;

           }

           if ((Input.mousePosition != null) && (active)) {

               Vector3 point = new Vector3 () ;

               Vector3 mousePos = Input.mousePosition ;

               float deltaZ = Input.mouseScrollDelta.y / 10.0f ;

               transform.Translate (0, 0, deltaZ) ;

               point = theCamera.ScreenToWorldPoint (new Vector3 (mousePos.x, mousePos.y, transform.localPosition.z)) ;

               transform.position = point ;

           }              

       }

   }

}

