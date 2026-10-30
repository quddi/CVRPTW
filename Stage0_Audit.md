# Етап 0. Аудит CVRPTW перед інтеграцією дорожніх відстаней

## 1. Внесені зміни

| Файл | Зміна |
|---|---|
| `CVRPTW/Computing/Optimizers/MainResult/PointTransposeMainResultOptimizer.cs` | `CanTranspose` порівнював ємність авто з `CarResult.Estimation`, а це довжина маршруту, а не завантаження. Тепер завантаження рахується як сума `Demand` точок цільового шляху, умова `завантаження + Demand <= Capacity` (така сама нестрога, як `CanVisit` у `ByPairsMainComputer`). |
| `CVRPTW/Data/Distances.cs` | Видалено `GetNearestPointId`: ніде не використовувався, повертав сам рядок (відстань 0) і не знав про недосяжні пари. |

Збірку не перевірено: у середовищі сесії немає `dotnet`. Потрібно виконати `dotnet build` локально.

## 2. Недосяжні переходи: де що ламається

Зараз у матриці немає поняття «немає шляху»: пари або є, або їх немає (тоді виняток).

| Місце | Поведінка зараз | Що потрібно |
|---|---|---|
| `Distances.GetDistance` | `KeyNotFoundException` для відсутньої пари | `TryGetDistance`, статус пари (`Ok`/`Unreachable`) |
| `ByDistanceMainResultEstimator` | одразу кидає виняток | штраф за недосяжне ребро + лічильник таких ребер |
| `ByPairsMainComputer` / `DistanceMainComputer` | `MinBy` за відстанню; недосяжну пару вибере, якщо інших немає | пропускати недосяжні кандидати; якщо їх немає, завершувати маршрут |
| `TabuSearchMainResultOptimizer.SelectBestNeighbor` | початкове `double.MaxValue`; якщо всі оцінки `∞` або `NaN`, `bestNeighbor == null` і далі `NullReferenceException` | скінченний штраф (див. п. 4) |
| `DeterministicAnnealingOptimizer` | при `∞` ймовірність прийняття 0, при `NaN` порівняння хибні | скінченний штраф |
| `SimpleTimeEstimator` | бере `Times.GetTime` з окремої матриці; `mainData.Times!` | узгодити джерело часу окремо (ТЗ, «Межі етапу») |

## 3. Асиметрія

Безпечно:
- `DistancesDataParser` записує рядки файлу як є, нічого не дзеркалить.
- `Opt2`, `Opt3`, `OrOpt`, `Swap`, `PointTranspose`, `AlternativePoints` після кожного ходу викликають повну переоцінку шляху, а не дельта-формули.
- Команди Opt3 (`Do`/`Undo`) лише перестановлюють точки. Коректність `Undo` для `SwapSegments` перевірена за індексами.
- `DistanceMainComputer.EstimatePair` бере відстань від поточної точки до кандидата, тобто напрям збігається з оцінювачем.

Лишається ризик продуктивності, а не коректності: `Opt3` робить повну переоцінку для кожного кандидата (приблизно O(n^4)).

## 4. Рішення щодо недосяжних ребер

1. У `Distances` зберігати статус пари. Відсутнє значення не підміняти нулем, евклідовою відстанню чи `∞`.
2. Оцінювач додає за кожне недосяжне ребро скінченний налаштований штраф `UnreachablePenalty`. Значення за замовчуванням: більше за будь-яку реальну довжину маршруту, наприклад `(максимальна скінченна відстань матриці) × (кількість точок + 1)`. Так оптимізатори відрізняють «одне погане ребро» від «десяти» і можуть їх прибирати.
3. `MainResult` отримує `UnreachableEdgesCount`; ненульове значення означає недопустимий розв'язок і має показуватися в UI.
4. Жадібна побудова (`ByPairsMainComputer`) не додає недосяжні точки в маршрут навмисно.

## 5. Пам'ять і швидкість матриці

`Distances.Matrix` має тип `Dictionary<int, Dictionary<int, Dictionary<int, double>>>`: три пошуки на ребро й великі накладні витрати пам'яті. Для дорожніх матриць тисяч точок це стане вузьким місцем. Рекомендація: щільне сховище (`double[]` або `float[]` розміром N×N) за `Point.Index` з тим самим публічним API. `Point.Index` призначається послідовно в `MainParser.ParsePoints` (0, 1, 2, ...), тож підходить як щільний індекс.

## 6. Знахідки поза межами задачі (код не змінювався)

1. **`ByPairsMainComputer.GetCarResult`**: змінна `currentPointId` задається лише як початкова точка й ніколи не оновлюється. Тому кожна наступна точка обирається як найближча до депо, а не до попередньої. Схоже на помилку (жадібний «найближчий сусід» так не працює). Виправлення змінить початкові розв'язки й результати експериментів, тому потрібне ваше рішення.
2. **Місткість у `TabuSearchMainResultOptimizer` (`TryTransferMove`) і `DeterministicAnnealingOptimizer` (`ApplyRelocate`, `ApplyInterRouteSwap`)** не перевіряється: після таких ходів завантаження авто може перевищити ємність, а штрафу за це в оцінювачі немає.
3. **`DeterministicAnnealingOptimizer.Optimize`** на кожну спробу перераховує вартість поточного розв'язку (`_estimator.Estimate(currentSolution)`), хоча вона не змінилась.
4. **`ComplexMainResultEstimator`**: `CarResult.Estimation` містить лише відстань, штрафи за час потрапляють тільки в `MainResult.Estimation`.
5. **`Benchmarks/Cases/DistanceMatrixBenchmark.cs`** має `PointsCount = 40000`, а звіт у `BenchmarkDotNet.Artifacts` отримано для 100, 500 і 1000. `DistanceMatrixComputing` рахує евклідову матрицю, а не дорожній граф.
6. **`CVRPTW/Program.cs`** містить жорстко прописані шляхи `C:\Users\Admin\...`.
7. **Два UI розійшлися.** `VisualizationApplication` (WPF) останній раз змінювався 2026-09-14, `VisualizationApplication.Avalonia` 2026-09-19 (асинхронне завантаження, потрібен `Window`). Проєкти `Application` і `Empty` не входять до `CVRPTW.sln`. Основним для інтеграції логічно вважати Avalonia (macOS); WPF оновлювати окремо, якщо він ще потрібен.

## 7. Що це змінює в плані задачі 1

- Етап 6 починається зі щільного `Distances` зі статусами та `TryGetDistance`, а вже потім маппер `RoadDistanceMatrix` → `MainData.Distances`.
- Штраф за недосяжність і лічильник ребер реалізуються в `ByDistanceMainResultEstimator` та `MainResult`.
- Вибір джерела відстаней додається в `VisualizationApplication.Avalonia/MainWindowReactionHandler.cs` перед `Compute()`.
- Джерело `Times` потрібно узгодити окремо, інакше штрафи за час лишаться на старих даних.
