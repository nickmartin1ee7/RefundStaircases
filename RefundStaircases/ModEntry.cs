using System.Diagnostics;

using StardewModdingAPI;

using StardewValley;
using StardewValley.Locations;

namespace RefundStaircases
{
    public class ModEntry : Mod
    {
        private Item? _lastKnownStaircase;
        private int _usedStaircases;
        private bool _inMines;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Player.Warped += (o, e) =>
            {
                Debug.WriteLine($"[I] Warped! Old Location: {e.OldLocation.NameOrUniqueName}; New Location: {e.NewLocation.NameOrUniqueName}");

                if (e.OldLocation is MineShaft
                    && e.NewLocation is not MineShaft
                    && _lastKnownStaircase is not null
                    && _usedStaircases > 0)
                {
                    Debug.WriteLine($"[I] Re-adding {_usedStaircases} staircases!");

                    var heldStairs = e.Player.Items.FirstOrDefault(item => item?.Name == "Staircase");
                    if (heldStairs is not null)
                    {
                        heldStairs.Stack += _usedStaircases;
                    }
                    else
                    {
                        _lastKnownStaircase.Stack = _usedStaircases;
                        e.Player.addItemToInventory(_lastKnownStaircase);
                    }

                    _usedStaircases = 0;
                    _inMines = false;
                }
                else if (e.NewLocation is MineShaft)
                {
                    _inMines = true;
                    Debug.WriteLine("[I] In mines!");
                }
            };

            helper.Events.Player.InventoryChanged += (o, e) =>
            {
                if (!_inMines)
                    return;

                var stairCaseStacksChanges = e.QuantityChanged
                    .Where(stack => stack.Item.Name == "Staircase");

                if (stairCaseStacksChanges.Any()
                    || e.Removed.Any(item => item.Name == "Staircase"))
                {
                    if (stairCaseStacksChanges.All(stack => stack.NewSize < stack.OldSize))
                    {
                        var changedStaircases = stairCaseStacksChanges
                            .Select(stack => stack.Item);

                        _lastKnownStaircase = changedStaircases.FirstOrDefault();
                        _usedStaircases += stairCaseStacksChanges.Sum(stack => stack.OldSize) - stairCaseStacksChanges.Sum(stack => stack.NewSize);
                    }

                    var removedStaircase = e.Removed.FirstOrDefault(item => item.Name == "Staircase");

                    if (removedStaircase is { Stack: > 0 })
                    {
                        _lastKnownStaircase = removedStaircase;
                        _usedStaircases++;
                    }

                    Debug.WriteLine($"[I] In mines, observed used staircases: {_usedStaircases}");
                }
                else if (e.Added.Any(item => item.Name == "Staircase"))
                {
                    _lastKnownStaircase = e.Added.First(item => item.Name == "Staircase");

                    if (_usedStaircases > 0)
                        _usedStaircases--;
                }
            };
        }
    }
}