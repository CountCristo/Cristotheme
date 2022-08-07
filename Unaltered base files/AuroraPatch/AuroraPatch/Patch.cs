using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

namespace AuroraPatch
{
    public abstract class Patch
    {
        public string Name => GetType().Name;
        public string AuroraExecutablePath => Loader.AuroraExecutablePath; // available on Loaded
        public string AuroraChecksum => Loader.AuroraChecksum; // available on Loaded
        public Assembly AuroraAssembly => Loader.AuroraAssembly; // available on Loaded
        public Form TacticalMap => Loader.TacticalMap; // available on Started

        /// <summary>
        /// A description for your mod, shown on the AuroraPatch UI.
        /// </summary>
        public virtual string Description { get { return ""; } }
        /// <summary>
        /// Return the names of other patches you depend on.
        /// </summary>
        public virtual IEnumerable<string> Dependencies { get { return Enumerable.Empty<string>(); } }

        internal Loader Loader { get; set; }

        /// <summary>
        /// Get one of your dependencies.
        /// </summary>
        public T GetDependency<T>(string name) where T : Patch
        {
            try
            {
                return (T)Loader.LoadedPatches.Single(p => p.Name == name);
            }
            catch (Exception e)
            {
                LogError($"Failed to get dependency {name}. {e}");

                return null;
            }
        }

        /// <summary>
        /// Run code on Aurora's UI thread. Only available on Started..
        /// </summary>
        public object InvokeOnUIThread(Delegate method, params object[] args)
        {
            try
            {
                return TacticalMap.Invoke(method, args);
            }
            catch (Exception e)
            {
                LogError($"Invoke exception {e.Message}");

                return null;
            }
        }

        /// <summary>
        /// Serialize arbitrary objects, useful for settings.
        /// </summary>
        public void Serialize<T>(string id, T obj)
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Converters.Add(new StringEnumConverter());

                var dir = Path.Combine(Path.GetDirectoryName(AuroraExecutablePath), "Patches", Name);
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, id + ".json");

                using (var reader = new StreamWriter(file))
                using (var json = new JsonTextWriter(reader))
                {
                    serializer.Serialize(json, obj);
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to serialize resourcce {id}. {e}");
            }
        }

        /// <summary>
        /// Deserialize arbitrary objects, useful for settings.
        /// </summary>
        public T Deserialize<T>(string id)
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Converters.Add(new StringEnumConverter());

                var file = Path.Combine(Path.GetDirectoryName(AuroraExecutablePath), "Patches", Name, id + ".json");

                using (var reader = new StreamReader(file))
                using (var json = new JsonTextReader(reader))
                {
                    return serializer.Deserialize<T>(json);
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to deserialize resource {id}. {e}");

                return default(T);
            }
        }

        public void LogDebug(string message)
        {
            Program.Logger.LogDebug($"Patch {Name}: {message}");
        }

        public void LogInfo(string message)
        {
            Program.Logger.LogInfo($"Patch {Name}: {message}");
        }

        public void LogWarning(string message)
        {
            Program.Logger.LogWarning($"Patch {Name}: {message}");
        }

        public void LogError(string message)
        {
            Program.Logger.LogError($"Patch {Name}: {message}");
        }

        public void LogCritical(string message)
        {
            Program.Logger.LogCritical($"Patch {Name}: {message}", false);
        }

        /// <summary>
        /// Called after the Aurora assembly is loaded.
        /// </summary>
        /// <param name="harmony"></param>
        protected virtual void Loaded(Harmony harmony)
        {

        }

        /// <summary>
        /// Called after the game is started. You can now invoke code on Aurora's UI thread and access the TacticalMap.
        /// </summary>
        protected virtual void Started()
        {

        }

        /// <summary>
        /// Called when the user clicks "Change settings" on the AuroraPatch UI.
        /// </summary>
        protected virtual void ChangeSettings()
        {

        }

        internal void LoadedInternal()
        {
            Loaded(new Harmony(Name));
        }

        internal void StartedInternal()
        {
            Started();
        }

        internal void ChangeSettingsInternal()
        {
            ChangeSettings();
        }
    }
}
