namespace ABLUnitySimulation
{
    public enum Team
    {
        Undefined = 0,
        None = 0,
        Red = 1,
        Blue = 2,
        Civilian = 4
    }

    public enum ActionStatus
    {
        Undefined,
        CompletedSuccessfully, // the action completed and was successful (not valid for all actions)
        Impossible, // this agent cannot complete the action
        InProgress // this agent is trying to complete the action
    }

    public enum SimAgentType
    {
        TypeA,
        TypeB,
        TypeC,
        TypeD
    }

}