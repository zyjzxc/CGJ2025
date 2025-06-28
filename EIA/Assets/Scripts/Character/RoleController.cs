using System.Collections;
using System.Collections.Generic;
using System;
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
	public float powerCost = 1.0f;
	public float maxPowerAmount = 2.0f;
	public static float powerInitialValue = 1.0f;

	[Header("��������")]
	public static float bounceCost = 3.0f;
	public float checkRadius = 3f; // �����ӵ����뾶

	public float curPower = powerInitialValue;
	public float bouncingMana = bounceCost;
	public RoleState currentstate = RoleState.Idle;
	private CharacterController controller;
	private Vector3 moveDirection;
	private Animator animator;          // ��ѡ�����ڶ�������
	private PlayerHealth playerHealth;
	private float turnVelocity;         // ת���ٶȻ���
	private Vector3 targetPos; //���Ŀ�ĵػ���

	void Start()
	{
		controller = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		playerHealth = GetComponent<PlayerHealth>();
	}

	void Update()
	{
		UpdataMana();
		if(currentstate == RoleState.Idle)
		{
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
		else if(currentstate == RoleState.BouncingPrepare)
		{
			HanldeTestBouncing();
		}
		
	}

	void HandleMovement()
	{
		curPower += powerRecover;
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
			if (animator != null)
			{
				animator.SetFloat("Speed", movement.magnitude);
			}
		}
		else
		{
			// ֹͣ�ƶ�ʱ�Ķ�������
			if (animator != null)
			{
				animator.SetFloat("Speed", 0);
			}
		}
	}

	void UpdataMana()
	{
		bouncingMana = Math.Min(bounceCost, bouncingMana + Time.deltaTime); 
	}

	void CastSpellSprint()
	{
		if(curPower < powerCost)
		{
			return;
		}
		curPower -= powerCost;
		playerHealth.ModifyDamageLabel();
		currentstate = RoleState.Sprinting;
		//���ų�̶���
	}

	void HandleSpelSprinting()
	{
		//
	}

	void OnSpellSprintEnd()
	{
		playerHealth.ModifyDamageLabel();
		ResetRoleState();
	}

	void CastSpellBounce()
	{
		if(bouncingMana < bounceCost)
		{
			return;
		}
		//���Ŷ�����ʼ��������
		currentstate = RoleState.BouncingPrepare;
	}

	//�������������¼�
	void CastSpellBouncing()
	{
		currentstate = RoleState.Bouncing;
		HandleSpellBouncing();
	}

	//�������������¼�
	void EndSpellBouncing()
	{
		ResetRoleState();
	}

	void HandleSpellBouncing()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, GameContext.BulletLayer);
		foreach (Collider col in colliders)
		{
			Bullet bullet = col.GetComponent<Bullet>();
			if (bullet != null)
			{

			}
		}

	}

	void HanldeTestBouncing()
	{
		
	}

	public void ResetRoleState()
	{
		currentstate = RoleState.Idle;
	}
}