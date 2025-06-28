using System.Collections;
using System.Collections.Generic;
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
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(true);
	}

	//动画开启弹反事件
	public void OnSpellBounceEffect()
	{
		RoleController.Instance.bounceLabel = true;
		RoleController.Instance.bounceMoveTag = true;
	}

	//动画结束弹反事件
	public void OnSpellBounceEnd()
	{
		RoleController.Instance.bounceLabel = false;
	}
}
