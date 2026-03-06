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
using UnityEngine.UI;

namespace OneManEscapePlan.UIList.Scripts {

	/// <summary>
	/// A 'virtualized' list is one in which we only create and render a view for as many items as fit in the list. 
	/// This is opposed to a simple list where we create a view for every single item in the list. In a virtualized list, 
	/// views that move offscreen are immediately recycled to the other side of the screen. This offers much better 
	/// performance for large lists (hundreds or thousands of items).
	/// 
	/// THIS IS A BETA FEATURE. IT MAY HAVE BUGS OR OTHER ISSUES. USE AT YOUR OWN RISK.
	/// 
	/// </summary>
	/// <typeparam name="ItemModel">The data type that will be displayed in the list</typeparam>
	/// <typeparam name="ItemView">A concrete implementation of UIListItemView that will be used to render the data for a model</typeparam>
	[RequireComponent(typeof(RectTransform))]
	public abstract class VirtualizedUIListController<ItemModel, ItemView> : AbstractUIListController<ItemModel, ItemView, LinkedList<ItemView>> where ItemView : UIListItemView<ItemModel> {

		public enum Direction {
			Horizontal, Vertical
		}

		public enum HorizontalAlign {
			Left, Middle, Right
		}

		public enum VerticalAlign {
			Top, Middle, Bottom
		}

		#region FIELDS
		[SerializeField] protected int itemSize = 100;
		[SerializeField] protected int gap = 10;
		[SerializeField] protected RectTransform container;
		[SerializeField] protected Direction direction = Direction.Vertical;
		[SerializeField] protected HorizontalAlign horizontalAlign = HorizontalAlign.Left;
		[SerializeField] protected VerticalAlign verticalAlign = VerticalAlign.Top;

		protected RectTransform rectTransform;
		protected int startIndex = 0;
		Vector3[] containerCorners = new Vector3[4];
		protected float lastCoordinate = 0;
		protected Vector2 spacing;

		#endregion

		private void Awake() {
			rectTransform = GetComponent<RectTransform>();
		}

		override protected void Start() {
			Assert.IsNotNull(prefab, "Prefab cannot be null");
			Assert.IsTrue(maxPoolSize < 0 || maxPoolSize >= startingPoolSize, "Starting pool size cannot be greater than max pool size");
			Assert.IsNull(GetComponent<LayoutGroup>(), "Virtualized UI lists do not work correctly when used with a Unity LayoutGroup.");

			for (int i = 0; i < startingPoolSize; i++) {
				ItemView view = ItemViewFactory();
				viewPool.AddLast(view);
				view.gameObject.SetActive(false);
			}

			if (direction == Direction.Horizontal) {
				spacing = new Vector2(itemSize + gap, 0);
			} else {
				spacing = new Vector2(0, itemSize + gap);
			}
			
		}

		private void Update() {
			if (itemViews == null || itemViews.Count == 0) return;

			container.GetWorldCorners(containerCorners);

			bool loop = true;
			float currentCoordinate = 0;
			while (loop) {
				loop = false;
				if (direction == Direction.Horizontal) {
					currentCoordinate = transform.position.x;

					float firstItemX = itemViews.First.Value.transform.position.x;
					if (startIndex + itemViews.Count < itemModels.Count && currentCoordinate < lastCoordinate && firstItemX < containerCorners[0].x) { //scrolling to the right
						increaseStartIndex();
						loop = true;
					} else if (currentCoordinate > lastCoordinate && startIndex > 0) { //scrolling to the left
						float lastItemX = itemViews.Last.Value.transform.position.x;
						if (lastItemX > containerCorners[2].x) {
							decreaseStartIndex();
							loop = true;
						}
					}
				} else if (direction == Direction.Vertical) {
					currentCoordinate = transform.position.y;

					float firstItemY = itemViews.First.Value.transform.position.y;
					if (startIndex + itemViews.Count < itemModels.Count && currentCoordinate > lastCoordinate && firstItemY > containerCorners[1].y) { //scrolling down
						increaseStartIndex();
						loop = true;
					} else if (currentCoordinate < lastCoordinate && startIndex > 0) { //scrolling up
						float lastItemY = itemViews.Last.Value.transform.position.y;
						if (lastItemY < containerCorners[0].y) {
							decreaseStartIndex();
							loop = true;
						}
					}
				}
			}
			lastCoordinate = currentCoordinate;

		}

		virtual protected void increaseStartIndex() {
			startIndex++;
			var view = itemViews.First.Value;
			itemViews.RemoveFirst();
			itemViews.AddLast(view);
			//Debug.Log("StartIndex increased to " + startIndex);
			BuildViews();
		}

		virtual protected void decreaseStartIndex() {
			startIndex--;
			var view = itemViews.Last.Value;
			itemViews.RemoveLast();
			itemViews.AddFirst(view);
			//Debug.Log("StartIndex decreased to " + startIndex);
			BuildViews();
		}

