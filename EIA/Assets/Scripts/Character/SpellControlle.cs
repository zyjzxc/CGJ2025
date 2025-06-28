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

	//����ע�ṥ��������Ч�¼�
	public void SpellAttackHit()
	{
		Map.MapInstance.SpatterOnMap(RoleController.Instance.transform.position, RoleController.Instance.attackRadius);
	}

	//��̽����¼�
	public void OnSpellSprintEnd()
	{
		PlayerHealth.PlayerHealthInstance.ModifyDamageLabel(true);
	}

	//�������������¼�
	public void OnSpellBounceEffect()
	{
		RoleController.Instance.bounceLabel = true;
		RoleController.Instance.bounceMoveTag = true;
	}

	//�������������¼�
	public void OnSpellBounceEnd()
	{
		RoleController.Instance.bounceLabel = false;
	}
}
