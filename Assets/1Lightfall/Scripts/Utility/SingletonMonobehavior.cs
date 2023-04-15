using Opsive.UltimateCharacterController.Game;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MBS.Lightfall
{
    public abstract class SingletonMonobehavior<T> : MonoBehaviour where T : Component
    {
        protected static T _instance;
        public static bool HasInstance => _instance != null;
        /// <summary>
        /// The singleton will lazy instanciate if no instance exists and Instance is called. In most cases, calling Instance is preffered.
        /// </summary>
        /// <returns>Returns Instance if Instance exists, otherwise returns null.</returns>
        public static T TryGetInstance() => HasInstance ? _instance : null;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name + "_AutoCreated";
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (HasInstance)
                Destroy(_instance.gameObject);

            _instance = this as T;

            if (gameObject.name.Contains("_AutoCreated"))
                OnAutoCreate();

        }

        /// <summary>
        /// Use this method to initalize some default values if an object is lazily instanciated
        /// </summary>
        protected virtual void OnAutoCreate()
        {

        }
    }



    public abstract class SingletonMonobehaviorOdinSerialized<T> : SerializedMonoBehaviour where T : Component
    {
        protected static T _instance;
        public static bool HasInstance => _instance != null;
        /// <summary>
        /// The singleton will lazy instanciate if no instance exists and Instance is called. In most cases, calling Instance is preffered.
        /// </summary>
        /// <returns>Returns Instance if Instance exists, otherwise returns null.</returns>
        public static T TryGetInstance() => HasInstance ? _instance : null;

        /// <summary>
        /// Singleton design pattern
        /// </summary>
        /// <value>The instance.</value>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name + "_AutoCreated";
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
        /// </summary>
        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            _instance = this as T;
        }
    }
}
