using UnityEngine;
namespace ThirdPersonShooter {

  [System.Serializable]
  public class CameraRotate {
    private GameObject objectToRotate;
    [SerializeField] float incrementInSpeed;
    public float IncrementInSpeed {
      get { return this.incrementInSpeed; }
      set { this.incrementInSpeed = value; }
    }
    public GameObject ObjectToRotate {
      get { return this.objectToRotate; }
      set { this.objectToRotate = value; }
    }
    public CameraRotate (GameObject _go) {
      ObjectToRotate = _go;
      IncrementInSpeed = 1f;
    }
    public void Rotate (float _speed) {
      this.objectToRotate.transform.Rotate (Vector3.up * _speed * IncrementInSpeed);
    }
  }
}
