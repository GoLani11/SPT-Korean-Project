using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KoreanPatchFix
{
    internal sealed class ClientLocaleTargets
    {
        private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        internal MethodInfo Init { get; private set; }
        internal MethodInfo UpdateGlobal { get; private set; }
        internal MethodInfo UpdateMenu { get; private set; }
        internal MethodInfo Reload { get; private set; }
        internal MethodInfo Fonts { get; private set; }
        internal PropertyInfo Instance { get; private set; }
        internal PropertyInfo Culture { get; private set; }
        internal PropertyInfo DefaultLanguage { get; private set; }
        internal ConstructorInfo LocaleConstructor { get; private set; }

        internal static ClientLocaleTargets Resolve(Assembly gameAssembly)
        {
            var types = GetTypes(gameAssembly);
            var manager = types.Single(type => Find(type, "UpdateLocales", typeof(void), typeof(string),
                typeof(Dictionary<string, string>)) != null);
            var result = new ClientLocaleTargets
            {
                Init = Required(manager, "Init", typeof(void), typeof(Dictionary<string, string>)),
                UpdateGlobal = Required(manager, "UpdateLocales", typeof(void), typeof(string), typeof(Dictionary<string, string>))
            };
            result.UpdateMenu = manager.GetMethods(Declared).Single(method =>
                method.Name == "UpdateMainMenuLocales" && !method.IsStatic && method.ReturnType == typeof(void)
                && method.GetParameters().Length == 2 && method.GetParameters()[0].ParameterType == typeof(string)
                && typeof(IDictionary<string, string>).IsAssignableFrom(method.GetParameters()[1].ParameterType));
            result.LocaleConstructor = result.UpdateMenu.GetParameters()[1].ParameterType
                .GetConstructor(new[] { typeof(IDictionary<string, string>) })
                ?? throw new MissingMethodException("The game locale copy constructor was not found.");

            result.Reload = types.Select(type => type.GetMethod("ReloadBackendLocale", Declared)).Where(method => method != null).Single(method =>
                method.IsStatic && !method.ContainsGenericParameters
                && method.ReturnType == typeof(Task) && method.GetParameters().Length == 3
                && method.GetParameters()[2].ParameterType == typeof(string));
            result.Fonts = manager.GetMethods(Declared).Single(method =>
                !method.IsStatic && method.ReturnType == typeof(void) && method.GetParameters().Length == 1
                && method.GetParameters()[0].ParameterType == typeof(string)
                && method.GetParameters()[0].Name == "localeType");
            result.Instance = manager.GetProperties(Declared).Single(property => property.PropertyType == manager
                && property.GetMethod != null && property.GetMethod.IsStatic && property.GetIndexParameters().Length == 0);
            result.Culture = manager.GetProperties(Declared).Single(property => property.PropertyType == typeof(string)
                && property.CanRead && property.CanWrite && !property.GetMethod.IsStatic && property.GetIndexParameters().Length == 0);
            var defaults = manager.GetProperty("DefaultLanguage", Declared)
                ?? gameAssembly.GetType("EFT.UI.WelcomeScreen", false)?.GetProperty("DefaultLanguage", Declared);
            if (defaults == null || defaults.PropertyType != typeof(string) || defaults.GetMethod == null || !defaults.GetMethod.IsStatic)
            {
                throw new MissingMemberException("The game's default-language property was not found.");
            }
            result.DefaultLanguage = defaults;
            return result;
        }

        internal object CopyMenu(IDictionary<string, string> source)
        {
            return LocaleConstructor.Invoke(new object[] { ClientLocaleBundle.MenuWithNames(source) });
        }

        private static MethodInfo Find(Type type, string name, Type returnType, params Type[] parameters)
        {
            var method = type.GetMethod(name, Declared, null, parameters, null);
            return method != null && !method.IsStatic && method.ReturnType == returnType ? method : null;
        }

        private static MethodInfo Required(Type type, string name, Type returnType, params Type[] parameters)
        {
            return Find(type, name, returnType, parameters) ?? throw new MissingMethodException(type.FullName, name);
        }

        private static Type[] GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException error)
            {
                return error.Types.Where(type => type != null).ToArray();
            }
        }
    }
}
