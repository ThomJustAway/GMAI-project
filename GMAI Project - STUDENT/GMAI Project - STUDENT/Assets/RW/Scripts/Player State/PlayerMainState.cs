using PGGE.Patterns;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Drawing.Printing;
using UnityEngine;

namespace Player
{
    public class PlayerMainState : FSMState
    {
        protected FSM substates;
        protected Character character;

        public PlayerMainState(Character character , FSM mfsm)
        {
            this.character = character;
            mFsm = mfsm;
            //implement 
        }
    }

    public class PlayerMovementState : PlayerMainState
    {
        public PlayerMovementState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Movement;
            //todo add the states here
        }

        public override void Update()
        {
            DetermineStateChange();
        }
        public override void FixedUpdate()
        {
            MoveCharacter();
        }

        private void MoveCharacter()
        {
            float vertSpeed = Input.GetAxis("Vertical") * character.MovementSpeed;
            float horzontalSpeed = Input.GetAxis("Horizontal") * character.RotationSpeed;
            if(!Input.GetKey(KeyCode.LeftShift))
            { 
                horzontalSpeed /= 2;
                vertSpeed /= 2;
            }

            character.Move(vertSpeed, horzontalSpeed);
        }

        private void DetermineStateChange() 
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                mFsm.SetCurrentState((int)MainState.Crouch);
            }
        }
    }

    public class PlayerCrouchState : PlayerMainState
    {
        public PlayerCrouchState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Crouch;
            //todo set up sub states
        }

        //to fix: player can still exit crouch if under bridge. make sure
        //to not allow player to leave crouch state if something is above them.
        public override void Enter()
        {
            character.SetAnimationBool(character.Crouching, true);
            character.SetCrouchCollider(true);
        }

        public override void Exit()
        {
            character.SetAnimationBool(character.Crouching, false);
            character.SetCrouchCollider(false);

        }

        public override void Update()
        {
            DetermineStateChange();
        }

        public override void FixedUpdate()
        {
            MoveCharacter();
        }

        private void MoveCharacter()
        {
            float vertSpeed = Input.GetAxis("Vertical") * character.MovementSpeed;
            float horzontalSpeed = Input.GetAxis("Horizontal") * character.RotationSpeed;
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                horzontalSpeed /= 2;
                vertSpeed /= 2;
            }

            character.Move(vertSpeed, horzontalSpeed);
        }

        private void DetermineStateChange()
        {
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                mFsm.SetCurrentState((int)MainState.Movement);
            }
        }
    }

    public class PlayerJumpState : PlayerMainState
    {
        public PlayerJumpState(Character character, FSM mfsm) : base(character, mfsm)
        {
        }
    }

    public enum MainState
    {
        Movement,
        Jump,
        Falling,
        Crouch
    }

}