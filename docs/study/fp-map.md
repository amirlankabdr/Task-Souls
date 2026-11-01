# Functional Programming Map

| FP concept | File and member | Use | Benefit |
|---|---|---|---|
| Pure function | `Domain.cs`, `Progression.RewardFor` | Maps difficulty to XP | Deterministic and testable |
| Immutability | records and `ApplyReward` | Returns a new state using `with` | Prevents hidden mutation |
| Higher-order function | `Achievement.Rule` | Stores `Func<PlayerState, bool>` | Rules are composable |
| Function composition | `Rule.And/Or/Not` | Combines predicates | Builds complex rules simply |
| Pattern matching | `RewardFor` | Switch expression over difficulty | Readable business rule |
| LINQ/lambdas | `NewlyUnlocked` | Filters rules | Declarative transformation |
| Result type | `Result<T>` | Returns expected failures | Avoids exception control flow |
| Functional core / imperative shell | Core vs Infrastructure | Core computes; service coordinates | Isolates side effects |
