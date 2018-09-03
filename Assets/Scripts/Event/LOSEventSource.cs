using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace LOS.Event {

	public class LOSEventSource : MonoBehaviour {

		private Transform _trans;

		[Tooltip ("Related light. Can be null if the event source is not a light.")]
		public LOSLightBase lightSource;
		public LayerMask triggerLayers;
		public LayerMask obstacleLayers;

		[Tooltip ("Event source detect range.")]
		public float distance;

	    private LOSManager _manager;

		public delegate void HandleTriggersDelegate (List<LOSEventTrigger> triggers);
		public event HandleTriggersDelegate OnNewTriggersDetected;
		public event HandleTriggersDelegate OnTriggersExitDetected;

		private List<LOSEventTrigger> _triggeredTriggers;


		void Awake () {
			_trans = transform;
			_triggeredTriggers = new List<LOSEventTrigger>();

		    _manager = GameObject.FindObjectOfType<LOSManager>();
		}

		void OnEnable () {
			LOSEventManager.TryGetInstance().AddEventSource(this);
		}
		
		void OnDisable () {
			if (LOSEventManager.TryGetInstance() != null) {
				LOSEventManager.TryGetInstance().RemoveEventSource(this);
			}
		}

		public void Clear () {
			if (OnTriggersExitDetected != null) {
				OnTriggersExitDetected(_triggeredTriggers);
			}
			_triggeredTriggers.Clear();
		}

		public void Process (List<LOSEventTrigger> triggers) {
			RaycastHit hit;
		    RaycastHit2D hit2D;

			List<LOSEventTrigger> triggeredTriggers = new List<LOSEventTrigger>();
			List<LOSEventTrigger> notTriggeredTriggers = new List<LOSEventTrigger>();

		    foreach (LOSEventTrigger trigger in triggers)
		    {

		        if (!SHelper.CheckGameObjectInLayer(trigger.gameObject, triggerLayers))
		        {
		            continue;
		        }

		        bool triggered = false;

		        Vector3 direction;

		        if (_manager.physicsOpt == LOSManager.PhysicsOpt.Physics_3D)
		        {
                    direction = trigger.position - _trans.position;
		        }
		        else
		        {
		            var v2d = new Vector3(1, 1);
                    direction = Vector3.Scale(trigger.position, v2d) - Vector3.Scale(_trans.position,v2d);
		        }
                
		        float degree = SMath.VectorToDegree(direction);

		        if (lightSource != null)
		        {
		            if (!lightSource.CheckDegreeWithinCone(degree))
		            {
		                notTriggeredTriggers.Add(trigger);
		                continue;
		            }
		        }

		        if (direction.sqrMagnitude <= distance*distance)
		        {
		            // Within distance
		            if (triggeredTriggers.Contains(trigger))
		            {
		                continue; // May be added previously
		            }

		            LayerMask mask = 1 << trigger.gameObject.layer | obstacleLayers;

		            if (_manager.physicsOpt == LOSManager.PhysicsOpt.Physics_3D)
		            {
		                if (Physics.Raycast(_trans.position, direction, out hit, distance, mask))
		                {
		                    GameObject hitGo = hit.collider.gameObject;

		                    if (hitGo == trigger.gameObject)
		                    {
		                        triggered = true;
		                    }
		                    else if (hitGo.layer == trigger.gameObject.layer)
		                    {
		                        LOSEventTrigger triggerToAdd = hitGo.GetComponentInChildren<LOSEventTrigger>();
		                        if (triggerToAdd == null)
		                        {
		                            triggerToAdd = hitGo.GetComponentInParent<LOSEventTrigger>();
		                        }
		                        triggeredTriggers.Add(triggerToAdd);
		                    }
		                }
		            }
		            else
		            {
		                hit2D = Physics2D.Raycast(_trans.position, direction, distance, mask);

                        GameObject hitGo = hit2D.collider.gameObject;

                        if (hitGo == trigger.gameObject)
                        {
                            triggered = true;
                        }
                        else if (hitGo.layer == trigger.gameObject.layer)
                        {
                            LOSEventTrigger triggerToAdd = hitGo.GetComponentInChildren<LOSEventTrigger>();
                            if (triggerToAdd == null)
                            {
                                triggerToAdd = hitGo.GetComponentInParent<LOSEventTrigger>();
                            }
                            triggeredTriggers.Add(triggerToAdd);
                        }
		            }
		        }

		        if (triggered)
		        {
		            triggeredTriggers.Add(trigger);
		        }
		        else
		        {
		            notTriggeredTriggers.Add(trigger);
		        }
		    }
            
		    List<LOSEventTrigger> newTriggers = new List<LOSEventTrigger>();
			foreach (LOSEventTrigger trigger in triggeredTriggers) {
				trigger.TriggeredBySource(this);

				if (!_triggeredTriggers.Contains(trigger)) {
					newTriggers.Add(trigger);
				}
			}
			if (OnNewTriggersDetected != null && newTriggers.Count > 0) {
				OnNewTriggersDetected(newTriggers);
			}

			List<LOSEventTrigger> triggersExit = new List<LOSEventTrigger>();
			foreach (LOSEventTrigger trigger in _triggeredTriggers) {
				if (!triggeredTriggers.Contains(trigger)) {
					triggersExit.Add(trigger);
				}
			}
			if (OnTriggersExitDetected != null && triggersExit.Count > 0) {
				OnTriggersExitDetected(triggersExit);
			}

			foreach (LOSEventTrigger trigger in notTriggeredTriggers) {
				trigger.NotTriggeredBySource(this);
			}

			_triggeredTriggers = triggeredTriggers;
		}
	}

}