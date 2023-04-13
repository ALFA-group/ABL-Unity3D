using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ABLUnitySimulation.Actions;
using ABLUnitySimulation.Actions.Helpers;
using ABLUnitySimulation.Exceptions;
using Newtonsoft.Json;
using Sirenix.Serialization;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.Unity;
using Random = System.Random;

#nullable enable

namespace ABLUnitySimulation
{
    /// <summary>
    ///     A world state representation for the ABL-Unity3D simulation. Ideally, this should be fast to copy.
    ///     Since we want to be able to map objects to objects from other states,
    ///     we need to use handles for objects rather than direct references (handles are sort of like pointers).
    /// </summary>
    public class SimWorldState : ICanDeepCopy<SimWorldState>
    {
        /// <summary>
        ///     Delegate for providing a navmesh.
        /// </summary>
        public delegate IEnumerable<Vector2> PathProvider(Vector2 start, Vector2 end, CancellationToken cancel);

        /// <summary>
        ///     The maximum number of <see cref="SimAgent" />s which this world state can contain.
        /// </summary>
        private const int MAXIMUM_NUMBER_OF_SIM_AGENTS = 2000;

        /// <summary>
        ///     A helper property to return an empty list of <see cref="SimAgent" />s.
        /// </summary>
        private static readonly IReadOnlyList<SimAgent> EmptySimAgentList = new List<SimAgent>();

        /// <summary>
        ///     A cache to store <see cref="SimGroup" />s.
        /// </summary>
        private Dictionary<SimGroup, List<SimAgent>> _cacheSimGroups =
            new Dictionary<SimGroup, List<SimAgent>>();

        /// <summary>
        ///     Cache for storing <see cref="SimAgent" />s on the Blue team.
        /// </summary>
        private IReadOnlyList<SimAgent>? _teamBlue;
        

        /// <summary>
        ///     Cache for storing <see cref="SimAgent" />s on the Red team.
        /// </summary>
        private IReadOnlyList<SimAgent>? _teamRed;

        /// <summary>
        ///     The list of actions that the world state contains. These actions are to be executed
        ///     each game update.
        /// </summary>
        [JsonIgnore] // When deserializing, you should add new actions anyways
        // [OdinSerialize]
        public ActionParallel actions = new ActionParallel(Enumerable.Empty<SimAction>());

        /// <summary>
        ///     The area of operations for all actions to be executed within.
        ///     This is for information about the map. It is not used to impose rules on where
        ///     agents are located.
        /// </summary>
        
        public Rect areaOfOperations = new Rect(-27, -18.5f, 60, 34);

        public IReadOnlyList<Circle> circlesBlueKillZones = new List<Circle>();
        
        /// <summary>
        ///     Whether a replan for the planner has been requested. This is only used for convenience purposes for viewing within
        ///     the Unity Editor.
        /// </summary>
        public bool isReplanRequested;

        /// <summary>
        ///     A list of additional methods that can be used by the planner and world state.
        ///     For some reason we could not get this to work by declaring it as its actual type (MethodLibrary),
        ///     so we store it as a nullable object.
        /// </summary>
        public object? methodLibrary; 

        /// <summary>
        ///     Tracking UAVs separately from other agents to avoid having to think about them in general.
        /// </summary>
        /// <summary>
        ///     A value to track what values have been used for <see cref="SimId" />s.
        /// </summary>
        protected int nextSimId = 1;

        /// <summary>
        ///     Instance of <see cref="PathProvider" /> which provides a navmesh.
        /// </summary>
        [JsonIgnore] 
        public PathProvider pathProvider;

        /// <summary>
        ///     Backing field for <see cref="SimWorldState.Random" />.
        /// </summary>
        protected Random? random;

        
        public List<Func<SimWorldState, bool>> replanTests = new List<Func<SimWorldState, bool>>();

