using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SliderSlotMarkController : MonoBehaviour
{ 
	[SerializeField] private RectTransform slotMarkPrefab;     // 槽位标记预制体（隐藏状态下使用）
	[SerializeField] public List<float> markPercents;         // 标记百分比（0~1）

	private RectTransform sliderRect;
	private List<Image> markImages = new List<Image>();

	void Start()
	{
		sliderRect = GetComponent<RectTransform>();

		for (int i = 0; i < markPercents.Count; i++)
		{
			float percent = markPercents[i];

			// 实例化标记并设置为 active
			RectTransform mark = Instantiate(slotMarkPrefab, slotMarkPrefab.parent);
			mark.gameObject.SetActive(true);

			// 锚点设为左中，便于定位
			mark.anchorMin = new Vector2(0, 0.5f);
			mark.anchorMax = new Vector2(0, 0.5f);
			mark.pivot = new Vector2(0.5f, 0.5f);

			float width = sliderRect.rect.width;
			mark.anchoredPosition = new Vector2(width * percent, 0);

			// 设置默认颜色并记录
			Image img = mark.GetComponent<Image>();
			if (img != null)
			{
				markImages.Add(img);
			}
		}

		// 隐藏模板
		slotMarkPrefab.gameObject.SetActive(false);
	}

	/// <summary>
	/// 外部调用：设置指定索引标记的颜色
	/// </summary>
	public void SetSlotColor(int index, Color color)
	{
		if (index >= 0 && index < markImages.Count)
		{
			markImages[index].color = color;
		}
		else
		{
			Debug.LogWarning($"[SliderSlotMarkController] Slot index {index} out of range.");
		}
	}
}
