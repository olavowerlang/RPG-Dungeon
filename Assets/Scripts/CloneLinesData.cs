using UnityEngine;

[CreateAssetMenu(fileName = "CloneLinesData", menuName = "Boss/Clone Lines Data")]
public class CloneLinesData : ScriptableObject
{
    [System.Serializable]
    public class FightLine
    {
        [TextArea] public string text;
        [TextArea] public string followUp;        // optional follow-up line
        public float            followUpDelay = 1f;
    }

    [Header("Lines (fired in shuffled order, then reshuffled)")]
    public FightLine[] lines = new FightLine[]
    {
        new FightLine { text = "Stop resisting, man! Please!" },
        new FightLine { text = "Let me kill you and I'll give you some upgrades!" },
        new FightLine { text = "This doesn't have to end like this!", followUp = "Oh wait it does actually!", followUpDelay = 1f },
        new FightLine { text = "Let's wrap this up, I'm getting hungry!" },
        new FightLine { text = "Does that tent of yours have a bed?" },
        new FightLine { text = "You're not that bad with that sword, I'm proud!" },
        new FightLine { text = "Hey look, there's a monster behind you! Look there, trust me!" },
        new FightLine { text = "I'm a wizard, why am I even using a sword?" },
        new FightLine { text = "Whew! Sure feels good walking on two legs again!" },
    };

    [Header("Timing")]
    public float minInterval = 6f;
    public float maxInterval = 13f;
}
