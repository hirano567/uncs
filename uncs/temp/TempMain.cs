using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

//================================================================================
// DEBUG
//================================================================================
#if DEBUG
namespace temp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            //CResources.MakeResourceID();
            //CResources.MakeResourceDef();

            //CErrorProcessing.MakeCSCErrorInfos();
            //CErrorProcessing.MakeALErrorInfos();
            //CErrorProcessing.Temp01();

            //CTempOptions.Run();
            //CTempConsole.Run();

            Temp03();
        }

        //------------------------------------------------------------
        // Temp03
        //------------------------------------------------------------
        private static void Temp03()
        {
        }

        //------------------------------------------------------------
        // Temp02
        //------------------------------------------------------------
        private static void Temp02()
        {
            string a = "hello";
            int b = 123;

            int ah = a.GetHashCode();
            int bh = b.GetHashCode();

            int c = unchecked(a.GetHashCode() + b.GetHashCode()) & 0x7FFFFFFF;
            int d = (a.GetHashCode() ^ b.GetHashCode()) & 0x7FFFFFFF;

            int e = 0;
            e ^= a.GetHashCode();
            e ^= b.GetHashCode();
            e &= 0x7FFFFFFF;
        }

        //------------------------------------------------------------
        // Temp01
        //------------------------------------------------------------
        internal enum CorTypeAttr2 : int
        {
            // Use this mask to retrieve the type visibility information.
            VisibilityMask = 0x00000007,    //                                                      (tdVisibilityMask)
            NotPublic = 0x00000000,         // Class is not public scope.                           (tdNotPublic)
            Public = 0x00000001,            // Class is public scope.                               (tdPublic)
            NestedPublic = 0x00000002,      // Class is nested with public visibility.              (tdNestedPublic)
            NestedPrivate = 0x00000003,     // Class is nested with private visibility.             (tdNestedPrivate)
            NestedFamily = 0x00000004,      // Class is nested with family visibility.              (tdNestedFamily)
            NestedAssembly = 0x00000005,    // Class is nested with assembly visibility.            (tdNestedAssembly)
            NestedFamANDAssem = 0x00000006, // Class is nested with family and assembly visibility. (tdNestedFamANDAssem)
            NestedFamORAssem = 0x00000007,  // Class is nested with family or assembly visibility.  (tdNestedFamORAssem)

            // Use this mask to retrieve class layout information
            LayoutMask = 0x00000018,        //                                          (tdLayoutMask)
            AutoLayout = 0x00000000,        // Class fields are auto-laid out           (tdAutoLayout)
            SequentialLayout = 0x00000008,  // Class fields are laid out sequentially   (tdSequentialLayout)
            ExplicitLayout = 0x00000010,    // Layout is supplied explicitly            (tdExplicitLayout)
            // end layout mask

            // Use this mask to retrieve class semantics information.
            ClassSemanticsMask = 0x00000060,    //                          (tdClassSemanticsMask)
            Class = 0x00000000,                 // Type is a class.         (tdClass)
            Interface = 0x00000020,             // Type is an interface.    (tdInterface)
            // end semantics mask

            // Special semantics in addition to class semantics.
            Abstract = 0x00000080,          // Class is abstract                            (tdAbstract)
            Sealed = 0x00000100,            // Class is concrete and may not be extended    (tdSealed)
            SpecialName = 0x00000400,       // Class name is special.  Name describes how.  (tdSpecialName)

            // Implementation attributes.
            Import = 0x00001000,            // Class / interface is imported    (tdImport)
            Serializable = 0x00002000,      // The class is Serializable.       (tdSerializable)

            // Use tdStringFormatMask to retrieve string information for native interop
            StringFormatMask = 0x00030000,  // (tdStringFormatMask)
            AnsiClass = 0x00000000,         // LPTSTR is interpreted as ANSI in this class  (tdAnsiClass)
            UnicodeClass = 0x00010000,      // LPTSTR is interpreted as UNICODE             (tdUnicodeClass)
            AutoClass = 0x00020000,         // LPTSTR is interpreted automatically          (tdAutoClass)
            CustomFormatClass = 0x00030000, // A non-standard encoding specified by CustomFormatMask (tdCustomFormatClass)
            CustomFormatMask = 0x00C00000,  // Use this mask to retrieve non-standard encoding information for native interop.
            //The meaning of the values of these 2 bits is unspecified. (tdCustomFormatMask)

            // end string format mask

            BeforeFieldInit = 0x00100000,   // Initialize the class any time before first static field access.  (tdBeforeFieldInit)
            Forwarder = 0x00200000,         // This ExportedType is a type forwarder.                           (tdForwarder)

            // Flags reserved for runtime use.
            ReservedMask = 0x00040800,      //                                          (tdReservedMask)
            RTSpecialName = 0x00000800,     // Runtime should check name encoding.      (td RTSpecialName)
            HasSecurity = 0x00040000,       // Class has security associate with it.    (tdHasSecurity)
        }

        private static void Temp01()
        {
            int i = 0;
            bool br = false;

            br = ((int)CorTypeAttr2.Abstract == (int)TypeAttributes.Abstract);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.AnsiClass == (int)TypeAttributes.AnsiClass);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.AutoClass == (int)TypeAttributes.AutoClass);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.AutoLayout == (int)TypeAttributes.AutoLayout);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Class == (int)TypeAttributes.Class);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.ClassSemanticsMask == (int)TypeAttributes.ClassSemanticsMask);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.CustomFormatClass == (int)TypeAttributes.CustomFormatClass);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.CustomFormatMask == (int)TypeAttributes.CustomFormatMask);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.ExplicitLayout == (int)TypeAttributes.ExplicitLayout);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.HasSecurity == (int)TypeAttributes.HasSecurity);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Import == (int)TypeAttributes.Import);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Interface == (int)TypeAttributes.Interface);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.LayoutMask == (int)TypeAttributes.LayoutMask);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedAssembly == (int)TypeAttributes.NestedAssembly);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedFamANDAssem == (int)TypeAttributes.NestedFamANDAssem);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedFamily == (int)TypeAttributes.NestedFamily);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedFamORAssem == (int)TypeAttributes.NestedFamORAssem);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedPrivate == (int)TypeAttributes.NestedPrivate);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NestedPublic == (int)TypeAttributes.NestedPublic);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.NotPublic == (int)TypeAttributes.NotPublic);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Public == (int)TypeAttributes.Public);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.ReservedMask == (int)TypeAttributes.ReservedMask);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.RTSpecialName == (int)TypeAttributes.RTSpecialName);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Sealed == (int)TypeAttributes.Sealed);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.SequentialLayout == (int)TypeAttributes.SequentialLayout);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.Serializable == (int)TypeAttributes.Serializable);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.SpecialName == (int)TypeAttributes.SpecialName);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.StringFormatMask == (int)TypeAttributes.StringFormatMask);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.UnicodeClass == (int)TypeAttributes.UnicodeClass);
            Console.WriteLine("{0}: {1}", ++i, br);
            br = ((int)CorTypeAttr2.VisibilityMask == (int)TypeAttributes.VisibilityMask);
            Console.WriteLine("{0}: {1}", ++i, br);

            Console.WriteLine("ClassSemanticsMask = {0}", (int)TypeAttributes.ClassSemanticsMask);

            Console.ReadKey();
        }
    }
}
#else
//================================================================================
// Not DEBUG
//================================================================================
public static class Program
{
    public static void Main()
    {
        Console.Write("Run in DEBUG mode.");
        Console.Read();
    }
}
#endif
