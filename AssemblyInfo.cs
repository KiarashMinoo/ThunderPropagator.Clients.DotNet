using System.Runtime.CompilerServices;
#if !NET10_0_OR_GREATER
using System.Runtime.Versioning;
#endif

[assembly: InternalsVisibleTo("ThunderPropagator.Clients.DotNet.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

#if !NET10_0_OR_GREATER
[assembly: RequiresPreviewFeatures]
#endif