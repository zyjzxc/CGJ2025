using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum AnimState
{
	Idle, //��ֹ
	Walk, //�ƶ�
	Hit, //�ܻ�
	Sprint, //����
	Parry, //����
	Attack, //���ܶ���
	None
}

public enum RoleState 
{
	None,
	Idle,
	CastSpellUncontrollable,
	CastSpellMoveable,
	CastSpellStopable
}

[RequireComponent(typeof(CharacterController))]
public class RoleController : MonoBehaviour
{
	[Header("�ƶ�����")]
	public float moveSpeed = 5f;        // �ƶ��ٶ�
	public float turnSmoothing = 0.1f;  // ת��ƽ����

	[Header("��������")]
	public float sprintSpeed = 10f; //����ٶ�
	public float sprintDistance = 5f; //��̾���
	public float sprintCost = 5f; //��̺���

	[Header("��������")]
	public float powerRecover = 1f;
	public float maxPowerAmount = 6.0f; 

	[Header("��������")]
	public float bounceCost = 3.0f;
	public float checkRadius = 3f; // �����ӵ����뾶
	public float bounceRewardTime = 0.5f; //�����ɹ��޵�֡

	[Header("��������")]
	public float attackCost = 3.0f;
	public float attackRadius = 3f; // ���ܹ����뾶

	public float curPower
	{
		get;
		private set;
	} = 0;

	private float sprinteMovement;
	public bool bounceLabel = false; //������ǩ
	private CharacterController controller;
	private Vector3 moveDirection;
	private PlayerAnimController playerAnimaController;          // ��ѡ�����ڶ�������
	private float turnVelocity;         // ת���ٶȻ���
	private RoleState roleState = RoleState.Idle;

	public Dictionary<string, AnimState> AnimStateMap = new Dictionary<string, AnimState>
	{
		{ "idle" ,AnimState.Idle},
		{ "walk" ,AnimState.Walk},
		{ "hit",AnimState.Hit },
		{ "dodge",AnimState.Sprint },
		{ "parry" ,AnimState.Parry},
		{ "attack",AnimState.Attack}
	};
	public Dictionary<RoleState, List<RoleState>> RoleStateMap = new Dictionary<RoleState, List<RoleState>> 
	{
		{RoleState.Idle, new List<RoleState> {RoleState.CastSpellUncontrollable} },
		{RoleState.CastSpellUncontrollable, new List<RoleState> {RoleState.CastSpellMoveable, RoleState.Idle } },
		{RoleState.CastSpellMoveable, new List<RoleState> {RoleState.Idle, RoleState.CastSpellUncontrollable} },
	};
	public static RoleController Instance;

	public void RoleChangeState(RoleState targetState)
	{
		if (RoleStateMap[roleState].Contains(targetState) == false)
		{
			return;
		}
		roleState = targetState;
	}

	private void Awake()
	{
		Instance = this;
		Time.fixedDeltaTime = 0.02f;
	}

	void Start()
	{
		controller = GetComponent<CharacterController>();
		playerAnimaController = transform.Find("Player_Tpose").GetComponent<PlayerAnimController>();
	}

	void FixedUpdate()
	{
		if(GameContext.GameFinish)
			return;
		
		var tempState = GetState();

		if (roleState == RoleState.CastSpellUncontrollable)
		{
			if(tempState == AnimState.Sprint)
			{
				HandleSpelSprinting();
			}
			return;
		}
		else if(roleState == RoleState.CastSpellMoveable)
		{
            if (tempState == AnimState.Parry)
            {
				HandleSpellBouncing();
            }
            HandleInput(true);
		}
		else if(roleState == RoleState.Idle)
		{
			UpdatePower();
			HandleInput(false);
		}
	}

	void HandleInput(bool onlyMove)
	{
		HandleMovement();
		if (onlyMove)
		{
			return;
		}
		if (Input.GetMouseButton(0))
		{
			CastSpellBounce();
		}
		else if (Input.GetKey(KeyCode.Space))
		{
			CastSpellSprint();
		}
		else if (Input.GetMouseButton(1))
		{
			CastSpellAttack();
		}
	}

	void HandleMovement()
	{
		// ��ȡWASD����
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// �����ƶ������������������ϵ��
		Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

		if (movement.magnitude > 0.1f)
		{
			// ����Ŀ�곯��Ƕ�
			float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

			// ƽ��ת��
			float angle = Mathf.SmoothDampAngle(
				transform.eulerAngles.y,
				targetAngle,
				ref turnVelocity,
				turnSmoothing
			);

			// Ӧ����ת
			transform.rotation = Quaternion.Euler(0, angle, 0);

			// Ӧ���ƶ�
			moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
			controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
			playerAnimaController.Walk();
		}
		else
		{
			playerAnimaController.Idle();
		}
	}

