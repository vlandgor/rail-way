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

namespace OneManEscapePlan.UIList.Examples.Virtualized_List.Scripts {

	[System.Serializable]
	public class VirtualizedListItemView : UIListItemView<string> {

		[SerializeField] protected Text text = null;

		void Start() {
			Assert.IsNotNull(text);
		}

		public override void Refresh() {
			text.text = model;
		}

	}
}
