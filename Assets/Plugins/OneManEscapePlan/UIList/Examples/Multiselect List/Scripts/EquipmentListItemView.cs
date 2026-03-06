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

namespace OneManEscapePlan.UIList.Examples.Multiselect_List.Scripts {
	
	[RequireComponent(typeof(Image))]
	public class EquipmentListItemView : UIListItemView<Equipment> {
		
		[SerializeField] protected Image icon = null;
		[SerializeField] protected Text nameText = null;
		[SerializeField] protected Text categoryText = null;
		[SerializeField] protected Text priceText = null;
		[SerializeField] protected Text inStockText = null;
		[SerializeField] protected Color selectedColor = Color.white;

		private Image background;
		private Color defaultColor;

		private void Start() {
			Assert.IsNotNull(icon);
			Assert.IsNotNull(nameText);
			Assert.IsNotNull(categoryText);
			Assert.IsNotNull(priceText);
			Assert.IsNotNull(inStockText);
			background = GetComponent<Image>();
			defaultColor = background.color;
		}

		public override void Refresh() {
			nameText.text = model.name;
			priceText.text = model.price.ToString("N0");
			categoryText.text = model.category;
			inStockText.text = model.quantityInStock.ToString();
			icon.sprite = model.icon;
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
