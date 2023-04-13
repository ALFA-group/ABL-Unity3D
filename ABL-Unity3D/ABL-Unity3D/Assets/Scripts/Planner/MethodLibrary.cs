using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Planner.ManyWorlds;
using UnityEngine;
using Utilities.GeneralCSharp;

namespace Planner
{
    /// <summary>
    ///     An utility to hold <see cref="Method" />s that can be used by a <see cref="ManyWorldsPlanner" />.
    /// </summary>
    public class MethodLibrary
    {
        /// <summary>
        ///     All methods that are contained in this library.
        /// </summary>
        protected MultiTypeCollection methods = new MultiTypeCollection();

        /// <summary>
        ///     Add the given <paramref name="method" /> which satisfies the taskspec <typeparamref name="T" /> to this library.
        /// </summary>
        /// <param name="method">The method to </param>
        /// <typeparam name="T"></typeparam>
        public void Add<T>(Method<T> method) where T : struct, ITaskSpec
        {
            this.methods.Add(method);
        }

        /// <summary>
        ///     Add the given <paramref name="methodSpec" /> to this library.
        /// </summary>
        /// <param name="methodSpec">The <see cref="MethodSpec" /> to add.</param>
        public void Add(MethodSpec methodSpec)
        {
            this.methods.Add(methodSpec.methodType, methodSpec.method);
        }

        /// <summary>
        ///     Remove methods of the given <paramref name="methodType" />.
        /// </summary>
        /// <param name="methodType">The type of methods to remove.</param>
        public void RemoveMethods(Type methodType)
        {
            this.methods.RemoveAllOfType(methodType);
        }

        /// <summary>
        ///     Get method templates that achieve the given <paramref name="taskSpec" />.
        /// </summary>
        /// <param name="taskSpec">The task spec to achieve.</param>
        /// <typeparam name="TTaskSpec">The type of taskSpec to achieve.</typeparam>
        /// <returns>Method templates that achieve the given <paramref name="taskSpec" />.</returns>
        protected IEnumerable<Method<TTaskSpec>> GetMethodOptionTemplates<TTaskSpec>(TTaskSpec taskSpec)
            where TTaskSpec : struct, ITaskSpec
        {
            var cachedOptions = this.methods.Get<Method<TTaskSpec>>();
            return cachedOptions;
        }

        /// <summary>
        ///     Get methods that achieve the given <paramref name="taskSpec" />.
        /// </summary>
        /// <param name="taskSpec">The task spec to achieve.</param>
        /// <typeparam name="TTaskSpec">The type of task spec to achieve.</typeparam>
        /// <returns>Methods which achieve <paramref name="taskSpec" />.</returns>
        public IEnumerable<Method<TTaskSpec>> GetMethodOptions<TTaskSpec>(TTaskSpec taskSpec)
            where TTaskSpec : struct, ITaskSpec
        {
            foreach (var template in this.GetMethodOptionTemplates(taskSpec))
            {
                var instance = template.CloneWith(taskSpec);
                yield return instance;
            }
        }

        /// <summary>
        ///     Get the task spec for the given methodType.
        /// </summary>
        /// <param name="initialMethodType">The methodType to get the task spec of.</param>
        /// <returns>The task spec for <paramref name="initialMethodType" />.</returns>
        private static Type GetTaskSpecType(Type initialMethodType)
        {
            var methodType = initialMethodType;
            var genericMethod = typeof(Method<>);

            while (methodType != null && methodType != typeof(Method) && methodType != typeof(object))
            {
                var cur = methodType.IsGenericType ? methodType.GetGenericTypeDefinition() : methodType;
                if (genericMethod == cur && methodType.GenericTypeArguments.Length == 1)
                    return methodType.GenericTypeArguments[0];
                methodType = methodType.BaseType;
            }

            return null;
        }

        /// <summary>
        ///     Get all methods defined in the given <see cref="Assembly" />.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly" /> to add methods from.</param>
        /// <returns>All methods defined in the given <see cref="Assembly" />.</returns>
        public static IEnumerable<MethodSpec> GetReflectedMethodOptions(Assembly assembly)
        {
            var baseType = typeof(Method);

            var methodTypes = assembly
                .GetTypes()
                .Where(t => baseType.IsAssignableFrom(t) && t != baseType);

            foreach (var methodType in methodTypes)
            {
                var taskSpecType = GetTaskSpecType(methodType);
                if (null == taskSpecType || taskSpecType.ContainsGenericParameters) continue;

                var constructor = methodType.GetConstructor(new[] { taskSpecType });
                if (null == constructor)
                {
                    Debug.LogWarning($"No Constructor found for {methodType.Name} taking only {taskSpecType.Name}");
                }
                else
                {
                    object taskSpecInstance = Activator.CreateInstance(taskSpecType);
                    var method = (Method)constructor.Invoke(new[] { taskSpecInstance });

                    yield return new MethodSpec
                    {
                        methodType = typeof(Method<>).MakeGenericType(taskSpecType),
                        method = method
                    };
                }
            }
        }

        /// <summary>
        ///     Add all methods defined in the current executing assembly a new <see cref="MethodLibrary" />.
        /// </summary>
        /// <returns>A new <see cref="MethodLibrary" /> containing all the methods defined in the current executing assembly.</returns>
        public static MethodLibrary FromReflection()
        {
            var library = new MethodLibrary();
            foreach (var reflectedMethodOption in GetReflectedMethodOptions(Assembly.GetExecutingAssembly()))
                library.Add(reflectedMethodOption);

            return library;
        }

        /// <summary>
        ///     Wrapper for a <see cref="Method" /> and the type of that method.
        /// </summary>
        public struct MethodSpec
        {
            public Type methodType;
            public Method method;
        }
    }
}