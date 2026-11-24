using System;
using System.Collections.Generic;

namespace Pegasus.Core;

public enum Stat { Knowledge, Guts, Fitness }

/// <summary>Late = both day slots spent, only bed is left. Night = the snow world.</summary>
public enum TimeSlot { Afternoon, Evening, Late, Night }

/// <summary>
/// Everything that persists between scenes and sessions. Plain data with no Godot
/// types, so it serialises to JSON and unit-tests outside the engine. Scenes read it
/// when they load and rebuild themselves from it. Change it through the methods
/// below so listeners hear about it.
/// </summary>
public class GameState
{
    /// <summary>Stats and bonds both run 0 to 3 in the slice.</summary>
    public const int MaxStat = 3;
    public const int MaxBond = 3;
    /// <summary>Money in hand on a new game.</summary>
    public const int StartingMoney = 3000;

    public int Day { get; set; } = 1;
    public TimeSlot Slot { get; set; } = TimeSlot.Afternoon;
    public int Money { get; set; }
    public Dictionary<Stat, int> Stats { get; set; } = new() { [Stat.Knowledge] = 0, [Stat.Guts] = 0, [Stat.Fitness] = 0 };
    public Dictionary<string, int> Bonds { get; set; } = new();
    public HashSet<string> Items { get; set; } = new();
    public HashSet<string> Flags { get; set; } = new();

    /// <summary>Raised after any change made through the methods below.</summary>
    public event Action? Changed;

    public int GetStat(Stat stat) => Stats.GetValueOrDefault(stat);
    public int GetBond(string character) => Bonds.GetValueOrDefault(character);
    public bool HasItem(string item) => Items.Contains(item);
    public bool HasFlag(string flag) => Flags.Contains(flag);

    public void AddStat(Stat stat, int amount) => Mutate(() => Stats[stat] = Math.Clamp(GetStat(stat) + amount, 0, MaxStat));
    public void AddBond(string character, int amount) => Mutate(() => Bonds[character] = Math.Clamp(GetBond(character) + amount, 0, MaxBond));
    public void AddMoney(int amount) => Mutate(() => Money += amount);
    public void GiveItem(string item) => Mutate(() => Items.Add(item));

    /// <summary>Pays for an item he doesn't have yet. False if owned or too expensive.</summary>
    public bool TryBuy(string item, int price)
    {
        if (HasItem(item) || Money < price) return false;
        Mutate(() => { Money -= price; Items.Add(item); });
        return true;
    }
    public void TakeItem(string item) => Mutate(() => Items.Remove(item));
    public void SetFlag(string flag, bool on = true) => Mutate(() => { if (on) Flags.Add(flag); else Flags.Remove(flag); });

    /// <summary>Wrap direct edits of the properties (e.g. by the clock) so listeners are told.</summary>
    public void Mutate(Action change)
    {
        change();
        Changed?.Invoke();
    }
}
