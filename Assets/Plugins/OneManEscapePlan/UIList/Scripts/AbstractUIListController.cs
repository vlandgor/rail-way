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
	/// Defines an abstract base class for UI Lists.
	/// 
	/// This class supports view pooling, meaning that unused views can be cached and re-used as needed. This can
	/// improve performance for lists where the number of items in the list changes frequently. When an item is removed from
	/// the list, and the pool is not full, that item's view is returned to the pool rather than being destroyed. The next
	/// time an item is added and a view is needed, the existing view can be retrieved from the pool and reused. This reduces
	/// object instantiation and garbage collection, at the cost of increased memory usage.
	/// 
	/// </summary>
	/// <typeparam name="ItemModel">The data type that will be displayed in the list</typeparam>
	/// <typeparam name="ItemView">A concrete implementation of UIListItemView that will be used to render the data for a model</typeparam>
	/// <typeparam name="ItemViewCollection">The collection type that will be used to store active item views</typeparam>
	public abstract class AbstractUIListController<ItemModel, ItemView, ItemViewCollection> : MonoBehaviour 
		where ItemView : UIListItemView<ItemModel>
		where ItemViewCollection : ICollection<ItemView>, new()
	{

		#region FIELDS
		[System.Serializable]
		public class ModelEvent : UnityEvent<ItemModel> { }

		[SerializeField] protected ItemView prefab;
		[Tooltip("Maximum number of List Item Views that can be pooled. Use a negative value for unlimited pool size.")]
		[SerializeField] protected int maxPoolSize = 0;
		[Tooltip("How many List Item Views will start in the pool")]
		[SerializeField] protected int startingPoolSize = 0;

		[SerializeField] protected ModelEvent clickedItemEvent = new ModelEvent();
		public ModelEvent ClickedItemEvent => clickedItemEvent;

		[SerializeField] protected UIListItemView<ItemModel>.UIListItemViewEvent clickedViewEvent = new UIListItemView<ItemModel>.UIListItemViewEvent();
		internal UIListItemView<ItemModel>.UIListItemViewEvent ClickedViewEvent => clickedViewEvent;

		protected List<ItemModel> itemModels = new List<ItemModel>();
		protected ItemViewCollection itemViews = new ItemViewCollection();

		/// <summary>
		/// Stores a pool of inactive item views
		/// </summary>
		protected LinkedList<ItemView> viewPool = new LinkedList<ItemView>();

		#endregion

		virtual protected void Start() {
			Assert.IsNotNull(prefab, "Prefab cannot be null");
			Assert.IsTrue(maxPoolSize < 0 || maxPoolSize >= startingPoolSize, "Starting pool size cannot be greater than max pool size");

			for (int i = 0; i < startingPoolSize; i++) {
				ItemView view = ItemViewFactory();
				viewPool.AddLast(view);
				view.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Add a new item to the end of the list
		/// </summary>
		/// <param name="model"></param>
		virtual public void AddItem(ItemModel model) {
			itemModels.Add(model);
		}

		/// <summary>
		/// Add a range of items to the end of the list
		/// </summary>
		/// <param name="range"></param>
		virtual public void AddRange(ICollection<ItemModel> range) {
			itemModels.AddRange(range);
			BuildViews();
		}

		/// <summary>
		/// Insert a new item at a specific index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="model"></param>
		virtual public void InsertItem(int index, ItemModel model) {
			itemModels.Insert(index, model);
			BuildViews();
		}

		virtual public bool Contains(ItemModel model) {
			return itemModels.Contains(model);
		}

		virtual public int IndexOf(ItemModel model) {
			return itemModels.IndexOf(model);
		}

		/// <summary>
		/// Remove the given item from the list, if present
		/// </summary>
		/// <param name="item">Item to remove</param>
		/// <returns><c>true</c> if the item was removed, <c>false if the item was not found</c></returns>
		abstract public bool RemoveItem(ItemModel item);

		/// <summary>
		/// Remove the item with the given index from the list
		/// </summary>
		/// <param name="index"></param>
		virtual public void RemoveItemAt(int index) {
			if (index < 0 || index >= itemModels.Count) throw new System.ArgumentOutOfRangeException("index", index, "Invalid item index");
			itemModels.RemoveAt(index);
		}

		/// <summary>
		/// Refresh all item views in the list. This is probably not necessary if your item model implements IBindable.
		/// </summary>
		virtual public void Refresh() {
			foreach (ItemView view in itemViews) {
				view.Refresh();
			}
		}

		/// <summary>
		/// Remove all items from the list
		/// </summary>
		virtual public void Clear() {
			itemModels.Clear();
			PruneViews();
		}

		#region PROPERTIES
		virtual public IEnumerable<ItemModel> Data {
			set {
				if (this.itemModels == null) {
					this.itemModels = new List<ItemModel>(value); //shallow copy
				} else {
					this.itemModels.Clear();
					if (value != null) this.itemModels.AddRange(value);
				}				

				BuildViews();
			}
			get {
				return itemModels.AsReadOnly();
			}
		}

		/// <summary>
		/// Number of items in the list
		/// </summary>
		public int Count => itemModels.Count;

		/// <summary>
		/// Number of unused items currently in the pool
		/// </summary>
		public int CurrentPoolSize => viewPool.Count;

		/// <summary>
		/// Maximum number of List Item Views that can be pooled. Use a negative value for unlimited pool size.
		/// </summary>
		public int MaxPoolSize {
			get => maxPoolSize;
			set => maxPoolSize = value;
		}
		#endregion

		#region HELPERS

		/// <summary>
		/// Synchronizes the item views with the current item models.
		/// </summary>
		abstract protected void BuildViews();

		/// <summary>
		/// Remove any excess views from the collection of views
		/// </summary>
		virtual protected void PruneViews() {
			while (itemViews.Count > itemModels.Count) {
				RemoveItemView(itemViews.Count - 1);
			}
		}

		/// <summary>
		/// Remove the item view with the given index
		/// </summary>
		/// <param name="index"></param>
		abstract protected void RemoveItemView(int index);

		/// <summary>
		/// Move the given view to the pool if applicable, or destroy it otherwise
		/// </summary>
		/// <param name="view"></param>
		virtual protected void RecycleItemView(ItemView view) {
			if (maxPoolSize < 0 || viewPool.Count < maxPoolSize) {
				view.gameObject.SetActive(false);
				viewPool.AddLast(view);
			} else {
				Destroy(view.gameObject);
			}
		}

		/// <summary>
		/// Get an unused ItemView (either from the pool or a new instance)
		/// </summary>
		/// <returns></returns>
		virtual protected ItemView ItemViewFactory() {
			ItemView view;

			if (viewPool.Count > 0) {
				//use a pooled view if available
				view = viewPool.Last.Value;
				view.transform.SetSiblingIndex(transform.childCount - 1); //move to bottom
				view.gameObject.SetActive(true);
				viewPool.RemoveLast();
			} else {
				//otherwise, create a new view
				view = CreateView();
			}

			return view;
		}

		virtual protected ItemView CreateView() {
			ItemView view = Instantiate(prefab, transform);
			view.ClickedEvent.AddListener(onItemClicked);
			return view;
		}

		virtual protected void onItemClicked(UIListItemView<ItemModel> view) {
			clickedViewEvent.Invoke(view);
			clickedItemEvent.Invoke(view.Model);
		}

		#endregion
	}
}
