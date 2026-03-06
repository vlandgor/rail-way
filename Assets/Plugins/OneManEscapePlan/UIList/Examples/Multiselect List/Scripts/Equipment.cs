/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OneManEscapePlan.UIList.Examples.Multiselect_List.Scripts {
	public class Equipment {
		public string name;
		public string category;
		public float price;
		public Sprite icon;
		public int quantityInStock;

		public Equipment(string name, string category, float price, Sprite icon, int quantityInStock) {
			this.name = name;
			this.category = category;
			this.price = price;
			this.icon = icon;
			this.quantityInStock = quantityInStock;
		}

		public override string ToString() {
			return name;
		}
	}
}
