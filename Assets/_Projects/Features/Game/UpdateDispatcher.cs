using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace _Projects.Features.Game
{
    /// <summary>
    /// Update処理を挟み込みたい段階。値の並び順がそのまま毎フレームの実行順序になる
    /// (Inputで値を読み、その結果を使ってAI/Actionが意思決定し、Movementで反映し、
    /// CameraでCameraTarget等を更新、CameraApplyでCinemachineBrain.ManualUpdateを呼んで
    /// 実際のカメラTransformを確定させ、その後のAnimation/UI/Audioが追従する)。
    /// </summary>
    public enum UpdatePhase
    {
        Input, // 入力
        AI, // AIの意思決定
        Action, // Actionの決定
        Movement, // RigidbodyやTransformの移動（基本はFixedUpdateで行うが、InputやAIの結果を受けて毎フレーム反映したい場合はここで行う）
        Camera, // カメラの更新
        Animation, // アニメーション
        UI, // UI
        Audio, // オーディオ
    }

    /// <summary>
    /// UnityのUpdate()を直接実装する代わりにこれを実装し、UpdateDispatcherへ登録することで、
    /// スクリプトの実行順序に頼らずPhaseの並び順どおりにOnPhaseUpdateが呼ばれるようにする。
    /// </summary>
    public interface IPhaseUpdatable
    {
        UpdatePhase Phase { get; }
        void OnPhaseUpdate();
    }

    /// <summary>
    /// 登録されたIPhaseUpdatableを、毎フレームUpdatePhaseの並び順どおりに呼び出す集約役。
    /// MonoBehaviourではなくVContainerのITickableとして登録し(GameScopeRoot参照)、
    /// PlayerLoopから直接駆動する。
    /// </summary>
    public class UpdateDispatcher : ITickable
    {
        private static readonly UpdatePhase[] _phases = (UpdatePhase[])Enum.GetValues(typeof(UpdatePhase));

        private readonly Dictionary<UpdatePhase, List<IPhaseUpdatable>> _buckets = new();

        public UpdateDispatcher()
        {
            foreach (var phase in _phases)
            {
                _buckets[phase] = new List<IPhaseUpdatable>();
            }
        }

        public void Register(IPhaseUpdatable target)
        {
            _buckets[target.Phase].Add(target);
        }

        public void Unregister(IPhaseUpdatable target)
        {
            _buckets[target.Phase].Remove(target);
        }

        public void Tick()
        {
            foreach (var phase in _phases)
            {
                var bucket = _buckets[phase];
                for (var i = 0; i < bucket.Count; i++)
                {
                    bucket[i].OnPhaseUpdate();
                }
            }
        }
    }
}