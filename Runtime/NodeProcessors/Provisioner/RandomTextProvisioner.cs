using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace CLogic.Dialogue.Provisioner
{
    [Serializable]
    public class RandomTextData : ProvisionerData
    {
        [SerializeField] public Dictionary<string, float> options;
    }

    public class RandomTextProvisioner : DialogueProvisioner<RandomTextData, string>
    {
        public override bool SupportsCaching => false;

        protected override string CreateProvisionedData(RandomTextData nodeData) => GetRandomText(nodeData.options);

        private string GetRandomText(Dictionary<string, float> options)
        {
            float totalProbability = 0f;

            foreach (var pair in options)
            {
                if (pair.Value < 0f)
                    continue;

                totalProbability += pair.Value;
            }

            if (totalProbability <= 0f)
                return string.Empty;

            float randomPoint = Random.value * totalProbability;

            foreach (var pair in options)
            {
                randomPoint -= pair.Value;

                if (randomPoint < 0f)
                    return pair.Key;
            }

            return options.Keys.Last();
        }
    }
}
