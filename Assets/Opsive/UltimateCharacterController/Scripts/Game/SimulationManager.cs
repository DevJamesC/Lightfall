/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateCharacterController.Camera;
    using Opsive.UltimateCharacterController.Character;
    using Opsive.UltimateCharacterController.Game;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Manages when the character and cameras should move.
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        private static SimulationManager s_Instance;
        private static SimulationManager Instance
        {
            get
            {
                if (!s_Initialized) {
                    s_Instance = new GameObject("SimulationManager").AddComponent<SimulationManager>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }
        private static bool s_Initialized;

        /// <summary>
        /// Stores the CharacterLocomotion and handler.
        /// </summary>
        private struct CharacterComponents
        {
            [Tooltip("A reference to the CharacterLocomotion.")]
            public CharacterLocomotion Locomotion;
            [Tooltip("A reference to the CharacterHandler.")]
            public ICharacterHandler Handler;

            /// <summary>
            /// Moves the character.
            /// </summary>
            public void Move()
            {
                float horizontalMovement = 0, forwardMovement = 0, deltaYaw = 0;
                if (Handler != null) {
                    Handler.GetPositionInput(out horizontalMovement, out forwardMovement);
                    Handler.GetRotationInput(horizontalMovement, forwardMovement, out deltaYaw);
                }
                Locomotion.Move(horizontalMovement, forwardMovement, deltaYaw);
            }
        }

        /// <summary>
        /// Stores the CameraController and handler.
        /// </summary>
        private struct CameraComponents
        {
            [Tooltip("A reference to the CameraController.")]
            public CameraController Camera;
            [Tooltip("A reference to the CameraControllerHandler.")]
            public CameraControllerHandler Handler;

            /// <summary>
            /// Rotates the character.
            /// </summary>
            public void Rotate()
            {
                float horizontalMovement = 0, forwardMovement = 0;
                if (Handler != null) {
                    Handler.GetMoveInput(out horizontalMovement, out forwardMovement);
                }
                Camera.Rotate(horizontalMovement, forwardMovement);
            }

            /// <summary>
            /// Moves the character.
            /// </summary>
            public void Move()
            {
                float horizontalMovement = 0, forwardMovement = 0;
                if (Handler != null) {
                    Handler.GetMoveInput(out horizontalMovement, out forwardMovement);
                }
                Camera.Move(horizontalMovement, forwardMovement);
            }
        }

        private ResizableArray<CharacterComponents> m_Characters = new ResizableArray<CharacterComponents>();
        private ResizableArray<CameraComponents> m_Cameras = new ResizableArray<CameraComponents>();
        private ResizableArray<IKinematicObject> m_KinematicObjects = new ResizableArray<IKinematicObject>();

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null) {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        /// Registers the character with the SimulationManager.
        /// </summary>
        /// <param name="CharacterLocomotion">The motor of the character that should be registered.</param>
        public static void RegisterCharacter(CharacterLocomotion CharacterLocomotion)
        {
            Instance.RegisterCharacterInternal(CharacterLocomotion);
        }

        /// <summary>
        /// Internal method which registers the character with the SimulationManager.
        /// </summary>
        /// <param name="CharacterLocomotion">The motor of the character that should be registered.</param>
        private void RegisterCharacterInternal(CharacterLocomotion CharacterLocomotion)
        {
            m_Characters.Add(new CharacterComponents { Locomotion = CharacterLocomotion, Handler = CharacterLocomotion.GetComponent<ICharacterHandler>() });
        }

        /// <summary>
        /// Registers the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be registered.</param>
        public static void RegisterCamera(CameraController cameraController)
        {
            Instance.RegisterCameraInternal(cameraController);
        }

        /// <summary>
        /// Internal method which registers the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be registered.</param>
        private void RegisterCameraInternal(CameraController cameraController)
        {
            m_Cameras.Add(new CameraComponents { Camera = cameraController, Handler = cameraController.GetComponent<CameraControllerHandler>() });
        }

        /// <summary>
        /// Registers the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be registered.</param>
        public static void RegisterKinematicObject(IKinematicObject kinematicObject)
        {
            Instance.RegisterKinematicObjectInternal(kinematicObject);
        }

        /// <summary>
        /// Internal method which registers the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be registered.</param>
        private void RegisterKinematicObjectInternal(IKinematicObject kinematicObject)
        {
            m_KinematicObjects.Add(kinematicObject);
        }

        /// <summary>
        /// Unregisters the character with the SimulationManager.
        /// </summary>
        /// <param name="CharacterLocomotion">The motor of the character that should be unregistered.</param>
        public static void UnregisterCharacter(CharacterLocomotion CharacterLocomotion)
        {
            Instance.UnregisterCharacterInternal(CharacterLocomotion);
        }

        /// <summary>
        /// Internal method which unregisters the character with the SimulationManager.
        /// </summary>
        /// <param name="CharacterLocomotion">The motor of the character that should be unregistered.</param>
        private void UnregisterCharacterInternal(CharacterLocomotion CharacterLocomotion)
        {
            m_Characters.Remove(new CharacterComponents { Locomotion = CharacterLocomotion, Handler = CharacterLocomotion.GetComponent<ICharacterHandler>() });
        }

        /// <summary>
        /// Unregisters the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be unregistered.</param>
        public static void UnregisterCamera(CameraController cameraController)
        {
            Instance.UnregisterCameraInternal(cameraController);
        }

        /// <summary>
        /// Internal method which unregisters the camera with the SimulationManager.
        /// </summary>
        /// <param name="cameraController">The camera controller that should be unregistered.</param>
        private void UnregisterCameraInternal(CameraController cameraController)
        {
            m_Cameras.Remove(new CameraComponents { Camera = cameraController, Handler = cameraController.GetComponent<CameraControllerHandler>() });
        }

        /// <summary>
        /// Unregisters the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be unregistered.</param>
        public static void UnregisterKinematicObject(IKinematicObject kinematicObject)
        {
            Instance.UnregisterKinematicObjectInternal(kinematicObject);
        }

        /// <summary>
        /// Internal method which unregisters the kinematic object with the SimulationManager.
        /// </summary>
        /// <param name="kinematicObject">The kinematic object that should be unregistered.</param>
        private void UnregisterKinematicObjectInternal(IKinematicObject kinematicObject)
        {
            m_KinematicObjects.Remove(kinematicObject);
        }

        /// <summary>
        /// Executes during the FixedUpdate loop.
        /// </summary>
        private void FixedUpdate()
        {
            RotateCameras();

            MoveKinematicObjects();

            MoveCharacters();

            MoveCameras();
        }

        /// <summary>
        /// Rotates the cameras.
        /// </summary>
        public void RotateCameras()
        {
            for (int i = 0; i < m_Cameras.Count; ++i) {
                m_Cameras[i].Rotate();
            }
        }

        /// <summary>
        /// Moves the kinematic objects.
        /// </summary>
        public void MoveKinematicObjects()
        {
            for (int i = 0; i < m_KinematicObjects.Count; ++i) {
                m_KinematicObjects[i].Move();
            }
        }

        /// <summary>
        /// Moves the characters.
        /// </summary>
        public void MoveCharacters()
        {
            for (int i = 0; i < m_Characters.Count; ++i) {
                m_Characters[i].Move();
            }
        }

        /// <summary>
        /// Moves the cameras.
        /// </summary>
        public void MoveCameras()
        {
            for (int i = 0; i < m_Cameras.Count; ++i) {
                m_Cameras[i].Move();
            }
        }

        /// <summary>
        /// Sets the update order so the the first character executes before the second character.
        /// </summary>
        /// <param name="firstLocomotion">A reference to the character that should be executed first.</param>
        /// <param name="secondLocomotion">A reference to the character that should be executed second.</param>
        public static void SetUpdateOrder(CharacterLocomotion firstLocomotion, CharacterLocomotion secondLocomotion)
        {
            // Schedule the reorder so the elements aren't swapped as they are executing.
            Shared.Game.Scheduler.ScheduleFixed(Time.fixedDeltaTime / 2, Instance.SetUpdateOrderInternal, firstLocomotion, secondLocomotion);
        }

        /// <summary>
        /// Internal method which sets the update order so the the first character executes before the second character.
        /// </summary>
        /// <param name="firstLocomotion">A reference to the character that should be executed first.</param>
        /// <param name="secondLocomotion">A reference to the character that should be executed second.</param>
        public void SetUpdateOrderInternal(CharacterLocomotion firstLocomotion, CharacterLocomotion secondLocomotion)
        {
            var firstIndex = -1;
            var secondIndex = -1;
            for (int i = 0; i < m_Characters.Count; ++i) {
                if (m_Characters[i].Locomotion == firstLocomotion) {
                    firstIndex = i;
                } else if (m_Characters[i].Locomotion == secondLocomotion) {
                    secondIndex = i;
                }

                if (firstIndex != -1 && secondIndex != -1) {
                    break;
                }
            }

            if (firstIndex == -1 || secondIndex == -1) {
                Debug.LogError($"Error: The character {(firstIndex == -1 ? firstLocomotion.name.ToString() : secondLocomotion.ToString())} is not registered with the SimulationManager so the update order cannot be changed.");
                return;
            }

            // No changes need to be made.
            if (firstIndex < secondIndex) {
                return;
            }

            // Swap the elements.
            var tempElement = m_Characters[firstIndex];
            m_Characters[firstIndex] = m_Characters[secondIndex];
            m_Characters[secondIndex] = tempElement;
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded(Scene scene)
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        /// <summary>
        /// Reset the static variables for domain reloading.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void DomainReset()
        {
            s_Instance = null;
            s_Initialized = false;
        }
    }
}