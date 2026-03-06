/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Examples.Performance_Test.Scripts {
	[System.Serializable]
	public class TestListItemModel {
		public string title;
		public float price;
		public Sprite graphic;

		public TestListItemModel(string title, float price, Sprite graphic) {
			this.title = title;
			this.price = price;
			this.graphic = graphic;
		}
	}
}