		/// <summary>
		/// Add a new item to the end of the list
		/// </summary>
		/// <param name="model"></param>
		override public void AddItem(ItemModel model) {
			base.AddItem(model);
			BuildViews();
		}

		public override bool RemoveItem(ItemModel item) {
			bool removed = itemModels.Remove(item);
			if (removed) BuildViews();
			return removed;
		}

		/// <summary>
		/// Remove the item with the given index from the list
		/// </summary>
		/// <param name="index"></param>
		override public void RemoveItemAt(int index) {
			base.RemoveItemAt(index);
			BuildViews();
		}

		public override IEnumerable<ItemModel> Data { 
			get => base.Data;
			set {
				startIndex = 0;
				base.Data = value;
			}
		}

		#region HELPERS

		/// <summary>
		/// Synchronizes the item views with the current item models.
		/// </summary>
		override protected void BuildViews() {
			if (!gameObject.activeInHierarchy) return;

			int itemIndex = startIndex;
			float startPadding = itemSize / 2f;
			int maxViews;
			if (direction == Direction.Horizontal) {
				if (container.rect.width == 0) Canvas.ForceUpdateCanvases();
				maxViews = Mathf.FloorToInt(container.rect.width / (float)(itemSize + gap)) + 2;
			} else {
				if (container.rect.height == 0) Canvas.ForceUpdateCanvases();
				maxViews = Mathf.FloorToInt(container.rect.height / (float)(itemSize + gap)) + 2;
			}

			rectTransform.sizeDelta = spacing * (itemModels.Count + .5f);

			LinkedListNode<ItemView> currentViewNode = itemViews.First;

			while (itemIndex < itemModels.Count) {
				if (currentViewNode == null) {
					var view = ItemViewFactory();
					itemViews.AddLast(view);
					currentViewNode = itemViews.Last;
					//Debug.Log("adding new item view");
				} else {
				}

				currentViewNode.Value.Model = itemModels[itemIndex];
				if (direction == Direction.Horizontal) {
					currentViewNode.Value.transform.localPosition = new Vector3(itemIndex * (itemSize + gap) + startPadding, 0, 0);
				} else {
					currentViewNode.Value.transform.localPosition = new Vector3(0, -itemIndex * (itemSize + gap) - startPadding, 0);
				}
				AlignView(currentViewNode.Value);
				
				itemIndex++;
				if (itemIndex - startIndex >= maxViews) break;
				currentViewNode = currentViewNode.Next;
			}
			PruneViews();
		}

		protected void AlignView(ItemView view) {
			RectTransform rt = view.GetComponent<RectTransform>();
			Vector2 pivot = new Vector2(0, 1);
			Vector2 anchorMin = rt.anchorMin;
			Vector2 anchorMax = rt.anchorMax;
			Vector2 anchoredPosition = rt.anchoredPosition;

			if (horizontalAlign == HorizontalAlign.Middle) {
				pivot.x = .5f;
				if (direction == Direction.Vertical) {
					anchorMin.x = .5f;
					anchorMax.x = .5f;
					anchoredPosition.x = 0;
				}
			} else if (horizontalAlign == HorizontalAlign.Right) {
				pivot.x = 1f;
				if (direction == Direction.Vertical) {
					anchorMin.x = 1;
					anchorMax.x = 1;
					anchoredPosition.x = 0;
				}
			}
			if (verticalAlign == VerticalAlign.Middle) {
				pivot.y = .5f;
				if (direction == Direction.Horizontal) {
					anchorMin.y = .5f;
					anchorMax.y = .5f;
					anchoredPosition.y = 0;
				}
			} else if (verticalAlign == VerticalAlign.Bottom) {
				pivot.y = 0f;
				if (direction == Direction.Horizontal) {
					anchorMin.y = 0f;
					anchorMax.y = 0f;
					anchoredPosition.y = 0;
				}
			}

			rt.pivot = pivot;
			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
			rt.anchoredPosition = anchoredPosition;
		}

		public override void Clear() {
			startIndex = 0;
			base.Clear();
		}

		/// <summary>
		/// Remove any excess views from the collection of views
		/// </summary>
		override protected void PruneViews() {
			if (itemViews.First == null || itemViews.First.Value == null) return;
			while (startIndex + itemViews.Count > itemModels.Count) {
				RemoveItemView(itemViews.Count - 1);
			}
		}

		/// <summary>
		/// Remove the item view with the given index
		/// </summary>
		/// <param name="index"></param>
		override protected void RemoveItemView(int index) {
			Assert.IsTrue(index >= 0 && index < itemViews.Count, "Tried to remove view at index " + index + " but only had " + itemViews.Count + " views");

			var currentNode = itemViews.First;
			for (int i = 1; i < index; i++) {
				currentNode = currentNode.Next;
			}

			var view = currentNode.Value;
			RecycleItemView(view);
			itemViews.Remove(currentNode);
		}

		#endregion

	}
}
