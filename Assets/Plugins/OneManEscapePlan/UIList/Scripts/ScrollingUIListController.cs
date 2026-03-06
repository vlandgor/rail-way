/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace OneManEscapePlan.UIList.Scripts {

	/// <summary>
	/// Defines a generic base class for a standard list with support for scrolling to a specific item. This class is NOT required
	/// to support user scrolling in a ScrollRect, it is only required if you need code-level support for scrolling directly to a
	/// specific item.
	/// </summary>
	/// <typeparam name="ItemModel"></typeparam>
	/// <typeparam name="ItemView"></typeparam>
	public class ScrollingUIListController<ItemModel, ItemView> : UIListController<ItemModel, ItemView> where ItemView : UIListItemView<ItemModel> {

		[SerializeField] protected float autoScrollDuration = .25f;

		protected ScrollRect scrollRect;
		protected Coroutine autoScroll;

		override protected void Start() {
			base.Start();
			scrollRect = GetComponentInParent<ScrollRect>();
			Assert.IsNotNull(scrollRect, "This scrolling UI list is not a child of a ScrollRect");
		}

		virtual public void ScrollTo(ItemModel model) {
			int index = itemModels.IndexOf(model);
			if (index >= 0) ScrollTo(index);
		}

		virtual public void ScrollTo(int index) {
			if (index < 0 || index >= itemModels.Count) throw new System.ArgumentOutOfRangeException("index", index, "Invalid item index");

			if (autoScroll != null) StopCoroutine(autoScroll);

			Vector2 targetPosition;
			LayoutGroup layout = GetComponent<LayoutGroup>();
			if (layout is VerticalLayoutGroup) {
				targetPosition = new Vector2(0, 1 - (index / (float)(itemModels.Count - 1)));
			} else if (layout is HorizontalLayoutGroup) {
				targetPosition = new Vector2(index / (float)(itemModels.Count - 1), 0);
			} else {
				targetPosition = new Vector2(); //TODO: grid layout
			}

			autoScroll = StartCoroutine(AutoScroll(targetPosition));
		}

		virtual protected IEnumerator AutoScroll(Vector2 targetPosition) {
			Vector2 startPosition = new Vector2(scrollRect.horizontalNormalizedPosition, scrollRect.verticalNormalizedPosition);

			Vector2 position = startPosition;

			float t = 0;
			while (t <= autoScrollDuration) {
				t += Time.deltaTime;
				position = Vector2.Lerp(startPosition, targetPosition, t / autoScrollDuration);
				scrollRect.horizontalNormalizedPosition = position.x;
				scrollRect.verticalNormalizedPosition = position.y;
				yield return null;
			}
		}
	}
}
