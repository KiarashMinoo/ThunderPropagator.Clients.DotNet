---
title: Clarify message-encryption key handling (metadata exposes only IsEnabled)
labels: question,security,priority:P2
---

When a channel enables message encryption the server RSA-encrypts each field value (structured formats) or the whole payload (Proprietary) with the channel's public key; metadata exposes only `MessageEncryption.IsEnabled` (key material is JsonIgnored). The client's `SetMessageCipheringMetadata` and `RsaEncryptionService.Decrypt` expect key material supplied out of band, and decrypting data encrypted with a public key requires the matching private key.

Decide and document: how clients obtain the key, whether the algorithm is really RSA public-encrypt/private-decrypt for streaming data (size limits), and update `CipheringMetadata`/docs accordingly. Add an integration test against an encrypted channel.
