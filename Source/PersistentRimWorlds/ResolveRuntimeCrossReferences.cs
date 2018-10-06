using System.Collections.Generic;
using Verse;

namespace PersistentWorlds
{
    public class ResolveRuntimeCrossReferences
    {
        // TODO: Instead of relying on ScribeMultiLoader for referencing. This will allow things like maps to be loaded dynamically without worry for missing references.
        public List<ILoadReferenceable> Referenceables;
    }
}