using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace ThirdPersonShooter {
	[System.Serializable]
	public class Gun {
		SpawnerController spawner;
		SpawnerController impact;
		RaycastHit hit;
		[SerializeField] GameObject muzzle;

		[SerializeField] GameObject impactParticle;
		public Gun (GameObject _prefab) {
			this.muzzle = _prefab;
		}

		public void Init () {
			spawner = new SpawnerController (muzzle, .250f);
		}
		public void Shooting () {
			spawner.shouldActivate.Value = true;
		}
		public void Hits () {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 1000)) {
				if (hit.transform.tag == "Stone") {
					impact = new SpawnerController (impactParticle, 2000f);

				}
			}
		}
	}
}
