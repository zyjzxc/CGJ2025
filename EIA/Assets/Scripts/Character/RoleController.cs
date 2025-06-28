using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoleState
{
	Idle,
	OutOfControl
}

[RequireComponent(typeof(CharacterController))]
public class RoleController : MonoBehaviour
{
	[Header("移动设置")]
	public float moveSpeed = 5f;        // 移动速度
	public float turnSmoothing = 0.1f;  // 转向平滑度

	public RoleState currentstate = RoleState.Idle;
	private CharacterController controller;
	private Vector3 moveDirection;
	private Animator animator;          // 可选：用于动画控制
	private float turnVelocity;         // 转向速度缓存

	void Start()
	{
		controller = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
	}

	void Update()
	{
		if(currentstate == RoleState.Idle)
		{
			HandleMovement();
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
			if (animator != null)
			{
				animator.SetFloat("Speed", movement.magnitude);
			}
		}
		else
		{
			// 停止移动时的动画控制
			if (animator != null)
			{
				animator.SetFloat("Speed", 0);
			}
		}
	}

	public void ChangeRoleState()
	{
		if (currentstate == RoleState.Idle)
		{
			currentstate = RoleState.OutOfControl;
		}
		else
		{
			currentstate = RoleState.Idle;
		}
	}

}