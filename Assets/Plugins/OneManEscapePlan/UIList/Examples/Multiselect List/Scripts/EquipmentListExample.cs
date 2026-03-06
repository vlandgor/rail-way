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

namespace OneManEscapePlan.UIList.Examples.Multiselect_List.Scripts {

	public class EquipmentListExample : MonoBehaviour {

		[SerializeField] private Sprite shovelSprite = null;
		[SerializeField] private Sprite pickaxeSprite = null;
		[SerializeField] private Sprite longswordSprite = null;
		[SerializeField] private Sprite shortswordSprite = null;
		[SerializeField] private Sprite daggerSprite = null;
		[SerializeField] private Sprite magicDaggerSprite = null;

		[SerializeField] protected EquipmentListController list = null;
		[SerializeField] protected EquipmentListSelectionManager selectionManager = null;
		[SerializeField] protected Text selectedEquipmentText = null;
		[SerializeField] protected Text selectedEquipmentValueText = null;
		[SerializeField] protected Text maxSelectionSizeText = null;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(list);
			Assert.IsNotNull(selectionManager);
			Assert.IsNotNull(selectedEquipmentText);
			Assert.IsNotNull(selectedEquipmentValueText);
			Assert.IsNotNull(maxSelectionSizeText);

			list.AddItem(new Equipment("Shovel", "Gear", 8, shovelSprite, 30));
			list.AddItem(new Equipment("Pickaxe", "Gear", 30, pickaxeSprite, 11));
			list.AddItem(new Equipment("+1 Longsword", "Weapons", 40, longswordSprite, 5));
			list.AddItem(new Equipment("+4 Short Sword", "Weapons", 25, shortswordSprite, 8));
			list.AddItem(new Equipment("Dagger", "Weapons", 10, daggerSprite, 12));
			list.AddItem(new Equipment("+1 Magic Dagger", "Weapons", 65, magicDaggerSprite, 2));

			selectionManager.SelectionChangedEvent.AddListener(updateSelection);
		}

		private void updateSelection() {
			string selection = "";

			var selectedItems = selectionManager.SelectedItems;
			float value = 0;
			foreach (var item in selectedItems) {
				if (selection.Length > 0) selection += ", ";
				selection += item.name;
				value += item.price;
			}

			selection = string.Format("({0}/{1})\n", selectedItems.Count, selectionManager.MaxSelectedItems) + selection;

			selectedEquipmentText.text = selection;

			selectedEquipmentValueText.text = value.ToString("C2");
		}

		public void SetMaxSelectionSize(float value) {
			selectionManager.MaxSelectedItems = (int)value;
			maxSelectionSizeText.text = selectionManager.MaxSelectedItems.ToString();
			updateSelection();
		}
	}

}