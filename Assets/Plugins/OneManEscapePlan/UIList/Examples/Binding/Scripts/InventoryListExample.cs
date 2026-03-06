/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OneManEscapePlan.UIList.Examples.Binding.Scripts {

	/// <summary>
	/// Demonstrates proper use of the IBindable interface to automatically refresh a list item view when its model changes
	/// </summary>
	public class InventoryListExample : MonoBehaviour {

		[SerializeField] protected InventoryListController list = null;
		[SerializeField] protected InventoryListSelectionManager selectionManager = null;
		[SerializeField] protected Text selectedItemNameText = null;
		[SerializeField] protected Text quantityText = null;

		[SerializeField] protected Slider quantitySlider = null; 

		private InventoryItemQuantity selectedItem = null;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(list);
			Assert.IsNotNull(selectionManager);
			Assert.IsNotNull(selectedItemNameText);
			Assert.IsNotNull(quantityText);

			list.AddItem(new InventoryItemQuantity("Potion", 3));
			list.AddItem(new InventoryItemQuantity("Bandage", 2));
			list.AddItem(new InventoryItemQuantity("Key", 1));
			list.AddItem(new InventoryItemQuantity("Rope", 3));
			list.AddItem(new InventoryItemQuantity("Torch", 1));
			list.AddItem(new InventoryItemQuantity("Arrow", 15));
			list.AddItem(new InventoryItemQuantity("Flint", 5));

			selectionManager.SelectedItemEvent.AddListener(onSelectedItem);
			selectionManager.DeselectedItemEvent.AddListener(onDeselectedItem);

			quantitySlider.interactable = false;
		}

		/// <summary>
		/// When we select an item, keep a reference to the item model and enable the slider
		/// </summary>
		/// <param name="item"></param>
		private void onSelectedItem(InventoryItemQuantity item) {
			this.selectedItem = item;
			selectedItemNameText.text = item.Name;
			quantitySlider.value = item.Quantity;
			quantitySlider.interactable = true;
			quantityText.text = item.Quantity.ToString();
		}

		/// <summary>
		/// If we deselect the selected item, disable the slider
		/// </summary>
		/// <param name="item"></param>
		private void onDeselectedItem(InventoryItemQuantity item) {
			if (this.selectedItem == item) {
				this.selectedItem = null;
				selectedItemNameText.text = "-";
				quantitySlider.value = 0;
				quantitySlider.interactable = false;
			}
		}

		public void SetQuantity(float value) {
			//here we directly edit the item model. the item view in the list will automatically update, because the item implements IBindable
			if (selectedItem != null) {
				selectedItem.Quantity = (int)value;
			}
			//update the value label next to the slider
			quantityText.text = value.ToString();
		}
	}

}