# State and Components

`PlayerStateService` is a scoped state container. It owns the latest immutable `DashboardSnapshot` and raises `OnChange` after refresh or completion. `MainLayout` provides the service through `CascadingValue`, subscribes to updates, and unsubscribes in `Dispose`.

`QuestCard` receives a quest with `[Parameter]` and reports complete/edit/delete actions with `EventCallback`. `ProgressBar`, `SkillCard`, and `AchievementBadge` contain reusable presentation. `DataList<TItem>` is a generic templated component using `RenderFragment<TItem>`.
