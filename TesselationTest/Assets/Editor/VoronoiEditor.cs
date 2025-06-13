using UnityEngine;
using UnityEditor;

namespace Demo {
	// If the second parameter is true, this will also be applied to subclasses.
	// If you want a custom inspector for a subclass, just add it, and this one will be ignored.
	[CustomEditor(typeof(VoronoiDiagram), true)] 
	public class VoronoiEditor : Editor {
		public override void OnInspectorGUI() {
            VoronoiDiagram targetShape = (VoronoiDiagram)target;

			//if (GUILayout.Button("Generate")) {
			//	targetShape.Generate();
			//}
			DrawDefaultInspector();
		}
	}
}