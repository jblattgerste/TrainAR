using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using Remote;

namespace Remote
{
    /// <summary>
    /// Holds metadata for a trainAR object.
    /// </summary>
    [Serializable, XmlRoot("StreamingData")]
    public class TrainARObjectValues
    {
        /// <summary>
        /// Serialized transform of the trainAR object.
        /// </summary>
        public SerializedTransform transform;
        /// <summary>
        /// Name of the object.
        /// </summary>
        public string name;
        /// <summary>
        /// True im grabbable.
        /// </summary>
        public bool isGrabbable;
        /// <summary>
        /// True if interactable.
        /// </summary>
        public bool isInteractable;
        /// <summary>
        /// true if combineable.
        /// </summary>
        public bool isCombinable;
        /// <summary>
        /// true if the object is disabled at the start of the scenario.
        /// </summary>
        public bool disabledOnStart;
        /// <summary>
        /// Lerping distance to the camera when grabbed.
        /// </summary>
        public float LerpingDistance;
        /// <summary>
        /// Serialized mesh of the object.
        /// </summary>
        public SerializedMesh mesh;
        /// <summary>
        /// Serialized material of the object. 
        /// </summary>
        public SerializedMaterial[] materials;
        /// <summary>
        /// Empty constructor.
        /// </summary>
        public TrainARObjectValues()
        {

        }
        /// <summary>
        /// Constructor for the value holder.
        /// </summary>
        /// <param name="selectedObject">TrainAR gameobject to get the data from.</param>
        public TrainARObjectValues(GameObject selectedObject)
        {
            transform = new SerializedTransform(selectedObject.GetComponent<Transform>());
            TrainARObject tempTrainARObject = selectedObject.GetComponent<TrainARObject>();
            name = tempTrainARObject.interactableName;
            isGrabbable = tempTrainARObject.isGrabbable;
            isInteractable = tempTrainARObject.isInteractable;
            isCombinable = tempTrainARObject.isCombineable;
            disabledOnStart = tempTrainARObject.TrainARObjectDisabled;
            LerpingDistance = tempTrainARObject.lerpingDistance;
            mesh = new SerializedMesh(selectedObject.GetComponent<MeshFilter>().sharedMesh);
            materials = new SerializedMaterial().getSerializedMaterials(mesh.submeshes.Length, selectedObject.GetComponent<MeshRenderer>().sharedMaterials);
        }
        /// <summary>
        /// Sets the stored values on the referenced gameobject.
        /// </summary>
        /// <param name="trainARObject">Empty TrainAR object.</param>
        public void setTrainARObjectValues(GameObject trainARObject)
        {
            TrainARObject tempTrainARObject = trainARObject.GetComponent<TrainARObject>();
            tempTrainARObject.interactableName = name;
            tempTrainARObject.isGrabbable = isGrabbable;
            tempTrainARObject.isInteractable = isInteractable;
            tempTrainARObject.isCombineable = isCombinable;
            tempTrainARObject.TrainARObjectDisabled = disabledOnStart;
            tempTrainARObject.lerpingDistance = LerpingDistance;
        }
    }
}

