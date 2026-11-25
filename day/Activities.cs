using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.UI;

namespace Pegasus.Day;

/// <summary>Doing an activity: play its Yarn scene if it has one, then pass the time.</summary>
public static class Activities
{
    public static void Do(ActivityData activity)
    {
        var node = activity.SceneFor(Game.State);
        if (node == "") PassTime(activity);
        // No scene, no slot spent: a missed milestone scene must stay replayable.
        else if (!Conversation.Start(node, () => PassTime(activity))) Toast.Show("Not now");
    }

    /// <summary>Fade out with the outcome line, spend the slot and apply the gains, then toast them.</summary>
    private static void PassTime(ActivityData activity) => SceneRouter.Instance.Interlude(activity.Outcome, () =>
    {
        var s = Game.State;
        var gained = activity.Describe(s);
        Clock.SpendSlot(s);
        activity.Gains.Apply(s);
        Toast.Show(gained);
    });
}
