using PGGE.Patterns;
using RayWenderlich.Unity.StatePatternInUnity;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Player
{
    #region main state
    /// <summary>
    /// What the main state should consist.
    /// </summary>
    public class PlayerMainState : FSMState
    {
        protected FSM subFSM; //will contain the FSM for the sub states
        protected Character character;

        public PlayerMainState(Character character , FSM mfsm)
        {
            this.character = character;
            mFsm = mfsm;
            //implement 
        }
    }
    /// <summary>
    /// Movement state allows and control how the player move when
    /// they press the WASD keys.
    /// </summary>
    public class PlayerMovementState : PlayerMainState
    {
        public PlayerMovementState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)MainState.Movement;
        }

        public PlayerMovementState(Character character, FSM mfsm , FSM subFSm) : base(character, mfsm)
        {
            //add the subfsm so that the state can update the subFSM
            mId = (int)MainState.Movement;
            this.subFSM = subFSm;
        }

        public override void Update()
        {//run the handFSM here so that the player can attack
            subFSM.Update();
            //Determine what state should be made.
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
            //get input from the player
            if(!Input.GetKey(KeyCode.LeftShift))
            { 
                horzontalSpeed /= 2;
                vertSpeed /= 2;
            }
            //if it is not sprinting, then reduce the input by half.
            //this is to make sure the player walk rather than run.

            //move the character based on the input.
            character.Move(vertSpeed, horzontalSpeed);
        }

        private void DetermineStateChange() 
        {
            //specify the input need or condition needed to switch states
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
            else if (!character.IsGrounded)
            {
                mFsm.SetCurrentState((int)MainState.Falling);
            }
        }

    }
    /// <summary>
    /// Player crouch state is when the player can sneaky go through
    /// holes to go through small areas.
    /// </summary>
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
        {//show that the player is crouching
            character.SetAnimationBool(character.Crouching, true);
            character.SetCrouchCollider(true);
        }

        public override void Exit()
        {
            //show that the player is not crouching.
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
            //get input from player
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                horzontalSpeed /= 2;
                vertSpeed /= 2;
            }
            //if it is not sprinting, then reduce the input by half.
            //this is to make sure the player walk rather than run.

            //move the character based on the input.

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
    /// <summary>
    /// jump is when the player jumps when the player press spacebar.
    /// </summary>
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
    /// <summary>
    /// Falling state is when the player dont have anymore ground left to step on
    /// This cause it to start falling down.
    /// </summary>
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
            //will do a countdown timer, it will see if the player
            //been falling for a long time, if it is, then player would 
            //do a divebomb. Else it would do a normal landing.
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
            //once it landed, reset the timer and haslanded variable
            elapseTime = 0f;
            hasLanded = false;
            
        }
    }
    /// <summary>
    /// Rolling state allows the player to roll around the scene when pressing F
    /// </summary>
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
        {//show the player is rolling around
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

    //For boxing the selectedEnemy
    #region boxing
    /// <summary>
    /// boxing state is trigger if there is a nearby enemy that is willing
    /// to box. This force the player to do the same (Because I thought
    /// it would be cool)
    /// </summary>
    public class PlayerBoxingState : PlayerMainState
    {
        Transform enemy;
        
        public PlayerBoxingState(Character character, FSM mfsm, FSM subFSM) : base(character, mfsm)
        {
            mId = (int)MainState.Boxing;

            this.subFSM = subFSM;
            //change this later

            subFSM.SetCurrentState((int)BoxStates.Idle);
        }



        public override void Enter()
        {
            //show that the character is in boxing stance.
            character.SetBoxingStance(true);
            //stop the player from moving
            character.ResetMoveParams();

            //afterwards, find the enemy so that they can do the face off.
            var colliders = Physics.OverlapSphere(character.transform.position, character.BoxingSenseRadius);
            foreach(var collider in colliders)
            {
                if(collider.tag == "Enemy")
                {
                    enemy = collider.transform;
                    //stop the loop and break out
                    return;
                }
            }//get the selectedEnemy and face them
        }


        public override void Update()
        {
            //make sure that the player is facing the enemy
            FaceEnemy();
            //update the FSM for the boxing state.
            subFSM.Update();
        }


        void FaceEnemy()
        {
            Vector3 targetDirectionVec = enemy.transform.position - character.transform.position;
            if(Vector3.Angle(targetDirectionVec,character.transform.forward) < 5)
            {
                return;
            }

            Quaternion targetDirection = Quaternion.LookRotation(targetDirectionVec);
            Quaternion curDirection = character.transform.rotation;
            character.transform.rotation = Quaternion.Lerp(curDirection,
                targetDirection, Time.deltaTime * character.RotationSpeed);
        }

        public override void Exit()
        {
            subFSM.SetCurrentState((int)BoxStates.Idle);
            character.SetBoxingStance(false);
        }

    }
    /// <summary>
    /// The idle state for the player when it waits for the player input.
    /// </summary>
    public class IdleBoxing : FSMState
    {
        public IdleBoxing(FSM fsm, int id) : base(fsm, id)
        {
        }

        public override void Update()
        {
            DecideStates();
        }

        //will decide what attack the player would do if
        //it hits a certain input.
        private void DecideStates()
        {
            if (Input.GetMouseButtonUp(0))
            {
                mFsm.SetCurrentState((int)BoxStates.Block);
            }
            else if (Input.GetMouseButtonUp(1))
            {
                DecideOnPunch();
            }
        }
        /// <summary>
        /// will randomly do a punch to the enemy.
        /// </summary>
        void DecideOnPunch()
        {
            int randInt = Random.Range(0, 2);

            switch(randInt)
            {
                case 0:
                    mFsm.SetCurrentState((int)BoxStates.LeftJab);
                    break;
                default:
                    mFsm.SetCurrentState((int)BoxStates.RightJab);
                    break;
            }
        }
    }

    //the states the player have when boxing.
    public enum BoxStates
    {
        Idle,
        Block,
        LeftJab,
        RightJab,
    }
    #endregion

    /// <summary>
    /// The state where the player could not do anything since it is hurt.
    /// </summary>
    public class PlayerHurtState : PlayerMainState
    {
        private float recoveryPeriod;
        private float elapseTime;
        public PlayerHurtState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)(MainState.Hurt);
        }

        public override void Enter()
        {
            //would randomly decide how long the recovery for the player.
            elapseTime = 0f;
            recoveryPeriod = Random.Range(character.MinRecoveryTime, character.MaxRecoveryTime);
        }

        public override void Update()
        {
            //wait for the countdown of the recovery period.
            while(elapseTime < recoveryPeriod)
            {
                elapseTime += Time.deltaTime;
                return;
            }
            mFsm.SetCurrentState(PreviousState);
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
        Hurt,
        Boxing
    }
    #endregion

    #region substate
    /// <summary>
    /// What the state the player substate would inheritS.
    /// </summary>
    public class PlayerSubState : FSMState
    {
        protected Character character;

        public PlayerSubState(Character character , FSM mfsm)
        {
            this.character = character;
            this.mFsm = mfsm;
        }
    }
    /// <summary>
    /// When the player equip the sword, it can swing the sword which 
    /// the melee attack state handle it.
    /// </summary>
    public class PlayerMeleeAttack : PlayerSubState
    {
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
            //when entering the state, draw the sword.
            DrawSword();
        }

        public override void Update()
        {
            //wait for next input 

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //show the attack and enable hit box.
                sword.SetHitBox(true);
                character.TriggerAnimation(swingSwordAnimation);
            }
            else if (!anim.GetCurrentAnimatorStateInfo(1).IsName("SwingSword") )
            {
                //disable hit box.
                sword.SetHitBox(false);
            }
            DetermineNextState();

        }

        private void DrawSword()
        {
            character.TriggerAnimation(drawSwordAnimation);
            character.Equip(character.MeleeWeapon);
            sword = character.CurrentWeapon.GetComponent<SwordBehaviour>();
        }

        private void SheathSword()
        {
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
            //if player press q, unequip and exit the state.
            if (Input.GetKeyUp(KeyCode.Q))
            {
                mFsm.SetCurrentState((int)Substate.Twohand);
            }
        }
    }
    /// <summary>
    /// Player would equip the fireball as the range attack for the player
    /// to shoot out.
    /// </summary>
    public class PlayerRangeAttack : PlayerSubState
    {
        int shootAnimation = Animator.StringToHash("Shoot");

        public PlayerRangeAttack(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int) Substate.Range;
        }

        public override void Enter()
        {
            //show the animation for range.
            character.SetAnimationBool(character.isMelee, false);
            character.Equip(character.ShootableWeapon);
        }

        public override void Update()
        {
            DetermineNextState();
            //when player press the left mouse buttn, it would do a shoot animation
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
            //if the player press E, unequip and exit the state
            if (Input.GetKeyUp(KeyCode.E))
            {
                mFsm.SetCurrentState((int)Substate.Twohand);
            }
        }

    }
    /// <summary>
    /// A state where the player is just waiting for player input.
    /// </summary>
    public class PlayerTwoHandState : PlayerSubState
    {
        public PlayerTwoHandState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)Substate.Twohand;
        }

        public override void Enter()
        {
            character.SetAnimationBool(character.isMelee, true);
        }

        public override void Update()
        {
            //decide transition
            DecideTransition();
        }

        private void DecideTransition()
        {
            //will wait for player to trigger a important event.
            if (Input.GetKeyUp(KeyCode.Q))
            {
                //switch to melee
                mFsm.SetCurrentState((int)Substate.Melee);
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                mFsm.SetCurrentState((int)Substate.Range);
            }
            else if (Input.GetKeyUp(KeyCode.Alpha1))
            {
                mFsm.SetCurrentState((int)Substate.Wave);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                mFsm.SetCurrentState((int)Substate.Block);
            }
        }
    }
    /// <summary>
    /// A state to wave at the creature npc
    /// </summary>
    public class PlayerWaveState : PlayerSubState
    {
        int waveState = Animator.StringToHash("Wave");
        public PlayerWaveState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)Substate.Wave;
        }

        public override void Enter()
        {
            //will trigger wave state
            character.TriggerAnimation(waveState);
        }

        public override void Update()
        {
            //will check if the wave animation is over. once it is over, go back to twohand state.
            if ((character.Anim.GetCurrentAnimatorStateInfo(2).IsName("Wave") &&
                character.Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.7f)
                )
            {
                mFsm.SetCurrentState((int)Substate.Twohand);
            }
        }

        public override void Exit()
        {
            character.Anim.ResetTrigger(waveState);
        }

    }
    /// <summary>
    /// Block attack when the player click left clikc
    /// </summary>
    public class PlayerBlockState : PlayerSubState
    {
        int blockState = Animator.StringToHash("Block");
        public PlayerBlockState(Character character, FSM mfsm) : base(character, mfsm)
        {
            mId = (int)Substate.Block;
        }

        public override void Enter()
        {
            character.TriggerAnimation(blockState);
        }

        public override void Update()
        {
            //will wait for the block state is complete
            if ((character.Anim.GetCurrentAnimatorStateInfo(2).IsName("block") &&
                character.Anim.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.7f) 
                //!character.Anim.GetCurrentAnimatorStateInfo(2).IsName("block")
                )
            {
                mFsm.SetCurrentState((int)Substate.Twohand);
            }
        }

        public override void Exit()
        {
            character.Anim.ResetTrigger(blockState);
        }
    }

    public class AnimationStateWaiter : PlayerSubState
    {
        protected string nameAnimation;
        protected int hashAnimation;
        protected int layerIndex;
        protected float maxTimeWaitNormalize;
        protected Animator animator;
        protected int transitionState;
        public AnimationStateWaiter(Character character, FSM mfsm,
            string nameOfAnimation,
            string nameOfHashAnimation,
            int layerIndex,
            int transitionState,
            int stateId,
            float maxTimerNormalize = 0.7f
            ) : base(character, mfsm)
        {
            mId = stateId;
            nameAnimation = nameOfAnimation;
            hashAnimation = Animator.StringToHash(nameOfHashAnimation);
            maxTimeWaitNormalize = maxTimerNormalize;
            this.layerIndex = layerIndex;
            this.transitionState = transitionState;
            animator = character.Anim;
        }

        public override void Enter()
        {
            character.TriggerAnimation(hashAnimation);
        }

        public override void Update()
        {

            if (((animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(nameAnimation) &&
                animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime > maxTimeWaitNormalize) ||
                !animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(nameAnimation)
                )
                //!character.Anim.GetCurrentAnimatorStateInfo(2).IsName("block")
                )
            {
                mFsm.SetCurrentState(transitionState);
            }
        }

        public override void Exit()
        {
            character.Anim.ResetTrigger(hashAnimation);
        }
    }

    public enum Substate
    {
        Melee,
        Range,
        Twohand,
        Wave,
        Block,

    }
    #endregion
}