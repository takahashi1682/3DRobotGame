using MyUtils.Parameter.Basic;
using MyUtils.VContainerExtensions;
using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit
{
    public class UnitDie : MonoBehaviour, IUnitScopeMember, IScopeLaunchable
    {
        [SerializeField] private GameObject _dieEffectPrefab;
        [SerializeField] private float _dieEffectScale = 5f;

        [Inject] private Health _health;

        public void OnLaunch()
        {
            _health.IsEmpty.Where(isEmpty => isEmpty).Subscribe(_ =>
            {
                var effect = Instantiate(_dieEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * _dieEffectScale;
                gameObject.SetActive(false);
            }).AddTo(this);
        }
    }
}