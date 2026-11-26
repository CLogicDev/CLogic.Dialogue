using System;
using Random = UnityEngine.Random;

namespace CLogic.Dialogue.Provisioner
{
    [Serializable]
    public class RandomTextData : ProvisionerData
    {
        public string[] options;
    }

    public class RandomTextProvisioner : DialogueProvisioner<RandomTextData, string>
    {
        public override bool SupportsCaching => false;

        protected override string CreateProvisionedData(RandomTextData nodeData) => nodeData.options[Random.Range(0, nodeData.options.Length)];
    }
}
