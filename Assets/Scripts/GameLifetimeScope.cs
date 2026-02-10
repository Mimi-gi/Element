using VContainer;
using VContainer.Unity;
using Element.Events;

public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // イベント登録（Singleton）
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

        // マネージャーはシーン内のコンポーネントをInHierarchyで登録
        builder.RegisterComponentInHierarchy<Element.Managers.GameStateManager>();
        builder.RegisterComponentInHierarchy<Element.Managers.InputProcessor>();
        builder.RegisterComponentInHierarchy<Element.Managers.TimeManager>();
        builder.RegisterComponentInHierarchy<Element.Managers.PossessionManager>();
        builder.RegisterComponentInHierarchy<Element.Managers.StageManager>();
        builder.RegisterComponentInHierarchy<Element.Managers.CameraController>();
    }
}
