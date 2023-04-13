using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Utilities.GeneralCSharp;
using Utilities.GP;

#nullable enable

namespace GP
{
    
    [HideReferenceObjectPicker]
    public class TypeProbability
    {
        [MinValue(0)]
        public double probability;

        [ValueDropdown("ProbabilityTreeView", ExpandAllMenuItems = true)]
        public Type type;

        public TypeProbability(Type type, double prob)
        {
            this.type = type;
            this.probability = prob;
        }

        public TypeProbability()
        {
            this.type = ProbabilityDistribution.ValidTypes.First();
            this.probability = 1;
        }

        [UsedImplicitly]
        public ValueDropdownList<Type> ProbabilityTreeView
        {
            get
            {
                var dropdown = new ValueDropdownList<Type>();

                foreach (var t in ProbabilityDistribution.ValidTypes)
                {
                    var returnType = GpReflectionCache.GetReturnTypeFromExecutableTreeSubClass(t);
                    string betterReturnTypeName = GpUtility.GetBetterClassName(returnType);
                    string betterTypeName = GpUtility.GetBetterClassName(t);
                    var treeViewPath = $"{betterReturnTypeName}/{betterTypeName}";
                    dropdown.Add(new ValueDropdownItem<Type>(treeViewPath, t));
                }

                return dropdown;
            }
        }

        public string TypeToString
        {
            get => this.type.Name;
            set { this.type = ProbabilityDistribution.ValidTypes.First(t => t.Name == value); }
        }
    }

    public class ProbabilityDistribution 
    {
        public static readonly IEnumerable<Type> ValidTypes =
            GpRunner.GetSubclassesOfExecutableTree(true);

        public readonly List<TypeProbability> distribution;

        public ProbabilityDistribution(IEnumerable<TypeProbability> distribution)
        {
            this.distribution = distribution.ToList();

            this.AddDefaultUnspecifiedTypeProbabilities();
        }

        public ProbabilityDistribution(List<Type> types)
        {
            this.distribution = new List<TypeProbability>(types.Count);

            foreach (var type in types) this.distribution.Add(new TypeProbability(type, 1));

            this.AddDefaultUnspecifiedTypeProbabilities();
        }

        public IEnumerable<Type> GetTypesWithProbabilityGreaterThanZero()
        {
            return this.distribution.Where(tp => tp.probability > 0).Select(tp => tp.type);
        }

        public double? GetProbabilityOfType(Type t)
        {
            return this.distribution.FirstOrDefault(tp => tp.type == t)?.probability;
        }

        public void AddDefaultUnspecifiedTypeProbabilities()
        {
            var typesOnly = this.distribution.Select(tp => tp.type).ToList();
            foreach (var type in ValidTypes)
                if (!typesOnly.Contains(type))
                    this.distribution.Add(new TypeProbability(type, 0));
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static ProbabilityDistribution? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<ProbabilityDistribution>(json);
        }

        public static ProbabilityDistribution GetProbabilityDistributionFromFile(string file)
        {
            file = GenericUtilities.GetRelativePath(file);
            return Deserialize(File.ReadAllText(file)) ??
                   throw new Exception($"File {file} does not contain a probability distribution.");
        }

        public void WriteToFile(string file)
        {
            file = GenericUtilities.GetRelativePath(file);
            File.WriteAllText(file, this.Serialize());
        }

        public Dictionary<Type, double> ToDictionary()
        {
            return this.distribution.ToDictionary(
                tp => tp.type,
                tp => tp.probability
            );
        }

        public static ProbabilityDistribution FromDictionary(Dictionary<Type, double> d)
        {
            var typeProbabilities = new List<TypeProbability>(d.Count);
            typeProbabilities.AddRange(
                d.Select(kvp =>
                    new TypeProbability(kvp.Key, kvp.Value)));

            return new ProbabilityDistribution(typeProbabilities);
        }
    }
}