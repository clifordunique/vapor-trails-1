﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public bool impactShake;
	public GameObject burstPrefab;

	GameObject impactDust;

	public bool ignorePlayer = false;

	void Start() {
		this.transform.parent = null;
		impactDust = (GameObject) Resources.Load("ImpactDustPrefab");
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (ignorePlayer && other.tag.ToLower().Contains("player")) {
			return;
		}

		if (other.CompareTag(Tags.EnemyHitbox)) {
			return;
		}

		if (impactShake) {
			CameraShaker.TinyShake();
		}
		OnImpact(other);
	}

	public void OnImpact(Collider2D other) {
		if (burstPrefab != null) {
			GameObject go = Instantiate(burstPrefab, transform.position, Quaternion.identity);
		}

		if (other.GetComponent<PlayerAttack>() != null) {
			other.GetComponent<PlayerAttack>().OnDeflect();
			SoundManager.HitSound();
			Vector2 originalMotion = this.GetComponent<Rigidbody2D>().velocity;
			Vector2 flipped = Vector2.Reflect(originalMotion, Vector2.up);
			float newAngle = Vector2.Angle(Vector2.left, flipped);
			GameObject g = (GameObject) Instantiate(impactDust, this.transform.position, Quaternion.Euler(0, 0, newAngle), null);
		} else {
			RaycastHit2D hit = Physics2D.CircleCast(
				this.transform.position, 
				0.5f, 
				Vector2.up, 
				0, 
				1 << LayerMask.NameToLayer(Layers.Ground) | 1 << LayerMask.NameToLayer(Layers.HitHurtboxes));
			if (hit.transform != null) {
				Vector2 originalMotion = this.GetComponent<Rigidbody2D>().velocity;
				Vector2 flipped = Vector2.Reflect(originalMotion, hit.normal);
				float newAngle = Vector2.Angle(Vector2.left, flipped);
				GameObject g = (GameObject) Instantiate(impactDust, hit.point, Quaternion.Euler(0, 0, newAngle+90), null);
			}
		}

		GetComponent<Rigidbody2D>().velocity = Vector3.zero;
		SoundManager.ExplosionSound();
		//remove the collider/sprites/etc and stop particle emission
		GetComponent<Collider2D>().enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<SelfDestruct>().Destroy(2f);
	}
}
