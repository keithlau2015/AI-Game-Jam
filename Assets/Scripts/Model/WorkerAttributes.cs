using UnityEngine;

namespace Platformer.Model
{
    [System.Serializable]
    public struct WorkerAttributes
    {
        public int builderSkill;
        public int analystSkill;
        public int courierSkill;
        public int happiness;

        public static WorkerAttributes CreateRandom(WorkerRole primaryRole)
        {
            return new WorkerAttributes
            {
                builderSkill = RollSkill(primaryRole, WorkerRole.Builder),
                analystSkill = RollSkill(primaryRole, WorkerRole.Analyst),
                courierSkill = RollSkill(primaryRole, WorkerRole.Courier),
                happiness = Random.Range(55, 92),
            };
        }

        static int RollSkill(WorkerRole primaryRole, WorkerRole skillRole)
        {
            if (primaryRole == skillRole)
                return Random.Range(70, 96);

            return Random.Range(35, 75);
        }

        public int GetSkill(WorkerRole jobRole)
        {
            return jobRole switch
            {
                WorkerRole.Builder => builderSkill,
                WorkerRole.Analyst => analystSkill,
                WorkerRole.Courier => courierSkill,
                _ => Mathf.Max(builderSkill, Mathf.Max(analystSkill, courierSkill)),
            };
        }

        public float GetEfficiency(WorkerRole jobRole)
        {
            return Mathf.Lerp(0.5f, 1.5f, GetSkill(jobRole) / 100f);
        }
    }
}
