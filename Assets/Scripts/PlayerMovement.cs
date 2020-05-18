using ThirdPersonShooter;
using UniRx;
using UnityEngine;

namespace ThirdPersonShooter {
	[RequireComponent (typeof (Animator))]
	public class PlayerMovement : MonoBehaviour {
		private AnimateController animateController = null;
		private CameraRotate cameraRotate = null;
		[SerializeField] Gun weapon;
		private bool isRunning;
		public bool IsRunning {
			get { return this.isRunning; }
			set { this.isRunning = value; }
		}
		private void Awake () {
			animateController = new AnimateController (GetComponent<Animator> ());
			cameraRotate = new CameraRotate (this.gameObject);
			weapon.Init ();
		}
		private void Start () {
			Observable.EveryUpdate ()
				.Select (movement => Input.GetAxis ("Vertical"))
				.Subscribe (x => CharacterMovement (x));

			Observable.EveryUpdate ()
				.Select (shift => Input.GetKey (KeyCode.LeftShift))
				.Subscribe (x => Running ("Running", x));

			Observable.EveryUpdate ()
				.Select (aiming => Input.GetMouseButton (1))
				.Subscribe (x => Aiming ("Aiming", x, Input.mousePosition));

			Observable.EveryUpdate ()
				.Select (rotation => Input.GetAxis ("Mouse X"))
				.Subscribe (x => RotateCamera (x));

			Observable.EveryUpdate ()
				.Select (aiming => Input.GetMouseButton (1))
				.Where (x => Input.GetMouseButton (0))
				.Subscribe (x => Shooting ());
		}

		private void Update () {}
		private void CharacterMovement (float _speedMovement) {
			animateController.CharacterAnimateMovement (_speedMovement, IsRunning);
		}
		private void Running (string s, bool _run) {
			animateController.BoolAnim (s, _run);
		}
		private void RotateCamera (float _speed) {
			this.cameraRotate.Rotate (_speed);
		}
		private void Aiming (string s, bool _bool, Vector3 _mousePosition) {
			this.animateController.BoolAnim (s, _bool);
			Camera.main.GetComponent<Animator> ().SetBool (s, _bool);
			GUIManager.instance.SetCrossHair (_bool);
			GUIManager.instance.CrossHair (_mousePosition);
		}
		private void Shooting () {
			this.weapon.Shooting ();
		}
	}
}
