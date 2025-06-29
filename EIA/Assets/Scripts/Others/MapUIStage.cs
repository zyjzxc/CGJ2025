using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SliderSlotMarkController : MonoBehaviour
{ 
	[SerializeField] private RectTransform slotMarkPrefab;     // ��λ���Ԥ���壨����״̬��ʹ�ã�
	[SerializeField] public List<float> markPercents;         // ��ǰٷֱȣ�0~1��

	private RectTransform sliderRect;
	private List<Image> markImages = new List<Image>();
	private int colorProgressIndex = 0;
	private float checkInterval = 0.3f; // ÿ�� 0.2 ����һ��
	private float checkTimer = 0f;

	void Start()
	{
		sliderRect = GetComponent<RectTransform>();

		for (int i = 0; i < markPercents.Count; i++)
		{
			float percent = markPercents[i];

			// ʵ������ǲ�����Ϊ active
			RectTransform mark = Instantiate(slotMarkPrefab, slotMarkPrefab.parent);
			mark.gameObject.SetActive(true);

			// ê����Ϊ���У����ڶ�λ
			mark.anchorMin = new Vector2(0, 0.5f);
			mark.anchorMax = new Vector2(0, 0.5f);
			mark.pivot = new Vector2(0.5f, 0.5f);

			float width = sliderRect.rect.width;
			mark.anchoredPosition = new Vector2(width * percent, 0);

			// ����Ĭ����ɫ����¼
			Image img = mark.GetComponent<Image>();
			if (img != null)
			{
				markImages.Add(img);
			}
		}
		colorProgressIndex = 0;
		// ����ģ��
		slotMarkPrefab.gameObject.SetActive(false);
	}

	private void Update()
	{
		checkTimer += Time.deltaTime;

		if (checkTimer >= checkInterval)
		{
			checkTimer = 0f; // ���ü�ʱ��

			float cur = Map.MapInstance.CurrCleanAreaRation;

			// ���� cur ����ʱ�����ƽ�
			while (colorProgressIndex < markPercents.Count && cur >= markPercents[colorProgressIndex])
			{
				SetSlotColor(colorProgressIndex, new Color32(132, 255, 132, 255));
				colorProgressIndex++;
			}
		}
	}

	/// <summary>
	/// �ⲿ���ã�����ָ��������ǵ���ɫ
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
