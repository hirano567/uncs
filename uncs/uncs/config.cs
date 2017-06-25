using System;
using System.Globalization;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("temp")]
[assembly: InternalsVisibleTo("test")]
#endif

internal static class Config
{
    internal static CultureInfo Culture = CultureInfo.CurrentCulture;

    internal static string[] AdditionalLibPaths =
    {
        //@"C:\Windows\assembly\GAC_MSIL\System.Core\3.5.0.0__b77a5c561934e089",  // System.Core.dll
    };
}
