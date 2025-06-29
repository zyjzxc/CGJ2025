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

	//动画注册攻击技能生效事件
	public void SpellAttackHit()
	{
		Map.MapInstance.SpatterOnMap(RoleController.Instance.transform.position, RoleController.Instance.attackRadius);
	}

	//冲刺结束事件
	public void OnSpellSprintEnd()
	{
		RoleController.Instance.SpellSprintEnd();
	}

	//动画开启弹反事件
	public void OnSpellBounceEffect()
	{
		RoleController.Instance.SpellBounceEffect();
	}

	//动画结束弹反事件
	public void OnSpellBounceEnd()
	{
		RoleController.Instance.SpellBounceEnd();
	}

	public void OnSpellSpellHitEnd()
	{
		RoleController.Instance.OnSpellHitEnd();
	}
}
