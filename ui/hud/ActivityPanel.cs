using Godot;
using Pegasus.Core;
using Pegasus.Day;

namespace Pegasus.UI;

/// <summary>
/// The confirm panel from the HUD mockup: what an activity spends and gives, then
/// E to do it or Esc to back out. Activities.Do runs it.
/// </summary>
public partial class ActivityPanel : Control
{
    private ActivityData? _activity;

    public override void _Ready()
    {
        Visible = false;
        GetNode<Button>("%Do").Pressed += Confirm;
        GetNode<Button>("%Cancel").Pressed += Close;
    }

    public void Offer(ActivityData activity)
    {
        if (Visible || !InputLock.IsFree) return;
        var s = Game.State;
        if (!Clock.CanSpendSlot(s))
        {
            Toast.Show("It's late. Nothing left today but sleep.");
            return;
        }
        _activity = activity;
        GetNode<Label>("%Title").Text = activity.Title;
        GetNode<Label>("%Spends").Text = $"{s.Slot} slot";
        GetNode<Label>("%Gain").Text = activity.Describe(s);
        GetNode<Label>("%DoText").Text = activity.Verb;
        Visible = true;
        InputLock.Acquire(nameof(ActivityPanel));
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible) return;
        if (e.IsActionPressed("interact") || e.IsActionPressed("ui_accept")) { GetViewport().SetInputAsHandled(); Confirm(); }
        else if (e.IsActionPressed("pause") || e.IsActionPressed("ui_cancel")) { GetViewport().SetInputAsHandled(); Close(); }
    }

    private void Confirm()
    {
        if (_activity is not { } activity) return;
        Close();
        // Start once the panel's lock is gone, so a hangout's conversation can take input.
        Callable.From(() => Activities.Do(activity)).CallDeferred();
    }

    private void Close()
    {
        Visible = false;
        _activity = null;
        // Release after this frame so the closing key does not also reach the world.
        Callable.From(() => InputLock.Release(nameof(ActivityPanel))).CallDeferred();
    }
}