        /// <summary>
        ///     The list of <see cref="SimAgent" />s that are currently being simulated in this world state.
        ///     Only supposed to be used in this class or subclasses.
        ///     Can also be seen as a backing field for <see cref="Agents" />.
        /// </summary>
        /// <remarks>
        ///     The first index of this list must always be null!
        ///     This is so that a <see cref="SimAgent" />s <see cref="SimId" /> can be used to index into this list.
        /// </remarks>
        [SerializeReference]
        protected List<SimAgent?> simAgents;

        
        // Ideally, we should refactor SimScoringFunction to be PlannerScoringFunction and give it access to PlannerParameters.
        public Circle? goalWaypoint;

        /// <summary>
        ///     The team friendly ("home") team for the simulation.
        ///     In other words, the opposition to the enemy team.
        /// </summary>
        public Team teamFriendly = Team.Red;

        public SimWorldState(
            IEnumerable<SimAgent?> agents,
            Team teamFriendly)
        {
            this.simAgents = new List<SimAgent?>();
            this.Add(agents);
            this.teamFriendly = teamFriendly;

            this.pathProvider = PathProviderNavMesh;
            this.WorldStateId = GetNextWorldStateId();
        }

        public SimWorldState(
            IEnumerable<SimAgent?> agents,
            Team teamFriendly,
            int? randomSeed) : this(agents, teamFriendly)
        {
            if (null != randomSeed) this.random = new Random((int)randomSeed);
        }

        public SimWorldState(PathProvider pathProvider)
        {
            this.simAgents = new List<SimAgent?>();
            this.WorldStateId = GetNextWorldStateId();
            this.pathProvider = pathProvider; // ?? pathProviderNoObstacles;
        }

        /// <summary>
        ///     The ID for the simulation.
        /// </summary>
        public long WorldStateId { get; protected set; }

        /// <summary>
        ///     The amount of simulated time in seconds that have passed since the simulation began.
        /// </summary>
        public int SecondsElapsed { get; set; }

        /// <summary>
        ///     The amount of time in simulated seconds that have passed since the last world state update.
        /// </summary>
        public int SecondsSinceLastUpdate { get; protected set; }

        /// <summary>
        ///     The enemy team for the simulation.
        /// </summary>
        public Team TeamEnemy => this.GetEnemyTeamOf(this.teamFriendly);


        /// <summary>
        ///     The number of simulated hours elapsed since the simulation began.
        /// </summary>
        [JsonIgnore]
        public float HoursElapsed => this.SecondsElapsed / (60.0f * 60.0f);

        /// <summary>
        ///     The number of simulated hours that have elapsed since the last world state update.
        /// </summary>
        [JsonIgnore]
        public float HoursSinceLastUpdate => this.SecondsSinceLastUpdate / (60.0f * 60.0f);

        /// <summary>
        ///     A random number generator.
        /// </summary>
        public Random Random
        {
            get => this.random ??= new Random();
            set => this.random = value;
        }

        /// <summary>
        ///     A property to get all <see cref="SimAgent" />s within this world state.
        ///     Essentially a wrapper for <see cref="simAgents" />
        /// </summary>
        public IEnumerable<SimAgent> 
            Agents => this.GetAll<SimAgent>();
        

        public SimWorldState DeepCopy()
        {
            var clone = (SimWorldState)this.MemberwiseClone();
            clone.simAgents = new List<SimAgent?>(this.simAgents.Count);
            foreach (var agent in this.simAgents) clone.simAgents.Add(agent?.DeepCopy());

            clone.actions = (ActionParallel)clone.actions.DeepCopy();
            clone.ClearAllCaches();

            clone.WorldStateId = GetNextWorldStateId();
            return clone;
        }

        /// <summary>
        ///     Provides a simple path from <paramref name="start" /> to <paramref name="end" /> with no obstacles.
        /// </summary>
        /// <param name="start">The start of the path with no obstacles.</param>
        /// <param name="end">The end of the path with no obstacles.</param>
        /// <param name="cancel">Cancellation token for cancelling this.</param>
        /// <returns>The simple path with no obstacles.</returns>
        public static IEnumerable<Vector2> PathProviderNoObstacles(Vector2 start, Vector2 end, CancellationToken cancel)
        {
            yield return start;
            yield return end;
        }

