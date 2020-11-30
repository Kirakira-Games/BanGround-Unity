namespace HierarchyGadget { // v1.2.0
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	public class HierarchyGadget {



		#region --- VAR ---


		private const int MAX_ICON_NUM = 5;

		private static List<System.Type> HideTypes = new List<System.Type>() {
			typeof(Transform),
			typeof(ParticleSystemRenderer),
			typeof(CanvasRenderer),
		};
		private static Transform OffsetObject = null;
		private static int Offset = 0;

		// Config
		private static bool ShowActiveToggle = true;
		private static bool SqueezeWhenOverflow = true;
		private static int IconSize = 16;


		#endregion



		#region --- MSG ---



		[InitializeOnLoadMethod]
		public static void Init () {
			EditorApplication.hierarchyWindowItemOnGUI += HieGUI;
			LoadConfig();
			SaveConfig();
		}


		public static void HieGUI (int instanceID, Rect rect) {


			// Check
			Object tempObj = EditorUtility.InstanceIDToObject(instanceID);
			if (!tempObj) {
				return;
			}


			// fix rect
			rect.width += rect.x;
			rect.x = 0;

			// Logic
			GameObject obj = tempObj as GameObject;
			List<Component> coms = new List<Component>(obj.GetComponents<Component>());
			for (int i = 0; i < coms.Count; i++) {
				if (!coms[i]) {
					continue;
				}
				if (TypeCheck(coms[i].GetType())) {
					coms.RemoveAt(i);
					i--;
				}
			}

			int maxIconNum = MAX_ICON_NUM;
			int iconSize = IconSize;
			int y = (18 - iconSize) / 2;
			int offset = obj.transform == OffsetObject ? Offset : 0;
			float globalOffsetX = PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab ? 0 : -16;

			// Active TG
			if (ShowActiveToggle) {
				globalOffsetX -= iconSize + 2;
				maxIconNum--;
			}

			// Has ...
			bool hasMoreButton = !SqueezeWhenOverflow && coms.Count > maxIconNum;
			if (hasMoreButton) {
				maxIconNum--;
			}

			// Main
			var oldC = GUI.color;
			int deltaCount = Mathf.Max(coms.Count, 1);
			float deltaX = SqueezeWhenOverflow ? Mathf.Min(
				(iconSize) * ((float)maxIconNum / deltaCount), iconSize
			) : (iconSize);

			for (int i = 0; i + offset < coms.Count && (SqueezeWhenOverflow || i <= maxIconNum); i++) {

				Component com = coms[i + offset];
				Texture2D texture = AssetPreview.GetMiniThumbnail(com);

				if (texture) {
					GUI.color = com.gameObject.activeInHierarchy ? Color.white : new Color(1, 1, 1, 0.4f);
					var _r = new Rect(
						rect.width - deltaX * i + globalOffsetX,
						rect.y + y,
						iconSize,
						iconSize
					);
					//GUI.Box(_r, GUIContent.none);
					GUI.DrawTexture(_r, texture);
				}
			}
			GUI.color = oldC;


			// "..." Button
			if (hasMoreButton) {
				GUIStyle style = new GUIStyle(GUI.skin.label) {
					fontSize = 9,
					alignment = TextAnchor.MiddleCenter
				};

				if (GUI.Button(new Rect(
					rect.width - (iconSize + 2) * (maxIconNum + 1) + globalOffsetX,
					rect.y + y,
					22,
					iconSize
				), "•••", style)) {
					if (OffsetObject != obj.transform) {
						OffsetObject = obj.transform;
						Offset = 0;
					}
					Offset += maxIconNum + 1;
					if (Offset >= coms.Count) {
						Offset = 0;
					}
				}

			}


			if (ShowActiveToggle) {
				// Active Toggle
				rect.x = rect.width;
				rect.width = rect.height;
				bool active = GUI.Toggle(rect, obj.activeSelf, GUIContent.none);
				if (active != obj.activeSelf) {
					obj.SetActive(active);
					EditorUtility.SetDirty(obj);
				}
			}



		}


		#endregion



		#region --- MNU ---


		// Validate
		[MenuItem("Tools/HierarchyGadget/Show Active Toggle", validate = true)]
		public static bool SetShowActiveToggle_True_V () => !ShowActiveToggle;


		[MenuItem("Tools/HierarchyGadget/Hide Active Toggle", validate = true)]
		public static bool SetShowActiveToggle_False_V () => ShowActiveToggle;


		[MenuItem("Tools/HierarchyGadget/Squeeze When Overflow", validate = true)]
		public static bool SetSqueezeWhenOverflow_True_V () => !SqueezeWhenOverflow;


		[MenuItem("Tools/HierarchyGadget/Fold When Overflow", validate = true)]
		public static bool SetSqueezeWhenOverflow_False_V () => SqueezeWhenOverflow;


		[MenuItem("Tools/HierarchyGadget/Small Icon", validate = true)]
		public static bool SetIconSize_Small_V () => IconSize != 14;


		[MenuItem("Tools/HierarchyGadget/Normal Icon", validate = true)]
		public static bool SetIconSize_Normal_V () => IconSize != 16;


		[MenuItem("Tools/HierarchyGadget/Large Icon", validate = true)]
		public static bool SetIconSize_Large_V () => IconSize != 18;



		// Menu
		[MenuItem("Tools/HierarchyGadget/Show Active Toggle", priority = 0)]
		public static void SetShowActiveToggle_True () {
			ShowActiveToggle = true;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/HierarchyGadget/Hide Active Toggle", priority = 1)]
		public static void SetShowActiveToggle_False () {
			ShowActiveToggle = false;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}



		[MenuItem("Tools/HierarchyGadget/Squeeze When Overflow", priority = 12)]
		public static void SetSqueezeWhenOverflow_True () {
			SqueezeWhenOverflow = true;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/HierarchyGadget/Fold When Overflow", priority = 13)]
		public static void SetSqueezeWhenOverflow_False () {
			SqueezeWhenOverflow = false;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/HierarchyGadget/Small Icon", priority = 24)]
		public static void SetIconSize_Small () {
			IconSize = 14;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/HierarchyGadget/Normal Icon", priority = 25)]
		public static void SetIconSize_Normal () {
			IconSize = 16;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}


		[MenuItem("Tools/HierarchyGadget/Large Icon", priority = 26)]
		public static void SetIconSize_Large () {
			IconSize = 18;
			SaveConfig();
			EditorApplication.RepaintHierarchyWindow();
		}





		#endregion



		#region --- LGC ---


		private static bool TypeCheck (System.Type type) {
			for (int i = 0; i < HideTypes.Count; i++) {
				if (type == HideTypes[i] || type.IsSubclassOf(HideTypes[i])) {
					return true;
				}
			}
			return false;
		}


		private static void SaveConfig () {
			EditorPrefs.SetBool("HierarchyGadget.ShowActiveToggle", ShowActiveToggle);
			EditorPrefs.SetBool("HierarchyGadget.SqueezeWhenOverflow", SqueezeWhenOverflow);
			EditorPrefs.SetInt("HierarchyGadget.IconSize", IconSize);
		}


		private static void LoadConfig () {
			try {
				ShowActiveToggle = EditorPrefs.GetBool("HierarchyGadget.ShowActiveToggle", ShowActiveToggle);
				SqueezeWhenOverflow = EditorPrefs.GetBool("HierarchyGadget.SqueezeWhenOverflow", SqueezeWhenOverflow);
				IconSize = EditorPrefs.GetInt("HierarchyGadget.IconSize", 16);
			} catch { }
		}


		#endregion



	}
}