	private AnimState GetState()
	{
		foreach(var kvp in AnimStateMap)
		{
			if (playerAnimaController.IsState(kvp.Key))
			{
				return kvp.Value;
			}
		}
		return AnimState.None;
	}
	void UpdatePower()
	{
		curPower = Math.Min(curPower + powerRecover * Time.deltaTime, maxPowerAmount);
		UpdatePowerUI();
	}

	void UpdatePowerUI()
	{
		if (curPower < 0 || curPower > maxPowerAmount)
		{
			Debug.LogError($"Power Add wrong curPower {curPower} and maxPowerAmount {maxPowerAmount}");
		}
		PlayerEnergy.PlayerEnergyInstance.UpdateBar(curPower, curPower / maxPowerAmount);
	}

	void CastSpellAttack()
	{
		if (curPower < attackCost)
		{
			Debug.Log("Role��ǰ���������ͷŹ�������");
			PlayerEnergy.PlayerEnergyInstance.Shining();
			return;
		}
		curPower -= attackCost;
		UpdatePowerUI();
		RoleChangeState(RoleState.CastSpellUncontrollable);
		playerAnimaController.Attack();
		Debug.Log("Role���ڹ���");

	}

	void CastSpellSprint()
	{
		if(curPower < sprintCost)
		{
			Debug.Log("Role��ǰ���������ͷų�̼���");
			PlayerEnergy.PlayerEnergyInstance.Shining();
			return;
		}
		curPower = curPower-sprintCost;
		sprinteMovement = sprintDistance;
		UpdatePowerUI();
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(false);
		RoleChangeState(RoleState.CastSpellUncontrollable);
		AudioMgr.Instance.PlayVoice("dodge");
		playerAnimaController.Dodge();
		Debug.Log("Role��������");

	}

	void HandleSpelSprinting()
	{
		// ����ѵ����ǳ��ӽ�Ŀ��㣬�������
		if (sprinteMovement > 0.1f)
		{
			sprinteMovement -= sprintSpeed * Time.deltaTime;
			Vector3 move = moveDirection * sprintSpeed * Time.deltaTime;
			controller.Move(move);
		}
		else
		{
			SpellSprintEnd();
			playerAnimaController.Idle(true);
		}
	}

	public void SpellSprintEnd()
	{
		RoleChangeState(RoleState.Idle);
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(true);
	}

	void CastSpellBounce()
	{
		if(curPower < bounceCost)
		{
			Debug.Log("Role��ǰ���������ͷŵ�������");
			PlayerEnergy.PlayerEnergyInstance.Shining();
			return;
		}
		curPower -= bounceCost;
		UpdatePowerUI();
		RoleChangeState(RoleState.CastSpellUncontrollable);
		AudioMgr.Instance.PlayVoice("bouncecast");
		playerAnimaController.Parry();
	}

	public void SpellBounceEffect()
	{
		RoleController.Instance.bounceLabel = true;
		RoleChangeState(RoleState.CastSpellMoveable);
	}

	public void SpellBounceEnd()
	{
		RoleController.Instance.bounceLabel = false;
		RoleChangeState(RoleState.Idle);
	}

	public void HandleSpellBouncing()
	{
		bool bounceSuccessTag = false;
		Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, 1 << GameContext.BulletLayer);
		foreach (Collider col in colliders)
		{
			Bullet bullet = col.GetComponent<Bullet>();
			if (bullet.BounceBack())
			{
				PlayEffect("fx_ParrySuccess", bullet.transform);
				bounceSuccessTag = true;
			}
		}
		if (bounceSuccessTag)
		{
			OnBouncingSuccess();
		}
	}

	void OnBouncingSuccess()
	{
		Map.MapInstance.SpatterOnMap(transform.position, attackRadius);
		ScreenShake.Instance.Shake(0);
		playerAnimaController.ApplyTimeDilation(0);
		curPower = Math.Min(curPower + bounceCost, maxPowerAmount);
		UpdatePowerUI();
		PlayerHealth.PlayerHealthInstance.SetInvincibleTime(bounceRewardTime);
	}

	public void PlayEffect(string effectName, Transform tempTransform)
	{
		var effectPrefab = Resources.Load<GameObject>($"Effect/{effectName}");
		Debug.Log($"Play Effect Assets/Resources/Effect/{effectName}");
		var currentEffect = Instantiate(effectPrefab, tempTransform.position, tempTransform.rotation);
		currentEffect.transform.position = tempTransform.position;
		currentEffect.transform.rotation = tempTransform.rotation;

		float effectDuration = currentEffect.GetComponent<ParticleSystem>()?.main.duration ?? 5f;
		Destroy(currentEffect, effectDuration);
	}

	public void OnBeingHit()
	{
		Debug.Log("Role���ڱ�����");
		var tempState = GetState();
		if(tempState == AnimState.Parry)
		{
			SpellBounceEnd();
		}
		RoleChangeState(RoleState.CastSpellUncontrollable);
		playerAnimaController.Hit();
	}

	public void OnSpellHitEnd()
	{
		RoleChangeState(RoleState.Idle);
	}
}