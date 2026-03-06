/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using OneManEscapePlan.UIList.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OneManEscapePlan.UIList.Examples.Performance_Test.Scripts {
	public class TestListItemView : UIListItemView<TestListItemModel> {
		[SerializeField] protected Image image = null;
		[SerializeField] protected Text titleText = null;
		[SerializeField] protected Text priceText = null;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(image);
			Assert.IsNotNull(titleText);
			Assert.IsNotNull(priceText);
		}

		public override void Refresh() {
			image.sprite = model.graphic;
			titleText.text = model.title;
			priceText.text = model.price.ToString("C2");
		}
	}
}