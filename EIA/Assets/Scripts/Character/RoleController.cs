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

	private float spriteMovement;
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
			UpdatePower();
			HandleMovement();
		}
		else if(tempState == AnimState.Sprint)
		{
			HandleSpelSprinting();
		}
		else if(tempState == AnimState.Parry && bounceLabel == true)
		{
			HandleSpellBouncing();
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

			// ����Ŀ��Ƕ�
			Debug.Log($"Target angle: {targetAngle}");

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
			Debug.Log("Role���ھ�ֹ");
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
		spriteMovement = sprintDistance;
		UpdatePowerUI();
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(false);
		Debug.Log("Role��������");
		playerAnimaController.Dodge();

	}

	void HandleSpelSprinting()
	{
		// ����ѵ����ǳ��ӽ�Ŀ��㣬�������
		if (spriteMovement > 0.1f)
		{
			spriteMovement -= sprintSpeed * Time.deltaTime;
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
		UpdatePowerUI();
		Debug.Log("Role���ڵ���");
		playerAnimaController.Parry();

	}

	//�������������¼�
	void OnSpellBounceEffect()
	{
		bounceLabel = true;
		HandleSpellBouncing();
	}

	//�������������¼�
	void OnSpellBounceEnd()
	{
		bounceLabel = false;
	}

	public void HandleSpellBouncing()
	{
		bool bounceSuccessTag = false;
		Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, 1 << GameContext.BulletLayer);
		foreach (Collider col in colliders)
		{
			Bullet bullet = col.GetComponent<Bullet>();
			if (bullet.GetBulletType() != BulletType.Big)
			{
				continue;
			}
			bullet.BounceBack();
			bounceSuccessTag = true;
		}
		if (bounceSuccessTag)
		{
			Map.MapInstance.SpatterOnMap(transform.position, attackRadius);
			OnSpellBounceEnd();
			playerAnimaController.Idle(true);
		}
	}

	public void OnBeingHit()
	{
		Debug.Log("Role���ڱ�����");
		var tempState = GetState();
		if(tempState == AnimState.Parry)
		{
			OnSpellBounceEnd();
		}
		playerAnimaController.Hit();
	}
}