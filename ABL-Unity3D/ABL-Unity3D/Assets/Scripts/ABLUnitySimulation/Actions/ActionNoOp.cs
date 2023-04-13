namespace ABLUnitySimulation.Actions
{
    public class ActionNoOp : SimActionPrimitive
    {
        public override StatusReport GetStatus(SimWorldState state, bool useExpensiveExplanation)
        {
            return new StatusReport(ActionStatus.CompletedSuccessfully, "NoOp always completes successfully.", this);
        }

        public override void DrawIntentDestructive(SimWorldState throwawayState, IIntentDrawer drawer)
        {
        }

        public override void Execute(SimWorldState state)
        {
        }

        public override string GetUsefulInspectorInformation(SimWorldState simWorldState)
        {
            return "";
        }
    }
}