using UnityEngine;

namespace ThirdPersonShooter {
	public class AnimateController {
		private Animator animator;
		private AnimatorStateInfo stateInfo;
		public Animator Animate {
			get { return this.animator; }
			set { this.animator = value; }
		}
		public AnimateController (Animator _anim) {
			Animate = _anim;
		}

		public void CharacterAnimateMovement (float _inputtedValue, bool _run) {
			bool controller = true ? (_inputtedValue != 0) : false;
			switch (controller) {
			case true:
				if (_inputtedValue > 0) {
					Animate.SetBool ("Walk_Forward", true);
				}
				else
					Animate.SetBool ("Walk_Backward", true);
				break;
			case false:
				Animate.SetBool ("Walk_Forward", false);
				Animate.SetBool ("Walk_Backward", false);
				break;
			}
		}
		public void BoolAnim (string s, bool value) {
			Animate.SetBool (s, value);
		}
	}
}
