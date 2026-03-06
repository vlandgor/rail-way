/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Scripts {

	/// <summary>
	/// This component can be added to UIListControllers to add support for selection management. It supports single- or multi-item selection,
	/// with two selection modes. 
	/// </summary>
	/// <typeparam name="ItemModel"></typeparam>
	/// <typeparam name="ItemView"></typeparam>
	//[RequireComponent(typeof(UIListController<ItemModel, ItemView>))] //doesn't work with generics
	public class UIListSelectionManager<ItemModel, ItemView> : MonoBehaviour where ItemView : UIListItemView<ItemModel> {

		[Serializable]
		public enum SelectionFullBehavior {
			Ignore, DeselectOldest
		}

		#region FIELDS
		[SerializeField] private int maxSelectedItems = 1;
		[Tooltip("What to do when the user clicks an item while the selection is full")]
		[SerializeField] private SelectionFullBehavior selectionFullBehaviour = SelectionFullBehavior.Ignore;

		[SerializeField] protected UIListController<ItemModel, ItemView>.ModelEvent selectedItemEvent = new UIListController<ItemModel, ItemView>.ModelEvent();
		/// <summary>
		/// Invoked when an item is selected
		/// </summary>
		public UIListController<ItemModel, ItemView>.ModelEvent SelectedItemEvent => selectedItemEvent;

		[SerializeField] protected UIListController<ItemModel, ItemView>.ModelEvent deselectedItemEvent = new UIListController<ItemModel, ItemView>.ModelEvent();
		/// <summary>
		/// Invoked when an item is deselected
		/// </summary>
		public UIListController<ItemModel, ItemView>.ModelEvent DeselectedItemEvent => deselectedItemEvent;

		[Tooltip("This event is fired when the selection changes (from selecting or deselecting items)")]
		[SerializeField] protected UnityEvent selectionChangedEvent = new UnityEvent();
		/// <summary>
		/// Invoked when the selection changes (from selecting or deselecting items)
		/// </summary>
		public UnityEvent SelectionChangedEvent => selectionChangedEvent;

		protected UIListController<ItemModel, ItemView> controller;
		protected List<ItemView> selectedViews = new List<ItemView>();
		protected List<ItemModel> selectedModels = new List<ItemModel>();
		#endregion

		virtual protected void Awake() {
			if (controller == null) FindController();
		}

		virtual protected void Start() {
			controller.ClickedViewEvent.AddListener(ToggleSelection);
		}

		virtual protected void FindController() {
			controller = GetComponent<UIListController<ItemModel, ItemView>>();
			Assert.IsNotNull(controller, "Couldn't find the UIListController");
		}

		/// <summary>
		/// Select the given item if it is not already selected. If the list contains duplicates, the first occurrence will be 
		/// selected.
		/// </summary>
		/// <param name="model">Item model to select</param>
		virtual public void Select(ItemModel model) {
			if (selectedModels.Contains(model)) return;

			if (controller == null) FindController();

			int index = controller.IndexOf(model);
			if (index >= 0) {
				ItemView view = controller.GetViewAt(index);
				SelectView(view);
			}
		}

		/// <summary>
		/// Deselect the given item if it is selected. If the list contains duplicates, the first occurrence will be deselected.
		/// </summary>
		/// <param name="model">Item model to select</param>
		virtual public void Deselect(ItemModel model) {
			int index = selectedModels.IndexOf(model);
			if (index >= 0) {
				DeselectAt(index);
				selectionChangedEvent.Invoke();
			}
		}

		virtual public void ToggleSelection(ItemModel model) {
			int index = selectedModels.IndexOf(model);
			if (index >= 0) {
				DeselectAt(index);
			} else {
				Select(model);
			}
			selectionChangedEvent.Invoke();
		}

		virtual public bool IsSelected(ItemModel model) {
			int index = selectedModels.IndexOf(model);
			return (index >= 0);
		}

		/// <summary>
		/// Deselect all selected items
		/// </summary>
		public void ClearSelection() {
			while (selectedViews.Count > 0) {
				DeselectAt(selectedViews.Count - 1);
			}
			selectionChangedEvent.Invoke();
		}

		#region HELPERS
		/// <summary>
		/// Toggle whether the given item view is selected
		/// </summary>
		virtual protected void ToggleSelection(UIListItemView<ItemModel> view) {
			if (view.IsSelected) {
				int index = selectedViews.IndexOf(view as ItemView);
				Assert.IsTrue(index >= 0, "Tried to deselect an item not currently selected by this selection manager");
				DeselectAt(index);
			} else {
				SelectView(view);
			}
		}

		/// <summary>
		/// Select the given view
		/// </summary>
		/// <param name="view"></param>
		virtual protected void SelectView(UIListItemView<ItemModel> view) {
			if (selectedViews.Count >= maxSelectedItems && selectionFullBehaviour == SelectionFullBehavior.DeselectOldest) {
				DeselectAt(0);
			}

			if (selectedViews.Count < maxSelectedItems) {
				view.IsSelected = true;
				selectedViews.Add(view as ItemView);
				selectedModels.Add(view.Model);
				selectedItemEvent.Invoke(view.Model);

				selectionChangedEvent.Invoke();
			}
		}

		/// <summary>
		/// Deselect the view at the given selection index
		/// </summary>
		/// <param name="index">Index of the view in the collection of currently selected views</param>
		virtual protected void DeselectAt(int index) {
			Assert.IsTrue(index >= 0 && index < selectedViews.Count);
			ItemView view = selectedViews[index];
			selectedViews.RemoveAt(index);
			selectedModels.RemoveAt(index);
			view.IsSelected = false;
			deselectedItemEvent.Invoke(view.Model);
			selectionChangedEvent.Invoke();
		}
		#endregion

		#region PROPERTIES
		/// <summary>
		/// The maximum number of items that can be selected simultaneously
		/// </summary>
		virtual public int MaxSelectedItems {
			get => maxSelectedItems;
			set {
				maxSelectedItems = value;
				if (selectedViews.Count > maxSelectedItems) {
					while (selectedViews.Count > maxSelectedItems) {
						DeselectAt(selectedViews.Count - 1);
					}
					selectionChangedEvent.Invoke();
				}
			}
		}
		/// <summary>
		/// How the selection manager will behave when the user tries to select a new item while the selection is already full
		/// </summary>
		public SelectionFullBehavior SelectBehaviorWhenFull {
			get => selectionFullBehaviour;
			set => selectionFullBehaviour = value;
		}

		/// <summary>
		/// The currently selected items
		/// </summary>
		public ICollection<ItemModel> SelectedItems {
			get {
				return selectedModels.AsReadOnly();
			}
			set {
				ClearSelection();
				foreach (var model in value) {
					Select(model);
				}
				selectionChangedEvent.Invoke();
			}
		}

		/// <summary>
		/// Get the first selected item. This property is particularly useful for lists with a maximum selection size of 1.
		/// </summary>
		public ItemModel FirstSelectedItem {
			get {
				if (selectedModels.Count > 0) {
					return selectedModels[0];
				}
				return default(ItemModel);
			}
		}
		#endregion

		virtual protected void OnDestroy() {
			if (controller != null) controller.ClickedViewEvent.RemoveListener(ToggleSelection);
		}
	}
}
