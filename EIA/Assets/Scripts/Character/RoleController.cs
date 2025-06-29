using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum AnimState
{
	Idle, //静止
	Walk, //移动
	Hit, //受击
	Sprint, //闪避
	Parry, //弹反
	Attack, //技能动作
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
	[Header("移动设置")]
	public float moveSpeed = 5f;        // 移动速度
	public float turnSmoothing = 0.1f;  // 转向平滑度

	[Header("闪避设置")]
	public float sprintSpeed = 10f; //冲刺速度
	public float sprintDistance = 5f; //冲刺距离
	public float sprintCost = 5f; //冲刺耗蓝

	[Header("能量设置")]
	public float powerRecover = 1f;
	public float maxPowerAmount = 6.0f; 

	[Header("弹反设置")]
	public float bounceCost = 3.0f;
	public float checkRadius = 3f; // 弹反子弹检测半径
	public float bounceRewardTime = 0.5f; //弹反成功无敌帧

	[Header("攻击设置")]
	public float attackCost = 3.0f;
	public float attackRadius = 3f; // 技能攻击半径

	public float curPower
	{
		get;
		private set;
	} = 0;

	private float sprinteMovement;
	public bool bounceLabel = false; //弹反标签
	private CharacterController controller;
	private Vector3 moveDirection;
	private PlayerAnimController playerAnimaController;          // 可选：用于动画控制
	private float turnVelocity;         // 转向速度缓存
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
			Debug.Log("Role当前能量不够释放攻击技能");
			PlayerEnergy.PlayerEnergyInstance.Shining();
			return;
		}
		curPower -= attackCost;
		UpdatePowerUI();
		RoleChangeState(RoleState.CastSpellUncontrollable);
		playerAnimaController.Attack();
		Debug.Log("Role正在攻击");

	}

	void CastSpellSprint()
	{
		if(curPower < sprintCost)
		{
			Debug.Log("Role当前能量不够释放冲刺技能");
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
		Debug.Log("Role正在闪避");

	}

	void HandleSpelSprinting()
	{
		// 如果已到达或非常接近目标点，结束冲刺
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
			Debug.Log("Role当前能量不够释放弹反技能");
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
		Debug.Log("Role正在被攻击");
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