/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace OneManEscapePlan.UIList.Scripts {
	public interface IBindable {
		UnityEvent ChangeEvent { get; }
	}
}
