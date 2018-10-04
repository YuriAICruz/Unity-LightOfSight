using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LOS
{
    /// <summary>
    /// LOS manager is a singleton.
    /// It coordinates the system & provide common functions for others to use.
    /// </summary>
    [ExecuteInEditMode]
    public class LOSManager : MonoBehaviour
    {
        public PhysicsOpt physicsOpt;
        public float viewboxExtension = 1.01f;
        public bool debugMode;
        
        [HideInInspector]
        public bool is2D
        {
            get { return physicsOpt == PhysicsOpt.Physics_2D; }
        }

        private static LOSManager _instance;

        private List<LOSObstacle> _obstacles;
        private List<LOSLightBase> _lights;
        private Transform _losCameraTrans;
        private List<LOSCamera> _losCamera;
        private bool _isDirty;


        /// <summary>
        /// Gets the instance of the singleton.
        /// </summary>
        /// <value>The instance.</value>
        public static LOSManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LOSManager>();

                    if (_instance == null)
                    {
                        var go = new GameObject();
                        go.name = "LOSManager";
                        _instance = go.AddComponent<LOSManager>();
                        DontDestroyOnLoad(go);

                        _instance.Init();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Tries the get instance.
        /// It is useful when you need to get the singleton near the end of an object's life cycle,
        /// 	as there are chances that it is the end of play mode, it's ok for the singleton to return null.
        /// </summary>
        /// <returns>The get instance.</returns>
        public static LOSManager TryGetInstance()
        {
            return _instance;
        }


        public List<LOSObstacle> obstacles
        {
            get
            {
                if (_obstacles == null)
                {
                    _obstacles = new List<LOSObstacle>();
                }
                return _obstacles;
            }
        }

        public List<LOSLightBase> lights
        {
            get
            {
                if (_lights == null)
                {
                    _lights = new List<LOSLightBase>();
                }
                return _lights;
            }
        }

        public LOSCamera GetLosCamera(int index)
        {
            if (_losCamera == null)
            {
                var losCameras = FindObjectsOfType<LOSCamera>().ToList();

                if (losCameras.Count == 0)
                {
                    Debug.LogError("No LOSCamera is found in the scene! Please remember to attach LOSCamera to the camera gameobjeect.");
                    return null;
                }

                _losCamera = losCameras;
                _losCamera = _losCamera.OrderBy((a) => a.name).ToList();
            }
            if (_losCamera.Count == 0 || index >= _losCamera.Count)
                return null;

            return _losCamera[index];
        }

        void Start()
        {
            Init();
        }

        void LateUpdate()
        {
            for (int i = 0; i < _losCamera.Count; i++)
            {
                UpdateLights(i);
            }
        }

        void OnEnable()
        {
            foreach (var light in lights)
            {
                light.ToggleOnOff(true);
            }
        }

        void OnDisable()
        {
            foreach (var light in lights)
            {
                if (light != null)
                {
                    light.ToggleOnOff(false);
                }
            }
        }

        /// <summary>
        /// Updates the lights. 
        /// It is the place tells the lights to draw.
        /// </summary>
        public void UpdateLights(int index)
        {
            if (GetLosCamera(index).CheckDirty())
            {
                UpdateViewingBox(index);
            }

            for (int i = 0, n = lights.Count; i < n; i++)
            {
                lights[i].TryDraw();
            }

            UpdatePreviousInfo(index);
        }

        private void UpdatePreviousInfo(int index)
        {
            _isDirty = false;

            GetLosCamera(index).UpdatePreviousInfo();

            for (int i = 0, n = obstacles.Count; i < n; i++)
            {
                obstacles[i].UpdatePreviousInfo();
            }
        }

        private void Init()
        {
            _instance = this;

            UpdateViewingBox();
        }

        private void UpdateViewingBox(int index = 0)
        {
            GetLosCamera(index).UpdateViewingBox();
        }

        public void AddObstacle(LOSObstacle obstacle)
        {
            if (!obstacles.Contains(obstacle))
            {
                _isDirty = true;
                obstacles.Add(obstacle);
            }
        }

        public void RemoveObstacle(LOSObstacle obstacle)
        {
            obstacles.Remove(obstacle);
            _isDirty = true;
        }

        public void AddLight(LOSLightBase light)
        {
            if (!lights.Contains(light))
            {
                lights.Add(light);
            }
        }

        public void RemoveLight(LOSLightBase light)
        {
            lights.Remove(light);
        }

        public bool CheckDirty(int index)
        {
            if (_isDirty) return true;

            bool result = false;
            foreach (LOSObstacle obstacle in obstacles)
            {
                if (!obstacle.isStatic && obstacle.CheckDirty())
                {
                    result = true;
                }
            }

            if (!Application.isPlaying)
            {
                UpdatePreviousInfo(index);
            }

            return result;
        }


        // ------------ Helper Functions ------------
        public Vector3 GetPointForRadius(Vector3 origin, Vector3 direction, float radius)
        {
            float c = direction.magnitude;

            float x = radius * direction.x / c + origin.x;
            float y = radius * direction.y / c + origin.y;
            return new Vector3(x, y, 0);
        }


        // ------------End------------


        public enum PhysicsOpt
        {
            Physics_3D,
            Physics_2D,
        }

//		private class ViewBoxLine {
//			public Vector2 start {get; set;}
//			public Vector2 end {get; set;}
//
//			public void SetStartEnd (Vector2 start, Vector2 end) {
//				this.start = start;
//				this.end = end;
//			}
//		}
    }
}