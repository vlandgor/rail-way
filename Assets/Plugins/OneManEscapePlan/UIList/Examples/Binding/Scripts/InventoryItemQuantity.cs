/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using OneManEscapePlan.UIList.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Examples.Binding.Scripts {

	/// <summary>
	/// This item model implements IBindable and invokes the ChangeEvent when its data changes, allowing any 
	/// UIListItemView that displays it to automatically refresh
	/// </summary>
	public class InventoryItemQuantity : IBindable {
		private string name;
		private int quantity;

		private UnityEvent changeEvent;

		public InventoryItemQuantity(string name, int quantity) {
			this.name = name;
			this.quantity = quantity;
			changeEvent = new UnityEvent();
		}

		public UnityEvent ChangeEvent => changeEvent;

		public string Name {
			get => name;
			set {
				name = value;
				changeEvent.Invoke();
			}
		}


		public int Quantity {
			get => quantity;
			set {
				quantity = value;
				changeEvent.Invoke();
			}
		}

		public override string ToString() {
			return name;
		}
	}
}
