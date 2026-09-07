using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

// Run the same contract in the game's actual Mono runtime, in an isolated Windows process.
// No game entry point, Unity graphics code, server, or profile is started.
internal static class UnityMonoHost
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

    [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void mono_set_dirs([MarshalAs(UnmanagedType.LPUTF8Str)] string assemblyDir,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string configDir);

    [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void mono_set_assemblies_path([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr mono_jit_init_version([MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string version);

    [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr mono_domain_assembly_open(IntPtr domain, [MarshalAs(UnmanagedType.LPUTF8Str)] string file);

    [DllImport("mono-2.0-bdwgc.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int mono_jit_exec(IntPtr domain, IntPtr assembly, int count, IntPtr arguments);

    internal static int Run(string gameRoot, string executable, string[] arguments)
    {
        var native = Path.Combine(gameRoot, "MonoBleedingEdge", "EmbedRuntime", "mono-2.0-bdwgc.dll");
        if (LoadLibraryEx(native, IntPtr.Zero, 8) == IntPtr.Zero) { throw new Win32Exception(); }
        var managed = Path.Combine(gameRoot, "EscapeFromTarkov_Data", "Managed");
        mono_set_dirs(managed, Path.Combine(gameRoot, "MonoBleedingEdge", "etc"));
        mono_set_assemblies_path(managed);
        var domain = mono_jit_init_version("spt-client-locale-contract", "v4.0.30319");
        if (domain == IntPtr.Zero) { throw new InvalidOperationException("Could not initialize Unity Mono."); }
        var assembly = mono_domain_assembly_open(domain, executable);
        if (assembly == IntPtr.Zero) { throw new InvalidOperationException("Could not load the contract in Unity Mono."); }

        var all = new string[arguments.Length + 1];
        all[0] = executable;
        Array.Copy(arguments, 0, all, 1, arguments.Length);
        var argv = Marshal.AllocHGlobal(IntPtr.Size * (all.Length + 1));
        var strings = new IntPtr[all.Length];
        try
        {
            for (var index = 0; index < all.Length; index++)
            {
                var bytes = Encoding.UTF8.GetBytes(all[index] + "\0");
                strings[index] = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, strings[index], bytes.Length);
                Marshal.WriteIntPtr(argv, index * IntPtr.Size, strings[index]);
            }
            Marshal.WriteIntPtr(argv, all.Length * IntPtr.Size, IntPtr.Zero);
            return mono_jit_exec(domain, assembly, all.Length, argv);
        }
        finally
        {
            foreach (var pointer in strings) { if (pointer != IntPtr.Zero) { Marshal.FreeHGlobal(pointer); } }
            Marshal.FreeHGlobal(argv);
            // This process exits after the contract; never unload a runtime with outstanding worker threads.
        }
    }
}
