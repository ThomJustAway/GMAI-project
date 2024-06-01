using PGGE.Patterns;
using RayWenderlich.Unity.StatePatternInUnity;
using UnityEngine;

namespace Player
{
    public class PlayerMainState : FSMState
    {
        protected FSM subFSM;
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

        public PlayerMovementState(Character character, FSM mfsm , FSM subFSm) : base(character, mfsm)
        {
            mId = (int)MainState.Movement;
            this.subFSM = subFSm;
            //todo add the states here
        }

        public override void Update()
        {
            subFSM.Update();
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
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                mFsm.SetCurrentState((int)MainState.Jump);
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                mFsm.SetCurrentState((int)MainState.Rolling);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                mFsm.SetCurrentState((int)MainState.Wave);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                mFsm.SetCurrentState((int)MainState.Block);
            }
            else if (!character.IsGrounded)
            {
                mFsm.SetCurrentState((int)MainState.Falling);
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

        public PlayerCrouchState(Character character, FSM mfsm , FSM subStateFSM) : base(character, mfsm)
        {
            mId = (int)MainState.Crouch;
            subFSM = subStateFSM;
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
            subFSM.Update();
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
            if (Input.GetKeyUp(KeyCode.Tab) && !character.IsSomethingAbove)
            {
                mFsm.SetCurrentState((int)MainState.Movement);
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                mFsm.SetCurrentState((int)MainState.Jump);
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                mFsm.SetCurrentState((int)MainState.Rolling);
            }
            else if (!character.IsGrounded)
            {
                mFsm.SetCurrentState((int)MainState.Falling);
            }
        }
    }

    public class PlayerJumpState : PlayerMainState
    {
        private int jumpParam = Animator.StringToHash("Jump");

        public PlayerJumpState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Jump;
        }

        

        public override void Enter()
        {
            Jump();
            mFsm.SetCurrentState((int)MainState.Falling);
        }

        private void Jump()
        {
            character.transform.Translate(Vector3.up * (character.CollisionOverlapRadius + 0.1f));
            character.ApplyImpulse(Vector3.up * character.JumpForce);
            character.TriggerAnimation(jumpParam);
        }
    }

    public class PlayerFallingState : PlayerMainState
    {
        int landParam = Animator.StringToHash("Land");
        int hardLand = Animator.StringToHash("HardLand");
        float elapseTime;
        bool hasLanded;
        public PlayerFallingState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Falling;
        }

        public override void Enter()
        {
            character.Anim.ResetTrigger(landParam);
            character.Anim.ResetTrigger(hardLand);
        }

        public override void Update()
        {
            elapseTime += Time.deltaTime;
            if (character.IsGrounded && !hasLanded)
            {
                hasLanded = true;   
                if(elapseTime < 2f)
                {
                    character.TriggerAnimation(landParam);
                }
                else 
                {
                    character.DiveBomb();
                }
                mFsm.SetCurrentState((int)MainState.Movement);
            }
        }


