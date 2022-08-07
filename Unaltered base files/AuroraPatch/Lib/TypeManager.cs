using AuroraPatch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Lib
{
    /// <summary>
    /// Represents the various Aurora form types accessible by 3rd party patches.
    /// </summary>
    public enum AuroraFormType
    {
        TacticalMap,
        Economics,
    }

    /// <summary>
    /// Helper class to interacting with the various Aurora assembly Types.
    /// </summary>
    public class TypeManager
    {
        private Assembly AuroraAssembly;
        private Lib Logger;
        private ConcurrentDictionary<AuroraFormType, Type> AuroraFormTypes = new ConcurrentDictionary<AuroraFormType, Type>();
        private BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public TypeManager(Assembly assembly, Lib logger)
        {
            AuroraAssembly = assembly;
            Logger = logger;
            FindFormTypes();
        }

        /// <summary>
        /// Helper method to pull out Types from the Aurora assembly.
        /// Can supply various filters.
        /// Useful when obfuscation makes it difficult to find a Type by name.
        /// 3rd party patches should use GetFormType(AuroraFormType) instead whenever possible as it implements caching.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="name"></param>
        /// <param name="baseType"></param>
        /// <param name="minMembers"></param>
        /// <param name="maxMembers"></param>
        /// <param name="minProperties"></param>
        /// <param name="maxProperties"></param>
        /// <param name="minFields"></param>
        /// <param name="maxFields"></param>
        /// <param name="isPublic"></param>
        /// <param name="isNotPublic"></param>
        /// <param name="isAbstract"></param>
        /// <param name="isInterface"></param>
        /// <param name="isClass"></param>
        /// <param name="isEnum"></param>
        /// <param name="minPropertyTypes"></param>
        /// <param name="maxPropertyTypes"></param>
        /// <param name="minFieldTypes"></param>
        /// <param name="maxFieldTypes"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetAuroraTypes(
            string name = null,
            Type baseType = null,
            int? minMembers = null,
            int? maxMembers = null,
            int? minProperties = null,
            int? maxProperties = null,
            int? minFields = null,
            int? maxFields = null,
            bool? isPublic = null,
            bool? isNotPublic = null,
            bool? isAbstract = null,
            bool? isInterface = null,
            bool? isClass = null,
            bool? isEnum = null,
            IEnumerable<Tuple<Type, int>> minPropertyTypes = null,
            IEnumerable<Tuple<Type, int>> maxPropertyTypes = null,
            IEnumerable<Tuple<Type, int>> minFieldTypes = null,
            IEnumerable<Tuple<Type, int>> maxFieldTypes = null)
        {
            if (name != null) return new List<Type> { AuroraAssembly.GetType(name) };
            IEnumerable<Type> types = AuroraAssembly.GetTypes().ToList();
            if (baseType != null) types = types.Where(t => t.BaseType.Equals(baseType));
            if (minMembers != null) types = types.Where(t => t.GetMembers().Length >= minMembers);
            if (maxMembers != null) types = types.Where(t => t.GetMembers().Length <= maxMembers);
            if (minProperties != null) types = types.Where(t => t.GetProperties().Length >= minProperties);
            if (maxProperties != null) types = types.Where(t => t.GetProperties().Length <= maxProperties);
            if (minFields != null) types = types.Where(t => t.GetFields().Length >= minFields);
            if (maxFields != null) types = types.Where(t => t.GetFields().Length <= maxFields);
            if (isPublic != null) types = types.Where(t => t.IsPublic);
            if (isNotPublic != null) types = types.Where(t => t.IsNotPublic);
            if (isAbstract != null) types = types.Where(t => t.IsAbstract);
            if (isInterface != null) types = types.Where(t => t.IsInterface);
            if (isClass != null) types = types.Where(t => t.IsClass);
            if (isEnum != null) types = types.Where(t => t.IsEnum);
            if (minPropertyTypes != null)
            {
                foreach (Tuple<Type, int> minPropertyTuple in minPropertyTypes)
                {
                    types = types.Where(t =>
                    {
                        var properties = t.GetProperties(DefaultBindingFlags)
                            .Where(p => p.PropertyType.Equals(minPropertyTuple.Item1));
                        return properties.Count() >= minPropertyTuple.Item2;
                    });
                }
            }
            if (maxPropertyTypes != null)
            {
                foreach (Tuple<Type, int> maxPropertyTuple in maxPropertyTypes)
                {
                    types = types.Where(t =>
                    {
                        var properties = t.GetProperties(DefaultBindingFlags)
                            .Where(p => p.PropertyType.Equals(maxPropertyTuple.Item1));
                        return properties.Count() <= maxPropertyTuple.Item2;
                    });
                }
            }
            if (minFieldTypes != null)
            {
                foreach (Tuple<Type, int> minFieldTuple in minFieldTypes)
                {
                    types = types.Where(t =>
                    {
                        var fields = t.GetFields(DefaultBindingFlags)
                            .Where(f => f.FieldType.Equals(minFieldTuple.Item1));
                        return fields.Count() >= minFieldTuple.Item2;
                    });
                }
            }
            if (maxFieldTypes != null)
            {
                foreach (Tuple<Type, int> maxFieldTuple in maxFieldTypes)
                {
                    types = types.Where(t =>
                    {
                        var fields = t.GetFields(DefaultBindingFlags)
                            .Where(f => f.FieldType.Equals(maxFieldTuple.Item1));
                        return fields.Count() <= maxFieldTuple.Item2;
                    });
                }
            }
            return types;
        }

        /// <summary>
        /// Find the TacticalMap Form type and populate our AuroraFormTypes dictionary.
        /// This can be a bit tricky due to the obfuscation. We're going in blind and counting buttons/checkboxes.
        /// As of May 3rd 2021 (Aurora 1.13), the TacticalMap had 66 buttons and 68 checkboxes so that's our signature.
        /// We got a bit of wiggle room as we're looking for a Form object with anywhere between 60 and 80 of each.
        /// </summary>
        private void FindTacticalMapFormType()
        {
            IEnumerable<Type> types = GetAuroraTypes(
                baseType: typeof(Form),
                minFieldTypes: new List<Tuple<Type, int>> {
                    new Tuple<Type, int>(typeof(Button), 60),
                    new Tuple<Type, int>(typeof(CheckBox), 60)
                },
                maxFieldTypes: new List<Tuple<Type, int>> {
                    new Tuple<Type, int>(typeof(Button), 80),
                    new Tuple<Type, int>(typeof(CheckBox), 80)
                }
            );
            if (types.Count() < 1)
            {
                Logger.LogError("TacticalMap Form type could not be identified");
                return;
            }
            if (types.Count() > 1)
            {
                Logger.LogError("Found " + types.Count() + " TacticalMap Form types - attempting to use the first one");
            }
            var type = types.First();
            Logger.LogDebug("TacticalMap Type found: " + type.Name);
            AuroraFormTypes[AuroraFormType.TacticalMap] = type;
        }

        /// <summary>
        /// Find the Economics Form type and populate our AuroraFormTypes dictionary.
        /// This can be a bit tricky due to the obfuscation. We're going in blind and counting buttons/checkboxes.
        /// As of May 3rd 2021 (Aurora 1.13), the Economics Form had 73 buttons and 12 checkboxes so that's our signature.
        /// We got a bit of wiggle room as we're looking for a Form object with anywhere between 70 to 80 buttons and
        /// 10 to 15 checkboxes.
        /// </summary>
        private void FindEconomicsFormType()
        {
            IEnumerable<Type> types = GetAuroraTypes(
                baseType: typeof(Form),
                minFieldTypes: new List<Tuple<Type, int>> {
                    new Tuple<Type, int>(typeof(Button), 70),
                    new Tuple<Type, int>(typeof(CheckBox), 10)
                },
                maxFieldTypes: new List<Tuple<Type, int>> {
                    new Tuple<Type, int>(typeof(Button), 80),
                    new Tuple<Type, int>(typeof(CheckBox), 15)
                }
            );
            if (types.Count() < 1)
            {
                Logger.LogError("Economics Form type could not be identified");
                return;
            }
            if (types.Count() > 1)
            {
                Logger.LogError("Found " + types.Count() + " Economics Form types - attempting to use the first one");
            }
            var type = types.First();
            Logger.LogDebug("Economics Type found: " + type.Name);
            AuroraFormTypes[AuroraFormType.Economics] = type;
        }

        /// <summary>
        /// Finds all supported Aurora Form types and populates the AuroraFormTypes static dictionary instance.
        /// </summary>
        private void FindFormTypes()
        {
            FindTacticalMapFormType();
            FindEconomicsFormType();
        }

        /// <summary>
        /// Public interface to access common/cached Form types pulled from the Aurora assembly.
        /// </summary>
        /// <param name="auroraFormType"></param>
        /// <returns></returns>
        public Type GetFormType(AuroraFormType auroraFormType)
        {
            if (AuroraFormTypes.ContainsKey(auroraFormType))
            {
                return AuroraFormTypes[auroraFormType];
            }
            Logger.LogWarning("Could not find Aurora Form " + Enum.GetName(typeof(AuroraFormType), auroraFormType));
            return null;
        }

        /// <summary>
        /// Helper method to pull out methods from a Type.
        /// Can supply various filters.
        /// Useful when obfuscation makes it difficult to find a method by name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="minParameters"></param>
        /// <param name="maxParameters"></param>
        /// <param name="minStackSize"></param>
        /// <param name="maxStackSize"></param>
        /// <param name="minLocalVariables"></param>
        /// <param name="maxLocalVariables"></param>
        /// <param name="minBodySize"></param>
        /// <param name="maxBodySize"></param>
        /// <param name="isAbstract"></param>
        /// <param name="isVirtual"></param>
        /// <param name="isStatic"></param>
        /// <param name="isConstructor"></param>
        /// <param name="isPrivate"></param>
        /// <param name="isPublic"></param>
        /// <param name="returnType"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="localVariableTypes"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetTypeMethods(
            Type type,
            string name = null,
            int? minParameters = null,
            int? maxParameters = null,
            int? minBodySize = null,
            int? maxBodySize = null,
            int? minStackSize = null,
            int? maxStackSize = null,
            int? minLocalVariables = null,
            int? maxLocalVariables = null,
            bool? isAbstract = null,
            bool? isVirtual = null,
            bool? isStatic = null,
            bool? isConstructor = null,
            bool? isPrivate = null,
            bool? isPublic = null,
            Type returnType = null,
            IEnumerable<Type> parameterTypes = null,
            IEnumerable<Type> localVariableTypes = null)
        {
            if (name != null) return new List<MethodInfo> { type.GetMethod(name, DefaultBindingFlags) };
            IEnumerable<MethodInfo> methods = type.GetMethods(DefaultBindingFlags);
            if (minParameters != null) methods = methods.Where(m => m.GetParameters().Count() >= minParameters);
            if (maxParameters != null) methods = methods.Where(m => m.GetParameters().Count() <= maxParameters);
            if (minBodySize != null) methods = methods.Where(m => m.GetMethodBody().GetILAsByteArray().Length >= minBodySize);
            if (maxBodySize != null) methods = methods.Where(m => m.GetMethodBody().GetILAsByteArray().Length <= maxBodySize);
            if (minStackSize != null) methods = methods.Where(m => m.GetMethodBody().MaxStackSize >= minStackSize);
            if (maxStackSize != null) methods = methods.Where(m => m.GetMethodBody().MaxStackSize <= maxStackSize);
            if (minLocalVariables != null) methods = methods.Where(m => m.GetMethodBody().LocalVariables.Count() >= minLocalVariables);
            if (maxLocalVariables != null) methods = methods.Where(m => m.GetMethodBody().LocalVariables.Count() <= maxLocalVariables);
            if (isAbstract != null) methods = methods.Where(m => m.IsAbstract);
            if (isVirtual != null) methods = methods.Where(m => m.IsVirtual);
            if (isStatic != null) methods = methods.Where(m => m.IsStatic);
            if (isConstructor != null) methods = methods.Where(m => m.IsConstructor);
            if (isPrivate != null) methods = methods.Where(m => m.IsPrivate);
            if (isPublic != null) methods = methods.Where(m => m.IsPublic);
            if (returnType != null) methods = methods.Where(m => m.ReturnType.Equals(returnType));
            if (parameterTypes != null)
            {
                foreach (Type parameterType in parameterTypes)
                {
                    methods = methods.Where(m =>
                    {
                        var parameters = m.GetParameters().Where(p => p.ParameterType.Equals(parameterType));
                        return parameters.Count() > 0;
                    });
                }
            }
            if (localVariableTypes != null)
            {
                foreach (Type localVariableType in localVariableTypes)
                {
                    methods = methods.Where(m =>
                    {
                        var localVariables = m.GetMethodBody().LocalVariables.Where(lv => lv.LocalType.Equals(localVariableType));
                        return localVariables.Count() > 0;
                    });
                }
            }
            return methods;
        }

        /// <summary>
        /// Helper method to pull out fields of a particular type from a parent type.
        /// Useful when obfuscation makes it difficult to find a field by name.
        /// </summary>
        /// <typeparam name="T">The field type requested.</typeparam>
        /// <typeparam name="U">The parent type.</typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<T> GetTypeFields<T, U>(U type)
        {
            List<T> fields = new List<T>();
            foreach (FieldInfo field in type.GetType().GetFields(DefaultBindingFlags))
            {
                if (field.FieldType.Equals(typeof(T))) fields.Add((T)field.GetValue(type));
            }
            return fields;
        }
    }
}
