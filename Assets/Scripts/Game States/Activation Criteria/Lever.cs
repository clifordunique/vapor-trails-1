﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ActivationCriteria {
	public bool flipped = false;
	public bool active = true;
	public string flagName;
	public bool flipToInactive = false;
	bool satisfied = false;

	bool hitTimeout = false;

	public override bool CheckSatisfied() {
		Animator anim;
		if ((anim = GetComponent<Animator>()) != null) {
			anim.SetBool("Active", active);
		}
		bool preSatisfied = this.satisfied;
		this.satisfied = false;
		return preSatisfied;
	}

	void ReEnableHitting() {
		hitTimeout = false;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (active && (other.GetComponent<PlayerAttack>() != null) && !hitTimeout) {
			hitTimeout = true;
			Invoke("ReEnableHitting", 1);
			PlayerAttack a = other.GetComponent<PlayerAttack>();
			if (a != null) {
				CameraShaker.Shake(a.cameraShakeIntensity, a.cameraShakeTime);
				SoundManager.HitSound();
				//instantiate the hitmarker
				if (a.hitmarker != null) {
					a.MakeHitmarker(this.transform);
				}
			}

			Flip();

			//then at the end of the flip, reset
			satisfied = true;
		}
	}

	public void Flip() {
		flipped = !flipped;
		if (flipToInactive) active = false;

		Animator anim;
		if ((anim = GetComponent<Animator>()) != null) {
			anim.SetBool("Flipped", flipped);
			if (flipToInactive) {
				anim.SetBool("Active", false);
			}
		}
	}

	public void FlipOff() {
		Flip();
		satisfied = false;
	}
}