        /// <summary>
        ///     Get the pathfinding from the navmesh given by <see cref="SimWorldState.pathProvider" /> from point
        ///     <paramref name="start" /> to point <paramref name="end" />.
        /// </summary>
        /// <param name="start">The starting point of the path to return.</param>
        /// <param name="end">The end point of the path to return.</param>
        /// <param name="cancel">A cancellation token to cancel the pathfinding.</param>
        /// <returns>
        ///     A path from point <paramref name="start" /> to point <paramref name="end" /> in the form of an enumerable of
        ///     points.
        /// </returns>
        public static IEnumerable<Vector2> PathProviderNavMesh(Vector2 start, Vector2 end, CancellationToken cancel)
        {
            return PathfindingWrapper.GetPathBlocking(start, end, cancel);
        }

        /// <summary>
        ///     Get an instance of type <see cref="ISimObject" /> of type <typeparamref name="T" /> contained within this world
        ///     state.
        /// </summary>
        /// <param name="handle">The <see cref="Handle{T}" /> for the <see cref="ISimObject" />.</param>
        /// <typeparam name="T">The concrete type of the <see cref="ISimObject" />.</typeparam>
        /// <returns>The instance of type <typeparamref name="T" /> contained within this world state.</returns>
        /// <exception cref="HandleException">Throws when the handle is invalid.</exception>
        public T Get<T>(Handle<T> handle) where T : class, ISimObject
        {
            if (handle.simId == 0) Debug.LogError("Getting handle simId of 0");

            ISimObject simObject = this.simAgents.MaybeGet(handle.simId.id) ??
                                   throw HandleException.CreateNotFound(handle, this);
            Debug.Assert(simObject is T);
            return (T)simObject;
        }

        /// <summary>
        ///     Wrapper method for getting an enumerable of instances of type <typeparamref name="T" /> from an enumerable of type
        ///     <see cref="Handle{T}" />.
        ///     Essentially maps the method <see cref="Get{T}(Handle{T})" /> onto the given list of handles.
        /// </summary>
        /// <param name="handles">The enumerable of handles to get concrete instances from.</param>
        /// <typeparam name="T">A concrete type which implements <see cref="ISimObject" /></typeparam>
        /// <returns>The enumerable of instances of type <typeparamref name="T" /> contained within this world state.</returns>
        public IEnumerable<T> Get<T>(IEnumerable<Handle<T>> handles) where T : class, ISimObject
        {
            return handles.Select(this.Get);
        }

        /// <summary>
        ///     Get a <see cref="SimAgent" /> whose <see cref="SimAgent.Name" /> is <paramref name="agentName" />.
        ///     If there are multiple agents with the same name (which shouldn't be the case),
        ///     then it returns the first agent found. Otherwise, it returns null.
        /// </summary>
        /// <param name="agentName">The name of the agent to retrieve.</param>
        /// <returns>The <see cref="SimAgent" /> whose <see cref="SimAgent.Name" /> is <paramref name="agentName" />. Otherwise, null.</returns>
        public SimAgent? GetByNameCanFail(string agentName)
        {
            return this.simAgents.FirstOrDefault(agent => agent?.Name == agentName);
        }

        /// <summary>
        ///     Wrapper for <see cref="Get{T}(Handle{T})" /> which returns null if the concrete instance of
        ///     <paramref name="handle" /> is not found/is invalid.
        /// </summary>
        /// <param name="handle">The handle of the simulation object instance to retrieve.</param>
        /// <typeparam name="T">The type of the simulation object to retrieve.</typeparam>
        /// <returns>The simulation object if found, otherwise null.</returns>
        public T? GetCanFail<T>(Handle<T> handle) where T : class, ISimObject
        {
            ISimObject? gotten = this.simAgents.MaybeGet(handle.simId.id);
            return gotten as T;
        }

