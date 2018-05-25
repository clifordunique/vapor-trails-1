﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : Entity {

	[HideInInspector] public Rigidbody2D rb2d;

	public int hp;
	public int totalHP;
	public int moveForce;
	public int maxSpeed;

	public float healthChance = 2f;
	public float moneyChance = 0f;

	public GameObject healthPrefab, moneyPrefab;

	public GameObject playerObject;

	[HideInInspector] public Animator anim;
	[HideInInspector] public bool hasAnimator;

	[HideInInspector] public EnemyBehavior[] behaviors;

	Material whiteMaterial;
	Material defaultMaterial;
	bool white;

	bool dead = false;
	[HideInInspector] public bool invincible = false;

	public bool staggerable = true;
	public bool envDmgSusceptible = true;

	[HideInInspector] public SpriteRenderer spr;

	public bool burstOnDeath = false;
	public Transform burstEffect;

	bool stunned;

	private Coroutine unStunRoutine;

	public bool IsStunned() {
		return stunned;
	}

	void OnEnable() {
		totalHP = hp;
		rb2d = this.GetComponent<Rigidbody2D>();
		playerObject = GameObject.Find("Player");
		if ((anim = this.GetComponent<Animator>()) != null) {
			this.hasAnimator = true;
		}
		behaviors = this.GetComponents<EnemyBehavior>();

		spr = this.GetComponent<SpriteRenderer>();
		defaultMaterial = spr.material;
		whiteMaterial = Resources.Load<Material>("Shaders/WhiteFlash");
		Initialize();
	}

	public virtual void Initialize() {

	}

	public void DamageFor(int dmg) {
		this.hp -= dmg;
		if (this.hp <= 0 && !dead) {
			Die();
		}
	}

	public void OnHit(PlayerAttack attack) {
		WhiteSprite();
		DamageFor(attack.GetDamage());
		//compute potential stun
		StunFor(attack.GetStunLength());
		//compute potential knockback
		//unfreeze if this enemy is in hitstop to preserve the knockback vector
		if (inHitstop) {
			UnLockInSpace();
			inHitstop = false;
		}
		if (attack.knockBack) {
			KnockBack(attack.GetKnockback());
		}
	}

	public void Die(){
		CloseHurtboxes();
		this.frozen = true;
		this.dead = true;
		DropPickups();
		if (this.GetComponent<Animator>() != null && !burstOnDeath) {
			this.GetComponent<Animator>().SetTrigger("die");
		} else {
			if (burstEffect != null) {
				Burst();
			} else {
				Destroy();
			}
		}
	}

	//for each added behavior, call it
	public void Update() {
		if (!stunned) {
			foreach (EnemyBehavior eb in this.behaviors) {
				eb.Move();
			}
		}
		CheckFlip();
		if (white) {
			white = false;
			StartCoroutine(normalSprite());
		}
		ExtendedUpdate();
	}

	public virtual void ExtendedUpdate() {

	}

	public void DropPickups() {
		DropHealth();
		DropMoney();
	}

	public void DropHealth() {
		if (Random.Range(0f, 1f) < healthChance) {
			GameObject h = (GameObject) Instantiate(healthPrefab, this.transform.position, Quaternion.identity);
			h.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1, 1), Random.Range(3, 5));
		}
	}

	//on death, remove damage dealing even though it'll live a little bit while the dying animation finishes
	public void CloseHurtboxes() {
		foreach (Transform child in transform) {
			if (child.gameObject.tag.Equals(Tags.EnemyHurtbox)) {
				child.GetComponent<Collider2D>().enabled = false;
			}
		}
	}

	public void DropMoney() {
		if (Random.Range(0f, 1f) < moneyChance) {
			GameObject m = (GameObject) Instantiate(moneyPrefab, this.transform.position, Quaternion.identity);
			m.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-1, 1), Random.Range(3, 5));
		}
	}

	public void WhiteSprite() {
		white = true;
        spr.material = whiteMaterial;
    }

	IEnumerator normalSprite() {
		yield return new WaitForSeconds(.1f);
		spr.material = defaultMaterial;
	}

	public virtual void OnDamage() {

	}

	public void Burst() {
		Instantiate(burstEffect, this.transform.position, Quaternion.identity);
		Destroy();
	}

	public void StunFor(float seconds) {
		if (staggerable) {
			//if the enemy is already stunned, then resstart the stun period
			if (stunned) {
				StopCoroutine(unStunRoutine);
				unStunRoutine = StartCoroutine(WaitAndUnStun(seconds));
			} else {
				stunned = true;
				anim.SetBool("Stunned", true);
				unStunRoutine = StartCoroutine(WaitAndUnStun(seconds));
			}
		}
	}

	public void KnockBack(Vector2 kv) {
		if (staggerable) {
			rb2d.velocity = kv;
		}
	}

	IEnumerator WaitAndUnStun(float seconds) {
		yield return new WaitForSeconds(seconds);
		stunned = false;
		anim.SetBool("Stunned", false);
	}
}