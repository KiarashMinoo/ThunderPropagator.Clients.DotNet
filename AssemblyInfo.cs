using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

#if !NET10_0_OR_GREATER
[assembly: RequiresPreviewFeatures]
#endif
[assembly: InternalsVisibleTo("ArchTests")]
[assembly: InternalsVisibleTo("UnitTests")]