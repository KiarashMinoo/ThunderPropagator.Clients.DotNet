# Connections

## Contents

- [Overview](#overview)
- [Files](#files)
- [Diagrams](#diagrams)
- [Examples](#examples)
- [See Also](#see-also)

## Overview

The **Connections** area organizes 3 direct sub-areas. Each child is documented separately so responsibilities and APIs remain easy to navigate.

## Files

*None.*

### Direct child areas

- [InfiniteDataStream](./InfiniteDataStream/README.md) `Types:2` `Files:2`
- [Quic](./Quic/README.md) `Types:2` `Files:2`
- [WebSocket](./WebSocket/README.md) `Types:2` `Files:2`

## Diagrams

### Component overview

```mermaid
graph TD
  Current["Connections"]
  Current --> C0["InfiniteDataStream"]
  Current --> C1["Quic"]
  Current --> C2["WebSocket"]
```

The diagram shows the direct components documented by the **Connections** area.

## Examples

Choose the child area that matches the required capability; parent documentation intentionally does not duplicate child implementation details.

## See Also

- [Documentation home](../README.md)
- [Channels](../Channels/README.md)
- [Clients](../Clients/README.md)
- [Infrastructure](../Infrastructure/README.md)
- [Models](../Models/README.md)
- [instructions](../instructions/README.md)

[↑ Back to top](#contents)
