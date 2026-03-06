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

namespace OneManEscapePlan.UIList.Scripts {

	/// <summary>
	/// Defines a generic base class for standard UI Lists. This list controller is designed to work with Unity LayoutGroups
	/// and creates one item view for each item in the data set. By extending this class with a concrete type, you can easily
	/// create lists to display any type of data. For example, this creates a list of strings with a single line of code:
	/// 
	/// <c>public class StringListController : UIListController<string, StringListItemView> { }</c>
	/// 
	/// For each concrete list implementation, you will need to create a corresponding concrete view implementation.
	/// 
	/// </summary>
	/// <typeparam name="ItemModel">The data type that will be displayed in the list</typeparam>
	/// <typeparam name="ItemView">A concrete implementation of UIListItemView that will be used to render the data for a model</typeparam>
	[RequireComponent(typeof(LayoutGroup))]
	public abstract class UIListController<ItemModel, ItemView> : AbstractUIListController<ItemModel, ItemView, List<ItemView>> where ItemView : UIListItemView<ItemModel> {

		#region FIELDS
		#endregion

		/// <summary>
		/// Add a new item to the end of the list
		/// </summary>
		/// <param name="model"></param>
		override public void AddItem(ItemModel model) {
			base.AddItem(model);
			ItemView view = ItemViewFactory();
			itemViews.Add(view);
			view.Model = model;
		}

		/// <summary>
		/// Remove the given item from the list, if present
		/// </summary>
		/// <param name="item">Item to remove</param>
		/// <returns><c>true</c> if the item was removed, <c>false if the item was not found</c></returns>
		override public bool RemoveItem(ItemModel item) {
			int index = itemModels.IndexOf(item);
			if (index >= 0) {
				RemoveItemAt(index);
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Remove the item with the given index from the list
		/// </summary>
		/// <param name="index"></param>
		override public void RemoveItemAt(int index) {
			if (index < 0 || index >= itemModels.Count) throw new System.ArgumentOutOfRangeException("index", index, "Invalid item index");
			itemModels.RemoveAt(index);
			RemoveItemView(index);
		}

		#region HELPERS

		internal ItemView GetViewAt(int index) {
			if (index < 0 || index >= itemModels.Count) throw new System.ArgumentOutOfRangeException("index", index, "Invalid item index");
			return itemViews[index];
		}

		/// <summary>
		/// Synchronizes the item views with the current item models. We add/remove views as necessary to match the number of models,
		/// and assign each view to its corresponding model.
		/// </summary>
		override protected void BuildViews() {
			for (int i = 0; i < itemModels.Count; i++) {
				ItemView view;
				if (i < itemViews.Count) {
					view = itemViews[i];
				} else {
					view = ItemViewFactory();
					itemViews.Add(view);
				}
				view.Model = itemModels[i];
			}
			PruneViews();
		}

		/// <summary>
		/// Remove the item view with the given index
		/// </summary>
		/// <param name="index"></param>
		override protected void RemoveItemView(int index) {
			Assert.IsTrue(index >= 0 && index < itemViews.Count, "Tried to remove view at index " + index + " but only had " + itemViews.Count + " views");

			ItemView view = itemViews[index];
			RecycleItemView(view);
			itemViews.RemoveAt(index);
		}
		
		#endregion
	}
}
