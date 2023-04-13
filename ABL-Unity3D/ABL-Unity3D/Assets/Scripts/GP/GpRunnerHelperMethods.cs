using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ABLUnitySimulation;
using GP.ExecutableNodeTypes;
using GP.ExecutableNodeTypes.ABLUnityActionTypes;
using GP.FitnessFunctions;
using Newtonsoft.Json;
using UnityEngine;
using Utilities.GeneralCSharp;
using Utilities.GP;

#nullable enable

namespace GP
{
    public partial class GpRunner
    {
        public Node GenerateRandomTreeFromTypeOrReturnType<T>(int maxDepth, bool forceFullyGrow)
        {
            var t = typeof(T);
            bool isSubclassOfExecutableTree = GpRunner.IsSubclassOfExecutableTree(t);
            var randomTree = isSubclassOfExecutableTree
                ? this.GenerateRandomTreeOfType(t, maxDepth, forceFullyGrow) 
                : this.GenerateRootNodeOfReturnType<T>(maxDepth, forceFullyGrow);
            return randomTree;
        }
        
        public TypedRootNode<T> GenerateRootNodeOfReturnType<T>(int maxDepth, bool forceFullyGrow)
        {
            IEnumerable<FilterAttribute> filters = GetFilterAttributes(typeof(T));
            var returnTypeSpecification = new ReturnTypeSpecification(typeof(T), filters);
            var child = (GpBuildingBlock<T>)this.GenerateRandomTreeOfReturnType(returnTypeSpecification, maxDepth - 1,
                forceFullyGrow);

            if (child.GetHeight() + 1 > maxDepth)
                throw new Exception("Somehow the max depth has been violated");

            return new TypedRootNode<T>(child);
        }

        public static bool IsListOfSubTypeOfExecutableNode(Type t)
        {
            return typeof(List<>).IsAssignableFrom(t) &&
                   IsSubclassOfExecutableTree(GetReturnTypeSpecification(t).returnType);
        }

        private Node GetChildFromParam(ParameterInfo param, int maxDepth, bool forceFullyGrow)
        {
            var returnType = GpReflectionCache.GetReturnTypeFromExecutableTreeSubClass(param.ParameterType);
            var filters = GetFilterAttributes(param);
            var returnTypeSpecification = new ReturnTypeSpecification(returnType, filters);
            var child = this.GenerateRandomTreeOfReturnType(returnTypeSpecification, maxDepth - 1, forceFullyGrow);

            if (child.GetHeight() > maxDepth - 1) throw new Exception("Somehow the max depth has been violated");

            return child;
        }

        private static bool IsTypedRootNodeLegal(Node typedRootNode, ProbabilityDistribution probabilityDistribution)
        {
            var maybeTypedRootNodeType = typedRootNode.GetType();
            if (maybeTypedRootNodeType.GetGenericTypeDefinition() != typeof(TypedRootNode<>))
                throw new Exception("Root node must be a TypedRootNode");

            var returnTypeSpecification = GetReturnTypeSpecification(maybeTypedRootNodeType);
            var allTypes = GetTerminalsOfReturnType(returnTypeSpecification).ToList();
            allTypes.AddRange(GetNonTerminalsOfReturnType(returnTypeSpecification));
            return allTypes.Any(t => probabilityDistribution.GetProbabilityOfType(t) > 0);
        }

        public static bool IsValidTree(Node root, ProbabilityDistribution probabilityDistribution, int maxDepth)
        {
            var satisfiesTypeConstraints = true;
            if (root.GetType().GetGenericTypeDefinition() == typeof(TypedRootNode<>))
                satisfiesTypeConstraints = IsTypedRootNodeLegal(root, probabilityDistribution);

            satisfiesTypeConstraints =
                satisfiesTypeConstraints && NodeSatisfiesTypeConstraints(root, probabilityDistribution);

            return root.GetHeight() <= maxDepth && satisfiesTypeConstraints;
        }

        private static bool HaveCompatibleReturnTypes(Type executableNodeType1, Type executableNodeType2)
        {
            Debug.Assert(IsSubclassOfExecutableTree(executableNodeType1) &&
                         IsSubclassOfExecutableTree(executableNodeType2));
            return executableNodeType1.IsAssignableFrom(executableNodeType2) ||
                   executableNodeType2.IsAssignableFrom(executableNodeType1);
        }

