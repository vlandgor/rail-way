/// © 2019 Kevin Foley
/// For distribution only on the Unity Asset Store
/// Terms/EULA: https://unity3d.com/legal/as_terms

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OneManEscapePlan.UIList.Examples.Performance_Test.Scripts {
	public class TestListPerformanceTest : MonoBehaviour {

		[SerializeField] protected TestListController listController = null;
		[SerializeField] protected Sprite[] graphics = null;
		[SerializeField] protected int listSize = 100;

		[SerializeField] protected Text elapsedText = null;
		[SerializeField] protected Text averageText = null;

		protected long elapsed;
		protected long average;

		// Start is called before the first frame update
		void Start() {
			Assert.IsNotNull(listController);
			Assert.IsNotNull(graphics);
			Assert.IsTrue(graphics.Length > 0);
			Assert.IsNotNull(elapsedText);
			Assert.IsNotNull(averageText);
		}

		public void RunPooledTest() {
			StopAllCoroutines();
			listController.MaxPoolSize = listSize;
			StartCoroutine(runTest());
		}

		public void RunUnpooledTest() {
			StopAllCoroutines();
			listController.MaxPoolSize = 0;
			StartCoroutine(runTest());
		}

		private IEnumerator runTest() {
			elapsed = 0;
			average = 0;
			long iterations = 0;
			long sum = 0;

			while (true) {
				long startTime = System.DateTime.Now.Ticks;

				listController.Clear();

				for (int i = 0; i < listSize; i++) {
					int index = Random.Range(0, graphics.Length - 1);
					listController.AddItem(new TestListItemModel(Random.Range(0, 999f).ToString(), Random.Range(0, 999f), graphics[index]));
				}

				elapsed = System.DateTime.Now.Ticks - startTime;
				iterations++;
				sum += elapsed;
				average = sum / iterations;

				elapsedText.text = elapsed.ToString();
				averageText.text = average.ToString();

				

				yield return null;
			}
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(TestListPerformanceTest))]
		public class TestListPerformanceTestInspector : Editor {
			public override void OnInspectorGUI() {
				base.OnInspectorGUI();

				var tester = (TestListPerformanceTest)target;

				EditorGUILayout.LabelField("Elapsed", tester.elapsed.ToString());
				EditorGUILayout.LabelField("Average", tester.average.ToString());
			}
		}
#endif
	}
}