        public override void Exit()
        {
            elapseTime = 0f;
            hasLanded = false;
            
        }
    }

    public class PlayerRollingState : PlayerMainState
    {
        int rollAnimation = Animator.StringToHash("Roll");
        Animator animator;
        public PlayerRollingState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Rolling;
            animator = character.Anim;
        }

        public override void Enter()
        {
            character.TriggerAnimation(rollAnimation);
            character.SetCrouchCollider(true);
        }

        public override void FixedUpdate()
        {
            float vertInput = Input.GetAxis("Vertical");
            if(vertInput > 0)
            {
                //will continuously apply impulse.
                character.ApplyImpulse(character.transform.forward * character.RollForce);
            }
            if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7 && 
                animator.GetCurrentAnimatorStateInfo(0).IsName("Roll"))
            {
                //if the animation about to end then move to another state;
                if(PreviousState.ID == (int)MainState.Movement && 
                    character.IsSomethingAbove)
                {
                    mFsm.SetCurrentState((int)MainState.Crouch);
                }
                else
                {
                    mFsm.SetCurrentState(PreviousState);
                }
            }
        }

        public override void Exit()
        {
            character.SetCrouchCollider(false);
            animator.ResetTrigger(rollAnimation);
        }


    }

    public class PlayerWaveState : PlayerMainState
    {
        int waveState = Animator.StringToHash("Wave");
        public PlayerWaveState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Wave;
        }

        public override void Enter()
        {
            character.TriggerAnimation(waveState);
        }

        public override void Update()
        {

            if (character.Anim.GetCurrentAnimatorStateInfo(0).IsName("Wave") &&
                character.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
            {
                mFsm.SetCurrentState((int)MainState.Movement);
            }
        }

        public override void Exit()
        {
            character.Anim.ResetTrigger(waveState);
        }

    }

    public class PlayerBlockState : PlayerMainState
    {
        int blockState = Animator.StringToHash("Block");
        public PlayerBlockState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Block;
        }

        public override void Enter()
        {
            character.TriggerAnimation(blockState);
        }
        public override void Update()
        {

            if (character.Anim.GetCurrentAnimatorStateInfo(0).IsName("block") &&
                character.Anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
            {
                mFsm.SetCurrentState((int)MainState.Movement);
            }
        }

        public override void Exit()
        {
            character.Anim.ResetTrigger(blockState);
        }
    }

    public enum MainState
    {
        Movement,
        Jump,
        Falling,
        Crouch,
        Rolling,
        Wave,
        Block,
        
    }

    public class PlayerSubState : FSMState
    {
        protected Character character;

        public PlayerSubState(Character character , FSM mfsm)
        {
            this.character = character;
            this.mFsm = mfsm;
        }
    }

    public class PlayerMeleeAttack : PlayerSubState
    {
        bool hasDrawSword;
        SwordBehaviour sword;
        int sheathSwordAnimation = Animator.StringToHash("SheathMelee");
        int swingSwordAnimation = Animator.StringToHash("SwingMelee");
        int drawSwordAnimation = Animator.StringToHash("DrawMelee");
        Animator anim;
        public PlayerMeleeAttack(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)Substate.Melee;
            anim = character.Anim;
        }

        public override void Enter()
        {
            character.SetAnimationBool(character.isMelee, true);
            hasDrawSword = false;
            anim.ResetTrigger(drawSwordAnimation);
            anim.ResetTrigger(swingSwordAnimation);
            anim.ResetTrigger(sheathSwordAnimation);
        }

        public override void Update()
        {
            DetermineNextState();

            if (Input.GetKeyUp(KeyCode.Q) )
            {
                if (hasDrawSword)
                {
                    SheathSword();
                }
                else
                {
                    DrawSword();
                }
            }

            if (!hasDrawSword) return;
            //wait for next input 

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //show the attack
                sword.SetHitBox(true);
                character.TriggerAnimation(swingSwordAnimation);
            }
            else if (!anim.GetCurrentAnimatorStateInfo(1).IsName("SwingSword") )
            {
                sword.SetHitBox(false);
            }

        }

        private void DrawSword()
        {
            hasDrawSword = true;
            character.TriggerAnimation(drawSwordAnimation);
            character.Equip(character.MeleeWeapon);
            sword = character.CurrentWeapon.GetComponent<SwordBehaviour>();
        }

        private void SheathSword()
        {
            hasDrawSword = false;
            character.TriggerAnimation(sheathSwordAnimation);
            sword = null;
            character.Unequip();
        }

        public override void Exit()
        {
            SheathSword();
        }

        void DetermineNextState()
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                mFsm.SetCurrentState((int)Substate.Range);
            }
        }
    }

    public class PlayerRangeAttack : PlayerSubState
    {
        int shootAnimation = Animator.StringToHash("Shoot");

        public PlayerRangeAttack(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int) Substate.Range;
        }

        public override void Enter()
        {
            character.SetAnimationBool(character.isMelee, false);
            character.Equip(character.ShootableWeapon);
        }

        public override void Update()
        {
            DetermineNextState();
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //show the attack
                character.TriggerAnimation(shootAnimation);
                character.Shoot();
            }
        }

        public override void Exit()
        {
            character.Unequip();
        }

        void DetermineNextState()
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                mFsm.SetCurrentState((int)Substate.Melee);
            }
        }

    }

    public enum Substate
    {
        Melee,
        Range,
    }

}