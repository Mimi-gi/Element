using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using Element.Events;
using Element.Managers;
using Cysharp.Threading.Tasks;

public class GameLifetimeScope : LifetimeScope
{
    [Header("Unity References")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private InputActionAsset _inputAsset;

    [Header("Prefab References")]
    [SerializeField] private GameObject _darkPrefab;

    [Header("Stage")]
    [SerializeField] private DarkSource[] _darkSources;

    protected override void Awake()
    {
        // VContainer Build
        base.Awake();

        // GameBootstrap初期化実行
        var bootstrap = Container.Resolve<Element.GameBootstrap>();
        bootstrap.InitializeGameAsync().Forget();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // Events registration (Singleton)
        builder.Register<GameStateEvents>(Lifetime.Singleton)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.Register<PossessionEvents>(Lifetime.Singleton)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.Register<FocusEvents>(Lifetime.Singleton)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.Register<CameraEvents>(Lifetime.Singleton)
            .AsImplementedInterfaces()
            .AsSelf();

        builder.Register<StageEvents>(Lifetime.Singleton)
            .AsImplementedInterfaces()
            .AsSelf();

        // POCO Managers (Singleton)
        builder.Register<StageManager>(Lifetime.Singleton);
        builder.Register<GameStateManager>(Lifetime.Singleton);
        builder.Register<TimeManager>(Lifetime.Singleton);

        // CameraController with Camera injection
        builder.Register<CameraController>(container =>
        {
            return new CameraController(
                _mainCamera,
                container.Resolve<CameraEvents>(),
                container.Resolve<IPossessionEvents>(),
                container.Resolve<IStageEvents>()
            );
        }, Lifetime.Singleton);

        // InputProcessor with InputActionAsset injection
        builder.Register<Element.Managers.InputProcessor>(container =>
        {
            return new Element.Managers.InputProcessor(
                _inputAsset,
                container.Resolve<FocusEvents>(),
                container.Resolve<IGameStateEvents>()
            );
        }, Lifetime.Singleton);

        // PossessionManager with DarkPrefab injection
        builder.Register<PossessionManager>(container =>
        {
            return new PossessionManager(
                container.Resolve<PossessionEvents>(),
                container.Resolve<IStageEvents>(),
                container.Resolve<IGameStateEvents>(),
                container.Resolve<Element.Managers.InputProcessor>(),
                container,
                _darkPrefab
            );
        }, Lifetime.Singleton);

        // GameBootstrap with all dependencies
        builder.Register<Element.GameBootstrap>(container =>
        {
            return new Element.GameBootstrap(
                container.Resolve<StageManager>(),
                container.Resolve<PossessionManager>(),
                container.Resolve<CameraController>(),
                container.Resolve<GameStateManager>(),
                container,
                _darkSources,
                _darkPrefab
            );
        }, Lifetime.Singleton);
    }
}
