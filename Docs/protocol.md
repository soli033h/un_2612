# Network Protocol (Draft)

- Transport: UDP, one JSON-encoded `NetworkEnvelope` per datagram (UTF-8).
- Authority: the dedicated server (Unity headless build) simulates all cars with PhysX. Clients send input only.
- Tick: the server simulates at a fixed tick and broadcasts `WorldSnapshot` every tick (rates are TBD, e.g. 50 Hz simulation / 20–30 Hz broadcast).
- Car stats and paint color are **not** sent. Both sides derive them from `statSeed` with `DeterministicRandom`.

C# definitions live in `Assets/Scripts/Shared/Networking/Protocol/`. Field names below are the JSON keys.

## Envelope

```json
{ "type": 5, "seq": 1024, "payload": "{...}" }
```

| Field | Type | Description |
|---|---|---|
| `type` | int (`MessageType`) | Payload kind |
| `seq` | uint | Per-sender sequence number; used to drop stale or duplicate datagrams |
| `payload` | string | JSON of the message below |

## Messages

### Client → Server

| Message | Fields | When |
|---|---|---|
| `JoinRequest` | `nickname` | Retried until `JoinAccepted` arrives |
| `InputCommand` | `tick`, `throttle` (0..1), `brake` (0..1), `steer` (-1..1), `handbrake` (bool), `boost` (bool) | Every client tick |
| `Ping` | `clientTime` | Periodically, for RTT and keep-alive |
| `Leave` | — | On quit |

### Server → Client

| Message | Fields | When |
|---|---|---|
| `JoinAccepted` | `playerId`, `serverTick`, `tickRate`, `snapshotRate` | Reply to `JoinRequest` |
| `PlayerSpawn` | `playerId`, `nickname`, `statSeed`, `spawnIndex` | A player joins (sent to all; the joining player also receives one per existing player) |
| `PlayerDespawn` | `playerId`, `reason` | A player leaves or times out |
| `WorldSnapshot` | `tick`, `serverTime`, `cars` (`CarSnapshot[]`) | Every broadcast tick |
| `RaceEvent` | `type` (Countdown / Start / Lap / Finish / Collision), `playerId`, `value` | On event |
| `Pong` | `clientTime`, `serverTime` | Reply to `Ping` |

### CarSnapshot

| Field | Type | Used for |
|---|---|---|
| `playerId` | int | Which car |
| `position` | float[3] | Transform |
| `rotation` | float[4] (quaternion) | Transform |
| `velocity` | float[3] | Interpolation / extrapolation |
| `angularVelocity` | float[3] | Interpolation / extrapolation |
| `steer` | float | Front wheel angle visuals |
| `throttle` / `brake` | float | Engine sound, brake lights |
| `state` | int (`CarPhysicsState`) | Drift smoke, boost flames, camera effects |
| `driftAngle` | float | Drift visuals |
| `boostGauge` | float (0..1) | HUD, boost visuals |
| `lastProcessedInputTick` | uint | Client-side prediction reconciliation (local car only) |

## Open items

- Reliability for `JoinAccepted`, `PlayerSpawn`, `PlayerDespawn`, `RaceEvent` (ack + resend) since UDP may drop them.
- JSON library: `JsonUtility` cannot serialize arrays of arrays or polymorphic payloads cleanly; consider `com.unity.nuget.newtonsoft-json`.
- Datagram size: keep each `WorldSnapshot` under ~1200 bytes to avoid IP fragmentation; JSON may need trimming (fewer decimals) as player count grows.
