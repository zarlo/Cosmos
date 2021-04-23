using System;
using System.Collections.Generic;

namespace Cosmos.TestRunner.Full
{
    public static class TestKernelSets
    {
        // Kernel types to run: the ones that will run in test runners that use the default engine configuration.
        public static IEnumerable<Type> GetKernelTypesToRun()
        {
            //yield return typeof(KernelGen3.Boot);
            return GetStableKernelTypes();
        }

        // Stable kernel types: the ones that are stable and will run in AppVeyor
        public static IEnumerable<Type> GetStableKernelTypes()
        {
            yield return typeof(NetworkTest.Kernel);
        }
    }
}
