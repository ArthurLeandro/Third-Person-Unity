using System.Threading;
using UniRx;
using UnityEngine;
public class IsometricCamera : MonoBehaviour {

  #region Properties

  #region Attributes
  [SerializeField] private Transform target;
  [SerializeField] private float targetHeight = 1.7f;
  [SerializeField] private float distance = 5.0f;
  [SerializeField] private float offsetFromWall = 0.1f;
  [SerializeField] private float maxDistance = 20;
  [SerializeField] private float minDistance = .6f;
  [SerializeField] private float xSpeed = 200.0f;
  [SerializeField] private float ySpeed = 200.0f;
  [SerializeField] private float targetSpeed = 5.0f;
  [SerializeField] private int yMinLimit = -80;
  [SerializeField] private int yMaxLimit = 80;
  [SerializeField] private int zoomRate = 40;
  [SerializeField] private float rotationDampening = 3.0f;
  [SerializeField] private float zoomDampening = 5.0f;
  [SerializeField] private LayerMask collisionLayers = -1;
  [SerializeField] private float xDeg = 0.0f;
  [SerializeField] private float yDeg = 0.0f;
  [SerializeField] private float currentDistance;
  [SerializeField] private float desiredDistance;
  [SerializeField] private float correctedDistance;
  [SerializeField] private Vector3 TargetOffset;

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
  void Start () {
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
    var cameraMove = Observable.EveryUpdate ()
      .Where (input => Input.GetMouseButton (0));
    cameraMove.Subscribe (input => MoveCamera ());
    var cameraRotate = Observable.EveryUpdate ()
      .Where (input => Input.GetMouseButton (1))
      .Select (horizontal => Input.GetAxis ("Mouse X"));
    cameraRotate.Subscribe (horizontalValue => RotateCamera (horizontalValue));
    var cameraFollow = Observable.EveryUpdate ()
      .Where (follow => Input.GetAxis ("Vertical") != 0 || Input.GetAxis ("Horizontal") != 0);
    cameraFollow.Subscribe (follow => FollowCamera ());
    var zoomCam = Observable.EveryUpdate ()
      .Select (zoomValue => Input.GetAxis ("Mouse ScrollWheel"));
    zoomCam.Subscribe (zoom => ZoomCamera (zoom));
  }

  #endregion
  #region Procedures
  public void ZoomCamera (float zoom) {
    if (zoom != 0f) {
      desiredDistance -= zoom * Time.deltaTime * zoomRate * Mathf.Abs (desiredDistance);
      desiredDistance = Mathf.Clamp (desiredDistance, minDistance, maxDistance);
      correctedDistance = desiredDistance;
      // calculate desired camera position
      TargetOffset = new Vector3 (0, -targetHeight, 0);
      Vector3 position = target.position - (Vector3.forward * desiredDistance + TargetOffset);
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
      position = target.position - (Vector3.forward * currentDistance + TargetOffset);
      transform.position = position;
    }
  }
  /// <summary>
  /// Correciona a posição da câmera caso colida com algo.
  /// </summary>
  public void CameraUpdate () {
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
    CameraUpdate ();
  }
  /// <summary>
  /// Reseta o ângulo da câmera.
  /// Depois, rotaciona ao redor do mundo.
  /// </summary>
  public void RotateCamera (float mouseHorizontalValue) {
    float targetRotationAngle = target.eulerAngles.y;
    float currentRotationAngle = transform.eulerAngles.y;
    xDeg = Mathf.LerpAngle (currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
    target.transform.Rotate (0, mouseHorizontalValue * xSpeed * 0.02f, 0);
    xDeg += mouseHorizontalValue * xSpeed * 0.02f;
    CameraUpdate ();
  }
  /// <summary>
  /// Siga o alvo baseado em algum fator, neste caso será caso qualquer tecla seja apertada.
  /// </summary>
  public void FollowCamera () {
    float targetRotationAngle = target.eulerAngles.y;
    float currentRotationAngle = transform.eulerAngles.y;
    xDeg = Mathf.LerpAngle (currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
    CameraUpdate ();
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
