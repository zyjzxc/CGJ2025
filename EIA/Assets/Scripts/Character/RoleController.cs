using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoleState
{
	Idle,       // 静止or移动
	Sprinting,  // 冲刺
	BouncingPrepare, //弹反蓄力
	Bouncing   // 弹反
}


[RequireComponent(typeof(CharacterController))]
public class RoleController : MonoBehaviour
{
	[Header("移动设置")]
	public float moveSpeed = 5f;        // 移动速度
	public float turnSmoothing = 0.1f;  // 转向平滑度

	[Header("闪避设置")]
	public float sprintSpeed = 10f; //冲刺速度
	public float sprintDistance = 5f; //冲刺距离

	[Header("能量设置")]
	public float powerRecover = 0.1f;
	public float sprintPowerCost = 2.0f;
	public float minSprintPowerCost = 1.0f;
	public float maxPowerAmount = 2.0f; 

	[Header("弹反设置")]
	public float bounceCost = 3.0f;
	public float checkRadius = 3f; // 弹反子弹检测半径

	private float curPower = 0;
	private float bouncingMana = 0;
	private bool bounceLabel = false; //弹反标签
	private RoleState currentstate = RoleState.Idle;
	private CharacterController controller;
	private Vector3 moveDirection;
	private PlayerAnimController playerAnimaController;          // 可选：用于动画控制
	private PlayerHealth playerHealth;
	private float turnVelocity;         // 转向速度缓存
	
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
		//计算弹反的CD
		UpdateMana();

		if(currentstate == RoleState.Idle)
		{
			UpdatePower();
			// 先检查是否按下技能键
			if (Input.GetKeyDown(KeyCode.J))
			{
				CastSpellSprint();
				return; // 按下技能键后不再处理其他逻辑
			}

			if (Input.GetKeyDown(KeyCode.K))
			{
				CastSpellBounce();
				return; // 按下技能键后不再处理其他逻辑
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
		// 获取WASD输入
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		// 计算移动方向（相对于世界坐标系）
		Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

		if (movement.magnitude > 0.1f)
		{
			// 计算目标朝向角度
			float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;

			// 平滑转向
			float angle = Mathf.SmoothDampAngle(
				transform.eulerAngles.y,
				targetAngle,
				ref turnVelocity,
				turnSmoothing
			);

			// 应用旋转
			transform.rotation = Quaternion.Euler(0, angle, 0);

			// 应用移动
			moveDirection = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
			controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);

			// 控制动画（如果有Animator组件）
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
		//能量条UI更新
	}

	void CastSpellSprint()
	{
		if(curPower < minSprintPowerCost)
		{
			Debug.Log("冲刺技能能量不够释放");
			return;
		}
		curPower = Math.Max(curPower-sprintPowerCost, 0);
		playerHealth.ModifyDamageLabel();
		//播放冲刺动画
	}

	void HandleSpelSprinting()
	{
		Vector3 move = transform.forward * sprintSpeed * Time.deltaTime;

		// 使用 CharacterController 进行移动
		controller.Move(move);
	}

	//动画注册无敌事件结束
	void OnSpellSprintEnd()
	{
		playerHealth.ModifyDamageLabel();
	}

	void CastSpellBounce()
	{
		if(bouncingMana < bounceCost)
		{
			Debug.Log("弹反技能CD没好");
			return;
		}
		bouncingMana -= bounceCost;
		playerAnimaController.Parry();
	}

	//动画开启弹反事件
	void CastSpellBouncing()
	{
		bounceLabel = true;
		HandleSpellBouncing();
	}

	//动画结束弹反事件
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