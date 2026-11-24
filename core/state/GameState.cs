using System;
using System.Collections.Generic;

namespace Pegasus.Core;

public enum Stat { Knowledge, Guts, Fitness }

public enum TimeSlot { Afternoon, Evening, Night }

/// <summary>
/// Everything that persists between scenes and sessions. Plain data with no Godot
/// types, so it serialises to JSON and unit-tests outside the engine. Scenes read it
/// when they load and rebuild themselves from it. Change it through the methods
/// below so listeners hear about it.
/// </summary>
public class GameState
{
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

    public void AddStat(Stat stat, int amount) => Mutate(() => Stats[stat] = GetStat(stat) + amount);
    public void AddBond(string character, int amount) => Mutate(() => Bonds[character] = GetBond(character) + amount);
    public void AddMoney(int amount) => Mutate(() => Money += amount);
    public void GiveItem(string item) => Mutate(() => Items.Add(item));
    public void TakeItem(string item) => Mutate(() => Items.Remove(item));
    public void SetFlag(string flag, bool on = true) => Mutate(() => { if (on) Flags.Add(flag); else Flags.Remove(flag); });

    /// <summary>Wrap direct edits of the properties (e.g. by the clock) so listeners are told.</summary>
    public void Mutate(Action change)
    {
        change();
        Changed?.Invoke();
    }
}