        /// <summary>
        ///     Get the <see cref="SimAgent" />s within a given <see cref="SimGroup" />.
        /// </summary>
        /// <param name="group">The <see cref="SimGroup" /> to retrieve <see cref="SimAgent" />s from.</param>
        /// <returns>All <see cref="SimAgent" />s within <paramref name="group" /></returns>
        public IReadOnlyList<SimAgent> GetGroupMembers(SimGroup group)
        {
            if (this._cacheSimGroups.TryGetValue(group, out var groupMembers)) return groupMembers;

            if (group.Count == 1)
            {
                // Special case.  We get a lot of solo groups, just return the cached list for the solo agent.
                var soloAgent = this.GetCanFail(group.First());
                return null != soloAgent ? new List<SimAgent>(){soloAgent} : EmptySimAgentList; 
            }

            var newGroupMembers = new List<SimAgent>(group.Count);
            foreach (var handle in group)
            {
                var maybeT = this.GetCanFail(handle);
                if (null != maybeT) newGroupMembers.Add(maybeT);
            }

            this._cacheSimGroups[group] = newGroupMembers;
            return newGroupMembers;
        }


        /// <summary>
        ///     Add a enumerable of <see cref="SimAgent"/>?s to this world state.
        /// </summary>
        /// <param name="agentsToAdd"></param>
        public void Add(IEnumerable<SimAgent?> agentsToAdd)
        {
            foreach (var agent in agentsToAdd.WhereNotNull()) this.Add(agent);
        }

        /// <summary>
        ///     If a <see cref="SimAgent" /> is already contained in this world state, replace it with <paramref name="agent" />.
        ///     Otherwise, add it.
        /// </summary>
        /// <param name="agent">The <see cref="SimAgent" /> to add to this world state.</param>
        /// <returns>A <see cref="Handle{SimAgent}" /> to <paramref name="agent" /> in this world state.</returns>
        /// <exception cref="Exception">
        ///     Throws if we cannot add anymore <see cref="SimAgent" />s due to the maximum number of
        ///     <see cref="SimAgent" />s being reached (a.k.a <see cref="MAXIMUM_NUMBER_OF_SIM_AGENTS" />).
        /// </exception>
        public Handle<SimAgent> ReplaceOrAdd(SimAgent agent)
        {
            if (!agent.SimId.IsValid) agent.SetSimId(this);

            var simId = agent.SimId;
            this.nextSimId = Math.Max(simId.id + 1, this.nextSimId);
            int desiredIndex = agent.SimId.id;

            if (desiredIndex >= this.simAgents.Count)
            {
                if (desiredIndex >= MAXIMUM_NUMBER_OF_SIM_AGENTS)
                    throw new Exception(
                        $"Cannot add SimAgent with id {desiredIndex}.  We are arbitrarily capping at {MAXIMUM_NUMBER_OF_SIM_AGENTS} to detect bugs.");

                this.simAgents.AddRange(Enumerable.Repeat<SimAgent?>(null, desiredIndex - this.simAgents.Count));
                this.simAgents.Add(agent);
            }
            else
            {
                this.simAgents[desiredIndex] = agent;
            }

            Debug.Assert(this.simAgents[desiredIndex] == agent);

            this.ClearAllCaches();
            return agent;
        }


        /// <summary>
        ///     Add a <see cref="SimAgent" /> to this world state.
        /// </summary>
        /// <param name="agent">The <see cref="SimAgent" /> to add to this world state.</param>
        /// <returns>A <see cref="Handle{SimAgent}" /> to <paramref name="agent" />.</returns>
        /// <exception cref="ArgumentException"></exception>
        public Handle<SimAgent> Add(SimAgent agent)
        {
            if (agent.SimId.IsValid)
            {
                // Is this agent already set?
                var alreadyThere = this.simAgents.MaybeGet(agent.SimId.id);

                if (null != alreadyThere)
                {
                    if (!agent.Equals(alreadyThere))
                        throw new ArgumentException(
                            $"SimId conflict: {agent.DebugName()} trying to replace " +
                            $"{alreadyThere.DebugName()} in {this.WorldStateId}", nameof(agent));

                    Debug.LogWarning(
                        $"ISimObject {agent.DebugName()} added to SimWorldState {this.WorldStateId} twice.");
                }
            }

            return this.ReplaceOrAdd(agent);
        }

        /// <summary>
        ///     Get an unused <see cref="SimId" />.
        /// </summary>
        /// <returns>A new unused <see cref="SimId" />.</returns>
        public SimId GetUnusedSimId()
        {
            int unused = this.nextSimId;
            ++this.nextSimId;

            return new SimId(unused);
        }

