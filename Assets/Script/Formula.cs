using UnityEngine;

public static class Formula
{
    public static Character FindClosestEnemyChar(Character me)
    {
        LayerMask charLayer = LayerMask.GetMask("Character");
        Character closestTarget = null;
        float closestDist = 0f;
        // OverlapSphere detects ALL colliders inside the radius (including ones that
        // already overlap the start point — SphereCast misses those).
        Collider[] hits = Physics.OverlapSphere(me.transform.position, me.FindingRange, charLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Character target = hits[i].GetComponent<Character>();
            if (target == null || target.CurHP <= 0 || target == me) continue;
            if (!me.IsMyEnemy(target.tag)) continue;

            float distance = Vector3.Distance(me.transform.position, target.transform.position);
            if (closestTarget == null || distance < closestDist)
            {
                closestTarget = target;
                closestDist = distance;
            }
        }
        return closestTarget;
    }
}