        private static bool NodeSatisfiesTypeConstraints(Node node, ProbabilityDistribution probabilityDistribution)
        {
            var nodeType = node.GetType();

            if (IsTerminal(nodeType) && probabilityDistribution.GetProbabilityOfType(nodeType) > 0) return true;

            if (probabilityDistribution.GetProbabilityOfType(nodeType) <= 0) return false;

            var constructors = nodeType.GetConstructors();
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (node.children.Count != parameters.Length) continue;

                var parameterTypes = parameters.Select(pInfo => pInfo.ParameterType).ToList();
                var childrenTypes = node.children.Select(child => child.GetType()).ToList();
                // This is assuming constructor arguments and the children in a Node are in the same order respectively 
                // which is currently the case because we do so elsewhere in the code (ie. GenerateRandomTreeOfType)
                var zippedTypes = parameterTypes.Zip(childrenTypes, (p, c) => (p, c));
                if (zippedTypes.All(t => HaveCompatibleReturnTypes(t.Item1, t.Item2)))
                    return node.children.All(child => NodeSatisfiesTypeConstraints(child, probabilityDistribution));
            }

            return false;
        }

        public Node GenerateRandomTreeOfType(Type t, int currentMaxDepth, bool forceFullyGrow)
        {
            Debug.Assert(IsSubclassOfExecutableTree(t));

            var constructor =
                GetRandomTreeConstructor(t) ??
                throw new Exception(
                    $"The type {t.Name} does not have a constructor with the RandomTreeConstructor attribute");

            var parameters = constructor.GetParameters();

            var constructorArguments = new List<object?>();

            

            foreach (var param in parameters)
                if (param.ParameterType == typeof(GpFieldsWrapper))
                    constructorArguments.Add(new GpFieldsWrapper(this));
                else if (param.ParameterType == typeof(int) && param.Name == "maxDepth")
                    constructorArguments.Add(currentMaxDepth - 1);
                else if (param.ParameterType == typeof(bool) && param.Name == "forceFullyGrow")
                    constructorArguments.Add(forceFullyGrow);
                else if (param.ParameterType == typeof(Team?))
                    constructorArguments.Add(null);
                else
                    constructorArguments.Add(this.GetChildFromParam(param, currentMaxDepth, forceFullyGrow));

            var node = (Node)constructor.Invoke(constructorArguments.ToArray());

            if (node.GetHeight() > currentMaxDepth) throw new Exception("Somehow the max depth has been violated.");

            return node;
        }

        public Node GenerateRandomTreeOfReturnType(ReturnTypeSpecification returnTypeSpecification, int currentMaxDepth,
            bool forceFullyGrow) 
        {
            
            Debug.Assert(GetAllReturnTypes().Contains(returnTypeSpecification.returnType));

            Type randomSubType;
            var filterAttributes = returnTypeSpecification.filters.ToList();

            var nonTerminals = GetNonTerminalsOfReturnType(returnTypeSpecification);
            double? nonTerminalsProbabilitySum = nonTerminals.Sum(t =>
                this.populationParameters.probabilityDistribution.GetProbabilityOfType(t));
            bool hasLegalNonTerminals = nonTerminals.Count > 0 && nonTerminalsProbabilitySum > 0;

            var terminals =
                GetTerminalsOfReturnType(new ReturnTypeSpecification(returnTypeSpecification.returnType,
                    filterAttributes));
            var terminalsList = terminals.ToList();
            double? terminalsProbabilitySum = terminalsList.Sum(t =>
                this.populationParameters.probabilityDistribution.GetProbabilityOfType(t));
            bool hasLegalTerminals = terminalsList.Count > 0 && terminalsProbabilitySum > 0;

            bool randomChanceToStopGrowing = hasLegalTerminals && !forceFullyGrow && this.rand.NextBool();

            if (!this.nodeReturnTypeToMinTreeDictionary.TryGetValue(returnTypeSpecification, out var minTree))
                throw new MinTreeNotSatisfiable(returnTypeSpecification.returnType);

            if (minTree.heightOfMinTree > currentMaxDepth)
                throw new MinTreeNotSatisfiable(returnTypeSpecification.returnType);

            if (minTree.heightOfMinTree == currentMaxDepth)
                // Generate Random Tree will generate the min tree if given the height of the min
                // tree as the current max depth which right now is equal to the current max depth anyways.
                randomSubType = minTree.permissibleNodeTypes.GetRandomEntry(this.rand);
            else if (currentMaxDepth < 1 || !hasLegalNonTerminals || randomChanceToStopGrowing)
                randomSubType = this.GetRandomTerminalOfReturnType(returnTypeSpecification);
            else
                randomSubType = this.GetRandomNonTerminalOfReturnType(returnTypeSpecification, currentMaxDepth);

            var tree = this.GenerateRandomTreeOfType(randomSubType, currentMaxDepth, forceFullyGrow);
            if (tree.GetHeight() > currentMaxDepth) throw new Exception("Somehow the max depth has been violated.");

            return tree;
        }

        public static IEnumerable<FilterAttribute> GetFilterAttributes(Type t)
        {
            return t.GetCustomAttributes<FilterAttribute>();
        }