        /// <summary>
        ///     Get a new <see cref="SimWorldState.WorldStateId" />.
        /// </summary>
        /// <returns>A new <see cref="SimWorldState.WorldStateId" />.</returns>
        protected static long GetNextWorldStateId()
        {
            return DateTime.UtcNow.Ticks;
        }

        /// <summary>
        ///     Get all <see cref="SimAgent" />s of subclass type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">A child class that inherits from <see cref="SimAgent" />.</typeparam>
        /// <returns>An enumerable of all <see cref="SimAgent" />s of subclass type <typeparamref name="T" />.</returns>
        public IEnumerable<T> GetAll<T>() where T : SimAgent
        {
            return this.simAgents.OfType<T>();
        }

        /// <summary>
        ///     Get a random <see cref="SimAgent" /> of subclass type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">A child class that inherits from <see cref="SimAgent" />.</typeparam>
        /// <returns>A random <see cref="SimAgent" /> of subclass type <typeparamref name="T" />.</returns>
        public T GetRandom<T>() where T : SimAgent
        {
            return this.GetAll<T>().GetRandomEntry(this.Random);
        }


        /// <summary>
        ///     Get all <see cref="SimAgent" />s on a given <paramref name="team" />, including UAVs.
        /// </summary>
        /// <param name="team">The team of the agents to retrieve.</param>
        /// <returns>All <see cref="SimAgent" />s on a given <paramref name="team" />, including UAVs.</returns>
        /// <exception cref="ArgumentException">
        ///     Throws exception if the team is not <see cref="Team.Blue" /> or
        ///     <see cref="Team.Red" />
        /// </exception>
        /// .
        public IReadOnlyList<SimAgent> GetTeam(Team team)
        {
            switch (team)
            {
                case Team.Red:
                    return this._teamRed ??= this.simAgents
                        .WhereNotNull()
                        .Where(u => u.team == Team.Red)
                        .ToList();
                case Team.Blue:
                    return this._teamBlue ??= this.simAgents
                        .WhereNotNull()
                        .Where(u => u.team == Team.Blue)
                        .ToList();
                default:
                    throw new ArgumentException("Team is undefined");
            }
        }

        /// <summary>
        ///     Get all <see cref="SimAgent" />s on the enemy team of the <see cref="SimAgent" /> <paramref name="simAgent" /> which
        ///     are not UAVs.
        /// </summary>
        /// <param name="simAgent">The <see cref="SimAgent" /> to get the enemy team of.</param>
        /// <returns>
        ///     All <see cref="SimAgent" />s on the enemy team of the <see cref="SimAgent" /> <paramref name="simAgent" /> which
        ///     are not UAVs.
        /// </returns>
        public IEnumerable<SimAgent> GetTeamEnemies(SimAgent simAgent)
        {
            return this.GetTeam(this.GetEnemyTeamOf(simAgent.team));
        }

        /// <summary>
        ///     Get the enemy team of a given friendly <see cref="Team" />.
        /// </summary>
        /// <param name="team">The friendly team.</param>
        /// <returns>The enemy team.</returns>
        public Team GetEnemyTeamOf(Team team)
        {
            return team == Team.Blue ? Team.Red : Team.Blue;
        }

        /// <summary>
        ///     Get <see cref="Handle{SimAgent}" />s of every <see cref="SimAgent" />s on a given <paramref name="team" />.
        /// </summary>
        /// <param name="team">The team to retrieve agents for.</param>
        /// <returns>An enumerable of all <see cref="Handle{SimAgent}" />s on <paramref name="team" />.</returns>
        public IEnumerable<Handle<SimAgent>> GetTeamHandles(Team team)
        {
            // return this.simAgents.WhereNotNull().Where(u => u.team == team);
            return this.GetTeam(team).Select(u => u.Handle);
        }

        /// <summary>
        ///     Clear all caches for getting agents in a team,
        ///     <see cref="SimGroup" />s, and values cached within each <see cref="SimAgent" />.
        /// </summary>
        private void ClearAllCaches()
        {
            this._teamRed = null;
            this._teamBlue = null;

            this._cacheSimGroups = new Dictionary<SimGroup, List<SimAgent>>();
        }

