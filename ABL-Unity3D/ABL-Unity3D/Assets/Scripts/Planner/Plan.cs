using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ABLUnitySimulation;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.GeneralCSharp;

#nullable enable

namespace Planner
{
    /// <summary>
    ///     A plan is a series of compound actions which decompose into primitive actions. These actions can be executed
    ///     to achieve a specific goal. It can be thought of as a tree data structure. For example, given the goal
    ///     <i>Kill All Enemies On The Map</i>, a plan to achieve that could be visualized as follows:
    ///     <code>
    ///                                    Kill All Enemies On The Map
    ///                                   /                           \
    ///                                  /                             \
    ///                     Kill enemies in area 1               Kill enemies in area 2
    ///                        /             \                        /             \
    ///                       /               \                      /               \
    ///             Move to area 1     Attack using guns      Move to area 2    Attack using guns
    ///   
    /// </code>
    ///     Another way a plan can be described is as a top level method which contains a dictionary of sub-methods to execute.
    /// </summary>
    /// <remarks>
    ///     Typically, plans are thought of as a series of primitive methods, i.e. instead of thinking of it as a tree of
    ///     compound actions
    ///     which decompose into primitive actions, plans are typically thought of as a list of sequential actions. This would
    ///     cause two
    ///     problems for our simulator:
    ///     <list type="number">
    ///         <item>
    ///             We would have no simple way of supporting the notion of parallel actions vs. sequential actions.
    ///         </item>
    ///         <item>
    ///             We lose out information as to how methods are decomposed, which causes problems for re-planning if we want
    ///             to retain
    ///             particular parts of decompositions at planning time. For example, if our goal is to destroy an area via a
    ///             waypoint,
    ///             then on replan we may want to reuse that same waypoint.
    ///         </item>
    ///     </list>
    /// </remarks>
    [HideReferenceObjectPicker]
    public class Plan
    {
        /// <summary>
        ///     A pretty version of this plan in string form.
        /// </summary>
        [NonSerialized] private string? _prettyPlanString;

        /// <summary>
        ///     A dictionary of sub-methods to their chosen decompositions. The sub-methods an expansion of <see cref="topTask" />.
        /// </summary>
        [HideInInspector]
        public Dictionary<Method, Decomposition> dMethodToDecomposition = new Dictionary<Method, Decomposition>();

        /// <summary>
        ///     The resulting world state after executing this plan.
        /// </summary>
        [HideInInspector]
        public SimWorldState? endState; 

        /// <summary>
        ///     Used during planning for tracking the current state at the current phase of the planning algorithm
        /// </summary>
        [HideInInspector]
        public SimWorldState planTimeStateSoFar;

        /// <summary>
        ///     The starting world state that the plan should be executed on.
        /// </summary>
        [HideInInspector]
        public readonly SimWorldState startState;

        /// <summary>
        ///     The goal to achieve through execution of this plan. This plan gets decomposed into the sub-methods in
        ///     <see cref="dMethodToDecomposition" />.
        /// </summary>
        [HideDuplicateReferenceBox]
        public Method? topTask;

        public Plan(SimWorldState worldStateSoFar)
        {
            this.startState = worldStateSoFar.DeepCopy();
            this.planTimeStateSoFar = worldStateSoFar.DeepCopy();
        }

        public Plan(Plan cloneMe)
        {
            this.startState = cloneMe.startState.DeepCopy();
            this.planTimeStateSoFar = cloneMe.planTimeStateSoFar.DeepCopy();

            // shallow copy of methods is good enough; methods can be re-used across plans and should contain no internal state
            this.topTask = cloneMe.topTask;
            this.dMethodToDecomposition = new Dictionary<Method, Decomposition>(cloneMe.dMethodToDecomposition);
        }

        /// <summary>
        ///     A helper property to get the pretty string if already cached, otherwise generate it.
        /// </summary>
        [ShowInInspector]
        [EnableGUI]
        [FoldoutGroup("Plan Text")]
        [MultiLineProperty(20)]
        [HideLabel]
        public string PrettyPlanString => this._prettyPlanString ??= this.PrettyPrint().ToString();

        public Plan DeepCopy()
        {
            return new Plan(this);
        }

