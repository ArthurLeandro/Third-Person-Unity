using System;
using UniRx;
using UnityEngine;
[RequireComponent (typeof (Camera))]
public class TariumCamera : MonoBehaviour {

  #region Zoom
  [SerializeField] private float zoomSpeedMouse = 100f;
  [SerializeField] private float zoomDampening = 2f;
  [SerializeField] private float [] zoomBounds = new float [] { 10f, 85f };
  #endregion
  #region Pan
  [SerializeField] private float panSpeed = 20f;
  [SerializeField] private float [] boundsX = new float [] {-10f, 5f };
  [SerializeField] private float [] boundsZ = new float [] {-18f, -4f };
  private Vector3 lastPanPosition;
  #endregion
  #region Rotate

  #endregion
  private Camera cam;
  [SerializeField] private Transform target;
  private void Awake () {
    this.cam = GetComponent<Camera> ();
  }
  private void Start () {
    var zoomCamera = Observable.EveryUpdate ()
      .Select (zoom => Input.GetAxis ("Mouse ScrollWheel"));
    zoomCamera.Subscribe (zoomValue => ZoomCamera (zoomValue));

    var panCameraX = Observable.EveryUpdate ()
      .Where (input => Input.GetMouseButton (1))
      .Select (input => Input.GetAxis ("Mouse X"));
    panCameraX.Subscribe (pos => PanCamera (pos));
    lastPanPosition = target.position;
  }

  public void ZoomCamera (float zoomValue) {
    if (zoomValue != 0) {
      cam.fieldOfView = Mathf.Clamp (Mathf.Lerp (cam.fieldOfView, (cam.fieldOfView - (zoomValue * zoomSpeedMouse)), zoomDampening), zoomBounds [0], zoomBounds [1]);
    }
  }
  public void PanCamera (float panPosition) {
    Vector3 positionToTranslate = transform.position;
    positionToTranslate.z += panPosition;
    Debug.Log (positionToTranslate);
  }
}
/*


    // lastPanPosition = Input.mousePosition;
    // Determine how much to move the camera
    Vector3 offset = cam.ScreenToViewportPoint (lastPanPosition - newPanPosition);
    Vector3 move = new Vector3 (offset.x * panSpeed, 0, offset.y * panSpeed);

    // Perform the movement
    transform.Translate (move, Space.World);

    // Ensure the camera remains within bounds.
    Vector3 pos = transform.position;
    pos.x = Mathf.Clamp (transform.position.x, boundsX [0], boundsX [1]);
    pos.z = Mathf.Clamp (transform.position.z, boundsZ [0], boundsZ [1]);
    transform.position = pos;

    // Cache the position
    lastPanPosition = newPanPosition;



    void HandleMouse() {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0)) {
            lastPanPosition = Input.mousePosition;
        } else if (Input.GetMouseButton(0)) {
            PanCamera(Input.mousePosition);
        }
    
        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }
    
    void PanCamera(Vector3 newPanPosition) {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);
        
        // Perform the movement
        transform.Translate(move, Space.World);  
        
        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;
    
        // Cache the position
        lastPanPosition = newPanPosition;
    }
    
    void ZoomCamera(float offset, float speed) {
        if (offset == 0) {
            return;
        }
    
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}
*/