        /// <summary>
        ///     Update the amount of time passed since the last world state update and total seconds elapsed since the start of the
        ///     simulation,
        ///     run all vision tests, update whether a replan is requested, execute all simulation actions.
        /// </summary>
        /// <param name="seconds">The amount of time passed since the last world state update.</param>
        public void Execute(int seconds)
        {
            this.SecondsSinceLastUpdate = seconds;
            this.SecondsElapsed += seconds;


            this.DoCompleteVisionTest();

            if (this.replanTests.Any(f => f(this))) this.isReplanRequested = true;

            this.actions.Execute(this);
            this.actions.UpdateForExternalSimChange(this);
        }

        /// <summary>
        ///     Execute vision tests for all <see cref="SimAgent" />s.
        /// </summary>
        private void DoCompleteVisionTest()
        {
            var reds = this.GetTeam(Team.Red);
            var blues = this.GetTeam(Team.Blue);

            foreach (var redAgent in reds)
            {
                // Reds know where all reds are!
                redAgent.positionObservedByRed.SetPosition(redAgent.positionActual, this.SecondsElapsed);
                
                float kmRedVisionSqr = redAgent.kmVisualRange * redAgent.kmVisualRange;
                var redWasSeen = false;

                foreach (var b in blues)
                {
                    b.positionObservedByRed.SetPosition(b.positionActual, this.SecondsElapsed);   
                    bool blueWasSeen = b.positionObservedByRed.lastObservationTimestamp >= this.SecondsElapsed;
                    if (blueWasSeen && redWasSeen) continue;
                    
                    float d2ActualSqr = redAgent.DistanceSqr2dActual(b.positionActual);
                    if (!redWasSeen && d2ActualSqr <= b.kmVisualRange * b.kmVisualRange)
                    {
                        // Blue sees Red!
                        redWasSeen = true;
                        redAgent.positionObservedByBlue.SetPosition(redAgent.positionActual, this.SecondsElapsed);
                    }
                    
                    // if (d2ActualSqr <= kmRedVisionSqr)
                    //     // Red see Blue!
                    //     b.positionObservedByRed.SetPosition(b.positionActual, this.SecondsElapsed);
                }
            }

            foreach (var blue in blues)
                // Blue knows where all blues are.
                blue.positionObservedByBlue.SetPosition(blue.positionActual, this.SecondsElapsed);
        }

        /// <summary>
        ///     Executes actions in this world state until given exit condition is met, or until <paramref name="maxSeconds" /> are
        ///     simulated.
        /// </summary>
        /// <param name="exitConditionFunc">The exit condition.</param>
        /// <param name="secondsPerStep">The amount of real seconds to simulate per simulation step.</param>
        /// <param name="maxSeconds">The maximum number of real seconds to simulate</param>
        /// <param name="cancel">A cancellation token to cancel operations within this method.</param>
        /// <returns>True if <see cref="exitConditionFunc" /> is met before <paramref name="maxSeconds" /> elapses.</returns>
        public bool ExecuteUntil(Func<SimWorldState, bool>? exitConditionFunc, int secondsPerStep, int maxSeconds,
            CancellationToken cancel)
        {
            if (secondsPerStep < 1) secondsPerStep = 1;
            exitConditionFunc ??= state => false;

            int endSecond = maxSeconds + this.SecondsElapsed;
            while (endSecond > this.SecondsElapsed)
            {
                if (cancel.IsCancellationRequested) break;
                if (exitConditionFunc(this)) return true;

                int naiveNextTimestamp = this.SecondsElapsed + secondsPerStep;

                // Want to evaluate timestamps in multiples of secondsPerStep, so let's do a smaller jump if it will get us on track
                int nextTimeStamp = naiveNextTimestamp - naiveNextTimestamp % secondsPerStep;
                int secondsToMultipleOfStepSize = nextTimeStamp - this.SecondsElapsed;
                int secondsToSimulate = 0 == secondsToMultipleOfStepSize ? secondsPerStep : secondsToMultipleOfStepSize;

                // Don't execute past the requested end time.
                int secondsRemaining = endSecond - this.SecondsElapsed;

                this.Execute(Math.Min(secondsToSimulate, secondsRemaining));
            }

            return exitConditionFunc(this);
        }