        /// <summary>
        ///     Get a pretty version of the plan in the form of a <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="maxCapacity">The maximum capacity of the string builder.</param>
        /// <returns>A string builder for the pretty string.</returns>
        public StringBuilder PrettyPrint(int maxCapacity = 1024 * 16)
        {
            var sb = new StringBuilder(1024, maxCapacity);
            sb.AppendLine("PLAN:");
            try
            {
                this.PrettyPrint(sb, 0, this.topTask, "");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Overflowing the max capacity.  Okay, just return the string so far.
            }
            catch (OutOfMemoryException)
            {
                // Overflowing the max capacity.  Okay, just return the string so far.
            }

            return sb;
        }

        public override string ToString()
        {
            return this.PrettyPlanString;
        }

        /// <summary>
        ///     A helper function for <see cref="PrettyPrint" />.
        /// </summary>
        /// <param name="sb">The string builder to use.</param>
        /// <param name="indent">How far to indent the current line.</param>
        /// <param name="method">The method to get a prettified version for.</param>
        /// <param name="suffix">A string to add after each method.</param>
        protected void PrettyPrint(StringBuilder sb, int indent, Method? method, string suffix)
        {
            sb.Append(' ', indent);
            if (null == method)
            {
                sb.AppendLine("null");
                return;
            }

            sb.Append(method);
            sb.Append(" ");
            sb.Append(suffix);
            if (this.dMethodToDecomposition.TryGetValue(method, out var d))
                foreach (var subtask in d.subtasks)
                    this.PrettyPrint(sb, indent + 2, subtask, d.mode.ToString());
        }

        public Plan Copy()
        {
            return new Plan(this);
        }

        /// <summary>
        ///     Add this plan as a <see cref="SimAction" /> to the given world <paramref name="state" />.
        /// </summary>
        /// <param name="state">The world state to add this plan to.</param>
        /// <returns>This plan in this form of a <see cref="SimAction" />.</returns>
        public PlannerSimAction ApplyToSim(SimWorldState state)
        {
            var plannerActions = new PlannerSimAction(this);
            state.actions.Add(plannerActions);
            return plannerActions;
        }


        
        public static bool HasPlanApplied(SimWorldState state)
        {
            return GetPlanActionsApplied(state).Any();
        }

        public static IEnumerable<PlannerSimAction> GetPlanActionsApplied(SimWorldState state)
        {
            return state.actions.actions
                .Select(entry => entry.subAction)
                .ConditionalCast<SimAction, PlannerSimAction>();
        }


        /// <summary>
        ///     Return an enumerable of all sequential actions for this plan.
        /// </summary>
        /// <returns>An enumerable of all sequential actions for this plan.</returns>
        public IEnumerable<Method> EnumerateSequentialActions()
        {
            return this.EnumerateSequentialActions(this.topTask);
        }

        /// <summary>
        ///     Helper function for <see cref="EnumerateSequentialActions()" />.
        /// </summary>
        /// <param name="task">The task to enumerate sequential actions for.</param>
        /// <returns>An enumerable of sequential actions for the given <paramref name="task" />.</returns>
        public IEnumerable<Method> EnumerateSequentialActions(Method? task)
        {
            if (task is null) yield break;

            if (this.dMethodToDecomposition.TryGetValue(task, out var d))
                if (d.mode == Decomposition.ExecutionMode.Sequential)
                    foreach (var subtask in d.subtasks)
                    foreach (var subMethod in this.EnumerateSequentialActions(subtask))
                        yield return subMethod;

            yield return task;
        }

        public IEnumerable<Func<SimWorldState, bool>> EnumerateReplanTests()
        {
            return this.EnumerateReplanTests(this.topTask);
        }

        public IEnumerable<Func<SimWorldState, bool>> EnumerateReplanTests(Method? task)
        {
            if (task is null) yield break;
            //
            // var func = task.GetReplanTestForSim();
            // if (null != func) yield return func;
            if (this.dMethodToDecomposition.TryGetValue(task, out var d))
                foreach (var subtask in d.subtasks)
                foreach (var func2 in this.EnumerateReplanTests(subtask))
                    yield return func2;
        }
        
    }
}