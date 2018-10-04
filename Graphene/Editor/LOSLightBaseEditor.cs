using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace LOS.Editor {

	[CustomEditor (typeof(LOSLightBase))]
	public class LOSLightBaseEditor : UnityEditor.Editor {

		protected SerializedProperty _isStatic;
		protected SerializedProperty _color;
		protected SerializedProperty _degreeStep;
		protected SerializedProperty _coneAngle;
		protected SerializedProperty _faceAngle;
		protected SerializedProperty _obstacleLayer;
		protected SerializedProperty _material;
		protected SerializedProperty _orderInLayer;
		private LOSLightBase light;

		protected virtual void OnEnable () {
			serializedObject.Update();

			light = (LOSLightBase) target;

			EditorUtility.SetSelectedWireframeHidden(light.GetComponent<Renderer>(), !LOSManager.instance.debugMode);

			_isStatic = serializedObject.FindProperty("isStatic");
			_obstacleLayer = serializedObject.FindProperty("obstacleLayer");
			_degreeStep = serializedObject.FindProperty("degreeStep");
			_coneAngle = serializedObject.FindProperty("coneAngle");
			_faceAngle = serializedObject.FindProperty("faceAngle");
			_color = serializedObject.FindProperty("color");
			_orderInLayer = serializedObject.FindProperty("orderInLayer");
			_material = serializedObject.FindProperty("material");
		}


		public override void OnInspectorGUI () {
			serializedObject.Update();

			EditorGUILayout.PropertyField(_isStatic);

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_obstacleLayer);
			EditorGUILayout.Slider(_degreeStep, 0.1f, 2f);
//			EditorGUILayout.PropertyField(_degreeStep);
			EditorGUILayout.PropertyField(_coneAngle);
			if (_coneAngle.floatValue != 0) {
				EditorGUILayout.PropertyField(_faceAngle);
			}

			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(_color);

			var layers = SortingLayer.layers.ToList();
			var index = layers.FindIndex(x=>x.id == light.sortingLayer);
			if (index == -1)
				index = 0;
			var layer = EditorGUILayout.Popup("Sorting Layer", index, layers.Select(x=>x.name).ToArray());
			
			light.sortingLayer = layers[layer].id;
			
			EditorGUILayout.PropertyField(_orderInLayer);
			EditorGUILayout.PropertyField(_material);
		}
	}

}