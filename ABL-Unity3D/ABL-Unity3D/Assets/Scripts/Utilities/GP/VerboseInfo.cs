using JetBrains.Annotations;
using Sirenix.OdinInspector;

namespace Utilities.GP
{
    
    [HideReferenceObjectPicker]
    public class VerboseInfo
    {
        private bool _verbose;

        [UsedImplicitly]
        public int numberOfTimesNoLegalCrossoverPoints = 0;
        [UsedImplicitly]
        public int numberOfTimesCrossoverWasTooDeep = 0;
        
        [UsedImplicitly]
        public int numberOfTimesCrossoverSkipped = 0;
        [UsedImplicitly]
        public int numberOfTimesMutationSkipped = 0;

        [UsedImplicitly]
        public int numberOfTimesMutationCreatedEquivalentNode = 0;
        [UsedImplicitly]
        public int numberOfTimesCrossoverSwappedEquivalentNode = 0;

        public static implicit operator bool(VerboseInfo v) => v._verbose;
        public static implicit operator VerboseInfo(bool v) => new VerboseInfo { _verbose = v };

        public void ResetCountInfo()
        {
            this.numberOfTimesCrossoverWasTooDeep = 0;
            this.numberOfTimesCrossoverSkipped = 0;
            this.numberOfTimesMutationCreatedEquivalentNode = 0;
            this.numberOfTimesMutationSkipped = 0;
            this.numberOfTimesCrossoverSwappedEquivalentNode = 0;
            this.numberOfTimesNoLegalCrossoverPoints = 0;
        }
    }
}