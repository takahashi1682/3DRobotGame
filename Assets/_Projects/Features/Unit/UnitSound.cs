using R3;
using UnityEngine;
using VContainer;

namespace _Projects.Features.Unit
{
    public class UnitSound : MonoBehaviour
    {
        [SerializeField] private AudioClip _boostClip;

        [Inject] private AudioSource _audioSource;
        [Inject] private UnitBoost _unitBoost;

        private void Start()
        {
            _unitBoost.IsAction
                .Where(isAction => isAction)
                .Subscribe(_ => _audioSource.PlayOneShot(_boostClip))
                .AddTo(this);
        }
    }
}