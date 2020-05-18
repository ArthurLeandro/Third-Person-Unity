using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RXDevelopmentKit {
  public class CameraSwitcher : MonoBehaviour {
    public Camera [] allCameras;
    int currentCamera = 0;

    private void Start () {
      if (allCameras == null || allCameras.Length == 0) {
        Debug.LogError ("CAMERASWITCHER Awake - No camera assigned.");
        this.enabled = false;
      }
      EnableOnlyFirstCamera ();
      var cameraObservable = Observable.EveryUpdate ()
        .Where (key => Input.GetKeyDown (KeyCode.C));

      cameraObservable.Subscribe (camera => SwitchCamera ());

    }
    public void SwitchCamera () {
      // disable current
      allCameras [currentCamera].enabled = false;
      // increment index and wrap after finished array
      currentCamera = (currentCamera + 1) % allCameras.Length;
      // enable next
      allCameras [currentCamera].enabled = true;
    }
    public void EnableOnlyFirstCamera () {
      for (int i = 0; i < allCameras.Length; i++) {
        allCameras [i].enabled = (i == 0);
      }
    }
  }

  public class CameraZoom : MonoBehaviour {
    float zoomSpeed = 20f;
    public float ZoomSpeed {
      get { return this.zoomSpeed; }
      set { this.zoomSpeed = value; }
    }
    private void Start () {
      var mouseScroll = Observable.EveryUpdate ()
        .Select (zoom => Input.GetAxis ("Mouse ScrollWheel"));
      mouseScroll.Subscribe (zoom => ZoomCamera (zoom));
    }

    public void ZoomCamera (float _zoomValue) {
      if (_zoomValue != 0) {
        this.transform.Translate (transform.forward * _zoomValue * ZoomSpeed * Time.deltaTime, Space.Self);
      }
    }
  }

  public class IsometricCamera : MonoBehaviour {

    #region Properties

    #region Attributes
    private Transform target;
    private float targetHeight = 1.7f;
    private float distance = 5.0f;
    private float offsetFromWall = 0.1f;
    private float maxDistance = 20;
    private float minDistance = .6f;
    private float xSpeed = 200.0f;
    private float ySpeed = 200.0f;
    private float targetSpeed = 5.0f;
    private int yMinLimit = -80;
    private int yMaxLimit = 80;
    private int zoomRate = 40;
    private float rotationDampening = 3.0f;
    private float zoomDampening = 5.0f;
    private LayerMask collisionLayers = -1;
    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;
    private Vector3 TargetOffset;

    #endregion

    #region Getters & Setters
    Vector3 targetOffset;
    public Transform Target {
      get { return target; }

      set { target = value; }
    }
    public float TargetHeight {
      get { return targetHeight; }
      set { targetHeight = value; }
    }
    public float Distance {
      get { return distance; }
      set { distance = value; }
    }
    public float OffsetFromWall {
      get { return offsetFromWall; }
      set { offsetFromWall = value; }
    }
    public float MaxDistance {
      get { return maxDistance; }
      set { maxDistance = value; }
    }
    public float MinDistance {
      get { return minDistance; }
      set { minDistance = value; }
    }
    public float XSpeed {
      get { return xSpeed; }
      set { xSpeed = value; }
    }
    public float YSpeed {
      get { return ySpeed; }
      set { ySpeed = value; }
    }
    public float TargetSpeed {
      get { return targetSpeed; }
      set { targetSpeed = value; }
    }
    public int YMinLimit {
      get { return yMinLimit; }
      set { yMinLimit = value; }
    }
    public int YMaxLimit {
      get { return yMaxLimit; }
      set { yMaxLimit = value; }
    }
    public int ZoomRate {
      get { return zoomRate; }
      set { zoomRate = value; }
    }
    public float RotationDampening {
      get { return rotationDampening; }
      set { rotationDampening = value; }
    }
    public float ZoomDampening {
      get { return zoomDampening; }
      set { zoomDampening = value; }
    }
    public LayerMask CollisionLayers {
      get { return collisionLayers; }
      set { collisionLayers = value; }
    }
    public float XDeg {
      get { return xDeg; }
      set { xDeg = value; }
    }
    public float YDeg {
      get { return yDeg; }
      set { yDeg = value; }
    }
    #endregion
    #endregion

    #region Behaviours

    #region Life Cycle Hooks
    private void Start () {
      Vector3 angles = transform.eulerAngles;
      XDeg = angles.x;
      YDeg = angles.y;
      currentDistance = desiredDistance = correctedDistance = Distance;
      //In Case of RigidBody , this camera collides with stuff
      /*
        RigidBody rb =GetComponent<RigidBody>(); 
        if(rb !=null)
          rb.freezeRotation = true;
      */
      var cameraMove = Observable.EveryLateUpdate ()
        .Where (input => Input.GetMouseButton (0));
      cameraMove.Subscribe (input => MoveCamera ());
      var cameraRotate = Observable.EveryLateUpdate ()
        .Where (input => Input.GetMouseButton (1));
      cameraRotate.Subscribe (input => RotateCamera ());
      var cameraFollow = Observable.EveryLateUpdate ()
        .Where (follow => Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0);
      cameraFollow.Subscribe (follow => FollowCamera ());
      Observable.EveryUpdate ()
        .Do (input => CorrectCameraBasedOnCollision ());
    }
    #endregion
    #region Procedures

    /// <summary>
    /// Correciona a posição da câmera caso colida com algo.
    /// </summary>
    public void CorrectCameraBasedOnCollision () {
      yDeg = ClampAngle (yDeg, yMinLimit, yMaxLimit);
      // set camera rotation
      Quaternion rotation = Quaternion.Euler (yDeg, xDeg, 0);
      // calculate the desired distance
      desiredDistance -= Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
      desiredDistance = Mathf.Clamp (desiredDistance, minDistance, maxDistance);
      correctedDistance = desiredDistance;
      // calculate desired camera position
      TargetOffset = new Vector3 (0, -targetHeight, 0);
      Vector3 position = target.position - (rotation * Vector3.forward * desiredDistance + TargetOffset);
      // check for collision using the true target's desired registration point as set by user using height
      RaycastHit collisionHit;
      Vector3 trueTargetPosition = new Vector3 (target.position.x, target.position.y + targetHeight, target.position.z);
      // if there was a collision, correct the camera position and calculate the corrected distance
      bool isCorrected = false;
      if (Physics.Linecast (trueTargetPosition, position, out collisionHit, collisionLayers.value)) {
        // calculate the distance from the original estimated position to the collision location,
        // subtracting out a safety "offset" distance from the object we hit.  The offset will help
        // keep the camera from being right on top of the surface we hit, which usually shows up as
        // the surface geometry getting partially clipped by the camera's front clipping plane.
        correctedDistance = Vector3.Distance (trueTargetPosition, collisionHit.point) - offsetFromWall;
        isCorrected = true;
      }
      // For smoothing, lerp distance only if either distance wasn't corrected, or correctedDistance is more than currentDistance
      currentDistance = !isCorrected || correctedDistance > currentDistance ? Mathf.Lerp (currentDistance, correctedDistance, Time.deltaTime * zoomDampening) : correctedDistance;
      // keep within legal limits
      currentDistance = Mathf.Clamp (currentDistance, minDistance, maxDistance);
      // recalculate position based on the new currentDistance
      position = target.position - (rotation * Vector3.forward * currentDistance + TargetOffset);
      transform.rotation = rotation;
      transform.position = position;
    }
    /// <summary>
    /// Movimenta a camera do personagem permitindo que o mouse controle ela.
    /// </summary>
    public void MoveCamera () {
      xDeg += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
      yDeg -= Input.GetAxis ("Mouse Y") * ySpeed * 0.02f;
    }
    /// <summary>
    /// Reseta o ângulo da câmera.
    /// Depois, rotaciona ao redor do mundo.
    /// </summary>
    public void RotateCamera () {
      float targetRotationAngle = target.eulerAngles.y;
      float currentRotationAngle = transform.eulerAngles.y;
      xDeg = Mathf.LerpAngle (currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
      target.transform.Rotate (0, Input.GetAxis ("Mouse X") * xSpeed * 0.02f, 0);
      xDeg += Input.GetAxis ("Mouse X") * xSpeed * 0.02f;
    }
    /// <summary>
    /// Siga o alvo baseado em algum fator, neste caso será caso qualquer tecla seja apertada.
    /// </summary>
    public void FollowCamera () {
      float targetRotationAngle = target.eulerAngles.y;
      float currentRotationAngle = transform.eulerAngles.y;
      xDeg = Mathf.LerpAngle (currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
    }
    #endregion
    #region Functions
    private static float ClampAngle (float angle, float min, float max) {
      if (angle < -360)
        angle += 360;
      if (angle > 360)
        angle -= 360;
      return Mathf.Clamp (angle, min, max);
    }
    #endregion
    #endregion
  }

  public class SmoothMouse : MonoBehaviour {
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;
    public Vector2 clampInDegrees = new Vector2 (360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2 (2, 2);
    public Vector2 smoothing = new Vector2 (3, 3);
    public Vector2 targetDirection;
    private void Start () {
      // Set target direction to the camera's initial orientation.
      targetDirection = transform.rotation.eulerAngles;
      Cursor.visible = !lockCursor;

      var smoothCameraMove = Observable.EveryLateUpdate ()
        .Where (input => Input.GetKeyDown (KeyCode.Escape));
      smoothCameraMove.Subscribe (move => SmoothCamera ());
    }
    public void SmoothCamera () {
      lockCursor = !lockCursor;
      // Ensure the cursor is always locked when set
      Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
      Cursor.visible = !lockCursor;
      // Allow the script to clamp based on a desired target value.
      Quaternion targetOrientation = Quaternion.Euler (targetDirection);
      // Get raw mouse input for a cleaner reading on more sensitive mice.
      var mouseDelta = new Vector2 (Input.GetAxisRaw ("Mouse X"), Input.GetAxisRaw ("Mouse Y"));
      // Scale input against the sensitivity setting and multiply that against the smoothing value.
      mouseDelta = Vector2.Scale (mouseDelta, new Vector2 (sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
      // Interpolate mouse movement over time to apply smoothing delta.
      _smoothMouse.x = Mathf.Lerp (_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
      _smoothMouse.y = Mathf.Lerp (_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
      // Find the absolute mouse movement value from point zero.
      _mouseAbsolute += _smoothMouse;
      // Clamp and apply the local x value first, so as not to be affected by world transforms.
      if (clampInDegrees.x < 360)
        _mouseAbsolute.x = Mathf.Clamp (_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);
      var xRotation = Quaternion.AngleAxis (-_mouseAbsolute.y, targetOrientation * Vector3.right);
      transform.localRotation = xRotation;
      // Then clamp and apply the global y value.
      if (clampInDegrees.y < 360)
        _mouseAbsolute.y = Mathf.Clamp (_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);
      var yRotation = Quaternion.AngleAxis (_mouseAbsolute.x, transform.InverseTransformDirection (Vector3.up));
      transform.localRotation *= yRotation;
      transform.rotation *= targetOrientation;
    }
  }

  public class CameraShake : MonoBehaviour {

    public bool shakePosition;
    public bool shakeRotation;
    public float shakeIntensityMin = 0.1f;
    public float shakeIntensityMax = 0.5f;
    public float shakeDecay = 0.02f;
    private Vector3 OriginalPos;
    private Quaternion OriginalRot;
    private bool isShakeRunning = false;

    //TODO Usar MicroCorrotina

    // call this function to start shaking
    public void Shake () {
      OriginalPos = transform.position;
      OriginalRot = transform.rotation;
      StartCoroutine ("ProcessShake");
    }
    IEnumerator ProcessShake () {
      if (!isShakeRunning) {
        isShakeRunning = true;
        float currentShakeIntensity = Random.Range (shakeIntensityMin, shakeIntensityMax);
        while (currentShakeIntensity > 0) {
          if (shakePosition) {
            transform.position = OriginalPos + Random.insideUnitSphere * currentShakeIntensity;
          }
          if (shakeRotation) {
            transform.rotation = new Quaternion (OriginalRot.x + Random.Range (-currentShakeIntensity, currentShakeIntensity) * .2f,
              OriginalRot.y + Random.Range (-currentShakeIntensity, currentShakeIntensity) * .2f,
              OriginalRot.z + Random.Range (-currentShakeIntensity, currentShakeIntensity) * .2f,
              OriginalRot.w + Random.Range (-currentShakeIntensity, currentShakeIntensity) * .2f);
          }
          currentShakeIntensity -= shakeDecay;
          yield return null;
        }
        isShakeRunning = false;
      }
    }
  }

}
