---
title: WebSocket receive appends the whole buffer instead of `result.Count` bytes
labels: bug,transport,priority:P1
---

## Summary
`ThunderPropagatorWebSocketConnection.ReceiveAsync` does `bytes.AddRange(buffer.ToArray())` for every chunk, i.e. the full `BufferSize` array rather than `result.Count` bytes. Stale bytes from earlier reads leak into later messages; the connection base class papers over it with `stringBuilder.Replace("\0", "")` and `Trim()`.

## To do
- Append only `buffer.Array![..result.Count]`; handle `WebSocketMessageType.Close` (return/raise closed instead of decoding); remove the NUL-stripping workaround; add tests with multi-fragment messages and messages smaller than the buffer.
- Server context: text frames only, `MaxPushSize` 128 KB, `MaxRequestSize` 512 KB; inbound binary frames are ignored.
- Same review for the QUIC and IDS receive paths (QUIC: one unidirectional stream per push, read to EOF; IDS: one chunk per push terminated by a newline).
