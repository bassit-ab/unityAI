using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public abstract class AbstractSavable : MonoBehaviour
    {
        [SerializeField, HideInInspector] protected string id;
        public string Id { get => id; set => id = value; }
        public Vector3 Position => transform.position;
    }
}
