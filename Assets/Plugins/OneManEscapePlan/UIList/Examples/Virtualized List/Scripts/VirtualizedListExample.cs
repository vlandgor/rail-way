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

namespace OneManEscapePlan.UIList.Examples.Virtualized_List.Scripts {
	public class VirtualizedListExample : MonoBehaviour {

		[SerializeField] protected VirtualizedListController list = null;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(list);

			string[] namesArray = new string[10000];
			for (int i = 0; i < namesArray.Length; i++) {
				namesArray[i] = i.ToString("");
			}
			list.Data = namesArray;
		}
	}
}
