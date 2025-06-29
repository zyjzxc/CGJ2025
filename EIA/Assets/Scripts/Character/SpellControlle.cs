using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class SpellController : MonoBehaviour
{
	public static SpellController  spellController;

	private void Awake()
	{
		spellController = this;
	}

	void Start()
	{
	}

	//����ע�ṥ��������Ч�¼�
	public void SpellAttackHit()
	{
		Map.MapInstance.SpatterOnMap(RoleController.Instance.transform.position, RoleController.Instance.attackRadius);
	}

	//��̽����¼�
	public void OnSpellSprintEnd()
	{
		RoleController.Instance.SpellSprintEnd();
	}

	//�������������¼�
	public void OnSpellBounceEffect()
	{
		RoleController.Instance.SpellBounceEffect();
	}

	//�������������¼�
	public void OnSpellBounceEnd()
	{
		RoleController.Instance.SpellBounceEnd();
	}

	public void OnSpellSpellHitEnd()
	{
		RoleController.Instance.OnSpellHitEnd();
	}
}
