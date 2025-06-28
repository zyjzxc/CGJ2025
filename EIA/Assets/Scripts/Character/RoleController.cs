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
	[Header("�ƶ�����")]
	public float moveSpeed = 5f;        // �ƶ��ٶ�
	public float turnSmoothing = 0.1f;  // ת��ƽ����

	public RoleState currentstate = RoleState.Idle;
	private CharacterController controller;
	private Vector3 moveDirection;
	private Animator animator;          // ��ѡ�����ڶ�������
	private float turnVelocity;         // ת���ٶȻ���

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