        private bool Remove(SimId simId)
        {
            int index = simId.id;
            if (!this.simAgents.ContainsIndex(index)) return false;
            if (this.simAgents[index] == null) return false;

            this.simAgents[index] = null;

            this.ClearAllCaches();
            return true;
        }

        public bool Remove<T>(Handle<T> handle) where T : class, ISimObject
        {
            return this.Remove(handle.simId);
        }

        /// <summary>
        ///     Check whether a <see cref="SimAgent" /> has any actions left to execute.
        /// </summary>
        /// <param name="agentHandle">The <see cref="Handle{SimAgent}" /> of the <see cref="SimAgent" /> to check.</param>
        /// <returns>Whether a <see cref="SimAgent" /> has any actions left to execute.</returns>
        public bool IsBusy(Handle<SimAgent> agentHandle)
        {
            return this.actions.IsBusy(agentHandle, this, false).IsBusy;
        }

        /// <summary>
        ///     Checks if any <see cref="Handle{SimAgent}" />s in <paramref name="agentHandles" /> are busy.
        /// </summary>
        /// <param name="agentHandles">The enumerable of <see cref="Handle{SimAgent}" />s to check.</param>
        /// <returns>Whether any <see cref="Handle{SimAgent}" />s in <paramref name="agentHandles" /> are busy.</returns>
        public bool AreAnyBusy(IEnumerable<Handle<SimAgent>> agentHandles)
        {
            return agentHandles.Any(this.IsBusy);
        }

        /// <summary>
        ///     Get the value of a member variable of type <typeparamref name="T" /> for a given <see cref="Handle{SimAgent}" />.
        ///     Currently only used in combination
        ///     with the GP code.
        /// </summary>
        /// <param name="simAgent">
        ///     The <see cref="Handle{SimAgent}" /> of which to retrieve the value of
        ///     <paramref name="memberInfo" /> from.
        /// </param>
        /// <param name="memberInfo"></param>
        /// <typeparam name="T">The type of the value of the given member info to retrieve,</typeparam>
        /// <returns>The value of <paramref name="memberInfo" /> for <paramref name="simAgent" />.</returns>
        /// <exception cref="Exception">Throws if <paramref name="simAgent" /> does not have a valid <see cref="SimId" />.</exception>
        public T GetAttributeValueOfType<T>(Handle<SimAgent> simAgent, MemberInfo memberInfo)
        {
            Debug.Assert(memberInfo.GetUnderlyingType() == typeof(T));

            var agent = this.Get(simAgent) ??
                       throw new Exception($"Agent with id {simAgent.simId} does not exist");
            return (T)memberInfo.GetValue(agent);
        }

        /// <summary>
        ///     Get a random <see cref="MemberInfo" /> for a given type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the member info to return.</typeparam>
        /// <returns>A random <see cref="MemberInfo" /> for a given type <typeparamref name="T" /></returns>
        public MemberInfo GetRandomMemberInfo<T>()
        {
            const BindingFlags binding_flags = BindingFlags.Public | BindingFlags.Instance;

            var fields = typeof(SimAgent)
                .GetFields(binding_flags)
                .Where(f => f.FieldType == typeof(T));

            var properties = typeof(SimAgent)
                .GetProperties(binding_flags)
                .Where(p => p.PropertyType == typeof(T));

            return fields.Cast<MemberInfo>().Concat(properties).GetRandomEntry(this.Random);
        }

        public void CullCompletedActions()
        {
            this.actions.CullTopLevelCompletedActions();
        }

        public void RemoveAllActions()
        {
            this.actions = new ActionParallel(Enumerable.Empty<SimAction>());
        }

        /// <summary>
        ///     Remove all <see cref="SimAgent" />s from the world state.
        ///     Also clear all relevant caches.
        /// </summary>
        public void ClearAllAgents()
        {
            this.simAgents.Clear();
            this.ClearAllCaches();
        }
    }
}