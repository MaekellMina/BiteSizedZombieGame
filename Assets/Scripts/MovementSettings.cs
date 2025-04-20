
using UnityEngine;

[System.Serializable]
public struct MovementSettings
{
    public float speed;
    public string animName;

    public override bool Equals(object obj)
    {
        if (!(obj is MovementSettings))
            return false;

        MovementSettings other = (MovementSettings)obj;
        return Mathf.Approximately(speed, other.speed) && animName == other.animName;
    }

    public override int GetHashCode()
    {
        return speed.GetHashCode() ^ animName.GetHashCode();
    }
}