        private static IEnumerable<FilterAttribute> GetFilterAttributes(ParameterInfo param)
        {
            return param.GetCustomAttributes<FilterAttribute>();
        }

        private static bool SatisfiesAllFilterAttributes(Type t, IEnumerable<FilterAttribute> filters)
        {
            return filters.All(f => f.IsSatisfiedBy(t));
        }

        private static IEnumerable<Type> GetAllSubTypesWithReturnType(Type openGenericType,
            ReturnTypeSpecification returnTypeSpecification)
        {
            var closedGenericType = openGenericType.MakeGenericType(returnTypeSpecification.returnType);
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    closedGenericType.IsAssignableFrom(t) &&
                    t != closedGenericType &&
                    SatisfiesAllFilterAttributes(t, returnTypeSpecification.filters)
                );
        }


        public static bool IsSubclassOfExecutableTree(Type type)
        {
            return GpReflectionCache.IsSubclass(typeof(GpBuildingBlock<>), type);
        }

        private static bool IsTerminal(Type t)
        {
            var constructor = GetRandomTreeConstructor(t);
            if (constructor == null) return false;

            var parameters = constructor.GetParameters();
            bool zeroParams = parameters.Length == 0;
            bool gpRunnerParam =
                parameters.Length == 1 &&
                parameters[0].ParameterType == typeof(GpFieldsWrapper);
            bool gpRunnerAndTeamParam =
                parameters.Length == 2 &&
                parameters[0].ParameterType == typeof(GpFieldsWrapper) &&
                parameters[1].ParameterType == typeof(Team);

            return zeroParams || gpRunnerParam || gpRunnerAndTeamParam;
        }

        private static IEnumerable<Type> GetTerminalsOfReturnType(ReturnTypeSpecification returnTypeSpecification)
        {
            var allTypes = GetAllSubTypesWithReturnType(typeof(GpBuildingBlock<>), returnTypeSpecification);
            return allTypes.Where(IsTerminal);
        }

        private Type GetRandomTerminalOfReturnType(ReturnTypeSpecification returnTypeSpecification)
        {
            var terminals = GetTerminalsOfReturnType(returnTypeSpecification).ToList();

            return this.GetRandomTypeFromDistribution(terminals); 
        }

        private static IEnumerable<Type> GetTerminals()
        {
            var allTypes = GetSubclassesOfExecutableTree();
            return allTypes.Where(IsTerminal);
        }

        private static List<Type> GetNonTerminalsOfReturnType(ReturnTypeSpecification returnTypeSpecification)
        {
            var allTypes = GetAllSubTypesWithReturnType(typeof(GpBuildingBlock<>), returnTypeSpecification);
            return allTypes.Except(GetTerminalsOfReturnType(returnTypeSpecification)).ToList();
        }

        private Type GetRandomNonTerminalOfReturnType(ReturnTypeSpecification returnTypeSpecification,
            int currentMaxDepth)
        {
            var nonTerminals = GetNonTerminalsOfReturnType(returnTypeSpecification)
                .Where(t =>
                    this.nodeTypeToMinTreeDictionary.TryGetValue(t, out var minTree) &&
                    minTree.heightOfMinTree <= currentMaxDepth);
            var allowedTypes = nonTerminals.ToList();
            if (!allowedTypes.Any()) throw new MinTreeNotSatisfiable(returnTypeSpecification.returnType);
            return this.GetRandomTypeFromDistribution(allowedTypes);
        }

        private static ConstructorInfo? GetRandomTreeConstructor(Type t)
        {
            Debug.Assert(IsSubclassOfExecutableTree(t));

            var constructors = t.GetConstructors();
            if (constructors.Length == 1) return constructors[0];

            var constructorsWithParameters = 0;
            ConstructorInfo? constructorToReturn = null;
            foreach (var candidateConstructor in constructors)
            {
                if (null != candidateConstructor.GetCustomAttribute<RandomTreeConstructorAttribute>())
                {
                    Debug.Assert(candidateConstructor != null);
                    return candidateConstructor;
                }

                if (candidateConstructor.GetParameters().Length > 0)
                {
                    constructorToReturn = candidateConstructor;
                    constructorsWithParameters++;
                }
            }

            if (constructorsWithParameters > 1)
                throw new Exception(
                    "There is no constructor decorated with the attribute [RandomTreeConstructor] " +
                    $"and there are multiple constructors with more than 0 parameters defined for the type {t.Name}. " +
                    "This is a limitation of the code we have written. " +
                    $"You must either change the code or change how the type {t.Name} is defined in order to continue");

            return constructorToReturn;
        }

        private Type GetRandomTypeFromDistribution(ProbabilityDistribution typeProbabilities)
        {
            return GenericUtilities.GetRandomElementFromDistribution(typeProbabilities.ToDictionary(), this.rand);
        }

        public Type GetRandomTypeFromDistribution()
        {
            return this.GetRandomTypeFromDistribution(this.populationParameters.probabilityDistribution);
        }

        private Type GetRandomTypeFromDistribution(IEnumerable<Type> allowedTypes)
        {
            var filteredTypes =
                this.populationParameters.probabilityDistribution.distribution.Where(tp =>
                    tp != null && allowedTypes.Contains(tp.type));
            return this.GetRandomTypeFromDistribution(new ProbabilityDistribution(filteredTypes.ToList()));
        }

        public static IEnumerable<Type> GetSubclassesOfExecutableTree(bool sortAlphabetically = false)
        {
            var types = GpReflectionCache.GetAllSubTypes(typeof(GpBuildingBlock<>))
                .Except(new List<Type>
                {
                    typeof(TypedRootNode<>)
                })
                .Where(t => !t.IsAbstract);

            return sortAlphabetically
                ? types.OrderBy(GpUtility.GetBetterClassName)
                : types; 
        }

        public static Node LoadTreeFromFile(string file)
        {
            file = GenericUtilities.GetRelativePath(file);
            var tree = JsonConvert.DeserializeObject<Node>(File.ReadAllText(file)) ??
                       throw new Exception($"File {file} does not contain a GP Tree.");

            return tree;
        }

        public static IEnumerable<Type> GetFitnessFunctionTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(t =>
                typeof(FitnessFunction).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                null == Attribute.GetCustomAttribute(t,
                    typeof(CompilerGeneratedAttribute))); // Exclude compiler generated classes
        }

        public static IEnumerable<Type> GetAllReturnTypes()
        {
            var allTypes = GetSubclassesOfExecutableTree();
            var allTypesArray = allTypes as Type[] ?? allTypes.ToArray();
            var returnTypes = allTypesArray
                .Where(t =>
                    t.BaseType is { IsGenericType: true } && 
                    t.BaseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(GpBuildingBlock<>)))
                // ReSharper disable once NullableWarningSuppressionIsUsed
                // The previous line checks that it's a subclass of ExecutableNode, so it obviously has a non-null base type.
                .Select(t => GpReflectionCache.GetReturnTypeFromExecutableTreeSubClass(t.BaseType!)).ToList();
            returnTypes.AddRange(allTypesArray);
            return returnTypes;
        }

        public static bool IsInDenyList(Individual i)
        {
            return i.genome.IterateTerminals().All(child => child is NoOp);
        }

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static PropertyInfo[] GetAllExecutableTreeProperties(Type t)
        {
            if (!IsSubclassOfExecutableTree(t))
                throw new Exception($"Type {t.Name} is not a subclass of executable tree");

            var props = new List<PropertyInfo>();

            
            // This is the case because t is always a subclass of ExecutableTree which
            // always extends from Node.
            var currentType = t;
            do
            {
                var newProps = currentType?
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .ToList();
                if (newProps != null) props.AddRange(newProps);
            } while ((currentType = currentType!.BaseType) != typeof(Node));

            return props.ToArray();
        }

        public static string? GetChildPropertyNameAtChildrenIndex(int i, Node? child)
        {
            if (child == null) throw new Exception("Child cannot be null");

            var childProperties = GetAllExecutableTreeProperties(child.GetType());

            return childProperties
                .Where(prop =>
                    ReferenceEquals(prop.GetValue(child, null), child.children[i]))
                .Select(prop => prop.Name)
                .FirstOrDefault();
        }

        private static ReturnTypeSpecification GetReturnTypeSpecification(Type t)
        {
            var returnType = GpReflectionCache.GetReturnTypeFromExecutableTreeSubClass(t);
            var filters = GetFilterAttributes(t);
            return new ReturnTypeSpecification(returnType, filters);
        }

        public static Dictionary<int, List<int>> GetLegalCrossoverPointsInChildren(Node a, Node b)
        {
            var xPoints = new Dictionary<int, List<int>>();
            var typesFound = new Dictionary<ReturnTypeSpecification, List<int>>();
            var i = 1;
            foreach (var node in a.IterateNodes().Skip(1))
            {
                var filters = GetFilterAttributes(node.GetType()).ToList();
                var nodeSpec = new ReturnTypeSpecification(node.returnType, filters);
                var locations = typesFound.ContainsKey(nodeSpec)
                    ? typesFound[nodeSpec]
                    : b.GetSymTypeAndFilterLocationsInDescendants(node.returnType, filters).ToList();

                if (locations.Count > 0)
                {
                    typesFound[nodeSpec] = locations;
                    xPoints[i] = typesFound[nodeSpec];
                }

                i++;
            }

            return xPoints;
        }
    }
}