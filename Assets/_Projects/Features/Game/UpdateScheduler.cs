using System.Collections.Generic;
using MyUtils.VContainerExtensions;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Projects.Features.Game
{
    /// <summary>
    /// 毎フレームの処理の段階。上から順番に実行される
    /// (入力→Action(AIの意思決定含む)→CameraPrepare→CameraApply→Animation→UI)。
    /// CameraPrepareは各UnitがCameraTarget等を書き込む段階、CameraApplyはCinemachineBrainが
    /// それを読んで実際のカメラTransformを確定させる段階。この2つを分けているのは、
    /// 「書き込みが先、確定が後」という順序を暗黙のSubscribe順に頼らず保証するため。
    /// </summary>
    public enum EUpdatePhase
    {
        Input, // 入力
        Default, // デフォルトの更新処理
        CameraPrepare, // カメラの操作
        CameraApply, // カメラの更新
        Animation, // アニメーションの更新
        UI // UIの更新
    }

    /// <summary>
    /// Update()の代わりにこれを使う。実行順が保証されるので、
    /// 「どのスクリプトが先に動くか」を気にしなくてよくなる。
    /// </summary>
    public interface IUpdateObservable
    {
        Observable<R3.Unit> OnUpdate(EUpdatePhase phase);
    }

    public class UpdateScheduler : MonoBehaviour
        , IGameScopeMember
        , IScopeRegisterable
        , IUpdateObservable
    {
        private readonly SortedDictionary<EUpdatePhase, Subject<R3.Unit>> _updateStreams = new();

        public void OnRegister(IContainerBuilder builder)
        {
            builder.RegisterComponent(this).As<IUpdateObservable>();
        }

        private void Awake()
        {
            this.UpdateAsObservable().Subscribe(_ =>
            {
                foreach (var pair in _updateStreams)
                {
                    var phase = pair.Value;
                    phase.OnNext(R3.Unit.Default);
                }
            }).AddTo(this);
        }

        public Observable<R3.Unit> OnUpdate(EUpdatePhase phase)
        {
            if (_updateStreams.TryGetValue(phase, out var stream))
            {
                return stream;
            }

            _updateStreams[phase] = new Subject<R3.Unit>();
            return _updateStreams[phase];
        }
    }
}