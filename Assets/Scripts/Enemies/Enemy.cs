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

	public float moneyChance = 0f;

	public GameObject moneyPrefab;

	public GameObject playerObject;

	[HideInInspector] public Animator anim;
	[HideInInspector] public bool hasAnimator;

	[HideInInspector] public EnemyBehavior[] behaviors;

	Material whiteMaterial;
	Material defaultMaterial;
	bool white;

	bool dead = false;

	[HideInInspector] public SpriteRenderer spr;
	List<SpriteRenderer> spriteRenderers;

	public bool burstOnDeath = false;
	public Transform burstEffect;

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
		
		whiteMaterial = Resources.Load<Material>("Shaders/WhiteFlash");
		spriteRenderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(includeInactive:true));
		if (spr != null) {
			defaultMaterial = spr.material;
		} else {
			defaultMaterial = spriteRenderers[0].material;
		}
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

	public override void OnHit(Attack attack) {
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
		CameraShaker.SmallShake();
		DropPickups();
		if (this.GetComponent<Animator>() != null && !burstOnDeath) {
			this.GetComponent<Animator>().SetTrigger("Die");
		} else {
			if (burstEffect != null) {
				Burst();
			} else {
				if (GetComponent<SelfDestruct>() == null) {
					Destroy();
				}
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
		DropMoney();
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
		foreach (SpriteRenderer x in spriteRenderers) {
			x.material = whiteMaterial;
		}
		if (anim != null) {
			anim.SetBool("WhiteSprite", true);
		}
		if (spr != null) {
        	spr.material = whiteMaterial;
		}
    }

	IEnumerator normalSprite() {
		yield return new WaitForSeconds(.1f);
		spriteRenderers.ForEach(x => {
			x.material = defaultMaterial;
		});
		if (spr != null) {
        	spr.material = defaultMaterial;
		}
		if (anim != null) {
			anim.SetBool("WhiteSprite", false);
		}
	}

	public virtual void OnDamage() {

	}

	public void Burst() {
		Instantiate(burstEffect, this.transform.position, Quaternion.identity);
		Destroy();
	}

	public override void OnGroundHit() {
		anim.SetBool("Grounded", true);
		foreach (EnemyBehavior eb in this.behaviors) {
			eb.OnGroundHit();
		}
	}

	public override void OnGroundLeave() {
		anim.SetBool("Grounded", false);
		foreach (EnemyBehavior eb in this.behaviors) {
			eb.OnGroundLeave();
		}
	}
}