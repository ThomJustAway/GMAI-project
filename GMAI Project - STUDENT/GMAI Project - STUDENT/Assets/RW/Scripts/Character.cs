/*
 * Copyright (c) 2019 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using Assets.RW.Scripts;
using PGGE.Patterns;
using Player;
using UnityEngine;

namespace RayWenderlich.Unity.StatePatternInUnity
{
    /// <summary>
    /// Character is teh script that will control the player actions and what they do.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class Character : MonoBehaviour, IDamageable
    {
        #region Variables


#pragma warning disable 0649
        [SerializeField]
        private Transform handTransform;
        [SerializeField]
        private Transform sheathTransform;
        [SerializeField]
        private Transform shootTransform;
        [SerializeField]
        private CharacterData data;
        [SerializeField]
        private LayerMask whatIsGround;
        [SerializeField]
        private Collider hitBox;
        [SerializeField]
        private CapsuleCollider playerCollider;
        [SerializeField]
        private Animator anim;
        [SerializeField]
        private ParticleSystem shockWave;
        [SerializeField]
        private Transform playerFoot;
        [SerializeField]
        private Transform playerHead;
        [SerializeField]
        private float Sensor;
#pragma warning restore 0649
        [SerializeField]
        private float meleeRestThreshold = 10f;
        [SerializeField]
        private float diveThreshold = 1f;
        [SerializeField]
        private float collisionOverlapRadius = 0.1f;
        [SerializeField]
        private float rollForce = 0.5f;

        [SerializeField]
        private float minRecoveryTime;
        [SerializeField]
        private float maxRecoveryTime;

        [SerializeField]
        private float boxingSenseRadius = 2f;

        private GameObject currentWeapon;
        private int horizonalMoveParam = Animator.StringToHash("H_Speed");
        private int verticalMoveParam = Animator.StringToHash("V_Speed");
        private int shootParam = Animator.StringToHash("Shoot");
        private int hardLanding = Animator.StringToHash("HardLand");
        private int crouching = Animator.StringToHash("Crouch");
        private int hurtAnim = Animator.StringToHash("Hurt");
        private int boxingStanceAnimation = Animator.StringToHash("Box");

        
        #endregion

        #region Properties

        public float NormalColliderHeight => data.normalColliderHeight;
        public float CrouchColliderHeight => data.crouchColliderHeight;
        public float DiveForce => data.diveForce;
        public float JumpForce => data.jumpForce;
        public float MovementSpeed => data.movementSpeed;
        public float CrouchSpeed => data.crouchSpeed;
        public float RotationSpeed => data.rotationSpeed;
        public float CrouchRotationSpeed => data.crouchRotationSpeed;
        public GameObject MeleeWeapon => data.meleeWeapon;
        public GameObject ShootableWeapon => data.staticShootable;
        public float DiveCooldownTimer => data.diveCooldownTimer;
        public float CollisionOverlapRadius => collisionOverlapRadius;
        public float DiveThreshold => diveThreshold;
        public float MeleeRestThreshold => meleeRestThreshold;
        public int isMelee => Animator.StringToHash("IsMelee");
        public int crouchParam => Animator.StringToHash("Crouch");

        public float ColliderSize
        {
            get => GetComponent<CapsuleCollider>().height;

            set
            {
                GetComponent<CapsuleCollider>().height = value;
                Vector3 center = GetComponent<CapsuleCollider>().center;
                center.y = value / 2f;
                GetComponent<CapsuleCollider>().center = center;
            }
        }

        public int Crouching { get => crouching;}

        public bool IsGrounded { get => CheckIfGrounded(); }
        public bool IsSomethingAbove { get => CheckIfSomethingIsAbove(); }
        public Animator Anim { get => anim; set => anim = value; }
        public float RollForce { get => rollForce; set => rollForce = value; }
        #endregion

        //fsm for the Hierarchical FSM  
        private FSM movementFSM;
        private FSM handFSM;
        private FSM boxingFSM;
        //getters
        public FSMState currentPlayerState { get { return movementFSM.GetCurrentState(); } }
        public FSMState subStatePlayerState { get { return handFSM.GetCurrentState(); } }
        public GameObject CurrentWeapon { get => currentWeapon; set => currentWeapon = value; }
        public float MinRecoveryTime { get => minRecoveryTime; set => minRecoveryTime = value; }
        public float MaxRecoveryTime { get => maxRecoveryTime; set => maxRecoveryTime = value; }
        public float BoxingSenseRadius { get => boxingSenseRadius; set => boxingSenseRadius = value; }

        private void Start()
        {
            playerCollider = GetComponent<CapsuleCollider>();
            
            //setting up the FSM for both sub and main states.
            //hand fsm is to control the sword, blocking and wave action
            handFSM = new FSM();
            handFSM.Add(new PlayerMeleeAttack(this, handFSM));
            handFSM.Add(new PlayerRangeAttack(this, handFSM));
            handFSM.Add(new PlayerWaveState(this, handFSM));
            handFSM.Add(new PlayerBlockState(this, handFSM));
            handFSM.Add(new PlayerTwoHandState(this, handFSM));
            handFSM.SetCurrentState((int)Substate.Twohand);


            //boxing is when it encounters the boxing enemy, it would trigger the boxing
            //state to start boxing with the enemy.
            boxingFSM = new();
            boxingFSM.Add(new AnimationStateWaiter(
                this,
                boxingFSM,
                "punch 2",
                "Punch1",
                0,
                (int)BoxStates.Idle,
                (int)BoxStates.RightJab
                ));
            boxingFSM.Add(new AnimationStateWaiter(this,
                boxingFSM,
                "punch",
                "Punch2",
                0,
                (int)BoxStates.Idle,
                (int)BoxStates.LeftJab
                ));
            boxingFSM.Add(new AnimationStateWaiter(this,
                boxingFSM,
                "block",
                "Block",
                2,
                (int)BoxStates.Idle,
                (int)BoxStates.Block,
                1f
                ));

            boxingFSM.Add(new IdleBoxing(boxingFSM, (int)BoxStates.Idle));
            boxingFSM.SetCurrentState((int)BoxStates.Idle);

            //movement state is where the player move its entire body. This include
            //moving around, jumping and rolling
            movementFSM = new FSM();
            movementFSM.Add(new PlayerMovementState(this,movementFSM , handFSM));
            movementFSM.Add(new PlayerCrouchState(this,movementFSM, handFSM));
            movementFSM.Add(new PlayerJumpState(this, movementFSM));
            movementFSM.Add(new PlayerFallingState(this, movementFSM));
            movementFSM.Add(new PlayerRollingState(this, movementFSM));
            movementFSM.Add(new PlayerHurtState(this, movementFSM));
            movementFSM.Add(new PlayerBoxingState(this, movementFSM , boxingFSM));
            movementFSM.SetCurrentState((int)MainState.Movement);

        }

        private void Update()
        {
            //will start and run the FSM
            movementFSM.Update();
        }

        private void FixedUpdate()
        {
            movementFSM.FixedUpdate();
        }

        #region Methods
        /// <summary>
        /// Move the character to a certain location based on the speed
        /// and rotation that is specfied.
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="rotationSpeed"></param>
        public void Move(float speed, float rotationSpeed)
        {
            Vector3 targetVelocity = speed * transform.forward * Time.deltaTime;
            targetVelocity.y = GetComponent<Rigidbody>().velocity.y;
            GetComponent<Rigidbody>().velocity = targetVelocity;

            GetComponent<Rigidbody>().angularVelocity = rotationSpeed * Vector3.up * Time.deltaTime;

            if (targetVelocity.magnitude > 0.01f || GetComponent<Rigidbody>().angularVelocity.magnitude > 0.01f)
            {
                SoundManager.Instance.PlayFootSteps(Mathf.Abs(speed));
            }

            anim.SetFloat(horizonalMoveParam, GetComponent<Rigidbody>().angularVelocity.y);
            anim.SetFloat(verticalMoveParam, speed * Time.deltaTime);
        }

        public void SetCrouchCollider(bool shouldCrouch)
        {
            if (shouldCrouch)
            {
                playerCollider.height /= 2;
                playerCollider.center /= 2;
            }
            else
            {
                playerCollider.height *= 2;
                playerCollider.center *= 2;
            }
        }

        public void ResetMoveParams()
        {
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            anim.SetFloat(horizonalMoveParam, 0f);
            anim.SetFloat(verticalMoveParam, 0f);
        }

        public void ApplyImpulse(Vector3 force)
        {
            GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        }

        public void SetAnimationBool(int param, bool value)
        {
            anim.SetBool(param, value);
        }

        public void TriggerAnimation(int param)
        {
            anim.SetTrigger(param);
        }

        public void Shoot()
        {
            TriggerAnimation(shootParam);
            GameObject shootable = Instantiate(data.shootableObject, shootTransform.position, shootTransform.rotation);
            shootable.GetComponent<Rigidbody>().velocity = shootable.transform.forward * data.bulletInitialSpeed;
            SoundManager.Instance.PlaySound(SoundManager.Instance.shoot, true);
        }

        public bool CheckCollisionOverlap(Vector3 point)
        {
            return Physics.OverlapSphere(point, CollisionOverlapRadius, whatIsGround).Length > 0;
        }

        public void Equip(GameObject weapon = null)
        {
            if (weapon != null)
            {
                currentWeapon = Instantiate(weapon, handTransform.position, handTransform.rotation, handTransform);
            }
            else
            {
                ParentCurrentWeapon(handTransform);
            }
        }

        public void DiveBomb()
        {
            TriggerAnimation(hardLanding);
            SoundManager.Instance.PlaySound(SoundManager.Instance.hardLanding);
            shockWave.Play();
        }

        public void SheathWeapon()
        {
            ParentCurrentWeapon(sheathTransform);
        }

        public void Unequip()
        {
            Destroy(currentWeapon);
        }

        public void SetBoxingStance(bool canBoxStance)
        {
            anim.SetBool(boxingStanceAnimation, canBoxStance);

        }

        private bool CheckIfGrounded()
        {
            //shoot a raycast down

            //Debug.DrawRay(playerFoot.transform.targetPosition, Vector3.down * Sensor, Color.yellow);

            return Physics.OverlapSphere(playerFoot.transform.position,Sensor,whatIsGround).Length > 0;
        }

        private bool CheckIfSomethingIsAbove()
        {

            //Debug.DrawRay(playerFoot.transform.targetPosition, Vector3.down * Sensor, Color.yellow);

            return Physics.OverlapSphere(playerHead.transform.position, Sensor, whatIsGround).Length > 0;
        }
        public void ActivateHitBox()
        {
            hitBox.enabled = true;
        }

        public void DeactivateHitBox()
        {
            hitBox.enabled = false;
        }

        private void ParentCurrentWeapon(Transform parent)
        {
            if (currentWeapon.transform.parent == parent)
            {
                return;
            }

            currentWeapon.transform.SetParent(parent);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerFoot.transform.position, Sensor);
            Gizmos.DrawWireSphere(playerHead.transform.position, Sensor);

        }

        //implement IDamageable. Will check if the player have
        //block the damage. Else it would take it.
        public void TakeDamage(object sender, int damage)
        {
            int id = movementFSM.GetCurrentState().ID;
            if (id != (int)(MainState.Hurt ) &&
                handFSM.GetCurrentState().ID != (int)(Substate.Block) &&
                boxingFSM.GetCurrentState().ID != (int)BoxStates.Block
                )
            {
                anim.SetTrigger(hurtAnim);

                movementFSM.SetCurrentState((int)(MainState.Hurt));
            }
        }
        /// <summary>
        /// Set the animation for the boxing state. 
        /// </summary>
        /// <param name="canBox">the value which you want to set it too</param>
        public void SetBoxingState(bool canBox)
        {
            if (canBox)
            {
                handFSM.SetCurrentState((int)Substate.Twohand);
                movementFSM.SetCurrentState((int)MainState.Boxing);
            }
            else
            {
                movementFSM.SetCurrentState((int)MainState.Movement);
            }

        }
        #endregion

        protected void LateUpdate()
        {
            //This is to prevent any weird occurance where the rotation of the player is change due to Rigidbody.
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
    }
}
