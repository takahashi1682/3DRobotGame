using MyUtils.Parameter.Basic;
using R3;
using UnityEngine;
using VContainer;

namespace Features.Unit
{
    public class UnitDie : MonoBehaviour, IUnitScopeInitializable
    {
        [SerializeField] private GameObject _dieEffectPrefab;
        [SerializeField] private float _dieEffectScale = 5f;

        public void OnRegister(IContainerBuilder builder)
        {
        }

        public void OnResolve(IObjectResolver resolver)
        {
            var health = resolver.Resolve<Health>();
            health.IsEmpty.Where(isEmpty => isEmpty).Subscribe(_ =>
            {
                var effect = Instantiate(_dieEffectPrefab, transform.position, Quaternion.identity);
                effect.transform.localScale = Vector3.one * _dieEffectScale;
                gameObject.SetActive(false);
            }).AddTo(this);
        }
    }
}