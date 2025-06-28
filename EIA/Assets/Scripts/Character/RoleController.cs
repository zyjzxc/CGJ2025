using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoleState
{
	Idle,       // ��ֹor�ƶ�
	Sprinting,  // ���
	BouncingPrepare, //��������
	Bouncing   // ����
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

	[Header("��������")]
	public float powerRecover = 0.1f;
	public float sprintPowerCost = 2.0f;
	public float minSprintPowerCost = 1.0f;
	public float maxPowerAmount = 2.0f; 

	[Header("��������")]
	public float bounceCost = 3.0f;
	public float checkRadius = 3f; // �����ӵ����뾶

	private float curPower = 0;
	private float bouncingMana = 0;
	private bool bounceLabel = false; //������ǩ
	private RoleState currentstate = RoleState.Idle;
	private CharacterController controller;
	private Vector3 moveDirection;
	private PlayerAnimController playerAnimaController;          // ��ѡ�����ڶ�������
	private PlayerHealth playerHealth;
	private float turnVelocity;         // ת���ٶȻ���
	
	public static RoleController Instance;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		controller = GetComponent<CharacterController>();
		playerAnimaController = GetComponent<PlayerAnimController>();
		playerHealth = GetComponent<PlayerHealth>();
		bouncingMana = bounceCost;
	}

	void Update()
	{
		//���㵯����CD
		UpdateMana();

		if(currentstate == RoleState.Idle)
		{
			UpdatePower();
			// �ȼ���Ƿ��¼��ܼ�
			if (Input.GetKeyDown(KeyCode.J))
			{
				CastSpellSprint();
				return; // ���¼��ܼ����ٴ��������߼�
			}

			if (Input.GetKeyDown(KeyCode.K))
			{
				CastSpellBounce();
				return; // ���¼��ܼ����ٴ��������߼�
			}
			HandleMovement();
		}
		else if(currentstate == RoleState.Sprinting)
		{
			HandleSpelSprinting();
		}
		else if(currentstate == RoleState.Bouncing)
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

			// ���ƶ����������Animator�����
			playerAnimaController.Walk();
		}
		else
		{
			playerAnimaController.Idle();
		}
	}

	void UpdateMana()
	{
		bouncingMana = Math.Min(bounceCost, bouncingMana + Time.deltaTime); 
	}

	void UpdatePower()
	{
		//������UI����
	}

	void CastSpellSprint()
	{
		if(curPower < minSprintPowerCost)
		{
			Debug.Log("��̼������������ͷ�");
			return;
		}
		curPower = Math.Max(curPower-sprintPowerCost, 0);
		playerHealth.ModifyDamageLabel();
		//���ų�̶���
	}

	void HandleSpelSprinting()
	{
		Vector3 move = transform.forward * sprintSpeed * Time.deltaTime;

		// ʹ�� CharacterController �����ƶ�
		controller.Move(move);
	}

	//����ע���޵��¼�����
	void OnSpellSprintEnd()
	{
		playerHealth.ModifyDamageLabel();
	}

	void CastSpellBounce()
	{
		if(bouncingMana < bounceCost)
		{
			Debug.Log("��������CDû��");
			return;
		}
		bouncingMana -= bounceCost;
		playerAnimaController.Parry();
	}

	//�������������¼�
	void CastSpellBouncing()
	{
		bounceLabel = true;
		HandleSpellBouncing();
	}

	//�������������¼�
	void EndSpellBouncing()
	{
		bounceLabel = false;
	}

	void HandleSpellBouncing()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, GameContext.BulletLayer);
		foreach (Collider col in colliders)
		{
			Bullet bullet = col.GetComponent<Bullet>();
			if (bullet.GetBulletType() != BulletType.Big)
			{
				continue;
			}
			bullet.BounceBack();
		}

	}
}