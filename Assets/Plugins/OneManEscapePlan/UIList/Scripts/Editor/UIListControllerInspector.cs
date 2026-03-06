/// © 2019-2025 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Scripts.Editor {
	public class UIListControllerInspector<ItemModel, ItemView> : UnityEditor.Editor where ItemView : UIListItemView<ItemModel> {

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			UIListController<ItemModel, ItemView> list = (UIListController<ItemModel, ItemView>)target;

			EditorGUILayout.LabelField("Current List Size: " + list.Count);
			EditorGUILayout.LabelField("Current Pool Size: " + list.CurrentPoolSize);
		}
	}
}