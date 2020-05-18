using UniRx;
using UnityEngine;
namespace ThirdPersonShooter {
  [System.Serializable]
  public class SpawnerController {
    // [SerializeField] Transform positionToSpawn;
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] float timeToDisable;
    public ReactiveProperty<bool> shouldActivate;

    public SpawnerController (GameObject _objectToSpawn, float _timeToDisable) {
      // this.positionToSpawn = _position;
      this.objectToSpawn = _objectToSpawn;
      shouldActivate = new ReactiveProperty<bool> ();
      shouldActivate.Value = false;
      this.timeToDisable = _timeToDisable;
      shouldActivate.Select (activate => shouldActivate.Value == true)
        .Subscribe (x => ShowEffect ());
    }
    public void ShowEffect () {
      this.objectToSpawn.SetActive (true);
      shouldActivate.Throttle (System.TimeSpan.FromMilliseconds (this.timeToDisable))
        .Subscribe (x => DisableEffect ());
    }
    public void DisableEffect () {
      shouldActivate.Value = false;
      this.objectToSpawn.SetActive (false);

    }
  }
}
