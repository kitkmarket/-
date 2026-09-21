using System.Runtime.InteropServices;

namespace LosyandexBrowser;

internal static class ProtectedData
{
    public static byte[] Protect(byte[] userData, byte[]? optionalEntropy, DataProtectionScope scope) => ProtectedDataNative.Protect(userData, optionalEntropy);
    public static byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy, DataProtectionScope scope) => ProtectedDataNative.Unprotect(encryptedData, optionalEntropy);
}

public enum DataProtectionScope { CurrentUser, LocalMachine }

internal static class ProtectedDataNative
{
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern bool CryptProtectData(ref Blob dataIn, string? description, ref Blob entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob dataOut);
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)] private static extern bool CryptUnprotectData(ref Blob dataIn, IntPtr description, ref Blob entropy, IntPtr reserved, IntPtr prompt, int flags, out Blob dataOut);
    [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr hMem);
    [StructLayout(LayoutKind.Sequential)] private struct Blob { public int cbData; public IntPtr pbData; }

    public static byte[] Protect(byte[] input, byte[] entropy) => Transform(input, entropy, true);
    public static byte[] Unprotect(byte[] input, byte[] entropy) => Transform(input, entropy, false);
    private static byte[] Transform(byte[] input, byte[] entropy, bool protect)
    {
        var inputPtr = Marshal.AllocHGlobal(input.Length); var entropyPtr = Marshal.AllocHGlobal(entropy.Length);
        Marshal.Copy(input, 0, inputPtr, input.Length); Marshal.Copy(entropy, 0, entropyPtr, entropy.Length);
        var data = new Blob { cbData = input.Length, pbData = inputPtr }; var ent = new Blob { cbData = entropy.Length, pbData = entropyPtr }; Blob output;
        try
        {
            var ok = protect ? CryptProtectData(ref data, null, ref ent, IntPtr.Zero, IntPtr.Zero, 0, out output) : CryptUnprotectData(ref data, IntPtr.Zero, ref ent, IntPtr.Zero, IntPtr.Zero, 0, out output);
            if (!ok) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            var result = new byte[output.cbData]; Marshal.Copy(output.pbData, result, 0, output.cbData); LocalFree(output.pbData); return result;
        }
        finally { Marshal.FreeHGlobal(inputPtr); Marshal.FreeHGlobal(entropyPtr); }
    }
}
