using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace Lib
{
    public class SignatureManager
    {
        private class Signature
        {
            public AuroraType Name { get; set; }
            public Dictionary<string, bool> IsUniqueByChecksum { get; set; } = new Dictionary<string, bool>();
            public Dictionary<string, int> MinFieldTypes { get; set; } = new Dictionary<string, int>();
            public Dictionary<string, int> MaxFieldTypes { get; set; } = new Dictionary<string, int>();
        }

        private readonly Lib Lib;
        private readonly Dictionary<AuroraType, Signature> Signatures = new Dictionary<AuroraType, Signature>();
        private readonly Dictionary<AuroraType, Type> TypeCache = new Dictionary<AuroraType, Type>();

        internal SignatureManager(Lib lib)
        {
            Lib = lib;
            Load();

            foreach (var type in Lib.KnowledgeBase.GetKnownTypeNames())
            {
                GenerateSignatureFromType(type.Key, Lib.AuroraAssembly.GetType(type.Value));
            }

            Save();
        }

        public Type Get(AuroraType name)
        {
            lock (TypeCache)
            {
                if (TypeCache.ContainsKey(name))
                {
                    return TypeCache[name];
                }
            }

            Signature signature = null;
            lock (Signatures)
            {
                if (Signatures.ContainsKey(name))
                {
                    signature = Signatures[name];
                }
            }

            if (signature == null)
            {
                return null;
            }

            try
            {
                if (!signature.IsUniqueByChecksum.ContainsKey(Lib.AuroraChecksum))
                {
                    var types = GetTypes(signature);
                    if (types.Count == 1)
                    {
                        signature.IsUniqueByChecksum.Add(Lib.AuroraChecksum, true);
                    }
                    else
                    {
                        signature.IsUniqueByChecksum.Add(Lib.AuroraChecksum, false);
                    }

                    Save();
                }

                if (signature.IsUniqueByChecksum[Lib.AuroraChecksum])
                {
                    var type = GetTypes(signature).First();
                    lock (TypeCache)
                    {
                        TypeCache[name] = type;
                    }

                    return type;
                }
            }
            catch (Exception e)
            {
                Lib.LogError($"SignatureManager failed to get Type for {name}. {e}");
            }

            return null;
        }

        private List<Type> GetTypes(Signature signature)
        {
            var types = new List<Type>();
            var fieldtypes = new Dictionary<string, int>();

            foreach (var type in Lib.AuroraAssembly.GetTypes())
            {
                fieldtypes.Clear();
                var good = true;

                foreach (var field in type.GetFields(AccessTools.all))
                {
                    var name = field.FieldType.Name;
                    if (!fieldtypes.ContainsKey(name))
                    {
                        fieldtypes.Add(name, 0);
                    }

                    fieldtypes[name]++;
                }

                foreach (var min in signature.MinFieldTypes)
                {
                    var count = fieldtypes.ContainsKey(min.Key) ? fieldtypes[min.Key] : 0;

                    if (count < min.Value)
                    {
                        good = false;
                        break;
                    }
                }

                foreach (var max in signature.MaxFieldTypes)
                {
                    var count = fieldtypes.ContainsKey(max.Key) ? fieldtypes[max.Key] : 0;

                    if (count > max.Value)
                    {
                        good = false;
                        break;
                    }
                }

                if (good)
                {
                    types.Add(type);
                }
            }

            return types;
        }

        private void GenerateSignatureFromType(AuroraType name, Type type)
        {
            var fieldtypes = new Dictionary<Type, int>();

            foreach (var field in type.GetFields(AccessTools.all))
            {
                if (field.FieldType.Assembly.FullName != Lib.AuroraAssembly.FullName 
                    && field.FieldType.IsGenericType == false
                    && field.FieldType.IsInterface == false)
                {
                    if (!fieldtypes.ContainsKey(field.FieldType))
                    {
                        fieldtypes.Add(field.FieldType, 0);
                    }

                    fieldtypes[field.FieldType]++;
                }
            }

            var signature = new Signature()
            {
                Name = name,
            };

            signature.IsUniqueByChecksum.Add(Lib.AuroraChecksum, false);
            var deltas = new[] { 10, 5, 3, 2, 1, 0 };
            foreach (var delta in deltas)
            {
                foreach (var kvp in fieldtypes)
                {
                    signature.MinFieldTypes[kvp.Key.Name] = kvp.Value - delta;
                    signature.MaxFieldTypes[kvp.Key.Name] = kvp.Value + delta;
                }

                var types = GetTypes(signature);
                if (types.Count == 1)
                {
                    signature.IsUniqueByChecksum[Lib.AuroraChecksum] = true;
                    break;
                }
            } 

            lock (Signatures)
            {
                Signatures[name] = signature;
            }
        }

        private void Load()
        {
            lock (Signatures)
            {
                Signatures.Clear();

                try
                {
                    var signatures = Lib.Deserialize<List<Signature>>("signatures");
                    foreach (var signature in signatures)
                    {
                        Signatures.Add(signature.Name, signature);
                    }
                }
                catch (Exception)
                {
                    Lib.LogInfo("Signatures not found.");
                }
            }
        }

        private void Save()
        {
            lock (Signatures)
            {
                var signatures = Signatures.Values.ToList();
                Lib.Serialize("signatures", signatures);
            }
        }
    }
}
