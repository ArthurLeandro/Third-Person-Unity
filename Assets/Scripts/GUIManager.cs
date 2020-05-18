using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace ThirdPersonShooter {
	public class GUIManager : MonoBehaviour {
		public static GUIManager instance;
		[SerializeField] private GameObject crossHair;
		private void Awake () {
			if (instance == null) {
				instance = this;
			}
			DontDestroyOnLoad (this);
		}
		void Start () {
			Cursor.visible = false;
			Observable.EveryUpdate ()
				.Select (position => Input.mousePosition)
				.Subscribe (x => CrossHair (x));
		}

		public void CrossHair (Vector3 _position) {
			crossHair.transform.position = _position;
		}
		public void SetCrossHair (bool value) {
			crossHair.SetActive (value);
		}
	}
}
