/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateCharacterController.Game
{
    /// <summary>
    /// Interface for any kinematic object that can be moved.
    /// </summary>
    public interface IKinematicObject
    {
        /// <summary>
        /// Moves the object.
        /// </summary>
        void Move();
    }
}