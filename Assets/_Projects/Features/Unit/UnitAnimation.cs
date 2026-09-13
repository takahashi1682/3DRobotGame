using MyUtils.VContainerExtensions;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit
{
    public class UnitAnimation : MonoBehaviour, IUnitScopeMember, IScopeResolvable, IScopeStartable
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Light _boostLight;

        [Header("Settings")]
        [SerializeField] private float _dampTime = 0.2f;

        [Header("Light Settings")]
        [SerializeField] private float _tweenSpeed = 100f;
        [SerializeField] private float _defaultLightIntensity = 10f;
        [SerializeField] private float _boostLightIntensity = 200f;

        private static readonly int _boostHash = Animator.StringToHash("Boost");
        private static readonly int _groundHash = Animator.StringToHash("Ground");
        private static readonly int _moveXHash = Animator.StringToHash("MoveX");
        private static readonly int _moveZHash = Animator.StringToHash("MoveZ");
        private static readonly int _jumpOnHash = Animator.StringToHash("JumpOn");
        private static readonly int _jumpOffHash = Animator.StringToHash("JumpOff");

        private UnitMove _unitMove;
        private GroundDetection _groundDetection;
        private IBoostActionObservable _boost;

        public void OnResolve(IObjectResolver resolver)
        {
            _unitMove = resolver.Resolve<UnitMove>();
            _groundDetection = resolver.Resolve<GroundDetection>();
            _boost = resolver.Resolve<IBoostActionObservable>();
        }

        public void OnStart()
        {
            _groundDetection.IsHit.Subscribe(isGround => _animator.SetTrigger(isGround ? _jumpOffHash : _jumpOnHash))
                .AddTo(this);

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    var deltaTime = Time.deltaTime;
                    var isBoost = _boost.IsAction.CurrentValue;

                    UpdateBoostLight(isBoost, deltaTime);
                    UpdateAnimatorParameters(isBoost, _unitMove.MoveDirection, deltaTime);
                })
                .AddTo(this);
        }

        private void UpdateBoostLight(bool isBoost, float deltaTime)
        {
            var targetIntensity = isBoost ? _boostLightIntensity : _defaultLightIntensity;
            _boostLight.intensity = Mathf.Lerp(_boostLight.intensity, targetIntensity, deltaTime * _tweenSpeed);
        }

        private void UpdateAnimatorParameters(bool isBoost, Vector3 moveDirection, float deltaTime)
        {
            _animator.SetBool(_boostHash, isBoost);
            _animator.SetBool(_groundHash, _groundDetection.IsHit.CurrentValue);
            _animator.SetFloat(_moveXHash, moveDirection.x, _dampTime, deltaTime);
            _animator.SetFloat(_moveZHash, moveDirection.z, _dampTime, deltaTime);
        }
    }
}