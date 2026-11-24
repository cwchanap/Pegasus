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
        if (node == "") { PassTime(activity); return; }

        void Done(string _) { Conversation.Finished -= Done; PassTime(activity); }
        Conversation.Finished += Done;
        if (!Conversation.Start(node)) Done(node);
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
