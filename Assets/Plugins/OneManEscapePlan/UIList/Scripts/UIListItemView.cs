/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Scripts {

	/// <summary>
	/// Abstract base class for list item views, which are used to display the data from a list. Each item view is assigned a single model;
	/// concrete implementations should define how data from the model is displayed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[System.Serializable]
	abstract public class UIListItemView<T> : MonoBehaviour {

		[System.Serializable] public class UIListItemViewEvent : UnityEvent<UIListItemView<T>> { }

		[SerializeField] protected UIListItemViewEvent clickedEvent = new UIListItemViewEvent();
		public UIListItemViewEvent ClickedEvent => clickedEvent;

		protected T model;

		protected bool isSelected = false;

		virtual public bool IsSelected {
			get => isSelected;
			set => isSelected = value;
		}

		/// <summary>
		/// Display data from the model on this item view
		/// </summary>
		abstract public void Refresh();

		/// <summary>
		/// Toggle the selection state of this item between selected and not selected
		/// </summary>
		virtual public void ToggleSelection() {
			IsSelected = !isSelected;
		}

		/// <summary>
		/// Invoke the 'clicked' event
		/// </summary>
		public void Click() {
			clickedEvent.Invoke(this);
		}

		/// <summary>
		/// The model displayed by this item view
		/// </summary>
		public T Model {
			get => model;
			set {
				removeBinding();

				model = value;

				if (model != null && model is IBindable bindable) {
					bindable.ChangeEvent.AddListener(Refresh);
				}

				Refresh();
			}
		}

		virtual protected void OnDestroy() {
			removeBinding();
		}

		/// <summary>
		/// Remove the binding event, if applicable
		/// </summary>
		protected void removeBinding() {
			if (model != null && model is IBindable bindable) {
				bindable.ChangeEvent.RemoveListener(Refresh);
			}
		}
	}
}
