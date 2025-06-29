using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	[Header("��������")]
	public float attackCost = 3.0f;
	public float attackRadius = 3f; // ���ܹ����뾶

	public float curPower
	{
		get;
		private set;
	} = 0;

	private float sprinteMovement;
	public bool bounceMoveTag;
	public bool bounceLabel = false; //������ǩ
	private CharacterController controller;
	private Vector3 moveDirection;
	private PlayerAnimController playerAnimaController;          // ��ѡ�����ڶ�������
	private float turnVelocity;         // ת���ٶȻ���

	public Dictionary<string, AnimState> AnimStateMap = new Dictionary<string, AnimState>
	{
		{ "idle" ,AnimState.Idle},
		{ "walk" ,AnimState.Walk},
		{ "hit",AnimState.Hit },
		{ "dodge",AnimState.Sprint },
		{ "parry" ,AnimState.Parry},
		{ "attack",AnimState.Attack}
	};
	public static RoleController Instance;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		controller = GetComponent<CharacterController>();
		playerAnimaController = transform.Find("Player_Tpose").GetComponent<PlayerAnimController>();
	}

	void Update()
	{
		if(GameContext.GameFinish)
			return;
		
		var tempState = GetState();
		if (tempState == AnimState.Idle || tempState == AnimState.Walk  ||  tempState == AnimState.Attack)
		{
			// �ȼ���Ƿ��¼��ܼ�
			if (Input.GetMouseButtonDown(0))
			{
				CastSpellAttack();
				//return; // ���¼��ܼ����ٴ��������߼�
			}

			if (Input.GetMouseButtonDown(1))
			{
				CastSpellBounce();
				return; // ���¼��ܼ����ٴ��������߼�
			}

			if (Input.GetKeyDown(KeyCode.Space)) // �ո��
			{
				CastSpellSprint();
				return;
			}
		}
		else if(tempState == AnimState.Sprint)
		{
			HandleSpelSprinting();
			return;
		}
		else if(tempState == AnimState.Parry)
		{
			if (bounceMoveTag == false)
			{
				return;
			}
			if (bounceLabel == true) 
			{
				HandleSpellBouncing();
			}
		}
		UpdatePower();
		HandleMovement();
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
			return;
		}
		curPower -= attackCost;
		UpdatePowerUI();
		playerAnimaController.Attack();
		Debug.Log("Role���ڹ���");

	}

	void CastSpellSprint()
	{
		if(curPower < sprintCost)
		{
			Debug.Log("Role��ǰ���������ͷų�̼���");
			return;
		}
		curPower = curPower-sprintCost;
		sprinteMovement = sprintDistance;
		UpdatePowerUI();
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(false);
		Debug.Log("Role��������");
		playerAnimaController.Dodge();

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
			OnSpellSprintEnd();
			playerAnimaController.Idle(true);
		}
	}

	void OnSpellSprintEnd()
	{
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(true);
	}

	void CastSpellBounce()
	{
		if(curPower < bounceCost)
		{
			Debug.Log("Role��ǰ���������ͷŵ�������");
			return;
		}
		curPower -= bounceCost;
		bounceMoveTag = false;
		UpdatePowerUI();
		playerAnimaController.Parry();
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
			SpellController.spellController.OnSpellBounceEnd();
		}
		playerAnimaController.Hit();
	}
}