using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace GP
{
    public static class GpReflectionCache
    {
        private static readonly ConcurrentDictionary<(Type superType, Type subType), bool> DCacheIsSubclass =
            new ConcurrentDictionary<(Type superType, Type subType), bool>();


        private static readonly ConcurrentDictionary<Type, List<Type>> DCacheGetAllSubTypes =
            new ConcurrentDictionary<Type, List<Type>>();

        private static readonly ConcurrentDictionary<Type, Type> DCacheGetReturnTypeFromExecutableTreeSubClass =
            new ConcurrentDictionary<Type, Type>();

        public static bool IsSubclass(Type superType, Type subType)
        {
            var key = (superType, subType);
            if (DCacheIsSubclass.TryGetValue(key, out bool isSubclass)) return isSubclass;

            if (subType.IsGenericType && subType.GetGenericTypeDefinition() == superType ||
                subType.BaseType == superType)
            {
                DCacheIsSubclass[key] = true;
                return true;
            }

            isSubclass =
                subType.BaseType != null &&
                IsSubclass(superType, subType.BaseType); //&& GpRunner.IsSubclassOfExecutableTree(subType.BaseType);
            DCacheIsSubclass[key] = isSubclass;
            return isSubclass;
        }

        public static IEnumerable<Type> GetAllSubTypes(Type parentType)
        {
            // if (!genericType.IsGenericTypeDefinition)
            //     throw new ArgumentException("Specified type must be a generic type definition.", nameof(genericType));

            if (DCacheGetAllSubTypes.TryGetValue(parentType, out var subTypes)) return subTypes;

            subTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t =>
                    t.BaseType != null &&
                    t != parentType &&
                    IsSubclass(parentType, t))
                .ToList();

            DCacheGetAllSubTypes[parentType] = subTypes;
            return subTypes;
        }

        public static Type GetReturnTypeFromExecutableTreeSubClass(Type type)
        {
            if (DCacheGetReturnTypeFromExecutableTreeSubClass.TryGetValue(type, out var returnType)) return returnType;

            returnType = Internal_GetReturnTypeFromExecutableTreeSubClass(type);
            DCacheGetReturnTypeFromExecutableTreeSubClass[type] = returnType;
            return returnType;
        }

        private static Type Internal_GetReturnTypeFromExecutableTreeSubClass(Type type)
        {
            var parentType = type;

            while (null != parentType)
            {
                var templateParameters = parentType.GetGenericArguments();
                if (templateParameters.Length > 0) return templateParameters[0];

                parentType = parentType.BaseType;
            }

            throw new Exception($"Type {type.Name} does not descend from a generic type");
        }
    }
}