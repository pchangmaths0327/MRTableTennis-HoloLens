// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Examples.Demos;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Microsoft.MixedReality.Toolkit.Experimental.SceneUnderstanding
{
    /// <summary>
    /// Demo class to show different ways of visualizing the space using scene understanding.
    /// </summary>
    public class DemoSceneUnderstandingController : DemoSpatialMeshHandler, IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>
    {
        public GameObject TableSurface = null;
        public TextMeshPro ScanningText = null;
        public TextMeshPro ScanningSubText = null;
        public TextMeshPro DetectedText = null;
        public TextMeshPro DetectedSubText = null;
        public GameObject Player = null;

        #region Private Fields

        private bool detected = false;

        #region Serialized Fields

        [SerializeField]
        private string SavedSceneNamePrefix = "DemoSceneUnderstanding";

        #endregion Serialized Fields

        private IMixedRealitySceneUnderstandingObserver observer;

        private Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>> observedSceneObjects;

        #endregion Private Fields

        #region MonoBehaviour Functions

        protected override void Start()
        {
            observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySceneUnderstandingObserver>();

            if (observer == null)
            {
                Debug.LogError("Couldn't access Scene Understanding Observer! Please make sure the current build target is set to Universal Windows Platform. "
                    + "Visit https://docs.microsoft.com/windows/mixed-reality/mrtk-unity/features/spatial-awareness/scene-understanding for more information.");
                return;
            }

            InitPlatformDetection();
            observedSceneObjects = new Dictionary<SpatialAwarenessSurfaceTypes, Dictionary<int, SpatialAwarenessSceneObject>>();
        }

        void Update()
        {
            if (!detected && observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Platform, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                UpdateScene();
            }
        }

        protected override void OnEnable()
        {
            RegisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDisable()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        protected override void OnDestroy()
        {
            UnregisterEventHandlers<IMixedRealitySpatialAwarenessObservationHandler<SpatialAwarenessSceneObject>, SpatialAwarenessSceneObject>();
        }

        #endregion MonoBehaviour Functions

        #region IMixedRealitySpatialAwarenessObservationHandler Implementations

        /// <inheritdoc />
        public void OnObservationAdded(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            // This method called everytime a SceneObject created by the SU observer
            // The eventData contains everything you need do something useful

            AddToData(eventData.Id);

            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                eventData.SpatialObject.GameObject.SetActive(eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.World);
                sceneObjectDict.Add(eventData.Id, eventData.SpatialObject);
            }
            else
            {
                eventData.SpatialObject.GameObject.SetActive(eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.World);
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }
        }

        /// <inheritdoc />
        public void OnObservationUpdated(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            UpdateData(eventData.Id);

            if (observedSceneObjects.TryGetValue(eventData.SpatialObject.SurfaceType, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                eventData.SpatialObject.GameObject.SetActive(eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.World);
                observedSceneObjects[eventData.SpatialObject.SurfaceType][eventData.Id] = eventData.SpatialObject;
            }
            else
            {
                eventData.SpatialObject.GameObject.SetActive(eventData.SpatialObject.SurfaceType == SpatialAwarenessSurfaceTypes.World);
                observedSceneObjects.Add(eventData.SpatialObject.SurfaceType, new Dictionary<int, SpatialAwarenessSceneObject> { { eventData.Id, eventData.SpatialObject } });
            }
        }

        /// <inheritdoc />
        public void OnObservationRemoved(MixedRealitySpatialAwarenessEventData<SpatialAwarenessSceneObject> eventData)
        {
            RemoveFromData(eventData.Id);

            foreach (var sceneObjectDict in observedSceneObjects.Values)
            {
                sceneObjectDict?.Remove(eventData.Id);
            }
        }

        #endregion IMixedRealitySpatialAwarenessObservationHandler Implementations

        #region Public Functions

        #region UI Functions

        public void UpdateTableToNearest()
        {
            observer.Suspend();
            if(observedSceneObjects.TryGetValue(SpatialAwarenessSurfaceTypes.Platform, out Dictionary<int, SpatialAwarenessSceneObject> sceneObjectDict))
            {
                // find nearest platform to user
                float minDist = 1000.0f;
                int minID = -1;
                foreach(var obj in sceneObjectDict)
                {
                    var objPos2D = new Vector2(obj.Value.Position.x, obj.Value.Position.z);
                    var playerPos2D = new Vector2(Player.transform.position.x, Player.transform.position.z);
                    if((objPos2D - playerPos2D).magnitude < minDist)
                    {
                        minDist = (objPos2D - playerPos2D).magnitude;
                        minID = obj.Key;
                    }
                }

                // set table to that platfom location
                var nearestObj = observedSceneObjects[SpatialAwarenessSurfaceTypes.Platform][minID];
                TableSurface.transform.SetPositionAndRotation(nearestObj.Position, nearestObj.Rotation);
                float sx = nearestObj.Quads[0].Extents.x;
                float sy = nearestObj.Quads[0].Extents.y;
                TableSurface.transform.localScale = new Vector3(sx, sy, .1f);
                ScanningText.alpha = 0.0f;
                ScanningSubText.alpha = 0.0f;
                DetectedText.alpha = 1.0f;
                DetectedSubText.alpha = 1.0f;
                detected = true;
            }
            else
            {
                Debug.Log("No Platform object detected");
            }
            //observer.Resume();
        }

        /// <summary>
        /// Request the observer to update the scene
        /// </summary>
        public void UpdateScene()
        {
            observer.Resume();
            observer.UpdateOnDemand();
            UpdateTableToNearest();
        }

        /// <summary>
        /// Request the observer to save the scene
        /// </summary>
        public void SaveScene()
        {
            observer.SaveScene(SavedSceneNamePrefix);
        }

        /// <summary>
        /// Request the observer to clear the observations in the scene
        /// </summary>
        public void ClearScene()
        {
            observer.ClearObservations();
        }

        public void EndPlatformDetection()
        {
            observer.SurfaceTypes = 0;
            observer.RequestMeshData = false;
            observer.RequestPlaneData = false;

            observer.UpdateOnDemand();
            ClearScene();
            observer.Suspend();
        }

        #endregion UI Functions

        #endregion Public Functions

        #region Helper Functions

        private void InitPlatformDetection()
        {
            observer.SurfaceTypes = SpatialAwarenessSurfaceTypes.Platform; // | SpatialAwarenessSurfaceTypes.World;

            observer.RequestMeshData = true;
            observer.RequestPlaneData = true;
        }

        private void ClearAndUpdateObserver()
        {
            ClearScene();
            observer.UpdateOnDemand();
            UpdateTableToNearest();
        }

        #endregion Helper Functions
    }
}
