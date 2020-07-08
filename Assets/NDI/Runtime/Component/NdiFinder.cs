using System.Collections.Generic;
using UnityEngine;

namespace NDI
{

    public static class NdiFinder
    {
        public static IEnumerable<string> sourceNames => EnumerateSourceNames();

        public static IEnumerable<string> EnumerateSourceNames()
        {
            if (SharedInstance.Find == null)
            {
                Debug.LogError(SharedInstance.Find);
            }

            var list = new List<string>();
            foreach (var source in SharedInstance.Find.CurrentSources)
                list.Add(source.NdiName);
            return list;
        }
    }

}
