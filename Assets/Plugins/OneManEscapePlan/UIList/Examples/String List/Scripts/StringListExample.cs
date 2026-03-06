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

namespace OneManEscapePlan.UIList.Examples.String_List.Scripts {
	public class StringListExample : MonoBehaviour {

		[SerializeField] protected StringListController list = null;
		[SerializeField] protected StringListSelectionManager selectionManager = null;
		[SerializeField] protected InputField input = null;
		[SerializeField] protected TextAsset names = null;
		[SerializeField] protected Text poolSizeText = null;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(list);
			Assert.IsNotNull(selectionManager);
			Assert.IsNotNull(input);
			Assert.IsNotNull(names);
			Assert.IsNotNull(poolSizeText);

			string[] namesArray = names.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
			list.Data = namesArray;
			selectionManager.SelectedItemEvent.AddListener(onSelectedItem);
		}

		private void onSelectedItem(string item) {
			input.text = item;
		}

		public void Add() {
			if (input.text.Length > 0) {
				list.AddItem(input.text);
				list.ScrollTo(list.Count - 1);
				refreshText();
			}
		}

		public void Remove() {
			if (input.text.Length > 0) {
				list.RemoveItem(input.text);
				refreshText();
			}
		}

		private void refreshText() {
			poolSizeText.text = "Pool size: " + list.CurrentPoolSize;
		}
	}
}
