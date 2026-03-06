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

namespace OneManEscapePlan.UIList.Examples.Binding.Scripts {
	
	[RequireComponent(typeof(Image))]
	public class InventoryListItemView : UIListItemView<InventoryItemQuantity> {

		[SerializeField] protected Text nameText = null;
		[SerializeField] protected Text quantityText = null;
		[SerializeField] protected Color selectedColor = Color.white;

		private Image background;
		private Color defaultColor;

		private void Start() {
			Assert.IsNotNull(nameText);
			Assert.IsNotNull(quantityText);
			background = GetComponent<Image>();
			defaultColor = background.color;
		}

		public override void Refresh() {
			nameText.text = model.Name;
			quantityText.text = model.Quantity.ToString();
		}

		public override bool IsSelected {
			get => base.IsSelected;
			set {
				base.IsSelected = value;

				if (value) {
					background.color = selectedColor;
				} else {
					background.color = defaultColor;
				}
			}
		}
	}
}
